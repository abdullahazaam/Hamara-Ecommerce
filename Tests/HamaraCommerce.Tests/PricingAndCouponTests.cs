using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class PricingAndCouponTests
    {
        [Fact]
        public async Task CalculateCartAsync_RecalculatesFromDatabase_IgnoringClientPricing()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var product = new Product
            {
                Id = 1,
                Title = "Wireless Noise Cancelling Headphones",
                Price = 15000m,
                OldPrice = 18000m,
                Stock = 10,
                Status = ProductStatus.Published,
                CategoryName = "Electronics"
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var pricingService = new PricingService(context, NullLogger<PricingService>.Instance);

            var cartData = new CartData
            {
                Items = new List<CartItemData>
                {
                    new() { ProductId = 1, Quantity = 2 }
                }
            };

            // Act
            var cart = await pricingService.CalculateCartAsync(cartData, null);

            // Assert
            Assert.Single(cart.Items);
            Assert.Equal(15000m, cart.Items[0].UnitPrice);
            Assert.Equal(30000m, cart.SubTotal);
            Assert.True(cart.GrandTotal > 0);
        }

        [Fact]
        public async Task ValidateCouponAsync_RejectsExpiredCoupon()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Coupons.Add(new Coupon
            {
                Id = 1,
                Code = "EXPIRED20",
                DiscountPercentage = 20,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();

            var pricingService = new PricingService(context, NullLogger<PricingService>.Instance);

            // Act
            var result = await pricingService.ValidateCouponAsync("EXPIRED20", 10000m, null, new List<CartItemData>());

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("expired", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ValidateCouponAsync_EnforcesMinimumSpendRequirement()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Coupons.Add(new Coupon
            {
                Id = 2,
                Code = "SAVE500",
                FixedDiscountAmount = 500,
                IsActive = true,
                MinimumSpend = 5000m
            });
            await context.SaveChangesAsync();

            var pricingService = new PricingService(context, NullLogger<PricingService>.Instance);

            // Act
            var result = await pricingService.ValidateCouponAsync("SAVE500", 3000m, null, new List<CartItemData>());

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("minimum", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ValidateCouponAsync_CapsDiscountAtMaximumLimit()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Coupons.Add(new Coupon
            {
                Id = 3,
                Code = "MEGA50",
                DiscountPercentage = 50, // 50%
                MaxDiscountAmount = 2000m, // Capped at 2000
                IsActive = true
            });
            await context.SaveChangesAsync();

            var pricingService = new PricingService(context, NullLogger<PricingService>.Instance);

            // Act: 50% of 100,000 = 50,000, but capped at 2,000
            var result = await pricingService.ValidateCouponAsync("MEGA50", 100000m, null, new List<CartItemData>());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(2000m, result.CalculatedDiscount);
        }
    }
}
