using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using HamaraCommerce.Controllers;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class CheckoutIdempotencyRealSqlTests : IDisposable
    {
        private readonly string _dbName;
        private readonly ApplicationDbContext _seedContext;
        private readonly ShippingTaxService _shippingTaxService;
        private readonly PricingService _seedPricingService;

        public CheckoutIdempotencyRealSqlTests()
        {
            _dbName = "HamaraCommerce_IdemTest_" + Guid.NewGuid().ToString("N")[..12];
            _seedContext = TestDbContextFactory.CreateSqlServerDbContext(_dbName);
            _shippingTaxService = new ShippingTaxService();
            _seedPricingService = new PricingService(_seedContext, _shippingTaxService, NullLogger<PricingService>.Instance);
        }

        public void Dispose()
        {
            try
            {
                _seedContext.Database.EnsureDeleted();
                _seedContext.Dispose();
            }
            catch
            {
                // Ignored on cleanup
            }
        }

        private ApplicationDbContext CreateTestDbContext()
        {
            var connStr = TestDbContextFactory.GetSqlServerConnectionString(_dbName);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;
            return new ApplicationDbContext(options);
        }

        private (CheckoutController controller, Mock<IPaymentGateway> paymentMock, Mock<ICartService> cartMock) CreateController(
            ApplicationDbContext context,
            CartData cartData,
            string? userId = null,
            string userEmail = "buyer@test.com",
            bool emailConfirmed = true)
        {
            var cartMock = new Mock<ICartService>();
            cartMock.Setup(c => c.GetRawCartDataAsync()).ReturnsAsync(cartData);
            cartMock.Setup(c => c.ClearCartAsync()).Returns(Task.CompletedTask);

            var shippingTaxService = new ShippingTaxService();
            var pricingService = new PricingService(context, shippingTaxService, NullLogger<PricingService>.Instance);

            var paymentMock = new Mock<IPaymentGateway>();
            paymentMock.Setup(p => p.IsMethodSupported(It.IsAny<string>())).Returns(true);
            paymentMock.Setup(p => p.IsDevelopmentSandboxAvailable).Returns(true);
            paymentMock.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()))
                .ReturnsAsync((PaymentProcessingRequest req) => new PaymentProcessingResult
                {
                    Success = true,
                    Provider = "TestGateway",
                    ProviderReference = "TXN-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(),
                    Status = PaymentStatus.Paid,
                    ProcessedAt = DateTime.UtcNow
                });

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            if (!string.IsNullOrEmpty(userId))
            {
                var appUser = new ApplicationUser
                {
                    Id = userId,
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Test Customer",
                    EmailConfirmed = emailConfirmed
                };
                userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(appUser);
            }
            else
            {
                userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);
            }

            var emailSenderMock = new Mock<IEmailSender>();
            var emailTemplateMock = new Mock<IEmailTemplateService>();
            emailTemplateMock.Setup(t => t.GenerateOrderConfirmationEmail(It.IsAny<Order>())).Returns("<html>Order Email</html>");

            var controller = new CheckoutController(
                context,
                cartMock.Object,
                pricingService,
                shippingTaxService,
                paymentMock.Object,
                userManagerMock.Object,
                emailSenderMock.Object,
                emailTemplateMock.Object,
                NullLogger<CheckoutController>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (!string.IsNullOrEmpty(userId))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, userEmail)
                };
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return (controller, paymentMock, cartMock);
        }

        [Fact]
        public async Task Test1_SameKey_SimultaneousSubmissions_CreatesOneOrderAndOnePaymentAttempt()
        {
            // Arrange
            var category = new Category { Name = "Tech", Slug = "tech-" + Guid.NewGuid().ToString("N") };
            _seedContext.Categories.Add(category);
            await _seedContext.SaveChangesAsync();

            var product = new Product
            {
                Title = "Wireless Mouse",
                SKU = "WM-001",
                CategoryId = category.Id,
                CategoryName = "Tech",
                Price = 3000m,
                Stock = 20,
                Status = ProductStatus.Published
            };
            _seedContext.Products.Add(product);
            await _seedContext.SaveChangesAsync();

            var cartData = new CartData
            {
                Items = new List<CartItemData>
                {
                    new CartItemData { ProductId = product.Id, Quantity = 1 }
                }
            };

            var calculatedCart = await _seedPricingService.CalculateCartAsync(cartData, null, "Standard");

            string idempotencyToken = "idempotent-key-" + Guid.NewGuid().ToString("N");
            int paymentCalls = 0;

            var model1 = new CheckoutFormViewModel
            {
                IdempotencyToken = idempotencyToken,
                CustomerName = "Alice Buyer",
                CustomerEmail = "alice@test.com",
                CustomerPhone = "03001234567",
                StreetAddress = "123 Main St",
                City = "Lahore",
                State = "Punjab",
                PostalCode = "54000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            var model2 = new CheckoutFormViewModel
            {
                IdempotencyToken = idempotencyToken,
                CustomerName = "Alice Buyer",
                CustomerEmail = "alice@test.com",
                CustomerPhone = "03001234567",
                StreetAddress = "123 Main St",
                City = "Lahore",
                State = "Punjab",
                PostalCode = "54000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            using var ctx1 = CreateTestDbContext();
            using var ctx2 = CreateTestDbContext();

            var (ctrl1, payMock1, _) = CreateController(ctx1, cartData, null, "alice@test.com");
            var (ctrl2, payMock2, _) = CreateController(ctx2, cartData, null, "alice@test.com");

            payMock1.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()))
                .ReturnsAsync(() =>
                {
                    Interlocked.Increment(ref paymentCalls);
                    return new PaymentProcessingResult
                    {
                        Success = true,
                        Provider = "TestGateway",
                        ProviderReference = "TXN-SIMUL-01",
                        Status = PaymentStatus.Paid,
                        ProcessedAt = DateTime.UtcNow
                    };
                });

            payMock2.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()))
                .ReturnsAsync(() =>
                {
                    Interlocked.Increment(ref paymentCalls);
                    return new PaymentProcessingResult
                    {
                        Success = true,
                        Provider = "TestGateway",
                        ProviderReference = "TXN-SIMUL-02",
                        Status = PaymentStatus.Paid,
                        ProcessedAt = DateTime.UtcNow
                    };
                });

            // Act: Run two simultaneous checkout submissions with the exact same key
            var task1 = ctrl1.ProcessOrder(model1);
            var task2 = ctrl2.ProcessOrder(model2);
            await Task.WhenAll(task1, task2);

            // Assert: Exactly ONE payment attempt was executed
            Assert.Equal(1, paymentCalls);

            // Exactly ONE order exists in the database
            using var verifyCtx = CreateTestDbContext();
            var orders = await verifyCtx.Orders.ToListAsync();
            Assert.Single(orders);

            var idempotencyRecord = await verifyCtx.CheckoutIdempotencyRecords
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyToken);
            Assert.NotNull(idempotencyRecord);
            Assert.Equal(IdempotencyStatus.Completed, idempotencyRecord.Status);
            Assert.Equal(orders[0].OrderNumber, idempotencyRecord.OrderNumber);
            using var replayContext = CreateTestDbContext();
            var (replayController, replayPayment, _) = CreateController(replayContext, new CartData(), null, "alice@test.com");
            var replay = await replayController.ProcessOrder(model1);
            Assert.IsType<RedirectToActionResult>(replay);
            replayPayment.Verify(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()), Times.Never);
            Assert.Single(await replayContext.EmailOutboxMessages.Where(e => e.EventKey!.StartsWith("order-confirmation:")).ToListAsync());
        }

        [Fact]
        public async Task Test2_ChangedPayload_WithSameIdempotencyKey_IsRejectedWithHashMismatch()
        {
            // Arrange
            var category = new Category { Name = "Tech", Slug = "tech-" + Guid.NewGuid().ToString("N") };
            _seedContext.Categories.Add(category);
            await _seedContext.SaveChangesAsync();

            var product = new Product
            {
                Title = "USB Cable",
                SKU = "USB-001",
                CategoryId = category.Id,
                CategoryName = "Tech",
                Price = 500m,
                Stock = 20,
                Status = ProductStatus.Published
            };
            _seedContext.Products.Add(product);
            await _seedContext.SaveChangesAsync();

            var cartData = new CartData
            {
                Items = new List<CartItemData> { new CartItemData { ProductId = product.Id, Quantity = 1 } }
            };

            var calculatedCart = await _seedPricingService.CalculateCartAsync(cartData, null, "Standard");

            string sharedKey = "shared-key-" + Guid.NewGuid().ToString("N");

            var modelInitial = new CheckoutFormViewModel
            {
                IdempotencyToken = sharedKey,
                CustomerName = "Bob Buyer",
                CustomerEmail = "bob@test.com",
                CustomerPhone = "03009999999",
                StreetAddress = "Street 1, Lahore",
                City = "Lahore",
                State = "Punjab",
                PostalCode = "54000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            // First submission succeeds
            using var ctx1 = CreateTestDbContext();
            var (ctrl1, _, _) = CreateController(ctx1, cartData, null, "bob@test.com");
            var result1 = await ctrl1.ProcessOrder(modelInitial);
            Assert.IsType<RedirectToActionResult>(result1);

            // Second submission: SAME KEY, but CHANGED DELIVERY ADDRESS (tampered/different payload)
            var modelChanged = new CheckoutFormViewModel
            {
                IdempotencyToken = sharedKey,
                CustomerName = "Bob Buyer",
                CustomerEmail = "bob@test.com",
                CustomerPhone = "03009999999",
                StreetAddress = "Different Street 99, Karachi", // Changed!
                City = "Karachi",                              // Changed!
                State = "Sindh",
                PostalCode = "75000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            using var ctx2 = CreateTestDbContext();
            var (ctrl2, _, _) = CreateController(ctx2, cartData, null, "bob@test.com");

            // Act: Submit changed payload on same idempotency key
            var result2 = await ctrl2.ProcessOrder(modelChanged);

            // Assert: Must be rejected with 400 Bad Request and error message
            Assert.Equal(StatusCodes.Status400BadRequest, ctrl2.Response.StatusCode);
            Assert.True(ctrl2.ModelState.ErrorCount > 0);
            var modelError = ctrl2.ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault();
            Assert.NotNull(modelError);
            Assert.Contains("different request parameters", modelError.ErrorMessage, StringComparison.OrdinalIgnoreCase);

            // Database still has only 1 order
            using var verifyCtx = CreateTestDbContext();
            Assert.Single(await verifyCtx.Orders.ToListAsync());
        }

        [Fact]
        public async Task Test3_LastUnitContention_PreservesInventory_WithoutTestOnlyLocks()
        {
            // Arrange
            var category = new Category { Name = "RareItems", Slug = "rare-" + Guid.NewGuid().ToString("N") };
            _seedContext.Categories.Add(category);
            await _seedContext.SaveChangesAsync();

            var rareProduct = new Product
            {
                Title = "Collector Item 1/1",
                SKU = "RARE-001",
                CategoryId = category.Id,
                CategoryName = "RareItems",
                Price = 50000m,
                Stock = 1, // EXACTLY 1 REMAINING UNIT
                Status = ProductStatus.Published
            };
            _seedContext.Products.Add(rareProduct);
            await _seedContext.SaveChangesAsync();

            var cartData = new CartData
            {
                Items = new List<CartItemData> { new CartItemData { ProductId = rareProduct.Id, Quantity = 1 } }
            };

            var calculatedCart = await _seedPricingService.CalculateCartAsync(cartData, null, "Standard");

            using var ctx1 = CreateTestDbContext();
            using var ctx2 = CreateTestDbContext();

            var (ctrl1, _, _) = CreateController(ctx1, cartData, null, "buyer1@test.com");
            var (ctrl2, _, _) = CreateController(ctx2, cartData, null, "buyer2@test.com");

            var model1 = new CheckoutFormViewModel
            {
                IdempotencyToken = "buyer1-key-" + Guid.NewGuid().ToString("N"),
                CustomerName = "Buyer One",
                CustomerEmail = "buyer1@test.com",
                CustomerPhone = "03001111111",
                StreetAddress = "Address 1",
                City = "Lahore",
                State = "Punjab",
                PostalCode = "54000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            var model2 = new CheckoutFormViewModel
            {
                IdempotencyToken = "buyer2-key-" + Guid.NewGuid().ToString("N"),
                CustomerName = "Buyer Two",
                CustomerEmail = "buyer2@test.com",
                CustomerPhone = "03002222222",
                StreetAddress = "Address 2",
                City = "Lahore",
                State = "Punjab",
                PostalCode = "54000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            // Act: Submit two concurrent checkouts competing for the last unit
            var task1 = ctrl1.ProcessOrder(model1);
            var task2 = ctrl2.ProcessOrder(model2);
            await Task.WhenAll(task1, task2);

            // Assert: Exactly ONE succeeded and redirected to confirmation
            var res1 = await task1;
            var res2 = await task2;
            bool result1Success = res1 is RedirectToActionResult;
            bool result2Success = res2 is RedirectToActionResult;

            Assert.True(result1Success ^ result2Success); // Exactly one true

            // Verify final inventory in SQL Server: Stock must be 0 (NEVER negative)
            using var verifyCtx = CreateTestDbContext();
            var finalProduct = await verifyCtx.Products.FirstAsync(p => p.Id == rareProduct.Id);
            Assert.Equal(0, finalProduct.Stock);

            // Exactly 1 order in the database
            var orders = await verifyCtx.Orders.ToListAsync();
            Assert.Single(orders);
        }

        [Fact]
        public async Task Test4_PaymentSuccess_FollowedByPersistenceFailure_IsRecoverableWithoutDoubleCharging()
        {
            // Arrange
            var category = new Category { Name = "Goods", Slug = "goods-" + Guid.NewGuid().ToString("N") };
            _seedContext.Categories.Add(category);
            await _seedContext.SaveChangesAsync();

            var product = new Product
            {
                Title = "Ergonomic Chair",
                SKU = "CHAIR-001",
                CategoryId = category.Id,
                CategoryName = "Goods",
                Price = 18000m,
                Stock = 1,
                Status = ProductStatus.Published
            };
            _seedContext.Products.Add(product);
            await _seedContext.SaveChangesAsync();

            var cartData = new CartData
            {
                Items = new List<CartItemData> { new CartItemData { ProductId = product.Id, Quantity = 1 } }
            };

            var calculatedCart = await _seedPricingService.CalculateCartAsync(cartData, null, "Standard");

            string idempotencyToken = "recovery-key-" + Guid.NewGuid().ToString("N");
            int paymentCalls = 0;

            var model = new CheckoutFormViewModel
            {
                IdempotencyToken = idempotencyToken,
                CustomerName = "Charlie Buyer",
                CustomerEmail = "charlie@test.com",
                CustomerPhone = "03003333333",
                StreetAddress = "789 Pine Ave",
                City = "Lahore",
                State = "Punjab",
                PostalCode = "54000",
                Country = "Pakistan",
                ShippingMethod = "Standard",
                PaymentMethod = "SandboxCard",
                ExpectedGrandTotal = calculatedCart.GrandTotal
            };

            // Attempt 1: Simulate payment succeeds, but right before the order transaction commits,
            // another transaction temporarily sets stock to 0 to simulate stock contention / DB error.
            using var ctx1 = CreateTestDbContext();
            var (ctrl1, payMock1, _) = CreateController(ctx1, cartData, null, "charlie@test.com");

            payMock1.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()))
                .ReturnsAsync(() =>
                {
                    Interlocked.Increment(ref paymentCalls);
                    // Right after payment gateway succeeds, exhaust stock before DB transaction runs:
                    using var tempCtx = CreateTestDbContext();
                    var p = tempCtx.Products.Find(product.Id);
                    if (p != null) { p.Stock = 0; tempCtx.SaveChanges(); }

                    return new PaymentProcessingResult
                    {
                        Success = true,
                        Provider = "StripeSandbox",
                        ProviderReference = "TXN-PAID-RECOVER-12345",
                        Status = PaymentStatus.Paid,
                        ProcessedAt = DateTime.UtcNow
                    };
                });

            // Act 1: First attempt
            var result1 = await ctrl1.ProcessOrder(model);

            // Assert 1: Order transaction failed, but payment was charged -> state must be RecoveryRequired
            Assert.IsType<ViewResult>(result1);
            Assert.Equal(1, paymentCalls);

            using var verifyCtx1 = CreateTestDbContext();
            var record = await verifyCtx1.CheckoutIdempotencyRecords
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyToken);
            Assert.NotNull(record);
            Assert.Equal(IdempotencyStatus.RecoveryRequired, record.Status);
            Assert.Equal("TXN-PAID-RECOVER-12345", record.PaymentReference);
            Assert.Null(record.OrderId); // No order created yet

            // Replenish stock for recovery
            using (var replenishCtx = CreateTestDbContext())
            {
                var p = replenishCtx.Products.Find(product.Id);
                if (p != null) { p.Stock = 5; replenishCtx.SaveChanges(); }
            }

            // Act 2: Customer or recovery service retries with the SAME idempotency key
            using var ctx2 = CreateTestDbContext();
            var (ctrl2, payMock2, _) = CreateController(ctx2, cartData, null, "charlie@test.com");
            payMock2.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()))
                .ReturnsAsync(() =>
                {
                    Interlocked.Increment(ref paymentCalls); // Should NEVER be called in recovery!
                    throw new InvalidOperationException("Gateway should not be called twice for already-paid idempotency record!");
                });

            var result2 = await ctrl2.ProcessOrder(model);

            // Assert 2: Recovery succeeded without re-charging gateway!
            Assert.IsType<RedirectToActionResult>(result2);
            Assert.Equal(1, paymentCalls); // Payment calls count is STILL 1!

            using var verifyCtx2 = CreateTestDbContext();
            var finalRecord = await verifyCtx2.CheckoutIdempotencyRecords
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyToken);
            Assert.NotNull(finalRecord);
            Assert.Equal(IdempotencyStatus.Completed, finalRecord.Status);
            Assert.NotNull(finalRecord.OrderId);

            var createdOrder = await verifyCtx2.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.Id == finalRecord.OrderId.Value);
            Assert.NotNull(createdOrder);
            Assert.Equal("TXN-PAID-RECOVER-12345", createdOrder.Payments.First().ProviderTransactionId);
        }

        [Fact]
        public async Task UncertainGatewayFailureDoesNotTriggerABlindSecondPayment()
        {
            var category = new Category { Name = "Recovery", Slug = "uncertain" };
            _seedContext.Categories.Add(category); await _seedContext.SaveChangesAsync();
            var product = new Product { Title = "Recovery Item", SKU = "UNCERTAIN", CategoryId = category.Id,
                CategoryName = category.Name, Price = 100m, Stock = 5, Status = ProductStatus.Published };
            _seedContext.Products.Add(product); await _seedContext.SaveChangesAsync();
            var data = new CartData { Items = new List<CartItemData> { new() { ProductId = product.Id, Quantity = 1 } } };
            var cart = await _seedPricingService.CalculateCartAsync(data, null, "Standard");
            var model = new CheckoutFormViewModel { IdempotencyToken = Guid.NewGuid().ToString("N"),
                CustomerName = "Recovery Buyer", CustomerEmail = "buyer@test.com", CustomerPhone = "03001234567",
                StreetAddress = "123 Main Street", City = "Karachi", State = "Sindh", PostalCode = "74000",
                Country = "Pakistan", ShippingMethod = "Standard", PaymentMethod = "SandboxCard", ExpectedGrandTotal = cart.GrandTotal };
            using var first = CreateTestDbContext();
            var (controller, payment, _) = CreateController(first, data);
            payment.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>())).ThrowsAsync(new TimeoutException("Ambiguous provider response"));
            Assert.IsType<ConflictObjectResult>(await controller.ProcessOrder(model));
            using var second = CreateTestDbContext();
            var (retry, retryPayment, _) = CreateController(second, data);
            Assert.IsType<ConflictObjectResult>(await retry.ProcessOrder(model));
            retryPayment.Verify(p => p.ProcessPaymentAsync(It.IsAny<PaymentProcessingRequest>()), Times.Never);
            Assert.Empty(await second.Orders.ToListAsync());
            Assert.Equal(IdempotencyStatus.RecoveryRequired, (await second.CheckoutIdempotencyRecords.SingleAsync()).Status);
        }

        [Fact]
        public async Task Test5_ConcurrentCouponRestoration_HappensExactlyOnce()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "FLASHRESTORE",
                DiscountPercentage = 20,
                UsageCount = 1,
                UsageLimit = 10,
                IsActive = true
            };
            _seedContext.Coupons.Add(coupon);

            var order = new Order
            {
                OrderNumber = "HC-PK-RESTORE-01",
                CustomerEmail = "restore@test.com",
                CouponCode = "FLASHRESTORE",
                DiscountAmount = 200m,
                TotalAmount = 800m,
                Status = OrderStatus.Processing
            };
            _seedContext.Orders.Add(order);
            await _seedContext.SaveChangesAsync();

            var redemption = new CouponRedemption
            {
                CouponId = coupon.Id,
                CouponCode = coupon.Code,
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                DiscountAmount = 200m,
                IsRestored = false,
                RedeemedAt = DateTime.UtcNow
            };
            _seedContext.CouponRedemptions.Add(redemption);
            await _seedContext.SaveChangesAsync();

            using var ctx1 = CreateTestDbContext();
            using var ctx2 = CreateTestDbContext();

            var shippingTaxService = new ShippingTaxService();
            var pricingService1 = new PricingService(ctx1, shippingTaxService, NullLogger<PricingService>.Instance);
            var pricingService2 = new PricingService(ctx2, shippingTaxService, NullLogger<PricingService>.Instance);

            int restoredCount = 0;
            int failedRestoreCount = 0;

            // Act: Run concurrent restoration calls on the exact same order
            var task1 = Task.Run(async () =>
            {
                var success = await pricingService1.RestoreCouponRedemptionAsync(order);
                if (success) Interlocked.Increment(ref restoredCount);
                else Interlocked.Increment(ref failedRestoreCount);
            });

            var task2 = Task.Run(async () =>
            {
                var success = await pricingService2.RestoreCouponRedemptionAsync(order);
                if (success) Interlocked.Increment(ref restoredCount);
                else Interlocked.Increment(ref failedRestoreCount);
            });

            await Task.WhenAll(task1, task2);

            // Assert: Exactly ONE restoration succeeded, and ONE returned false (exactly-once execution)
            Assert.Equal(1, restoredCount);
            Assert.Equal(1, failedRestoreCount);

            // Verify database state in SQL Server
            using var verifyCtx = CreateTestDbContext();
            var finalCoupon = await verifyCtx.Coupons.FirstAsync(c => c.Code == "FLASHRESTORE");
            Assert.Equal(0, finalCoupon.UsageCount); // Decremented from 1 to 0, NOT -1

            var finalRedemption = await verifyCtx.CouponRedemptions.FirstAsync(r => r.OrderId == order.Id);
            Assert.True(finalRedemption.IsRestored);
            Assert.NotNull(finalRedemption.RestoredAt);
        }
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _storage.Remove(key);
        public void Set(string key, byte[] value) => _storage[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value!);
    }
}
