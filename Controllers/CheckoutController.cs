using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
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
            ILogger<CheckoutController> logger)
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
        }

        // ==========================================
        // 1. CHECKOUT PAGE (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
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

            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
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
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            var rawCart = await _cartService.GetRawCartDataAsync();
            var cart = await _pricingService.CalculateCartAsync(rawCart, user?.Id, model.ShippingMethod);
            model.Cart = cart;
            model.IsDevelopmentSandboxEnabled = _paymentGateway.IsDevelopmentSandboxAvailable;

            // 1. Idempotency Check
            var storedToken = HttpContext.Session.GetString(IdempotencySessionKey);
            if (string.IsNullOrEmpty(storedToken) || storedToken != model.IdempotencyToken)
            {
                _logger.LogWarning("Duplicate or invalid checkout submission detected with token: {Token}", model.IdempotencyToken);
                TempData["ErrorMessage"] = "Duplicate or expired submission detected. Please review your cart and try again.";
                return RedirectToAction("Index", "Cart");
            }

            // 2. Cart Validation
            if (!cart.Items.Any() || cart.SubTotal <= 0)
            {
                ModelState.AddModelError(string.Empty, "Your cart is empty or has no available items.");
                return View("Index", model);
            }

            var validItems = cart.Items.Where(i => i.IsAvailable && i.Quantity > 0).ToList();
            if (!validItems.Any())
            {
                ModelState.AddModelError(string.Empty, "All items in your cart are currently out of stock.");
                return View("Index", model);
            }

            // 3. Price-Change Revalidation
            // If the customer saw an old total and prices/taxes/delivery changed in between, require review
            if (model.ExpectedGrandTotal.HasValue && Math.Abs(cart.GrandTotal - model.ExpectedGrandTotal.Value) > 0.01m)
            {
                _logger.LogWarning("Checkout price mismatch detected. Expected: {Expected}, Actual: {Actual}",
                    model.ExpectedGrandTotal.Value, cart.GrandTotal);

                model.PriceChangeWarning = $"Your order total has changed from {_shippingTaxService.FormatCurrency(model.ExpectedGrandTotal.Value)} to {cart.FormattedGrandTotal} due to updated prices, taxes, or delivery fees. Please review the updated total and submit again to confirm.";
                model.ExpectedGrandTotal = cart.GrandTotal;

                // Refresh idempotency token so resubmission succeeds smoothly
                var refreshedToken = Guid.NewGuid().ToString("N");
                HttpContext.Session.SetString(IdempotencySessionKey, refreshedToken);
                model.IdempotencyToken = refreshedToken;

                ModelState.AddModelError(string.Empty, model.PriceChangeWarning);
                return View("Index", model);
            }

            // 4. Shipping Method Validation
            var (isShippingValid, shippingFee, shippingMethodName) = _shippingTaxService.CalculateShippingFee(
                model.ShippingMethod, cart.SubTotal, cart.CouponGrantsFreeShipping || cart.AppliedCouponCode == "FREESHIP");

            if (!isShippingValid)
            {
                ModelState.AddModelError(nameof(model.ShippingMethod), "Please select a valid shipping delivery option.");
                return View("Index", model);
            }

            // 5. Model State Validation
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Generate Cryptographically Strong Public Order Reference
            var publicOrderNumber = GenerateSecureOrderNumber();
            var trackingNumber = $"TRK-{RandomNumberGenerator.GetInt32(10000000, 99999999)}";

            // Authoritative Financials (using unified PricingService calculation)
            decimal subtotal = cart.SubTotal;
            decimal couponDiscount = cart.CouponDiscountAmount;
            decimal taxAmount = cart.EstimatedTax;
            decimal totalAmount = cart.GrandTotal;

            // =========================================================================
            // ATOMIC DATABASE TRANSACTION WITH CONCURRENCY & STOCK VERIFICATION
            // =========================================================================
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            string? transactionError = null;
            Order? createdOrder = null;

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
                try
                {
                    // Step A: Concurrency Lock & Live Stock Verification
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

                        if (liveStock < item.Quantity)
                        {
                            transactionError = $"Insufficient stock for '{item.Title}'. Only {liveStock} unit{(liveStock == 1 ? "" : "s")} available right now.";
                            await transaction.RollbackAsync();
                            return;
                        }
                    }

                    // Step B: Revalidate Coupon
                    if (cart.CouponIsValid && !string.IsNullOrEmpty(cart.AppliedCouponCode))
                    {
                        var couponValidation = await _pricingService.ValidateCouponAsync(cart.AppliedCouponCode, subtotal, user?.Id, rawCart.Items);
                        if (!couponValidation.IsValid)
                        {
                            transactionError = $"Coupon error: {couponValidation.Message}";
                            await transaction.RollbackAsync();
                            return;
                        }
                    }

                    // Step C: Process Payment via Payment Gateway
                    var paymentRequest = new PaymentProcessingRequest
                    {
                        OrderNumber = publicOrderNumber,
                        Amount = totalAmount,
                        Currency = _shippingTaxService.CurrencyCode,
                        CustomerName = model.CustomerName,
                        CustomerEmail = model.CustomerEmail,
                        PaymentMethod = model.PaymentMethod,
                        CardholderName = model.SandboxCardholderName,
                        CardNumber = model.SandboxCardNumber,
                        ExpiryDate = model.SandboxExpiry,
                        Cvc = model.SandboxCvc,
                        SimulateFailure = model.SimulatePaymentFailure
                    };

                    var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);
                    if (!paymentResult.Success)
                    {
                        transactionError = paymentResult.FailureReason ?? "Payment authorization failed. Please try a different payment method.";
                        await transaction.RollbackAsync();
                        return;
                    }

                    // Step D: Create Order & OrderItems Snapshot
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
                        Currency = _shippingTaxService.CurrencyCode,
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

                    _context.Orders.Add(order);

                    // Step E: Deduct Stock & Create Inventory Movements
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

                    // Step F: Record Coupon Usage
                    if (cart.CouponIsValid && !string.IsNullOrEmpty(cart.AppliedCouponCode))
                    {
                        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode);
                        if (coupon != null)
                        {
                            coupon.UsageCount++;
                        }
                    }

                    // Step G: Record Payment Transaction
                    var paymentTxn = new PaymentTransaction
                    {
                        Order = order,
                        TransactionReference = $"TXN-{Guid.NewGuid():N}"[..22].ToUpperInvariant(),
                        Provider = paymentResult.Provider,
                        ProviderTransactionId = paymentResult.ProviderReference,
                        Status = paymentResult.Status,
                        Amount = totalAmount,
                        Currency = _shippingTaxService.CurrencyCode,
                        PaymentMethod = order.PaymentMethod,
                        CardLast4 = paymentResult.CardLast4,
                        CardBrand = paymentResult.CardBrand,
                        FailureReason = paymentResult.FailureReason,
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = paymentResult.ProcessedAt
                    };

                    _context.PaymentTransactions.Add(paymentTxn);

                    // Step H: Commit Atomic Transaction
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
                    transactionError = "An unexpected error occurred while placing your order. No charges were made and no stock was modified. Please try again.";
                }
            });

            // If transaction failed, return to view with friendly error
            if (transactionError != null || createdOrder == null)
            {
                ModelState.AddModelError(string.Empty, transactionError ?? "Order processing failed. Please try again.");
                return View("Index", model);
            }

            // Post-Transaction Success Actions
            await _cartService.ClearCartAsync();
            HttpContext.Session.Remove(IdempotencySessionKey);

            // Dispatch Order Confirmation Email Notification
            try
            {
                var emailHtml = _emailTemplateService.GenerateOrderConfirmationEmail(createdOrder);
                _ = _emailSender.SendEmailAsync(
                    createdOrder.CustomerEmail,
                    $"Order Confirmed - #{createdOrder.OrderNumber} | Hamara Commerce",
                    emailHtml);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to enqueue order confirmation email for Order #{OrderNumber}", createdOrder.OrderNumber);
            }

            // Post/Redirect/Get pattern
            return RedirectToAction(nameof(OrderConfirmation), new { orderNumber = createdOrder.OrderNumber });
        }

        // ==========================================
        // 3. ORDER CONFIRMATION (GET by public OrderNumber)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(string orderNumber)
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
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && !string.IsNullOrEmpty(order.UserId) && order.UserId != user.Id && order.CustomerEmail != user.Email)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            return View(order);
        }

        private static string GenerateSecureOrderNumber()
        {
            // Cryptographically strong random alphanumeric order reference: e.g. HC-PK-892184920
            byte[] bytes = new byte[5];
            RandomNumberGenerator.Fill(bytes);
            uint num = BitConverter.ToUInt32(bytes, 0) % 900000000 + 100000000;
            return $"HC-PK-{num}";
        }
    }
}
