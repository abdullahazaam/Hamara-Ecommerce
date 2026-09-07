using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using HamaraCommerce.Controllers;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class ProductReviewAndQuestionEndpointTests
    {
        private (ShopController controller, ApplicationDbContext context) CreateShopController(ApplicationUser? user = null, ApplicationDbContext? existingContext = null)
        {
            var context = existingContext ?? TestDbContextFactory.CreateInMemoryDbContext();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var seoService = new SeoService(context);
            var shippingTaxService = new ShippingTaxService();

            var controller = new ShopController(context, userManagerMock.Object, seoService, shippingTaxService);

            var httpContext = new DefaultHttpContext();
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName ?? user.Email ?? "Customer"),
                    new(ClaimTypes.Email, user.Email ?? "")
                };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                httpContext.User = new ClaimsPrincipal(identity);
                userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            }

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return (controller, context);
        }

        private (AdminController controller, ApplicationDbContext context) CreateAdminController(ApplicationUser? adminUser = null, ApplicationDbContext? existingContext = null)
        {
            var context = existingContext ?? TestDbContextFactory.CreateInMemoryDbContext();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var envMock = new Mock<IWebHostEnvironment>();
            var shippingTaxService = new ShippingTaxService();
            var pricingService = new PricingService(context, shippingTaxService, NullLogger<PricingService>.Instance);
            var emailSenderMock = new Mock<IEmailSender>();
            var emailTemplateMock = new Mock<IEmailTemplateService>();

            var controller = new AdminController(
                context,
                userManagerMock.Object,
                envMock.Object,
                shippingTaxService,
                pricingService,
                emailSenderMock.Object,
                emailTemplateMock.Object,
                NullLogger<AdminController>.Instance);

            var httpContext = new DefaultHttpContext();
            adminUser ??= new ApplicationUser { Id = "admin-1", UserName = "admin@hamaracommerce.pk", Email = "admin@hamaracommerce.pk", FullName = "Admin User" };
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, adminUser.Id),
                new(ClaimTypes.Name, adminUser.UserName ?? "admin@hamaracommerce.pk"),
                new(ClaimTypes.Email, adminUser.Email ?? "admin@hamaracommerce.pk"),
                new(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);
            userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(adminUser);

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return (controller, context);
        }

        [Fact]
        public async Task AddReview_NewReviewEntersModerationWithIsApprovedFalse_AndRatingNotInflated()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-mod-1", UserName = "moduser@example.com", Email = "moduser@example.com", FullName = "Moderated User" };
            var (shopController, context) = CreateShopController(user);

            var product = new Product
            {
                Id = 1001,
                Title = "Test Wireless Earbuds",
                Price = 4500,
                Stock = 20,
                Status = ProductStatus.Published,
                Rating = 0.0,
                ReviewCount = 0
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act: Submit new 5-star review
            var result = await shopController.AddReview(1001, 5, "Amazing Earbuds", "Sound quality is outstanding.");

            // Assert: Review saved with IsApproved = false
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);

            var review = await context.Reviews.FirstOrDefaultAsync(r => r.ProductId == 1001 && r.UserId == user.Id);
            Assert.NotNull(review);
            Assert.False(review.IsApproved, "New reviews must enter moderation with IsApproved = false.");
            Assert.Equal(5.0, review.Rating);

            // Product live storefront rating and review count must NOT be inflated by unapproved reviews
            var updatedProduct = await context.Products.FindAsync(1001);
            Assert.NotNull(updatedProduct);
            Assert.Equal(0, updatedProduct.ReviewCount);
            Assert.Equal(0.0, updatedProduct.Rating);

            Assert.Contains("pending moderation", shopController.TempData["SuccessMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AskQuestion_NewQuestionRemainsUnpublishedUntilApprovedOrAnswered()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-qa-1", UserName = "qauser@example.com", Email = "qauser@example.com", FullName = "Curious Customer" };
            var (shopController, context) = CreateShopController(user);

            var product = new Product
            {
                Id = 1002,
                Title = "Smart Health Fitness Band",
                Price = 3200,
                Stock = 15,
                Status = ProductStatus.Published
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act: Submit question
            var result = await shopController.AskQuestion(1002, "Does this fitness band support swimming mode?");

            // Assert: Question is saved with IsApproved = false and IsAnswered = false
            Assert.IsType<RedirectToActionResult>(result);
            var qa = await context.QuestionAnswers.FirstOrDefaultAsync(q => q.ProductId == 1002 && q.UserId == user.Id);
            Assert.NotNull(qa);
            Assert.False(qa.IsApproved, "New questions must remain unpublished until approved.");
            Assert.False(qa.IsAnswered);
            Assert.Equal("Does this fitness band support swimming mode?", qa.Question);

            // Act 2: Administrator answers and publishes the question
            var (adminController, _) = CreateAdminController(null, context);
            var answerResult = await adminController.AnswerQuestion(qa.Id, "Yes, it is 5ATM waterproof and supports swimming tracking.");

            Assert.IsType<RedirectToActionResult>(answerResult);
            var answeredQa = await context.QuestionAnswers.FindAsync(qa.Id);
            Assert.NotNull(answeredQa);
            Assert.True(answeredQa.IsApproved, "Question must be approved when answered by administrator.");
            Assert.True(answeredQa.IsAnswered);
            Assert.Equal("Yes, it is 5ATM waterproof and supports swimming tracking.", answeredQa.Answer);
        }

        [Fact]
        public async Task AddReview_DuplicateReviewBySameUser_IsRejected()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-dup-1", UserName = "dupuser@example.com", Email = "dupuser@example.com", FullName = "Duplicate Tester" };
            var (shopController, context) = CreateShopController(user);

            var product = new Product
            {
                Id = 1003,
                Title = "Mechanical Gaming Keyboard",
                Price = 8500,
                Stock = 10,
                Status = ProductStatus.Published
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act 1: First review submission succeeds
            await shopController.AddReview(1003, 5, "First Review", "Solid build quality.");
            var count1 = await context.Reviews.CountAsync(r => r.ProductId == 1003 && r.UserId == user.Id);
            Assert.Equal(1, count1);

            // Act 2: Second review submission by same user on same product
            var result2 = await shopController.AddReview(1003, 4, "Second Review Attempt", "Trying to review again.");

            // Assert
            Assert.IsType<RedirectToActionResult>(result2);
            Assert.Contains("already submitted a review", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            var count2 = await context.Reviews.CountAsync(r => r.ProductId == 1003 && r.UserId == user.Id);
            Assert.Equal(1, count2);
        }

        [Theory]
        [InlineData(OrderStatus.Pending, PaymentStatus.Pending, "Cash on Delivery", false, "Pending COD must never qualify")]
        [InlineData(OrderStatus.Confirmed, PaymentStatus.Pending, "Cash on Delivery", false, "Unpaid COD must never qualify")]
        [InlineData(OrderStatus.Shipped, PaymentStatus.Pending, "Cash on Delivery", false, "Shipped unpaid COD must never qualify")]
        [InlineData(OrderStatus.Delivered, PaymentStatus.Pending, "Cash on Delivery", false, "Delivered but unrecorded unpaid COD must never qualify")]
        [InlineData(OrderStatus.Delivered, PaymentStatus.Paid, "Cash on Delivery", true, "Properly recorded paid COD delivery MUST qualify")]
        [InlineData(OrderStatus.Pending, PaymentStatus.Paid, "CardGateway", false, "Pending order must never qualify even if paid")]
        [InlineData(OrderStatus.Cancelled, PaymentStatus.Paid, "CardGateway", false, "Cancelled order must never qualify")]
        [InlineData(OrderStatus.Refunded, PaymentStatus.Refunded, "CardGateway", false, "Refunded order must never qualify")]
        [InlineData(OrderStatus.Confirmed, PaymentStatus.Failed, "CardGateway", false, "Failed payment must never qualify")]
        [InlineData(OrderStatus.Confirmed, PaymentStatus.Paid, "CardGateway", true, "Paid confirmed order MUST qualify")]
        [InlineData(OrderStatus.Delivered, PaymentStatus.Paid, "CardGateway", true, "Paid delivered card order MUST qualify")]
        public async Task AddReview_VerifiedPurchaseRule_OnlyQualifiesPaidOrPaidCodDelivered(
            OrderStatus orderStatus,
            PaymentStatus paymentStatus,
            string paymentMethod,
            bool expectedVerified,
            string explanation)
        {
            // Arrange
            string userId = $"user-vp-{Guid.NewGuid():N}";
            var user = new ApplicationUser { Id = userId, UserName = $"{userId}@example.com", Email = $"{userId}@example.com", FullName = "VP Tester" };
            var (shopController, context) = CreateShopController(user);

            int prodId = new Random().Next(10000, 90000);
            var product = new Product
            {
                Id = prodId,
                Title = $"Product {prodId}",
                Price = 2500,
                Stock = 50,
                Status = ProductStatus.Published
            };
            context.Products.Add(product);

            // Create Order matching test parameters
            var order = new Order
            {
                OrderNumber = $"HC-TEST-{Guid.NewGuid():N}"[..16],
                TrackingNumber = $"TRK-{Guid.NewGuid():N}"[..12],
                UserId = userId,
                CustomerEmail = user.Email,
                CustomerName = user.FullName,
                Status = orderStatus,
                PaymentStatus = paymentStatus,
                PaymentMethod = paymentMethod,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ProductId = prodId,
                        ProductTitle = product.Title,
                        UnitPrice = product.Price,
                        Quantity = 1
                    }
                }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act
            await shopController.AddReview(prodId, 5, "Order Verification Test", "Testing verified purchase status.");

            // Assert
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.ProductId == prodId && r.UserId == userId);
            Assert.NotNull(review);
            Assert.True(review.IsVerifiedPurchase == expectedVerified,
                $"Verification failed for ({orderStatus}, {paymentStatus}, {paymentMethod}). Expected: {expectedVerified}. Reason: {explanation}");
        }

        [Fact]
        public async Task AddReview_OrderForDifferentProduct_DoesNotGrantVerifiedPurchase()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-diff-1", UserName = "diff@example.com", Email = "diff@example.com", FullName = "Different Product Buyer" };
            var (shopController, context) = CreateShopController(user);

            var productA = new Product { Id = 2001, Title = "Product A", Price = 1000, Stock = 10, Status = ProductStatus.Published };
            var productB = new Product { Id = 2002, Title = "Product B", Price = 2000, Stock = 10, Status = ProductStatus.Published };
            context.Products.AddRange(productA, productB);

            // User bought Product A (Paid & Delivered)
            var order = new Order
            {
                OrderNumber = "HC-DIFF-01",
                TrackingNumber = "TRK-DIFF-01",
                UserId = user.Id,
                CustomerEmail = user.Email,
                Status = OrderStatus.Delivered,
                PaymentStatus = PaymentStatus.Paid,
                PaymentMethod = "CardGateway",
                Items = new List<OrderItem> { new() { ProductId = 2001, ProductTitle = "Product A", UnitPrice = 1000, Quantity = 1 } }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act: Review Product B (which user did NOT buy)
            await shopController.AddReview(2002, 4, "Review for B", "Did not buy B.");

            // Assert
            var review = await context.Reviews.FirstOrDefaultAsync(r => r.ProductId == 2002 && r.UserId == user.Id);
            Assert.NotNull(review);
            Assert.False(review.IsVerifiedPurchase, "Review for unpurchased product must not be marked verified purchase.");
        }

        [Fact]
        public async Task RatingRecalculation_RecalculatesRatingsOnlyFromApprovedReviews()
        {
            // Arrange
            var context = TestDbContextFactory.CreateInMemoryDbContext();
            var product = new Product
            {
                Id = 3001,
                Title = "Stainless Steel Electric Kettle",
                Price = 3800,
                Stock = 25,
                Status = ProductStatus.Published,
                Rating = 0.0,
                ReviewCount = 0
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var user1 = new ApplicationUser { Id = "u1", UserName = "u1@example.com", Email = "u1@example.com", FullName = "User One" };
            var user2 = new ApplicationUser { Id = "u2", UserName = "u2@example.com", Email = "u2@example.com", FullName = "User Two" };

            var (shopCtrl1, _) = CreateShopController(user1, context);
            var (shopCtrl2, _) = CreateShopController(user2, context);
            var (adminCtrl, _) = CreateAdminController(null, context);

            // Step 1: User 1 submits 5-star review (Unapproved)
            await shopCtrl1.AddReview(3001, 5, "Great Kettle", "Heats water in seconds.");
            var pAfterU1 = await context.Products.FindAsync(3001);
            Assert.Equal(0.0, pAfterU1!.Rating);
            Assert.Equal(0, pAfterU1.ReviewCount);

            // Step 2: Admin approves User 1's review
            var rev1 = await context.Reviews.FirstAsync(r => r.ProductId == 3001 && r.UserId == "u1");
            await adminCtrl.ToggleReviewApproval(rev1.Id);

            var pAfterApprove1 = await context.Products.FindAsync(3001);
            Assert.Equal(5.0, pAfterApprove1!.Rating);
            Assert.Equal(1, pAfterApprove1.ReviewCount);

            // Step 3: User 2 submits 3-star review (Unapproved)
            await shopCtrl2.AddReview(3001, 3, "Average Kettle", "A bit noisy.");
            var pAfterU2 = await context.Products.FindAsync(3001);
            Assert.Equal(5.0, pAfterU2!.Rating); // Still 5.0 from approved only
            Assert.Equal(1, pAfterU2.ReviewCount);

            // Step 4: Admin approves User 2's review -> Rating becomes (5+3)/2 = 4.0
            var rev2 = await context.Reviews.FirstAsync(r => r.ProductId == 3001 && r.UserId == "u2");
            await adminCtrl.ToggleReviewApproval(rev2.Id);

            var pAfterApprove2 = await context.Products.FindAsync(3001);
            Assert.Equal(4.0, pAfterApprove2!.Rating);
            Assert.Equal(2, pAfterApprove2.ReviewCount);

            // Step 5: Admin hides User 1's review -> Rating becomes 3.0 (from only User 2's approved review)
            await adminCtrl.ToggleReviewApproval(rev1.Id);

            var pAfterHide1 = await context.Products.FindAsync(3001);
            Assert.Equal(3.0, pAfterHide1!.Rating);
            Assert.Equal(1, pAfterHide1.ReviewCount);

            // Step 6: Admin deletes User 2's review -> Rating becomes 0.0 and count 0
            await adminCtrl.DeleteReview(rev2.Id);

            var pAfterDelete2 = await context.Products.FindAsync(3001);
            Assert.Equal(0.0, pAfterDelete2!.Rating);
            Assert.Equal(0, pAfterDelete2.ReviewCount);
        }

        [Fact]
        public async Task AddReview_ValidatesLengthAndRating_AndStoresNormalTextWithoutDoubleEncoding()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-val-1", UserName = "val@example.com", Email = "val@example.com", FullName = "O'Connor & Sons" };
            var (shopController, context) = CreateShopController(user);

            var product = new Product { Id = 4001, Title = "Cotton Crew Neck T-Shirt", Price = 1800, Stock = 30, Status = ProductStatus.Published };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // 1. Invalid rating < 1
            await shopController.AddReview(4001, 0, "Zero Stars", "Invalid rating test.");
            Assert.Contains("valid rating", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 2. Invalid rating > 5
            await shopController.AddReview(4001, 6, "Six Stars", "Invalid rating test.");
            Assert.Contains("valid rating", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 3. Empty comment
            await shopController.AddReview(4001, 4, "No Comment", "   ");
            Assert.Contains("provide a review comment", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 4. Comment exceeding 2,000 characters
            string longComment = new('x', 2005);
            await shopController.AddReview(4001, 4, "Too Long", longComment);
            Assert.Contains("cannot exceed 2,000 characters", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 5. Title exceeding 200 characters
            string longTitle = new('y', 205);
            await shopController.AddReview(4001, 4, longTitle, "Valid comment length.");
            Assert.Contains("cannot exceed 200 characters", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 6. Normal text with quotes, ampersands, angle brackets stored cleanly without double encoding
            string specialTitle = "Men's & Women's Fashion <Top Pick>";
            string specialComment = "Don't miss this! 100% cotton & pre-shrunk <verified>.";

            await shopController.AddReview(4001, 5, specialTitle, specialComment);

            var review = await context.Reviews.FirstOrDefaultAsync(r => r.ProductId == 4001 && r.UserId == user.Id);
            Assert.NotNull(review);
            Assert.Equal("Men's & Women's Fashion <Top Pick>", review.Title);
            Assert.Equal("Don't miss this! 100% cotton & pre-shrunk <verified>.", review.Comment);
            Assert.Equal("O'Connor & Sons", review.UserName);
            Assert.DoesNotContain("&amp;", review.Title);
            Assert.DoesNotContain("&#39;", review.Title);
            Assert.DoesNotContain("&amp;", review.Comment);
            Assert.DoesNotContain("&#39;", review.Comment);
        }

        [Fact]
        public async Task AskQuestion_ValidatesLength_AndStoresNormalText()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-qval-1", UserName = "qval@example.com", Email = "qval@example.com", FullName = "Ask Tester" };
            var (shopController, context) = CreateShopController(user);

            var product = new Product { Id = 5001, Title = "Bluetooth Speaker", Price = 6000, Stock = 10, Status = ProductStatus.Published };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // 1. Question too short (< 5 chars)
            await shopController.AskQuestion(5001, "Hi?");
            Assert.Contains("at least 5 characters", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 2. Question exceeding 1,000 characters
            string longQuestion = new('z', 1005);
            await shopController.AskQuestion(5001, longQuestion);
            Assert.Contains("cannot exceed 1,000 characters", shopController.TempData["ErrorMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);

            // 3. Valid question with special characters stored without double-encoding
            string specialQuestion = "Is it compatible with iPhone & Android's fast pairing <NFC>?";
            await shopController.AskQuestion(5001, specialQuestion);

            var qa = await context.QuestionAnswers.FirstOrDefaultAsync(q => q.ProductId == 5001 && q.UserId == user.Id);
            Assert.NotNull(qa);
            Assert.Equal(specialQuestion, qa.Question);
            Assert.DoesNotContain("&amp;", qa.Question);
            Assert.DoesNotContain("&#39;", qa.Question);
        }
    }
}
