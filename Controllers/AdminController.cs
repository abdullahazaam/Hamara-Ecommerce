using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IShippingTaxService _shippingTaxService;
        private readonly IPricingService _pricingService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailOutboxService? _emailOutboxService;
        private readonly IReturnRefundService? _returnRefundService;
        private readonly IOperationalRecoveryService _operationalRecoveryService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IShippingTaxService shippingTaxService,
            IPricingService pricingService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<AdminController> logger,
            IEmailOutboxService? emailOutboxService = null,
            IReturnRefundService? returnRefundService = null,
            IOperationalRecoveryService? operationalRecoveryService = null)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _shippingTaxService = shippingTaxService;
            _pricingService = pricingService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
            _emailOutboxService = emailOutboxService;
            _returnRefundService = returnRefundService;
            _operationalRecoveryService = operationalRecoveryService ?? new OperationalRecoveryService(context, Microsoft.Extensions.Logging.Abstractions.NullLogger<OperationalRecoveryService>.Instance);
        }

        // ==========================================
        // 1. DASHBOARD (REAL DATABASE-DRIVEN DATA)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var settings = await GetOrCreateSettingsAsync();
            int lowStockThreshold = settings.LowStockThreshold;

            var validPaidOrdersQuery = _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled &&
                           (o.PaymentStatus == PaymentStatus.Paid || o.Status == OrderStatus.Delivered));

            decimal totalRevenue = await validPaidOrdersQuery.SumAsync(o => o.TotalAmount);
            int totalOrders = await _context.Orders.CountAsync();
            int totalCustomers = await _context.Users.CountAsync();
            int totalProducts = await _context.Products.CountAsync(p => p.Status == ProductStatus.Published);
            decimal totalRefunds = await _context.Orders.Where(o => o.Status == OrderStatus.Refunded).SumAsync(o => o.TotalAmount);
            int lowStockCount = await _context.Products.CountAsync(p => p.Stock <= lowStockThreshold && p.Status == ProductStatus.Published);
            decimal averageOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0m;

            var recentOrders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var lowStockProducts = await _context.Products
                .Where(p => p.Stock <= lowStockThreshold && p.Status == ProductStatus.Published)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            var recentAuditLogs = await _context.AdminAuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Real monthly aggregated revenue & order chart (Last 6 Months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var startOfMonth = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var ordersInPeriod = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth)
                .Select(o => new { o.OrderDate, o.TotalAmount, o.Status, o.PaymentStatus })
                .ToListAsync();

            var chartLabels = new List<string>();
            var chartRevenue = new List<decimal>();
            var chartOrders = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.UtcNow.AddMonths(-i);
                string monthLabel = targetMonth.ToString("MMM yyyy");
                chartLabels.Add(monthLabel);

                var monthOrders = ordersInPeriod.Where(o => o.OrderDate.Year == targetMonth.Year && o.OrderDate.Month == targetMonth.Month).ToList();
                decimal rev = monthOrders.Where(o => o.Status != OrderStatus.Cancelled && (o.PaymentStatus == PaymentStatus.Paid || o.Status == OrderStatus.Delivered)).Sum(o => o.TotalAmount);
                int count = monthOrders.Count;

                chartRevenue.Add(rev);
                chartOrders.Add(count);
            }

            var viewModel = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                TotalRefunds = totalRefunds,
                LowStockCount = lowStockCount,
                AverageOrderValue = averageOrderValue,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts,
                RecentAuditLogs = recentAuditLogs,
                ChartLabels = chartLabels,
                ChartRevenue = chartRevenue,
                ChartOrders = chartOrders
            };

            return View(viewModel);
        }

        // ==========================================
        // 2. ORDER MANAGEMENT & WORKFLOWS
        // ==========================================
        public async Task<IActionResult> Orders(string? search, OrderStatus? status, PaymentStatus? paymentStatus, int page = 1)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(o => o.OrderNumber.ToLower().Contains(s) ||
                                         o.TrackingNumber.ToLower().Contains(s) ||
                                         o.CustomerName.ToLower().Contains(s) ||
                                         o.CustomerEmail.ToLower().Contains(s));
            }

            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            if (paymentStatus.HasValue) query = query.Where(o => o.PaymentStatus == paymentStatus.Value);

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminOrderListViewModel
            {
                Orders = orders,
                SearchTerm = search,
                StatusFilter = status,
                PaymentStatusFilter = paymentStatus,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status, string? trackingNumber, string? courierMethod)
        {
            return CommerceDatabaseWork.TransactionAsync<IActionResult>(_context, () => UpdateOrderStatusCore(id, status, trackingNumber, courierMethod));
        }

        private async Task<IActionResult> UpdateOrderStatusCore(int id, OrderStatus status, string? trackingNumber, string? courierMethod)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var prevStatus = order.Status;
            if (!Enum.IsDefined(status)) return BadRequest("Invalid order status.");
            if (status == OrderStatus.Cancelled && prevStatus != OrderStatus.Cancelled)
                return await CancelOrderCore(id, "Cancelled through status update", true);
            if (status == OrderStatus.Refunded && order.PaymentStatus != PaymentStatus.Refunded)
            {
                TempData["ErrorMessage"] = "Record a verified payment refund before marking an order refunded.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }
            if (status != prevStatus && (prevStatus == OrderStatus.Refunded || (int)status < (int)prevStatus ||
                (status == OrderStatus.Refunded && prevStatus != OrderStatus.Delivered)))
            {
                TempData["ErrorMessage"] = "This order status transition is not allowed.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            // Reject invalid transitions
            if (prevStatus == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Cancelled orders cannot be transitioned back to active statuses.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (prevStatus == OrderStatus.Delivered && (status == OrderStatus.Pending || status == OrderStatus.Packed || status == OrderStatus.Shipped))
            {
                TempData["ErrorMessage"] = "Delivered orders cannot be reverted to pre-delivery statuses.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            order.Status = status;
            if (!string.IsNullOrWhiteSpace(trackingNumber)) order.TrackingNumber = trackingNumber.Trim();
            if (!string.IsNullOrWhiteSpace(courierMethod)) order.ShippingMethod = courierMethod.Trim();

            // Delivery is not evidence of a captured payment or COD remittance.
            if (status == OrderStatus.Delivered && !order.DeliveredAt.HasValue)
                order.DeliveredAt = DateTime.UtcNow;

            if ((status == OrderStatus.Cancelled || status == OrderStatus.Refunded) && (prevStatus != OrderStatus.Cancelled && prevStatus != OrderStatus.Refunded))
            {
                await _pricingService.RestoreCouponRedemptionAsync(order);
            }

            await LogAuditAsync("OrderStatusUpdated", "Order", order.OrderNumber, $"Order status changed from {prevStatus} to {status}. Courier: {order.ShippingMethod}");
            await _context.SaveChangesAsync();

            if (status != prevStatus)
            {
                _context.EmailOutboxMessages.Add(EmailOutboxService.CreateMessage(order.CustomerEmail,
                    $"Order #{order.OrderNumber} Update: {status}",
                    _emailTemplateService.GenerateOrderStatusUpdateEmail(order, prevStatus.ToString(), status.ToString()),
                    eventKey: $"order-status:{order.OrderNumber}:{status}"));
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} status successfully updated to {status}.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CancelOrder(int id, string reason, bool restoreInventory = true)
        {
            return CommerceDatabaseWork.TransactionAsync<IActionResult>(_context, () => CancelOrderCore(id, reason, restoreInventory));
        }

        private async Task<IActionResult> CancelOrderCore(int id, string reason, bool restoreInventory = true)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded or OrderStatus.Delivered or OrderStatus.Shipped or OrderStatus.OutForDelivery)
            {
                TempData["ErrorMessage"] = "This order cannot be cancelled in its current state.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            if (restoreInventory)
            {
                foreach (var item in order.Items)
                {
                    var prod = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    if (prod != null)
                    {
                        int prevStock = prod.Stock;
                        prod.Stock += item.Quantity;

                        if (item.VariantId.HasValue && item.VariantId.Value > 0)
                        {
                            var v = prod.Variants.FirstOrDefault(vr => vr.Id == item.VariantId.Value);
                            if (v != null) v.Stock += item.Quantity;
                        }

                        _context.InventoryMovements.Add(new InventoryMovement
                        {
                            ProductId = prod.Id,
                            VariantId = item.VariantId,
                            MovementType = InventoryMovementType.OrderCancellationRestoration,
                            QuantityChange = item.Quantity,
                            OldStock = prevStock,
                            NewStock = prod.Stock,
                            Reason = $"Admin cancelled Order #{order.OrderNumber}: {reason}",
                            OrderId = order.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.CustomerNotes = (order.CustomerNotes ?? "") + $" | Admin cancellation: {reason}";
            await _pricingService.RestoreCouponRedemptionAsync(order);

            await LogAuditAsync("OrderCancelled", "Order", order.OrderNumber, $"Cancelled order #{order.OrderNumber}. Stock restored: {restoreInventory}. Reason: {reason}");
            await _context.SaveChangesAsync();

            _context.EmailOutboxMessages.Add(EmailOutboxService.CreateMessage(order.CustomerEmail,
                $"Order #{order.OrderNumber} Cancellation Notice",
                _emailTemplateService.GenerateOrderCancellationEmail(order, reason),
                eventKey: $"order-cancelled:{order.OrderNumber}"));
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} cancelled successfully.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ExportOrdersCsv()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("OrderNumber,TrackingNumber,OrderDate,CustomerName,CustomerEmail,City,TotalAmount,Currency,PaymentStatus,Status,ItemCount");

            foreach (var o in orders)
            {
                sb.AppendLine($"{CsvEscape(o.OrderNumber)},{CsvEscape(o.TrackingNumber)},{o.OrderDate:yyyy-MM-dd HH:mm},{CsvEscape(o.CustomerName)},{CsvEscape(o.CustomerEmail)},{CsvEscape(o.City)},{o.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)},{o.Currency},{o.PaymentStatus},{o.Status},{o.Items.Sum(i => i.Quantity)}");
            }

            await LogAuditAsync("ExportCsv", "Order", "All", "Exported orders report CSV");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"Orders_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // ==========================================
        // 3. CUSTOMER MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Customers(string? search, int page = 1)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(u => u.FullName.ToLower().Contains(s) ||
                                         (u.Email != null && u.Email.ToLower().Contains(s)) ||
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
            }

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userEmails = users.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email!).ToList();
            var userIds = users.Select(u => u.Id).ToList();

            var orders = await _context.Orders
                .Where(o => (o.UserId != null && userIds.Contains(o.UserId)) || (o.CustomerEmail != null && userEmails.Contains(o.CustomerEmail)))
                .ToListAsync();

            var customerModels = users.Select(u => new CustomerAdminViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive,
                TotalOrders = orders.Count(o => o.UserId == u.Id || o.CustomerEmail == u.Email),
                TotalSpent = orders.Where(o => (o.UserId == u.Id || o.CustomerEmail == u.Email) && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount)
            }).ToList();

            var viewModel = new AdminCustomerListViewModel
            {
                Customers = customerModels,
                SearchTerm = search,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CustomerDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id || o.CustomerEmail == user.Email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Customer = user;
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCustomerStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            await LogAuditAsync("CustomerStatusToggled", "Customer", user.Email, $"User active status changed to: {user.IsActive}");
            TempData["SuccessMessage"] = $"Customer '{user.FullName}' status changed to {(user.IsActive ? "Active" : "Suspended")}.";

            return RedirectToAction(nameof(Customers));
        }

        // ==========================================
        // 4. PRODUCT & INVENTORY MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Products(string? search, int? categoryId, ProductStatus? status, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(p => p.Title.ToLower().Contains(s) ||
                                         p.SKU.ToLower().Contains(s) ||
                                         p.Brand.ToLower().Contains(s));
            }

            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
            if (status.HasValue) query = query.Where(p => p.Status == status.Value);

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.SearchTerm = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.StatusFilter = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var viewModel = new ProductCreateViewModel
            {
                Categories = await _context.Categories.OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                return View(model);
            }

            var category = await _context.Categories.FindAsync(model.CategoryId);
            var product = new Product
            {
                Title = model.Title.Trim(),
                Slug = GenerateSlug(model.Title),
                SKU = model.SKU.Trim().ToUpperInvariant(),
                Brand = model.Brand?.Trim() ?? string.Empty,
                CategoryId = model.CategoryId,
                CategoryName = category?.Name ?? "General",
                Price = model.Price,
                OldPrice = model.OldPrice,
                CostPrice = model.CostPrice,
                Stock = model.Stock,
                ShortDescription = model.ShortDescription?.Trim() ?? string.Empty,
                FullDescription = model.FullDescription?.Trim() ?? string.Empty,
                Status = model.Status,
                IsFeatured = model.IsFeatured,
                IsFlashDeal = model.IsFlashDeal,
                IsBestSeller = model.IsBestSeller,
                IsNewArrival = model.IsNewArrival,
                DeliveryEstimate = model.DeliveryEstimate?.Trim() ?? "2-4 Business Days",
                Warranty = model.Warranty?.Trim() ?? "Official Store Warranty",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Main image upload / fallback
            if (model.MainImageFile != null && model.MainImageFile.Length > 0)
            {
                product.MainImage = await SaveImageFileAsync(model.MainImageFile);
            }
            else if (!string.IsNullOrWhiteSpace(model.MainImageUrl))
            {
                product.MainImage = model.MainImageUrl.Trim();
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Initial stock inventory movement log
            _context.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                MovementType = InventoryMovementType.PurchaseRestock,
                QuantityChange = product.Stock,
                OldStock = 0,
                NewStock = product.Stock,
                Reason = "Initial catalog creation",
                CreatedAt = DateTime.UtcNow
            });

            await LogAuditAsync("ProductCreated", "Product", product.SKU, $"Created product '{product.Title}' with price {product.Price} and stock {product.Stock}");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product '{product.Title}' created successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var viewModel = new ProductEditViewModel
            {
                Id = product.Id,
                Title = product.Title,
                SKU = product.SKU,
                Brand = product.Brand,
                CategoryId = product.CategoryId,
                Price = product.Price,
                OldPrice = product.OldPrice,
                CostPrice = product.CostPrice,
                Stock = product.Stock,
                ExistingMainImageUrl = product.MainImage,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                Status = product.Status,
                IsFeatured = product.IsFeatured,
                IsFlashDeal = product.IsFlashDeal,
                IsBestSeller = product.IsBestSeller,
                IsNewArrival = product.IsNewArrival,
                DeliveryEstimate = product.DeliveryEstimate,
                Warranty = product.Warranty,
                RowVersion = product.RowVersion,
                ExistingImages = product.Images.ToList(),
                ExistingVariants = product.Variants.ToList(),
                Categories = await _context.Categories.OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                return View(model);
            }

            var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == model.Id);
            if (product == null) return NotFound();

            int oldStock = product.Stock;
            decimal oldPrice = product.Price;

            product.Title = model.Title.Trim();
            product.Slug = GenerateSlug(model.Title);
            product.SKU = model.SKU.Trim().ToUpperInvariant();
            product.Brand = model.Brand?.Trim() ?? string.Empty;
            product.CategoryId = model.CategoryId;
            var category = await _context.Categories.FindAsync(model.CategoryId);
            product.CategoryName = category?.Name ?? "General";
            product.Price = model.Price;
            product.OldPrice = model.OldPrice;
            product.CostPrice = model.CostPrice;
            product.Stock = model.Stock;
            product.ShortDescription = model.ShortDescription?.Trim() ?? string.Empty;
            product.FullDescription = model.FullDescription?.Trim() ?? string.Empty;
            product.Status = model.Status;
            product.IsFeatured = model.IsFeatured;
            product.IsFlashDeal = model.IsFlashDeal;
            product.IsBestSeller = model.IsBestSeller;
            product.IsNewArrival = model.IsNewArrival;
            product.DeliveryEstimate = model.DeliveryEstimate?.Trim() ?? "2-4 Business Days";
            product.Warranty = model.Warranty?.Trim() ?? "Official Store Warranty";
            product.UpdatedAt = DateTime.UtcNow;

            if (model.MainImageFile != null && model.MainImageFile.Length > 0)
            {
                product.MainImage = await SaveImageFileAsync(model.MainImageFile);
            }
            else if (!string.IsNullOrWhiteSpace(model.MainImageUrl))
            {
                product.MainImage = model.MainImageUrl.Trim();
            }

            if (oldStock != product.Stock)
            {
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = product.Id,
                    MovementType = InventoryMovementType.ManualAdjustment,
                    QuantityChange = product.Stock - oldStock,
                    OldStock = oldStock,
                    NewStock = product.Stock,
                    Reason = "Direct product stock edit by administrator",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await LogAuditAsync("ProductUpdated", "Product", product.SKU, $"Updated product '{product.Title}'. Price: {oldPrice}->{product.Price}, Stock: {oldStock}->{product.Stock}");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product '{product.Title}' updated successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.Status = ProductStatus.Archived;
                await LogAuditAsync("ProductArchived", "Product", product.SKU, $"Archived product #{product.Id} ({product.Title})");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.Title}' has been archived.";
            }

            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> ExportProductsCsv()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Id)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ID,SKU,Title,Category,Brand,Price,OldPrice,CostPrice,Stock,Status,Rating,ReviewCount");

            foreach (var p in products)
            {
                sb.AppendLine($"{p.Id},{CsvEscape(p.SKU)},{CsvEscape(p.Title)},{CsvEscape(p.CategoryName)},{CsvEscape(p.Brand)},{p.Price.ToString("F2", CultureInfo.InvariantCulture)},{p.OldPrice.ToString("F2", CultureInfo.InvariantCulture)},{p.CostPrice.ToString("F2", CultureInfo.InvariantCulture)},{p.Stock},{p.Status},{p.Rating},{p.ReviewCount}");
            }

            await LogAuditAsync("ExportCsv", "Product", "All", "Exported product catalog CSV");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"Products_Catalog_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // ==========================================
        // 4B. CATEGORY MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            // Refresh live product counts
            foreach (var cat in categories)
            {
                cat.ProductCount = await _context.Products.CountAsync(p => p.CategoryId == cat.Id && p.Status == ProductStatus.Published);
            }

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(string name, string? icon, string? description, int? id, int? parentCategoryId, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Category name is required.";
                return RedirectToAction(nameof(Categories));
            }

            name = name.Trim();
            icon = string.IsNullOrWhiteSpace(icon) ? "fa-box" : icon.Trim();
            description = description?.Trim() ?? string.Empty;

            if (id.HasValue && id.Value > 0)
            {
                var category = await _context.Categories.FindAsync(id.Value);
                if (category == null) return NotFound();

                category.Name = name;
                category.Icon = icon;
                category.Description = description;
                category.ParentCategoryId = parentCategoryId;
                category.DisplayOrder = displayOrder;
                category.IsActive = true;

                await LogAuditAsync("CategoryUpdated", "Category", category.Id.ToString(), $"Updated category '{category.Name}'");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' updated successfully.";
            }
            else
            {
                string baseSlug = GenerateSlug(name);
                string slug = baseSlug;
                int suffix = 1;
                while (await _context.Categories.AnyAsync(c => c.Slug == slug))
                {
                    slug = $"{baseSlug}-{suffix++}";
                }

                var category = new Category
                {
                    Name = name,
                    Slug = slug,
                    Icon = icon,
                    Description = description,
                    ParentCategoryId = parentCategoryId,
                    DisplayOrder = displayOrder,
                    IsActive = true,
                    IsFeatured = true
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                await LogAuditAsync("CategoryCreated", "Category", category.Id.ToString(), $"Created category '{category.Name}'");
                TempData["SuccessMessage"] = $"Category '{category.Name}' created successfully.";
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsActive = false;
                await LogAuditAsync("CategoryArchived", "Category", category.Id.ToString(), $"Archived category '{category.Name}'");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' has been archived.";
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Products).Include(c => c.SubCategories).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();

            if (category.Products.Any() || category.SubCategories.Any())
            {
                category.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = $"Category '{category.Name}' contains products or subcategories and cannot be deleted. It has been deactivated instead.";
            }
            else
            {
                _context.Categories.Remove(category);
                await LogAuditAsync("CategoryDeleted", "Category", category.Id.ToString(), $"Deleted category '{category.Name}'");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' deleted successfully.";
            }
            return RedirectToAction(nameof(Categories));
        }

        // ==========================================
        // 4C. INVENTORY & STOCK MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Inventory(string? search, int? categoryId, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(p => p.Title.ToLower().Contains(s) ||
                                         p.SKU.ToLower().Contains(s) ||
                                         p.Brand.ToLower().Contains(s));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.Title)
                .ToListAsync();

            var recentMovements = await _context.InventoryMovements
                .Include(m => m.Product)
                .Include(m => m.Variant)
                .OrderByDescending(m => m.CreatedAt)
                .Take(25)
                .ToListAsync();

            ViewBag.RecentMovements = recentMovements;
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(int productId, int? variantId, InventoryMovementType movementType, int quantityChange, string reason)
        {
            if (quantityChange == 0)
            {
                TempData["ErrorMessage"] = "Quantity change cannot be 0.";
                return RedirectToAction(nameof(Inventory));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "An audit reason is required for stock adjustment.";
                return RedirectToAction(nameof(Inventory));
            }

            var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            string adminName = user?.FullName ?? "Administrator";
            string adminId = user?.Id ?? "System";

            int oldStock;
            int newStock;

            if (variantId.HasValue && variantId.Value > 0)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == variantId.Value);
                if (variant == null) return NotFound("Variant not found.");

                oldStock = variant.Stock;
                newStock = oldStock + quantityChange;
                if (newStock < 0)
                {
                    TempData["ErrorMessage"] = $"Adjustment would result in negative variant stock ({newStock}). Minimum is 0.";
                    return RedirectToAction(nameof(Inventory));
                }

                variant.Stock = newStock;
                // Also synchronize base product stock sum from all variants
                product.Stock = product.Variants.Sum(v => v.Stock);

                _context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = product.Id,
                    VariantId = variant.Id,
                    MovementType = movementType,
                    QuantityChange = quantityChange,
                    OldStock = oldStock,
                    NewStock = newStock,
                    Reason = reason.Trim(),
                    AdminUserId = adminId,
                    AdminUserName = adminName,
                    CreatedAt = DateTime.UtcNow
                });

                await LogAuditAsync("StockAdjusted", "ProductVariant", variant.SKU, $"Adjusted stock for variant '{variant.Name}' of '{product.Title}' by {quantityChange} ({oldStock}->{newStock}). Reason: {reason}");
            }
            else
            {
                oldStock = product.Stock;
                newStock = oldStock + quantityChange;
                if (newStock < 0)
                {
                    TempData["ErrorMessage"] = $"Adjustment would result in negative stock ({newStock}). Minimum is 0.";
                    return RedirectToAction(nameof(Inventory));
                }

                product.Stock = newStock;

                _context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = product.Id,
                    MovementType = movementType,
                    QuantityChange = quantityChange,
                    OldStock = oldStock,
                    NewStock = newStock,
                    Reason = reason.Trim(),
                    AdminUserId = adminId,
                    AdminUserName = adminName,
                    CreatedAt = DateTime.UtcNow
                });

                await LogAuditAsync("StockAdjusted", "Product", product.SKU, $"Adjusted base stock for '{product.Title}' by {quantityChange} ({oldStock}->{newStock}). Reason: {reason}");
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Stock for '{product.Title}' successfully updated.";
            return RedirectToAction(nameof(Inventory));
        }

        // ==========================================
        // 5. COUPON MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons
                .Include(c => c.ApplicableCategory)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(coupons);
        }

        [HttpGet]
        public async Task<IActionResult> AddCoupon()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
            return View(new Coupon { StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(30) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCoupon(Coupon model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                return View(model);
            }

            model.Code = model.Code.Trim().ToUpperInvariant();
            _context.Coupons.Add(model);
            await LogAuditAsync("CouponCreated", "Coupon", model.Code, $"Created coupon code {model.Code} (Discount: {model.DiscountPercentage}% / Fixed: {model.FixedDiscountAmount})");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coupon code '{model.Code}' created successfully.";
            return RedirectToAction(nameof(Coupons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["ErrorMessage"] = "Coupon not found.";
                return RedirectToAction(nameof(Coupons));
            }

            bool hasRedemptions = await _context.CouponRedemptions.AnyAsync(r => r.CouponId == id || r.CouponCode == coupon.Code);
            bool hasOrders = await _context.Orders.AnyAsync(o => o.CouponCode == coupon.Code);

            if (hasRedemptions || hasOrders)
            {
                coupon.IsActive = false;
                coupon.IsArchived = true;
                await LogAuditAsync("CouponArchived", "Coupon", coupon.Code, $"Coupon {coupon.Code} has historical usage; deactivated and archived instead of hard deleted.");
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = $"Coupon '{coupon.Code}' has historical orders/redemptions and cannot be hard-deleted. It has been deactivated and archived instead.";
            }
            else
            {
                try
                {
                    _context.Coupons.Remove(coupon);
                    await LogAuditAsync("CouponDeleted", "Coupon", coupon.Code, $"Deleted unused coupon {coupon.Code}");
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' was safely removed.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete coupon {Code}", coupon.Code);
                    coupon.IsActive = false;
                    coupon.IsArchived = true;
                    await _context.SaveChangesAsync();
                    TempData["WarningMessage"] = $"Coupon '{coupon.Code}' could not be deleted due to database constraints and has been archived instead.";
                }
            }

            return RedirectToAction(nameof(Coupons));
        }

        // ==========================================
        // 6. REVIEW & QUESTION MODERATION
        // ==========================================
        public async Task<IActionResult> Reviews(bool? approved, int page = 1)
        {
            var query = _context.Reviews
                .Include(r => r.Product)
                .AsQueryable();

            if (approved.HasValue) query = query.Where(r => r.IsApproved == approved.Value);

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminReviewModerationViewModel
            {
                Reviews = reviews,
                ApprovedFilter = approved,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReviewApproval(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                await LogAuditAsync("ReviewModerated", "Review", review.Id.ToString(), $"Review approval set to {review.IsApproved}");
                await _context.SaveChangesAsync();

                // Recalculate rating & review count ONLY from approved reviews
                var product = await _context.Products.FindAsync(review.ProductId);
                if (product != null)
                {
                    var approvedReviews = await _context.Reviews
                        .Where(r => r.ProductId == review.ProductId && r.IsApproved)
                        .ToListAsync();
                    product.ReviewCount = approvedReviews.Count;
                    product.Rating = approvedReviews.Any() ? Math.Round(approvedReviews.Average(r => r.Rating), 1) : 0.0;
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Review status updated to {(review.IsApproved ? "Approved" : "Hidden")}.";
            }

            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                int productId = review.ProductId;
                _context.Reviews.Remove(review);
                await LogAuditAsync("ReviewDeleted", "Review", review.Id.ToString(), "Deleted abusive review");
                await _context.SaveChangesAsync();

                // Recalculate rating & review count ONLY from approved reviews
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    var approvedReviews = await _context.Reviews
                        .Where(r => r.ProductId == productId && r.IsApproved)
                        .ToListAsync();
                    product.ReviewCount = approvedReviews.Count;
                    product.Rating = approvedReviews.Any() ? Math.Round(approvedReviews.Average(r => r.Rating), 1) : 0.0;
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Review removed.";
            }

            return RedirectToAction(nameof(Reviews));
        }

        public async Task<IActionResult> Questions()
        {
            var questions = await _context.QuestionAnswers
                .Include(q => q.Product)
                .OrderByDescending(q => q.QuestionDate)
                .ToListAsync();

            return View(questions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerQuestion(int id, string answer)
        {
            var q = await _context.QuestionAnswers.FindAsync(id);
            if (q != null && !string.IsNullOrWhiteSpace(answer))
            {
                string cleanAnswer = answer.Trim();
                if (cleanAnswer.Length > 2000)
                {
                    TempData["ErrorMessage"] = "Answer cannot exceed 2,000 characters.";
                    return RedirectToAction(nameof(Questions));
                }

                var user = await _userManager.GetUserAsync(User);
                q.Answer = cleanAnswer;
                q.AnsweredBy = user?.FullName ?? "Store Administrator";
                q.AnswerDate = DateTime.UtcNow;
                q.IsAnswered = true;
                q.IsApproved = true;

                await LogAuditAsync("QuestionAnswered", "Question", q.Id.ToString(), $"Answered customer question #{q.Id}");
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Question answered and published.";
            }

            return RedirectToAction(nameof(Questions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleQuestionApproval(int id)
        {
            var q = await _context.QuestionAnswers.FindAsync(id);
            if (q != null)
            {
                q.IsApproved = !q.IsApproved;
                await LogAuditAsync("QuestionModerated", "Question", q.Id.ToString(), $"Question approval set to {q.IsApproved}");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Question status updated to {(q.IsApproved ? "Approved" : "Hidden")}.";
            }

            return RedirectToAction(nameof(Questions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var q = await _context.QuestionAnswers.FindAsync(id);
            if (q != null)
            {
                _context.QuestionAnswers.Remove(q);
                await LogAuditAsync("QuestionDeleted", "Question", q.Id.ToString(), "Deleted question");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Question removed.";
            }

            return RedirectToAction(nameof(Questions));
        }

        // ==========================================
        // 6B. RETURNS & REFUNDS GOVERNANCE
        // ==========================================
        public async Task<IActionResult> Returns(string? status, int page = 1)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .Where(o => o.ReturnRequestedAt != null || o.Status == OrderStatus.Refunded || o.RefundStatus != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o => o.RefundStatus == null || o.RefundStatus == "Pending" || o.RefundStatus == ReturnStatus.Requested || o.RefundStatus == ReturnStatus.Approved || o.RefundStatus == ReturnStatus.Inspected || o.RefundStatus == ReturnStatus.RefundPending);
                }
                else if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o => o.RefundStatus == ReturnStatus.Approved);
                }
                else if (status.Equals("RefundPending", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o => o.RefundStatus == ReturnStatus.RefundPending || o.RefundStatus == ReturnStatus.Inspected);
                }
                else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o => o.RefundStatus == ReturnStatus.Completed);
                }
                else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o => o.RefundStatus == ReturnStatus.Rejected);
                }
            }

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.ReturnRequestedAt ?? o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminReturnsViewModel
            {
                ReturnOrders = orders,
                StatusFilter = status,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(
            int orderId,
            string decision,
            string? inspectionState,
            bool restock,
            string? refundMethod,
            decimal? refundAmount,
            string? adminNotes,
            string? refundTransactionReference = null)
        {
            var user = await _userManager.GetUserAsync(User);
            string adminId = user?.Id ?? "System";
            string adminName = user?.FullName ?? user?.UserName ?? "Administrator";

            if (_returnRefundService != null)
            {
                ReturnOperationResult result;
                if (decision.Equals("Reject", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _returnRefundService.RejectReturnAsync(orderId, inspectionState, adminNotes, adminId, adminName);
                }
                else if (decision.Equals("Inspect", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _returnRefundService.InspectReturnAsync(orderId, inspectionState ?? ReturnInspectionState.PassedInspection, adminNotes, adminId, adminName);
                }
                else if (decision.Equals("CompleteRefund", StringComparison.OrdinalIgnoreCase) || decision.Equals("Complete", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _returnRefundService.CompleteRefundAsync(orderId, refundTransactionReference ?? string.Empty, refundAmount, refundMethod, restock, adminNotes, adminId, adminName);
                }
                else // "Approve"
                {
                    if (!string.IsNullOrWhiteSpace(refundTransactionReference) && refundTransactionReference.Trim().Length >= 4)
                    {
                        result = await _returnRefundService.CompleteRefundAsync(orderId, refundTransactionReference, refundAmount, refundMethod, restock, adminNotes, adminId, adminName);
                    }
                    else
                    {
                        result = await _returnRefundService.ApproveReturnAsync(orderId, adminNotes, adminId, adminName);
                        if (result.Success && !string.IsNullOrWhiteSpace(inspectionState) && !inspectionState.Equals(ReturnInspectionState.AwaitingInspection, StringComparison.OrdinalIgnoreCase))
                        {
                            result = await _returnRefundService.InspectReturnAsync(orderId, inspectionState, adminNotes, adminId, adminName);
                        }
                    }
                }

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                }
                else
                {
                    TempData["SuccessMessage"] = result.Message;
                }

                return RedirectToAction(nameof(Returns));
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            if (decision.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                order.ReturnInspectionState = string.IsNullOrWhiteSpace(inspectionState) ? ReturnInspectionState.RejectedInspection : inspectionState.Trim();
                order.ReturnAdminNotes = adminNotes?.Trim();
                order.RefundStatus = ReturnStatus.Rejected;
                order.ReturnProcessedAt = DateTime.UtcNow;

                if (_emailOutboxService != null)
                {
                    await _emailOutboxService.QueueEmailAsync(
                        order.CustomerEmail,
                        $"Return Request Update - Order #{order.OrderNumber}",
                        $"<p>Dear {order.CustomerName},</p><p>Your return request for order <strong>#{order.OrderNumber}</strong> has been reviewed and declined.</p><p>Reason: {order.ReturnAdminNotes}</p>",
                        eventKey: $"ReturnRejected_{order.OrderNumber}");
                }

                await LogAuditAsync("ReturnRejected", "Order", order.OrderNumber, $"Declined return for order #{order.OrderNumber}. Reason: {order.ReturnAdminNotes}");
                TempData["WarningMessage"] = $"Return request for order #{order.OrderNumber} has been rejected.";
            }
            else if (decision.Equals("Inspect", StringComparison.OrdinalIgnoreCase))
            {
                order.ReturnInspectionState = string.IsNullOrWhiteSpace(inspectionState) ? ReturnInspectionState.PassedInspection : inspectionState.Trim();
                order.ReturnAdminNotes = adminNotes?.Trim();
                order.RefundStatus = ReturnStatus.RefundPending;
                order.ReturnProcessedAt = DateTime.UtcNow;

                if (_emailOutboxService != null)
                {
                    await _emailOutboxService.QueueEmailAsync(
                        order.CustomerEmail,
                        $"Return Item Inspected - Order #{order.OrderNumber}",
                        $"<p>Dear {order.CustomerName},</p><p>Your return inspection has been recorded ({order.ReturnInspectionState}) for order <strong>#{order.OrderNumber}</strong>. Refund is now pending remittance.</p>",
                        eventKey: $"ReturnInspected_{order.OrderNumber}");
                }

                await LogAuditAsync("ReturnInspected", "Order", order.OrderNumber, $"Inspection recorded ({order.ReturnInspectionState}) for order #{order.OrderNumber}.");
                TempData["SuccessMessage"] = $"Return inspection recorded. Order #{order.OrderNumber} is pending refund remittance.";
            }
            else if (decision.Equals("CompleteRefund", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(refundTransactionReference))
            {
                if (string.IsNullOrWhiteSpace(refundTransactionReference) || refundTransactionReference.Trim().Length < 4)
                {
                    TempData["ErrorMessage"] = "A valid refund transaction reference is required to record a completed refund.";
                    return RedirectToAction(nameof(Returns));
                }

                string trimmedRef = refundTransactionReference.Trim();
                decimal alreadyRefunded = order.Payments.Where(p => p.Status == PaymentStatus.Refunded).Sum(p => p.Amount);
                decimal refundableAmount = order.TotalAmount - alreadyRefunded;
                decimal finalRefund = refundAmount ?? refundableAmount;

                if (finalRefund <= 0 || finalRefund > refundableAmount)
                {
                    TempData["ErrorMessage"] = $"Invalid refund amount. Must be > 0 and <= refundable remainder ({refundableAmount:N2}).";
                    return RedirectToAction(nameof(Returns));
                }

                order.ReturnInspectionState = string.IsNullOrWhiteSpace(inspectionState) ? ReturnInspectionState.PassedInspection : inspectionState.Trim();
                order.ReturnAdminNotes = adminNotes?.Trim();
                order.ReturnProcessedAt = DateTime.UtcNow;
                order.RefundMethod = string.IsNullOrWhiteSpace(refundMethod) ? order.PaymentMethod : refundMethod.Trim();
                order.RefundAmount = (order.RefundAmount ?? 0) + finalRefund;
                order.RefundTransactionReference = trimmedRef;
                order.RefundStatus = ReturnStatus.Completed;

                if (restock && !order.IsRestockedOnReturn)
                {
                    foreach (var item in order.Items)
                    {
                        var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            int oldStock = product.Stock;
                            product.Stock += item.Quantity;
                            int newStock = product.Stock;

                            if (item.VariantId.HasValue)
                            {
                                var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value);
                                if (variant != null) variant.Stock += item.Quantity;
                            }

                            _context.InventoryMovements.Add(new InventoryMovement
                            {
                                ProductId = product.Id,
                                VariantId = item.VariantId,
                                OrderId = order.Id,
                                MovementType = InventoryMovementType.ReturnRestoration,
                                QuantityChange = item.Quantity,
                                OldStock = oldStock,
                                NewStock = newStock,
                                Reason = $"Restocked from approved return of order #{order.OrderNumber}",
                                AdminUserId = adminId,
                                AdminUserName = adminName,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    order.IsRestockedOnReturn = true;
                }

                bool isFull = (alreadyRefunded + finalRefund) >= order.TotalAmount;
                if (isFull)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                    order.Status = OrderStatus.Refunded;
                    await _pricingService.RestoreCouponRedemptionAsync(order);
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.PartiallyRefunded;
                }

                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    OrderId = order.Id,
                    TransactionReference = trimmedRef,
                    Provider = order.RefundMethod,
                    PaymentMethod = order.PaymentMethod,
                    Amount = finalRefund,
                    Currency = order.Currency,
                    Status = PaymentStatus.Refunded,
                    CreatedAt = DateTime.UtcNow
                });

                if (_emailOutboxService != null)
                {
                    await _emailOutboxService.QueueEmailAsync(
                        order.CustomerEmail,
                        $"Return Approved & Refund Processed - Order #{order.OrderNumber}",
                        $"<p>Dear {order.CustomerName},</p><p>Your refund for order <strong>#{order.OrderNumber}</strong> has been processed.</p><p>Refund Amount: <strong>Rs. {finalRefund:N2}</strong> via {order.RefundMethod} (Ref: {trimmedRef}).</p>",
                        eventKey: $"RefundCompleted_{order.OrderNumber}_{trimmedRef}");
                }

                await LogAuditAsync("RefundCompleted", "Order", order.OrderNumber, $"Completed refund for order #{order.OrderNumber}. Refund: Rs. {finalRefund}, Ref: {trimmedRef}, Restocked: {restock}");
                TempData["SuccessMessage"] = $"Refund of Rs. {finalRefund:N2} recorded for order #{order.OrderNumber} (Ref: {trimmedRef}).";
            }
            else // Approve
            {
                order.ReturnInspectionState = string.IsNullOrWhiteSpace(inspectionState) ? ReturnInspectionState.AwaitingInspection : inspectionState.Trim();
                order.ReturnAdminNotes = adminNotes?.Trim();
                order.ReturnProcessedAt = DateTime.UtcNow;
                order.RefundStatus = ReturnStatus.Approved;

                if (_emailOutboxService != null)
                {
                    await _emailOutboxService.QueueEmailAsync(
                        order.CustomerEmail,
                        $"Return Request Approved - Order #{order.OrderNumber}",
                        $"<p>Dear {order.CustomerName},</p><p>Your return request for order <strong>#{order.OrderNumber}</strong> has been approved. Please dispatch items for condition inspection.</p>",
                        eventKey: $"ReturnApproved_{order.OrderNumber}");
                }

                await LogAuditAsync("ReturnApproved", "Order", order.OrderNumber, $"Approved return for order #{order.OrderNumber}. Awaiting inspection.");
                TempData["SuccessMessage"] = $"Return for order #{order.OrderNumber} approved and awaiting item inspection.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Returns));
        }

        // ==========================================
        // 6C. OPERATIONAL RECOVERY & OUTBOX HEALTH
        // ==========================================
        public async Task<IActionResult> OperationalRecovery()
        {
            var flaggedIdempotency = await _context.CheckoutIdempotencyRecords
                .Where(r => r.Status == IdempotencyStatus.RecoveryRequired ||
                           (r.Status == IdempotencyStatus.Failed && r.PaymentReference != null))
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .ToListAsync();

            var problematicEmails = await _context.EmailOutboxMessages
                .Where(e => e.Status == EmailOutboxStatus.Failed ||
                            e.Status == EmailOutboxStatus.Blocked ||
                           (e.Status == EmailOutboxStatus.Processing && e.LockExpiresAt < DateTime.UtcNow))
                .OrderByDescending(e => e.CreatedAt)
                .Take(50)
                .ToListAsync();

            var unresolvedOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending && o.PaymentStatus == PaymentStatus.Pending && o.OrderDate < DateTime.UtcNow.AddHours(-24))
                .OrderByDescending(o => o.OrderDate)
                .Take(50)
                .ToListAsync();

            var model = new AdminRecoveryViewModel
            {
                FlaggedIdempotencyRecords = flaggedIdempotency,
                ProblematicEmails = problematicEmails,
                UnresolvedOrders = unresolvedOrders
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryOutboxEmail(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _operationalRecoveryService.RetryOutboxEmailAsync(
                id,
                user?.Id ?? "System",
                user?.FullName ?? "Administrator");

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else if (result.AlreadySent)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["WarningMessage"] = result.Message;
            }

            return RedirectToAction(nameof(OperationalRecovery));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReconcilePayment(
            int id,
            string? outcome = null,
            string? action = null,
            string? adminNotes = null,
            string? providerReference = null,
            string? providerName = null)
        {
            // Rule: Do not mark a payment checkout record Completed merely because an administrator clicked a button.
            // If legacy action button was submitted without outcome, notes, and provider reference:
            if (string.IsNullOrWhiteSpace(outcome) && !string.IsNullOrWhiteSpace(action))
            {
                TempData["ErrorMessage"] = "Payment checkout records cannot be marked completed merely by clicking a button. An explicit reconciliation outcome (Paid, Failed, Cancelled, or Refunded), administrator notes, and provider/reference evidence are strictly required. Uncertain payments remain unresolved.";
                return RedirectToAction(nameof(OperationalRecovery));
            }

            var user = await _userManager.GetUserAsync(User);
            var request = new PaymentReconciliationRequest
            {
                Id = id,
                Outcome = outcome,
                Action = action,
                AdminNotes = adminNotes,
                ProviderReference = providerReference,
                ProviderName = providerName
            };

            var result = await _operationalRecoveryService.ReconcilePaymentAsync(
                request,
                user?.Id ?? "System",
                user?.FullName ?? "Administrator");

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(OperationalRecovery));
        }

        // ==========================================
        // 7. PERSISTENT STORE SETTINGS
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var settings = await GetOrCreateSettingsAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(StoreSetting model)
        {
            if (!ModelState.IsValid) return View(model);

            var setting = await _context.StoreSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = model;
                _context.StoreSettings.Add(setting);
            }
            else
            {
                setting.StoreName = model.StoreName.Trim();
                setting.StoreEmail = model.StoreEmail.Trim();
                setting.StorePhone = model.StorePhone.Trim();
                setting.StoreAddress = model.StoreAddress.Trim();
                setting.CurrencyCode = model.CurrencyCode.Trim().ToUpperInvariant();
                setting.CurrencySymbol = model.CurrencySymbol.Trim();
                setting.TaxRatePercent = model.TaxRatePercent;
                setting.FreeShippingThreshold = model.FreeShippingThreshold;
                setting.StandardShippingFee = model.StandardShippingFee;
                setting.ExpressShippingFee = model.ExpressShippingFee;
                setting.LowStockThreshold = model.LowStockThreshold;
                setting.EnableGuestCheckout = model.EnableGuestCheckout;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await LogAuditAsync("SettingsUpdated", "Settings", "Global", $"Store settings updated: Name={setting.StoreName}, Tax={setting.TaxRatePercent}%, Currency={setting.CurrencyCode}");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Store operational settings have been successfully updated in database.";
            return RedirectToAction(nameof(Settings));
        }

        // ==========================================
        // 8. AUDIT LOGS
        // ==========================================
        public async Task<IActionResult> AuditLogs(string? entityFilter, int page = 1)
        {
            var query = _context.AdminAuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityFilter))
            {
                query = query.Where(a => a.EntityType == entityFilter.Trim());
            }

            int pageSize = 15;
            int totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminAuditLogListViewModel
            {
                Logs = logs,
                EntityFilter = entityFilter,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private async Task<StoreSetting> GetOrCreateSettingsAsync()
        {
            var setting = await _context.StoreSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new StoreSetting
                {
                    StoreName = "Hamara Commerce",
                    StoreEmail = "support@hamaracommerce.pk",
                    StorePhone = "+92 300 1234567",
                    StoreAddress = "Plaza 45, Main Boulevard, Gulberg III, Lahore, Pakistan",
                    CurrencyCode = "PKR",
                    CurrencySymbol = "Rs. ",
                    TaxRatePercent = 5.0m,
                    FreeShippingThreshold = 5000.0m,
                    StandardShippingFee = 250.0m,
                    ExpressShippingFee = 500.0m,
                    LowStockThreshold = 5,
                    EnableGuestCheckout = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.StoreSettings.Add(setting);
                await _context.SaveChangesAsync();
            }
            return setting;
        }

        private async Task LogAuditAsync(string action, string entityType, string? entityId, string details)
        {
            var user = await _userManager.GetUserAsync(User);
            var log = new AdminAuditLog
            {
                AdminUserId = user?.Id ?? "System",
                AdminUserName = user?.FullName ?? "Administrator",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _context.AdminAuditLogs.Add(log);
        }

        private async Task<string> SaveImageFileAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return $"/uploads/products/{uniqueFileName}";
        }

        private static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        private static string CsvEscape(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "\"\"";
            // Prevent formula injection
            if (input.StartsWith("=") || input.StartsWith("+") || input.StartsWith("-") || input.StartsWith("@"))
            {
                input = "'" + input;
            }
            return $"\"{input.Replace("\"", "\"\"")}\"";
        }
    }
}
