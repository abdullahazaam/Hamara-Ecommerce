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
        public async Task<IActionResult> Index(string? trackingNumber, string? email)
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
                        var userOrder = await _context.Orders
                            .Include(o => o.Items)
                            .Include(o => o.Payments)
                            .Where(o => o.UserId == user.Id || o.CustomerEmail == user.Email)
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
            bool isUserOwner = false;

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.TrackingNumber.ToLower() == num || o.OrderNumber.ToLower() == num);

            if (order != null)
            {
                // Check authorization:
                // 1. Authenticated customer who owns the order
                if (currentUser != null && (order.UserId == currentUser.Id || order.CustomerEmail.ToLower() == (currentUser.Email ?? "").ToLower() || User.IsInRole("Admin")))
                {
                    isUserOwner = true;
                }
                // 2. Guest tracking with exact matching email
                else if (!string.IsNullOrEmpty(mail) && order.CustomerEmail.ToLower() == mail)
                {
                    isUserOwner = true;
                }
            }

            if (!isUserOwner || order == null)
            {
                // Generic secure message: does not reveal whether the tracking number or email exists
                ViewBag.NotFoundMessage = "No matching order found for the provided details. Please verify your tracking number and matching email address.";
                return View((Order?)null);
            }

            return View(order);
        }
    }
}
