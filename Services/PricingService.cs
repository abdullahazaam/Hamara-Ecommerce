using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class PricingService : IPricingService
    {
        private const int MaxAllowedQuantityPerItem = 50;
        private const decimal DefaultSalesTaxRate = 0.08m; // 8% sales tax
        private const decimal StandardShippingFee = 15.00m;
        private const decimal FreeShippingThreshold = 100.00m;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<PricingService> _logger;

        public PricingService(ApplicationDbContext context, ILogger<PricingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ShoppingCartViewModel> CalculateCartAsync(CartData cartData, string? userId = null)
        {
            var result = new ShoppingCartViewModel
            {
                AppliedCouponCode = cartData.AppliedCouponCode?.Trim().ToUpperInvariant()
            };

            if (cartData.Items == null || !cartData.Items.Any())
            {
                result.ShippingFee = 0m;
                result.EffectiveShippingFee = 0m;
                return result;
            }

            // Extract all unique product IDs to load in a single query
            var productIds = cartData.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var itemData in cartData.Items)
            {
                var itemVm = new CartItemViewModel
                {
                    ProductId = itemData.ProductId,
                    VariantId = itemData.VariantId
                };

                // 1. Verify Product Existence & Publication State
                if (!products.TryGetValue(itemData.ProductId, out var product) || product.Status != ProductStatus.Published)
                {
                    itemVm.IsAvailable = false;
                    itemVm.Title = product?.Title ?? $"Product #{itemData.ProductId}";
                    itemVm.ImageUrl = product?.MainImage ?? string.Empty;
                    itemVm.WarningMessage = "This product is no longer available in our store catalog.";
                    itemVm.Quantity = 0;
                    itemVm.UnitPrice = 0m;
                    result.Items.Add(itemVm);
                    continue;
                }

                itemVm.Title = product.Title;
                itemVm.SKU = product.SKU;
                itemVm.ImageUrl = product.MainImage;
                itemVm.CategoryName = product.CategoryName;
                itemVm.CategoryId = product.CategoryId;
                itemVm.Brand = product.Brand;
                itemVm.OldPrice = Math.Max(0m, product.OldPrice);

                // 2. Verify Variant State (if variant was chosen)
                ProductVariant? variant = null;
                int availableStock = product.Stock;
                decimal authoritativeUnitPrice = product.Price;

                if (itemData.VariantId.HasValue && itemData.VariantId.Value > 0)
                {
                    variant = product.Variants.FirstOrDefault(v => v.Id == itemData.VariantId.Value);
                    if (variant == null || !variant.IsActive)
                    {
                        itemVm.IsAvailable = false;
                        itemVm.WarningMessage = "The selected variant configuration is no longer available.";
                        itemVm.Quantity = 0;
                        itemVm.UnitPrice = 0m;
                        result.Items.Add(itemVm);
                        continue;
                    }

                    itemVm.VariantName = variant.Name;
                    itemVm.SKU = variant.SKU;
                    availableStock = variant.Stock;
                    authoritativeUnitPrice = Math.Max(0m, product.Price + variant.PriceAdjustment);
                }

                itemVm.UnitPrice = authoritativeUnitPrice;
                itemVm.AvailableStock = availableStock;

                // 3. Stock & Quantity Validation
                if (availableStock <= 0)
                {
                    itemVm.IsAvailable = false;
                    itemVm.Quantity = 0;
                    itemVm.WarningMessage = "This item is currently out of stock.";
                }
                else
                {
                    // Enforce quantity bounds: min 1, max per item 50, and cannot exceed stock
                    int requestedQty = itemData.Quantity;
                    if (requestedQty < 1)
                    {
                        requestedQty = 1;
                    }

                    if (requestedQty > MaxAllowedQuantityPerItem)
                    {
                        requestedQty = MaxAllowedQuantityPerItem;
                        itemVm.WarningMessage = $"Maximum allowed quantity per order is {MaxAllowedQuantityPerItem}.";
                    }

                    if (requestedQty > availableStock)
                    {
                        itemVm.Quantity = availableStock;
                        itemVm.WarningMessage = $"Requested quantity was adjusted to available stock ({availableStock} units remaining).";
                    }
                    else
                    {
                        itemVm.Quantity = requestedQty;
                    }
                }

                result.Items.Add(itemVm);
            }

            // 4. Calculate Authoritative Financial Subtotal
            var activeItems = result.Items.Where(i => i.IsAvailable && i.Quantity > 0).ToList();
            result.SubTotal = Math.Max(0m, activeItems.Sum(i => i.LineTotal));
            result.ProductDiscountTotal = Math.Max(0m, activeItems.Sum(i => i.SavingsTotal));

            // 5. Coupon Evaluation
            if (!string.IsNullOrWhiteSpace(result.AppliedCouponCode) && result.SubTotal > 0)
            {
                var couponCheck = await ValidateCouponAsync(result.AppliedCouponCode, result.SubTotal, userId, cartData.Items);
                if (couponCheck.IsValid && couponCheck.Coupon != null)
                {
                    result.CouponIsValid = true;
                    result.CouponDescription = couponCheck.Coupon.Description;
                    result.CouponDiscountPercentage = couponCheck.Coupon.DiscountPercentage;
                    result.CouponFixedDiscountAmount = couponCheck.Coupon.FixedDiscountAmount;
                    result.CouponGrantsFreeShipping = couponCheck.Coupon.FreeShipping;
                    
                    // Strictly clamp coupon discount so it never exceeds SubTotal and is non-negative
                    result.CouponDiscountAmount = Math.Min(result.SubTotal, Math.Max(0m, couponCheck.CalculatedDiscount));
                }
                else
                {
                    result.CouponIsValid = false;
                    result.CouponValidationMessage = couponCheck.Message;
                    result.CouponDiscountAmount = 0m;
                }
            }
            else
            {
                result.CouponDiscountAmount = 0m;
            }

            // 6. Tax Calculation (on taxable subtotal)
            decimal taxableAmount = Math.Max(0m, result.SubTotal - result.CouponDiscountAmount);
            result.EstimatedTax = Math.Max(0m, Math.Round(taxableAmount * DefaultSalesTaxRate, 2));

            // 7. Shipping Calculation
            result.ShippingFee = StandardShippingFee;
            if (result.SubTotal == 0 || result.SubTotal >= FreeShippingThreshold || result.CouponGrantsFreeShipping || result.AppliedCouponCode == "FREESHIP")
            {
                result.EffectiveShippingFee = 0.00m;
            }
            else
            {
                result.EffectiveShippingFee = StandardShippingFee;
            }

            // 8. Authoritative Grand Total (strictly non-negative)
            result.GrandTotal = Math.Max(0m, (result.SubTotal - result.CouponDiscountAmount) + result.EstimatedTax + result.EffectiveShippingFee);

            return result;
        }

        public async Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal subtotal, string? userId = null, List<CartItemData>? items = null)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return new CouponValidationResult { IsValid = false, Message = "Coupon code cannot be empty." };
            }

            var code = couponCode.Trim().ToUpperInvariant();
            var coupon = await _context.Coupons
                .Include(c => c.ApplicableCategory)
                .Include(c => c.ApplicableProduct)
                .FirstOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
            {
                return new CouponValidationResult { IsValid = false, Message = "Invalid coupon code." };
            }

            // Rule 1: Active Status
            if (!coupon.IsActive)
            {
                return new CouponValidationResult { IsValid = false, Message = "This coupon code is no longer active." };
            }

            // Rule 2: Start and Expiry Dates
            var now = DateTime.UtcNow;
            if (now < coupon.StartDate)
            {
                return new CouponValidationResult { IsValid = false, Message = $"This coupon is not active yet (starts {coupon.StartDate:MMM dd, yyyy})." };
            }

            if (now > coupon.ExpiryDate)
            {
                return new CouponValidationResult { IsValid = false, Message = $"This coupon expired on {coupon.ExpiryDate:MMM dd, yyyy}." };
            }

            // Rule 3: Total Usage Limit
            if (coupon.UsageCount >= coupon.UsageLimit)
            {
                return new CouponValidationResult { IsValid = false, Message = "This coupon has reached its maximum global redemption limit." };
            }

            // Rule 4: Per-User Redemption Limit
            if (!string.IsNullOrEmpty(userId))
            {
                int userRedemptionCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId && o.CouponCode == code);

                if (userRedemptionCount >= coupon.PerUserLimit)
                {
                    return new CouponValidationResult { IsValid = false, Message = $"You have already redeemed this coupon the maximum allowed times ({coupon.PerUserLimit} per account)." };
                }
            }

            // Rule 5: Minimum Spend Requirement
            if (subtotal < coupon.MinimumSpend)
            {
                return new CouponValidationResult { IsValid = false, Message = $"This coupon requires a minimum subtotal of {coupon.MinimumSpend:C} (current subtotal is {subtotal:C})." };
            }

            // Rule 6: Applicable Category Restriction
            if (coupon.ApplicableCategoryId.HasValue && items != null && items.Any())
            {
                var pIds = items.Select(i => i.ProductId).ToList();
                bool hasCategoryMatch = await _context.Products
                    .AnyAsync(p => pIds.Contains(p.Id) && p.CategoryId == coupon.ApplicableCategoryId.Value);

                if (!hasCategoryMatch)
                {
                    return new CouponValidationResult { IsValid = false, Message = $"This coupon is only valid for products in the '{coupon.ApplicableCategory?.Name ?? "specified"}' category." };
                }
            }

            // Rule 7: Applicable Product Restriction
            if (coupon.ApplicableProductId.HasValue && items != null && items.Any())
            {
                bool hasProductMatch = items.Any(i => i.ProductId == coupon.ApplicableProductId.Value);
                if (!hasProductMatch)
                {
                    return new CouponValidationResult { IsValid = false, Message = $"This coupon is only valid for '{coupon.ApplicableProduct?.Title ?? "the specified product"}'." };
                }
            }

            // Rule 8: Calculate Valid Discount
            decimal discount = 0m;
            if (coupon.DiscountPercentage > 0)
            {
                decimal percentageDiscount = subtotal * (decimal)(coupon.DiscountPercentage / 100.0);
                if (coupon.MaxDiscountAmount.HasValue && coupon.MaxDiscountAmount.Value > 0)
                {
                    percentageDiscount = Math.Min(percentageDiscount, coupon.MaxDiscountAmount.Value);
                }
                discount = percentageDiscount;
            }
            else if (coupon.FixedDiscountAmount > 0)
            {
                discount = coupon.FixedDiscountAmount;
            }

            // Discount can never exceed subtotal
            discount = Math.Min(subtotal, Math.Max(0m, discount));

            return new CouponValidationResult
            {
                IsValid = true,
                Coupon = coupon,
                CalculatedDiscount = discount,
                Message = $"Coupon '{coupon.Code}' applied successfully!"
            };
        }

        public async Task<bool> RecordCouponRedemptionAsync(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode)) return false;

            var code = couponCode.Trim().ToUpperInvariant();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon != null)
            {
                coupon.UsageCount++;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Recorded redemption for coupon {Code}. Total usages: {Count}/{Limit}", coupon.Code, coupon.UsageCount, coupon.UsageLimit);
                return true;
            }
            return false;
        }
    }
}
