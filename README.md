# Hamara Commerce

Hamara Commerce is the main ASP.NET Core project in my portfolio. I started it as an e-commerce store and kept expanding it so I could work through more than basic product management. The project now covers customer accounts, shopping, checkout, orders and several admin workflows.

## What is included

- Product catalogue with categories, brands, search, filters and pagination
- Product details, variants, stock information and image galleries
- Customer registration, email confirmation and account management
- Cart and wishlist
- Guest and signed-in checkout flows
- Orders, coupons, pricing calculations and order tracking
- Admin screens for products, orders and store operations
- Transactional email queue with retry handling
- Automated tests for important account, pricing and checkout behaviour

## Technology

- ASP.NET Core MVC on .NET 9
- C# and Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Razor views, Bootstrap, CSS and JavaScript
- xUnit for tests
- GitHub Actions for build and test checks

## Areas I focused on

The most useful part of this project was working through problems that do not appear in a simple CRUD store. These included checking who can access an order, keeping displayed and saved prices consistent, updating stock safely, preventing repeated checkout submissions and recovering queued email work after an interruption.

## Running the project

You will need the .NET 9 SDK and SQL Server.

1. Clone the repository.
2. Set the SQL Server connection string in local configuration or user secrets.
3. Restore packages and apply the migrations:

```bash
dotnet restore
dotnet ef database update
```

4. Start the application:

```bash
dotnet run
```

Do not commit production connection strings, SMTP credentials or payment credentials. Use user secrets or environment variables for local development.

## Tests

```bash
dotnet test HamaraCommerce.sln
```

Some checkout and concurrency tests require SQL Server. They should not be treated as passing if the required database is unavailable.

## Project status

This is a portfolio project, not a live commercial marketplace. COD and test payment flows are available for demonstration, but a real deployment would still need production payment, email, monitoring and operational configuration.

