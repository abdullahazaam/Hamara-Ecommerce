using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using HamaraCommerce.Controllers;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class CustomerPrivacyAndIdorTests
    {
        private (AccountController controller, ApplicationDbContext context, Mock<UserManager<ApplicationUser>> userManagerMock) CreateAccountController(ApplicationUser? currentUser = null, bool isAdmin = false)
        {
            var context = TestDbContextFactory.CreateInMemoryDbContext();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

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

            var controller = new AccountController(
                userManagerMock.Object,
                signInManagerMock.Object,
                context,
                cartServiceMock.Object,
                pricingServiceMock.Object,
                emailSenderMock.Object,
                emailTemplateMock.Object,
                NullLogger<AccountController>.Instance);

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
                if (isAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                var identity = new ClaimsIdentity(claims, "TestAuth");
                httpContext.User = new ClaimsPrincipal(identity);
                userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(currentUser);
            }

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(httpContext, Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
            return (controller, context, userManagerMock);
        }

        [Fact]
        public async Task OrderDetail_BlocksCustomerFromAccessingOtherCustomerOrder_ReturnsForbid()
        {
            // Arrange
            var customerB = new ApplicationUser { Id = "user-customer-b", Email = "customerb@example.com", EmailConfirmed = true, IsActive = true };
            var (controller, context, _) = CreateAccountController(customerB, isAdmin: false);

            var customerAOrder = new Order
            {
                OrderNumber = "HC-PK-001",
                UserId = "user-customer-a",
                CustomerEmail = "customera@example.com",
                TotalAmount = 5000m
            };
            context.Orders.Add(customerAOrder);
            await context.SaveChangesAsync();

            // Act: Customer B attempts to access Customer A's order
            var result = await controller.OrderDetail("HC-PK-001");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task OrderDetail_AllowsAdminToAccessAnyOrder_ReturnsViewWithOrder()
        {
            // Arrange
            var adminUser = new ApplicationUser { Id = "admin-user", Email = "admin@hamaracommerce.pk", EmailConfirmed = true, IsActive = true };
            var (controller, context, _) = CreateAccountController(adminUser, isAdmin: true);

            var customerAOrder = new Order
            {
                OrderNumber = "HC-PK-002",
                UserId = "user-customer-a",
                CustomerEmail = "customera@example.com",
                TotalAmount = 5000m
            };
            context.Orders.Add(customerAOrder);
            await context.SaveChangesAsync();

            // Act: Admin accesses Customer A's order
            var result = await controller.OrderDetail("HC-PK-002");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Order>(viewResult.Model);
            Assert.Equal("HC-PK-002", model.OrderNumber);
        }

        [Fact]
        public async Task OrderTracking_DeniesAnonymousLookupOfRegisteredUserOrder()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var trackingController = new OrderTrackingController(context, userManagerMock.Object, memoryCache);
            var httpContext = new DefaultHttpContext();
            var sessionMock = new Mock<ISession>();
            byte[]? val;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out val)).Returns(false);
            httpContext.Session = sessionMock.Object;
            trackingController.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var registeredOrder = new Order
            {
                OrderNumber = "HC-PK-REG-01",
                TrackingNumber = "TRK-REG-01",
                UserId = "registered-user-123",
                CustomerEmail = "victim@example.com",
                TotalAmount = 12000m
            };
            context.Orders.Add(registeredOrder);
            await context.SaveChangesAsync();

            // Act: Attacker attempts anonymous tracking providing order number and matching victim email
            var result = await trackingController.Index("TRK-REG-01", "victim@example.com");

            // Assert: Access is denied because order is registered and anonymous user is not authenticated owner
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.NotNull(trackingController.ViewBag.NotFoundMessage);
        }

        [Fact]
        public async Task OrderTracking_AllowsGuestWithValidGuestAccessToken()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var trackingController = new OrderTrackingController(context, userManagerMock.Object, memoryCache);
            var httpContext = new DefaultHttpContext();
            var sessionMock = new Mock<ISession>();
            byte[]? val;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out val)).Returns(false);
            httpContext.Session = sessionMock.Object;
            trackingController.ControllerContext = new ControllerContext { HttpContext = httpContext };

            string guestToken = Guid.NewGuid().ToString("N");
            var guestOrder = new Order
            {
                OrderNumber = "HC-PK-GUEST-01",
                TrackingNumber = "TRK-GUEST-01",
                UserId = null,
                CustomerEmail = "guest@example.com",
                GuestAccessToken = guestToken,
                GuestAccessExpiry = DateTime.UtcNow.AddDays(30),
                TotalAmount = 8500m
            };
            context.Orders.Add(guestOrder);
            await context.SaveChangesAsync();

            // Act: Guest queries with matching email and valid guestToken
            var result = await trackingController.Index("TRK-GUEST-01", "guest@example.com", guestToken);

            // Assert: Order is successfully retrieved
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Order>(viewResult.Model);
            Assert.Equal("HC-PK-GUEST-01", model.OrderNumber);
        }

        [Fact]
        public async Task OrderTracking_DeniesGuestWhenTokenIsExpiredOrInvalid()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var trackingController = new OrderTrackingController(context, userManagerMock.Object, memoryCache);
            var httpContext = new DefaultHttpContext();
            var sessionMock = new Mock<ISession>();
            byte[]? val;
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out val)).Returns(false);
            httpContext.Session = sessionMock.Object;
            trackingController.ControllerContext = new ControllerContext { HttpContext = httpContext };

            string validToken = "valid-secret-token";
            var expiredOrder = new Order
            {
                OrderNumber = "HC-PK-EXPIRED",
                TrackingNumber = "TRK-EXPIRED",
                UserId = null,
                CustomerEmail = "guest@example.com",
                GuestAccessToken = validToken,
                GuestAccessExpiry = DateTime.UtcNow.AddHours(-1), // Expired!
                TotalAmount = 4500m
            };
            context.Orders.Add(expiredOrder);
            await context.SaveChangesAsync();

            // Act: Attempt with expired token
            var resultExpired = await trackingController.Index("TRK-EXPIRED", "guest@example.com", validToken);

            // Act: Attempt with wrong token
            var resultWrong = await trackingController.Index("TRK-EXPIRED", "guest@example.com", "wrong-token");

            // Assert
            var viewExpired = Assert.IsType<ViewResult>(resultExpired);
            Assert.Null(viewExpired.Model);
            var viewWrong = Assert.IsType<ViewResult>(resultWrong);
            Assert.Null(viewWrong.Model);
        }

        [Fact]
        public async Task Invoice_AllowsGuestWithMatchingToken_AndDeniesWithoutToken()
        {
            // Arrange: Anonymous user
            var (controller, context, _) = CreateAccountController(currentUser: null);

            string guestToken = "guest-invoice-token-123";
            var guestOrder = new Order
            {
                OrderNumber = "HC-PK-INV-01",
                UserId = null,
                CustomerEmail = "buyer@test.com",
                GuestAccessToken = guestToken,
                GuestAccessExpiry = DateTime.UtcNow.AddDays(14),
                TotalAmount = 9000m
            };
            context.Orders.Add(guestOrder);
            await context.SaveChangesAsync();

            // Act 1: Anonymous access with valid guestToken
            var successResult = await controller.Invoice("HC-PK-INV-01", guestToken);

            // Act 2: Anonymous access without token
            var failureResult = await controller.Invoice("HC-PK-INV-01", null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(successResult);
            var model = Assert.IsType<Order>(viewResult.Model);
            Assert.Equal("HC-PK-INV-01", model.OrderNumber);

            Assert.IsType<ChallengeResult>(failureResult);
        }

        [Fact]
        public async Task ConfirmEmail_ClaimsPreviousGuestOrders_ForVerifiedCustomer()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-claim-01",
                Email = "claimant@test.com",
                UserName = "claimant@test.com",
                EmailConfirmed = false,
                IsActive = true
            };
            var (controller, context, userManagerMock) = CreateAccountController(user);

            // Add previous guest orders placed with claimant's email
            var guestOrder1 = new Order
            {
                OrderNumber = "HC-PK-GUEST-10",
                UserId = null,
                CustomerEmail = "claimant@test.com",
                GuestAccessToken = "token-1",
                TotalAmount = 2500m
            };
            var guestOrder2 = new Order
            {
                OrderNumber = "HC-PK-GUEST-11",
                UserId = null,
                CustomerEmail = "claimant@test.com",
                GuestAccessToken = "token-2",
                TotalAmount = 4000m
            };
            context.Orders.AddRange(guestOrder1, guestOrder2);
            await context.SaveChangesAsync();

            userManagerMock.Setup(m => m.FindByIdAsync("user-claim-01")).ReturnsAsync(user);
            userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "valid-token")).ReturnsAsync(IdentityResult.Success).Callback(() => user.EmailConfirmed = true);

            // Act: Confirm email
            var result = await controller.ConfirmEmail("user-claim-01", "valid-token");

            // Assert
            Assert.IsType<ViewResult>(result);

            var claimedOrder1 = await context.Orders.FirstAsync(o => o.OrderNumber == "HC-PK-GUEST-10");
            var claimedOrder2 = await context.Orders.FirstAsync(o => o.OrderNumber == "HC-PK-GUEST-11");

            Assert.Equal("user-claim-01", claimedOrder1.UserId);
            Assert.Null(claimedOrder1.GuestAccessToken);
            Assert.Equal("user-claim-01", claimedOrder2.UserId);
            Assert.Null(claimedOrder2.GuestAccessToken);
        }
    }
}
