using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ReturnAndRefundGovernanceTests
    {
        private ReturnRefundService CreateService(ApplicationDbContext context)
        {
            var shippingTaxService = new ShippingTaxService();
            var pricingService = new PricingService(context, shippingTaxService, NullLogger<PricingService>.Instance);
            var mockEmailSender = new Mock<IEmailSender>();
            var outboxService = new EmailOutboxService(context, mockEmailSender.Object, NullLogger<EmailOutboxService>.Instance);

            return new ReturnRefundService(
                context,
                pricingService,
                NullLogger<ReturnRefundService>.Instance,
                outboxService);
        }

        private Order CreateDeliveredOrder(ApplicationDbContext context, decimal totalAmount = 5000m, int itemQty = 2, int initialStock = 20, bool withVariant = false, string? userId = null)
        {
            var category = new Category
            {
                Name = "Electronics",
                Slug = "electronics-" + Guid.NewGuid().ToString("N")[..8],
                Icon = "fa-laptop"
            };
            context.Categories.Add(category);

            var product = new Product
            {
                Title = "Smart Audio Device",
                Slug = "smart-audio-" + Guid.NewGuid().ToString("N")[..8],
                SKU = "AUD-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Category = category,
                Price = totalAmount / itemQty,
                Stock = initialStock,
                Status = ProductStatus.Published
            };
            context.Products.Add(product);

            ProductVariant? variant = null;
            if (withVariant)
            {
                variant = new ProductVariant
                {
                    Product = product,
                    Name = "Midnight Black",
                    SKU = product.SKU + "-BLK",
                    PriceAdjustment = 0,
                    Stock = initialStock
                };
                context.ProductVariants.Add(variant);
            }

            context.SaveChanges();

            var order = new Order
            {
                UserId = userId,
                OrderNumber = "HC-PK-RET-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                CustomerName = "Usman Khan",
                CustomerEmail = "usman.khan@example.com",
                CustomerPhone = "+92 300 1234567",
                ShippingAddress = "House 12, Street 4, F-7/2",
                City = "Islamabad",
                State = "Federal Capital",
                PostalCode = "44000",
                Country = "Pakistan",
                PaymentMethod = "Cash on Delivery",
                PaymentStatus = PaymentStatus.Paid,
                Status = OrderStatus.Delivered,
                DeliveredAt = DateTime.UtcNow.AddDays(-2),
                OrderDate = DateTime.UtcNow.AddDays(-5),
                TotalAmount = totalAmount,
                Subtotal = totalAmount,
                Currency = "PKR",
                ReturnRequestedAt = DateTime.UtcNow.AddHours(-12),
                ReturnReason = "Item did not meet specifications",
                RefundStatus = ReturnStatus.Requested,
                ReturnInspectionState = ReturnInspectionState.AwaitingInspection
            };

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                VariantId = variant?.Id,
                ProductTitle = product.Title,
                UnitPrice = product.Price,
                Quantity = itemQty
            });

            order.Payments.Add(new PaymentTransaction
            {
                TransactionReference = "COD-REC-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                Provider = "Cash on Delivery",
                PaymentMethod = "Cash on Delivery",
                Amount = totalAmount,
                Currency = "PKR",
                Status = PaymentStatus.Paid,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });

            context.Orders.Add(order);
            context.SaveChanges();

            return order;
        }

        // =========================================================================
        // 1. REPEATED APPROVAL IDEMPOTENCY & NEVER FAKE REF / AUTO PAYMENT REFUND
        // =========================================================================
        [Fact]
        public async Task ApproveReturn_RepeatedApproval_IsIdempotent_PreservesState_NeverMarksPaymentRefunded()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context);

            // Act 1: Initial approval
            var result1 = await service.ApproveReturnAsync(order.Id, adminNotes: "Authorized for courier pickup");

            // Assert 1: Approval succeeds, sets Approved state, but DOES NOT mark payment refunded or make fake reference
            Assert.True(result1.Success);
            Assert.Equal(ReturnStatus.Approved, order.RefundStatus);
            Assert.Equal(ReturnInspectionState.AwaitingInspection, order.ReturnInspectionState);
            Assert.NotNull(order.ReturnProcessedAt);
            Assert.Equal(PaymentStatus.Paid, order.PaymentStatus); // Payment untouched!
            Assert.Equal(OrderStatus.Delivered, order.Status); // Order status untouched!
            Assert.Null(order.RefundTransactionReference); // No synthetic REF-xxx generated!
            Assert.False(order.IsRestockedOnReturn); // Not restocked yet (item hasn't been returned/inspected)

            int emailCountAfterFirst = await context.EmailOutboxMessages.CountAsync();
            Assert.True(emailCountAfterFirst > 0);

            // Act 2: Repeated approval (idempotency verification)
            var result2 = await service.ApproveReturnAsync(order.Id, adminNotes: "Second approval attempt");

            // Assert 2: Repeated approval is idempotent and does not corrupt state or re-add emails
            Assert.True(result2.Success);
            Assert.Equal(ReturnStatus.Approved, order.RefundStatus);
            Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
            Assert.Null(order.RefundTransactionReference);

            int emailCountAfterSecond = await context.EmailOutboxMessages.CountAsync();
            Assert.Equal(emailCountAfterFirst, emailCountAfterSecond); // No duplicate emails
        }

        // =========================================================================
        // 2. REPEATED REFUND COMPLETION IDEMPOTENCY & EXACTLY ONCE RESTOCKING
        // =========================================================================
        [Fact]
        public async Task CompleteRefund_RepeatedCompletion_IsIdempotent_PreventsDoubleRefundAndDoubleRestock()
        {
            // Arrange: 1 item with quantity 2, product initial stock = 20
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context, totalAmount: 6000m, itemQty: 2, initialStock: 20);
            var product = await context.Products.FindAsync(order.Items.First().ProductId);
            Assert.NotNull(product);
            Assert.Equal(20, product.Stock);

            // Move through explicit workflow
            await service.ApproveReturnAsync(order.Id);
            await service.InspectReturnAsync(order.Id, ReturnInspectionState.PassedInspection);
            Assert.Equal(ReturnStatus.RefundPending, order.RefundStatus);

            string genuineReference = "HBL-FT-98472910";

            // Act 1: Complete refund with real reference
            var result1 = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: genuineReference,
                refundAmount: 6000m,
                refundMethod: "BankTransfer",
                restock: true,
                adminNotes: "Remitted via HBL Raast Account");

            // Assert 1: First completion succeeds
            Assert.True(result1.Success);
            Assert.Equal(ReturnStatus.Completed, order.RefundStatus);
            Assert.Equal(genuineReference, order.RefundTransactionReference);
            Assert.Equal(PaymentStatus.Refunded, order.PaymentStatus);
            Assert.Equal(OrderStatus.Refunded, order.Status);
            Assert.True(order.IsRestockedOnReturn);

            // Stock restored by quantity (20 + 2 = 22)
            Assert.Equal(22, product.Stock);

            // Exactly 1 refund payment transaction recorded
            var refundPayments = await context.PaymentTransactions
                .Where(p => p.OrderId == order.Id && p.Status == PaymentStatus.Refunded)
                .ToListAsync();
            Assert.Single(refundPayments);
            Assert.Equal(genuineReference, refundPayments[0].TransactionReference);
            Assert.Equal(6000m, refundPayments[0].Amount);

            // Exactly 1 inventory movement recorded
            var movements = await context.InventoryMovements
                .Where(m => m.OrderId == order.Id && m.MovementType == InventoryMovementType.ReturnRestoration)
                .ToListAsync();
            Assert.Single(movements);
            Assert.Equal(2, movements[0].QuantityChange);

            // Act 2: Repeated completion with SAME reference (idempotency verification)
            var result2 = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: genuineReference,
                refundAmount: 6000m,
                refundMethod: "BankTransfer",
                restock: true);

            // Assert 2: Repeated completion is idempotent - NO double refund and NO double restock
            Assert.True(result2.Success);
            Assert.Equal(22, product.Stock); // Still 22, NOT 24!
            
            var refundPaymentsAfter = await context.PaymentTransactions
                .Where(p => p.OrderId == order.Id && p.Status == PaymentStatus.Refunded)
                .ToListAsync();
            Assert.Single(refundPaymentsAfter); // Still 1, NOT 2!

            var movementsAfter = await context.InventoryMovements
                .Where(m => m.OrderId == order.Id && m.MovementType == InventoryMovementType.ReturnRestoration)
                .ToListAsync();
            Assert.Single(movementsAfter); // Still 1, NOT 2!
        }

        // =========================================================================
        // 3. PARTIAL REFUND VALIDATION & STEPWISE TRACKING
        // =========================================================================
        [Fact]
        public async Task CompleteRefund_PartialRefund_ValidatesAmount_UpdatesPaymentStatusAndTracksRemainder()
        {
            // Arrange: Total order amount = 10,000 PKR
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context, totalAmount: 10000m, itemQty: 2, initialStock: 10);

            await service.ApproveReturnAsync(order.Id);
            await service.InspectReturnAsync(order.Id, ReturnInspectionState.PassedInspection);

            // Act 1: Partial refund of 4,000 PKR
            var result1 = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: "EP-PARTIAL-001",
                refundAmount: 4000m,
                refundMethod: "EasyPaisa",
                restock: false);

            // Assert 1: Partial refund succeeded, status is PartiallyRefunded, order is not yet fully refunded
            Assert.True(result1.Success);
            Assert.Equal(PaymentStatus.PartiallyRefunded, order.PaymentStatus);
            Assert.Equal(4000m, order.RefundAmount);
            Assert.Equal(OrderStatus.Delivered, order.Status); // Not full cancellation

            // Act 2: Attempting refund that exceeds remainder (7,000 > remaining 6,000)
            var excessiveResult = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: "EP-EXCESS-002",
                refundAmount: 7000m,
                refundMethod: "EasyPaisa",
                restock: false);

            // Assert 2: Excess refund is strictly rejected
            Assert.False(excessiveResult.Success);
            Assert.Contains("cannot exceed", excessiveResult.Message, StringComparison.OrdinalIgnoreCase);

            // Act 3: Remit remaining 6,000 PKR with genuine reference
            var result3 = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: "EP-PARTIAL-002",
                refundAmount: 6000m,
                refundMethod: "EasyPaisa",
                restock: true);

            // Assert 3: Second refund completes the entire order
            Assert.True(result3.Success);
            Assert.Equal(PaymentStatus.Refunded, order.PaymentStatus);
            Assert.Equal(OrderStatus.Refunded, order.Status);
            Assert.Equal(10000m, order.RefundAmount);

            var payments = await context.PaymentTransactions
                .Where(p => p.OrderId == order.Id && p.Status == PaymentStatus.Refunded)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            Assert.Equal(2, payments.Count);
            Assert.Equal(4000m, payments[0].Amount);
            Assert.Equal(6000m, payments[1].Amount);
            Assert.Equal(10000m, payments.Sum(p => p.Amount));
        }

        // =========================================================================
        // 4. REFUND AMOUNT VALIDATION (NEGATIVE, ZERO, EXCESSIVE)
        // =========================================================================
        [Theory]
        [InlineData(-500, "strictly greater than zero")]
        [InlineData(0, "strictly greater than zero")]
        [InlineData(5500, "cannot exceed")]
        public async Task CompleteRefund_ValidatesAmount_RejectsNegativeZeroOrExcessive(decimal invalidAmount, string expectedErrorSubstr)
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context, totalAmount: 5000m);

            await service.ApproveReturnAsync(order.Id);

            // Act
            var result = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: "TX-REF-VAL-01",
                refundAmount: invalidAmount,
                refundMethod: "BankTransfer");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(expectedErrorSubstr, result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================================
        // 5. GENUINE REFERENCE REQUIRED (REJECTS EMPTY, WHITESPACE, DUMMY)
        // =========================================================================
        [Theory]
        [InlineData("", "required")]
        [InlineData("   ", "required")]
        [InlineData("0", "not a valid remittance reference")]
        [InlineData("test", "not a valid remittance reference")]
        [InlineData("fake", "not a valid remittance reference")]
        [InlineData("n/a", "not a valid remittance reference")]
        [InlineData("none", "not a valid remittance reference")]
        public async Task CompleteRefund_ValidatesReference_RejectsEmptyOrDummyPlaceholders(string invalidRef, string expectedErrorSubstr)
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context, totalAmount: 3000m);

            await service.ApproveReturnAsync(order.Id);

            // Act
            var result = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: invalidRef,
                refundAmount: 3000m,
                refundMethod: "BankTransfer");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(expectedErrorSubstr, result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================================
        // 6. VARIANT RESTOCKING (PRODUCT AND VARIANT QUANTITIES RESTORED ONCE)
        // =========================================================================
        [Fact]
        public async Task VariantRestocking_RestoresBothProductAndVariantQuantitiesExactlyOnce()
        {
            // Arrange: Product with base stock 15, Variant with stock 15, Order qty = 3
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context, totalAmount: 9000m, itemQty: 3, initialStock: 15, withVariant: true);

            var item = order.Items.First();
            Assert.NotNull(item.VariantId);

            var product = await context.Products.Include(p => p.Variants).FirstAsync(p => p.Id == item.ProductId);
            var variant = product.Variants.First(v => v.Id == item.VariantId.Value);

            Assert.Equal(15, product.Stock);
            Assert.Equal(15, variant.Stock);

            await service.ApproveReturnAsync(order.Id);
            await service.InspectReturnAsync(order.Id, ReturnInspectionState.PassedInspection);

            // Act 1: Complete refund with restocking enabled
            var result = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: "JC-VARIANT-RESTOCK-01",
                refundAmount: 9000m,
                refundMethod: "JazzCash",
                restock: true);

            // Assert 1: Both base product and specific variant stock are incremented by 3
            Assert.True(result.Success);
            Assert.Equal(18, product.Stock); // 15 + 3
            Assert.Equal(18, variant.Stock); // 15 + 3
            Assert.True(order.IsRestockedOnReturn);

            // Act 2: Repeat call to verify no double variant restocking
            var repeatResult = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: "JC-VARIANT-RESTOCK-01",
                refundAmount: 9000m,
                restock: true);

            // Assert 2: Stocks remain 18
            Assert.True(repeatResult.Success);
            Assert.Equal(18, product.Stock);
            Assert.Equal(18, variant.Stock);
        }

        // =========================================================================
        // 7. TRANSACTION ROLLBACK INTEGRITY
        // =========================================================================
        [Fact]
        public async Task TransactionRollback_EnsuresAllModificationsRollbackOnFailure()
        {
            var dbName = "HamaraCommerce_Rollback_" + Guid.NewGuid().ToString("N");
            using var context = TestDbContextFactory.CreateSqlServerDbContext(dbName);
            try
            {
                var order = CreateDeliveredOrder(context, totalAmount: 4000m, itemQty: 1, initialStock: 10);
                var product = await context.Products.FindAsync(order.Items.First().ProductId);
                Assert.NotNull(product);
                Assert.Equal(10, product.Stock);

                // Simulate transactional execution where an unhandled exception occurs after state changes
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await CommerceDatabaseWork.TransactionAsync<bool>(context, async () =>
                    {
                        order.RefundStatus = ReturnStatus.Completed;
                        order.PaymentStatus = PaymentStatus.Refunded;
                        order.Status = OrderStatus.Refunded;
                        product.Stock += 1;

                        context.PaymentTransactions.Add(new PaymentTransaction
                        {
                            OrderId = order.Id,
                            TransactionReference = "FAIL-TX-REF",
                            Provider = "BankTransfer",
                            PaymentMethod = "BankTransfer",
                            Amount = 4000m,
                            Currency = "PKR",
                            Status = PaymentStatus.Refunded,
                            CreatedAt = DateTime.UtcNow
                        });

                        context.EmailOutboxMessages.Add(EmailOutboxService.CreateMessage(
                            order.CustomerEmail,
                            "Failed Email",
                            "<p>Test</p>",
                            eventKey: "failed_event_key"));

                        await context.SaveChangesAsync();

                        // Injected runtime failure triggering transaction rollback
                        throw new InvalidOperationException("Simulated unexpected database failure during remittance recording.");
                    });
                });

                // Open a separate DbContext instance to inspect the actual database state
                var connStr = TestDbContextFactory.GetSqlServerConnectionString(dbName);
                using var verifyContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connStr).Options);

                var freshOrder = await verifyContext.Orders
                    .Include(o => o.Payments)
                    .AsNoTracking()
                    .FirstAsync(o => o.Id == order.Id);

                Assert.Equal(ReturnStatus.Requested, freshOrder.RefundStatus); // Rolled back, remains Requested!
                Assert.Equal(PaymentStatus.Paid, freshOrder.PaymentStatus); // Rolled back, remains Paid!
                Assert.Equal(OrderStatus.Delivered, freshOrder.Status); // Rolled back, remains Delivered!

                var freshProduct = await verifyContext.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
                Assert.Equal(10, freshProduct.Stock); // Not incremented!

                var failedPayments = await verifyContext.PaymentTransactions
                    .Where(p => p.TransactionReference == "FAIL-TX-REF")
                    .ToListAsync();
                Assert.Empty(failedPayments); // Payment transaction rolled back!

                var failedEmails = await verifyContext.EmailOutboxMessages
                    .Where(e => e.EventKey == "failed_event_key")
                    .ToListAsync();
                Assert.Empty(failedEmails); // Outbox email rolled back!
            }
            finally
            {
                context.Database.EnsureDeleted();
            }
        }

        // =========================================================================
        // 8. COD & TEST-CARD TRUTHFUL MANUAL-RECORDING LIFECYCLE
        // =========================================================================
        [Fact]
        public async Task COD_And_TestCard_Refunds_TruthfulManualRecordingLifecycle()
        {
            // Arrange: Cash on Delivery order where cash was paid to courier
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = CreateService(context);
            var order = CreateDeliveredOrder(context, totalAmount: 7500m, userId: "user-cod-1");
            order.PaymentMethod = "Cash on Delivery";

            // Stage 1: Customer requests return
            var reqResult = await service.RequestReturnAsync(order.OrderNumber, "Defective product switch", "user-cod-1");
            Assert.True(reqResult.Success);
            Assert.Equal(ReturnStatus.Requested, order.RefundStatus);

            // Stage 2: Admin approves return (waiting for item arrival)
            var appResult = await service.ApproveReturnAsync(order.Id, adminNotes: "TCS Return pickup booked");
            Assert.True(appResult.Success);
            Assert.Equal(ReturnStatus.Approved, order.RefundStatus);
            Assert.Equal(PaymentStatus.Paid, order.PaymentStatus); // Not refunded!
            Assert.Null(order.RefundTransactionReference); // Truthful: no automated fake card refund

            // Stage 3: Admin inspects received parcel
            var inspResult = await service.InspectReturnAsync(order.Id, ReturnInspectionState.PassedInspection, "Seals intact");
            Assert.True(inspResult.Success);
            Assert.Equal(ReturnStatus.RefundPending, order.RefundStatus); // Truthful manual remittance pending

            // Stage 4: Admin manually transfers funds via Interbank IBAN transfer and records genuine ref
            string manualIbanReference = "IBAN-PK78BAHL-09182390123";
            var compResult = await service.CompleteRefundAsync(
                order.Id,
                refundTransactionReference: manualIbanReference,
                refundAmount: 7500m,
                refundMethod: "BankTransfer",
                restock: true,
                adminNotes: "Remitted to customer Habib Bank account via Raast");

            Assert.True(compResult.Success);
            Assert.Equal(ReturnStatus.Completed, order.RefundStatus);
            Assert.Equal(manualIbanReference, order.RefundTransactionReference);
            Assert.Equal(PaymentStatus.Refunded, order.PaymentStatus);
            Assert.Equal(OrderStatus.Refunded, order.Status);
            Assert.Equal("BankTransfer", order.RefundMethod);
        }

        // =========================================================================
        // 9. CONTROLLER ENDPOINT INTEGRATION: ADMIN PROCESSRETURN WORKFLOW
        // =========================================================================
        [Fact]
        public async Task AdminController_ProcessReturn_ExecutesWorkflow_AndHandlesManualRemittance()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var order = CreateDeliveredOrder(context, totalAmount: 4500m, itemQty: 1, initialStock: 8);

            var shippingTaxService = new ShippingTaxService();
            var pricingService = new PricingService(context, shippingTaxService, NullLogger<PricingService>.Instance);
            var mockEmailSender = new Mock<IEmailSender>();
            var outboxService = new EmailOutboxService(context, mockEmailSender.Object, NullLogger<EmailOutboxService>.Instance);
            var returnService = new ReturnRefundService(context, pricingService, NullLogger<ReturnRefundService>.Instance, outboxService);

            var mockUserManager = CreateMockUserManager();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var mockTemplate = new Mock<IEmailTemplateService>();

            var controller = new AdminController(
                context,
                mockUserManager.Object,
                mockEnv.Object,
                shippingTaxService,
                pricingService,
                mockEmailSender.Object,
                mockTemplate.Object,
                NullLogger<AdminController>.Instance,
                outboxService,
                returnService);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                        new Claim(ClaimTypes.Name, "Admin User"),
                        new Claim(ClaimTypes.Role, "Admin")
                    }, "TestAuth"))
                }
            };

            // Step 1: Approve return
            var approveAction = await controller.ProcessReturn(
                orderId: order.Id,
                decision: "Approve",
                inspectionState: null,
                restock: true,
                refundMethod: null,
                refundAmount: null,
                adminNotes: "Approved by governance");

            var redirectApprove = Assert.IsType<RedirectToActionResult>(approveAction);
            Assert.Equal(nameof(AdminController.Returns), redirectApprove.ActionName);
            Assert.Equal(ReturnStatus.Approved, order.RefundStatus);

            // Step 2: Complete refund with real reference
            var completeAction = await controller.ProcessReturn(
                orderId: order.Id,
                decision: "CompleteRefund",
                inspectionState: ReturnInspectionState.PassedInspection,
                restock: true,
                refundMethod: "EasyPaisa",
                refundAmount: 4500m,
                adminNotes: "Dispatched via EasyPaisa",
                refundTransactionReference: "EP-CNIC-9812401");

            var redirectComplete = Assert.IsType<RedirectToActionResult>(completeAction);
            Assert.Equal(nameof(AdminController.Returns), redirectComplete.ActionName);
            Assert.Equal(ReturnStatus.Completed, order.RefundStatus);
            Assert.Equal("EP-CNIC-9812401", order.RefundTransactionReference);
            Assert.Equal(PaymentStatus.Refunded, order.PaymentStatus);
            Assert.True(order.IsRestockedOnReturn);

            var product = await context.Products.FindAsync(order.Items.First().ProductId);
            Assert.Equal(9, product?.Stock); // 8 + 1
        }

        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            var user = new ApplicationUser { Id = "admin-1", UserName = "admin@hamara.pk", FullName = "Admin User", EmailConfirmed = true };
            mock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            return mock;
        }
    }
}
