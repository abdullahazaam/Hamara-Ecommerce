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
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IShippingTaxService shippingTaxService,
            IPricingService pricingService,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _shippingTaxService = shippingTaxService;
            _pricingService = pricingService;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
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
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status, string? trackingNumber, string? courierMethod)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var prevStatus = order.Status;

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

            // Auto-mark Paid on Delivered if COD
            if (status == OrderStatus.Delivered && order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatus = PaymentStatus.Paid;
            }

            if ((status == OrderStatus.Cancelled || status == OrderStatus.Refunded) && (prevStatus != OrderStatus.Cancelled && prevStatus != OrderStatus.Refunded))
            {
                await _pricingService.RestoreCouponRedemptionAsync(order);
            }

            await LogAuditAsync("OrderStatusUpdated", "Order", order.OrderNumber, $"Order status changed from {prevStatus} to {status}. Courier: {order.ShippingMethod}");
            await _context.SaveChangesAsync();

            // Dispatch customer status update notification
            try
            {
                var emailBody = _emailTemplateService.GenerateOrderStatusUpdateEmail(order, prevStatus.ToString(), status.ToString());
                _ = _emailSender.SendEmailAsync(order.CustomerEmail, $"Order #{order.OrderNumber} Update: {status}", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send status update email to {Email}", order.CustomerEmail);
            }

            TempData["SuccessMessage"] = $"Order #{order.OrderNumber} status successfully updated to {status}.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id, string reason, bool restoreInventory = true)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "This order is already cancelled.";
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

            // Dispatch customer cancellation email
            try
            {
                var emailBody = _emailTemplateService.GenerateOrderCancellationEmail(order, reason);
                _ = _emailSender.SendEmailAsync(order.CustomerEmail, $"Order #{order.OrderNumber} Cancellation Notice", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send cancellation email to {Email}", order.CustomerEmail);
            }

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
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await LogAuditAsync("CouponDeleted", "Coupon", coupon.Code, $"Deleted coupon {coupon.Code}");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coupon removed.";
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
                _context.Reviews.Remove(review);
                await LogAuditAsync("ReviewDeleted", "Review", review.Id.ToString(), "Deleted abusive review");
                await _context.SaveChangesAsync();
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
                var user = await _userManager.GetUserAsync(User);
                q.Answer = answer.Trim();
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
