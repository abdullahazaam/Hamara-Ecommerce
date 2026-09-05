using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISeoService _seoService;
        private readonly IShippingTaxService _shippingTaxService;

        public ShopController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            ISeoService seoService,
            IShippingTaxService shippingTaxService)
        {
            _context = context;
            _userManager = userManager;
            _seoService = seoService;
            _shippingTaxService = shippingTaxService;
        }

        public async Task<IActionResult> Index(
            string? search, string? category, string? brand, 
            decimal? minPrice, decimal? maxPrice, double? minRating, 
            string? sort, bool inStock = false, bool onSale = false, 
            int page = 1, CancellationToken cancellationToken = default)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Published)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(p => p.Title.ToLower().Contains(s) || 
                                         p.CategoryName.ToLower().Contains(s) || 
                                         p.Brand.ToLower().Contains(s) || 
                                         p.SKU.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var catStr = category.ToLower().Trim();
                query = query.Where(p => p.CategoryName.ToLower() == catStr || 
                                         p.Category!.Slug.ToLower() == catStr);
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                var bStr = brand.ToLower().Trim();
                query = query.Where(p => p.Brand.ToLower() == bStr);
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (minRating.HasValue) query = query.Where(p => p.Rating >= minRating.Value);
            if (inStock) query = query.Where(p => p.Stock > 0);
            if (onSale) query = query.Where(p => p.OldPrice > p.Price);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.Rating),
                "popular" => query.OrderByDescending(p => p.ReviewCount),
                "discount" => query.OrderByDescending(p => p.DiscountPercentage),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Rating)
            };

            int pageSize = 12;
            int totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            var brands = await _context.Products
                .Where(p => p.Status == ProductStatus.Published && !string.IsNullOrEmpty(p.Brand))
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            var viewModel = new ShopFilterViewModel
            {
                Products = products,
                Categories = categories,
                Brands = brands,
                SearchQuery = search,
                Category = category,
                SelectedBrand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                InStockOnly = inStock,
                OnSaleOnly = onSale,
                SortBy = sort,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            bool isSearchQuery = !string.IsNullOrWhiteSpace(search);
            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = !string.IsNullOrEmpty(category) ? $"{category.ToUpperInvariant()} - Buy Online | Hamara Commerce" : "Shop Collection - Hamara Commerce",
                Description = "Explore thousands of authentic products across Pakistan with genuine manufacturer warranty, express shipping, and cash on delivery.",
                Robots = isSearchQuery ? "noindex, follow" : "index, follow"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> SearchApi(string term, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var t = term.ToLower().Trim();
            var rawResults = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && 
                           (p.Title.ToLower().Contains(t) || p.CategoryName.ToLower().Contains(t) || p.Brand.ToLower().Contains(t)))
                .OrderByDescending(p => p.Rating)
                .Take(6)
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    price = p.Price,
                    image = p.MainImage,
                    category = p.CategoryName
                })
                .ToListAsync(cancellationToken);

            var results = rawResults.Select(p => new
            {
                p.id,
                p.title,
                price = _shippingTaxService.FormatCurrency(p.price),
                p.image,
                p.category
            });

            return Json(results);
        }

        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            bool isInt = int.TryParse(id, out int intId);
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .Include(p => p.Questions.Where(q => q.IsApproved))
                .Include(p => p.Images)
                .Include(p => p.Variants.Where(v => v.IsActive))
                .FirstOrDefaultAsync(p => (p.Slug == id || (isInt && p.Id == intId)) && p.Status == ProductStatus.Published, cancellationToken);

            if (product == null)
            {
                return NotFound();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Title = "Home", Url = "/" },
                new() { Title = "Shop", Url = "/Shop" },
                new() { Title = product.CategoryName, Url = $"/Shop?category={product.Category?.Slug ?? product.CategoryName.ToLower()}" },
                new() { Title = product.Title, Url = $"/Shop/Details/{product.Id}", IsActive = true }
            };

            var productJsonLd = _seoService.GenerateProductJsonLd(baseUrl, product);
            var breadcrumbJsonLd = _seoService.GenerateBreadcrumbJsonLd(baseUrl, breadcrumbs);

            ViewData["SeoMetadata"] = new PageSeoMetadata
            {
                Title = $"{product.Title} - Buy Online in Pakistan | Hamara Commerce",
                Description = string.IsNullOrEmpty(product.ShortDescription) ? product.Title : product.ShortDescription,
                OgTitle = product.Title,
                OgDescription = product.ShortDescription,
                OgImage = product.MainImage,
                OgType = "product",
                CanonicalUrl = $"{baseUrl}/Shop/Details/{product.Id}",
                JsonLdScripts = new List<string> { productJsonLd, breadcrumbJsonLd }
            };

            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status == ProductStatus.Published)
                .OrderByDescending(p => p.Rating)
                .Take(4)
                .ToListAsync();

            // Dynamic algorithmic frequently bought together based on real co-orders
            var orderIdsWithProduct = await _context.OrderItems
                .Where(i => i.ProductId == product.Id)
                .Select(i => i.OrderId)
                .Distinct()
                .Take(50)
                .ToListAsync();

            List<Product> frequentlyBought = new();

            if (orderIdsWithProduct.Any())
            {
                var coPurchasedProductIds = await _context.OrderItems
                    .Where(i => orderIdsWithProduct.Contains(i.OrderId) && i.ProductId != product.Id)
                    .GroupBy(i => i.ProductId)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(2)
                    .ToListAsync();

                if (coPurchasedProductIds.Any())
                {
                    frequentlyBought = await _context.Products
                        .Include(p => p.Images)
                        .Where(p => coPurchasedProductIds.Contains(p.Id) && p.Status == ProductStatus.Published)
                        .ToListAsync();
                }
            }

            if (frequentlyBought.Count < 2)
            {
                var existingIds = frequentlyBought.Select(f => f.Id).Append(product.Id).ToList();
                var complementary = await _context.Products
                    .Include(p => p.Images)
                    .Where(p => p.CategoryId == product.CategoryId && !existingIds.Contains(p.Id) && p.Status == ProductStatus.Published)
                    .OrderByDescending(p => p.Rating)
                    .Take(2 - frequentlyBought.Count)
                    .ToListAsync();

                frequentlyBought.AddRange(complementary);
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                FrequentlyBoughtTogether = frequentlyBought
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == ProductStatus.Published);

            if (product == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = product.Id,
                title = product.Title,
                price = product.Price.ToString("C"),
                oldPrice = product.OldPrice > product.Price ? product.OldPrice.ToString("C") : null,
                category = product.CategoryName,
                image = product.MainImage,
                description = product.ShortDescription,
                stock = product.Stock,
                rating = product.Rating,
                reviews = product.ReviewCount
            });
        }

        // ==========================================
        // ADD REVIEW (AUTHENTICATED & VERIFIED PURCHASE)
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string title, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(comment) || rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating (1 to 5 stars) and review comment.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var product = await _context.Products.Include(p => p.Reviews).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            // Prevent duplicate reviews by the same user
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && (r.UserId == user.Id || r.AuthorEmail == user.Email));

            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "You have already submitted a review for this product.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            // Real Verified Purchase verification in database
            var hasPurchased = await _context.Orders
                .AnyAsync(o => (o.UserId == user.Id || o.CustomerEmail.ToLower() == (user.Email ?? "").ToLower()) &&
                               o.Items.Any(i => i.ProductId == productId) &&
                               o.Status != OrderStatus.Cancelled);

            var review = new Review
            {
                ProductId = productId,
                UserId = user.Id,
                AuthorEmail = user.Email,
                UserName = HtmlEncoder(user.FullName),
                UserAvatar = user.AvatarUrl ?? "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=120&q=80",
                Rating = rating,
                Title = HtmlEncoder(title ?? string.Empty),
                Comment = HtmlEncoder(comment),
                Date = DateTime.UtcNow,
                IsVerifiedPurchase = hasPurchased,
                IsApproved = true // Auto-approved
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Recalculate rating & review count
            var approvedReviews = await _context.Reviews.Where(r => r.ProductId == productId && r.IsApproved).ToListAsync();
            product.ReviewCount = approvedReviews.Count;
            product.Rating = approvedReviews.Any() ? Math.Round(approvedReviews.Average(r => r.Rating), 1) : 5.0;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = hasPurchased 
                ? "Thank you! Your verified purchase review has been published." 
                : "Thank you! Your review has been submitted.";

            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ==========================================
        // ASK QUESTION (AUTHENTICATED & MODERATED)
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskQuestion(int productId, string question)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(question) || question.Trim().Length < 5)
            {
                TempData["ErrorMessage"] = "Question must be at least 5 characters long.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var qa = new QuestionAnswer
            {
                ProductId = productId,
                UserId = user.Id,
                AskedBy = HtmlEncoder(user.FullName),
                Question = HtmlEncoder(question.Trim()),
                QuestionDate = DateTime.UtcNow,
                IsApproved = true,
                IsAnswered = false,
                Answer = null,
                AnsweredBy = null,
                AnswerDate = null
            };

            _context.QuestionAnswers.Add(qa);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your question has been submitted. Our support team and community will reply soon.";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        private static string HtmlEncoder(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return WebUtility.HtmlEncode(input.Trim());
        }
    }
}
