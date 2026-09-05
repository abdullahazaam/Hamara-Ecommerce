using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using HamaraCommerce.Models;

namespace HamaraCommerce.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
        public DbSet<StoreSetting> StoreSettings { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }
        public DbSet<CheckoutIdempotencyRecord> CheckoutIdempotencyRecords { get; set; }
        public DbSet<CouponRedemption> CouponRedemptions { get; set; }
        public DbSet<EmailOutboxMessage> EmailOutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // VALUE COMPARERS FOR JSON COLLECTIONS
            // ==========================================
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            var intListComparer = new ValueComparer<List<int>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            var dictComparer = new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1 != null && c2 != null && c1.OrderBy(kv => kv.Key).SequenceEqual(c2.OrderBy(kv => kv.Key)),
                c => c.Aggregate(0, (a, kv) => HashCode.Combine(a, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
                c => new Dictionary<string, string>(c));

            var addressListComparer = new ValueComparer<List<Address>>(
                (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                c => JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<List<Address>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions) ?? new List<Address>());

            // ==========================================
            // APPLICATION USER (IDENTITY) CONFIGURATION
            // ==========================================
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
                entity.Property(u => u.AvatarUrl).HasMaxLength(500);

                entity.Property(u => u.SavedAddresses)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<List<Address>>(v, jsonOptions) ?? new List<Address>())
                    .Metadata.SetValueComparer(addressListComparer);

                entity.Property(u => u.WishlistProductIds)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>())
                    .Metadata.SetValueComparer(intListComparer);
            });

            // ==========================================
            // CATEGORY CONFIGURATION
            // ==========================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Slug).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Icon).HasMaxLength(50);
                entity.Property(c => c.ImageUrl).HasMaxLength(500);
                entity.Property(c => c.Description).HasMaxLength(500);

                entity.HasIndex(c => c.Slug).IsUnique();
                entity.HasIndex(c => c.Name);
                entity.HasIndex(c => c.DisplayOrder);
            });

            // ==========================================
            // PRODUCT CONFIGURATION
            // ==========================================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Title).IsRequired().HasMaxLength(250);
                entity.Property(p => p.Slug).IsRequired().HasMaxLength(250);
                entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
                entity.Property(p => p.CategoryName).HasMaxLength(100);
                entity.Property(p => p.Brand).HasMaxLength(100);
                entity.Property(p => p.ShortDescription).HasMaxLength(1000);
                entity.Property(p => p.FullDescription).HasColumnType("nvarchar(max)");
                entity.Property(p => p.Dimensions).HasMaxLength(100);
                entity.Property(p => p.DeliveryEstimate).HasMaxLength(100);
                entity.Property(p => p.ReturnPolicy).HasMaxLength(250);
                entity.Property(p => p.Warranty).HasMaxLength(100);

                // Decimal Precision
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.OldPrice).HasPrecision(18, 2);
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);
                entity.Property(p => p.WeightKg).HasPrecision(18, 2);

                // Concurrency RowVersion
                entity.Property(p => p.RowVersion).IsRowVersion();

                // JSON Specifications
                entity.Property(p => p.Specifications)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions) ?? new Dictionary<string, string>())
                    .Metadata.SetValueComparer(dictComparer);

                // Indexes & Unique Constraints
                entity.HasIndex(p => p.Slug).IsUnique();
                entity.HasIndex(p => p.SKU).IsUnique();
                entity.HasIndex(p => p.Title);
                entity.HasIndex(p => p.CategoryName);
                entity.HasIndex(p => p.Brand);
                entity.HasIndex(p => p.Price);
                entity.HasIndex(p => p.Rating);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.IsFeatured);
                entity.HasIndex(p => p.IsTrending);
                entity.HasIndex(p => p.IsFlashDeal);
                entity.HasIndex(p => p.IsBestSeller);
                entity.HasIndex(p => p.IsNewArrival);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Images)
                    .WithOne(i => i.Product)
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Variants)
                    .WithOne(v => v.Product)
                    .HasForeignKey(v => v.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.InventoryMovements)
                    .WithOne(m => m.Product)
                    .HasForeignKey(m => m.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // PRODUCT IMAGE CONFIGURATION
            // ==========================================
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(i => i.AltText).HasMaxLength(250);

                entity.HasIndex(i => i.ProductId);
                entity.HasIndex(i => i.SortOrder);
            });

            // ==========================================
            // PRODUCT VARIANT CONFIGURATION
            // ==========================================
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("ProductVariants");
                entity.HasKey(v => v.Id);

                entity.Property(v => v.SKU).IsRequired().HasMaxLength(50);
                entity.Property(v => v.Name).IsRequired().HasMaxLength(150);
                entity.Property(v => v.Color).HasMaxLength(50);
                entity.Property(v => v.Size).HasMaxLength(50);
                entity.Property(v => v.Storage).HasMaxLength(50);

                entity.Property(v => v.PriceAdjustment).HasPrecision(18, 2);

                entity.HasIndex(v => v.ProductId);
                entity.HasIndex(v => v.SKU);
            });

            // ==========================================
            // INVENTORY MOVEMENT CONFIGURATION
            // ==========================================
            modelBuilder.Entity<InventoryMovement>(entity =>
            {
                entity.ToTable("InventoryMovements");
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Reason).IsRequired().HasMaxLength(250);
                entity.Property(m => m.AdminUserId).HasMaxLength(450);
                entity.Property(m => m.AdminUserName).HasMaxLength(150);

                entity.HasIndex(m => m.ProductId);
                entity.HasIndex(m => m.VariantId);
                entity.HasIndex(m => m.CreatedAt);

                entity.HasOne(m => m.Variant)
                    .WithMany(v => v.InventoryMovements)
                    .HasForeignKey(m => m.VariantId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.Order)
                    .WithMany()
                    .HasForeignKey(m => m.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==========================================
            // REVIEW CONFIGURATION
            // ==========================================
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.UserId).HasMaxLength(450);
                entity.Property(r => r.AuthorEmail).HasMaxLength(150);
                entity.Property(r => r.UserName).IsRequired().HasMaxLength(100);
                entity.Property(r => r.UserAvatar).HasMaxLength(500);
                entity.Property(r => r.Title).HasMaxLength(200);
                entity.Property(r => r.Comment).HasMaxLength(2000);
                entity.Property(r => r.ImageUrl).HasMaxLength(500);

                entity.HasIndex(r => r.ProductId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.IsApproved);
                entity.HasIndex(r => r.Rating);
                entity.HasIndex(r => r.Date);

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // QUESTION ANSWER CONFIGURATION
            // ==========================================
            modelBuilder.Entity<QuestionAnswer>(entity =>
            {
                entity.ToTable("QuestionAnswers");
                entity.HasKey(q => q.Id);

                entity.Property(q => q.UserId).HasMaxLength(450);
                entity.Property(q => q.Question).IsRequired().HasMaxLength(1000);
                entity.Property(q => q.AskedBy).HasMaxLength(100);
                entity.Property(q => q.Answer).HasMaxLength(2000);
                entity.Property(q => q.AnsweredBy).HasMaxLength(100);

                entity.HasIndex(q => q.ProductId);
                entity.HasIndex(q => q.UserId);
                entity.HasIndex(q => q.IsApproved);
                entity.HasIndex(q => q.IsAnswered);

                entity.HasOne(q => q.Product)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(q => q.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // ORDER CONFIGURATION
            // ==========================================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);

                entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(o => o.TrackingNumber).IsRequired().HasMaxLength(50);
                entity.Property(o => o.Currency).IsRequired().HasMaxLength(10);
                entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(150);
                entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(150);
                entity.Property(o => o.CustomerPhone).HasMaxLength(50);
                entity.Property(o => o.ShippingAddress).HasMaxLength(250);
                entity.Property(o => o.City).HasMaxLength(100);
                entity.Property(o => o.State).HasMaxLength(100);
                entity.Property(o => o.PostalCode).HasMaxLength(50);
                entity.Property(o => o.Country).HasMaxLength(100);
                entity.Property(o => o.ShippingMethod).HasMaxLength(100);
                entity.Property(o => o.PaymentMethod).HasMaxLength(100);
                entity.Property(o => o.CouponCode).HasMaxLength(50);
                entity.Property(o => o.CustomerNotes).HasMaxLength(1000);
                entity.Property(o => o.GuestAccessToken).HasMaxLength(64);

                // Decimal Precision
                entity.Property(o => o.Subtotal).HasPrecision(18, 2);
                entity.Property(o => o.DiscountAmount).HasPrecision(18, 2);
                entity.Property(o => o.TaxAmount).HasPrecision(18, 2);
                entity.Property(o => o.ShippingFee).HasPrecision(18, 2);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                // Indexes & Unique Constraints
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.HasIndex(o => o.TrackingNumber).IsUnique();
                entity.HasIndex(o => o.CustomerEmail);
                entity.HasIndex(o => o.GuestAccessToken);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.PaymentStatus);
                entity.HasIndex(o => o.UserId);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(o => o.Payments)
                    .WithOne(p => p.Order)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // ORDER ITEM CONFIGURATION
            // ==========================================
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.ProductTitle).IsRequired().HasMaxLength(250);
                entity.Property(i => i.ProductImage).HasMaxLength(500);
                entity.Property(i => i.SKU).HasMaxLength(50);
                entity.Property(i => i.VariantName).HasMaxLength(150);

                // Decimal Precision
                entity.Property(i => i.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(i => i.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Variant)
                    .WithMany(v => v.OrderItems)
                    .HasForeignKey(i => i.VariantId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==========================================
            // PAYMENT TRANSACTION CONFIGURATION
            // ==========================================
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("PaymentTransactions");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.TransactionReference).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Provider).IsRequired().HasMaxLength(100);
                entity.Property(p => p.ProviderTransactionId).HasMaxLength(100);
                entity.Property(p => p.Currency).IsRequired().HasMaxLength(10);
                entity.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(100);
                entity.Property(p => p.CardLast4).HasMaxLength(10);
                entity.Property(p => p.CardBrand).HasMaxLength(50);
                entity.Property(p => p.FailureReason).HasMaxLength(500);

                // Decimal Precision
                entity.Property(p => p.Amount).HasPrecision(18, 2);

                entity.HasIndex(p => p.OrderId);
                entity.HasIndex(p => p.TransactionReference).IsUnique();
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.CreatedAt);
            });

            // ==========================================
            // COUPON CONFIGURATION
            // ==========================================
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.ToTable("Coupons");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Code).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Description).HasMaxLength(250);

                // Decimal Precision
                entity.Property(c => c.FixedDiscountAmount).HasPrecision(18, 2);
                entity.Property(c => c.MinimumSpend).HasPrecision(18, 2);
                entity.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);

                entity.HasIndex(c => c.Code).IsUnique();
                entity.HasIndex(c => c.IsActive);
                entity.HasIndex(c => c.StartDate);
                entity.HasIndex(c => c.ExpiryDate);

                entity.HasOne(c => c.ApplicableCategory)
                    .WithMany()
                    .HasForeignKey(c => c.ApplicableCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.ApplicableProduct)
                    .WithMany()
                    .HasForeignKey(c => c.ApplicableProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==========================================
            // ADMIN AUDIT LOG CONFIGURATION
            // ==========================================
            modelBuilder.Entity<AdminAuditLog>(entity =>
            {
                entity.ToTable("AdminAuditLogs");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.AdminUserId).HasMaxLength(450);
                entity.Property(a => a.AdminUserName).HasMaxLength(150);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
                entity.Property(a => a.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(a => a.EntityId).HasMaxLength(100);
                entity.Property(a => a.Details).HasMaxLength(2000);
                entity.Property(a => a.IpAddress).HasMaxLength(50);

                entity.HasIndex(a => a.AdminUserId);
                entity.HasIndex(a => a.Action);
                entity.HasIndex(a => a.EntityType);
                entity.HasIndex(a => a.CreatedAt);
            });

            // ==========================================
            // STORE SETTINGS CONFIGURATION
            // ==========================================
            modelBuilder.Entity<StoreSetting>(entity =>
            {
                entity.ToTable("StoreSettings");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.StoreName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.StoreEmail).IsRequired().HasMaxLength(150);
                entity.Property(s => s.StorePhone).IsRequired().HasMaxLength(50);
                entity.Property(s => s.StoreAddress).IsRequired().HasMaxLength(250);
                entity.Property(s => s.CurrencyCode).IsRequired().HasMaxLength(10);
                entity.Property(s => s.CurrencySymbol).IsRequired().HasMaxLength(10);

                entity.Property(s => s.TaxRatePercent).HasPrecision(18, 2);
                entity.Property(s => s.FreeShippingThreshold).HasPrecision(18, 2);
                entity.Property(s => s.StandardShippingFee).HasPrecision(18, 2);
                entity.Property(s => s.ExpressShippingFee).HasPrecision(18, 2);
            });

            // ==========================================
            // CONTACT MESSAGES CONFIGURATION
            // ==========================================
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.ToTable("ContactMessages");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Subject).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Message).IsRequired().HasMaxLength(2500);
                entity.Property(c => c.PhoneNumber).HasMaxLength(30);
                entity.Property(c => c.IpAddress).HasMaxLength(50);
                entity.Property(c => c.AdminNotes).HasMaxLength(1000);

                entity.HasIndex(c => c.Email);
                entity.HasIndex(c => c.CreatedAt);
                entity.HasIndex(c => c.IsRead);
            });

            // ==========================================
            // NEWSLETTER SUBSCRIPTIONS CONFIGURATION
            // ==========================================
            modelBuilder.Entity<NewsletterSubscription>(entity =>
            {
                entity.ToTable("NewsletterSubscriptions");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Email).IsRequired().HasMaxLength(150);
                entity.Property(n => n.UnsubscribeToken).IsRequired().HasMaxLength(100);
                entity.Property(n => n.IpAddress).HasMaxLength(50);

                entity.HasIndex(n => n.Email).IsUnique();
                entity.HasIndex(n => n.UnsubscribeToken).IsUnique();
                entity.HasIndex(n => n.IsActive);
                entity.HasIndex(n => n.SubscribedAt);
            });

            // ==========================================
            // CHECKOUT IDEMPOTENCY RECORD CONFIGURATION
            // ==========================================
            modelBuilder.Entity<CheckoutIdempotencyRecord>(entity =>
            {
                entity.ToTable("CheckoutIdempotencyRecords");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.IdempotencyKey).IsRequired().HasMaxLength(64);
                entity.Property(r => r.UserId).HasMaxLength(450);
                entity.Property(r => r.CustomerEmail).IsRequired().HasMaxLength(150);
                entity.Property(r => r.RequestHash).IsRequired().HasMaxLength(128);
                entity.Property(r => r.OrderNumber).HasMaxLength(50);
                entity.Property(r => r.GuestAccessToken).HasMaxLength(64);
                entity.Property(r => r.PaymentProvider).HasMaxLength(50);
                entity.Property(r => r.PaymentReference).HasMaxLength(100);
                entity.Property(r => r.PaymentAmount).HasPrecision(18, 2);
                entity.Property(r => r.FailureReason).HasMaxLength(500);

                entity.HasIndex(r => r.IdempotencyKey).IsUnique();
                entity.HasIndex(r => r.CreatedAt);
                entity.HasIndex(r => r.CustomerEmail);
                entity.HasIndex(r => r.Status);
            });

            // ==========================================
            // COUPON REDEMPTION CONFIGURATION
            // ==========================================
            modelBuilder.Entity<CouponRedemption>(entity =>
            {
                entity.ToTable("CouponRedemptions");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.CouponCode).IsRequired().HasMaxLength(50);
                entity.Property(r => r.UserId).HasMaxLength(450);
                entity.Property(r => r.CustomerEmail).IsRequired().HasMaxLength(150);
                entity.Property(r => r.DiscountAmount).HasPrecision(18, 2);
                entity.Property(r => r.RestoreReason).HasMaxLength(250);

                entity.HasIndex(r => r.CouponCode);
                entity.HasIndex(r => r.OrderId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.CustomerEmail);
                entity.HasIndex(r => r.IsRestored);

                entity.HasOne(r => r.Coupon)
                    .WithMany()
                    .HasForeignKey(r => r.CouponId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Order)
                    .WithMany()
                    .HasForeignKey(r => r.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // EMAIL OUTBOX CONFIGURATION
            // ==========================================
            modelBuilder.Entity<EmailOutboxMessage>(entity =>
            {
                entity.ToTable("EmailOutboxMessages");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EventKey).HasMaxLength(150);
                entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
                entity.Property(e => e.HtmlBody).IsRequired();
                entity.Property(e => e.LastError).HasMaxLength(1000);

                entity.HasIndex(e => e.EventKey)
                    .IsUnique()
                    .HasFilter("[EventKey] IS NOT NULL");

                entity.HasIndex(e => new { e.Status, e.NextAttemptAt });
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
