using System.Collections.Generic;
using System.Threading.Tasks;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public interface IPricingService
    {
        Task<ShoppingCartViewModel> CalculateCartAsync(CartData cartData, string? userId = null, string? shippingMethod = null);
        Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal subtotal, string? userId = null, string? customerEmail = null, List<CartItemData>? items = null);
        Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal subtotal, string? userId, List<CartItemData>? items);
        Task<bool> RecordCouponRedemptionAsync(string couponCode);
        Task<bool> RecordCouponRedemptionAsync(string couponCode, Order order);
        Task<bool> RestoreCouponRedemptionAsync(Order order);
        Task<int> BackfillHistoricalCouponRedemptionsAsync();
    }
}
