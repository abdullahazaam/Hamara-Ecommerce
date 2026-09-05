using System;

namespace HamaraCommerce.Models
{
    public enum IdempotencyStatus
    {
        Processing = 0,
        PaymentCompleted = 1,
        Completed = 2,
        Failed = 3,
        RecoveryRequired = 4
    }

    public class CheckoutIdempotencyRecord
    {
        public int Id { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string RequestHash { get; set; } = string.Empty;
        public IdempotencyStatus Status { get; set; } = IdempotencyStatus.Processing;

        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string? GuestAccessToken { get; set; }

        public string? PaymentProvider { get; set; }
        public string? PaymentReference { get; set; }
        public decimal? PaymentAmount { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }

        public string? FailureReason { get; set; }
        public DateTime? LockedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    }
}
