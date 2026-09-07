using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;

namespace HamaraCommerce.Services
{
    public class OperationalRecoveryService : IOperationalRecoveryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OperationalRecoveryService> _logger;
        private static readonly object _inMemoryLock = new();

        public OperationalRecoveryService(ApplicationDbContext context, ILogger<OperationalRecoveryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OutboxRetryResult> RetryOutboxEmailAsync(
            long id,
            string? adminUserId = null,
            string? adminUserName = null,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Rule: Check existing record state
            var existing = await _context.EmailOutboxMessages.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (existing == null)
            {
                return OutboxRetryResult.Failed($"Outbox email #{id} not found.");
            }

            // Rule: RetryOutboxEmail must never requeue a message already marked Sent.
            if (existing.Status == EmailOutboxStatus.Sent)
            {
                _logger.LogWarning("Rejection: Attempted to requeue already Sent outbox message #{MessageId}", id);
                return OutboxRetryResult.Failed(
                    $"Outbox email #{id} has already been marked Sent and cannot be requeued.",
                    alreadySent: true,
                    priorStatus: EmailOutboxStatus.Sent);
            }

            // Rule: Reclaim only Failed, Blocked or expired Processing messages using an atomic conditional update.
            int updatedCount = 0;
            if (_context.Database.IsRelational())
            {
                // Atomic SQL update guarantees only one concurrent worker/retry request succeeds
                updatedCount = await _context.EmailOutboxMessages
                    .Where(e => e.Id == id &&
                               (e.Status == EmailOutboxStatus.Failed ||
                                e.Status == EmailOutboxStatus.Blocked ||
                               (e.Status == EmailOutboxStatus.Processing && (e.LockExpiresAt == null || e.LockExpiresAt <= now))))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.Status, EmailOutboxStatus.Queued)
                        .SetProperty(e => e.AttemptCount, 0)
                        .SetProperty(e => e.NextAttemptAt, now)
                        .SetProperty(e => e.LockToken, (string?)null)
                        .SetProperty(e => e.LockExpiresAt, (DateTime?)null), cancellationToken);
            }
            else
            {
                // Thread-safe in-memory synchronization for unit testing
                lock (_inMemoryLock)
                {
                    var target = _context.EmailOutboxMessages
                        .FirstOrDefault(e => e.Id == id &&
                                       (e.Status == EmailOutboxStatus.Failed ||
                                        e.Status == EmailOutboxStatus.Blocked ||
                                       (e.Status == EmailOutboxStatus.Processing && (e.LockExpiresAt == null || e.LockExpiresAt <= now))));
                    if (target != null)
                    {
                        target.Status = EmailOutboxStatus.Queued;
                        target.AttemptCount = 0;
                        target.NextAttemptAt = now;
                        target.LockToken = null;
                        target.LockExpiresAt = null;
                        _context.SaveChanges();
                        updatedCount = 1;
                    }
                }
            }

            // If 0 rows were updated, determine reason for accurate truthful response
            if (updatedCount == 0)
            {
                var current = await _context.EmailOutboxMessages.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
                var currentStatus = current?.Status ?? existing.Status;

                if (currentStatus == EmailOutboxStatus.Sent)
                {
                    return OutboxRetryResult.Failed(
                        $"Outbox email #{id} has already been marked Sent and cannot be requeued.",
                        alreadySent: true,
                        priorStatus: EmailOutboxStatus.Sent);
                }

                if (currentStatus == EmailOutboxStatus.Queued)
                {
                    return OutboxRetryResult.Failed(
                        $"Outbox email #{id} is already queued for dispatch.",
                        priorStatus: EmailOutboxStatus.Queued);
                }

                if (currentStatus == EmailOutboxStatus.Processing)
                {
                    return OutboxRetryResult.Failed(
                        $"Outbox email #{id} is currently being dispatched under an active worker lease.",
                        priorStatus: EmailOutboxStatus.Processing);
                }

                return OutboxRetryResult.Failed(
                    $"Outbox email #{id} is not in a reclaimable state (Current Status: {currentStatus}).",
                    priorStatus: currentStatus);
            }

