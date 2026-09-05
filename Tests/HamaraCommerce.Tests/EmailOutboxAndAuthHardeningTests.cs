using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using HamaraCommerce.Controllers;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

namespace HamaraCommerce.Tests
{
    public class EmailOutboxAndAuthHardeningTests
    {
        private (AccountController controller, ApplicationDbContext context, Mock<UserManager<ApplicationUser>> userManagerMock, Mock<SignInManager<ApplicationUser>> signInManagerMock)
            CreateAccountController(ApplicationUser? currentUser = null)
        {
            var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                null!, null!, null!, null!);

            var cartServiceMock = new Mock<ICartService>();
            var pricingServiceMock = new Mock<IPricingService>();
            var emailSenderMock = new Mock<IEmailSender>();
            var emailTemplateMock = new Mock<IEmailTemplateService>();
            var outboxServiceMock = new Mock<IEmailOutboxService>();

            var controller = new AccountController(
                userManagerMock.Object,
                signInManagerMock.Object,
                context,
                cartServiceMock.Object,
                pricingServiceMock.Object,
                emailSenderMock.Object,
                emailTemplateMock.Object,
                NullLogger<AccountController>.Instance,
                outboxServiceMock.Object);

            var httpContext = new DefaultHttpContext();
            var sessionMock = new Mock<ISession>();
            byte[]? sessionVal;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out sessionVal)).Returns(false);
            httpContext.Session = sessionMock.Object;

            if (currentUser != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, currentUser.Id),
                    new Claim(ClaimTypes.Name, currentUser.UserName ?? currentUser.Email ?? ""),
                    new Claim(ClaimTypes.Email, currentUser.Email ?? "")
                };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                httpContext.User = new ClaimsPrincipal(identity);
                userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(currentUser);
            }

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            return (controller, context, userManagerMock, signInManagerMock);
        }

        [Fact]
        public async Task ConfirmEmail_VerifiesEmail_WithoutAutoSignIn_DirectsToExplicitLogin()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-no-autosignin",
                Email = "testconfirm@example.com",
                UserName = "testconfirm@example.com",
                EmailConfirmed = false,
                IsActive = true
            };
            var (controller, context, userManagerMock, signInManagerMock) = CreateAccountController(user);

            userManagerMock.Setup(m => m.FindByIdAsync("user-no-autosignin")).ReturnsAsync(user);
            userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "valid-token"))
                .ReturnsAsync(IdentityResult.Success)
                .Callback(() => user.EmailConfirmed = true);

            // Act
            var result = await controller.ConfirmEmail("user-no-autosignin", "valid-token");

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.True(user.EmailConfirmed);

            // Verify SignInAsync was NEVER called (user must log in explicitly)
            signInManagerMock.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
            Assert.Contains("sign in with your credentials", controller.TempData["SuccessMessage"]?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ToggleWishlist_PersistenceFailure_ReturnsHonestFailureResponse()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-wishlist-fail",
                Email = "wishlist@example.com",
                UserName = "wishlist@example.com",
                EmailConfirmed = true,
                IsActive = true,
                WishlistProductIds = new List<int> { 1, 2 }
            };
            var (controller, context, userManagerMock, _) = CreateAccountController(user);

            // Setup failure when saving user
            userManagerMock.Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Database concurrency conflict" }));

            // Act: Ajax request
            controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
            var result = await controller.ToggleWishlist(3);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            var successProp = value?.GetType().GetProperty("success")?.GetValue(value);
            var messageProp = value?.GetType().GetProperty("message")?.GetValue(value);

            Assert.Equal(false, successProp);
            Assert.Contains("Could not update wishlist", messageProp?.ToString() ?? "");
        }

        [Fact]
        public async Task EmailOutboxService_DeduplicatesByEventKey()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var emailSenderMock = new Mock<IEmailSender>();
            var outboxService = new EmailOutboxService(context, emailSenderMock.Object, NullLogger<EmailOutboxService>.Instance);

            var eventKey = "order-confirmation:HC-DUP-001";

            // Act: Queue first time
            var firstQueue = await outboxService.QueueEmailAsync(
                toEmail: "buyer@example.com",
                subject: "Order Confirmation #HC-DUP-001",
                htmlBody: "<p>Thank you</p>",
                eventKey: eventKey);

            // Act: Queue duplicate identical eventKey
            var secondQueue = await outboxService.QueueEmailAsync(
                toEmail: "buyer@example.com",
                subject: "Order Confirmation #HC-DUP-001",
                htmlBody: "<p>Thank you</p>",
                eventKey: eventKey);

            // Assert
            Assert.NotNull(firstQueue);
            Assert.NotNull(secondQueue);
            Assert.Equal(firstQueue.Id, secondQueue.Id); // Same message returned

            var count = await context.EmailOutboxMessages.CountAsync(m => m.EventKey == eventKey);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task EmailOutboxService_ProcessOutbox_MarksBlocked_WhenSmtpNotConfigured()
        {
            // Arrange: Configure SmtpEmailSender without SMTP credentials in Production environment
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Smtp:Host"] = "",
                    ["Smtp:User"] = "",
                    ["Smtp:Password"] = ""
                })
                .Build();

            var hostEnvMock = new Mock<IHostEnvironment>();
            hostEnvMock.Setup(e => e.EnvironmentName).Returns("Production");

            var realEmailSender = new SmtpEmailSender(config, NullLogger<SmtpEmailSender>.Instance, hostEnvMock.Object);
            var outboxService = new EmailOutboxService(context, realEmailSender, NullLogger<EmailOutboxService>.Instance);

            await outboxService.QueueEmailAsync(
                toEmail: "client@example.com",
                subject: "Test Delivery",
                htmlBody: "<p>Test</p>",
                eventKey: "test-smtp-unconfigured");

            // Act: Process outbox
            var processed = await outboxService.ProcessOutboxAsync();

            // Assert: Honest Blocked status, NOT fake Sent
            Assert.Equal(1, processed);
            var outboxMessage = await context.EmailOutboxMessages.FirstAsync(m => m.EventKey == "test-smtp-unconfigured");
            Assert.Equal(EmailOutboxStatus.Blocked, outboxMessage.Status);
            Assert.Contains("SMTP", outboxMessage.LastError ?? "");
        }

        [Fact]
        public void EmailTemplateService_IncludesExpiringGuestTrackingAndInvoiceLinks_ForGuestOrders()
        {
            // Arrange
            var shippingTaxMock = new Mock<IShippingTaxService>();
            shippingTaxMock.Setup(s => s.FormatCurrency(It.IsAny<decimal>())).Returns<decimal>(d => $"Rs. {d:N0}");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["PublicSiteUrl"] = "https://hamaracommerce.pk"
                })
                .Build();

            var templateService = new EmailTemplateService(shippingTaxMock.Object, config);

            var guestOrder = new Order
            {
                OrderNumber = "HC-PK-GUEST-99",
                TrackingNumber = "TRK-GUEST-99",
                UserId = null,
                CustomerName = "Amina Tariq",
                CustomerEmail = "amina@example.com",
                GuestAccessToken = "guest-sec-token-777",
                GuestAccessExpiry = DateTime.UtcNow.AddDays(30),
                TotalAmount = 5000m,
                ShippingAddress = "Gulberg III",
                City = "Lahore",
                State = "Punjab",
                Items = new List<OrderItem>
                {
                    new() { ProductTitle = "Lawn Kurti", Quantity = 1, UnitPrice = 5000m }
                }
            };

            // Act
            var emailHtml = templateService.GenerateOrderConfirmationEmail(guestOrder);

            // Assert
            Assert.Contains("Secure Guest Access Links", emailHtml);
            Assert.Contains("https://hamaracommerce.pk/OrderTracking?orderNumber=HC-PK-GUEST-99", emailHtml);
            Assert.Contains("guestToken=guest-sec-token-777", emailHtml);
            Assert.Contains("https://hamaracommerce.pk/Account/Invoice/HC-PK-GUEST-99?guestToken=guest-sec-token-777", emailHtml);
            Assert.Contains("These secure guest links will expire on", emailHtml);
        }

        [Fact]
        public async Task OrderTracking_ExpiredGuestToken_ReturnsHonestExpirationNotice()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());

            var order = new Order
            {
                OrderNumber = "HC-EXP-01",
                TrackingNumber = "TRK-EXP-01",
                UserId = null,
                CustomerEmail = "guest@example.com",
                GuestAccessToken = "expired-token-123",
                GuestAccessExpiry = DateTime.UtcNow.AddHours(-2), // expired 2 hours ago
                TotalAmount = 3000m
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var controller = new OrderTrackingController(context, userManagerMock.Object, cache);
            var httpContext = new DefaultHttpContext();
            var sessionMock = new Mock<ISession>();
            byte[]? sessionVal;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out sessionVal)).Returns(false);
            httpContext.Session = sessionMock.Object;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act: Access with expired guest token
            var result = await controller.Index(trackingNumber: null, email: "guest@example.com", guestToken: "expired-token-123", orderNumber: "HC-EXP-01");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model); // Order not revealed
            var notFoundMsg = controller.ViewBag.NotFoundMessage?.ToString() ?? "";
            Assert.Contains("expired for your security", notFoundMsg);
            Assert.Contains("guest@example.com", notFoundMsg);
        }

        [Fact]
        public async Task Invoice_ExpiredGuestToken_RedirectsToLoginWithNotice()
        {
            // Arrange
            var (controller, context, _, _) = CreateAccountController(currentUser: null);

            var order = new Order
            {
                OrderNumber = "HC-INV-EXP-01",
                TrackingNumber = "TRK-INV-EXP-01",
                UserId = null,
                CustomerEmail = "guestinv@example.com",
                GuestAccessToken = "guest-inv-expired",
                GuestAccessExpiry = DateTime.UtcNow.AddMinutes(-30),
                TotalAmount = 4500m
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Invoice("HC-INV-EXP-01", "guest-inv-expired");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Contains("expired for your security", controller.TempData["ErrorMessage"]?.ToString() ?? "");
        }
    }
}
