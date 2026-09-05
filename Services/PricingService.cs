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

        private readonly ApplicationDbContext _context;
        private readonly IShippingTaxService _shippingTaxService;
        private readonly ILogger<PricingService> _logger;

        public PricingService(
            ApplicationDbContext context,
            IShippingTaxService shippingTaxService,
            ILogger<PricingService> logger)
        {
            _context = context;
            _shippingTaxService = shippingTaxService;
            _logger = logger;
        }

        public async Task<ShoppingCartViewModel> CalculateCartAsync(CartData cartData, string? userId = null, string? shippingMethod = null)
        {
            var storeSettings = _shippingTaxService.GetStoreSettings();
            var selectedShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? "Standard" : shippingMethod.Trim();

            var result = new ShoppingCartViewModel
            {
                AppliedCouponCode = cartData.AppliedCouponCode?.Trim().ToUpperInvariant(),
                CurrencyCode = storeSettings.CurrencyCode,
                CurrencySymbol = storeSettings.CurrencySymbol,
                TaxRatePercent = storeSettings.TaxRatePercent,
                ShippingMethodCode = selectedShippingMethod
            };

            if (cartData.Items == null || !cartData.Items.Any())
            {
                result.ShippingFee = 0m;
                result.EffectiveShippingFee = 0m;
                result.FormattedSubTotal = _shippingTaxService.FormatCurrency(0m, result.CurrencyCode, result.CurrencySymbol);
                result.FormattedDiscount = _shippingTaxService.FormatCurrency(0m, result.CurrencyCode, result.CurrencySymbol);
                result.FormattedTax = _shippingTaxService.FormatCurrency(0m, result.CurrencyCode, result.CurrencySymbol);
                result.FormattedShipping = _shippingTaxService.FormatCurrency(0m, result.CurrencyCode, result.CurrencySymbol);
                result.FormattedGrandTotal = _shippingTaxService.FormatCurrency(0m, result.CurrencyCode, result.CurrencySymbol);
                result.FormattedTotalSavings = _shippingTaxService.FormatCurrency(0m, result.CurrencyCode, result.CurrencySymbol);
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
                string? userEmail = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    userEmail = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();
                }

                var activeItemData = activeItems.Select(i => new CartItemData 
                { 
                    ProductId = i.ProductId, 
                    VariantId = i.VariantId, 
                    Quantity = i.Quantity 
                }).ToList();

                var couponCheck = await ValidateCouponAsync(result.AppliedCouponCode, result.SubTotal, userId, userEmail, activeItemData);
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
                    result.CouponGrantsFreeShipping = false;
                }
            }
            else
            {
                result.CouponDiscountAmount = 0m;
            }

            // 6. Tax Calculation (on taxable subtotal via authoritative store settings)
            decimal taxableAmount = Math.Max(0m, result.SubTotal - result.CouponDiscountAmount);
            result.EstimatedTax = _shippingTaxService.CalculateTax(taxableAmount);

            // 7. Shipping Calculation (authoritative via store settings & selected delivery tier)
            var (isShipValid, calculatedShippingFee, shippingName) = _shippingTaxService.CalculateShippingFee(
                result.ShippingMethodCode, 
                result.SubTotal, 
                result.CouponGrantsFreeShipping);

            result.ShippingMethodName = string.IsNullOrEmpty(shippingName) ? result.ShippingMethodCode : shippingName;
            result.ShippingFee = calculatedShippingFee;
            result.EffectiveShippingFee = calculatedShippingFee;

            // 8. Authoritative Grand Total (strictly non-negative)
            result.GrandTotal = Math.Max(0m, (result.SubTotal - result.CouponDiscountAmount) + result.EstimatedTax + result.EffectiveShippingFee);

            // 9. Format all currency strings consistently
            result.FormattedSubTotal = _shippingTaxService.FormatCurrency(result.SubTotal, result.CurrencyCode, result.CurrencySymbol);
            result.FormattedDiscount = _shippingTaxService.FormatCurrency(result.CouponDiscountAmount, result.CurrencyCode, result.CurrencySymbol);
            result.FormattedTax = _shippingTaxService.FormatCurrency(result.EstimatedTax, result.CurrencyCode, result.CurrencySymbol);
            result.FormattedShipping = result.EffectiveShippingFee == 0m ? "FREE" : _shippingTaxService.FormatCurrency(result.EffectiveShippingFee, result.CurrencyCode, result.CurrencySymbol);
            result.FormattedGrandTotal = _shippingTaxService.FormatCurrency(result.GrandTotal, result.CurrencyCode, result.CurrencySymbol);
            result.FormattedTotalSavings = _shippingTaxService.FormatCurrency(result.TotalSavings, result.CurrencyCode, result.CurrencySymbol);

            return result;
        }

        public async Task<CouponValidationResult> ValidateCouponAsync(
            string couponCode, 
            decimal subtotal, 
            string? userId = null, 
            string? customerEmail = null, 
            List<CartItemData>? items = null)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return new CouponValidationResult { IsValid = false, Message = "Please enter a valid coupon code." };
            }

            var code = couponCode.Trim().ToUpperInvariant();
            var coupon = await _context.Coupons
                .Include(c => c.ApplicableCategory)
                .Include(c => c.ApplicableProduct)
                .FirstOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
            {
                return new CouponValidationResult { IsValid = false, Message = $"Coupon '{code}' does not exist." };
            }

            // Rule 1: Active Status
            if (!coupon.IsActive)
            {
                return new CouponValidationResult { IsValid = false, Message = $"Coupon '{coupon.Code}' is no longer active." };
            }

            // Rule 2: Start and Expiry Dates
            var now = DateTime.UtcNow;
            if (now < coupon.StartDate)
            {
                return new CouponValidationResult { IsValid = false, Message = $"Coupon '{coupon.Code}' is not active yet (starts on {coupon.StartDate:MMM dd, yyyy})." };
            }

            if (now > coupon.ExpiryDate)
            {
                return new CouponValidationResult { IsValid = false, Message = $"Coupon '{coupon.Code}' expired on {coupon.ExpiryDate:MMM dd, yyyy}." };
            }

            // Rule 3: Total Global Usage Limit
            if (coupon.UsageLimit > 0 && coupon.UsageCount >= coupon.UsageLimit)
            {
                return new CouponValidationResult { IsValid = false, Message = $"Coupon '{coupon.Code}' has reached its maximum global redemption limit." };
            }

            // Rule 4: Per-Customer Redemption Limit (Checked across non-cancelled and non-refunded orders)
            // Require explicit verified eligibility for limited coupons; entered email alone is insufficient.
            if (coupon.PerUserLimit > 0)
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Coupon '{coupon.Code}' has customer usage limits and requires an account with a verified email address. Entered email alone is not eligible. Please sign in to apply this coupon."
                    };
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null || !user.EmailConfirmed)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Coupon '{coupon.Code}' requires a verified email address. Please verify your email before applying this coupon."
                    };
                }

                var verifiedEmail = user.Email?.Trim().ToLowerInvariant();

                var redemptionsFromRecords = await _context.CouponRedemptions
                    .Where(r => r.CouponCode == code && !r.IsRestored)
                    .Where(r => r.UserId == userId || (verifiedEmail != null && r.CustomerEmail.ToLower() == verifiedEmail))
                    .Select(r => r.OrderId)
                    .Distinct()
                    .ToListAsync();

                var redemptionsFromOrders = await _context.Orders
                    .Where(o => o.CouponCode == code && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
                    .Where(o => o.UserId == userId || (verifiedEmail != null && o.CustomerEmail.ToLower() == verifiedEmail))
                    .Select(o => o.Id)
                    .Distinct()
                    .ToListAsync();

                int totalPriorOrders = redemptionsFromRecords.Union(redemptionsFromOrders).Distinct().Count();

                if (totalPriorOrders >= coupon.PerUserLimit)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"You have already redeemed coupon '{coupon.Code}' the maximum allowed times ({coupon.PerUserLimit} per customer)."
                    };
                }
            }

            // Rule 5: Restricted Category or Product Item-Level Calculation
            // Rule: Restricted coupons discount only the eligible items, not the entire basket.
            bool hasCategoryRestriction = coupon.ApplicableCategoryId.HasValue;
            bool hasProductRestriction = coupon.ApplicableProductId.HasValue;
            bool isRestricted = hasCategoryRestriction || hasProductRestriction;

            decimal eligibleSubtotal = subtotal;

            if (isRestricted)
            {
                if (items == null || !items.Any())
                {
                    string targetName = coupon.ApplicableCategory?.Name 
                        ?? (hasCategoryRestriction ? "the specified category" : (coupon.ApplicableProduct?.Title ?? "the specified product"));
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Coupon '{coupon.Code}' is restricted to '{targetName}', but no matching items were found in your cart."
                    };
                }

                var productIds = items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Include(p => p.Variants)
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                decimal restrictedSubtotal = 0m;
                int eligibleCount = 0;

                foreach (var item in items)
                {
                    if (products.TryGetValue(item.ProductId, out var prod) && prod.Status == ProductStatus.Published)
                    {
                        bool categoryMatch = !hasCategoryRestriction || prod.CategoryId == coupon.ApplicableCategoryId!.Value;
                        bool productMatch = !hasProductRestriction || prod.Id == coupon.ApplicableProductId!.Value;

                        if (categoryMatch && productMatch)
                        {
                            decimal unitPrice = prod.Price;
                            if (item.VariantId.HasValue && item.VariantId.Value > 0)
                            {
                                var variant = prod.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value && v.IsActive);
                                if (variant != null)
                                {
                                    unitPrice = Math.Max(0m, prod.Price + variant.PriceAdjustment);
                                }
                            }

                            int qty = Math.Max(1, item.Quantity);
                            restrictedSubtotal += unitPrice * qty;
                            eligibleCount += qty;
                        }
                    }
                }

                if (eligibleCount == 0 || restrictedSubtotal <= 0m)
                {
                    string targetName = coupon.ApplicableCategory?.Name 
                        ?? (hasCategoryRestriction ? "the specified category" : (coupon.ApplicableProduct?.Title ?? "the specified product"));
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Coupon '{coupon.Code}' is only valid for '{targetName}', but no matching items were found in your cart."
                    };
                }

                eligibleSubtotal = restrictedSubtotal;
            }

            // Rule 6: Minimum Spend Requirement
            // Minimum spend on restricted coupons applies to eligible items subtotal; on general coupons to overall subtotal.
            if (coupon.MinimumSpend > 0 && eligibleSubtotal < coupon.MinimumSpend)
            {
                if (isRestricted)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Coupon '{coupon.Code}' requires a minimum spend of {_shippingTaxService.FormatCurrency(coupon.MinimumSpend)} on eligible items (current eligible total: {_shippingTaxService.FormatCurrency(eligibleSubtotal)})."
                    };
                }
                else
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Coupon '{coupon.Code}' requires a minimum order subtotal of {_shippingTaxService.FormatCurrency(coupon.MinimumSpend)} (current subtotal: {_shippingTaxService.FormatCurrency(subtotal)})."
                    };
                }
            }

            // Rule 7: Calculate Authoritative Discount (strictly non-negative and clamped to eligible subtotal)
            decimal discount = 0m;
            if (coupon.DiscountPercentage > 0)
            {
                decimal calculatedDiscount = eligibleSubtotal * (decimal)(coupon.DiscountPercentage / 100.0);
                if (coupon.MaxDiscountAmount.HasValue && coupon.MaxDiscountAmount.Value > 0)
                {
                    calculatedDiscount = Math.Min(calculatedDiscount, coupon.MaxDiscountAmount.Value);
                }
                discount = calculatedDiscount;
            }
            else if (coupon.FixedDiscountAmount > 0)
            {
                discount = Math.Min(eligibleSubtotal, coupon.FixedDiscountAmount);
            }

            discount = Math.Min(eligibleSubtotal, Math.Max(0m, discount));

            return new CouponValidationResult
            {
                IsValid = true,
                Coupon = coupon,
                CalculatedDiscount = discount,
                Message = coupon.FreeShipping && discount == 0m
                    ? $"Coupon '{coupon.Code}' applied: Free shipping granted!"
                    : $"Coupon '{coupon.Code}' applied successfully!"
            };
        }

        public Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal subtotal, string? userId, List<CartItemData>? items)
        {
            return ValidateCouponAsync(couponCode, subtotal, userId, null, items);
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

        public async Task<bool> RecordCouponRedemptionAsync(string couponCode, Order order)
        {
            if (string.IsNullOrWhiteSpace(couponCode) || order == null) return false;

            var code = couponCode.Trim().ToUpperInvariant();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon != null)
            {
                coupon.UsageCount++;

                var redemption = new CouponRedemption
                {
                    CouponId = coupon.Id,
                    CouponCode = coupon.Code,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    CustomerEmail = order.CustomerEmail,
                    DiscountAmount = order.DiscountAmount,
                    RedeemedAt = DateTime.UtcNow,
                    IsRestored = false
                };
                _context.CouponRedemptions.Add(redemption);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Recorded dedicated redemption record for coupon {Code} on order #{OrderNumber}. Total usages: {Count}/{Limit}", coupon.Code, order.OrderNumber, coupon.UsageCount, coupon.UsageLimit);
                return true;
            }
            return false;
        }

        public async Task<bool> RestoreCouponRedemptionAsync(Order order)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.CouponCode))
            {
                return false;
            }

            var normalizedCode = order.CouponCode.Trim().ToUpperInvariant();

            // 1. Relational database atomic restoration (ensures exact-once execution under concurrent calls)
            if (_context.Database.IsRelational())
            {
                int affectedRedemptions = await _context.CouponRedemptions
                    .Where(r => r.OrderId == order.Id && r.CouponCode == normalizedCode && !r.IsRestored)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.IsRestored, true)
                        .SetProperty(r => r.RestoredAt, DateTime.UtcNow)
                        .SetProperty(r => r.RestoreReason, $"Restored on order #{order.OrderNumber} cancellation/refund"));

                if (affectedRedemptions > 0)
                {
                    await _context.Coupons
                        .Where(c => c.Code == normalizedCode && c.UsageCount > 0)
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsageCount, c => c.UsageCount - 1));

                    _logger.LogInformation("Atomically restored coupon {Code} on order #{OrderNumber}", normalizedCode, order.OrderNumber);
                    return true;
                }

                // If no row was updated, check if it was already restored
                var alreadyRestored = await _context.CouponRedemptions
                    .AnyAsync(r => r.OrderId == order.Id && r.CouponCode == normalizedCode && r.IsRestored);
                if (alreadyRestored)
                {
                    return false;
                }
            }
            else
            {
                // In-memory provider fallback for unit tests
                var redemption = await _context.CouponRedemptions
                    .FirstOrDefaultAsync(r => r.OrderId == order.Id && r.CouponCode == normalizedCode);

                if (redemption != null)
                {
                    if (redemption.IsRestored)
                    {
                        return false;
                    }

                    redemption.IsRestored = true;
                    redemption.RestoredAt = DateTime.UtcNow;
                    redemption.RestoreReason = $"Restored on order #{order.OrderNumber} cancellation/refund";

                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode);
                    if (coupon != null && coupon.UsageCount > 0)
                    {
                        coupon.UsageCount--;
                    }
                    await _context.SaveChangesAsync();
                    return true;
                }
            }

            // Fallback for pre-migration historical orders:
            var marker = $"[CouponRestored:{normalizedCode}]";
            if (order.CustomerNotes != null && (order.CustomerNotes.Contains(marker) || order.CustomerNotes.Contains($"[COUPON_RESTORED:{normalizedCode}]")))
            {
                return false;
            }

            var histCoupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode);
            if (histCoupon != null)
            {
                var newRedemption = new CouponRedemption
                {
                    CouponId = histCoupon.Id,
                    CouponCode = histCoupon.Code,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    CustomerEmail = order.CustomerEmail,
                    DiscountAmount = order.DiscountAmount,
                    RedeemedAt = order.OrderDate,
                    IsRestored = true,
                    RestoredAt = DateTime.UtcNow,
                    RestoreReason = $"Historical restoration for order #{order.OrderNumber}"
                };
                _context.CouponRedemptions.Add(newRedemption);
                if (histCoupon.UsageCount > 0)
                {
                    histCoupon.UsageCount--;
                }
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<int> BackfillHistoricalCouponRedemptionsAsync()
        {
            var ordersWithCoupons = await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.CouponCode))
                .ToListAsync();

            var existingRedemptions = await _context.CouponRedemptions
                .Select(r => new { r.OrderId, r.CouponCode })
                .ToListAsync();

            var existingSet = new HashSet<(int OrderId, string CouponCode)>(
                existingRedemptions.Select(r => (r.OrderId, r.CouponCode.ToUpperInvariant())));

            int backfilledCount = 0;
            var coupons = await _context.Coupons.ToDictionaryAsync(c => c.Code.ToUpperInvariant());

            foreach (var order in ordersWithCoupons)
            {
                var code = order.CouponCode!.Trim().ToUpperInvariant();
                if (existingSet.Contains((order.Id, code)))
                {
                    continue;
                }

                if (!coupons.TryGetValue(code, out var coupon))
                {
                    continue;
                }

                bool wasRestored = false;
                string? restoreReason = null;
                DateTime? restoredAt = null;

                if (!string.IsNullOrEmpty(order.CustomerNotes) &&
                    (order.CustomerNotes.Contains("[CouponRestored:") || order.CustomerNotes.Contains("[COUPON_RESTORED:")))
                {
                    wasRestored = true;
                    restoreReason = "Backfilled from historical CustomerNotes marker";
                    restoredAt = order.OrderDate;

                    order.CustomerNotes = System.Text.RegularExpressions.Regex.Replace(
                        order.CustomerNotes, @"\[(CouponRestored|COUPON_RESTORED):[^\]]+\]", "").Trim();
                }
                else if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Refunded)
                {
                    wasRestored = true;
                    restoreReason = "Order previously cancelled/refunded";
                    restoredAt = order.OrderDate;
                }

                var redemption = new CouponRedemption
                {
                    CouponId = coupon.Id,
                    CouponCode = coupon.Code,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    CustomerEmail = order.CustomerEmail,
                    DiscountAmount = order.DiscountAmount,
                    RedeemedAt = order.OrderDate,
                    IsRestored = wasRestored,
                    RestoredAt = restoredAt,
                    RestoreReason = restoreReason
                };

                _context.CouponRedemptions.Add(redemption);
                existingSet.Add((order.Id, code));
                backfilledCount++;
            }

            if (backfilledCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Backfilled {Count} historical coupon redemptions from legacy orders.", backfilledCount);
            }

            return backfilledCount;
        }
    }
}
