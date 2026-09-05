using System.Collections.Generic;
using System.Threading.Tasks;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public interface IPricingService
    {
        Task<ShoppingCartViewModel> CalculateCartAsync(CartData cartData, string? userId = null);
        Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal subtotal, string? userId = null, List<CartItemData>? items = null);
        Task<bool> RecordCouponRedemptionAsync(string couponCode);
    }
}
