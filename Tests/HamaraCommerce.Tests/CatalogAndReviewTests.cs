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
    }
}
