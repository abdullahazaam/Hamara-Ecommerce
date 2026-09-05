# Hamara Commerce — Production ASP.NET Core MVC .NET 9 E-Commerce Platform

**Hamara Commerce** is a production-grade, highly secure, full-stack ASP.NET Core MVC e-commerce platform architected for modern retail in Pakistan and international markets.

---

## Key Highlights & Architecture

- **Framework**: .NET 9.0 (C# 13, ASP.NET Core MVC)
- **Database Architecture**: Entity Framework Core 9.0 with Persistent Microsoft SQL Server & Atomic Transactions (`IsolationLevel.ReadCommitted` with explicit database-level row locking for stock safety).
- **Authentication & Security**:
  - ASP.NET Core Identity with PBKDF2 password hashing and brute-force lockout.
  - Role-Based Access Control (`Admin`, `Customer`) with `[Authorize(Roles = "Admin")]` strictly enforced across the management backoffice.
  - Cryptographically secure public Order and Tracking Numbers (`HC-PK-...`, `TRK-...`) preventing internal sequential database key exposure and IDOR attacks.
  - Global `AutoValidateAntiforgeryTokenAttribute` on all state-altering requests.
  - Strict Content Security Policy (CSP), `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, and custom cache-control headers preventing caching on customer and admin routes.
  - Client IP Rate Limiting for authentication, contact forms, newsletter, and order tracking.
- **Server-Authoritative Pricing & Cart**:
  - Real-time cart calculation loading product, variant, and pricing data directly from SQL Server on every calculation.
  - Dynamic coupon validation with minimum order amounts, maximum discount caps, usage limit enforcement, and expiration checks.
  - Concurrency-safe atomic checkout preventing stock overselling.
- **Storefront & Admin Experience**:
  - 2026 responsive design system with dark/light mode toggle (`[data-theme='dark']`), smooth animations, and WCAG AA contrast compliance.
  - Dynamic live search modal with keyboard navigation (`Ctrl+K`).
  - Comprehensive Admin management dashboard with real SQL database metrics, product CRUD, variant matrix, order processing, customer audit logs, and CSV exports.
- **SEO & Performance**:
  - Dynamic `/sitemap.xml` and `/robots.txt` generator reflecting published products and categories.
  - Schema.org JSON-LD structured data (Organization, WebSite, Breadcrumbs, Product, and Offer).
  - High-performance response compression (Brotli / Gzip) and client asset caching headers.

---

## Environment Variables & Configuration

The application can be configured via `appsettings.json` or system environment variables:

| Environment Variable | Description | Default / Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server Connection String | `Server=(localdb)\mssqllocaldb;Database=HamaraCommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True` |
| `AdminSeed:Email` | Seeded Master Admin Email | `admin@hamaracommerce.pk` |
| `AdminSeed:Password` | Seeded Master Admin Initial Password | `Admin@123!` |
| `Smtp:Host` | SMTP Host for Customer Notifications | *(Optional — Falls back to truthful dev logging if omitted)* |
| `Smtp:Port` | SMTP Port | `587` |
| `Smtp:Username` | SMTP Username | `smtp-user` |
| `Smtp:Password` | SMTP Password | `smtp-pass` |

---

## Getting Started & Exact Run Commands

### 1. Restore & Build
```powershell
dotnet restore
dotnet build
```

### 2. Apply Database Migrations & Initial Seed
```powershell
dotnet ef database update
```
*Note: The application automatically applies pending migrations and seeds initial admin/product catalog upon startup.*

### 3. Run Automated Tests
```powershell
dotnet test
```

### 4. Run the Web Application
```powershell
dotnet run --launch-profile http
```
Navigate to: `http://localhost:5071` (or `https://localhost:7146` via https profile)

---

## Seeded Default Accounts (Verified & Tested)

The following accounts are seeded by `DbInitializer.Initialize` and verified via automated authentication tests:

- **Administrator**:
  - Email: `admin@hamaracommerce.pk`
  - Password: `Admin@123!`
  - Role: `Admin` (Strictly enforced via `[Authorize(Roles = "Admin")]`)
  - Redirection: Default sign-in automatically redirects directly to `/Admin` backoffice (while honoring specific deep-linked return URLs).
  - Safety & Idempotency: In `Development` mode, the password is reset to `Admin@123!` if altered and any lockout/access-failed counts are cleared; in `Production`, passwords are never overwritten on startup.
- **Customer**:
  - Email: `customer@hamaracommerce.pk`
  - Password: `Customer@123!`
  - Role: `Customer`
  - Redirection: Standard customer redirect to `/` (or specified `returnUrl` such as `/Account`, `/Checkout`).
