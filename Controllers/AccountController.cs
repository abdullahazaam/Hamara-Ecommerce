using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Controllers
{
    [EnableRateLimiting("AuthPolicy")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IPricingService _pricingService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ICartService cartService,
            IPricingService pricingService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _cartService = cartService;
            _pricingService = pricingService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        private async Task ClaimGuestOrdersAsync(ApplicationUser user)
        {
            if (user == null || !user.EmailConfirmed || string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var normalizedEmail = user.Email.Trim().ToLower();
            var guestOrders = await _context.Orders
                .Where(o => o.UserId == null && o.CustomerEmail.ToLower() == normalizedEmail)
                .ToListAsync();

            if (guestOrders.Any())
            {
                foreach (var order in guestOrders)
                {
                    order.UserId = user.Id;
                    order.GuestAccessToken = null;
                    order.GuestAccessExpiry = null;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Claimed {Count} guest orders for verified user {UserId} ({Email})", guestOrders.Count, user.Id, user.Email);
            }
        }

        // ==========================================
        // 1. CUSTOMER DASHBOARD (AUTHENTICATED)
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.EmailConfirmed)
            {
                await ClaimGuestOrdersAsync(user);
            }

            var userOrders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                SavedAddresses = user.SavedAddresses ?? new List<Address>(),
                RecentOrders = userOrders.Take(5).ToList(),
                TotalOrders = userOrders.Count,
                TotalSpent = userOrders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
                RewardPoints = (int)(userOrders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount) * 0.1m),
                WishlistCount = user.WishlistProductIds?.Distinct().Count() ?? 0
            };

            return View(model);
        }

        // ==========================================
        // 2. PERSONAL ORDER HISTORY (PAGINATED)
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.EmailConfirmed)
            {
                await ClaimGuestOrdersAsync(user);
            }

            int pageSize = 6;
            var query = _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate);

            int totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new CustomerOrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(model);
        }

        // ==========================================
        // 3. ORDER DETAIL (SECURED PRIVACY)
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderDetail(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.EmailConfirmed)
            {
                await ClaimGuestOrdersAsync(user);
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == id.Trim() || o.TrackingNumber == id.Trim());

            if (order == null) return NotFound();

            // Strict privacy check: Must belong to current customer by UserId unless Admin
            if (!User.IsInRole("Admin") && order.UserId != user.Id)
            {
                _logger.LogWarning("Unauthorized attempt by User {UserId} to access Order {OrderNumber}", user.Id, order.OrderNumber);
                return Forbid();
            }

            return View(order);
        }

        // ==========================================
        // 4. CANCEL ORDER (ALLOWED STATES ONLY)
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(CancelOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.EmailConfirmed)
            {
                await ClaimGuestOrdersAsync(user);
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == model.OrderNumber.Trim());

            if (order == null) return NotFound();

            if (!User.IsInRole("Admin") && order.UserId != user.Id)
            {
                return Forbid();
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            {
                TempData["ErrorMessage"] = $"Order #{order.OrderNumber} cannot be cancelled as it is already {order.Status}.";
                return RedirectToAction(nameof(OrderDetail), new { id = order.OrderNumber });
            }

            // Restore product and variant stock
            foreach (var item in order.Items)
            {
                var prod = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (prod != null)
                {
                    int prevStock = prod.Stock;
                    prod.Stock += item.Quantity;

                    if (item.VariantId.HasValue && item.VariantId.Value > 0)
                    {
                        var variant = prod.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value);
                        if (variant != null)
                        {
                            variant.Stock += item.Quantity;
                        }
                    }

                    _context.InventoryMovements.Add(new InventoryMovement
                    {
                        ProductId = prod.Id,
                        VariantId = item.VariantId,
                        MovementType = InventoryMovementType.OrderCancellationRestoration,
                        QuantityChange = item.Quantity,
                        OldStock = prevStock,
                        NewStock = prod.Stock,
                        Reason = $"Order #{order.OrderNumber} cancelled by customer. Reason: {model.Reason}",
                        OrderId = order.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _pricingService.RestoreCouponRedemptionAsync(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} has been successfully cancelled and inventory has been restored.";
            return RedirectToAction(nameof(OrderDetail), new { id = order.OrderNumber });
        }

        // ==========================================
        // 5. RETURN / REFUND REQUEST
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnOrder(ReturnOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.EmailConfirmed)
            {
                await ClaimGuestOrdersAsync(user);
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == model.OrderNumber.Trim());
            if (order == null) return NotFound();

            if (!User.IsInRole("Admin") && order.UserId != user.Id)
            {
                return Forbid();
            }

            if (order.Status != OrderStatus.Delivered)
            {
                TempData["ErrorMessage"] = "Return requests can only be initiated for delivered orders.";
                return RedirectToAction(nameof(OrderDetail), new { id = order.OrderNumber });
            }

            if ((DateTime.UtcNow - order.OrderDate).TotalDays > 30)
            {
                TempData["ErrorMessage"] = "The 30-day return window for this order has expired.";
                return RedirectToAction(nameof(OrderDetail), new { id = order.OrderNumber });
            }

            order.Status = OrderStatus.Refunded;
            order.CustomerNotes = (order.CustomerNotes ?? "") + $" | Return requested by customer on {DateTime.UtcNow:yyyy-MM-dd}: {model.Reason}";
            await _pricingService.RestoreCouponRedemptionAsync(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Return request submitted for Order #{order.OrderNumber}. Our support team will contact you.";
            return RedirectToAction(nameof(OrderDetail), new { id = order.OrderNumber });
        }

        // ==========================================
        // 6. CUSTOMER PRINTABLE INVOICE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Invoice(string id, [FromQuery] string? guestToken = null)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == id.Trim() || o.TrackingNumber == id.Trim());

            if (order == null) return NotFound();

            // Check 1: Authenticated User (Admin or Owner)
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.EmailConfirmed)
                {
                    await ClaimGuestOrdersAsync(user);
                }

                if (User.IsInRole("Admin") || (user != null && order.UserId == user.Id))
                {
                    return View(order);
                }
            }

            // Check 2: Anonymous / Guest Access via GuestAccessToken
            if (order.UserId == null)
            {
                var sessionToken = HttpContext.Session.GetString($"GuestOrderToken_{order.OrderNumber}");
                var providedToken = !string.IsNullOrEmpty(guestToken) ? guestToken.Trim() : sessionToken;

                if (!string.IsNullOrEmpty(providedToken) &&
                    !string.IsNullOrEmpty(order.GuestAccessToken) &&
                    string.Equals(order.GuestAccessToken, providedToken, StringComparison.Ordinal) &&
                    (order.GuestAccessExpiry == null || order.GuestAccessExpiry.Value >= DateTime.UtcNow))
                {
                    return View(order);
                }
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                return Forbid();
            }

            return Challenge();
        }

        // ==========================================
        // 7. SAVED ADDRESSES
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            return View(user.SavedAddresses ?? new List<Address>());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(AddressViewModel model)
        {
            if (!ModelState.IsValid) return View("Addresses", (await _userManager.GetUserAsync(User))?.SavedAddresses ?? new List<Address>());

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var addresses = user.SavedAddresses ?? new List<Address>();
            int newId = addresses.Any() ? addresses.Max(a => a.Id) + 1 : 1;

            if (model.IsDefault || !addresses.Any())
            {
                addresses.ForEach(a => a.IsDefault = false);
            }

            addresses.Add(new Address
            {
                Id = newId,
                Title = model.Title.Trim(),
                FullName = model.FullName.Trim(),
                StreetAddress = model.StreetAddress.Trim(),
                City = model.City.Trim(),
                State = model.State.Trim(),
                ZipCode = model.PostalCode.Trim(),
                Country = model.Country.Trim(),
                IsDefault = model.IsDefault || !addresses.Any()
            });

            user.SavedAddresses = addresses;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "New delivery address added successfully.";
            return RedirectToAction(nameof(Addresses));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var addresses = user.SavedAddresses ?? new List<Address>();
            var target = addresses.FirstOrDefault(a => a.Id == id);
            if (target != null)
            {
                addresses.Remove(target);
                if (target.IsDefault && addresses.Any())
                {
                    addresses.First().IsDefault = true;
                }
                user.SavedAddresses = addresses;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = "Address deleted successfully.";
            }

            return RedirectToAction(nameof(Addresses));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var addresses = user.SavedAddresses ?? new List<Address>();
            foreach (var addr in addresses)
            {
                addr.IsDefault = (addr.Id == id);
            }

            user.SavedAddresses = addresses;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Default delivery address updated.";
            return RedirectToAction(nameof(Addresses));
        }

        // ==========================================
        // 8. CUSTOMER WISHLIST (PAGINATED & DEDUPLICATED)
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Wishlist(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            int pageSize = 8;
            var productIds = (user.WishlistProductIds ?? new List<int>()).Distinct().ToList();

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.Id) && p.Status == ProductStatus.Published);

            int totalCount = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new CustomerWishlistViewModel
            {
                Products = products,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        success = false,
                        requiresAuth = true,
                        message = "Please sign in to manage your wishlist."
                    });
                }
                TempData["ErrorMessage"] = "Please sign in to manage your wishlist.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Wishlist", "Account") });
            }

            var wishlist = (user.WishlistProductIds ?? new List<int>()).Distinct().ToList();
            bool isAdded;

            if (wishlist.Contains(productId))
            {
                wishlist.Remove(productId);
                isAdded = false;
            }
            else
            {
                wishlist.Add(productId);
                isAdded = true;
            }

            user.WishlistProductIds = wishlist;
            await _userManager.UpdateAsync(user);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Json(new
                {
                    success = true,
                    isAdded,
                    count = wishlist.Count,
                    message = isAdded ? "Added to your wishlist!" : "Removed from your wishlist."
                });
            }

            TempData["SuccessMessage"] = isAdded ? "Product added to your wishlist." : "Product removed from your wishlist.";
            return RedirectToAction(nameof(Wishlist));
        }

        // ==========================================
        // 9. PROFILE & PASSWORD SETTINGS
        // ==========================================
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid) return View(model);

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.AvatarUrl = model.AvatarUrl?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile details updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ==========================================
        // 10. AUTHENTICATION (LOGIN, REGISTER, LOGOUT)
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
                        return View(model);
                    }

                    if (!user.EmailConfirmed)
                    {
                        ViewBag.ShowResendConfirmation = true;
                        ViewBag.UnconfirmedEmail = user.Email;
                        ModelState.AddModelError(string.Empty, "Your email address has not been verified. Please verify your email before signing in.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {Email} logged in successfully.", model.Email);
                        await ClaimGuestOrdersAsync(user);
                        await _cartService.MergeGuestCartAsync(user.Id);

                        if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/" || returnUrl.Equals(Url.Action("Index", "Home"), StringComparison.OrdinalIgnoreCase))
                        {
                            if (await _userManager.IsInRoleAsync(user, "Admin"))
                            {
                                return RedirectToAction("Index", "Admin");
                            }
                        }

                        return RedirectToLocal(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account {Email} locked out.", model.Email);
                        ModelState.AddModelError(string.Empty, "Account locked out due to multiple failed login attempts. Please try again in 5 minutes.");
                        return View(model);
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    _logger.LogInformation("User {Email} created a new customer account.", model.Email);

                    // Send Account Verification Email (do NOT auto sign in unverified user)
                    try
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = token }, protocol: Request.Scheme) 
                                          ?? $"{Request.Scheme}://{Request.Host}/Account/ConfirmEmail?userId={user.Id}&code={Uri.EscapeDataString(token)}";
                        var emailBody = _emailTemplateService.GenerateAccountVerificationEmail(user.FullName, callbackUrl);
                        _ = _emailSender.SendEmailAsync(user.Email!, "Verify Your Email - Hamara Commerce", emailBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to dispatch verification email to {Email}", user.Email);
                    }

                    TempData["SuccessMessage"] = "Account registered successfully! A confirmation link has been sent to your email. Please verify your email before logging in.";
                    return RedirectToAction(nameof(Login));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // ==========================================
        // 11. PASSWORD RECOVERY (FORGOT & RESET)
        // ==========================================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null && user.IsActive)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = token, email = user.Email }, protocol: Request.Scheme) ?? $"{Request.Scheme}://{Request.Host}/Account/ResetPassword";
                
                try
                {
                    var emailBody = _emailTemplateService.GeneratePasswordResetEmail(user.FullName, callbackUrl);
                    _ = _emailSender.SendEmailAsync(user.Email!, "Reset Your Hamara Commerce Password", emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send password reset email to {Email}", user.Email);
                }
            }

            // Always display confirmation to prevent email enumeration
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code = null, string? email = null)
        {
            if (code == null) return BadRequest("A security code must be supplied for password reset.");
            var model = new ResetPasswordViewModel { Code = code, Email = email ?? string.Empty };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ==========================================
        // 12. EMAIL CONFIRMATION & RESEND
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = "Error confirming your email. The link may have expired or is invalid.";
                return View("ConfirmEmail");
            }

            // Email confirmed! Claim prior guest orders placed with this email
            await ClaimGuestOrdersAsync(user);

            // Auto-sign-in upon verified confirmation
            await _signInManager.SignInAsync(user, isPersistent: false);
            await _cartService.MergeGuestCartAsync(user.Id);

            TempData["SuccessMessage"] = "Your email has been verified! Welcome to Hamara Commerce.";
            return View("ConfirmEmail");
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation(string? email = null)
        {
            return View(new ResendEmailConfirmationViewModel { Email = email ?? string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null && !user.EmailConfirmed && user.IsActive)
            {
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = token }, protocol: Request.Scheme)
                                      ?? $"{Request.Scheme}://{Request.Host}/Account/ConfirmEmail?userId={user.Id}&code={Uri.EscapeDataString(token)}";
                    var emailBody = _emailTemplateService.GenerateAccountVerificationEmail(user.FullName, callbackUrl);
                    _ = _emailSender.SendEmailAsync(user.Email!, "Verify Your Email - Hamara Commerce", emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email verification to {Email}", user.Email);
                }
            }

            // Always display confirmation to prevent email enumeration
            TempData["SuccessMessage"] = "If an account with that email exists, a verification link has been sent. Please check your inbox.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
