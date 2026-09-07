using System;
using System.Threading;
using System.Threading.Tasks;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class OutboxRetryResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public EmailOutboxMessage? EmailMessage { get; set; }
        public EmailOutboxStatus? PriorStatus { get; set; }
        public bool AlreadySent { get; set; }

        public static OutboxRetryResult Succeeded(EmailOutboxMessage msg, EmailOutboxStatus priorStatus) =>
            new()
            {
                Success = true,
                EmailMessage = msg,
                PriorStatus = priorStatus,
                Message = $"Outbox email #{msg.Id} reset to Queued status for immediate retry."
            };

        public static OutboxRetryResult Failed(string message, bool alreadySent = false, EmailOutboxStatus? priorStatus = null) =>
            new()
            {
                Success = false,
                Message = message,
                AlreadySent = alreadySent,
                PriorStatus = priorStatus
            };
    }

    public class PaymentReconciliationRequest
    {
        public int Id { get; set; }
        public string? Outcome { get; set; } // Paid, Failed, Cancelled, Refunded
        public string? AdminNotes { get; set; }
        public string? ProviderReference { get; set; }
        public string? ProviderName { get; set; }
        public string? Action { get; set; } // Legacy form action if sent (e.g. MarkResolved)
    }

    public class PaymentReconciliationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CheckoutIdempotencyRecord? Record { get; set; }
        public IdempotencyStatus OldStatus { get; set; }
        public IdempotencyStatus NewStatus { get; set; }
        public PaymentStatus? OldPaymentStatus { get; set; }
        public PaymentStatus? NewPaymentStatus { get; set; }

        public static PaymentReconciliationResult Succeeded(
            CheckoutIdempotencyRecord record,
            IdempotencyStatus oldStatus,
            IdempotencyStatus newStatus,
            PaymentStatus? oldPaymentStatus,
            PaymentStatus? newPaymentStatus,
            string message) =>
            new()
            {
                Success = true,
                Record = record,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                OldPaymentStatus = oldPaymentStatus,
                NewPaymentStatus = newPaymentStatus,
                Message = message
            };

        public static PaymentReconciliationResult Failed(string message, CheckoutIdempotencyRecord? record = null) =>
            new()
            {
                Success = false,
                Message = message,
                Record = record
            };
    }

    public interface IOperationalRecoveryService
    {
        Task<OutboxRetryResult> RetryOutboxEmailAsync(long id, string? adminUserId = null, string? adminUserName = null, CancellationToken cancellationToken = default);
        Task<PaymentReconciliationResult> ReconcilePaymentAsync(PaymentReconciliationRequest request, string? adminUserId = null, string? adminUserName = null, CancellationToken cancellationToken = default);
    }
}
