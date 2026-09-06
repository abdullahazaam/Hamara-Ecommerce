# Hamara Commerce

An ASP.NET Core MVC e-commerce portfolio project built around a Pakistan-focused catalogue, secure customer accounts, server-authoritative pricing, and operational admin workflows.

> **Project status:** actively developed portfolio application. It demonstrates production-oriented patterns, but it is not presented as a hosted commercial marketplace or a PCI-certified payment system.

## What it demonstrates

- ASP.NET Core MVC on .NET 9 with C# and Razor views
- Entity Framework Core with SQL Server migrations
- ASP.NET Core Identity, email confirmation, role-based authorization, and account controls
- Product catalogue, categories, variants, search, filters, cart, wishlist, coupons, checkout, and order tracking
- Server-side totals, stock validation, checkout idempotency, and order-scoped guest access
- Admin catalogue and order-management workflows
- Transactional email outbox and recovery-oriented checkout records
- Responsive light/dark storefront, structured metadata, sitemap, and asset caching
- xUnit regression and SQL Server integration tests

## Architecture

```text
Browser / Razor Views
        |
ASP.NET Core MVC Controllers
        |
Application Services (cart, pricing, payments, email)
        |
Entity Framework Core
        |
SQL Server
```

The server remains the source of truth for price, discount, stock, ownership, and final order totals. Client-submitted totals are not trusted.

## Repository layout

| Path | Purpose |
| --- | --- |
| `Controllers/` | Storefront, account, checkout, and admin request handling |
| `Services/` | Pricing, cart, payment, email, and supporting business logic |
| `Models/` | Domain entities and view models |
| `Data/` | EF Core context, initialisation, and catalogue data |
| `Migrations/` | Additive SQL Server schema migrations |
| `Views/` | Razor storefront and back-office UI |
| `wwwroot/` | CSS, JavaScript, and local product assets |
| `Tests/HamaraCommerce.Tests/` | xUnit regression and integration tests |

## Run locally

### Requirements

- .NET 9 SDK
- SQL Server or SQL Server LocalDB
- EF Core CLI (`dotnet tool install --global dotnet-ef`)

### Setup

```bash
git clone https://github.com/abdullahazaam/Hamara-Ecommerce.git
cd Hamara-Ecommerce
dotnet restore
dotnet ef database update
dotnet run
```

The default development connection targets SQL Server LocalDB. Override it without committing secrets:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=(localdb)\MSSQLLocalDB;Database=HamaraCommerceDb;Trusted_Connection=True;TrustServerCertificate=True"
```

ASP.NET Core maps nested environment keys with double underscores, for example `Smtp__Host`, `Smtp__Username`, and `Smtp__Password`.

## Tests

```bash
dotnet build HamaraCommerce.sln
dotnet test HamaraCommerce.sln
```

Some concurrency and recovery tests require an actual SQL Server instance. Configure the test connection expected by `TestDbContextFactory`; do not treat an in-memory provider as proof of SQL locking behaviour.

## Development accounts

Demo accounts are created only by the development seed path. Treat them as local demonstration credentials and replace or disable seeded credentials before any deployment.

## Security and deployment notes

- Keep connection strings, SMTP credentials, signing keys, and payment credentials outside tracked configuration.
- Apply migrations to a backed-up database; never replace a database containing real customer/order data with seed data.
- Only configured payment providers should be exposed to customers.
- Production deployment still requires HTTPS, secret management, monitoring, backups, an SMTP provider, and payment-provider reconciliation.

## Portfolio focus

The most important engineering work in this repository is not the catalogue size. It is the treatment of identity, order ownership, pricing, stock contention, checkout replay safety, coupon consistency, and recoverable email/payment workflows.

## Author

**Abdullah Azaam** — junior web developer focused on ASP.NET Core, C#, SQL Server, PHP, and Laravel.

