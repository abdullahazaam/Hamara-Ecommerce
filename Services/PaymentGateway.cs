using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class PaymentGateway : IPaymentGateway
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PaymentGateway> _logger;

        public PaymentGateway(IWebHostEnvironment env, ILogger<PaymentGateway> logger)
        {
            _env = env;
            _logger = logger;
        }

        public bool IsDevelopmentSandboxAvailable => _env.IsDevelopment();

        public async Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentProcessingRequest request)
        {
            await Task.Delay(100); // Simulate network latency

            var method = (request.PaymentMethod ?? string.Empty).Trim();

            // 1. CASH ON DELIVERY
            if (string.Equals(method, "CashOnDelivery", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(method, "COD", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Processing Cash on Delivery for Order {OrderNumber}, Amount: {Amount} {Currency}", 
                    request.OrderNumber, request.Amount, request.Currency);

                return new PaymentProcessingResult
                {
                    Success = true,
                    Status = PaymentStatus.Pending,
                    Provider = "CashOnDelivery",
                    ProviderReference = $"COD-{Guid.NewGuid():N}"[..18].ToUpperInvariant(),
                    ProcessedAt = DateTime.UtcNow
                };
            }

            // 2. DEVELOPMENT SANDBOX CARD GATEWAY
            if (string.Equals(method, "SandboxCard", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(method, "CreditCard", StringComparison.OrdinalIgnoreCase))
            {
                if (!_env.IsDevelopment())
                {
                    _logger.LogWarning("Attempted to use Development Sandbox payment provider outside Development environment for Order {OrderNumber}.", request.OrderNumber);
                    return new PaymentProcessingResult
                    {
                        Success = false,
                        Status = PaymentStatus.Failed,
                        Provider = "Development Sandbox Gateway",
                        FailureReason = "Online payment gateway is not configured for production use. Please select Cash on Delivery."
                    };
                }

                _logger.LogInformation("Processing Sandbox Payment for Order {OrderNumber}, Amount: {Amount} {Currency}", 
                    request.OrderNumber, request.Amount, request.Currency);

                string cardNum = (request.CardNumber ?? string.Empty).Replace(" ", "").Replace("-", "");
                string last4 = cardNum.Length >= 4 ? cardNum[^4..] : "4242";

                // Check for simulated failure
                if (request.SimulateFailure || 
                    cardNum.EndsWith("0000") || 
                    string.Equals(request.CardholderName, "DECLINE", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Sandbox card payment simulated failure for Order {OrderNumber}.", request.OrderNumber);
                    return new PaymentProcessingResult
                    {
                        Success = false,
                        Status = PaymentStatus.Failed,
                        Provider = "Development Sandbox Gateway",
                        CardLast4 = last4,
                        CardBrand = "Visa",
                        FailureReason = "Payment was declined by simulated issuing bank (Sandbox Test Rule).",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Successful Sandbox Authorization
                return new PaymentProcessingResult
                {
                    Success = true,
                    Status = PaymentStatus.Paid,
                    Provider = "Development Sandbox Gateway",
                    ProviderReference = $"SBX-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                    CardLast4 = last4,
                    CardBrand = cardNum.StartsWith("5") ? "Mastercard" : "Visa",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            // 3. UNKNOWN / UNSUPPORTED PROVIDER
            return new PaymentProcessingResult
            {
                Success = false,
                Status = PaymentStatus.Failed,
                Provider = "Unknown",
                FailureReason = $"The payment method '{request.PaymentMethod}' is currently not available."
            };
        }
    }
}
