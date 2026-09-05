using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public interface ICartService
    {
        Task<CartData> GetRawCartDataAsync();
        Task<ShoppingCartViewModel> GetCartViewModelAsync();
        Task<(bool success, string message, string? warning)> AddToCartAsync(int productId, int quantity = 1, int? variantId = null);
        Task<(bool success, string message)> UpdateQuantityAsync(int productId, int quantity, int? variantId = null);
        Task<(bool success, string message)> RemoveFromCartAsync(int productId, int? variantId = null);
        Task<(bool success, string message)> ApplyCouponAsync(string couponCode);
        Task<(bool success, string message)> RemoveCouponAsync();
        Task ClearCartAsync();
        Task MergeGuestCartAsync(string userId);
    }

    public class CartService : ICartService
    {
        private const string CartSessionKey = "HamaraCommerce_CartData_v2";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly ILogger<CartService> _logger;

        public CartService(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            IPricingService pricingService,
            ILogger<CartService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
        private ISession? Session => HttpContext?.Session;

        private string? CurrentUserId
        {
            get
            {
                var user = HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    return user.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                return null;
            }
        }

        public async Task<CartData> GetRawCartDataAsync()
        {
            // 1. Check Session first
            if (Session != null)
            {
                var sessionJson = Session.GetString(CartSessionKey);
                if (!string.IsNullOrEmpty(sessionJson))
                {
                    try
                    {
                        var sessionCart = JsonSerializer.Deserialize<CartData>(sessionJson);
                        if (sessionCart != null)
                        {
                            return sessionCart;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize cart session data.");
                    }
                }
            }

            // 2. If authenticated user has saved cart in database, load it
            var userId = CurrentUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                var dbUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (dbUser != null && !string.IsNullOrEmpty(dbUser.CartJson))
                {
                    try
                    {
                        var userCart = JsonSerializer.Deserialize<CartData>(dbUser.CartJson);
                        if (userCart != null)
                        {
                            // Sync to session
                            SaveCartToSession(userCart);
                            return userCart;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize user database cart.");
                    }
                }
            }

            var emptyCart = new CartData();
            SaveCartToSession(emptyCart);
            return emptyCart;
        }

        private void SaveCartToSession(CartData cart)
        {
            if (Session != null)
            {
                var json = JsonSerializer.Serialize(cart);
                Session.SetString(CartSessionKey, json);
            }
        }

        private async Task PersistCartAsync(CartData cart)
        {
            SaveCartToSession(cart);

            // Persist to user record if logged in
            var userId = CurrentUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (dbUser != null)
                {
                    dbUser.CartJson = JsonSerializer.Serialize(cart);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<ShoppingCartViewModel> GetCartViewModelAsync()
        {
            var rawData = await GetRawCartDataAsync();
            return await _pricingService.CalculateCartAsync(rawData, CurrentUserId);
        }

        public async Task<(bool success, string message, string? warning)> AddToCartAsync(int productId, int quantity = 1, int? variantId = null)
        {
            // Validate basic inputs
            if (quantity < 1)
            {
                return (false, "Quantity must be at least 1.", null);
            }

            // Verify Product & Variant from DB
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null || product.Status != ProductStatus.Published)
            {
                return (false, "Product is currently unavailable.", null);
            }

            ProductVariant? variant = null;
            int availableStock = product.Stock;
            string itemName = product.Title;

            if (variantId.HasValue && variantId.Value > 0)
            {
                variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value && v.IsActive);
                if (variant == null)
                {
                    return (false, "Selected variant is not available.", null);
                }
                availableStock = variant.Stock;
                itemName = $"{product.Title} ({variant.Name})";
            }

            if (availableStock <= 0)
            {
                return (false, $"Sorry, '{itemName}' is currently out of stock.", null);
            }

            var rawCart = await GetRawCartDataAsync();
            int? vId = variant?.Id;
            var existingItem = rawCart.Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == vId);

            string? warning = null;
            if (existingItem != null)
            {
                int newTotalQuantity = existingItem.Quantity + quantity;
                if (newTotalQuantity > availableStock)
                {
                    existingItem.Quantity = availableStock;
                    warning = $"Quantity capped at maximum available stock ({availableStock} units).";
                }
                else if (newTotalQuantity > 50)
                {
                    existingItem.Quantity = 50;
                    warning = "Maximum 50 units allowed per item.";
                }
                else
                {
                    existingItem.Quantity = newTotalQuantity;
                }
            }
            else
            {
                int initialQty = Math.Min(quantity, availableStock);
                if (quantity > availableStock)
                {
                    warning = $"Quantity adjusted to available stock ({availableStock} units).";
                }

                rawCart.Items.Add(new CartItemData
                {
                    ProductId = productId,
                    VariantId = vId,
                    Quantity = initialQty
                });
            }

            await PersistCartAsync(rawCart);
            return (true, $"{itemName} added to your cart.", warning);
        }

        public async Task<(bool success, string message)> UpdateQuantityAsync(int productId, int quantity, int? variantId = null)
        {
            var rawCart = await GetRawCartDataAsync();
            var item = rawCart.Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

            if (item == null)
            {
                return (false, "Item not found in cart.");
            }

            if (quantity <= 0)
            {
                rawCart.Items.Remove(item);
                await PersistCartAsync(rawCart);
                return (true, "Item removed from cart.");
            }

            // Check stock from database
            var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null || product.Status != ProductStatus.Published)
            {
                rawCart.Items.Remove(item);
                await PersistCartAsync(rawCart);
                return (false, "This product is no longer available and was removed from your cart.");
            }

            int stock = product.Stock;
            if (variantId.HasValue && variantId.Value > 0)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value && v.IsActive);
                if (variant != null) stock = variant.Stock;
            }

            if (stock <= 0)
            {
                rawCart.Items.Remove(item);
                await PersistCartAsync(rawCart);
                return (false, "Item is out of stock and was removed from your cart.");
            }

            item.Quantity = Math.Min(Math.Min(quantity, stock), 50);
            await PersistCartAsync(rawCart);
            return (true, "Cart updated successfully.");
        }

        public async Task<(bool success, string message)> RemoveFromCartAsync(int productId, int? variantId = null)
        {
            var rawCart = await GetRawCartDataAsync();
            var item = rawCart.Items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);

            if (item != null)
            {
                rawCart.Items.Remove(item);
                await PersistCartAsync(rawCart);
                return (true, "Item removed from cart.");
            }

            return (false, "Item not found in cart.");
        }

        public async Task<(bool success, string message)> ApplyCouponAsync(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return (false, "Please enter a coupon code.");
            }

            var rawCart = await GetRawCartDataAsync();
            if (!rawCart.Items.Any())
            {
                return (false, "Your cart is empty. Add products before applying a coupon.");
            }

            // Compute current subtotal
            var cartVm = await _pricingService.CalculateCartAsync(rawCart, CurrentUserId);
            var validation = await _pricingService.ValidateCouponAsync(couponCode, cartVm.SubTotal, CurrentUserId, rawCart.Items);

            if (!validation.IsValid || validation.Coupon == null)
            {
                return (false, validation.Message ?? "Invalid coupon code.");
            }

            rawCart.AppliedCouponCode = validation.Coupon.Code;
            await PersistCartAsync(rawCart);
            return (true, $"Coupon '{validation.Coupon.Code}' applied successfully!");
        }

        public async Task<(bool success, string message)> RemoveCouponAsync()
        {
            var rawCart = await GetRawCartDataAsync();
            rawCart.AppliedCouponCode = null;
            await PersistCartAsync(rawCart);
            return (true, "Coupon removed from cart.");
        }

        public async Task ClearCartAsync()
        {
            if (Session != null)
            {
                Session.Remove(CartSessionKey);
            }

            var userId = CurrentUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (dbUser != null)
                {
                    dbUser.CartJson = null;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task MergeGuestCartAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || Session == null) return;

            var sessionJson = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(sessionJson)) return;

            CartData? guestCart = null;
            try
            {
                guestCart = JsonSerializer.Deserialize<CartData>(sessionJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize guest cart during login merge.");
                return;
            }

            if (guestCart == null || !guestCart.Items.Any()) return;

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (dbUser == null) return;

            CartData userCart = new CartData();
            if (!string.IsNullOrEmpty(dbUser.CartJson))
            {
                try
                {
                    userCart = JsonSerializer.Deserialize<CartData>(dbUser.CartJson) ?? new CartData();
                }
                catch
                {
                    userCart = new CartData();
                }
            }

            // Merge items: add or sum quantities
            foreach (var gItem in guestCart.Items)
            {
                var existing = userCart.Items.FirstOrDefault(i => i.ProductId == gItem.ProductId && i.VariantId == gItem.VariantId);
                if (existing != null)
                {
                    existing.Quantity = Math.Min(existing.Quantity + gItem.Quantity, 50);
                }
                else
                {
                    userCart.Items.Add(gItem);
                }
            }

            if (!string.IsNullOrEmpty(guestCart.AppliedCouponCode) && string.IsNullOrEmpty(userCart.AppliedCouponCode))
            {
                userCart.AppliedCouponCode = guestCart.AppliedCouponCode;
            }

            dbUser.CartJson = JsonSerializer.Serialize(userCart);
            await _context.SaveChangesAsync();

            // Sync merged cart to current session
            SaveCartToSession(userCart);
            _logger.LogInformation("Merged guest cart ({Count} items) into user {UserId} account.", guestCart.Items.Count, userId);
        }
    }
}
