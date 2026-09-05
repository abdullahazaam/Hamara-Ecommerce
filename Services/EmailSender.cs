using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HamaraCommerce.Services
{
    public class EmailSendResult
    {
        public bool Success { get; set; }
        public bool IsBlocked { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsDevelopmentFallback { get; set; }

        public static EmailSendResult Succeeded(string? messageId = null, bool isDev = false) =>
            new() { Success = true, MessageId = messageId ?? Guid.NewGuid().ToString("N"), IsDevelopmentFallback = isDev };

        public static EmailSendResult Failed(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static EmailSendResult Blocked(string reason) =>
            new() { Success = false, IsBlocked = true, ErrorMessage = reason };
    }

    public interface IEmailSender
    {
        Task<EmailSendResult> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null, CancellationToken cancellationToken = default);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly IHostEnvironment _env;

        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger, IHostEnvironment env)
        {
            _config = config;
            _logger = logger;
            _env = env;
        }

        public async Task<EmailSendResult> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return EmailSendResult.Failed("Recipient email cannot be empty.");
            }

            var host = _config["Smtp:Host"] ?? Environment.GetEnvironmentVariable("SMTP_HOST");
            var portStr = _config["Smtp:Port"] ?? Environment.GetEnvironmentVariable("SMTP_PORT");
            var user = _config["Smtp:User"] ?? Environment.GetEnvironmentVariable("SMTP_USER");
            var pass = _config["Smtp:Password"] ?? Environment.GetEnvironmentVariable("SMTP_PASS");
            var fromEmail = _config["Smtp:FromEmail"] ?? Environment.GetEnvironmentVariable("SMTP_FROM") ?? "support@hamaracommerce.pk";
            var fromName = _config["Smtp:FromName"] ?? "Hamara Commerce Support";
            var enableSsl = bool.TryParse(_config["Smtp:EnableSsl"] ?? Environment.GetEnvironmentVariable("SMTP_SSL"), out var ssl) ? ssl : true;

            // If SMTP is not configured, honestly report failure / blocked delivery
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                _logger.LogWarning("[SMTP NOT CONFIGURED] Delivery BLOCKED for recipient {ToEmail} (Subject: {Subject}). Missing host/user/password credentials.", toEmail, subject);
                return EmailSendResult.Blocked("SMTP credentials are not configured. Email delivery is blocked.");
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(toEmail));

                if (!string.IsNullOrWhiteSpace(plainTextBody))
                {
                    var altView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                    message.AlternateViews.Add(altView);
                }

                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Successfully sent email to {ToEmail} with subject: {Subject}", toEmail, subject);
                return EmailSendResult.Succeeded(Guid.NewGuid().ToString("N"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP host {Host}", toEmail, host);
                return EmailSendResult.Failed($"Email delivery failed: {ex.Message}");
            }
        }
    }
}
