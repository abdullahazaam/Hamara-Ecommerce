using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISeoService _seoService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailOutboxService? _emailOutboxService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            ISeoService seoService,
            IEmailSender emailSender,
            ILogger<HomeController> logger,
            IEmailOutboxService? emailOutboxService = null)
        {
            _context = context;
            _seoService = seoService;
            _emailSender = emailSender;
            _logger = logger;
            _emailOutboxService = emailOutboxService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            // 1. Popular Categories (15 Major Marketplace Departments)
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync(cancellationToken);

            var publishedProductCount = await _context.Products
                .CountAsync(p => p.Status == ProductStatus.Published, cancellationToken);
            var totalCategoriesCount = categories.Count;

            // 2. Everyday Low Prices (< PKR 3,500)
            var everydayLowPrices = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && p.Price < 3500)
                .OrderBy(p => p.StorefrontRank.HasValue ? p.StorefrontRank.Value : 999999)
                .ThenBy(p => p.Price)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 3. Budget Deals (discounted items)
            var budgetDeals = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && (p.OldPrice > p.Price || p.IsFlashDeal))
                .OrderByDescending(p => p.DiscountPercentage)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 4. Mobile Accessories
            var mobileAccessories = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && (p.CategoryName == "Mobile Accessories" || p.Category!.Slug == "mobile-accessories"))
                .OrderBy(p => p.Price)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 5. Beauty & Personal Care
            var beautyPersonalCare = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && (p.CategoryName == "Beauty & Personal Care" || p.CategoryName == "Health & Wellness" || p.Category!.Slug == "beauty-personal-care"))
                .OrderBy(p => p.Price)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 6. Home Essentials
            var homeEssentials = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && (p.CategoryName == "Home & Living" || p.CategoryName == "Kitchen Appliances" || p.CategoryName == "Grocery & Beverages"))
                .OrderBy(p => p.Price)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 7. Fashion Picks
            var fashionPicks = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && (p.CategoryName == "Men's Fashion" || p.CategoryName == "Women's Fashion" || p.CategoryName == "Shoes & Footwear"))
                .OrderBy(p => p.Price)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 8. Electronics
            var electronics = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && (p.CategoryName == "Mobile Phones" || p.CategoryName == "Laptops & Computers" || p.CategoryName == "TVs & Entertainment"))
                .OrderBy(p => p.Price)
                .Take(4)
                .ToListAsync(cancellationToken);

            // 9. New Arrivals
            var newArrivals = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync(cancellationToken);

            var heroProduct = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published && p.StorefrontRank == 4) // Anker Charger or Flagship
                .FirstOrDefaultAsync(cancellationToken)
                ?? await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Status == ProductStatus.Published, cancellationToken);

            ViewBag.Categories = categories;
            ViewBag.EverydayLowPrices = everydayLowPrices;
            ViewBag.BudgetDeals = budgetDeals;
            ViewBag.MobileAccessories = mobileAccessories;
            ViewBag.BeautyPersonalCare = beautyPersonalCare;
            ViewBag.HomeEssentials = homeEssentials;
            ViewBag.FashionPicks = fashionPicks;
            ViewBag.Electronics = electronics;
            ViewBag.NewArrivals = newArrivals;
            ViewBag.HeroProduct = heroProduct;
            ViewBag.PublishedProductCount = publishedProductCount;
            ViewBag.TotalCategoriesCount = totalCategoriesCount;

            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Hamara Commerce | Online Shopping in Pakistan",
                Description = "Discover consumer electronics, fashion, lifestyle, and home goods with nationwide cash on delivery and easy returns.",
                Keywords = "online shopping pakistan, cash on delivery lahore, electronics karachi, gadgets, hamara commerce"
            };

            return View();
        }

        [HttpGet("sitemap.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Sitemap(CancellationToken cancellationToken = default)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var sitemapXml = await _seoService.GenerateSitemapXmlAsync(baseUrl, cancellationToken);
            return Content(sitemapXml, "application/xml; charset=utf-8");
        }

        [HttpGet("robots.txt")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public IActionResult Robots()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var robotsTxt = _seoService.GenerateRobotsTxt(baseUrl);
            return Content(robotsTxt, "text/plain; charset=utf-8");
        }

        [HttpGet]
        public IActionResult About()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "About Us - Hamara Commerce",
                Description = "Learn about Hamara Commerce's mission, authentic sourcing, nationwide logistics, and customer protection commitment across Pakistan."
            };
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Contact Support & Help Desk - Hamara Commerce",
                Description = "Get in touch with Hamara Commerce's customer support team for order assistance, warranty claims, and corporate inquiries."
            };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("ContactPolicy")]
        public async Task<IActionResult> SubmitContact(
            [FromForm] ContactFormViewModel model,
            CancellationToken cancellationToken = default)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          Request.Headers["Accept"].ToString().Contains("application/json");

            // Honeypot spam bot check
            if (!string.IsNullOrEmpty(model.Honeypot))
            {
                _logger.LogWarning("Spam bot detected via contact honeypot field from IP: {Ip}", HttpContext.Connection.RemoteIpAddress);
                if (isAjax)
                {
                    return Json(new { success = true, message = "Thank you! Your inquiry has been received." });
                }
                TempData["SuccessMessage"] = "Thank you! Your inquiry has been received.";
                return RedirectToAction(nameof(Contact));
            }

            if (!ModelState.IsValid)
            {
                var errorMsg = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                if (isAjax)
                {
                    return Json(new { success = false, message = errorMsg });
                }
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Contact));
            }

            var contactMessage = new ContactMessage
            {
                Name = model.Name.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                Subject = model.Subject.Trim(),
                Message = model.Message.Trim(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ContactMessages.Add(contactMessage);
            var inquiryEmailBody = $"<p><strong>From:</strong> {System.Net.WebUtility.HtmlEncode(model.Name)} ({System.Net.WebUtility.HtmlEncode(model.Email)})</p><p><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(model.PhoneNumber ?? "N/A")}</p><p><strong>Subject:</strong> {System.Net.WebUtility.HtmlEncode(model.Subject)}</p><p><strong>Message:</strong><br />{System.Net.WebUtility.HtmlEncode(model.Message)}</p>";
            var notificationSubject = $"[Support Desk] New Inquiry: {model.Subject.Trim()}";
            if (notificationSubject.Length > 255) notificationSubject = notificationSubject[..255];
            _context.EmailOutboxMessages.Add(EmailOutboxService.CreateMessage("support@hamaracommerce.pk",
                notificationSubject, inquiryEmailBody, eventKey: $"contact-form:{Guid.NewGuid():N}"));
            await _context.SaveChangesAsync(cancellationToken);

            if (isAjax)
            {
                return Json(new { success = true, message = "Thank you for contacting Hamara Commerce! Our customer care team has received your message and will respond within 24 hours." });
            }

            TempData["SuccessMessage"] = "Thank you for contacting Hamara Commerce! Our customer care team has received your message and will respond within 24 hours.";
            return RedirectToAction(nameof(Contact));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeNewsletter([FromForm] NewsletterSubscriptionViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errorMsg = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            var cleanEmail = model.Email.Trim().ToLowerInvariant();
            var existing = await _context.NewsletterSubscriptions
                .FirstOrDefaultAsync(n => n.Email == cleanEmail, cancellationToken);

            if (existing != null)
            {
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    existing.UnsubscribedAt = null;
                    await _context.SaveChangesAsync(cancellationToken);
                    TempData["SuccessMessage"] = "Welcome back! Your subscription has been reactivated.";
                }
                else
                {
                    TempData["InfoMessage"] = "You are already subscribed to Hamara Club updates.";
                }
                return RedirectToAction(nameof(Index));
            }

            var subscription = new NewsletterSubscription
            {
                Email = cleanEmail,
                SubscribedAt = DateTime.UtcNow,
                IsActive = true,
                ConsentGiven = true,
                UnsubscribeToken = Guid.NewGuid().ToString("N"),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.NewsletterSubscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New newsletter subscriber: {Email}", cleanEmail);
            TempData["SuccessMessage"] = "Thank you for subscribing to Hamara Club! You'll receive secret voucher codes and early sale alerts.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Unsubscribe(string? token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Success = false;
                ViewBag.Message = "No unsubscribe token provided.";
                return View();
            }

            var sub = await _context.NewsletterSubscriptions
                .FirstOrDefaultAsync(n => n.UnsubscribeToken == token, cancellationToken);

            if (sub == null)
            {
                ViewBag.Success = false;
                ViewBag.Message = "The unsubscribe link is invalid or expired.";
                return View();
            }

            sub.IsActive = false;
            sub.UnsubscribedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            ViewBag.Success = true;
            ViewBag.Message = $"You have been successfully unsubscribed ({sub.Email}).";
            return View();
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Privacy Policy - Hamara Commerce",
                Description = "Learn how Hamara Commerce collects, stores, protects, and handles customer data with 256-bit encryption and strict non-disclosure."
            };
            return View();
        }

        [HttpGet]
        public IActionResult Terms()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Terms and Conditions - Hamara Commerce",
                Description = "Read the official customer service terms, ordering conditions, and dispute resolution policies of Hamara Commerce."
            };
            return View();
        }

        [HttpGet]
        public IActionResult ShippingPolicy()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Shipping & Delivery Policy - Hamara Commerce",
                Description = "Information on nationwide delivery timeframes, free shipping thresholds, courier partners, and tracking."
            };
            return View();
        }

        [HttpGet]
        public IActionResult ReturnPolicy()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Return & Refund Policy - Hamara Commerce",
                Description = "Details on our returns policy, doorstep pickup process, and refund procedure."
            };
            return View();
        }

        [HttpGet]
        public IActionResult Faq()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Frequently Asked Questions (FAQs) - Hamara Commerce",
                Description = "Find answers to common questions about product warranty, cash on delivery, courier tracking, and order cancellations."
            };
            return View();
        }

        [HttpGet]
        public IActionResult NotFoundPage(int? id)
        {
            int statusCode = id ?? 404;
            Response.StatusCode = statusCode;
            ViewBag.StatusCode = statusCode;
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = statusCode == 404 ? "Page Not Found (404) - Hamara Commerce" : $"Error ({statusCode}) - Hamara Commerce",
                Robots = "noindex, nofollow"
            };
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "System Error - Hamara Commerce",
                Robots = "noindex, nofollow"
            };
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
