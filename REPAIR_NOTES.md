# HamaraCommerce source repair — September 5, 2026

Input: HamaraCommerce(5).rar. This is a complete source package, not a published build.
UI constraint: all 62 files in Views and all 63 files in wwwroot match the input byte-for-byte. No Razor, CSS, JavaScript, icons, libraries or product assets were redesigned.

## Implemented code changes
- Preserved Identity security-stamp validation before deactivated/unverified-session rejection. Auth rate limits now target auth actions rather than every account action; wishlist additions validate published products.
- Saved immutable, server-priced checkout snapshots; canonical versioned request hashes cover contact/shipping/payment/order-note fields. Completed replays work without live cart contents; older completed records reconstruct their hash inputs from the order. Changed form input still rejects key reuse.
- Added SQL Server session application locks for same-key checkout, record rowversion concurrency, and transactional stock/coupon update locks. Multiple lines cannot collectively exceed product/variant stock. Collision responses never return another record's guest token.
- Saved a stable order/payment request reference and payment intent before calling the provider. A missing result is explicitly unresolved and blocked from a blind second charge. Known saved payment results recover against the original amount and snapshot.
- Saved order confirmation, admin lifecycle notifications and contact notifications with their corresponding database changes. Order email creation is inside the order transaction.
- Added atomic SQL outbox claims, expiring leases, token-fenced completion, bounded sends, crash recovery, and blocked-SMTP retry. DispatchSingle cannot resend Sent messages. Missing SMTP does not exhaust the delivery-attempt limit. Unique-key enqueue races return the existing event.
- Coupon restoration, inventory restoration and cancellation now share transactions. Coupon redemption has a unique (OrderId, CouponCode) constraint. Historical backfill is transactional.
- Admin status-based cancellation uses inventory restoration; shipped/delivered/refunded orders cannot be cancelled as unshipped orders. Delivery no longer asserts payment collection. Refund status requires verified PaymentStatus.Refunded.
- A customer return request is recorded for review, without falsely changing order/payment state or restoring coupons. New deliveries record DeliveredAt; legacy orders without that timestamp retain conservative order-date fallback.
- Production requires configured PublicSiteUrl (public HTTPS). Account verification/reset emails use its origin. Startup fails visibly if database migration/seeding fails.

## Required schema update
Included migration: 20260905223000_RepairCheckoutOutboxAndLifecycle.
It adds checkout CartSnapshotJson/HashVersion/RowVersion, outbox LockToken/LockExpiresAt, and order DeliveredAt/ReturnRequestedAt/ReturnReason; it adds a unique coupon-redemption index.
No existing migration was rewritten. The current model snapshot and the new migration target model are included.
The migration refuses to silently delete duplicate historical coupon-redemption records. If it reports duplicates, reconcile against a backup and the actual financial records; do not drop the database or disable the constraint.
Migration source was authored but NOT executed here. Validate model-snapshot parity and migration application locally before replacing the running instance.

## Verification performed here
- All storefront/admin UI asset and view bytes match the input.
- Project XML parses; JavaScript syntax check succeeds; source/package inventory and archive CRC verified.
- Existing source was reviewed for the targeted failure paths.
- Added tests: security-stamp and account validation, blocked/recovered email delivery, Sent-message protection, SQL multi-worker delivery, coupon/cancellation rollback, completed replay with an empty cart, and uncertain payment outcomes.
- .NET SDK and SQL Server are unavailable in this environment. Build, C# compilation, SQL tests, migration execution, SMTP delivery and live-browser tests have NOT been independently run. No passing test count is claimed. Run ANTIGRAVITY_NEXT_STEPS.txt before treating this package as ready to replace a working instance.

## Operational limits / unfinished product features
- SMTP delivery is at-least-once. If SMTP accepted a message immediately before the process/connection failed, a retry can duplicate it; SMTP provides no general exactly-once guarantee.
- There is no real online payment provider: only COD and the existing Development simulator. Unknown payment outcomes require operator reconciliation; no automated provider lookup/refund API or recovery dashboard is claimed. Never reset unresolved payment records to force a second payment.
- Legacy in-progress purchases without stored price snapshots may need reconciliation if their original amount cannot be recovered safely.
- Return requests are persisted, but a full inspection/partial-refund/courier workflow and staff review UI were not added. Existing future modules (dedicated category/inventory management, marketplace sellers, courier integrations) are outside this bug-fix package.
- Serializable transactions prioritize correctness and can retry/reject under contention. Load/performance testing remains necessary for production.

## Clean replacement
Extract to a NEW sibling folder and test there first. Keep the old project and database backup until verification completes. Preserve the existing Git repository metadata, actual local configuration, secrets and any newer uploaded files. The ZIP intentionally omits Git metadata; a ZIP cannot replace the SQL Server database.
After verification, use the same application configuration/database with the supplied migration applied; then commit/push from the existing repository. Do not overwrite production uploads with an older archive snapshot.

## Removed generated paths
- obj: 10,809,979 bytes
- bin: 44,521,100 bytes
- .vs: 15,423,695 bytes
- HamaraCommerce.csproj.user: 238 bytes
- Tests/HamaraCommerce.Tests/obj: 580,127 bytes
- Tests/HamaraCommerce.Tests/bin: 26,707,167 bytes

Git history was excluded from the distribution (not destroyed); existing .git must remain on the user machine. All third-party licenses and runtime assets are retained.

## Changed/new files
- .gitignore
- Controllers/AccountController.cs
- Controllers/AdminController.cs
- Controllers/CheckoutController.cs
- Controllers/HomeController.cs
- Data/ApplicationDbContext.cs
- Migrations/20260905223000_RepairCheckoutOutboxAndLifecycle.Designer.cs
- Migrations/20260905223000_RepairCheckoutOutboxAndLifecycle.cs
- Migrations/ApplicationDbContextModelSnapshot.cs
- Models/CheckoutIdempotencyRecord.cs
- Models/EmailOutboxMessage.cs
- Models/Order.cs
- Program.cs
- Services/CommerceDatabaseWork.cs
- Services/CommerceSessionValidator.cs
- Services/EmailOutboxService.cs
- Services/IPaymentGateway.cs
- Services/PaymentGateway.cs
- Services/PricingService.cs
- Tests/HamaraCommerce.Tests/CheckoutIdempotencyRealSqlTests.cs
- Tests/HamaraCommerce.Tests/OutboxRecoveryTests.cs
- Tests/HamaraCommerce.Tests/OutboxSqlReliabilityTests.cs
- Tests/HamaraCommerce.Tests/SessionValidationTests.cs
- Tests/HamaraCommerce.Tests/TestDbContextFactory.cs
