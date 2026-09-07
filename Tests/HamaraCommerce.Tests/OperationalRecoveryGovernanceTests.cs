using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class OperationalRecoveryGovernanceTests
    {
        // ---------------------------------------------------------------------
        // Helper Setup
        // ---------------------------------------------------------------------
        private (AdminController controller, ApplicationDbContext context, OperationalRecoveryService recoveryService) CreateAdminController(ApplicationDbContext context)
        {
            var recoveryService = new OperationalRecoveryService(context, NullLogger<OperationalRecoveryService>.Instance);
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
                returnService,
                recoveryService);

            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "admin-user-id"),
                        new Claim(ClaimTypes.Name, "Operational Admin"),
                        new Claim(ClaimTypes.Role, "Admin")
                    }, "TestAuth"))
                }
            };

            return (controller, context, recoveryService);
        }

        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            var user = new ApplicationUser { Id = "admin-user-id", UserName = "ops.admin@hamara.pk", FullName = "Operational Admin", EmailConfirmed = true };
            mock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            mock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            return mock;
        }

        // =====================================================================
        // 1. AUTHORIZATION & ANTI-FORGERY SECURITY TESTS
        // =====================================================================
        [Fact]
        public void AdminController_HasAuthorizeAttribute_WithAdminRole()
        {
            var authAttr = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authAttr);
            Assert.Equal("Admin", authAttr.Roles);
        }

        [Fact]
        public void RetryOutboxEmail_HasHttpPost_And_ValidateAntiForgeryTokenAttributes()
        {
            var method = typeof(AdminController).GetMethod(nameof(AdminController.RetryOutboxEmail));
            Assert.NotNull(method);
            Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
            Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        }

        [Fact]
        public void ReconcilePayment_HasHttpPost_And_ValidateAntiForgeryTokenAttributes()
        {
            var method = typeof(AdminController).GetMethod(nameof(AdminController.ReconcilePayment));
            Assert.NotNull(method);
            Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
            Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        }

        // =====================================================================
        // 2. OUTBOX RETRY - NEVER REQUEUE SENT MESSAGES
        // =====================================================================
        [Fact]
        public async Task RetryOutboxEmail_MessageAlreadySent_NeverRequeued_ReturnsFailure()
        {
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var (controller, _, service) = CreateAdminController(context);

            var sentMsg = new EmailOutboxMessage
            {
                ToEmail = "delivered@example.com",
                Subject = "Order Invoice",
                HtmlBody = "<p>Delivered invoice</p>",
                Status = EmailOutboxStatus.Sent,
                AttemptCount = 1,
                ProcessedAt = DateTime.UtcNow.AddMinutes(-30),
                NextAttemptAt = null
            };
            context.EmailOutboxMessages.Add(sentMsg);
            await context.SaveChangesAsync();

            // Act via controller
            var actionResult = await controller.RetryOutboxEmail(sentMsg.Id);

            // Assert: Never requeued
            var fresh = await context.EmailOutboxMessages.FindAsync(sentMsg.Id);
            Assert.NotNull(fresh);
            Assert.Equal(EmailOutboxStatus.Sent, fresh.Status); // Still Sent!
            Assert.Equal(1, fresh.AttemptCount); // Untouched!
            Assert.Null(fresh.NextAttemptAt);

            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Contains("already been marked Sent", controller.TempData["ErrorMessage"]?.ToString() ?? "");

            // Direct service invocation check
            var serviceResult = await service.RetryOutboxEmailAsync(sentMsg.Id);
            Assert.False(serviceResult.Success);
            Assert.True(serviceResult.AlreadySent);
        }

        // =====================================================================
        // 3. OUTBOX RETRY - RECLAIM ONLY FAILED, BLOCKED, OR EXPIRED PROCESSING
        // =====================================================================
        [Theory]
        [InlineData(EmailOutboxStatus.Failed, true, false)]
        [InlineData(EmailOutboxStatus.Blocked, true, false)]
        [InlineData(EmailOutboxStatus.Processing, true, true)] // Expired processing
        [InlineData(EmailOutboxStatus.Processing, false, false)] // Active processing lease (not expired)
        [InlineData(EmailOutboxStatus.Queued, false, false)] // Already Queued
        public async Task RetryOutboxEmail_ReclaimsOnlyEligibleStates(EmailOutboxStatus status, bool expectSuccess, bool isExpired)
        {
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = new OperationalRecoveryService(context, NullLogger<OperationalRecoveryService>.Instance);

            var msg = new EmailOutboxMessage
            {
                ToEmail = "target@example.com",
                Subject = "Notification",
                HtmlBody = "<p>Notice</p>",
                Status = status,
                AttemptCount = 3,
                LockToken = status == EmailOutboxStatus.Processing ? "active-token" : null,
                LockExpiresAt = status == EmailOutboxStatus.Processing
                    ? (isExpired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(5))
                    : null
            };
            context.EmailOutboxMessages.Add(msg);
            await context.SaveChangesAsync();

            var result = await service.RetryOutboxEmailAsync(msg.Id, "admin-1", "Audit Admin");

            Assert.Equal(expectSuccess, result.Success);

            var fresh = await context.EmailOutboxMessages.FindAsync(msg.Id);
            Assert.NotNull(fresh);

            if (expectSuccess)
            {
                Assert.Equal(EmailOutboxStatus.Queued, fresh.Status);
                Assert.Equal(0, fresh.AttemptCount);
                Assert.Null(fresh.LockToken);
                Assert.Null(fresh.LockExpiresAt);

                // Verified audit log written
                var audit = await context.AdminAuditLogs
                    .FirstOrDefaultAsync(a => a.EntityType == "EmailOutbox" && a.EntityId == msg.Id.ToString());
                Assert.NotNull(audit);
                Assert.Contains("Reclaimed", audit.Action);
            }
            else
            {
                Assert.Equal(status, fresh.Status);
            }
        }

        // =====================================================================
        // 4. CONCURRENT RETRY REQUESTS MUST QUEUE ONE RETRY ONLY (SQL SERVER)
        // =====================================================================
        [Fact]
        public async Task RetryOutboxEmail_ConcurrentRetryRequests_QueuesOneRetryOnly()
        {
            var dbName = "HamaraCommerce_RetryConcurrent_" + Guid.NewGuid().ToString("N");
            using var context = TestDbContextFactory.CreateSqlServerDbContext(dbName);
            try
            {
                var msg = new EmailOutboxMessage
                {
                    ToEmail = "concurrent.target@example.com",
                    Subject = "Important Order Notification",
                    HtmlBody = "<p>Test notification</p>",
                    Status = EmailOutboxStatus.Failed,
                    AttemptCount = 3,
                    LastError = "SMTP server dropped connection",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };
                context.EmailOutboxMessages.Add(msg);
                await context.SaveChangesAsync();

                var connStr = TestDbContextFactory.GetSqlServerConnectionString(dbName);

                // Run 10 concurrent retry operations against real SQL Server
                var tasks = Enumerable.Range(0, 10).Select(async i =>
                {
                    using var workerContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlServer(connStr).Options);
                    var workerService = new OperationalRecoveryService(workerContext, NullLogger<OperationalRecoveryService>.Instance);
                    return await workerService.RetryOutboxEmailAsync(msg.Id, $"admin-{i}", $"Admin {i}");
                }).ToList();

                var results = await Task.WhenAll(tasks);

                // Exactly ONE worker must succeed; all other 9 must receive non-success
                int successCount = results.Count(r => r.Success);
                int failCount = results.Count(r => !r.Success);

                Assert.Equal(1, successCount);
                Assert.Equal(9, failCount);

                // Verify the final database state: Queued once, attempt count 0
                using var verifyContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connStr).Options);
                var finalMsg = await verifyContext.EmailOutboxMessages.FindAsync(msg.Id);
                Assert.NotNull(finalMsg);
                Assert.Equal(EmailOutboxStatus.Queued, finalMsg.Status);
                Assert.Equal(0, finalMsg.AttemptCount);
                Assert.Null(finalMsg.LockToken);
            }
            finally
            {
                context.Database.EnsureDeleted();
            }
        }

        // =====================================================================
        // 5. PAYMENT RECONCILIATION - REJECT BLIND BUTTON CLICK / MISSING OUTCOME
        // =====================================================================
        [Fact]
        public async Task ReconcilePayment_ButtonClickWithoutOutcome_RejectsAndKeepsUnresolved()
        {
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var (controller, _, _) = CreateAdminController(context);

            var record = new CheckoutIdempotencyRecord
            {
                IdempotencyKey = "REC-KEY-" + Guid.NewGuid().ToString("N"),
                CustomerEmail = "buyer@example.com",
                Status = IdempotencyStatus.RecoveryRequired,
                PaymentReference = "JAZZ-UNCERTAIN-991",
                PaymentAmount = 7500m,
                FailureReason = "Payment gateway response timed out before commit."
            };
            context.CheckoutIdempotencyRecords.Add(record);
            await context.SaveChangesAsync();

            // Act: Administrator merely clicks legacy "MarkResolved" button with no explicit outcome, notes, or evidence
            var actionResult = await controller.ReconcilePayment(
                id: record.Id,
                outcome: null,
                action: "MarkResolved",
                adminNotes: null,
                providerReference: null);

            var redirect = Assert.IsType<RedirectToActionResult>(actionResult);
            Assert.Equal(nameof(AdminController.OperationalRecovery), redirect.ActionName);

            // Assert: Record remains unresolved in RecoveryRequired status
            var fresh = await context.CheckoutIdempotencyRecords.FindAsync(record.Id);
            Assert.NotNull(fresh);
            Assert.Equal(IdempotencyStatus.RecoveryRequired, fresh.Status); // NOT marked Completed!
            Assert.Null(fresh.PaymentStatus); // Untouched!

            Assert.True(controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Contains("cannot be marked completed merely by clicking a button", controller.TempData["ErrorMessage"]?.ToString() ?? "");
        }

        // =====================================================================
        // 6. PAYMENT RECONCILIATION - REQUIRE NOTES AND EVIDENCE
        // =====================================================================
        [Theory]
        [InlineData(null, "JAZZ-REAL-123", "administrator notes")] // Missing notes
        [InlineData("   ", "JAZZ-REAL-123", "administrator notes")]
        [InlineData("Verified", null, "reference evidence")] // Missing provider reference
        [InlineData("Verified", "   ", "reference evidence")]
        [InlineData("Verified", "test", "reference evidence")] // Dummy reference
        [InlineData("Verified", "0", "reference evidence")] // Placeholder reference
        [InlineData("Verified", "fake", "reference evidence")]
        public async Task ReconcilePayment_MissingNotesOrEvidence_RejectsAndKeepsUnresolved(string? notes, string? providerRef, string expectedErrorSubstr)
        {
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = new OperationalRecoveryService(context, NullLogger<OperationalRecoveryService>.Instance);

            var record = new CheckoutIdempotencyRecord
            {
                IdempotencyKey = "REC-VAL-" + Guid.NewGuid().ToString("N"),
                CustomerEmail = "customer@example.com",
                Status = IdempotencyStatus.RecoveryRequired,
                PaymentReference = "PENDING-REF",
                PaymentAmount = 3000m
            };
            context.CheckoutIdempotencyRecords.Add(record);
            await context.SaveChangesAsync();

            var request = new PaymentReconciliationRequest
            {
                Id = record.Id,
                Outcome = "Paid",
                AdminNotes = notes,
                ProviderReference = providerRef
            };

            var result = await service.ReconcilePaymentAsync(request, "admin-1", "Admin Name");

            Assert.False(result.Success);
            Assert.Contains(expectedErrorSubstr, result.Message, StringComparison.OrdinalIgnoreCase);

            var fresh = await context.CheckoutIdempotencyRecords.FindAsync(record.Id);
            Assert.NotNull(fresh);
            Assert.Equal(IdempotencyStatus.RecoveryRequired, fresh.Status); // Remained unresolved!
        }

        // =====================================================================
        // 7. PAYMENT RECONCILIATION - VALID OUTCOMES WITH EVIDENCE AND OLD/NEW AUDIT
        // =====================================================================
        [Fact]
        public async Task ReconcilePayment_ExplicitPaid_WithEvidenceAndNotes_UpdatesRecordAndOrder_RecordsAuditWithOldAndNewStates()
        {
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = new OperationalRecoveryService(context, NullLogger<OperationalRecoveryService>.Instance);

            var order = new Order
            {
                OrderNumber = "HC-REC-ORDER-01",
                CustomerName = "Ahmed Bilawal",
                CustomerEmail = "ahmed@example.com",
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                TotalAmount = 8500m
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var record = new CheckoutIdempotencyRecord
            {
                IdempotencyKey = "KEY-" + Guid.NewGuid().ToString("N"),
                CustomerEmail = order.CustomerEmail,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = IdempotencyStatus.RecoveryRequired,
                PaymentStatus = PaymentStatus.Pending,
                PaymentAmount = 8500m,
                PaymentProvider = "JazzCash",
                FailureReason = "Uncertain gateway response timeout."
            };
            context.CheckoutIdempotencyRecords.Add(record);
            await context.SaveChangesAsync();

            var request = new PaymentReconciliationRequest
            {
                Id = record.Id,
                Outcome = "Paid",
                AdminNotes = "Confirmed receipt in JazzCash merchant dashboard with statement TID #JC-9821038.",
                ProviderReference = "JC-9821038",
                ProviderName = "JazzCash"
            };

            var result = await service.ReconcilePaymentAsync(request, "admin-42", "Sarah Jenkins");

            // Assert: Reconciliation succeeded
            Assert.True(result.Success);
            Assert.Equal(IdempotencyStatus.RecoveryRequired, result.OldStatus);
            Assert.Equal(IdempotencyStatus.Completed, result.NewStatus);
            Assert.Equal(PaymentStatus.Pending, result.OldPaymentStatus);
            Assert.Equal(PaymentStatus.Paid, result.NewPaymentStatus);

            // Assert: Record updated truthfully
            var freshRecord = await context.CheckoutIdempotencyRecords.FindAsync(record.Id);
            Assert.NotNull(freshRecord);
            Assert.Equal(IdempotencyStatus.Completed, freshRecord.Status);
            Assert.Equal(PaymentStatus.Paid, freshRecord.PaymentStatus);
            Assert.Equal("JC-9821038", freshRecord.PaymentReference);
            Assert.Contains("Sarah Jenkins", freshRecord.FailureReason ?? "");
            Assert.Contains("JC-9821038", freshRecord.FailureReason ?? "");

            // Assert: Associated order updated
            var freshOrder = await context.Orders.Include(o => o.Payments).FirstAsync(o => o.Id == order.Id);
            Assert.Equal(PaymentStatus.Paid, freshOrder.PaymentStatus);
            Assert.Equal(OrderStatus.Confirmed, freshOrder.Status);

            // Verified PaymentTransaction was generated
            var tx = freshOrder.Payments.FirstOrDefault(p => p.TransactionReference == "JC-9821038");
            Assert.NotNull(tx);
            Assert.Equal(8500m, tx.Amount);
            Assert.Equal(PaymentStatus.Paid, tx.Status);

            // Assert: Audit log records identity, timestamp, and old/new states
            var audit = await context.AdminAuditLogs
                .FirstOrDefaultAsync(a => a.EntityType == "CheckoutIdempotency" && a.EntityId == record.IdempotencyKey);
            Assert.NotNull(audit);
            Assert.Equal("admin-42", audit.AdminUserId);
            Assert.Equal("Sarah Jenkins", audit.AdminUserName);
            Assert.Contains("OldStatus=RecoveryRequired", audit.Details);
            Assert.Contains("NewStatus=Completed", audit.Details);
            Assert.Contains("OldPaymentStatus=Pending", audit.Details);
            Assert.Contains("NewPaymentStatus=Paid", audit.Details);
            Assert.Contains("JC-9821038", audit.Details);
        }

        [Theory]
        [InlineData("Failed", IdempotencyStatus.Failed, PaymentStatus.Failed, OrderStatus.Cancelled)]
        [InlineData("Cancelled", IdempotencyStatus.Failed, PaymentStatus.Cancelled, OrderStatus.Cancelled)]
        [InlineData("Refunded", IdempotencyStatus.Failed, PaymentStatus.Refunded, OrderStatus.Refunded)]
        public async Task ReconcilePayment_ExplicitFailedCancelledRefunded_UpdatesRecordAndOrderTruthfully(
            string outcome,
            IdempotencyStatus expectedRecordStatus,
            PaymentStatus expectedPaymentStatus,
            OrderStatus expectedOrderStatus)
        {
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = new OperationalRecoveryService(context, NullLogger<OperationalRecoveryService>.Instance);

            var order = new Order
            {
                OrderNumber = "HC-ORD-" + Guid.NewGuid().ToString("N")[..6],
                CustomerName = "Hamza Ali",
                CustomerEmail = "hamza@example.com",
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                TotalAmount = 4000m
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var record = new CheckoutIdempotencyRecord
            {
                IdempotencyKey = "KEY-" + Guid.NewGuid().ToString("N"),
                CustomerEmail = order.CustomerEmail,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = IdempotencyStatus.RecoveryRequired,
                PaymentAmount = 4000m
            };
            context.CheckoutIdempotencyRecords.Add(record);
            await context.SaveChangesAsync();

            var request = new PaymentReconciliationRequest
            {
                Id = record.Id,
                Outcome = outcome,
                AdminNotes = $"Operational review completed: verified {outcome} with provider bank.",
                ProviderReference = "EP-DEC-88192"
            };

            var result = await service.ReconcilePaymentAsync(request, "admin-ops", "Ops Lead");

            Assert.True(result.Success);

            var freshRecord = await context.CheckoutIdempotencyRecords.FindAsync(record.Id);
            Assert.NotNull(freshRecord);
            Assert.Equal(expectedRecordStatus, freshRecord.Status);
            Assert.Equal(expectedPaymentStatus, freshRecord.PaymentStatus);

            var freshOrder = await context.Orders.FindAsync(order.Id);
            Assert.NotNull(freshOrder);
            Assert.Equal(expectedOrderStatus, freshOrder.Status);
            Assert.Equal(expectedPaymentStatus, freshOrder.PaymentStatus);
        }
    }
}
