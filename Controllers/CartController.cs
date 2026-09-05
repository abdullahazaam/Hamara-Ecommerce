using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IPricingService _pricingService;
        private readonly IShippingTaxService _shippingTaxService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            IPricingService pricingService,
            IShippingTaxService shippingTaxService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _pricingService = pricingService;
            _shippingTaxService = shippingTaxService;
            _logger = logger;
        }

        // ==========================================
        // 1. CART PAGE VIEW
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartVm = await _cartService.GetCartViewModelAsync();
            return View(cartVm);
        }

        // ==========================================
        // 2. ADD TO CART (MVC & AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, int? variantId = null)
        {
            var (success, message, warning) = await _cartService.AddToCartAsync(productId, quantity, variantId);

            if (IsAjaxRequest())
            {
                var cart = await _cartService.GetCartViewModelAsync();
                return Json(new
                {
                    success,
                    message,
                    warning,
                    itemCount = cart.ItemCount,
                    subtotal = _shippingTaxService.FormatCurrency(cart.SubTotal),
                    productDiscount = _shippingTaxService.FormatCurrency(cart.ProductDiscountTotal),
                    couponDiscount = _shippingTaxService.FormatCurrency(cart.CouponDiscountAmount),
                    tax = _shippingTaxService.FormatCurrency(cart.EstimatedTax),
                    shipping = cart.EffectiveShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(cart.EffectiveShippingFee),
                    grandTotal = _shippingTaxService.FormatCurrency(cart.GrandTotal),
                    couponCode = cart.AppliedCouponCode,
                    couponValid = cart.CouponIsValid,
                    hasOutOfStock = cart.HasOutOfStockItems,
                    items = cart.Items.Select(i => new
                    {
                        productId = i.ProductId,
                        variantId = i.VariantId,
                        variantName = i.VariantName,
                        sku = i.SKU,
                        title = i.Title,
                        imageUrl = i.ImageUrl,
                        unitPrice = _shippingTaxService.FormatCurrency(i.UnitPrice),
                        oldPrice = _shippingTaxService.FormatCurrency(i.OldPrice),
                        quantity = i.Quantity,
                        availableStock = i.AvailableStock,
                        isAvailable = i.IsAvailable,
                        lineTotal = _shippingTaxService.FormatCurrency(i.LineTotal),
                        warning = i.WarningMessage
                    })
                });
            }

            if (success)
            {
                if (!string.IsNullOrEmpty(warning))
                {
                    TempData["WarningMessage"] = warning;
                }
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. UPDATE QUANTITY (MVC & AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity, int? variantId = null)
        {
            var (success, message) = await _cartService.UpdateQuantityAsync(productId, quantity, variantId);
            var cart = await _cartService.GetCartViewModelAsync();

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success,
                    message,
                    itemCount = cart.ItemCount,
                    subtotal = _shippingTaxService.FormatCurrency(cart.SubTotal),
                    productDiscount = _shippingTaxService.FormatCurrency(cart.ProductDiscountTotal),
                    couponDiscount = _shippingTaxService.FormatCurrency(cart.CouponDiscountAmount),
                    tax = _shippingTaxService.FormatCurrency(cart.EstimatedTax),
                    shipping = cart.EffectiveShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(cart.EffectiveShippingFee),
                    grandTotal = _shippingTaxService.FormatCurrency(cart.GrandTotal),
                    couponCode = cart.AppliedCouponCode,
                    couponValid = cart.CouponIsValid,
                    hasOutOfStock = cart.HasOutOfStockItems,
                    items = cart.Items.Select(i => new
                    {
                        productId = i.ProductId,
                        variantId = i.VariantId,
                        variantName = i.VariantName,
                        sku = i.SKU,
                        title = i.Title,
                        imageUrl = i.ImageUrl,
                        unitPrice = _shippingTaxService.FormatCurrency(i.UnitPrice),
                        quantity = i.Quantity,
                        availableStock = i.AvailableStock,
                        isAvailable = i.IsAvailable,
                        lineTotal = _shippingTaxService.FormatCurrency(i.LineTotal),
                        warning = i.WarningMessage
                    })
                });
            }

            if (!success)
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. REMOVE ITEM FROM CART (MVC & AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId, int? variantId = null)
        {
            var (success, message) = await _cartService.RemoveFromCartAsync(productId, variantId);
            var cart = await _cartService.GetCartViewModelAsync();

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success,
                    message,
                    itemCount = cart.ItemCount,
                    subtotal = _shippingTaxService.FormatCurrency(cart.SubTotal),
                    productDiscount = _shippingTaxService.FormatCurrency(cart.ProductDiscountTotal),
                    couponDiscount = _shippingTaxService.FormatCurrency(cart.CouponDiscountAmount),
                    tax = _shippingTaxService.FormatCurrency(cart.EstimatedTax),
                    shipping = cart.EffectiveShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(cart.EffectiveShippingFee),
                    grandTotal = _shippingTaxService.FormatCurrency(cart.GrandTotal),
                    couponCode = cart.AppliedCouponCode,
                    couponValid = cart.CouponIsValid,
                    hasOutOfStock = cart.HasOutOfStockItems,
                    items = cart.Items.Select(i => new
                    {
                        productId = i.ProductId,
                        variantId = i.VariantId,
                        variantName = i.VariantName,
                        title = i.Title,
                        imageUrl = i.ImageUrl,
                        unitPrice = _shippingTaxService.FormatCurrency(i.UnitPrice),
                        quantity = i.Quantity,
                        lineTotal = _shippingTaxService.FormatCurrency(i.LineTotal)
                    })
                });
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. APPLY COUPON (MVC & AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            var (success, message) = await _cartService.ApplyCouponAsync(couponCode);
            var cart = await _cartService.GetCartViewModelAsync();

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success,
                    message,
                    itemCount = cart.ItemCount,
                    subtotal = _shippingTaxService.FormatCurrency(cart.SubTotal),
                    productDiscount = _shippingTaxService.FormatCurrency(cart.ProductDiscountTotal),
                    couponDiscount = _shippingTaxService.FormatCurrency(cart.CouponDiscountAmount),
                    tax = _shippingTaxService.FormatCurrency(cart.EstimatedTax),
                    shipping = cart.EffectiveShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(cart.EffectiveShippingFee),
                    grandTotal = _shippingTaxService.FormatCurrency(cart.GrandTotal),
                    couponCode = cart.AppliedCouponCode,
                    couponDescription = cart.CouponDescription,
                    couponValid = cart.CouponIsValid
                });
            }

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. REMOVE COUPON (MVC & AJAX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoupon()
        {
            var (success, message) = await _cartService.RemoveCouponAsync();
            var cart = await _cartService.GetCartViewModelAsync();

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success,
                    message,
                    itemCount = cart.ItemCount,
                    subtotal = _shippingTaxService.FormatCurrency(cart.SubTotal),
                    productDiscount = _shippingTaxService.FormatCurrency(cart.ProductDiscountTotal),
                    couponDiscount = _shippingTaxService.FormatCurrency(cart.CouponDiscountAmount),
                    tax = _shippingTaxService.FormatCurrency(cart.EstimatedTax),
                    shipping = cart.EffectiveShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(cart.EffectiveShippingFee),
                    grandTotal = _shippingTaxService.FormatCurrency(cart.GrandTotal),
                    couponCode = cart.AppliedCouponCode,
                    couponValid = false
                });
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 7. GET DRAWER DATA (AJAX GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetCartDrawerData()
        {
            var cart = await _cartService.GetCartViewModelAsync();
            return Json(new
            {
                itemCount = cart.ItemCount,
                subtotal = _shippingTaxService.FormatCurrency(cart.SubTotal),
                productDiscount = _shippingTaxService.FormatCurrency(cart.ProductDiscountTotal),
                couponDiscount = _shippingTaxService.FormatCurrency(cart.CouponDiscountAmount),
                tax = _shippingTaxService.FormatCurrency(cart.EstimatedTax),
                shipping = cart.EffectiveShippingFee == 0 ? "FREE" : _shippingTaxService.FormatCurrency(cart.EffectiveShippingFee),
                grandTotal = _shippingTaxService.FormatCurrency(cart.GrandTotal),
                couponCode = cart.AppliedCouponCode,
                couponValid = cart.CouponIsValid,
                hasOutOfStock = cart.HasOutOfStockItems,
                items = cart.Items.Select(i => new
                {
                    productId = i.ProductId,
                    variantId = i.VariantId,
                    variantName = i.VariantName,
                    sku = i.SKU,
                    title = i.Title,
                    imageUrl = i.ImageUrl,
                    unitPrice = _shippingTaxService.FormatCurrency(i.UnitPrice),
                    oldPrice = _shippingTaxService.FormatCurrency(i.OldPrice),
                    quantity = i.Quantity,
                    availableStock = i.AvailableStock,
                    isAvailable = i.IsAvailable,
                    lineTotal = _shippingTaxService.FormatCurrency(i.LineTotal),
                    warning = i.WarningMessage
                })
            });
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   Request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}
