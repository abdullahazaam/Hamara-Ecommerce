using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class SeoAndNotificationTests
    {
        [Fact]
        public async Task GenerateSitemapXml_ContainsCanonicalUrlsAndXmlNamespaces()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Products.Add(new Product
            {
                Id = 1,
                Title = "Flagship Smartphone",
                Slug = "flagship-smartphone",
                Status = ProductStatus.Published,
                Price = 120000m
            });
            await context.SaveChangesAsync();

            var seoService = new SeoService(context);

            // Act
            var sitemapXml = await seoService.GenerateSitemapXmlAsync("https://hamaracommerce.pk");

            // Assert
            Assert.Contains("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", sitemapXml);
            Assert.Contains("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">", sitemapXml);
            Assert.Contains("<loc>https://hamaracommerce.pk/Shop/Details/1</loc>", sitemapXml);
            Assert.Contains("<loc>https://hamaracommerce.pk/Home/Privacy</loc>", sitemapXml);
        }

        [Fact]
        public void GenerateRobotsTxt_DisallowsAdminAndPrivatePages()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var seoService = new SeoService(context);

            // Act
            var robots = seoService.GenerateRobotsTxt("https://hamaracommerce.pk");

            // Assert
            Assert.Contains("Disallow: /Admin/", robots);
            Assert.Contains("Disallow: /Account/", robots);
            Assert.Contains("Disallow: /Checkout/", robots);
            Assert.Contains("Disallow: /Cart/", robots);
            Assert.Contains("Disallow: /*?search=", robots);
            Assert.Contains("Sitemap: https://hamaracommerce.pk/sitemap.xml", robots);
        }

        [Fact]
        public void GenerateProductJsonLd_GeneratesValidSchema_WithOffers()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var seoService = new SeoService(context);
            var product = new Product
            {
                Id = 99,
                Title = "Wireless Bluetooth Earbuds",
                SKU = "HC-SKU-99",
                Brand = "Hamara Audio",
                Price = 6500m,
                Stock = 20,
                ShortDescription = "High fidelity sound with active noise cancellation."
            };

            // Act
            var jsonLd = seoService.GenerateProductJsonLd("https://hamaracommerce.pk", product);

            // Assert
            Assert.Contains("\"@type\":\"Product\"", jsonLd);
            Assert.Contains("\"name\":\"Wireless Bluetooth Earbuds\"", jsonLd);
            Assert.Contains("\"sku\":\"HC-SKU-99\"", jsonLd);
            Assert.Contains("\"priceCurrency\":\"PKR\"", jsonLd);
            Assert.Contains("\"price\":\"6500.00\"", jsonLd);
            Assert.Contains("https://schema.org/InStock", jsonLd);
        }

        [Fact]
        public void EmailTemplateService_GeneratesOrderConfirmation_WithLineItems()
        {
            // Arrange
            var mockShippingTaxService = new Mock<IShippingTaxService>();
            mockShippingTaxService.Setup(s => s.FormatCurrency(It.IsAny<decimal>()))
                .Returns<decimal>(d => $"Rs. {d:N0}");

            var templateService = new EmailTemplateService(mockShippingTaxService.Object);

            var order = new Order
            {
                OrderNumber = "HC-PK-2026-9999",
                TrackingNumber = "TRK-TEST1234",
                CustomerName = "Zubair Khan",
                CustomerEmail = "zubair@example.com",
                PaymentMethod = "CashOnDelivery",
                Subtotal = 10000m,
                TaxAmount = 500m,
                ShippingFee = 0m,
                TotalAmount = 10500m,
                Items = new List<OrderItem>
                {
                    new() { ProductTitle = "Smart Fitness Band", Quantity = 2, UnitPrice = 5000m }
                }
            };

            // Act
            var emailHtml = templateService.GenerateOrderConfirmationEmail(order);

            // Assert
            Assert.Contains("HC-PK-2026-9999", emailHtml);
            Assert.Contains("TRK-TEST1234", emailHtml);
            Assert.Contains("Smart Fitness Band", emailHtml);
            Assert.Contains("Rs. 10,500", emailHtml);
        }
    }
}
