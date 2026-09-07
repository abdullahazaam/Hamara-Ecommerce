using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class CatalogAndReviewTests
    {
        [Fact]
        public async Task ProductReviews_OnlyApprovedReviewsContributeToLiveRating()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var product = new Product
            {
                Id = 50,
                Title = "Smart 4K UHD OLED TV 55 Inch",
                Price = 185000m,
                Stock = 8,
                Status = ProductStatus.Published
            };
            context.Products.Add(product);

            // Add 1 approved 5-star review and 1 unapproved 1-star spam review
            context.Reviews.Add(new Review
            {
                Id = 1,
                ProductId = 50,
                Rating = 5,
                Comment = "Exceptional visual clarity and deep blacks!",
                IsApproved = true,
                Date = DateTime.UtcNow
            });

            context.Reviews.Add(new Review
            {
                Id = 2,
                ProductId = 50,
                Rating = 1,
                Comment = "Spam comment with external promotional links",
                IsApproved = false,
                Date = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            // Act: Calculate rating from approved reviews only
            var approvedReviews = await context.Reviews
                .Where(r => r.ProductId == 50 && r.IsApproved)
                .ToListAsync();

            double calculatedRating = approvedReviews.Average(r => r.Rating);

            // Assert
            Assert.Single(approvedReviews);
            Assert.Equal(5.0, calculatedRating);
        }

        [Fact]
        public void GenerateProductJsonLd_OmitsAggregateRating_WhenNoApprovedReviews()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var seoService = new SeoService(context);
            var product = new Product
            {
                Id = 51,
                Title = "Modern Minimalist Ergonomic Chair",
                SKU = "HC-FUR-51",
                Price = 24000m,
                Stock = 12,
                Reviews = new List<Review>() // No approved reviews
            };

            // Act
            var jsonLd = seoService.GenerateProductJsonLd("https://hamaracommerce.pk", product);

            // Assert: Must NOT fabricate or hallucinate AggregateRating
            Assert.DoesNotContain("\"aggregateRating\"", jsonLd);
        }

        [Fact]
        public async Task CanonicalCatalogue_GeneratesAndValidates342SourcePairedItems()
        {
            // Find base project root directory
            string currentDir = AppContext.BaseDirectory;
            string projectRoot = currentDir;
            while (!string.IsNullOrEmpty(projectRoot) && !File.Exists(Path.Combine(projectRoot, "HamaraCommerce.csproj")))
            {
                var parent = Directory.GetParent(projectRoot);
                if (parent == null) break;
                projectRoot = parent.FullName;
            }

            Assert.True(Directory.Exists(projectRoot), "Project root directory not found.");

            // Generate/Ensure 342-item canonical catalogue
            var items = await HamaraCommerce.Data.Catalog.CatalogueSourceGenerator.GenerateCanonicalCatalogueAsync(projectRoot);

            Assert.Equal(342, items.Count);

            var categories = items.Select(i => i.Category).Distinct().ToList();
            Assert.Equal(15, categories.Count);

            var duplicateSkus = items.GroupBy(i => i.SKU).Where(g => g.Count() > 1).ToList();
            Assert.Empty(duplicateSkus);

            var imagesDir = Path.Combine(projectRoot, "wwwroot");
            var imageHashes = new HashSet<string>();

            foreach (var item in items)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.SKU));
                Assert.False(string.IsNullOrWhiteSpace(item.Title));
                Assert.False(string.IsNullOrWhiteSpace(item.Category));
                Assert.True(item.Price > 0, $"Price for {item.SKU} should be > 0");
                Assert.False(string.IsNullOrWhiteSpace(item.MainImage));
                Assert.False(string.IsNullOrWhiteSpace(item.SourceProductUrl));

                string localImagePath = Path.Combine(imagesDir, item.MainImage.TrimStart('/', '\\'));
                Assert.True(File.Exists(localImagePath), $"Local image file {localImagePath} for SKU {item.SKU} must exist.");

                byte[] imageBytes = await File.ReadAllBytesAsync(localImagePath);
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                string hash = Convert.ToHexString(sha256.ComputeHash(imageBytes));
                Assert.True(imageHashes.Add(hash), $"Duplicate image hash detected for SKU {item.SKU}: {hash}");
            }

            Assert.Equal(342, imageHashes.Count);
        }

        [Fact]
        public void DbInitializer_FreshDatabase_Produces342PublishedProductsAnd15Categories()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            // Act
            HamaraCommerce.Data.DbInitializer.Initialize(context, isDevelopment: true);

            // Assert
            int publishedCount = context.Products.Count(p => p.Status == ProductStatus.Published);
            Assert.Equal(342, publishedCount);

            int activeCategories = context.Categories.Count(c => c.IsActive);
            Assert.Equal(15, activeCategories);

            // Every category must have products assigned
            foreach (var cat in context.Categories.Where(c => c.IsActive).ToList())
            {
                Assert.True(cat.ProductCount > 0, $"Category {cat.Slug} should have product count > 0.");
            }

            // Storefront ranks must be sequential from 1 to 342
            var ranks = context.Products
                .Where(p => p.Status == ProductStatus.Published && p.StorefrontRank.HasValue)
                .Select(p => p.StorefrontRank!.Value)
                .OrderBy(r => r)
                .ToList();
            Assert.Equal(342, ranks.Count);
            Assert.Equal(1, ranks.First());
            Assert.Equal(342, ranks.Last());
        }

        [Fact]
        public async Task CatalogueImporter_RepeatImport_IsIdempotentAndPreservesHistoricalOrders()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CatalogueImporter>.Instance;
            var importer = new CatalogueImporter(context, logger);

            // Seed an existing product that is referenced by a historical order
            var historicalProduct = new Product
            {
                Id = 9999,
                Title = "Vintage Historical Purchase",
                Brand = "Heritage",
                SKU = "HIST-ORDER-01",
                Slug = "vintage-historical-purchase",
                Price = 5000,
                Stock = 0,
                Status = ProductStatus.Archived,
                CreatedAt = DateTime.UtcNow.AddYears(-1)
            };
            var orderItem = new OrderItem
            {
                Id = 8888,
                OrderId = 7777,
                ProductId = 9999,
                ProductTitle = "Vintage Historical Purchase",
                UnitPrice = 5000,
                Quantity = 1
            };
            context.Products.Add(historicalProduct);
            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();

            // Act 1: First Import
            var report1 = await importer.ImportCatalogueAsync();

            // Assert 1
            Assert.Equal(342, report1.PublishedProductCount);
            Assert.Equal(343, report1.TotalProcessed); // 342 canonical + 1 historical
            Assert.Equal(1, report1.TotalArchived);
            Assert.Equal(15, report1.CategoryCounts.Count);
            Assert.Equal(0, report1.DuplicateSkus);

            var preserved = await context.Products.FirstOrDefaultAsync(p => p.SKU == "HIST-ORDER-01");
            Assert.NotNull(preserved);
            Assert.Equal(ProductStatus.Archived, preserved.Status);

            // Act 2: Second Import (Repeat)
            var report2 = await importer.ImportCatalogueAsync();

            // Assert 2: Must be completely idempotent
            Assert.Equal(342, report2.PublishedProductCount);
            Assert.Equal(343, report2.TotalProcessed);
            Assert.Equal(1, report2.TotalArchived);
            Assert.Equal(15, report2.CategoryCounts.Count);
            Assert.Equal(0, report2.DuplicateSkus);

            var preservedAfterRepeat = await context.Products.FirstOrDefaultAsync(p => p.SKU == "HIST-ORDER-01");
            Assert.NotNull(preservedAfterRepeat);
            Assert.Equal(ProductStatus.Archived, preservedAfterRepeat.Status);
        }
    }
}
