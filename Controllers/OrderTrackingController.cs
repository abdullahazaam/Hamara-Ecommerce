using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Controllers
{
    public class OrderTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public OrderTrackingController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? trackingNumber, string? email, string? guestToken = null)
        {
            // Initial GET with no query parameters
            if (string.IsNullOrWhiteSpace(trackingNumber) && string.IsNullOrWhiteSpace(email))
            {
                // If logged-in customer, show their most recent active order
                if (User.Identity?.IsAuthenticated == true)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        // Claim guest orders if email verified
                        if (user.EmailConfirmed && !string.IsNullOrWhiteSpace(user.Email))
                        {
                            var normalizedEmail = user.Email.Trim().ToLower();
                            var guestOrders = await _context.Orders
                                .Where(o => o.UserId == null && o.CustomerEmail.ToLower() == normalizedEmail)
                                .ToListAsync();

                            if (guestOrders.Any())
                            {
                                foreach (var go in guestOrders)
                                {
                                    go.UserId = user.Id;
                                    go.GuestAccessToken = null;
                                    go.GuestAccessExpiry = null;
                                }
                                await _context.SaveChangesAsync();
                            }
                        }

                        var userOrder = await _context.Orders
                            .Include(o => o.Items)
                            .Include(o => o.Payments)
                            .Where(o => o.UserId == user.Id)
                            .OrderByDescending(o => o.OrderDate)
                            .FirstOrDefaultAsync();

                        if (userOrder != null)
                        {
                            return View(userOrder);
                        }
                    }
                }

                // Return clean tracking form with no fake default sample
                return View((Order?)null);
            }

            // Rate-limiting check by client IP / Session to prevent tracking enumeration
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"TrackingAttempts_{ip}";
            if (_cache.TryGetValue<int>(cacheKey, out var attempts) && attempts >= 15)
            {
                ViewBag.NotFoundMessage = "Too many tracking lookups attempted. Please wait 5 minutes before trying again.";
                return View((Order?)null);
            }

            _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(5));

            var num = (trackingNumber ?? string.Empty).Trim().ToLowerInvariant();
            var mail = (email ?? string.Empty).Trim().ToLowerInvariant();

            // Authentication context
            var currentUser = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            bool isAuthorized = false;

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.TrackingNumber.ToLower() == num || o.OrderNumber.ToLower() == num);

            if (order != null)
            {
                // Check 1: Registered user order (strictly owned by UserId or Admin)
                if (order.UserId != null)
                {
                    if (User.IsInRole("Admin") || (currentUser != null && order.UserId == currentUser.Id))
                    {
                        isAuthorized = true;
                    }
                }
                // Check 2: Guest order
                else
                {
                    // If logged in with verified email matching the order, claim order and allow access
                    if (currentUser != null && currentUser.EmailConfirmed && string.Equals(order.CustomerEmail, currentUser.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        order.UserId = currentUser.Id;
                        order.GuestAccessToken = null;
                        order.GuestAccessExpiry = null;
                        await _context.SaveChangesAsync();
                        isAuthorized = true;
                    }
                    else
                    {
                        // Anonymous access: MUST match billing email AND valid unexpired GuestAccessToken
                        var sessionToken = HttpContext.Session.GetString($"GuestOrderToken_{order.OrderNumber}");
                        var providedToken = !string.IsNullOrEmpty(guestToken) ? guestToken.Trim() : sessionToken;

                        if (!string.IsNullOrEmpty(mail) && string.Equals(order.CustomerEmail, mail, StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrEmpty(providedToken) && !string.IsNullOrEmpty(order.GuestAccessToken) &&
                            string.Equals(order.GuestAccessToken, providedToken, StringComparison.Ordinal) &&
                            (order.GuestAccessExpiry == null || order.GuestAccessExpiry.Value >= DateTime.UtcNow))
                        {
                            isAuthorized = true;
                        }
                    }
                }
            }

            if (!isAuthorized || order == null)
            {
                // Generic secure message: does not reveal whether the tracking number or email exists
                ViewBag.NotFoundMessage = "No matching order found for the provided details. For guest orders, please use the tracking link sent to your email or sign in with your verified account.";
                return View((Order?)null);
            }

            return View(order);
        }
    }
}
