using System.Text.Json;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Controllers
{
    public class CheckoutController : Controller
    {
        private const string IdempotencySessionKey = "HamaraCommerce_Checkout_IdempotencyToken";

        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IPricingService _pricingService;
        private readonly IShippingTaxService _shippingTaxService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailOutboxService? _emailOutboxService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ApplicationDbContext context,
            ICartService cartService,
            IPricingService pricingService,
            IShippingTaxService shippingTaxService,
            IPaymentGateway paymentGateway,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<CheckoutController> logger,
            IEmailOutboxService? emailOutboxService = null)
        {
            _context = context;
            _cartService = cartService;
            _pricingService = pricingService;
            _shippingTaxService = shippingTaxService;
            _paymentGateway = paymentGateway;
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
            _emailOutboxService = emailOutboxService;
        }

        // ==========================================
        // 1. CHECKOUT PAGE (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;

            // Enforce EnableGuestCheckout
            var storeSettings = _shippingTaxService.GetStoreSettings();
            if (!storeSettings.EnableGuestCheckout && user == null)
            {
                TempData["ErrorMessage"] = "Guest checkout is disabled. Please log in or create an account to proceed.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }
            var cart = await _cartService.GetCartViewModelAsync();
            if (!cart.Items.Any() || cart.SubTotal <= 0)
            {
                TempData["ErrorMessage"] = "Your shopping cart is empty. Please add items before proceeding to checkout.";
                return RedirectToAction("Index", "Shop");
            }

            if (cart.HasOutOfStockItems)
            {
                TempData["WarningMessage"] = "One or more items in your cart are currently out of stock. Please adjust your cart before continuing.";
                return RedirectToAction("Index", "Cart");
            }

            var defaultAddr = user?.SavedAddresses.FirstOrDefault(a => a.IsDefault) ?? user?.SavedAddresses.FirstOrDefault();

            // Generate fresh anti-duplicate Idempotency Token
            var idempotencyToken = Guid.NewGuid().ToString("N");
            HttpContext.Session.SetString(IdempotencySessionKey, idempotencyToken);

            var model = new CheckoutFormViewModel
            {
                Cart = cart,
                CustomerName = user?.FullName ?? string.Empty,
                CustomerEmail = user?.Email ?? string.Empty,
                CustomerPhone = user?.PhoneNumber ?? string.Empty,
                StreetAddress = defaultAddr?.StreetAddress ?? string.Empty,
                City = defaultAddr?.City ?? "Lahore",
                State = defaultAddr?.State ?? "Punjab",
                PostalCode = defaultAddr?.PostalCode ?? "54000",
                Country = defaultAddr?.Country ?? "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "CashOnDelivery",
                IdempotencyToken = idempotencyToken,
                ExpectedGrandTotal = cart.GrandTotal,
                IsDevelopmentSandboxEnabled = _paymentGateway.IsDevelopmentSandboxAvailable
            };

            return View(model);
        }

        // ==========================================
        // 1.1 RECALCULATE CHECKOUT SUMMARY (AJAX)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Recalculate([FromBody] CheckoutRecalculateRequest request)
        {
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            var rawCart = await _cartService.GetRawCartDataAsync();
            var cart = await _pricingService.CalculateCartAsync(rawCart, user?.Id, request.ShippingMethod);

            return Json(new
            {
                subtotal = cart.FormattedSubTotal,
                subtotalValue = cart.SubTotal,
                discount = cart.FormattedDiscount,
                discountValue = cart.CouponDiscountAmount,
                couponCode = cart.AppliedCouponCode,
                tax = cart.FormattedTax,
                taxValue = cart.EstimatedTax,
                taxRatePercent = cart.TaxRatePercent,
                shipping = cart.FormattedShipping,
                shippingValue = cart.EffectiveShippingFee,
                shippingMethodName = cart.ShippingMethodName,
                grandTotal = cart.FormattedGrandTotal,
                grandTotalValue = cart.GrandTotal,
                totalSavings = cart.FormattedTotalSavings,
                currencyCode = cart.CurrencyCode,
                currencySymbol = cart.CurrencySymbol
            });
        }

        // ==========================================
        // 2. PROCESS ORDER (POST - ATOMIC TRANSACTION)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(CheckoutFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.IdempotencyToken) || model.IdempotencyToken.Length > 64)
                return BadRequest("A valid checkout token is required. Reload checkout and try again.");
            await using var gate = await CommerceDatabaseWork.TryLockAsync(_context,
                "checkout:" + model.IdempotencyToken, HttpContext.RequestAborted);
            if (gate == null) return Conflict("This checkout is processing. Retry the same request shortly.");
            return await ProcessOrderCore(model);
        }

        private async Task<IActionResult> ProcessOrderCore(CheckoutFormViewModel model)
        {
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;

            // 0. Enforce EnableGuestCheckout
            var storeSettings = _shippingTaxService.GetStoreSettings();
            if (!storeSettings.EnableGuestCheckout && user == null)
            {
                TempData["ErrorMessage"] = "Guest checkout is disabled. Please log in or create an account to proceed.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }

            var existingRecord = await _context.CheckoutIdempotencyRecords
                .FirstOrDefaultAsync(r => r.IdempotencyKey == model.IdempotencyToken);
            var emailLower = model.CustomerEmail?.Trim().ToLowerInvariant() ?? string.Empty;
            // Authenticate ownership BEFORE deserializing or exposing any saved purchase data.
            if (existingRecord != null &&
                (existingRecord.UserId != null ? existingRecord.UserId != user?.Id :
                    !string.Equals(existingRecord.CustomerEmail, emailLower, StringComparison.OrdinalIgnoreCase)))
                return Forbid();

            ShoppingCartViewModel cart;
            if (existingRecord?.CartSnapshotJson != null)
            {
                cart = JsonSerializer.Deserialize<ShoppingCartViewModel>(existingRecord.CartSnapshotJson)
                    ?? throw new InvalidOperationException("Checkout snapshot is invalid.");
            }
            else if (existingRecord?.Status == IdempotencyStatus.Completed && existingRecord.OrderId.HasValue)
            {
                // Compatibility for completed purchases made before snapshots were introduced.
                var saved = await _context.Orders.AsNoTracking().Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == existingRecord.OrderId);
                if (saved == null) return Conflict("Saved checkout order could not be found. Contact support.");
                cart = new ShoppingCartViewModel { GrandTotal = saved.TotalAmount, SubTotal = saved.Subtotal,
                    AppliedCouponCode = saved.CouponCode, CurrencyCode = saved.Currency,
                    Items = saved.Items.Select(i => new CartItemViewModel { ProductId = i.ProductId,
                        VariantId = i.VariantId, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() };
            }
            else
            {
                var rawCart = await _cartService.GetRawCartDataAsync();
                cart = await _pricingService.CalculateCartAsync(rawCart, user?.Id, model.ShippingMethod);
            }
            model.Cart = cart;
            model.IsDevelopmentSandboxEnabled = _paymentGateway.IsDevelopmentSandboxAvailable;
            if (!_paymentGateway.IsMethodSupported(model.PaymentMethod))
                ModelState.AddModelError(nameof(model.PaymentMethod), "Select an available payment method.");
            string requestHash = ComputeRequestHash(model, cart, existingRecord?.HashVersion ?? 2);

            bool isPaymentRecovered = false;

            if (existingRecord != null)
            {
                // Expiry Check
                if (existingRecord.ExpiresAt < DateTime.UtcNow && existingRecord.Status != IdempotencyStatus.Completed && string.IsNullOrEmpty(existingRecord.PaymentReference))
                {
                    _logger.LogWarning("Checkout idempotency token expired: {Token}", model.IdempotencyToken);
                    ModelState.AddModelError(string.Empty, "This checkout session has expired. Please return to your cart and start a fresh checkout.");
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return View("Index", model);
                }

                // Verify Owner Binding
                bool ownerValid = true;
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(existingRecord.UserId) && existingRecord.UserId != user.Id)
                    {
                        ownerValid = false;
                    }
                    else if (string.IsNullOrEmpty(existingRecord.UserId) && !string.Equals(existingRecord.CustomerEmail, emailLower, StringComparison.OrdinalIgnoreCase))
                    {
                        ownerValid = false;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(existingRecord.UserId))
                    {
                        ownerValid = false;
                    }
                    else if (!string.Equals(existingRecord.CustomerEmail, emailLower, StringComparison.OrdinalIgnoreCase))
                    {
                        ownerValid = false;
                    }
                }

                if (!ownerValid)
                {
                    _logger.LogWarning("Checkout idempotency owner mismatch for token {Token}", model.IdempotencyToken);
                    TempData["ErrorMessage"] = "Invalid checkout session owner. Please review your cart and try again.";
                    return RedirectToAction("Index", "Cart");
                }

                // Canonical Request Hash Matching: reject changed payloads immediately
                if (!string.Equals(existingRecord.RequestHash, requestHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Checkout idempotency hash mismatch for token {Token}", model.IdempotencyToken);
                    ModelState.AddModelError(string.Empty, "Idempotency key reused with different request parameters. Please return to your cart and submit a fresh order.");
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return View("Index", model);
                }

                // Status Evaluation
                if (existingRecord.Status == IdempotencyStatus.Completed && existingRecord.OrderId.HasValue && !string.IsNullOrEmpty(existingRecord.OrderNumber))
                {
                    _logger.LogInformation("Returning existing order {OrderNumber} for completed idempotent replay", existingRecord.OrderNumber);

                    if (!string.IsNullOrEmpty(existingRecord.GuestAccessToken))
                    {
                        HttpContext.Session.SetString($"GuestOrderToken_{existingRecord.OrderNumber}", existingRecord.GuestAccessToken);
                    }

                    return RedirectToAction(nameof(OrderConfirmation), new
                    {
                        orderNumber = existingRecord.OrderNumber,
                        guestToken = existingRecord.GuestAccessToken
                    });
                }

                if (existingRecord.Status == IdempotencyStatus.RecoveryRequired && string.IsNullOrEmpty(existingRecord.PaymentReference))
                {
                    // An external call may have succeeded before its result was saved. Never charge blindly.
                    return Conflict("Payment outcome is awaiting reconciliation. Contact support with your checkout reference; do not pay again.");
                }
                isPaymentRecovered = !string.IsNullOrEmpty(existingRecord.PaymentReference);
                existingRecord.LockedAt = DateTime.UtcNow;
                existingRecord.Status = isPaymentRecovered ? IdempotencyStatus.PaymentCompleted : IdempotencyStatus.Processing;
                await _context.SaveChangesAsync();
            }

            CheckoutIdempotencyRecord currentRecord;
            if (existingRecord != null)
            {
                currentRecord = existingRecord;
            }
            else
            {
                var newRecord = new CheckoutIdempotencyRecord
                {
                    IdempotencyKey = model.IdempotencyToken,
                    UserId = user?.Id,
                    CustomerEmail = emailLower,
                    RequestHash = requestHash,
                    CartSnapshotJson = JsonSerializer.Serialize(cart),
                    HashVersion = 2,
                    Status = IdempotencyStatus.Processing,
                    LockedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                _context.CheckoutIdempotencyRecords.Add(newRecord);
                try
                {
                    await _context.SaveChangesAsync();
                    currentRecord = newRecord;
                }
                catch (DbUpdateException)
                {
                    _context.Entry(newRecord).State = EntityState.Detached;
                    // Do not expose a collided record's order/token. The next request follows normal owner/hash checks.
                    if (!await _context.CheckoutIdempotencyRecords.AsNoTracking().AnyAsync(r => r.IdempotencyKey == model.IdempotencyToken)) throw;
                    return Conflict("Checkout state changed. Retry the same request.");
                }
            }

            // 2. Cart Validation
            if (!cart.Items.Any() || cart.SubTotal <= 0)
            {
                currentRecord.Status = IdempotencyStatus.Failed;
                currentRecord.FailureReason = "Cart empty";
                RefreshCheckoutToken(model);
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "Your cart is empty or has no available items.");
                return View("Index", model);
            }

            var validItems = cart.Items.Where(i => i.IsAvailable && i.Quantity > 0).ToList();
            if (!validItems.Any() || validItems.Count != cart.Items.Count)
            {
                currentRecord.Status = IdempotencyStatus.Failed;
                currentRecord.FailureReason = "All items out of stock";
                RefreshCheckoutToken(model);
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "All items in your cart are currently out of stock.");
                return View("Index", model);
            }

            // 3. Price-Change Revalidation
            if (model.ExpectedGrandTotal.HasValue && Math.Abs(cart.GrandTotal - model.ExpectedGrandTotal.Value) > 0.01m)
            {
                _logger.LogWarning("Checkout price mismatch detected. Expected: {Expected}, Actual: {Actual}",
                    model.ExpectedGrandTotal.Value, cart.GrandTotal);

                model.PriceChangeWarning = $"Your order total has changed from {_shippingTaxService.FormatCurrency(model.ExpectedGrandTotal.Value)} to {cart.FormattedGrandTotal} due to updated prices, taxes, or delivery fees. Please review the updated total and submit again to confirm.";
                model.ExpectedGrandTotal = cart.GrandTotal;

                currentRecord.Status = IdempotencyStatus.Failed;
                currentRecord.FailureReason = "Price change revalidation";
                await _context.SaveChangesAsync();

                // Refresh idempotency token so resubmission succeeds smoothly
                var refreshedToken = Guid.NewGuid().ToString("N");
                HttpContext.Session.SetString(IdempotencySessionKey, refreshedToken);
                model.IdempotencyToken = refreshedToken;

                ModelState.Remove(nameof(model.IdempotencyToken));
                ModelState.Remove(nameof(model.ExpectedGrandTotal));

                ModelState.AddModelError(string.Empty, model.PriceChangeWarning);
                return View("Index", model);
            }

            // 4. Shipping Method Validation
            var (isShippingValid, shippingFee, shippingMethodName) = _shippingTaxService.CalculateShippingFee(
                model.ShippingMethod, cart.SubTotal, cart.CouponGrantsFreeShipping);

            if (!isShippingValid)
            {
                ModelState.AddModelError(nameof(model.ShippingMethod), "Please select a valid shipping delivery option.");
            }

            // 5. Limited Coupon Verified Eligibility Validation
            if (cart.CouponIsValid && !string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                var couponCheck = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode);
                if (couponCheck != null && couponCheck.PerUserLimit > 0)
                {
                    if (user == null || !user.EmailConfirmed)
                    {
                        currentRecord.Status = IdempotencyStatus.Failed;
                        currentRecord.FailureReason = "Limited coupon requires account with verified email";
                RefreshCheckoutToken(model);
                        await _context.SaveChangesAsync();

                        ModelState.AddModelError(string.Empty, $"Coupon '{couponCheck.Code}' has customer usage limits and requires an account with a verified email address. Entered email alone is not eligible.");
                        return View("Index", model);
                    }
                }
            }

            // 6. Model State Validation
            if (!ModelState.IsValid)
            {
                currentRecord.Status = IdempotencyStatus.Failed;
                currentRecord.FailureReason = "Validation errors";
                RefreshCheckoutToken(model);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString(IdempotencySessionKey, model.IdempotencyToken);
                ModelState.Remove(nameof(model.IdempotencyToken));
                return View("Index", model);
            }

            // Authoritative Order Reference & Financials
            var publicOrderNumber = currentRecord.OrderNumber ?? GenerateSecureOrderNumber();
            currentRecord.OrderNumber = publicOrderNumber;
            currentRecord.CartSnapshotJson ??= JsonSerializer.Serialize(cart);
            await _context.SaveChangesAsync();
            var trackingNumber = $"TRK-{RandomNumberGenerator.GetInt32(10000000, 99999999)}";

            decimal subtotal = cart.SubTotal;
            decimal couponDiscount = cart.CouponDiscountAmount;
            decimal taxAmount = cart.EstimatedTax;
            decimal totalAmount = cart.GrandTotal;

            // =========================================================================
            // 7. EXTERNAL PAYMENT GATEWAY (OUTSIDE RETRYABLE DATABASE TRANSACTION)
            // =========================================================================
            PaymentProcessingResult paymentResult;

            if (isPaymentRecovered && !string.IsNullOrEmpty(currentRecord.PaymentReference))
            {
                if (currentRecord.PaymentAmount != totalAmount)
                    return Conflict("Saved payment amount does not match this purchase. Support reconciliation is required.");
                _logger.LogInformation("Checkout recovering previously authorized payment {Ref} for idempotency token {Token}",
                    currentRecord.PaymentReference, currentRecord.IdempotencyKey);

                paymentResult = new PaymentProcessingResult
                {
                    Success = true,
                    Provider = currentRecord.PaymentProvider ?? "AuthoritativeGateway",
                    ProviderReference = currentRecord.PaymentReference,
                    Status = currentRecord.PaymentStatus ?? PaymentStatus.Pending,
                    ProcessedAt = currentRecord.CreatedAt
                };
            }
            else
            {
                var paymentRequest = new PaymentProcessingRequest
                {
                    IdempotencyKey = currentRecord.IdempotencyKey,
                    OrderNumber = publicOrderNumber,
                    Amount = totalAmount,
                    Currency = cart.CurrencyCode,
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    PaymentMethod = model.PaymentMethod,
                    CardholderName = model.SandboxCardholderName,
                    CardNumber = model.SandboxCardNumber,
                    ExpiryDate = model.SandboxExpiry,
                    Cvc = model.SandboxCvc,
                    SimulateFailure = model.SimulatePaymentFailure
                };

                // Persist intent before the external side effect. A crash leaves an honest uncertain outcome.
                currentRecord.Status = IdempotencyStatus.RecoveryRequired;
                currentRecord.FailureReason = "Payment outcome awaiting reconciliation";
                await _context.SaveChangesAsync();
                try { paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest); }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Uncertain payment outcome for checkout {Key}", currentRecord.IdempotencyKey);
                    return Conflict("Payment outcome is awaiting reconciliation. Contact support before retrying payment.");
                }

                if (!paymentResult.Success)
                {
                    currentRecord.Status = IdempotencyStatus.Failed;
                    currentRecord.FailureReason = paymentResult.FailureReason;
                    currentRecord.PaymentStatus = paymentResult.Status;
                    await _context.SaveChangesAsync();

                    ModelState.AddModelError(string.Empty, paymentResult.FailureReason ?? "Payment authorization failed. Please try a different payment method.");
                    return View("Index", model);
                }

                // DURABLE PAYMENT STATE TRANSITION: Record payment success immediately before starting database transaction
                currentRecord.Status = IdempotencyStatus.PaymentCompleted;
                currentRecord.PaymentProvider = paymentResult.Provider;
                currentRecord.PaymentReference = paymentResult.ProviderReference;
                currentRecord.PaymentAmount = totalAmount;
                currentRecord.PaymentStatus = paymentResult.Status;
                await _context.SaveChangesAsync();
            }

            // =========================================================================
            // 8. ATOMIC DATABASE TRANSACTION WITH CONCURRENCY & STOCK RESERVATION
            // =========================================================================
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            string? transactionError = null;
            Order? createdOrder = null;

            await executionStrategy.ExecuteAsync(async () =>
            {
                transactionError = null;
                // Reload all tracked stock/coupon values inside the transaction, including after a retry.
                _context.ChangeTracker.Clear();
                using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                try
                {
                    currentRecord = await _context.CheckoutIdempotencyRecords.FirstAsync(r => r.IdempotencyKey == model.IdempotencyToken);
                    if (currentRecord.Status == IdempotencyStatus.Completed && currentRecord.OrderId.HasValue)
                    {
                        createdOrder = await _context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == currentRecord.OrderId);
                        await transaction.CommitAsync();
                        return;
                    }
                    // Lock products in stable order before reading tracked quantities.
                    if (_context.Database.IsSqlServer())
                        foreach (var productId in validItems.Select(i => i.ProductId).Distinct().OrderBy(i => i))
                            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Products WITH (UPDLOCK,HOLDLOCK) WHERE Id={productId}");
                    // Step A: Live Stock Verification & Concurrency Row Lock
                    foreach (var item in validItems)
                    {
                        var prod = await _context.Products
                            .Include(p => p.Variants)
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                        if (prod == null || prod.Status != ProductStatus.Published)
                        {
                            transactionError = $"Product '{item.Title}' is no longer available in our store catalog.";
                            await transaction.RollbackAsync();
                            return;
                        }

                        int liveStock = prod.Stock;
                        if (item.VariantId.HasValue && item.VariantId.Value > 0)
                        {
                            var variant = prod.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value && v.IsActive);
                            if (variant == null)
                            {
                                transactionError = $"The selected variant for '{item.Title}' is no longer active.";
                                await transaction.RollbackAsync();
                                return;
                            }
                            liveStock = variant.Stock;
                        }

                        if (liveStock < validItems.Where(i => i.ProductId == item.ProductId && i.VariantId == item.VariantId).Sum(i => i.Quantity) || prod.Stock < validItems.Where(i => i.ProductId == item.ProductId).Sum(i => i.Quantity))
                        {
                            transactionError = $"Insufficient stock for '{item.Title}'. Only {liveStock} unit{(liveStock == 1 ? "" : "s")} available right now.";
                            await transaction.RollbackAsync();
                            return;
                        }
                    }

                    // Step B: Atomically Revalidate Coupon Limits & Verified Customer Eligibility
                    Coupon? couponEntity = null;
                    if (cart.CouponIsValid && !string.IsNullOrEmpty(cart.AppliedCouponCode))
                    {
                        if (_context.Database.IsSqlServer())
                            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Coupons WITH (UPDLOCK,HOLDLOCK) WHERE Code={cart.AppliedCouponCode}");
                        couponEntity = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode);
                        if (couponEntity == null)
                        {
                            transactionError = "Applied coupon no longer exists. Support must reconcile the payment.";
                            await transaction.RollbackAsync();
                            return;
                        }
                        if (couponEntity != null)
                        {
                            var now = DateTime.UtcNow;
                            if (!couponEntity.IsActive || now < couponEntity.StartDate || now > couponEntity.ExpiryDate || (couponEntity.UsageLimit > 0 && couponEntity.UsageCount >= couponEntity.UsageLimit))
                            {
                                transactionError = $"Coupon '{couponEntity.Code}' is no longer valid or its global usage limit has been reached.";
                                await transaction.RollbackAsync();
                                return;
                            }

                            if (couponEntity.PerUserLimit > 0)
                            {
                                if (user == null || !user.EmailConfirmed)
                                {
                                    transactionError = $"Coupon '{couponEntity.Code}' requires an account with a verified email address. Entered email alone is not eligible.";
                                    await transaction.RollbackAsync();
                                    return;
                                }

                                var verifiedEmail = user.Email?.Trim().ToLowerInvariant();

                                var priorRedemptionsCount = await _context.CouponRedemptions
                                    .Where(r => r.CouponCode == couponEntity.Code && !r.IsRestored)
                                    .Where(r => r.UserId == user.Id || (verifiedEmail != null && r.CustomerEmail.ToLower() == verifiedEmail))
                                    .Select(r => r.OrderId)
                                    .Distinct()
                                    .CountAsync();

                                if (priorRedemptionsCount >= couponEntity.PerUserLimit)
                                {
                                    transactionError = $"You have already redeemed coupon '{couponEntity.Code}' the maximum allowed times ({couponEntity.PerUserLimit} per customer).";
                                    await transaction.RollbackAsync();
                                    return;
                                }
                            }

                            couponEntity.UsageCount++;
                        }
                    }

                    // Step C: Create Order Entity
                    var order = new Order
                    {
                        OrderNumber = publicOrderNumber,
                        TrackingNumber = trackingNumber,
                        UserId = user?.Id,
                        CustomerName = model.CustomerName.Trim(),
                        CustomerEmail = model.CustomerEmail.Trim(),
                        CustomerPhone = model.CustomerPhone.Trim(),
                        ShippingAddress = model.StreetAddress.Trim(),
                        City = model.City.Trim(),
                        State = model.State.Trim(),
                        PostalCode = model.PostalCode.Trim(),
                        Country = model.Country.Trim(),
                        ShippingMethod = shippingMethodName,
                        PaymentMethod = model.PaymentMethod switch
                        {
                            "CashOnDelivery" or "COD" => "Cash on Delivery",
                            "SandboxCard" => "Sandbox Test Card",
                            _ => model.PaymentMethod
                        },
                        PaymentStatus = paymentResult.Status,
                        Status = OrderStatus.Confirmed,
                        Currency = cart.CurrencyCode,
                        OrderDate = DateTime.UtcNow,
                        EstimatedDeliveryDate = DateTime.UtcNow.AddDays(model.ShippingMethod == "Overnight" ? 1 : (model.ShippingMethod == "Express" ? 2 : 4)),

                        Subtotal = subtotal,
                        DiscountAmount = couponDiscount,
                        TaxAmount = taxAmount,
                        ShippingFee = cart.EffectiveShippingFee,
                        TotalAmount = totalAmount,
                        CouponCode = cart.CouponIsValid ? cart.AppliedCouponCode : null,
                        CustomerNotes = model.CustomerNotes?.Trim(),

                        Items = validItems.Select(i => new OrderItem
                        {
                            ProductId = i.ProductId,
                            VariantId = i.VariantId,
                            VariantName = i.VariantName,
                            ProductTitle = i.Title,
                            ProductImage = i.ImageUrl,
                            SKU = i.SKU,
                            UnitPrice = i.UnitPrice,
                            Quantity = i.Quantity
                        }).ToList()
                    };

                    // For guest orders: generate secure, expiring order-scoped guest token
                    if (user == null)
                    {
                        order.GuestAccessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                        order.GuestAccessExpiry = DateTime.UtcNow.AddDays(30);
                    }

                    _context.Orders.Add(order);

                    // Step D: Deduct Stock & Record Inventory Movements
                    foreach (var item in validItems)
                    {
                        var prod = await _context.Products.Include(p => p.Variants).FirstAsync(p => p.Id == item.ProductId);
                        int prevStock = prod.Stock;
                        prod.Stock = Math.Max(0, prod.Stock - item.Quantity);

                        if (item.VariantId.HasValue && item.VariantId.Value > 0)
                        {
                            var variant = prod.Variants.First(v => v.Id == item.VariantId.Value);
                            variant.Stock = Math.Max(0, variant.Stock - item.Quantity);
                        }

                        _context.InventoryMovements.Add(new InventoryMovement
                        {
                            ProductId = prod.Id,
                            VariantId = item.VariantId,
                            MovementType = InventoryMovementType.OrderDeduction,
                            QuantityChange = -item.Quantity,
                            OldStock = prevStock,
                            NewStock = prod.Stock,
                            Reason = $"Purchased in Order #{order.OrderNumber}",
                            Order = order,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Step E: Create Dedicated Concurrency-Safe CouponRedemption Record
                    if (couponEntity != null)
                    {
                        _context.CouponRedemptions.Add(new CouponRedemption
                        {
                            CouponId = couponEntity.Id,
                            CouponCode = couponEntity.Code,
                            Order = order,
                            UserId = user?.Id,
                            CustomerEmail = model.CustomerEmail.Trim(),
                            DiscountAmount = couponDiscount,
                            RedeemedAt = DateTime.UtcNow,
                            IsRestored = false
                        });
                    }

                    // Step F: Record Payment Transaction
                    var paymentTxn = new PaymentTransaction
                    {
                        Order = order,
                        TransactionReference = $"TXN-{Guid.NewGuid():N}"[..22].ToUpperInvariant(),
                        Provider = paymentResult.Provider,
                        ProviderTransactionId = paymentResult.ProviderReference,
                        Status = paymentResult.Status,
                        Amount = totalAmount,
                        Currency = cart.CurrencyCode,
                        PaymentMethod = order.PaymentMethod,
                        CardLast4 = paymentResult.CardLast4,
                        CardBrand = paymentResult.CardBrand,
                        FailureReason = paymentResult.FailureReason,
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = paymentResult.ProcessedAt
                    };

                    _context.PaymentTransactions.Add(paymentTxn);

                    // Step G: Bind Order to Idempotency Record
                    currentRecord.OrderNumber = order.OrderNumber;
                    currentRecord.GuestAccessToken = order.GuestAccessToken;
                    currentRecord.Status = IdempotencyStatus.Completed;
                    await _context.SaveChangesAsync();

                    currentRecord.OrderId = order.Id;
                    await _context.SaveChangesAsync();

                    // Save the email event in the SAME transaction as the order.
                    _context.EmailOutboxMessages.Add(EmailOutboxService.CreateMessage(
                        order.CustomerEmail, $"Order Confirmed - #{order.OrderNumber} | Hamara Commerce",
                        _emailTemplateService.GenerateOrderConfirmationEmail(order), eventKey: $"order-confirmation:{order.OrderNumber}"));
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    createdOrder = order;

                    _logger.LogInformation("Order {OrderNumber} successfully created in atomic transaction. Amount: {Total} {Currency}",
                        order.OrderNumber, totalAmount, order.Currency);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Transaction aborted and rolled back during checkout process.");
                    transactionError = "An unexpected error occurred while placing your order. No stock was modified. Please try again.";
                }
            });

            if (transactionError != null || createdOrder == null)
            {
                // Payment succeeded on gateway, but order finalization failed -> mark RecoveryRequired
                _context.ChangeTracker.Clear();
                var recordToUpdate = await _context.CheckoutIdempotencyRecords
                    .FirstOrDefaultAsync(r => r.IdempotencyKey == model.IdempotencyToken);

                if (recordToUpdate != null)
                {
                    recordToUpdate.Status = IdempotencyStatus.RecoveryRequired;
                    recordToUpdate.FailureReason = transactionError ?? "Order finalization failed after payment.";
                    await _context.SaveChangesAsync();
                }

                _logger.LogError("CRITICAL: Payment {PaymentRef} succeeded for order {OrderNumber} but DB transaction failed: {Error}",
                    currentRecord.PaymentReference, publicOrderNumber, transactionError);

                ModelState.AddModelError(string.Empty, $"Payment authorization succeeded (Ref: {currentRecord.PaymentReference}), but order finalization encountered a temporary issue: {transactionError}. Please submit again to finalize your order without being re-charged.");
                return View("Index", model);
            }

            // Post-Transaction Success Actions
            await _cartService.ClearCartAsync();
            HttpContext.Session.Remove(IdempotencySessionKey);

            if (!string.IsNullOrEmpty(createdOrder.GuestAccessToken))
            {
                HttpContext.Session.SetString($"GuestOrderToken_{createdOrder.OrderNumber}", createdOrder.GuestAccessToken);
            }

            // Post/Redirect/Get pattern
            return RedirectToAction(nameof(OrderConfirmation), new { orderNumber = createdOrder.OrderNumber, guestToken = createdOrder.GuestAccessToken });
        }

        // ==========================================
        // 3. ORDER CONFIRMATION (GET by public OrderNumber)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(string orderNumber, string? guestToken = null)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim());

            if (order == null)
            {
                return NotFound();
            }

            // Secure customer order ownership check
            var currentUser = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            bool isAuthorized = false;

            if (User.IsInRole("Admin"))
            {
                isAuthorized = true;
            }
            else if (!string.IsNullOrEmpty(order.UserId))
            {
                // Registered order: MUST belong to currentUser.Id
                if (currentUser != null && order.UserId == currentUser.Id)
                {
                    isAuthorized = true;
                }
            }
            else
            {
                // Guest order: requires valid guest token (from query or session) or verified customer claim
                var sessionToken = HttpContext.Session.GetString($"GuestOrderToken_{order.OrderNumber}");
                var tokenToCheck = !string.IsNullOrEmpty(guestToken) ? guestToken : sessionToken;

                bool hasValidToken = !string.IsNullOrEmpty(tokenToCheck) &&
                                     order.GuestAccessToken == tokenToCheck &&
                                     (order.GuestAccessExpiry == null || order.GuestAccessExpiry > DateTime.UtcNow);

                if (hasValidToken)
                {
                    isAuthorized = true;
                }
                else if (currentUser != null && currentUser.EmailConfirmed && order.CustomerEmail.Equals(currentUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    // Verified customer claiming their guest order
                    order.UserId = currentUser.Id;
                    await _context.SaveChangesAsync();
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                _logger.LogWarning("Unauthorized access attempt to OrderConfirmation for Order #{OrderNumber}", order.OrderNumber);
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.GuestToken = order.GuestAccessToken;
            return View(order);
        }

        private static string ComputeRequestHash(CheckoutFormViewModel model, ShoppingCartViewModel cart, int version = 2)
        {
            if (version >= 2)
            {
                // JSON avoids delimiter ambiguity and uses invariant numeric serialization.
                var canonical = JsonSerializer.Serialize(new {
                    Email = model.CustomerEmail?.Trim().ToLowerInvariant(), Name = model.CustomerName?.Trim(),
                    Phone = model.CustomerPhone?.Trim(), Address = model.StreetAddress?.Trim(),
                    City = model.City?.Trim(), State = model.State?.Trim(), PostalCode = model.PostalCode?.Trim(),
                    Country = model.Country?.Trim(), Shipping = model.ShippingMethod?.Trim(),
                    Payment = model.PaymentMethod?.Trim(), Notes = model.CustomerNotes?.Trim(),
                    model.ExpectedGrandTotal, cart.GrandTotal, cart.CurrencyCode, cart.AppliedCouponCode,
                    Items = cart.Items.OrderBy(i => i.ProductId).ThenBy(i => i.VariantId).Select(i => new { i.ProductId, i.VariantId, i.Quantity, i.UnitPrice })
                });
                return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
            }
            var sb = new StringBuilder();
            sb.Append("EMAIL:").Append(model.CustomerEmail?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("NAME:").Append(model.CustomerName?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("ADDR:").Append(model.StreetAddress?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("CITY:").Append(model.City?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("STATE:").Append(model.State?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("ZIP:").Append(model.PostalCode?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("COUNTRY:").Append(model.Country?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("SHIP:").Append(model.ShippingMethod?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("PAY:").Append(model.PaymentMethod?.Trim().ToLowerInvariant()).Append(';');
            sb.Append("EXP_TOTAL:").Append(model.ExpectedGrandTotal?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).Append(';');
            sb.Append("TOTAL:").Append(cart.GrandTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).Append(';');
            sb.Append("COUPON:").Append(cart.AppliedCouponCode?.Trim().ToUpperInvariant()).Append(';');
            sb.Append("ITEMS:[");
            foreach (var item in cart.Items.OrderBy(i => i.ProductId).ThenBy(i => i.VariantId ?? 0))
            {
                sb.Append(FormattableString.Invariant($"{item.ProductId}:{item.VariantId}:{item.Quantity}:{item.UnitPrice:F2};"));
            }
            sb.Append(']');

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(hash);
        }

        private void RefreshCheckoutToken(CheckoutFormViewModel model)
        {
            model.IdempotencyToken = Guid.NewGuid().ToString("N");
            HttpContext.Session.SetString(IdempotencySessionKey, model.IdempotencyToken);
            ModelState.Remove(nameof(model.IdempotencyToken));
        }

        private static string GenerateSecureOrderNumber()
        {
            byte[] bytes = new byte[5];
            RandomNumberGenerator.Fill(bytes);
            uint num = BitConverter.ToUInt32(bytes, 0) % 900000000 + 100000000;
            return $"HC-PK-{num}";
        }
    }
}
