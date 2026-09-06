using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Data.Catalog;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class CatalogueImporter : ICatalogueImporter
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CatalogueImporter> _logger;

        public CatalogueImporter(ApplicationDbContext context, ILogger<CatalogueImporter> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CleanLegacyDemoDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting cleanup of legacy demo data...");

            // Find all products that do not belong to the genuine Pakistani catalog (missing SourceProductUrl or legacy SKU prefixes)
            var allLegacyProducts = await _context.Products
                .Include(p => p.OrderItems)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.InventoryMovements)
                .Include(p => p.Reviews)
                .Include(p => p.Questions)
                .Where(p => string.IsNullOrEmpty(p.SourceProductUrl) || !p.SKU.StartsWith("PK-"))
                .ToListAsync(cancellationToken);

            int archivedCount = 0;
            int deletedCount = 0;

            foreach (var product in allLegacyProducts)
            {
                // Check if product is referenced in orders
                bool hasOrders = product.OrderItems.Any() ||
                                 await _context.OrderItems.AnyAsync(oi => oi.ProductId == product.Id, cancellationToken);

                if (hasOrders)
                {
                    // Archive to preserve historical order integrity
                    product.Status = ProductStatus.Archived;
                    product.IsFeatured = false;
                    product.IsFlashDeal = false;
                    product.IsTrending = false;
                    product.IsBestSeller = false;
                    product.IsNewArrival = false;
                    product.Rating = 0.0;
                    product.ReviewCount = 0;

                    // Remove fake reviews or questions attached to legacy products
                    if (product.Reviews.Any()) _context.Reviews.RemoveRange(product.Reviews);
                    if (product.Questions.Any()) _context.QuestionAnswers.RemoveRange(product.Questions);

                    archivedCount++;
                    _logger.LogInformation("Archived legacy ordered product ID {Id}: {Title} ({SKU})", product.Id, product.Title, product.SKU);
                }
                else
                {
                    // Remove child entities first
                    if (product.Reviews.Any()) _context.Reviews.RemoveRange(product.Reviews);
                    if (product.Questions.Any()) _context.QuestionAnswers.RemoveRange(product.Questions);
                    if (product.Images.Any()) _context.ProductImages.RemoveRange(product.Images);
                    if (product.Variants.Any()) _context.ProductVariants.RemoveRange(product.Variants);
                    if (product.InventoryMovements.Any()) _context.InventoryMovements.RemoveRange(product.InventoryMovements);

                    _context.Products.Remove(product);
                    deletedCount++;
                }
            }

            // Remove any leftover fake seeded reviews
            var fakeReviewers = new[] { "Sarah Jenkins", "David Miller", "Marcus Vance", "Elena Rostova", "Jake Coleman" };
            var fakeReviews = await _context.Reviews
                .Where(r => fakeReviewers.Contains(r.UserName))
                .ToListAsync(cancellationToken);
            if (fakeReviews.Any())
            {
                _context.Reviews.RemoveRange(fakeReviews);
            }

            // Remove any leftover fake seeded Q&As
            var fakeQuestioners = new[] { "Tom K.", "Rachel G." };
            var fakeQuestions = await _context.QuestionAnswers
                .Where(q => fakeQuestioners.Contains(q.AskedBy))
                .ToListAsync(cancellationToken);
            if (fakeQuestions.Any())
            {
                _context.QuestionAnswers.RemoveRange(fakeQuestions);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Legacy cleanup finished: {Archived} archived, {Deleted} deleted, {FakeReviews} fake reviews removed.",
                archivedCount, deletedCount, fakeReviews.Count);

            return deletedCount;
        }

        public async Task SyncCategoriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Synchronizing 20 Pakistani e-commerce categories...");

            var officialCategories = PakistanCatalogBuilder.GetOfficialCategories();
            var existingCategories = await _context.Categories.ToListAsync(cancellationToken);

            // Legacy slug mappings to align existing DB records with the 20 official categories
            var slugMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "electronics", "tvs-entertainment" },
                { "laptops", "laptops-computers" },
                { "fashion", "mens-fashion" },
                { "shoes", "shoes-footwear" },
                { "watches", "watches-jewellery" },
                { "beauty", "beauty-personal-care" },
                { "home-kitchen", "kitchen-appliances" },
                { "gaming", "toys-games" },
                { "furniture", "furniture-decor" },
                { "grocery", "grocery-beverages" },
                { "sports", "sports-fitness" },
                { "books", "books-stationery" },
                { "automotive", "home-appliances" },
                { "pet-supplies", "mobile-accessories" },
                { "mobile-phones", "mobile-phones" }
            };

            // 1. Update existing categories
            foreach (var existing in existingCategories)
            {
                string targetSlug = existing.Slug;
                if (slugMapping.TryGetValue(existing.Slug, out var mapped))
                {
                    targetSlug = mapped;
                }

                var official = officialCategories.FirstOrDefault(c => c.Slug.Equals(targetSlug, StringComparison.OrdinalIgnoreCase));
                if (official != null)
                {
                    existing.Name = official.Name;
                    existing.Slug = official.Slug;
                    existing.Icon = official.Icon;
                    existing.Description = official.Description;
                    existing.DisplayOrder = official.DisplayOrder;
                    existing.IsFeatured = official.IsFeatured;
                }
            }

            // 2. Add missing categories
            var currentSlugs = existingCategories.Select(c => c.Slug.ToLower()).ToHashSet();
            foreach (var official in officialCategories)
            {
                if (!currentSlugs.Contains(official.Slug.ToLower()))
                {
                    _context.Categories.Add(new Category
                    {
                        Name = official.Name,
                        Slug = official.Slug,
                        Icon = official.Icon,
                        Description = official.Description,
                        DisplayOrder = official.DisplayOrder,
                        IsFeatured = official.IsFeatured,
                        ProductCount = 0
                    });
                    currentSlugs.Add(official.Slug.ToLower());
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Categories synchronized. Total active categories: {Count}", currentSlugs.Count);
        }

        public async Task<CatalogueImportReport> ImportCatalogueAsync(string? jsonFilePath = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Beginning catalogue import...");

            // Resolve JSON path
            string path = jsonFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), "Data", "Catalog", "pakistan-products-2026.json");
            if (!File.Exists(path))
            {
                _logger.LogInformation("Catalogue JSON file not found at {Path}. Generating from catalogue builder...", path);
                path = PakistanCatalogBuilder.EnsureJsonFileGenerated(path);
            }

            var jsonText = await File.ReadAllTextAsync(path, cancellationToken);
            var dtos = JsonSerializer.Deserialize<List<CatalogueItemDto>>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new List<CatalogueItemDto>();

            _logger.LogInformation("Loaded {Count} items from JSON catalogue.", dtos.Count);

            // Ensure categories are synced
            await SyncCategoriesAsync(cancellationToken);

            // Clean legacy demo items
            await CleanLegacyDemoDataAsync(cancellationToken);

            var categories = await _context.Categories.ToListAsync(cancellationToken);
            var catMap = categories.ToDictionary(c => c.Slug.ToLower(), c => c);

            var existingProducts = await _context.Products.ToDictionaryAsync(p => p.SKU, p => p, cancellationToken);

            int inserted = 0;
            int updated = 0;

            const int batchSize = 100;
            int count = 0;

            foreach (var dto in dtos)
            {
                if (!catMap.TryGetValue(dto.Category.ToLower(), out var cat))
                {
                    // Fallback search by name
                    cat = categories.FirstOrDefault(c => c.Name.Equals(dto.Category, StringComparison.OrdinalIgnoreCase));
                    if (cat == null)
                    {
                        _logger.LogWarning("Category {Category} not found for item {Title}. Skipping.", dto.Category, dto.Title);
                        continue;
                    }
                }

                double discount = 0;
                if (dto.OldPrice > 0 && dto.OldPrice > dto.Price)
                {
                    discount = Math.Round((double)((dto.OldPrice - dto.Price) / dto.OldPrice) * 100, 1);
                }

                DateTime priceChecked = DateTime.TryParse(dto.PriceCheckedAt, out var dt) ? dt : DateTime.UtcNow;

                if (existingProducts.TryGetValue(dto.SKU, out var existing))
                {
                    // Update
                    existing.Title = dto.Title;
                    existing.Brand = dto.Brand;
                    existing.CategoryId = cat.Id;
                    existing.CategoryName = cat.Name;
                    existing.Price = dto.Price;
                    existing.OldPrice = dto.OldPrice;
                    existing.DiscountPercentage = discount;
                    existing.Stock = dto.Stock;
                    existing.ShortDescription = dto.ShortDescription;
                    existing.MainImage = dto.MainImage;
                    existing.ImageSourceUrl = dto.ImageSourceUrl;
                    existing.SourceRetailer = dto.SourceRetailer;
                    existing.SourceProductUrl = dto.SourceProductUrl;
                    existing.PriceCheckedAt = priceChecked;
                    existing.Status = ProductStatus.Published;
                    existing.IsFeatured = dto.IsFeatured;
                    existing.IsFlashDeal = dto.IsFlashDeal;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
                else
                {
                    // Insert
                    var product = new Product
                    {
                        Title = dto.Title,
                        Brand = dto.Brand,
                        CategoryId = cat.Id,
                        CategoryName = cat.Name,
                        SKU = dto.SKU,
                        Slug = dto.Slug,
                        Price = dto.Price,
                        OldPrice = dto.OldPrice,
                        DiscountPercentage = discount,
                        Stock = dto.Stock,
                        Status = ProductStatus.Published,
                        Rating = 0.0,
                        ReviewCount = 0,
                        ShortDescription = dto.ShortDescription,
                        FullDescription = dto.ShortDescription,
                        MainImage = dto.MainImage,
                        ImageSourceUrl = dto.ImageSourceUrl,
                        SourceRetailer = dto.SourceRetailer,
                        SourceProductUrl = dto.SourceProductUrl,
                        PriceCheckedAt = priceChecked,
                        IsFeatured = dto.IsFeatured,
                        IsFlashDeal = dto.IsFlashDeal,
                        FlashDealEnd = dto.IsFlashDeal ? DateTime.UtcNow.AddDays(7) : null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = dto.MainImage,
                        AltText = dto.Title,
                        IsMain = true,
                        SortOrder = 0
                    });

                    _context.Products.Add(product);
                    existingProducts[product.SKU] = product;
                    inserted++;
                }

                count++;
                if (count % batchSize == 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Saved batch of products. Total processed: {Count}", count);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Update category product counts
            foreach (var cat in categories)
            {
                cat.ProductCount = await _context.Products
                    .CountAsync(p => p.CategoryId == cat.Id && p.Status == ProductStatus.Published, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Catalogue import complete. Inserted: {Inserted}, Updated: {Updated}", inserted, updated);

            return await GenerateVerificationReportAsync(cancellationToken);
        }

        public async Task<CatalogueImportReport> GenerateVerificationReportAsync(CancellationToken cancellationToken = default)
        {
            var report = new CatalogueImportReport();

            var publishedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Published)
                .ToListAsync(cancellationToken);

            report.PublishedProductCount = publishedProducts.Count;
            report.TotalProcessed = await _context.Products.CountAsync(cancellationToken);
            report.TotalArchived = await _context.Products.CountAsync(p => p.Status == ProductStatus.Archived, cancellationToken);

            // Category counts and price ranges
            var groups = publishedProducts.GroupBy(p => p.CategoryName ?? p.Category?.Name ?? "Uncategorized");
            foreach (var g in groups)
            {
                report.CategoryCounts[g.Key] = g.Count();
                report.CategoryPriceRanges[g.Key] = (g.Min(p => p.Price), g.Max(p => p.Price));
            }

            // Source counts
            var sources = publishedProducts.GroupBy(p => p.SourceRetailer ?? "Unknown");
            foreach (var s in sources)
            {
                report.SourceCounts[s.Key] = s.Count();
            }

            // Checks
            var skuGroups = publishedProducts.GroupBy(p => p.SKU).Where(g => g.Count() > 1);
            report.DuplicateSkus = skuGroups.Count();

            var slugGroups = publishedProducts.GroupBy(p => p.Slug).Where(g => g.Count() > 1);
            report.DuplicateSlugs = slugGroups.Count();

            report.MissingImages = publishedProducts.Count(p => string.IsNullOrWhiteSpace(p.MainImage));
            report.MissingSourceUrls = publishedProducts.Count(p => string.IsNullOrWhiteSpace(p.SourceProductUrl));
            report.InvalidPrices = publishedProducts.Count(p => p.Price <= 0);

            report.LegacyPublishedProducts = publishedProducts.Count(p => !p.SKU.StartsWith("PK-"));

            var fakeReviewers = new[] { "Sarah Jenkins", "David Miller", "Marcus Vance", "Elena Rostova", "Jake Coleman" };
            report.FakeSeededReviews = await _context.Reviews.CountAsync(r => fakeReviewers.Contains(r.UserName), cancellationToken);

            return report;
        }
    }
}
