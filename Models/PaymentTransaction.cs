using System;

namespace HamaraCommerce.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string TransactionReference { get; set; } = string.Empty;
        public string Provider { get; set; } = "CashOnDelivery";
        public string? ProviderTransactionId { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PKR";
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        // Sanitized card metadata for customer receipt display (NEVER store raw card/CVC/expiry)
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }

        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
