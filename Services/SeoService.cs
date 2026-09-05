using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public interface ISeoService
    {
        Task<string> GenerateSitemapXmlAsync(string baseUrl, CancellationToken cancellationToken = default);
        string GenerateRobotsTxt(string baseUrl);
        string GenerateOrganizationJsonLd(string baseUrl);
        string GenerateWebSiteJsonLd(string baseUrl);
        string GenerateBreadcrumbJsonLd(string baseUrl, List<BreadcrumbItem> items);
        string GenerateProductJsonLd(string baseUrl, Product product);
    }

    public class SeoService : ISeoService
    {
        private readonly ApplicationDbContext _context;

        public SeoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateSitemapXmlAsync(string baseUrl, CancellationToken cancellationToken = default)
        {
            baseUrl = baseUrl.TrimEnd('/');
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            void AddUrl(string path, string changefreq, double priority, DateTime? lastmod = null)
            {
                var mod = (lastmod ?? DateTime.UtcNow).ToString("yyyy-MM-dd");
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{baseUrl}{path}</loc>");
                sb.AppendLine($"    <lastmod>{mod}</lastmod>");
                sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
                sb.AppendLine($"    <priority>{priority:0.0}</priority>");
                sb.AppendLine("  </url>");
            }

            // Static Pages
            AddUrl("/", "daily", 1.0);
            AddUrl("/Shop", "daily", 0.9);
            AddUrl("/Home/About", "monthly", 0.7);
            AddUrl("/Home/Contact", "monthly", 0.7);
            AddUrl("/Home/Privacy", "monthly", 0.5);
            AddUrl("/Home/Terms", "monthly", 0.5);
            AddUrl("/Home/ShippingPolicy", "monthly", 0.5);
            AddUrl("/Home/ReturnPolicy", "monthly", 0.5);
            AddUrl("/Home/Faq", "weekly", 0.6);

            // Categories
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var cat in categories)
            {
                AddUrl($"/Shop?category={cat.Slug}", "weekly", 0.8);
            }

            // Products
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published)
                .Select(p => new { p.Id, p.Slug, p.UpdatedAt })
                .ToListAsync(cancellationToken);

            foreach (var p in products)
            {
                AddUrl($"/Shop/Details/{p.Id}", "weekly", 0.8, p.UpdatedAt);
            }

            sb.AppendLine("</urlset>");
            return sb.ToString();
        }

        public string GenerateRobotsTxt(string baseUrl)
        {
            baseUrl = baseUrl.TrimEnd('/');
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow: /Admin/");
            sb.AppendLine("Disallow: /admin/");
            sb.AppendLine("Disallow: /Account/");
            sb.AppendLine("Disallow: /account/");
            sb.AppendLine("Disallow: /Cart/");
            sb.AppendLine("Disallow: /cart/");
            sb.AppendLine("Disallow: /Checkout/");
            sb.AppendLine("Disallow: /checkout/");
            sb.AppendLine("Disallow: /*?search=");
            sb.AppendLine("Disallow: /*&search=");
            sb.AppendLine("Allow: /");
            sb.AppendLine();
            sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
            return sb.ToString();
        }

        public string GenerateOrganizationJsonLd(string baseUrl)
        {
            baseUrl = baseUrl.TrimEnd('/');
            var org = new
            {
                context = "https://schema.org",
                type = "Organization",
                name = "Hamara Commerce",
                url = baseUrl,
                logo = $"{baseUrl}/images/logo.png",
                contactPoint = new
                {
                    type = "ContactPoint",
                    telephone = "+92-300-1234567",
                    contactType = "customer service",
                    areaServed = "PK",
                    availableLanguage = new[] { "English", "Urdu" }
                },
                sameAs = new[]
                {
                    "https://facebook.com",
                    "https://instagram.com",
                    "https://twitter.com",
                    "https://linkedin.com"
                }
            };

            return $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(org).Replace("context", "@context").Replace("\"type\"", "\"@type\"")}</script>";
        }

        public string GenerateWebSiteJsonLd(string baseUrl)
        {
            baseUrl = baseUrl.TrimEnd('/');
            var site = new
            {
                context = "https://schema.org",
                type = "WebSite",
                name = "Hamara Commerce",
                url = baseUrl,
                potentialAction = new
                {
                    type = "SearchAction",
                    target = $"{baseUrl}/Shop?search={{search_term_string}}",
                    query_input = "required name=search_term_string"
                }
            };

            return $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(site).Replace("context", "@context").Replace("\"type\"", "\"@type\"").Replace("query_input", "query-input")}</script>";
        }

        public string GenerateBreadcrumbJsonLd(string baseUrl, List<BreadcrumbItem> items)
        {
            baseUrl = baseUrl.TrimEnd('/');
            var elements = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                elements.Add(new
                {
                    type = "ListItem",
                    position = i + 1,
                    name = item.Title,
                    item = !string.IsNullOrEmpty(item.Url) ? (item.Url.StartsWith("http") ? item.Url : $"{baseUrl}{item.Url}") : $"{baseUrl}/"
                });
            }

            var breadcrumb = new
            {
                context = "https://schema.org",
                type = "BreadcrumbList",
                itemListElement = elements
            };

            return $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(breadcrumb).Replace("context", "@context").Replace("\"type\"", "\"@type\"")}</script>";
        }

        public string GenerateProductJsonLd(string baseUrl, Product product)
        {
            baseUrl = baseUrl.TrimEnd('/');
            var approvedReviews = product.Reviews?.Where(r => r.IsApproved).ToList() ?? new();
            object? aggregateRating = null;

            if (approvedReviews.Count > 0)
            {
                aggregateRating = new
                {
                    type = "AggregateRating",
                    ratingValue = Math.Round(approvedReviews.Average(r => r.Rating), 1),
                    reviewCount = approvedReviews.Count,
                    bestRating = "5",
                    worstRating = "1"
                };
            }

            var offers = new
            {
                type = "Offer",
                price = product.Price.ToString("0.00"),
                priceCurrency = "PKR",
                availability = product.Stock > 0 ? "https://schema.org/InStock" : "https://schema.org/OutOfStock",
                itemCondition = "https://schema.org/NewCondition",
                url = $"{baseUrl}/Shop/Details/{product.Id}",
                priceValidUntil = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd")
            };

            var prod = new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Product",
                ["name"] = product.Title,
                ["image"] = product.MainImage,
                ["description"] = product.ShortDescription,
                ["sku"] = product.SKU,
                ["brand"] = new { @type = "Brand", name = string.IsNullOrEmpty(product.Brand) ? "Hamara Commerce" : product.Brand },
                ["offers"] = offers
            };

            if (aggregateRating != null)
            {
                prod["aggregateRating"] = aggregateRating;
            }

            return $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(prod).Replace("\"type\"", "\"@type\"")}</script>";
        }
    }
}
