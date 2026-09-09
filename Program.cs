using System;
using System.IO.Compression;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;

var builder = WebApplication.CreateBuilder(args);

// Allow an ignored, machine-local configuration file to supply production
// secrets without ever committing them. Environment variables remain the
// highest-precedence source (for example ConnectionStrings__DefaultConnection).
builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();


// ==========================================
// 1. MVC & GLOBAL ANTI-FORGERY SECURITY
// ==========================================
builder.Services.AddControllersWithViews(options =>
{
    // Enforce automatic anti-forgery validation on all unsafe HTTP methods (POST, PUT, DELETE, PATCH)
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.Configure<FormOptions>(options =>
{
    // Product images are limited to 5 MB each by ImageUploadService. Allow
    // enough multipart overhead and multiple-image admin uploads.
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024;
});

// ==========================================
// 2. RESPONSE COMPRESSION (Brotli & Gzip)
// ==========================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "image/svg+xml",
        "application/json",
        "application/xml",
        "text/css",
        "text/javascript"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// ==========================================
// 3. PERSISTENT SQL SERVER DATABASE
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required. Configure it with an environment variable or an ignored local settings file.");
}
if (builder.Environment.IsProduction() && connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "LocalDB cannot be used in production. Configure the production connection through an environment variable or ignored local settings file.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

// ==========================================
// 4. ASP.NET CORE IDENTITY CONFIGURATION
// ==========================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ==========================================
// 5. SECURE AUTHENTICATION COOKIE & SESSION VALIDATION
// ==========================================
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "HamaraCommerce.Auth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;

    // Disabled sessions rejection: invalidate principal if user is inactive or deleted
    options.Events.OnValidatePrincipal = CommerceSessionValidator.ValidateAsync;
});

// ==========================================
// 6. RATE LIMITING POLICIES
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General Auth rate limit: 10 requests per minute per IP
    options.AddPolicy("AuthPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Contact form rate limit: 5 submissions per minute per IP
    options.AddPolicy("ContactPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ==========================================
// 7. SESSION, CACHING & HEALTH CHECKS
// ==========================================
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // MonsterASP.NET terminates HTTPS at its IIS reverse proxy. The proxy
    // addresses are host-managed and may change, so accept its forwarded headers.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// ==========================================
// 8. APPLICATION BUSINESS SERVICES
// ==========================================
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IShippingTaxService, ShippingTaxService>();
builder.Services.AddScoped<IPaymentGateway, PaymentGateway>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailOutboxService, EmailOutboxService>();
builder.Services.AddHostedService<EmailOutboxBackgroundService>();
builder.Services.AddScoped<ISeoService, SeoService>();
builder.Services.AddScoped<ICatalogueImporter, CatalogueImporter>();
builder.Services.AddScoped<ICatalogueImageRepairService, CatalogueImageRepairService>();
builder.Services.AddScoped<MarketplaceCatalogueService>();
builder.Services.AddScoped<IReturnRefundService, ReturnRefundService>();
builder.Services.AddScoped<IOperationalRecoveryService, OperationalRecoveryService>();

if (builder.Environment.IsProduction())
{
    var publicSiteUrl = builder.Configuration["PublicSiteUrl"] ?? builder.Configuration["SiteUrl"];
    if (!Uri.TryCreate(publicSiteUrl, UriKind.Absolute, out var publicUri) ||
        publicUri.Scheme != Uri.UriSchemeHttps || publicUri.IsLoopback || !string.IsNullOrEmpty(publicUri.UserInfo) ||
        !string.IsNullOrEmpty(publicUri.Query) || !string.IsNullOrEmpty(publicUri.Fragment))
        throw new InvalidOperationException("Set PublicSiteUrl to this store's public HTTPS URL before production startup.");

    builder.Services.ConfigureApplicationCookie(options =>
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always);
    builder.Services.Configure<SessionOptions>(options =>
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always);
}

var app = builder.Build();

// ==========================================
// 9. EXPLICIT DATABASE MAINTENANCE COMMANDS
// ==========================================
var maintenanceRequested = args.Any(arg => arg is
    "--marketplace-transform" or "--seed-marketplace" or
    "--import-catalogue" or "--seed-catalogue" or
    "--repair-images" or "--repair-storefront");

if (maintenanceRequested)
{
    if (app.Environment.IsProduction() && !builder.Configuration.GetValue<bool>("AllowProductionDatabaseMaintenance"))
        throw new InvalidOperationException("Production database maintenance commands are disabled.");

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        if (args.Contains("--marketplace-transform") || args.Contains("--seed-marketplace"))
        {
            logger.LogInformation("Executing CLI marketplace catalogue transformation...");
            var marketService = services.GetRequiredService<MarketplaceCatalogueService>();
            marketService.RunAsync().GetAwaiter().GetResult();
            Console.WriteLine("=================================================");
            Console.WriteLine("MARKETPLACE CATALOGUE TRANSFORMATION COMPLETE");
            Console.WriteLine("=================================================");
            return;
        }

        if (args.Contains("--import-catalogue") || args.Contains("--seed-catalogue"))
        {
            logger.LogInformation("Executing CLI catalogue import...");
            var importer = services.GetRequiredService<ICatalogueImporter>();
            var report = importer.ImportCatalogueAsync().GetAwaiter().GetResult();
            Console.WriteLine("=================================================");
            Console.WriteLine("REAL PAKISTANI CATALOGUE IMPORT REPORT");
            Console.WriteLine("=================================================");
            Console.WriteLine($"Published Products: {report.PublishedProductCount}");
            Console.WriteLine($"Total Processed:    {report.TotalProcessed}");
            Console.WriteLine($"Total Categories:   {report.CategoryCounts.Count}");
            foreach (var kvp in report.CategoryCounts.OrderBy(k => k.Key))
            {
                var (min, max) = report.CategoryPriceRanges[kvp.Key];
                Console.WriteLine($"  - {kvp.Key,-24}: {kvp.Value} items (PKR {min:N0} - {max:N0})");
            }
            Console.WriteLine($"Duplicate SKUs:     {report.DuplicateSkus}");
            Console.WriteLine($"Duplicate Slugs:    {report.DuplicateSlugs}");
            Console.WriteLine($"Missing Images:     {report.MissingImages}");
            Console.WriteLine($"Missing Source URLs:{report.MissingSourceUrls}");
            Console.WriteLine($"Invalid Prices:     {report.InvalidPrices}");
            Console.WriteLine($"Legacy Published:   {report.LegacyPublishedProducts}");
            Console.WriteLine($"Fake Seeded Reviews:{report.FakeSeededReviews}");
            Console.WriteLine("=================================================");
            return;
        }

        if (args.Contains("--repair-images") || args.Contains("--repair-storefront"))
        {
            logger.LogInformation("Executing CLI catalogue image and storefront repair...");
            var repairService = services.GetRequiredService<ICatalogueImageRepairService>();
            var repairReport = repairService.RepairImagesAndSignalsAsync().GetAwaiter().GetResult();
            Console.WriteLine("=================================================");
            Console.WriteLine("STOREFRONT & IMAGE REPAIR REPORT");
            Console.WriteLine("=================================================");
            Console.WriteLine($"Total Processed:         {repairReport.TotalProcessed}");
            Console.WriteLine($"Successfully Repaired:   {repairReport.RepairedCount}");
            Console.WriteLine($"Archived (Unrepairable): {repairReport.ArchivedCount}");
            Console.WriteLine($"Duplicate Hashes Flagged:{repairReport.DuplicateHashesCount}");
            Console.WriteLine("=================================================");
            return;
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration or seeding failed; refusing to start with an incompatible schema.");
        throw;
    }
}

// ==========================================
// 10. HTTP REQUEST PIPELINE & SECURITY HEADERS
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage/{0}");

app.UseResponseCompression();
app.UseForwardedHeaders();
app.UseHttpsRedirection();

// Production Security Headers Middleware
app.Use(async (context, next) =>
{
    var response = context.Response;
    var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

    // Security Headers
    response.Headers.Append("X-Content-Type-Options", "nosniff");
    response.Headers.Append("X-Frame-Options", "DENY");
    response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
    response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: https: blob:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self';");

    // Strictly disable caching for private customer, checkout, cart, and admin routes
    if (path.StartsWith("/admin") || path.StartsWith("/account") || path.StartsWith("/checkout") || path.StartsWith("/cart"))
    {
        response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        response.Headers.Append("Pragma", "no-cache");
        response.Headers.Append("Expires", "0");
    }

    await next();
});

// Static Files with caching for public assets
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache public static assets for 1 year (versioned via asp-append-version)
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
    }
});

