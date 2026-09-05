using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public interface IEmailOutboxService
    {
        Task<EmailOutboxMessage> QueueEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? plainTextBody = null,
            string? eventKey = null,
            string? metadataJson = null,
            CancellationToken cancellationToken = default);

        Task<int> ProcessOutboxAsync(int batchSize = 10, CancellationToken cancellationToken = default);

        Task<EmailOutboxMessage?> DispatchSingleAsync(long messageId, CancellationToken cancellationToken = default);
    }

    public class EmailOutboxService : IEmailOutboxService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailOutboxService> _logger;
        public EmailOutboxService(ApplicationDbContext context, IEmailSender emailSender, ILogger<EmailOutboxService> logger)
        { _context = context; _emailSender = emailSender; _logger = logger; }

        public static EmailOutboxMessage CreateMessage(string toEmail, string subject, string htmlBody,
            string? plainTextBody = null, string? eventKey = null, string? metadataJson = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || toEmail.Trim().Length > 255) throw new ArgumentException("Invalid recipient.");
            if (string.IsNullOrWhiteSpace(subject) || subject.Trim().Length > 255) throw new ArgumentException("Invalid subject.");
            if (eventKey?.Trim().Length > 150) throw new ArgumentException("Event key is too long.");
            return new EmailOutboxMessage { ToEmail = toEmail.Trim(), Subject = subject.Trim(), HtmlBody = htmlBody,
                PlainTextBody = plainTextBody, EventKey = string.IsNullOrWhiteSpace(eventKey) ? null : eventKey.Trim(),
                MetadataJson = metadataJson, Status = EmailOutboxStatus.Queued, CreatedAt = DateTime.UtcNow, MaxAttempts = 3 };
        }

        public async Task<EmailOutboxMessage> QueueEmailAsync(string toEmail, string subject, string htmlBody,
            string? plainTextBody = null, string? eventKey = null, string? metadataJson = null,
            CancellationToken cancellationToken = default)
        {
            var message = CreateMessage(toEmail, subject, htmlBody, plainTextBody, eventKey, metadataJson);
            if (message.EventKey != null)
            {
                var previous = await _context.EmailOutboxMessages.FirstOrDefaultAsync(e => e.EventKey == message.EventKey, cancellationToken);
                if (previous != null) return previous;
            }
            _context.EmailOutboxMessages.Add(message);
            try { await _context.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateException ex) when (message.EventKey != null && ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                _context.Entry(message).State = EntityState.Detached;
                var previous = await _context.EmailOutboxMessages.FirstOrDefaultAsync(e => e.EventKey == message.EventKey, cancellationToken);
                if (previous == null) throw;
                return previous;
            }
            return message;
        }

        private IQueryable<EmailOutboxMessage> Eligible(DateTime now) => _context.EmailOutboxMessages.Where(e =>
            (e.Status == EmailOutboxStatus.Queued || e.Status == EmailOutboxStatus.Blocked ||
             (e.Status == EmailOutboxStatus.Failed && e.AttemptCount < e.MaxAttempts) ||
             (e.Status == EmailOutboxStatus.Processing && (e.LockExpiresAt == null || e.LockExpiresAt <= now))) &&
            (e.NextAttemptAt == null || e.NextAttemptAt <= now));

        public async Task<int> ProcessOutboxAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            var ids = await Eligible(DateTime.UtcNow).OrderBy(e => e.CreatedAt)
                .Take(Math.Clamp(batchSize, 1, 100)).Select(e => e.Id).ToListAsync(cancellationToken);
            int count = 0;
            foreach (var id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await DispatchSingleAsync(id, cancellationToken) != null) count++;
            }
            return count;
        }

        public async Task<EmailOutboxMessage?> DispatchSingleAsync(long messageId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var token = Guid.NewGuid().ToString("N");
            // Atomic SQL claim. Sent messages and live leases cannot be dispatched again.
            if (_context.Database.IsRelational())
            {
                var claimed = await Eligible(now).Where(e => e.Id == messageId).ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, EmailOutboxStatus.Processing)
                    .SetProperty(e => e.LockToken, token)
                    .SetProperty(e => e.LockExpiresAt, now.AddMinutes(5))
                    .SetProperty(e => e.ProcessedAt, now), cancellationToken);
                if (claimed == 0) return null;
            }
            else
            {
                // Unit-test provider only: this path does not prove database concurrency.
                var candidate = await Eligible(now).FirstOrDefaultAsync(e => e.Id == messageId, cancellationToken);
                if (candidate == null) return null;
                candidate.Status = EmailOutboxStatus.Processing;
                candidate.LockToken = token;
                candidate.LockExpiresAt = now.AddMinutes(5);
                await _context.SaveChangesAsync(cancellationToken);
            }
            var msg = await _context.EmailOutboxMessages.AsNoTracking().FirstAsync(e => e.Id == messageId, cancellationToken);
            var status = EmailOutboxStatus.Failed;
            string? error = null;
            DateTime? retryAt = null;
            var attempts = msg.AttemptCount + 1;
            try
            {
                // Bound sending well below the lease duration. Crash recovery is at-least-once:
                // SMTP itself cannot guarantee exactly-once after a server accepts a message.
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(60));
                var result = await _emailSender.SendEmailAsync(msg.ToEmail, msg.Subject, msg.HtmlBody, msg.PlainTextBody, timeout.Token);
                if (result.Success) status = EmailOutboxStatus.Sent;
                else if (result.IsBlocked)
                {
                    status = EmailOutboxStatus.Blocked;
                    attempts = msg.AttemptCount; // Missing configuration does not exhaust delivery retries.
                    retryAt = DateTime.UtcNow.AddMinutes(5);
                    error = result.ErrorMessage;
                }
                else error = result.ErrorMessage;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogWarning(ex, "Outbox delivery failed for message {MessageId}", messageId);
            }
            if (status == EmailOutboxStatus.Failed && attempts < msg.MaxAttempts)
                retryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, attempts));
            if (error?.Length > 1000) error = error[..1000];
            var finished = DateTime.UtcNow;
            // Fencing: a worker cannot overwrite another worker's reclaimed message.
            if (_context.Database.IsRelational())
            {
                await _context.EmailOutboxMessages.Where(e => e.Id == messageId && e.LockToken == token)
                    .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, status)
                        .SetProperty(e => e.AttemptCount, attempts).SetProperty(e => e.LastError, error)
                        .SetProperty(e => e.NextAttemptAt, retryAt).SetProperty(e => e.ProcessedAt, finished)
                        .SetProperty(e => e.LockToken, (string?)null).SetProperty(e => e.LockExpiresAt, (DateTime?)null), CancellationToken.None);
            }
            else
            {
                var tracked = await _context.EmailOutboxMessages.FirstAsync(e => e.Id == messageId, CancellationToken.None);
                if (tracked.LockToken == token)
                {
                    tracked.Status = status; tracked.AttemptCount = attempts; tracked.LastError = error;
                    tracked.NextAttemptAt = retryAt; tracked.ProcessedAt = finished;
                    tracked.LockToken = null; tracked.LockExpiresAt = null;
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
            }
            return await _context.EmailOutboxMessages.AsNoTracking().FirstAsync(e => e.Id == messageId, CancellationToken.None);
        }
    }

    public class EmailOutboxBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailOutboxBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15);

        public EmailOutboxBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EmailOutboxBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Outbox Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var outboxService = scope.ServiceProvider.GetRequiredService<IEmailOutboxService>();
                    await outboxService.ProcessOutboxAsync(batchSize: 10, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during transactional email outbox processing.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Email Outbox Background Service stopped.");
        }
    }
}
