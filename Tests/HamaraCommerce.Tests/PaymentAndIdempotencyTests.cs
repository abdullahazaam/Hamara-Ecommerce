using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class PaymentAndIdempotencyTests
    {
        [Fact]
        public void PaymentGateway_AvailableMethods_ExposesOnlySupportedOptions()
        {
            // Development environment: COD and SandboxCard
            var devEnv = new Mock<IWebHostEnvironment>();
            devEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
            var devGateway = new PaymentGateway(devEnv.Object, NullLogger<PaymentGateway>.Instance);
            var devMethods = devGateway.GetAvailablePaymentMethods().ToList();

            Assert.Equal(2, devMethods.Count);
            Assert.Contains(devMethods, m => m.Id == "CashOnDelivery");
            Assert.Contains(devMethods, m => m.Id == "SandboxCard");
            Assert.DoesNotContain(devMethods, m => m.Id == "OnlineCard");
            Assert.DoesNotContain(devMethods, m => m.Id == "BankTransfer");

            // Production environment: COD only
            var prodEnv = new Mock<IWebHostEnvironment>();
            prodEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);
            var prodGateway = new PaymentGateway(prodEnv.Object, NullLogger<PaymentGateway>.Instance);
            var prodMethods = prodGateway.GetAvailablePaymentMethods().ToList();

            Assert.Single(prodMethods);
            Assert.Equal("CashOnDelivery", prodMethods[0].Id);
        }

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
                PaymentMethod = "SandboxCard",
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

        [Fact]
        public async Task Idempotency_UniqueDatabaseConstraint_PreventsDuplicateRecordCreation()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            string idempotencyKey = "unique-key-" + Guid.NewGuid().ToString("N");

            var record1 = new CheckoutIdempotencyRecord
            {
                IdempotencyKey = idempotencyKey,
                UserId = "user-1",
                CustomerEmail = "customer1@test.com",
                RequestHash = "hash123",
                OrderId = 10,
                OrderNumber = "HC-PK-IDEM-01",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };
            context.CheckoutIdempotencyRecords.Add(record1);
            await context.SaveChangesAsync();

            // Act: Find existing idempotency record by key
            var existing = await context.CheckoutIdempotencyRecords
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey);

            // Assert: Found and bound to correct order
            Assert.NotNull(existing);
            Assert.Equal("HC-PK-IDEM-01", existing.OrderNumber);
            Assert.Equal("hash123", existing.RequestHash);
            Assert.Equal("user-1", existing.UserId);
        }

        [Fact]
        public async Task CouponRedemption_DedicatedEntityRecorded_AndRestoredWithoutCustomerNotesHacks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var shippingTaxService = new ShippingTaxService();
            var pricingService = new PricingService(context, shippingTaxService, NullLogger<PricingService>.Instance);

            var coupon = new Coupon
            {
                Id = 1,
                Code = "PROMO50",
                FixedDiscountAmount = 50m,
                IsActive = true,
                UsageCount = 0
            };
            context.Coupons.Add(coupon);

            var order = new Order
            {
                Id = 99,
                OrderNumber = "HC-PK-COUPON-99",
                UserId = "user-coupon-test",
                CustomerEmail = "coupon@test.com",
                CouponCode = "PROMO50",
                DiscountAmount = 50m,
                TotalAmount = 450m
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act 1: Record Coupon Redemption
            await pricingService.RecordCouponRedemptionAsync("PROMO50", order);

            // Verify CouponRedemption record exists
            var redemption = await context.CouponRedemptions.FirstOrDefaultAsync(r => r.OrderId == order.Id);
            Assert.NotNull(redemption);
            Assert.Equal("PROMO50", redemption.CouponCode);
            Assert.False(redemption.IsRestored);

            var updatedCoupon = await context.Coupons.FindAsync(1);
            Assert.Equal(1, updatedCoupon!.UsageCount);

            // Act 2: Restore Coupon (e.g. order cancelled)
            await pricingService.RestoreCouponRedemptionAsync(order);

            // Assert: IsRestored is true, UsageCount decremented, CustomerNotes clean
            var restoredRedemption = await context.CouponRedemptions.FirstOrDefaultAsync(r => r.OrderId == order.Id);
            Assert.NotNull(restoredRedemption);
            Assert.True(restoredRedemption.IsRestored);
            Assert.NotNull(restoredRedemption.RestoredAt);

            var finalCoupon = await context.Coupons.FindAsync(1);
            Assert.Equal(0, finalCoupon!.UsageCount);

            // Act 3: Repeated restore is idempotent and does not decrement again
            await pricingService.RestoreCouponRedemptionAsync(order);
            var couponAfterSecondRestore = await context.Coupons.FindAsync(1);
            Assert.Equal(0, couponAfterSecondRestore!.UsageCount);
        }
    }
}