app.UseRouting();
app.UseRateLimiter();


app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// 11. PORTFOLIO DEMO ACCOUNTS
// ==========================================
// These two public demo accounts are intentionally kept available on the
// portfolio deployment. Only these accounts are touched by this startup block.
using (var demoScope = app.Services.CreateScope())
{
    var demoServices = demoScope.ServiceProvider;
    var userManager = demoServices.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = demoServices.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = demoServices.GetRequiredService<ILogger<Program>>();

    foreach (var roleName in new[] { "Admin", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}': " +
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }

    async Task EnsureDemoUserAsync(
        string email,
        string password,
        string fullName,
        string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create demo user '{email}': " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            var userChanged = false;

            if (!string.Equals(user.UserName, email, StringComparison.OrdinalIgnoreCase))
            {
                user.UserName = email;
                userChanged = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                userChanged = true;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                userChanged = true;
            }

            if (userChanged)
            {
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to update demo user '{email}': " +
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }
            }

            // Keep the documented portfolio credentials reliable.
            if (!await userManager.CheckPasswordAsync(user, password))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);
                if (!resetResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to reset demo password for '{email}': " +
                        string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }
            }
        }

        var clearLockoutResult = await userManager.SetLockoutEndDateAsync(user, null);
        if (!clearLockoutResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to clear lockout for demo user '{email}': " +
                string.Join(", ", clearLockoutResult.Errors.Select(e => e.Description)));
        }

        var resetFailuresResult = await userManager.ResetAccessFailedCountAsync(user);
        if (!resetFailuresResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to reset access failures for demo user '{email}': " +
                string.Join(", ", resetFailuresResult.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign role '{roleName}' to '{email}': " +
                    string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation(
            "Portfolio demo account ensured: {Email} ({Role})",
            email,
            roleName);
    }

    await EnsureDemoUserAsync(
        "admin@hamaracommerce.pk",
        "Admin@123!",
        "Hamara Administrator",
        "Admin");

    await EnsureDemoUserAsync(
        "customer@hamaracommerce.pk",
        "Customer@123!",
        "Demo Customer",
        "Customer");
}

// Health Check Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/healthz");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
