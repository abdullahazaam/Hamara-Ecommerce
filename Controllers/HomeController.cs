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
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .Take(8)
                .ToListAsync(cancellationToken);

            var featuredProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsFeatured)
                .Take(8)
                .ToListAsync(cancellationToken);

            var trendingProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsTrending)
                .Take(8)
                .ToListAsync(cancellationToken);

            var newArrivals = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsNewArrival)
                .Take(8)
                .ToListAsync(cancellationToken);

            var bestSellers = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsBestSeller)
                .Take(8)
                .ToListAsync(cancellationToken);

            var flashDeals = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsFlashDeal)
                .Take(4)
                .ToListAsync(cancellationToken);

            ViewBag.Categories = categories;
            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.TrendingProducts = trendingProducts;
            ViewBag.NewArrivals = newArrivals;
            ViewBag.BestSellers = bestSellers;
            ViewBag.FlashDeals = flashDeals;

            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Hamara Commerce | Pakistan's Premier Online Marketplace",
                Description = "Discover authentic consumer electronics, fashion, lifestyle, and home goods with nationwide cash on delivery, 30-day easy returns, and guaranteed genuine quality.",
                Keywords = "online shopping pakistan, cash on delivery lahore, electronics karachi, buy verified gadgets, hamara commerce"
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
                Description = "Get in touch with Hamara Commerce's dedicated 24/7 customer support team for order assistance, warranty claims, and corporate inquiries."
            };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("ContactPolicy")]
        public async Task<IActionResult> SubmitContact(
            [FromForm] string name,
            [FromForm] string email,
            [FromForm] string? phoneNumber,
            [FromForm] string subject,
            [FromForm] string message,
            [FromForm] string? honeypot,
            CancellationToken cancellationToken = default)
        {
            // Honeypot spam bot check
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          Request.Headers["Accept"].ToString().Contains("application/json");

            if (!string.IsNullOrEmpty(honeypot))
            {
                _logger.LogWarning("Spam bot detected via contact honeypot field from IP: {Ip}", HttpContext.Connection.RemoteIpAddress);
                if (isAjax)
                {
                    return Json(new { success = true, message = "Thank you! Your inquiry has been received." });
                }
                TempData["SuccessMessage"] = "Thank you! Your inquiry has been received.";
                return RedirectToAction(nameof(Contact));
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "Please fill in all required fields (Name, Email, Subject, and Message)." });
                }
                TempData["ErrorMessage"] = "Please fill in all required fields (Name, Email, Subject, and Message).";
                return RedirectToAction(nameof(Contact));
            }

            var contactMessage = new ContactMessage
            {
                Name = name.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PhoneNumber = phoneNumber?.Trim(),
                Subject = subject.Trim(),
                Message = message.Trim(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ContactMessages.Add(contactMessage);
            var inquiryEmailBody = $"<p><strong>From:</strong> {System.Net.WebUtility.HtmlEncode(name)} ({System.Net.WebUtility.HtmlEncode(email)})</p><p><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(phoneNumber ?? "N/A")}</p><p><strong>Subject:</strong> {System.Net.WebUtility.HtmlEncode(subject)}</p><p><strong>Message:</strong><br />{System.Net.WebUtility.HtmlEncode(message)}</p>";
            var notificationSubject = $"[Support Desk] New Inquiry: {subject.Trim()}";
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
        public async Task<IActionResult> SubscribeNewsletter([FromForm] string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                TempData["ErrorMessage"] = "Please provide a valid email address to subscribe.";
                return RedirectToAction(nameof(Index));
            }

            var cleanEmail = email.Trim().ToLowerInvariant();
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
                Title = "30-Day Return & Refund Policy - Hamara Commerce",
                Description = "Details on our 30-day money-back guarantee, doorstep returns process, and rapid refund timelines."
            };
            return View();
        }

        [HttpGet]
        public IActionResult Faq()
        {
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = "Frequently Asked Questions (FAQs) - Hamara Commerce",
                Description = "Find instant answers to common questions about genuine warranty, cash on delivery, courier tracking, and order cancellations."
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
