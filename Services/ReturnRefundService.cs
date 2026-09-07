using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class ReturnRefundService : IReturnRefundService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly IEmailOutboxService? _emailOutboxService;
        private readonly ILogger<ReturnRefundService> _logger;

        public ReturnRefundService(
            ApplicationDbContext context,
            IPricingService pricingService,
            ILogger<ReturnRefundService> logger,
            IEmailOutboxService? emailOutboxService = null)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
            _emailOutboxService = emailOutboxService;
        }

        public Task<ReturnOperationResult> RequestReturnAsync(string orderNumber, string reason, string? customerUserId = null)
        {
            return CommerceDatabaseWork.TransactionAsync<ReturnOperationResult>(_context, async () =>
            {
                if (string.IsNullOrWhiteSpace(orderNumber))
                    return ReturnOperationResult.Fail("Order number is required.");

                if (string.IsNullOrWhiteSpace(reason))
                    return ReturnOperationResult.Fail("A reason for the return request is required.");

                var order = await _context.Orders
                    .Include(o => o.Items)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim());

                if (order == null)
                    return ReturnOperationResult.Fail($"Order #{orderNumber} not found.");

                if (!string.IsNullOrWhiteSpace(customerUserId) && order.UserId != customerUserId)
                    return ReturnOperationResult.Fail("You do not have permission to initiate a return for this order.");

                if (order.Status != OrderStatus.Delivered)
                    return ReturnOperationResult.Fail("Return requests can only be initiated for delivered orders.", order);

                if ((DateTime.UtcNow - (order.DeliveredAt ?? order.OrderDate)).TotalDays > 30)
                    return ReturnOperationResult.Fail("The 30-day return window for this order has expired.", order);

                if (order.ReturnRequestedAt.HasValue)
                    return ReturnOperationResult.Ok(order, "Your return request is already awaiting review.", order.RefundStatus);

                string cleanReason = reason.Trim();
                order.ReturnRequestedAt = DateTime.UtcNow;
                order.ReturnReason = cleanReason;
                order.RefundStatus = ReturnStatus.Requested;
                order.ReturnInspectionState = ReturnInspectionState.AwaitingInspection;
                order.CustomerNotes = (order.CustomerNotes ?? "") + $" | Return requested by customer on {DateTime.UtcNow:yyyy-MM-dd}: {cleanReason}";

                var email = EmailOutboxService.CreateMessage(
                    order.CustomerEmail,
                    $"Return Request Received - Order #{order.OrderNumber}",
                    $"<p>Dear {order.CustomerName},</p><p>We have received your return request for order <strong>#{order.OrderNumber}</strong>.</p><p>Reason: {cleanReason}</p><p>Our team will review your request shortly.</p>",
                    eventKey: $"ReturnRequested_{order.OrderNumber}");
                _context.EmailOutboxMessages.Add(email);

                _context.AdminAuditLogs.Add(new AdminAuditLog
                {
                    AdminUserId = customerUserId ?? "Customer",
                    AdminUserName = order.CustomerName,
                    Action = "ReturnRequested",
                    EntityType = "Order",
                    EntityId = order.OrderNumber,
                    Details = $"Customer requested return for order #{order.OrderNumber}. Reason: {cleanReason}",
                    CreatedAt = DateTime.UtcNow
                });

                return ReturnOperationResult.Ok(order, $"Return request submitted for Order #{order.OrderNumber}.", ReturnStatus.Requested);
            });
        }

        public Task<ReturnOperationResult> ApproveReturnAsync(int orderId, string? adminNotes = null, string? adminUserId = null, string? adminUserName = null)
        {
            return CommerceDatabaseWork.TransactionAsync<ReturnOperationResult>(_context, async () =>
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return ReturnOperationResult.Fail($"Order with ID {orderId} not found.");

                if (order.Status == OrderStatus.Cancelled)
                    return ReturnOperationResult.Fail($"Cannot approve return for cancelled order #{order.OrderNumber}.", order);

                // Idempotency: If already in Approved, Inspected, RefundPending, or Completed state, don't duplicate
                if (order.RefundStatus == ReturnStatus.Approved ||
                    order.RefundStatus == ReturnStatus.Inspected ||
                    order.RefundStatus == ReturnStatus.RefundPending ||
                    order.RefundStatus == ReturnStatus.Completed)
                {
                    return ReturnOperationResult.Ok(order, $"Return for order #{order.OrderNumber} is already approved.", order.RefundStatus);
                }

                order.RefundStatus = ReturnStatus.Approved;
                order.ReturnInspectionState = string.IsNullOrWhiteSpace(order.ReturnInspectionState)
                    ? ReturnInspectionState.AwaitingInspection
                    : order.ReturnInspectionState;
                order.ReturnProcessedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(adminNotes))
                    order.ReturnAdminNotes = adminNotes.Trim();

                // Truthful governance: PaymentStatus and OrderStatus remain intact (never auto-marked refunded)
                // No synthetic fake refund reference generated here

                var email = EmailOutboxService.CreateMessage(
                    order.CustomerEmail,
                    $"Return Request Approved - Order #{order.OrderNumber}",
                    $"<p>Dear {order.CustomerName},</p><p>Your return request for order <strong>#{order.OrderNumber}</strong> has been approved. Please prepare your package for courier pickup or dispatch for physical condition inspection.</p>",
                    eventKey: $"ReturnApproved_{order.OrderNumber}");
                _context.EmailOutboxMessages.Add(email);

                _context.AdminAuditLogs.Add(new AdminAuditLog
                {
                    AdminUserId = adminUserId ?? "System",
                    AdminUserName = adminUserName ?? "Administrator",
                    Action = "ReturnApproved",
                    EntityType = "Order",
                    EntityId = order.OrderNumber,
                    Details = $"Approved return request for order #{order.OrderNumber}. Notes: {order.ReturnAdminNotes}",
                    CreatedAt = DateTime.UtcNow
                });

                return ReturnOperationResult.Ok(order, $"Return for order #{order.OrderNumber} approved and awaiting physical inspection.", ReturnStatus.Approved);
            });
        }

        public Task<ReturnOperationResult> InspectReturnAsync(int orderId, string inspectionState, string? adminNotes = null, string? adminUserId = null, string? adminUserName = null)
        {
            return CommerceDatabaseWork.TransactionAsync<ReturnOperationResult>(_context, async () =>
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return ReturnOperationResult.Fail($"Order with ID {orderId} not found.");

                if (order.Status == OrderStatus.Cancelled)
                    return ReturnOperationResult.Fail($"Cannot inspect cancelled order #{order.OrderNumber}.", order);

                if (order.RefundStatus == ReturnStatus.Completed)
                    return ReturnOperationResult.Ok(order, $"Refund for order #{order.OrderNumber} is already completed.", ReturnStatus.Completed);

                string cleanInspection = string.IsNullOrWhiteSpace(inspectionState)
                    ? ReturnInspectionState.PassedInspection
                    : inspectionState.Trim();

                order.ReturnInspectionState = cleanInspection;
                order.ReturnProcessedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(adminNotes))
                    order.ReturnAdminNotes = adminNotes.Trim();

                if (cleanInspection.Equals(ReturnInspectionState.RejectedInspection, StringComparison.OrdinalIgnoreCase))
                {
                    order.RefundStatus = ReturnStatus.Rejected;

                    var rejEmail = EmailOutboxService.CreateMessage(
                        order.CustomerEmail,
                        $"Return Request Declined - Order #{order.OrderNumber}",
                        $"<p>Dear {order.CustomerName},</p><p>Your return request for order <strong>#{order.OrderNumber}</strong> has been declined following physical inspection.</p><p>Reason: {order.ReturnAdminNotes ?? "Failed inspection criteria."}</p>",
                        eventKey: $"ReturnRejected_{order.OrderNumber}");
                    _context.EmailOutboxMessages.Add(rejEmail);

                    _context.AdminAuditLogs.Add(new AdminAuditLog
                    {
                        AdminUserId = adminUserId ?? "System",
                        AdminUserName = adminUserName ?? "Administrator",
                        Action = "ReturnInspectionFailed",
                        EntityType = "Order",
                        EntityId = order.OrderNumber,
                        Details = $"Return inspection rejected for order #{order.OrderNumber}. Reason: {order.ReturnAdminNotes}",
                        CreatedAt = DateTime.UtcNow
                    });

                    return ReturnOperationResult.Ok(order, $"Return inspection for order #{order.OrderNumber} was rejected.", ReturnStatus.Rejected);
                }

                // Passed or transit damage inspection moves to RefundPending (truthful manual remittance state for COD / test card)
                order.RefundStatus = ReturnStatus.RefundPending;

                var insEmail = EmailOutboxService.CreateMessage(
                    order.CustomerEmail,
                    $"Return Inspected - Order #{order.OrderNumber}",
                    $"<p>Dear {order.CustomerName},</p><p>Your returned items for order <strong>#{order.OrderNumber}</strong> have been verified ({cleanInspection}). Your refund is now pending manual remittance.</p>",
                    eventKey: $"ReturnInspected_{order.OrderNumber}");
                _context.EmailOutboxMessages.Add(insEmail);

                _context.AdminAuditLogs.Add(new AdminAuditLog
                {
                    AdminUserId = adminUserId ?? "System",
                    AdminUserName = adminUserName ?? "Administrator",
                    Action = "ReturnInspected",
                    EntityType = "Order",
                    EntityId = order.OrderNumber,
                    Details = $"Return inspection recorded ({cleanInspection}) for order #{order.OrderNumber}. State moved to RefundPending.",
                    CreatedAt = DateTime.UtcNow
                });

                return ReturnOperationResult.Ok(order, $"Return inspection ({cleanInspection}) recorded. Order #{order.OrderNumber} moved to RefundPending.", ReturnStatus.RefundPending);
            });
        }

        public Task<ReturnOperationResult> CompleteRefundAsync(
            int orderId,
            string refundTransactionReference,
            decimal? refundAmount = null,
            string? refundMethod = null,
            bool restock = true,
            string? adminNotes = null,
            string? adminUserId = null,
            string? adminUserName = null)
        {
            return CommerceDatabaseWork.TransactionAsync<ReturnOperationResult>(_context, async () =>
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return ReturnOperationResult.Fail($"Order with ID {orderId} not found.");

                if (order.Status == OrderStatus.Cancelled)
                    return ReturnOperationResult.Fail($"Cannot refund cancelled order #{order.OrderNumber}.", order);

                // 1. Validate real reference
                if (string.IsNullOrWhiteSpace(refundTransactionReference))
                    return ReturnOperationResult.Fail("A genuine refund transaction reference (e.g. bank transfer IBAN ref, EasyPaisa/JazzCash transaction ID) is required.");

                string trimmedRef = refundTransactionReference.Trim();
                if (trimmedRef.Length < 4 || IsDummyReference(trimmedRef))
                    return ReturnOperationResult.Fail($"Reference '{trimmedRef}' is not a valid remittance reference. A genuine transaction reference is required.");

                // 2. Idempotency check: Already completed with same reference
                if (order.RefundStatus == ReturnStatus.Completed && string.Equals(order.RefundTransactionReference, trimmedRef, StringComparison.OrdinalIgnoreCase))
                {
                    return ReturnOperationResult.Ok(order, $"Refund with reference '{trimmedRef}' has already been completed for order #{order.OrderNumber}.", ReturnStatus.Completed);
                }

                if (order.Payments.Any(p => string.Equals(p.TransactionReference, trimmedRef, StringComparison.OrdinalIgnoreCase) && p.Status == PaymentStatus.Refunded))
                {
                    return ReturnOperationResult.Ok(order, $"Payment refund transaction '{trimmedRef}' has already been recorded.", ReturnStatus.Completed);
                }

                // 3. Validate refund amount
                decimal alreadyRefunded = order.Payments.Where(p => p.Status == PaymentStatus.Refunded).Sum(p => p.Amount);
                decimal refundableAmount = order.TotalAmount - alreadyRefunded;

                if (refundableAmount <= 0)
                    return ReturnOperationResult.Fail($"Order #{order.OrderNumber} has already been fully refunded.");

                decimal finalRefund = refundAmount ?? refundableAmount;

                if (finalRefund <= 0)
                    return ReturnOperationResult.Fail("Refund amount must be strictly greater than zero.");

                if (finalRefund > refundableAmount)
                    return ReturnOperationResult.Fail($"Refund amount (Rs. {finalRefund:N2}) cannot exceed the maximum refundable amount (Rs. {refundableAmount:N2}).");

                // 4. Restocking: Exactly once
                if (restock && !order.IsRestockedOnReturn)
                {
                    foreach (var item in order.Items)
                    {
                        var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            int oldStock = product.Stock;
                            product.Stock += item.Quantity;
                            int newStock = product.Stock;

                            if (item.VariantId.HasValue)
                            {
                                var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId.Value);
                                if (variant != null)
                                {
                                    variant.Stock += item.Quantity;
                                }
                            }

                            _context.InventoryMovements.Add(new InventoryMovement
                            {
                                ProductId = product.Id,
                                VariantId = item.VariantId,
                                OrderId = order.Id,
                                MovementType = InventoryMovementType.ReturnRestoration,
                                QuantityChange = item.Quantity,
                                OldStock = oldStock,
                                NewStock = newStock,
                                Reason = $"Restocked from completed refund of order #{order.OrderNumber} (Ref: {trimmedRef})",
                                AdminUserId = adminUserId ?? "System",
                                AdminUserName = adminUserName ?? "Administrator",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    order.IsRestockedOnReturn = true;
                }

                // 5. Update state truthfully
                string cleanMethod = string.IsNullOrWhiteSpace(refundMethod) ? order.PaymentMethod : refundMethod.Trim();
                order.RefundStatus = ReturnStatus.Completed;
                order.RefundTransactionReference = trimmedRef;
                order.RefundAmount = (order.RefundAmount ?? 0) + finalRefund;
                order.RefundMethod = cleanMethod;
                order.ReturnProcessedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(adminNotes))
                    order.ReturnAdminNotes = adminNotes.Trim();

                bool isFullRefund = (alreadyRefunded + finalRefund) >= order.TotalAmount;
                if (isFullRefund)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                    order.Status = OrderStatus.Refunded;
                    await _pricingService.RestoreCouponRedemptionAsync(order);
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.PartiallyRefunded;
                }

                // 6. Record PaymentTransaction
                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    OrderId = order.Id,
                    TransactionReference = trimmedRef,
                    Provider = cleanMethod,
                    PaymentMethod = order.PaymentMethod,
                    Amount = finalRefund,
                    Currency = order.Currency,
                    Status = PaymentStatus.Refunded,
                    CreatedAt = DateTime.UtcNow
                });

                // 7. Queue email in same transaction
                var outboxMsg = EmailOutboxService.CreateMessage(
                    order.CustomerEmail,
                    $"Refund Completed - Order #{order.OrderNumber}",
                    $"<p>Dear {order.CustomerName},</p><p>Your refund of <strong>Rs. {finalRefund:N2}</strong> for order <strong>#{order.OrderNumber}</strong> has been remitted via {cleanMethod}.</p><p>Transaction Reference: <strong>{trimmedRef}</strong></p>",
                    eventKey: $"RefundCompleted_{order.OrderNumber}_{trimmedRef}");
                _context.EmailOutboxMessages.Add(outboxMsg);

                // 8. Audit log in same transaction
                _context.AdminAuditLogs.Add(new AdminAuditLog
                {
                    AdminUserId = adminUserId ?? "System",
                    AdminUserName = adminUserName ?? "Administrator",
                    Action = "RefundCompleted",
                    EntityType = "Order",
                    EntityId = order.OrderNumber,
                    Details = $"Completed refund for order #{order.OrderNumber}. Amount: Rs. {finalRefund:N2}, Ref: {trimmedRef}, Method: {cleanMethod}, Restocked: {order.IsRestockedOnReturn}",
                    CreatedAt = DateTime.UtcNow
                });

                return ReturnOperationResult.Ok(order, $"Refund of Rs. {finalRefund:N2} recorded successfully (Ref: {trimmedRef}).", ReturnStatus.Completed);
            });
        }

        public Task<ReturnOperationResult> RejectReturnAsync(int orderId, string? inspectionState = null, string? adminNotes = null, string? adminUserId = null, string? adminUserName = null)
        {
            return CommerceDatabaseWork.TransactionAsync<ReturnOperationResult>(_context, async () =>
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return ReturnOperationResult.Fail($"Order with ID {orderId} not found.");

                order.RefundStatus = ReturnStatus.Rejected;
                order.ReturnInspectionState = string.IsNullOrWhiteSpace(inspectionState)
                    ? ReturnInspectionState.RejectedInspection
                    : inspectionState.Trim();
                order.ReturnProcessedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(adminNotes))
                    order.ReturnAdminNotes = adminNotes.Trim();

                var rejEmail = EmailOutboxService.CreateMessage(
                    order.CustomerEmail,
                    $"Return Request Declined - Order #{order.OrderNumber}",
                    $"<p>Dear {order.CustomerName},</p><p>Your return request for order <strong>#{order.OrderNumber}</strong> has been reviewed and declined.</p><p>Reason: {order.ReturnAdminNotes ?? "Policy decline."}</p>",
                    eventKey: $"ReturnRejected_{order.OrderNumber}");
                _context.EmailOutboxMessages.Add(rejEmail);

                _context.AdminAuditLogs.Add(new AdminAuditLog
                {
                    AdminUserId = adminUserId ?? "System",
                    AdminUserName = adminUserName ?? "Administrator",
                    Action = "ReturnRejected",
                    EntityType = "Order",
                    EntityId = order.OrderNumber,
                    Details = $"Declined return for order #{order.OrderNumber}. Reason: {order.ReturnAdminNotes}",
                    CreatedAt = DateTime.UtcNow
                });

                return ReturnOperationResult.Ok(order, $"Return request for order #{order.OrderNumber} has been rejected.", ReturnStatus.Rejected);
            });
        }

        private static bool IsDummyReference(string reference)
        {
            var lower = reference.Trim().ToLowerInvariant();
            return lower is "test" or "fake" or "dummy" or "n/a" or "na" or "none" or "null" or "0" or "0000" or "ref-" or "ref-0000";
        }
    }
}
