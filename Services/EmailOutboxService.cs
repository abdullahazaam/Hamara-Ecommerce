using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        public EmailOutboxService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<EmailOutboxService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<EmailOutboxMessage> QueueEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string? plainTextBody = null,
            string? eventKey = null,
            string? metadataJson = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Recipient email address cannot be empty.", nameof(toEmail));
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException("Email subject cannot be empty.", nameof(subject));
            }

            // Duplicate Protection: check by eventKey if provided
            if (!string.IsNullOrWhiteSpace(eventKey))
            {
                var cleanKey = eventKey.Trim();
                var existing = await _context.EmailOutboxMessages
                    .FirstOrDefaultAsync(e => e.EventKey == cleanKey, cancellationToken);

                if (existing != null)
                {
                    _logger.LogInformation("Outbox duplicate eventKey '{EventKey}' detected. Returning existing outbox message #{Id}.", cleanKey, existing.Id);
                    return existing;
                }
            }

            var message = new EmailOutboxMessage
            {
                EventKey = !string.IsNullOrWhiteSpace(eventKey) ? eventKey.Trim() : null,
                ToEmail = toEmail.Trim().ToLowerInvariant(),
                Subject = subject.Trim(),
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody,
                Status = EmailOutboxStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                AttemptCount = 0,
                MaxAttempts = 3,
                MetadataJson = metadataJson
            };

            _context.EmailOutboxMessages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Queued transactional email #{Id} for {ToEmail} (Subject: '{Subject}', EventKey: '{EventKey}')",
                message.Id, message.ToEmail, message.Subject, message.EventKey ?? "none");

            return message;
        }

        public async Task<int> ProcessOutboxAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Fetch pending messages: Queued OR (Failed with attempts remaining and retry backoff reached)
            var eligibleMessages = await _context.EmailOutboxMessages
                .Where(e => e.Status == EmailOutboxStatus.Queued || 
                           (e.Status == EmailOutboxStatus.Failed && e.AttemptCount < e.MaxAttempts && (e.NextAttemptAt == null || e.NextAttemptAt <= now)))
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (!eligibleMessages.Any())
            {
                return 0;
            }

            int processedCount = 0;

            foreach (var msg in eligibleMessages)
            {
                if (cancellationToken.IsCancellationRequested) break;

                msg.AttemptCount++;
                msg.Status = EmailOutboxStatus.Processing;
                msg.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                try
                {
                    var result = await _emailSender.SendEmailAsync(msg.ToEmail, msg.Subject, msg.HtmlBody, msg.PlainTextBody, cancellationToken);

                    if (result.Success)
                    {
                        msg.Status = EmailOutboxStatus.Sent;
                        msg.LastError = null;
                        msg.ProcessedAt = DateTime.UtcNow;
                        _logger.LogInformation("Email #{Id} successfully dispatched to {ToEmail}.", msg.Id, msg.ToEmail);
                    }
                    else if (result.IsBlocked)
                    {
                        // SMTP credentials missing: mark BLOCKED honestly, never claim sent
                        msg.Status = EmailOutboxStatus.Blocked;
                        msg.LastError = result.ErrorMessage ?? "SMTP credentials are not configured. Delivery blocked.";
                        msg.ProcessedAt = DateTime.UtcNow;
                        _logger.LogWarning("Email #{Id} to {ToEmail} BLOCKED: {Reason}", msg.Id, msg.ToEmail, msg.LastError);
                    }
                    else
                    {
                        msg.LastError = result.ErrorMessage;
                        msg.ProcessedAt = DateTime.UtcNow;

                        if (msg.AttemptCount >= msg.MaxAttempts)
                        {
                            msg.Status = EmailOutboxStatus.Failed;
                            _logger.LogError("Email #{Id} to {ToEmail} failed permanently after {Attempts} attempts. Error: {Error}",
                                msg.Id, msg.ToEmail, msg.AttemptCount, msg.LastError);
                        }
                        else
                        {
                            // Exponential retry backoff: 2^attempt minutes (2m, 4m, etc.)
                            var delayMinutes = Math.Pow(2, msg.AttemptCount);
                            msg.NextAttemptAt = DateTime.UtcNow.AddMinutes(delayMinutes);
                            msg.Status = EmailOutboxStatus.Failed;
                            _logger.LogWarning("Email #{Id} to {ToEmail} failed attempt {Attempt}. Will retry at {NextAttempt}. Error: {Error}",
                                msg.Id, msg.ToEmail, msg.AttemptCount, msg.NextAttemptAt, msg.LastError);
                        }
                    }
                }
                catch (Exception ex)
                {
                    msg.LastError = ex.Message;
                    msg.ProcessedAt = DateTime.UtcNow;

                    if (msg.AttemptCount >= msg.MaxAttempts)
                    {
                        msg.Status = EmailOutboxStatus.Failed;
                    }
                    else
                    {
                        msg.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, msg.AttemptCount));
                        msg.Status = EmailOutboxStatus.Failed;
                    }
                    _logger.LogError(ex, "Unhandled exception dispatching email #{Id} to {ToEmail}", msg.Id, msg.ToEmail);
                }

                await _context.SaveChangesAsync(cancellationToken);
                processedCount++;
            }

            return processedCount;
        }

        public async Task<EmailOutboxMessage?> DispatchSingleAsync(long messageId, CancellationToken cancellationToken = default)
        {
            var msg = await _context.EmailOutboxMessages.FirstOrDefaultAsync(e => e.Id == messageId, cancellationToken);
            if (msg == null) return null;

            msg.AttemptCount++;
            msg.Status = EmailOutboxStatus.Processing;
            msg.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                var result = await _emailSender.SendEmailAsync(msg.ToEmail, msg.Subject, msg.HtmlBody, msg.PlainTextBody, cancellationToken);
                if (result.Success)
                {
                    msg.Status = EmailOutboxStatus.Sent;
                    msg.LastError = null;
                }
                else if (result.IsBlocked)
                {
                    msg.Status = EmailOutboxStatus.Blocked;
                    msg.LastError = result.ErrorMessage;
                }
                else
                {
                    msg.Status = EmailOutboxStatus.Failed;
                    msg.LastError = result.ErrorMessage;
                    if (msg.AttemptCount < msg.MaxAttempts)
                    {
                        msg.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, msg.AttemptCount));
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Status = EmailOutboxStatus.Failed;
                msg.LastError = ex.Message;
                if (msg.AttemptCount < msg.MaxAttempts)
                {
                    msg.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, msg.AttemptCount));
                }
            }

            msg.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return msg;
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
