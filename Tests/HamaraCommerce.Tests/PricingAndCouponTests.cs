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

        [Fact]
        public async Task ValidateCouponAsync_MixedCategoryBasket_DiscountsOnlyEligibleItems()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var catElectronics = new Category { Id = 1, Name = "Electronics", Slug = "electronics" };
            var catFashion = new Category { Id = 2, Name = "Fashion", Slug = "fashion" };
            context.Categories.AddRange(catElectronics, catFashion);

            var prodHeadphones = new Product
            {
                Id = 101,
                Title = "Wireless Headphones",
                Price = 10000m,
                CategoryId = 1,
                Category = catElectronics,
                Status = ProductStatus.Published,
                Stock = 20
            };
            var prodShirt = new Product
            {
                Id = 102,
                Title = "Cotton Shirt",
                Price = 20000m,
                CategoryId = 2,
                Category = catFashion,
                Status = ProductStatus.Published,
                Stock = 20
            };
            context.Products.AddRange(prodHeadphones, prodShirt);

            var coupon = new Coupon
            {
                Id = 10,
                Code = "TECH20",
                DiscountPercentage = 20, // 20%
                ApplicableCategoryId = 1, // Restricted to Electronics
                ApplicableCategory = catElectronics,
                IsActive = true
            };
            context.Coupons.Add(coupon);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                Items = new List<CartItemData>
                {
                    new() { ProductId = 101, Quantity = 1 }, // 10,000 PKR (Eligible)
                    new() { ProductId = 102, Quantity = 1 }  // 20,000 PKR (Ineligible)
                }
            };

            // Act: Subtotal is 30,000. Eligible subtotal is 10,000. 20% discount = 2,000.
            var result = await pricingService.ValidateCouponAsync("TECH20", 30000m, null, null, cartData.Items);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(2000m, result.CalculatedDiscount); // 20% of 10,000 only, NOT 20% of 30,000 (6,000)
        }

        [Fact]
        public async Task ValidateCouponAsync_RestrictedCategory_NoEligibleItems_RejectsWithClearMessage()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var catElectronics = new Category { Id = 1, Name = "Electronics", Slug = "electronics" };
            var catFashion = new Category { Id = 2, Name = "Fashion", Slug = "fashion" };
            context.Categories.AddRange(catElectronics, catFashion);

            var prodShirt = new Product
            {
                Id = 102,
                Title = "Cotton Shirt",
                Price = 5000m,
                CategoryId = 2,
                Category = catFashion,
                Status = ProductStatus.Published,
                Stock = 20
            };
            context.Products.Add(prodShirt);

            var coupon = new Coupon
            {
                Id = 11,
                Code = "TECH20",
                DiscountPercentage = 20,
                ApplicableCategoryId = 1,
                ApplicableCategory = catElectronics,
                IsActive = true
            };
            context.Coupons.Add(coupon);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                Items = new List<CartItemData>
                {
                    new() { ProductId = 102, Quantity = 1 }
                }
            };

            // Act
            var result = await pricingService.ValidateCouponAsync("TECH20", 5000m, null, null, cartData.Items);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Electronics", result.Message);
            Assert.Contains("no matching items", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ValidateCouponAsync_RestrictedCategory_MinimumSpendOnEligibleItems_Enforced()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var catElectronics = new Category { Id = 1, Name = "Electronics", Slug = "electronics" };
            var catFashion = new Category { Id = 2, Name = "Fashion", Slug = "fashion" };
            context.Categories.AddRange(catElectronics, catFashion);

            var prodCable = new Product
            {
                Id = 103,
                Title = "USB Cable",
                Price = 2000m,
                CategoryId = 1,
                Category = catElectronics,
                Status = ProductStatus.Published,
                Stock = 20
            };
            var prodSuit = new Product
            {
                Id = 104,
                Title = "Wool Suit",
                Price = 25000m,
                CategoryId = 2,
                Category = catFashion,
                Status = ProductStatus.Published,
                Stock = 20
            };
            context.Products.AddRange(prodCable, prodSuit);

            var coupon = new Coupon
            {
                Id = 12,
                Code = "TECHSAVE",
                FixedDiscountAmount = 1000m,
                ApplicableCategoryId = 1,
                ApplicableCategory = catElectronics,
                MinimumSpend = 5000m, // Requires 5,000 on eligible items
                IsActive = true
            };
            context.Coupons.Add(coupon);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                Items = new List<CartItemData>
                {
                    new() { ProductId = 103, Quantity = 1 }, // 2,000 PKR eligible (< 5,000)
                    new() { ProductId = 104, Quantity = 1 }  // 25,000 PKR ineligible
                }
            };

            // Act: Total subtotal is 27,000 (which exceeds 5,000), but eligible subtotal is only 2,000 (< 5,000)
            var result = await pricingService.ValidateCouponAsync("TECHSAVE", 27000m, null, null, cartData.Items);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("minimum spend", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eligible", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ValidateCouponAsync_ExpiredFreeShippingCoupon_RejectsAndRevokesFreeShipping()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var prod = new Product
            {
                Id = 201,
                Title = "Running Shoes",
                Price = 3000m,
                Stock = 10,
                Status = ProductStatus.Published
            };
            context.Products.Add(prod);

            var expiredShipCoupon = new Coupon
            {
                Id = 13,
                Code = "EXPIREDSHIP",
                FreeShipping = true,
                DiscountPercentage = 0,
                FixedDiscountAmount = 0,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddDays(-2)
            };
            context.Coupons.Add(expiredShipCoupon);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                AppliedCouponCode = "EXPIREDSHIP",
                Items = new List<CartItemData> { new() { ProductId = 201, Quantity = 1 } }
            };

            // Act
            var cart = await pricingService.CalculateCartAsync(cartData, null, "Standard");

            // Assert
            Assert.False(cart.CouponIsValid);
            Assert.False(cart.CouponGrantsFreeShipping);
            Assert.Equal(0m, cart.CouponDiscountAmount);
            Assert.Equal(250m, cart.EffectiveShippingFee); // Standard fee of 250 applied since < 5000 free shipping threshold
            Assert.Contains("expired", cart.CouponValidationMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ValidateCouponAsync_ValidFreeShippingCoupon_GrantsFreeShipping()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var prod = new Product
            {
                Id = 202,
                Title = "Travel Mug",
                Price = 2000m, // Below the 5,000 standard free shipping threshold
                Stock = 10,
                Status = ProductStatus.Published
            };
            context.Products.Add(prod);

            var freeShipCoupon = new Coupon
            {
                Id = 14,
                Code = "SPECIALSHIP",
                FreeShipping = true,
                DiscountPercentage = 0,
                FixedDiscountAmount = 0,
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                ExpiryDate = DateTime.UtcNow.AddDays(30)
            };
            context.Coupons.Add(freeShipCoupon);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            var cartData = new CartData
            {
                AppliedCouponCode = "SPECIALSHIP",
                Items = new List<CartItemData> { new() { ProductId = 202, Quantity = 1 } }
            };

            // Act: Standard normally costs 250 (since 2,000 < 5,000 threshold), but coupon grants free shipping
            var cart = await pricingService.CalculateCartAsync(cartData, null, "Standard");

            // Assert
            Assert.True(cart.CouponIsValid);
            Assert.True(cart.CouponGrantsFreeShipping);
            Assert.Equal(0m, cart.EffectiveShippingFee);
            Assert.Equal("FREE", cart.FormattedShipping);
        }

        [Fact]
        public async Task ValidateCouponAsync_GuestOrder_EnforcesPerCustomerLimit()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var coupon = new Coupon
            {
                Id = 15,
                Code = "ONCEONLY",
                DiscountPercentage = 15,
                PerUserLimit = 1,
                IsActive = true
            };
            context.Coupons.Add(coupon);

            // Seed existing non-cancelled order by guest user
            var pastOrder = new Order
            {
                Id = 1,
                OrderNumber = "HC-PK-10001",
                CustomerEmail = "guest.shopper@example.com",
                UserId = null,
                CouponCode = "ONCEONLY",
                Status = OrderStatus.Confirmed,
                Subtotal = 10000m,
                TotalAmount = 8500m
            };
            context.Orders.Add(pastOrder);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            // Act 1: Same guest email tries to reuse coupon
            var resultBlocked = await pricingService.ValidateCouponAsync(
                "ONCEONLY", 10000m, userId: null, customerEmail: "guest.shopper@example.com", items: null);

            // Act 2: Different guest email uses the coupon
            var resultAllowed = await pricingService.ValidateCouponAsync(
                "ONCEONLY", 10000m, userId: null, customerEmail: "different.shopper@example.com", items: null);

            // Assert
            Assert.False(resultBlocked.IsValid);
            Assert.Contains("maximum allowed times", resultBlocked.Message, StringComparison.OrdinalIgnoreCase);

            Assert.True(resultAllowed.IsValid);
            Assert.Equal(1500m, resultAllowed.CalculatedDiscount);
        }

        [Fact]
        public async Task ValidateCouponAsync_GlobalUsageLimitReached_RejectsCoupon()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var coupon = new Coupon
            {
                Id = 16,
                Code = "LIMITED50",
                DiscountPercentage = 10,
                UsageLimit = 50,
                UsageCount = 50, // Limit reached
                IsActive = true
            };
            context.Coupons.Add(coupon);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            // Act
            var result = await pricingService.ValidateCouponAsync("LIMITED50", 10000m);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("maximum global redemption limit", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RestoreCouponRedemptionAsync_IdempotentOnCancellation()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var coupon = new Coupon
            {
                Id = 17,
                Code = "RESTOREME",
                UsageCount = 3,
                UsageLimit = 10,
                IsActive = true
            };
            context.Coupons.Add(coupon);

            var order = new Order
            {
                Id = 2,
                OrderNumber = "HC-PK-20002",
                CouponCode = "RESTOREME",
                CustomerNotes = "Initial order note",
                Status = OrderStatus.Confirmed
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var (pricingService, _) = CreatePricingServices(context);

            // Act 1: First cancellation call restores usage
            bool firstRestored = await pricingService.RestoreCouponRedemptionAsync(order);
            await context.SaveChangesAsync();

            // Assert 1: UsageCount decremented to 2
            Assert.True(firstRestored);
            Assert.Equal(2, coupon.UsageCount);
            Assert.Contains("[CouponRestored:RESTOREME]", order.CustomerNotes);

            // Act 2: Second cancellation call on same order (idempotent)
            bool secondRestored = await pricingService.RestoreCouponRedemptionAsync(order);
            await context.SaveChangesAsync();

            // Assert 2: Second call returns false, UsageCount stays at 2 (not decremented twice)
            Assert.False(secondRestored);
            Assert.Equal(2, coupon.UsageCount);
        }
    }
}
