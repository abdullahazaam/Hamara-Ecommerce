using System;
using System.IO.Compression;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
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

// ==========================================
// 1. MVC & GLOBAL ANTI-FORGERY SECURITY
// ==========================================
builder.Services.AddControllersWithViews(options =>
{
    // Enforce automatic anti-forgery validation on all unsafe HTTP methods (POST, PUT, DELETE, PATCH)
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=HamaraCommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

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

if (builder.Environment.IsProduction())
{
    var publicSiteUrl = builder.Configuration["PublicSiteUrl"] ?? builder.Configuration["SiteUrl"];
    if (!Uri.TryCreate(publicSiteUrl, UriKind.Absolute, out var publicUri) ||
        publicUri.Scheme != Uri.UriSchemeHttps || publicUri.IsLoopback || !string.IsNullOrEmpty(publicUri.UserInfo) ||
        !string.IsNullOrEmpty(publicUri.Query) || !string.IsNullOrEmpty(publicUri.Fragment))
        throw new InvalidOperationException("Set PublicSiteUrl to this store's public HTTPS URL before production startup.");
}

var app = builder.Build();

// ==========================================
// 9. DATABASE MIGRATION & SEEDING
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var config = services.GetRequiredService<IConfiguration>();

        context.Database.Migrate();
        DbInitializer.Initialize(context, userManager, roleManager, config, isDevelopment: app.Environment.IsDevelopment());
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
        // Cache public static assets for 7 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800, immutable");
    }
});

app.UseRouting();
app.UseRateLimiter();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

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