            // Record administrator audit log with identity, timestamp, and old/new states
            var reloaded = await _context.EmailOutboxMessages.FindAsync(new object[] { id }, cancellationToken);
            _context.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminUserId = adminUserId ?? "System",
                AdminUserName = adminUserName ?? "Administrator",
                Action = "EmailOutboxReclaimed",
                EntityType = "EmailOutbox",
                EntityId = id.ToString(),
                Details = $"Reclaimed outbox email #{id} (To: {existing.ToEmail}) from OldStatus={existing.Status} to NewStatus={EmailOutboxStatus.Queued}. Reset attempt count to 0.",
                CreatedAt = now
            });
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Outbox email #{MessageId} successfully reclaimed and reset to Queued by {AdminUser}", id, adminUserName ?? "Administrator");
            return OutboxRetryResult.Succeeded(reloaded ?? existing, existing.Status);
        }

        public Task<PaymentReconciliationResult> ReconcilePaymentAsync(
            PaymentReconciliationRequest request,
            string? adminUserId = null,
            string? adminUserName = null,
            CancellationToken cancellationToken = default)
        {
            return CommerceDatabaseWork.TransactionAsync<PaymentReconciliationResult>(_context, async () =>
            {
                var record = await _context.CheckoutIdempotencyRecords.FindAsync(new object[] { request.Id }, cancellationToken);
                if (record == null)
                {
                    return PaymentReconciliationResult.Failed($"Checkout payment record #{request.Id} not found.");
                }

                // Rule: Do not mark a payment checkout record Completed merely because an administrator clicked a button.
                // Require a reconciliation outcome, administrator notes and provider/reference evidence.
                // Keep uncertain payments unresolved until explicitly recorded as paid, failed, cancelled or refunded.
                if (string.IsNullOrWhiteSpace(request.Outcome))
                {
                    return PaymentReconciliationResult.Failed(
                        "Payment checkout records cannot be resolved merely by clicking a button. An explicit reconciliation outcome (Paid, Failed, Cancelled, or Refunded), administrator notes, and provider/reference evidence are strictly required. Uncertain payments remain unresolved.",
                        record);
                }

                string normalizedOutcome = NormalizeOutcome(request.Outcome);
                if (normalizedOutcome == "Unknown")
                {
                    return PaymentReconciliationResult.Failed(
                        $"Outcome '{request.Outcome}' is invalid. Allowed reconciliation outcomes are: Paid, Failed, Cancelled, or Refunded. Uncertain payments remain unresolved.",
                        record);
                }

                // Validate administrator notes
                if (string.IsNullOrWhiteSpace(request.AdminNotes) || request.AdminNotes.Trim().Length < 5)
                {
                    return PaymentReconciliationResult.Failed(
                        "Detailed administrator notes (at least 5 characters) explaining the reconciliation findings are required.",
                        record);
                }

                // Validate provider / reference evidence
                if (string.IsNullOrWhiteSpace(request.ProviderReference) || request.ProviderReference.Trim().Length < 4 || IsDummyReference(request.ProviderReference))
                {
                    return PaymentReconciliationResult.Failed(
                        "Genuine provider transaction reference evidence (e.g. gateway transaction reference, bank transfer reference, or Raast reference) is required.",
                        record);
                }

                string cleanNotes = request.AdminNotes.Trim();
                string cleanRef = request.ProviderReference.Trim();
                string? cleanProvider = string.IsNullOrWhiteSpace(request.ProviderName) ? record.PaymentProvider : request.ProviderName.Trim();

                // Idempotency check: Already reconciled with same outcome and reference
                if (IsAlreadyReconciledWithSameData(record, normalizedOutcome, cleanRef))
                {
                    return PaymentReconciliationResult.Succeeded(
                        record,
                        record.Status,
                        record.Status,
                        record.PaymentStatus,
                        record.PaymentStatus,
                        $"Payment record #{record.Id} is already reconciled as {normalizedOutcome} with reference '{cleanRef}'.");
                }

                var oldStatus = record.Status;
                var oldPaymentStatus = record.PaymentStatus;
                var now = DateTime.UtcNow;

                // Load associated order if available
                Order? associatedOrder = null;
                if (record.OrderId.HasValue)
                {
                    associatedOrder = await _context.Orders
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.Id == record.OrderId.Value, cancellationToken);
                }
                else if (!string.IsNullOrWhiteSpace(record.OrderNumber))
                {
                    associatedOrder = await _context.Orders
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.OrderNumber == record.OrderNumber, cancellationToken);
                }

                // Apply explicit outcome states (NEVER call external payment gateway during manual reconciliation)
                switch (normalizedOutcome)
                {
                    case "Paid":
                        record.Status = IdempotencyStatus.Completed;
                        record.PaymentStatus = PaymentStatus.Paid;

                        if (associatedOrder != null)
                        {
                            associatedOrder.PaymentStatus = PaymentStatus.Paid;
                            if (associatedOrder.Status == OrderStatus.Pending)
                            {
                                associatedOrder.Status = OrderStatus.Confirmed;
                            }

                            // Record PaymentTransaction if not already present
                            bool txExists = associatedOrder.Payments.Any(p =>
                                string.Equals(p.TransactionReference, cleanRef, StringComparison.OrdinalIgnoreCase) &&
                                p.Status == PaymentStatus.Paid);

                            if (!txExists)
                            {
                                associatedOrder.Payments.Add(new PaymentTransaction
                                {
                                    OrderId = associatedOrder.Id,
                                    TransactionReference = cleanRef,
                                    Provider = cleanProvider ?? "ManualReconciliation",
                                    PaymentMethod = associatedOrder.PaymentMethod,
                                    Amount = record.PaymentAmount ?? associatedOrder.TotalAmount,
                                    Currency = associatedOrder.Currency,
                                    Status = PaymentStatus.Paid,
                                    CreatedAt = now
                                });
                            }
                        }
                        break;

                    case "Failed":
                        record.Status = IdempotencyStatus.Failed;
                        record.PaymentStatus = PaymentStatus.Failed;

                        if (associatedOrder != null)
                        {
                            associatedOrder.PaymentStatus = PaymentStatus.Failed;
                            if (associatedOrder.Status == OrderStatus.Pending)
                            {
                                associatedOrder.Status = OrderStatus.Cancelled;
                            }
                        }
                        break;

                    case "Cancelled":
                        record.Status = IdempotencyStatus.Failed;
                        record.PaymentStatus = PaymentStatus.Cancelled;

                        if (associatedOrder != null)
                        {
                            associatedOrder.PaymentStatus = PaymentStatus.Cancelled;
                            if (associatedOrder.Status == OrderStatus.Pending)
                            {
                                associatedOrder.Status = OrderStatus.Cancelled;
                            }
                        }
                        break;

                    case "Refunded":
                        record.Status = IdempotencyStatus.Failed;
                        record.PaymentStatus = PaymentStatus.Refunded;

                        if (associatedOrder != null)
                        {
                            associatedOrder.PaymentStatus = PaymentStatus.Refunded;
                            associatedOrder.Status = OrderStatus.Refunded;
                        }
                        break;
                }

                // Update evidence and record metadata
                record.PaymentReference = cleanRef;
                if (!string.IsNullOrWhiteSpace(cleanProvider))
                {
                    record.PaymentProvider = cleanProvider;
                }
                record.FailureReason = $"Reconciled by {adminUserName ?? "Administrator"} ({adminUserId ?? "System"}) at {now:u}. Outcome: {normalizedOutcome}. Provider Ref: {cleanRef}. Notes: {cleanNotes} | Prior: Status={oldStatus}, PaymentStatus={oldPaymentStatus}";

                // Rule: Record administrator identity, timestamp and old/new states.
                _context.AdminAuditLogs.Add(new AdminAuditLog
                {
                    AdminUserId = adminUserId ?? "System",
                    AdminUserName = adminUserName ?? "Administrator",
                    Action = "PaymentReconciled",
                    EntityType = "CheckoutIdempotency",
                    EntityId = record.IdempotencyKey,
                    Details = $"Manual payment reconciliation for record #{record.Id} ({record.IdempotencyKey}). Admin={adminUserName} ({adminUserId}). Outcome={normalizedOutcome}. ProviderRef={cleanRef}. OldStatus={oldStatus}, NewStatus={record.Status}. OldPaymentStatus={oldPaymentStatus}, NewPaymentStatus={record.PaymentStatus}. Notes={cleanNotes}",
                    CreatedAt = now
                });

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Payment record #{RecordId} reconciled to {Outcome} by {AdminUser}. OldStatus={OldStatus}, NewStatus={NewStatus}",
                    record.Id, normalizedOutcome, adminUserName ?? "Administrator", oldStatus, record.Status);

                return PaymentReconciliationResult.Succeeded(
                    record,
                    oldStatus,
                    record.Status,
                    oldPaymentStatus,
                    record.PaymentStatus,
                    $"Payment record #{record.Id} successfully reconciled as {normalizedOutcome} (Ref: {cleanRef}).");
            });
        }

        private static string NormalizeOutcome(string outcome)
        {
            var clean = outcome.Trim().ToLowerInvariant();
            return clean switch
            {
                "paid" or "markpaid" or "completed" => "Paid",
                "failed" or "markfailed" => "Failed",
                "cancelled" or "canceled" or "markcancelled" => "Cancelled",
                "refunded" or "markrefunded" => "Refunded",
                _ => "Unknown"
            };
        }

        private static bool IsAlreadyReconciledWithSameData(CheckoutIdempotencyRecord record, string outcome, string reference)
        {
            if (!string.Equals(record.PaymentReference, reference, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return outcome switch
            {
                "Paid" => record.Status == IdempotencyStatus.Completed && record.PaymentStatus == PaymentStatus.Paid,
                "Failed" => record.Status == IdempotencyStatus.Failed && record.PaymentStatus == PaymentStatus.Failed,
                "Cancelled" => record.Status == IdempotencyStatus.Failed && record.PaymentStatus == PaymentStatus.Cancelled,
                "Refunded" => record.PaymentStatus == PaymentStatus.Refunded,
                _ => false
            };
        }

        private static bool IsDummyReference(string reference)
        {
            var lower = reference.Trim().ToLowerInvariant();
            return lower is "test" or "fake" or "dummy" or "n/a" or "na" or "none" or "null" or "0" or "0000" or "ref-" or "ref-0000";
        }
    }
}
