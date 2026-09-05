using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class PricingAndCouponTests
    {
        private (PricingService pricingService, IShippingTaxService shippingTaxService) CreatePricingServices(ApplicationDbContext context)
        {
            var shippingTaxService = new ShippingTaxService();
            var pricingService = new PricingService(context, shippingTaxService, NullLogger<PricingService>.Instance);
            return (pricingService, shippingTaxService);
        }

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

            var (pricingService, _) = CreatePricingServices(context);

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

            var (pricingService, _) = CreatePricingServices(context);

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

            var (pricingService, _) = CreatePricingServices(context);

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

            var (pricingService, _) = CreatePricingServices(context);

            // Act: 50% of 100,000 = 50,000, but capped at 2,000
            var result = await pricingService.ValidateCouponAsync("MEGA50", 100000m, null, new List<CartItemData>());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(2000m, result.CalculatedDiscount);
        }

        [Fact]
        public async Task PricingService_CalculatesMatchingCartAndCheckoutTotals_WithAuthoritativeSettings()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Products.Add(new Product
            {
                Id = 10,
                Title = "Smart Watch Active",
                Price = 4000m,
                Stock = 20,
                Status = ProductStatus.Published,
                CategoryName = "Wearables"
            });
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                Items = new List<CartItemData> { new() { ProductId = 10, Quantity = 1 } }
            };

            // Act: Calculate standard delivery (4000 subtotal < 5000 free shipping threshold => 250 fee)
            var cart = await pricingService.CalculateCartAsync(cartData, null, "Standard");

            // Assert: Subtotal = 4000, Shipping = 250, Tax (5%) = 200, Grand Total = 4450
            Assert.Equal(4000m, cart.SubTotal);
            Assert.Equal(250m, cart.EffectiveShippingFee);
            Assert.Equal(200m, cart.EstimatedTax);
            Assert.Equal(4450m, cart.GrandTotal);

            // Verify formatted strings
            Assert.Equal("Rs. 4,000.00", cart.FormattedSubTotal);
            Assert.Equal("Rs. 250.00", cart.FormattedShipping);
            Assert.Equal("Rs. 200.00", cart.FormattedTax);
            Assert.Equal("Rs. 4,450.00", cart.FormattedGrandTotal);
        }

        [Fact]
        public async Task PricingService_DeliveryMethodChange_RecalculatesAccurately()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Products.Add(new Product
            {
                Id = 11,
                Title = "Leather Messenger Bag",
                Price = 6000m, // >= 5000 threshold => standard shipping qualifies for free
                Stock = 10,
                Status = ProductStatus.Published,
                CategoryName = "Fashion"
            });
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                Items = new List<CartItemData> { new() { ProductId = 11, Quantity = 1 } }
            };

            // Standard: Free shipping
            var standardCart = await pricingService.CalculateCartAsync(cartData, null, "Standard");
            Assert.Equal(0m, standardCart.EffectiveShippingFee);
            Assert.Equal("FREE", standardCart.FormattedShipping);
            Assert.Equal(300m, standardCart.EstimatedTax); // 5% of 6000
            Assert.Equal(6300m, standardCart.GrandTotal);

            // Express: 500 fee
            var expressCart = await pricingService.CalculateCartAsync(cartData, null, "Express");
            Assert.Equal(500m, expressCart.EffectiveShippingFee);
            Assert.Equal("Rs. 500.00", expressCart.FormattedShipping);
            Assert.Equal(6800m, expressCart.GrandTotal);

            // Overnight: 1000 fee
            var overnightCart = await pricingService.CalculateCartAsync(cartData, null, "Overnight");
            Assert.Equal(1000m, overnightCart.EffectiveShippingFee);
            Assert.Equal("Rs. 1,000.00", overnightCart.FormattedShipping);
            Assert.Equal(7300m, overnightCart.GrandTotal);
        }

        [Fact]
        public void ShippingTaxService_PreservesHistoricalOrderCurrency()
        {
            // Arrange
            var service = new ShippingTaxService();

            // Act & Assert default PKR
            Assert.Equal("Rs. 1,234.50", service.FormatCurrency(1234.50m));
            Assert.Equal("Rs. 1,234.50", service.FormatCurrency(1234.50m, "PKR"));

            // Historical USD, EUR, GBP
            Assert.Equal("$1,234.50", service.FormatCurrency(1234.50m, "USD"));
            Assert.Equal("€1,234.50", service.FormatCurrency(1234.50m, "EUR"));
            Assert.Equal("£1,234.50", service.FormatCurrency(1234.50m, "GBP"));
        }
    }
}
