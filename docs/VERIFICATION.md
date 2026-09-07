# Verification Report

This document records the verifiable build, test, and database results for **HamaraCommerce** as of the latest `main` branch. All steps below are reproducible in a local development environment and on continuous integration runners.

---

## 1. Environment & Prerequisites

- **Framework**: .NET 9.0 (ASP.NET Core MVC, Entity Framework Core 9)
- **Language**: C# 13
- **Primary Database**: Microsoft SQL Server / LocalDB (`(localdb)\mssqllocaldb`)
- **Supported Platforms**: Windows (local development with LocalDB / SQL Server 2022), Linux/Windows CI runners
- **Test Runner**: Visual Studio Test Platform (`VSTest 17.14.1` via `dotnet test`)

---

## 2. Build Verification

Build command executed in Release configuration:

```bash
dotnet build --configuration Release
```

### Result
- **Exit Code**: `0`
- **Errors**: `0`
- **Status**: Succeeded across all projects (`HamaraCommerce.csproj`, `HamaraCommerce.Tests.csproj`).

---

## 3. Test Suite Results

The automated test suite contains **118 total tests**, separated into environment-independent unit/endpoint tests and real SQL Server integration tests.

```bash
dotnet test --configuration Release
```

### Overall Summary
| Metric | Total |
| :--- | :--- |
| **Total Tests** | **118** |
| **Passed** | **118** |
| **Failed** | **0** |
| **Skipped** | **0** |
| **Pass Rate** | **100%** |
| **Execution Duration** | ~24 seconds |

---

## 4. Test Suite Breakdown by Feature Area

### A. Environment-Independent Test Suite (107 Tests)
Run command:
```bash
dotnet test --configuration Release --filter "Category!=Integration"
```

1. **Product Reviews & Questions Moderation (`ProductReviewAndQuestionEndpointTests.cs` - 14 tests)**
   - New reviews require administrator moderation (`IsApproved = false`).
   - Customer questions remain unpublished until approved or answered.
   - Verified purchase qualification requires a paid order containing the item (completed card payment or paid COD).
   - Unpaid, pending, cancelled, failed, or refunded orders never qualify for verified purchase.
   - Strict `(ProductId, UserId)` single-review uniqueness enforced.
   - Rating values (1–5) and review body lengths validated.
   - Ratings recalculated exclusively from approved reviews.

2. **Returns & Refund Governance (`ReturnAndRefundGovernanceTests.cs` - 12 tests)**
   - Transitions order through requested, approved, inspected, and refund-pending states.
   - Prohibits automated fake refund reference generation; requires authentic banking/remittance references.
   - Validates refund amounts against order refundable balance.
   - Transactional, idempotent restocking: restores product and variant stock exactly once.
   - Customer notification emails queued within the same database transaction.
   - Audit logging captures old state, new state, administrator identity, and timestamps.

3. **Operational Recovery & Reclaim Governance (`OperationalRecoveryGovernanceTests.cs` - 14 tests)**
   - `RetryOutboxEmail` never requeues messages already marked `Sent`.
   - Atomic conditional updates reclaim only `Failed`, `Blocked`, or expired `Processing` outbox messages.
   - Concurrent retry requests queue at most one retry.
   - Payment reconciliation requires explicit outcome, administrator notes, and reference evidence.
   - Never triggers payment gateway charges during manual reconciliation.
   - State audit trail records administrator identity, timestamps, and transition history.

4. **Category Hierarchy & Inventory Concurrency (`CategoryAndInventoryGovernanceTests.cs` - 23 tests)**
   - Hierarchy cycle prevention: prevents a category from becoming its own ancestor.
   - Unique URL slug generation and safe category renaming.
   - Efficient single grouped query replaces N+1 product count queries.
   - True inventory pagination honoring `page` and `pageSize` parameters.
   - Atomic stock adjustments with concurrency token validation.
   - Base stock and variant total synchronization: prevents inconsistent inventory edits.
   - Complete inventory movement audit logging.

5. **Customer Privacy, Pricing, Security & IDOR (`HamaraCommerce.Tests` - 44 tests)**
   - **`CustomerPrivacyAndIdorTests.cs`**: IDOR protection for order tracking, invoice downloads, addresses, and profiles.
   - **`PricingAndCouponTests.cs`**: Server-side authoritative calculation, coupon validation, tier discounts, GST calculation.
   - **`CheckoutAndStockConcurrencyTests.cs`**: Concurrent checkouts against limited stock; zero overselling.
   - **`AdminVerificationTests.cs`**: Role-based authorization and administrative safeguards.
   - **`PaymentAndIdempotencyTests.cs`**: Payment state machines and duplicate submission protection.
   - **`SeoAndNotificationTests.cs`**: OpenGraph, Schema.org JSON-LD, robots.txt, and canonical URLs.
   - **`StorefrontResponsiveTests.cs`**: Storefront route integrity and responsive markup structure.

---

### B. Real SQL Server Integration Test Suite (11 Tests)
Run command:
```bash
dotnet test --configuration Release --filter "Category=Integration"
```

1. **Checkout Idempotency & Concurrency (`CheckoutIdempotencyRealSqlTests.cs` - 11 tests)**
   - Runs against real SQL Server (LocalDB or containerized SQL Server).
   - Tests true row-level locks, transactional rollback, and idempotency key uniqueness constraints under multi-threaded concurrency.
   - Verifies that parallel checkout submissions with identical idempotency tokens result in exactly one order, one payment record, and zero duplicate charges.
   - Proves variant and base stock consistency under high-contention concurrent transactions.

---

## 5. Database & Seed Verification

- **Seed Source**: `Data/Catalog/pakistan-products-2026.json`
- **Total Published Products**: 342
- **Total Categories**: 15 top-level departments with parent-child relationships
- **Currency & Pricing**: Pakistani Rupees (PKR) formatted with local standards
- **Migrations**: Additive EF Core migrations; database schema reflects all governance, moderation, return, outbox, and audit entities.

---

## 6. Continuous Integration (GitHub Actions)

The workflow defined in `.github/workflows/dotnet.yml` executes on every push and pull request to `main`:

1. **Environment-Independent Job** (`test-independent`):
   - Restores and compiles solution in Release mode.
   - Executes all 107 environment-independent tests.
   - Generates `.trx` test result artifacts on failure.
2. **SQL Server Integration Job** (`test-sql-integration`):
   - Spins up a real SQL Server service on Windows runner using LocalDB.
   - Runs all 11 real SQL integration tests against persistent database engine.
   - Enforces zero EF InMemory substitutions for concurrency and transaction isolation tests.
