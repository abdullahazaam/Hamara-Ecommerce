using System;
using System.Threading.Tasks;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class PaymentProcessingRequest
    {
        public string IdempotencyKey { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PKR";
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        // Ephemeral in-memory test card fields for Sandbox (NEVER logged or stored in database)
        public string? CardholderName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvc { get; set; }
        public bool SimulateFailure { get; set; } = false;
    }

    public class PaymentProcessingResult
    {
        public bool Success { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string Provider { get; set; } = "CashOnDelivery";
        public string? ProviderReference { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public string? FailureReason { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class PaymentMethodOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string? BadgeText { get; set; }
        public bool IsTestOnly { get; set; } = false;
        public bool RequiresCardInputs { get; set; } = false;
    }

    public interface IPaymentGateway
    {
        bool IsDevelopmentSandboxAvailable { get; }
        System.Collections.Generic.IEnumerable<PaymentMethodOption> GetAvailablePaymentMethods();
        bool IsMethodSupported(string paymentMethod);
        Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentProcessingRequest request);
    }
}
