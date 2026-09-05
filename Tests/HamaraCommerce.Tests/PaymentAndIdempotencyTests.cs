using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class PaymentAndIdempotencyTests
    {
        [Fact]
        public async Task PaymentGateway_RejectsSimulatedFailure_WithHonestReason()
        {
            // Arrange
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);

            var gateway = new PaymentGateway(mockEnv.Object, NullLogger<PaymentGateway>.Instance);
            var request = new PaymentProcessingRequest
            {
                OrderNumber = "HC-PK-FAIL-01",
                Amount = 15000m,
                Currency = "PKR",
                CustomerName = "Test Buyer",
                CustomerEmail = "buyer@test.com",
                PaymentMethod = "CreditCard",
                SimulateFailure = true
            };

            // Act
            var result = await gateway.ProcessPaymentAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(PaymentStatus.Failed, result.Status);
            Assert.Contains("declined", result.FailureReason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PaymentGateway_ProcessesCod_AsPendingConfirmation()
        {
            // Arrange
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);

            var gateway = new PaymentGateway(mockEnv.Object, NullLogger<PaymentGateway>.Instance);
            var request = new PaymentProcessingRequest
            {
                OrderNumber = "HC-PK-COD-01",
                Amount = 8500m,
                Currency = "PKR",
                CustomerName = "COD Customer",
                CustomerEmail = "cod@test.com",
                PaymentMethod = "CashOnDelivery"
            };

            // Act
            var result = await gateway.ProcessPaymentAsync(request);

            // Assert: COD must be marked pending on creation, never automatically Paid
            Assert.True(result.Success);
            Assert.Equal(PaymentStatus.Pending, result.Status);
        }
    }
}
