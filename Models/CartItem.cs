using System;
using System.Collections.Generic;
using System.Linq;

namespace HamaraCommerce.Models
{
    /// <summary>
    /// Stored cart item identifier - contains NO client-modifiable pricing or entities.
    /// </summary>
    public class CartItemData
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Stored shopping cart data structure (persisted in Session or ApplicationUser.CartJson).
    /// </summary>
    public class CartData
    {
        public List<CartItemData> Items { get; set; } = new();
        public string? AppliedCouponCode { get; set; }
    }

    /// <summary>
    /// Computed, server-authoritative cart item view model.
    /// Values are strictly loaded fresh from SQL Server database on every calculation.
    /// </summary>
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Brand { get; set; } = string.Empty;

        // Authoritative pricing from database
        public decimal UnitPrice { get; set; }
        public decimal Price => UnitPrice;
        public decimal OldPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? WarningMessage { get; set; }

        public decimal LineTotal => IsAvailable ? UnitPrice * Quantity : 0m;
        public decimal TotalPrice => LineTotal;
        public decimal SavingsTotal => (IsAvailable && OldPrice > UnitPrice) ? (OldPrice - UnitPrice) * Quantity : 0m;
    }

    /// <summary>
    /// Result of coupon validation against cart state and business rules.
    /// </summary>
    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public Coupon? Coupon { get; set; }
        public decimal CalculatedDiscount { get; set; }
    }

    /// <summary>
    /// Computed, server-authoritative shopping cart view model.
    /// All financial totals use decimal and are guaranteed non-negative.
    /// </summary>
    public class ShoppingCartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        
        // Coupon Information
        public string? AppliedCouponCode { get; set; }
        public string? CouponDescription { get; set; }
        public bool CouponIsValid { get; set; }
        public string? CouponValidationMessage { get; set; }
        public double CouponDiscountPercentage { get; set; }
        public decimal CouponFixedDiscountAmount { get; set; }
        public bool CouponGrantsFreeShipping { get; set; }

        // Server-Calculated Totals
        public decimal SubTotal { get; set; }
        public decimal ProductDiscountTotal { get; set; }
        public decimal CouponDiscountAmount { get; set; }
        public decimal TotalDiscount => CouponDiscountAmount;
        public decimal EstimatedTax { get; set; }
        public decimal ShippingFee { get; set; } = 15.00m;
        public decimal EffectiveShippingFee { get; set; }
        public decimal GrandTotal { get; set; }

        public int ItemCount => Items.Where(i => i.IsAvailable).Sum(i => i.Quantity);
        public bool HasOutOfStockItems => Items.Any(i => !i.IsAvailable);
        public bool HasWarnings => Items.Any(i => !string.IsNullOrEmpty(i.WarningMessage)) || !string.IsNullOrEmpty(CouponValidationMessage);
        public List<string> SystemWarnings { get; set; } = new();
    }

    // Aliases for compatibility
    public class CartItem : CartItemViewModel { }
    public class ShoppingCart : ShoppingCartViewModel { }
}
