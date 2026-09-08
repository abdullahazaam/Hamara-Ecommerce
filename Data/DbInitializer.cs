using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Data.Catalog;
using HamaraCommerce.Models;

namespace HamaraCommerce.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>? userManager = null, Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>? roleManager = null, Microsoft.Extensions.Configuration.IConfiguration? config = null, bool isDevelopment = false)
        {
            // Database migrated via EF Core Migrations

            // 1. SEED CATEGORIES (15 Official Pakistani Categories)
            var officialCategories = PakistanCatalogBuilder.GetOfficialCategories();
            var existingCategories = context.Categories.ToList();
            if (!existingCategories.Any())
            {
                context.Categories.AddRange(officialCategories);
                context.SaveChanges();
                existingCategories = context.Categories.ToList();
            }
            else
            {
                var officialMap = officialCategories.ToDictionary(c => c.Slug.ToLowerInvariant(), c => c);
                foreach (var existing in existingCategories)
                {
                    if (officialMap.TryGetValue(existing.Slug.ToLowerInvariant(), out var official))
                    {
                        existing.Name = official.Name;
                        existing.Icon = official.Icon;
                        existing.Description = official.Description;
                        existing.DisplayOrder = official.DisplayOrder;
                        existing.IsFeatured = official.IsFeatured;
                        existing.IsActive = true;
                    }
                    else
                    {
                        existing.IsActive = false;
                        existing.IsFeatured = false;
                    }
                }
                var existingSlugs = existingCategories.Select(c => c.Slug.ToLowerInvariant()).ToHashSet();
                foreach (var official in officialCategories)
                {
                    if (!existingSlugs.Contains(official.Slug.ToLowerInvariant()))
                    {
                        context.Categories.Add(new Category
                        {
                            Name = official.Name,
                            Slug = official.Slug,
                            Icon = official.Icon,
                            Description = official.Description,
                            DisplayOrder = official.DisplayOrder,
                            IsFeatured = official.IsFeatured,
                            IsActive = true,
                            ParentCategoryId = null,
                            ProductCount = 0
                        });
                    }
                }
                context.SaveChanges();
                existingCategories = context.Categories.ToList();
            }

            // 2. SEED CANONICAL 163 REAL PAKISTANI PRODUCTS
            var dtos = PakistanCatalogBuilder.GetAllProducts();
            var canonicalSkus = dtos.Select(d => d.SKU).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingDbProducts = context.Products
                .Include(p => p.OrderItems)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.InventoryMovements)
                .Include(p => p.Reviews)
                .Include(p => p.Questions)
                .ToList();

            foreach (var prod in existingDbProducts)
            {
                if (!canonicalSkus.Contains(prod.SKU))
                {
                    bool hasOrders = prod.OrderItems.Any() || context.OrderItems.Any(oi => oi.ProductId == prod.Id);
                    if (hasOrders)
                    {
                        prod.Status = ProductStatus.Archived;
                        prod.IsFeatured = false;
                        prod.IsFlashDeal = false;
                        prod.IsTrending = false;
                        prod.IsBestSeller = false;
                        prod.IsNewArrival = false;
                        prod.Rating = 0.0;
                        prod.ReviewCount = 0;
                    }
                    else
                    {
                        if (prod.Reviews.Any()) context.Reviews.RemoveRange(prod.Reviews);
                        if (prod.Questions.Any()) context.QuestionAnswers.RemoveRange(prod.Questions);
                        if (prod.Images.Any()) context.ProductImages.RemoveRange(prod.Images);
                        if (prod.Variants.Any()) context.ProductVariants.RemoveRange(prod.Variants);
                        if (prod.InventoryMovements.Any()) context.InventoryMovements.RemoveRange(prod.InventoryMovements);
                        context.Products.Remove(prod);
                    }
                }
            }
            context.SaveChanges();

            var existingMap = context.Products.ToDictionary(p => p.SKU, StringComparer.OrdinalIgnoreCase);
            var catMap = existingCategories.ToDictionary(c => c.Slug.ToLowerInvariant(), c => c);
            int rank = 1;

            foreach (var dto in dtos)
            {
                if (!catMap.TryGetValue(dto.Category.ToLowerInvariant(), out var cat))
                {
                    continue;
                }

                double discount = 0;
                if (dto.OldPrice > 0 && dto.OldPrice > dto.Price)
                {
                    discount = Math.Round((double)((dto.OldPrice - dto.Price) / dto.OldPrice) * 100, 1);
                }

                DateTime priceChecked = DateTime.TryParse(dto.PriceCheckedAt, out var dt)
                    ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                    : DateTime.UtcNow;

                if (existingMap.TryGetValue(dto.SKU, out var p))
                {
                    p.Title = dto.Title;
                    p.Brand = dto.Brand;
                    p.CategoryId = cat.Id;
                    p.CategoryName = cat.Name;
                    p.Slug = dto.Slug;
                    p.Price = dto.Price;
                    p.OldPrice = dto.OldPrice;
                    p.DiscountPercentage = discount;
                    p.Stock = dto.Stock;
                    p.Status = ProductStatus.Published;
                    p.Rating = 0.0;
                    p.ReviewCount = 0;
                    p.ShortDescription = dto.ShortDescription;
                    p.FullDescription = dto.ShortDescription;
                    p.MainImage = dto.MainImage;
                    p.ImageSourceUrl = dto.ImageSourceUrl;
                    p.SourceRetailer = dto.SourceRetailer;
                    p.SourceProductUrl = dto.SourceProductUrl;
                    p.PriceCheckedAt = priceChecked;
                    p.IsFeatured = dto.IsFeatured;
                    p.IsFlashDeal = dto.IsFlashDeal;
                    p.StorefrontRank = dto.StorefrontRank ?? (rank++);
                    p.FlashDealEnd = dto.IsFlashDeal ? DateTime.UtcNow.AddDays(7) : null;
                    p.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    p = new Product
                    {
                        Title = dto.Title,
                        Brand = dto.Brand,
                        CategoryId = cat.Id,
                        CategoryName = cat.Name,
                        SKU = dto.SKU,
                        Slug = dto.Slug,
                        Price = dto.Price,
                        OldPrice = dto.OldPrice,
                        DiscountPercentage = discount,
                        Stock = dto.Stock,
                        Status = ProductStatus.Published,
                        Rating = 0.0,
                        ReviewCount = 0,
                        ShortDescription = dto.ShortDescription,
                        FullDescription = dto.ShortDescription,
                        MainImage = dto.MainImage,
                        ImageSourceUrl = dto.ImageSourceUrl,
                        SourceRetailer = dto.SourceRetailer,
                        SourceProductUrl = dto.SourceProductUrl,
                        PriceCheckedAt = priceChecked,
                        IsFeatured = dto.IsFeatured,
                        IsFlashDeal = dto.IsFlashDeal,
                        StorefrontRank = dto.StorefrontRank ?? (rank++),
                        FlashDealEnd = dto.IsFlashDeal ? DateTime.UtcNow.AddDays(7) : null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    p.Images.Add(new ProductImage
                    {
                        ImageUrl = dto.MainImage,
                        AltText = dto.Title,
                        IsMain = true,
                        SortOrder = 0
                    });

                    context.Products.Add(p);
                    existingMap[p.SKU] = p;
                }
            }
            context.SaveChanges();

            foreach (var cat in context.Categories.ToList())
            {
                cat.ProductCount = context.Products.Count(p => p.CategoryId == cat.Id && p.Status == ProductStatus.Published);
            }
            context.SaveChanges();

            // 3. SEED COUPONS (PKR-based)
            if (!context.Coupons.Any())
            {
                var coupons = new List<Coupon>
                {
                    new Coupon { Code = "SAVE10", Description = "10% off your entire order", DiscountPercentage = 10, MinimumSpend = 500, IsActive = true },
                    new Coupon { Code = "HAMARA20", Description = "20% special discount on orders over Rs. 10,000", DiscountPercentage = 20, MinimumSpend = 10000, IsActive = true },
                    new Coupon { Code = "FREESHIP", Description = "Free Express Shipping on any order", DiscountPercentage = 0, FixedDiscountAmount = 0m, FreeShipping = true, MinimumSpend = 0, IsActive = true },
                    new Coupon { Code = "WELCOME500", Description = "Rs. 500 Flat discount on purchases above Rs. 5,000", FixedDiscountAmount = 500.00m, MinimumSpend = 5000, IsActive = true }
                };
                context.Coupons.AddRange(coupons);
                context.SaveChanges();
            }
            else
            {
                var seededFreeShip = context.Coupons.FirstOrDefault(c => c.Code == "FREESHIP");
                if (seededFreeShip != null && (!seededFreeShip.FreeShipping || seededFreeShip.FixedDiscountAmount != 0m))
                {
                    seededFreeShip.FreeShipping = true;
                    seededFreeShip.FixedDiscountAmount = 0m;
                    context.SaveChanges();
                }
            }

            // 4. SEED ROLES AND USERS (ASP.NET Core Identity)
            ApplicationUser? demoCustomer = null;
            if (roleManager != null && userManager != null)
            {
                // Ensure Roles
                string[] roles = { "Admin", "Customer" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }

                // Seed Admin User (configured via appsettings / env var, with safe fallback in development only)
                if (!isDevelopment)
                {
                    string? prodAdminEmail = config?["AdminSeed:Email"] ?? config?["AdminUser:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? Environment.GetEnvironmentVariable("HAMARA_ADMIN_EMAIL");
                    string? prodAdminPassword = config?["AdminSeed:Password"] ?? config?["AdminUser:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? Environment.GetEnvironmentVariable("HAMARA_ADMIN_PASSWORD");
                    string prodAdminFullName = config?["AdminUser:FullName"] ?? "Hamara Administrator";

                    if (string.IsNullOrWhiteSpace(prodAdminEmail) || string.IsNullOrWhiteSpace(prodAdminPassword) ||
                        prodAdminEmail.Trim().Equals("admin@hamaracommerce.pk", StringComparison.OrdinalIgnoreCase) ||
                        prodAdminPassword == "Admin@123!")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[CRITICAL ERROR] Production administrator credentials are not configured or match insecure development defaults. Admin seeding aborted. Set ADMIN_EMAIL and ADMIN_PASSWORD environment variables.");
                        Console.ResetColor();
                    }
                    else
                    {
                        var admin = userManager.FindByEmailAsync(prodAdminEmail.Trim()).GetAwaiter().GetResult();
                        if (admin == null)
                        {
                            admin = new ApplicationUser
                            {
                                UserName = prodAdminEmail.Trim(),
                                Email = prodAdminEmail.Trim(),
                                FullName = prodAdminFullName,
                                PhoneNumber = "+92 300 0000000",
                                EmailConfirmed = true,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            var createRes = userManager.CreateAsync(admin, prodAdminPassword).GetAwaiter().GetResult();
                            if (createRes.Succeeded)
                            {
                                userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                            }
                        }
                        // In Production: Never reset existing admin password on normal startup.
                        if (admin != null && !userManager.IsInRoleAsync(admin, "Admin").GetAwaiter().GetResult())
                        {
                            userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                        }
                    }
                }
                else
                {
                    // Development environment admin seeding
                    string devAdminEmail = config?["AdminSeed:Email"] ?? config?["AdminUser:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@hamaracommerce.pk";
                    string devAdminPassword = config?["AdminSeed:Password"] ?? config?["AdminUser:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123!";
                    string devAdminFullName = config?["AdminUser:FullName"] ?? "Hamara Administrator";

                    var admin = userManager.FindByEmailAsync(devAdminEmail).GetAwaiter().GetResult();
                    if (admin == null)
                    {
                        admin = new ApplicationUser
                        {
                            UserName = devAdminEmail,
                            Email = devAdminEmail,
                            FullName = devAdminFullName,
                            PhoneNumber = "+92 300 0000000",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        userManager.CreateAsync(admin, devAdminPassword).GetAwaiter().GetResult();
                    }
                    else
                    {
                        var resetToken = userManager.GeneratePasswordResetTokenAsync(admin).GetAwaiter().GetResult();
                        userManager.ResetPasswordAsync(admin, resetToken, devAdminPassword).GetAwaiter().GetResult();
                    }

                    if (admin != null)
                    {
                        bool userUpdated = false;
                        if (!admin.EmailConfirmed)
                        {
                            admin.EmailConfirmed = true;
                            userUpdated = true;
                        }
                        if (!admin.IsActive)
                        {
                            admin.IsActive = true;
                            userUpdated = true;
                        }
                        if (userUpdated)
                        {
                            userManager.UpdateAsync(admin).GetAwaiter().GetResult();
                        }

                        userManager.SetLockoutEndDateAsync(admin, null).GetAwaiter().GetResult();
                        userManager.ResetAccessFailedCountAsync(admin).GetAwaiter().GetResult();

                        if (!userManager.IsInRoleAsync(admin, "Admin").GetAwaiter().GetResult())
                        {
                            userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                        }
                    }
                }

                // Seed Demo Customer User (Development only)
                if (isDevelopment)
                {
                    string customerEmail = "customer@hamaracommerce.pk";
                    demoCustomer = userManager.FindByEmailAsync(customerEmail).GetAwaiter().GetResult();
                    if (demoCustomer == null)
                    {
                        demoCustomer = new ApplicationUser
                        {
                            UserName = customerEmail,
                            Email = customerEmail,
                            FullName = "Usman Tariq",
                            PhoneNumber = "+92 300 9876543",
                            AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=250&q=80",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddYears(-1),
                            SavedAddresses = new List<Address>
                            {
                                new Address { Id = 1, Title = "Home Address", FullName = "Usman Tariq", StreetAddress = "House 14, Street 8, Sector Y, DHA Phase 3", City = "Lahore", State = "Punjab", ZipCode = "54792", IsDefault = true },
                                new Address { Id = 2, Title = "Corporate Office", FullName = "Usman Tariq", StreetAddress = "Floor 5, Software Technology Park, Gulberg III", City = "Lahore", State = "Punjab", ZipCode = "54660", IsDefault = false }
                            },
                            WishlistProductIds = new List<int> { 1, 5, 9, 17, 33 }
                        };
                        var custRes = userManager.CreateAsync(demoCustomer, "Customer@123!").GetAwaiter().GetResult();
                        if (custRes.Succeeded)
                        {
                            userManager.AddToRoleAsync(demoCustomer, "Customer").GetAwaiter().GetResult();
                        }
                    }
                }
            }

            // 5. SEED DEMO ORDERS (Development only - For Order Tracking & Admin Analytics)
            if (isDevelopment && !context.Orders.Any())
            {
                var sonyProduct = context.Products.FirstOrDefault(p => p.SKU == "ELEC-SONY-001");
                var iphoneProduct = context.Products.FirstOrDefault(p => p.SKU == "MOB-APPL-005");
                var nespressoProduct = context.Products.FirstOrDefault(p => p.SKU == "HOME-NESP-029");
                var sampleOrders = new List<Order>
                {
                    new Order
                    {
                        OrderNumber = "HC-PK-100482910",
                        TrackingNumber = "HC-98421049",
                        Currency = "PKR",
                        CustomerName = "Usman Tariq",
                        CustomerEmail = "customer@hamaracommerce.pk",
                        CustomerPhone = "+923009876543",
                        ShippingAddress = "House 14, Street 8, Sector Y, DHA Phase 3",
                        City = "Lahore",
                        State = "Punjab",
                        PostalCode = "54792",
                        Country = "Pakistan",
                        ShippingMethod = "Express Courier (1-2 Days)",
                        PaymentMethod = "Cash on Delivery",
                        PaymentStatus = PaymentStatus.Paid,
                        Status = OrderStatus.OutForDelivery,
                        OrderDate = DateTime.UtcNow.AddDays(-2),
                        EstimatedDeliveryDate = DateTime.UtcNow,
                        Subtotal = 349.99m,
                        DiscountAmount = 35.00m,
                        TaxAmount = 25.20m,
                        ShippingFee = 0.00m,
                        TotalAmount = 340.19m,
                        CouponCode = "SAVE10",
                        Items = new List<OrderItem>
                        {
                            new OrderItem { ProductId = 1, ProductTitle = "Sony WH-1000XM5 Wireless Headphones", ProductImage = "/images/placeholder-product.svg", SKU = "AUD-SONY-001", UnitPrice = 349.99m, Quantity = 1 }
                        }
                    },
                    new Order
                    {
                        OrderNumber = "HC-PK-200948123",
                        TrackingNumber = "HC-77123984",
                        Currency = "PKR",
                        CustomerName = "Sarah Jenkins",
                        CustomerEmail = "sarah.j@gmail.com",
                        CustomerPhone = "+923219876543",
                        ShippingAddress = "500 Clifton Block 2",
                        City = "Karachi",
                        State = "Sindh",
                        PostalCode = "75600",
                        Country = "Pakistan",
                        ShippingMethod = "Standard Courier (3-5 Days)",
                        PaymentMethod = "Credit Card",
                        PaymentStatus = PaymentStatus.Paid,
                        Status = OrderStatus.Delivered,
                        OrderDate = DateTime.UtcNow.AddDays(-7),
                        EstimatedDeliveryDate = DateTime.UtcNow.AddDays(-4),
                        Subtotal = 1199.00m,
                        DiscountAmount = 0.00m,
                        TaxAmount = 83.93m,
                        ShippingFee = 0.00m,
                        TotalAmount = 1282.93m,
                        Items = new List<OrderItem>
                        {
                            new OrderItem { ProductId = 5, ProductTitle = "Apple iPhone 15 Pro Max 256GB Titanium", ProductImage = "/images/placeholder-product.svg", SKU = "MOB-APPL-005", UnitPrice = 1199.00m, Quantity = 1 }
                        }
                    },
                    new Order
                    {
                        OrderNumber = "HC-PK-300481948",
                        TrackingNumber = "HC-44581920",
                        Currency = "PKR",
                        CustomerName = "Michael Scott",
                        CustomerEmail = "mscott@dundermifflin.com",
                        CustomerPhone = "+923334567890",
                        ShippingAddress = "Sector F-7/2 Street 15",
                        City = "Islamabad",
                        State = "Islamabad Capital Territory",
                        PostalCode = "44000",
                        Country = "Pakistan",
                        ShippingMethod = "Standard Courier (3-5 Days)",
                        PaymentMethod = "Cash on Delivery",
                        PaymentStatus = PaymentStatus.Pending,
                        Status = OrderStatus.Packed,
                        OrderDate = DateTime.UtcNow.AddDays(-1),
                        EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                        Subtotal = 159.00m,
                        DiscountAmount = 15.90m,
                        TaxAmount = 11.45m,
                        ShippingFee = 15.00m,
                        TotalAmount = 169.55m,
                        CouponCode = "SAVE10",
                        Items = new List<OrderItem>
                        {
                            new OrderItem { ProductId = 29, ProductTitle = "Nespresso VertuoPlus Coffee & Espresso Machine", ProductImage = "/images/placeholder-product.svg", SKU = "HOME-NESP-029", UnitPrice = 159.00m, Quantity = 1 }
                        }
                    }
                };
                if (sonyProduct != null) sampleOrders[0].Items[0].ProductId = sonyProduct.Id;
                if (iphoneProduct != null) sampleOrders[1].Items[0].ProductId = iphoneProduct.Id;
                if (nespressoProduct != null) sampleOrders[2].Items[0].ProductId = nespressoProduct.Id;

                if (demoCustomer != null)
                {
                    sampleOrders[0].UserId = demoCustomer.Id;
                    sampleOrders[1].UserId = demoCustomer.Id;
                    sampleOrders[2].UserId = demoCustomer.Id;
                }

                context.Orders.AddRange(sampleOrders);
                context.SaveChanges();
            }

            // 6. BACKFILL HISTORICAL COUPON REDEMPTIONS
            var ordersWithCoupons = context.Orders.Where(o => !string.IsNullOrEmpty(o.CouponCode)).ToList();
            if (ordersWithCoupons.Any())
            {
                var existingRedemptions = context.CouponRedemptions.Select(r => new { r.OrderId, r.CouponCode }).ToList();
                var existingSet = new HashSet<(int OrderId, string CouponCode)>(
                    existingRedemptions.Select(r => (r.OrderId, r.CouponCode.ToUpperInvariant())));

                var coupons = context.Coupons.ToDictionary(c => c.Code.ToUpperInvariant());

                foreach (var order in ordersWithCoupons)
                {
                    var code = order.CouponCode!.Trim().ToUpperInvariant();
                    if (existingSet.Contains((order.Id, code))) continue;
                    if (!coupons.TryGetValue(code, out var coupon)) continue;

                    bool wasRestored = false;
                    string? restoreReason = null;
                    DateTime? restoredAt = null;

                    if (!string.IsNullOrEmpty(order.CustomerNotes) &&
                        (order.CustomerNotes.Contains("[CouponRestored:") || order.CustomerNotes.Contains("[COUPON_RESTORED:")))
                    {
                        wasRestored = true;
                        restoreReason = "Backfilled from historical CustomerNotes marker";
                        restoredAt = order.OrderDate;

                        order.CustomerNotes = System.Text.RegularExpressions.Regex.Replace(
                            order.CustomerNotes, @"\[(CouponRestored|COUPON_RESTORED):[^\]]+\]", "").Trim();
                    }
                    else if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Refunded)
                    {
                        wasRestored = true;
                        restoreReason = "Order previously cancelled/refunded";
                        restoredAt = order.OrderDate;
                    }

                    context.CouponRedemptions.Add(new CouponRedemption
                    {
                        CouponId = coupon.Id,
                        CouponCode = coupon.Code,
                        OrderId = order.Id,
                        UserId = order.UserId,
                        CustomerEmail = order.CustomerEmail,
                        DiscountAmount = order.DiscountAmount,
                        RedeemedAt = order.OrderDate,
                        IsRestored = wasRestored,
                        RestoredAt = restoredAt,
                        RestoreReason = restoreReason
                    });
                    existingSet.Add((order.Id, code));
                }
                context.SaveChanges();
            }
        }
    }
}
