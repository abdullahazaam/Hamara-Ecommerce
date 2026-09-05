using System;
using System.Collections.Generic;
using System.Linq;
using HamaraCommerce.Models;

namespace HamaraCommerce.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>? userManager = null, Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>? roleManager = null, Microsoft.Extensions.Configuration.IConfiguration? config = null, bool isDevelopment = false)
        {
            // Database migrated via EF Core Migrations

            // 1. SEED CATEGORIES (15 Categories)
            if (!context.Categories.Any())
            {
            var categories = new List<Category>
            {
                new Category { Name = "Electronics", Slug = "electronics", Icon = "fa-plug", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?auto=format&fit=crop&w=600&q=80", Description = "Cutting-edge gadgets & audio tech" },
                new Category { Name = "Mobile Phones", Slug = "mobile-phones", Icon = "fa-mobile-screen-button", ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=600&q=80", Description = "Smartphones, wearables & accessories" },
                new Category { Name = "Laptops", Slug = "laptops", Icon = "fa-laptop", ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=600&q=80", Description = "High performance laptops & ultrabooks" },
                new Category { Name = "Fashion", Slug = "fashion", Icon = "fa-shirt", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?auto=format&fit=crop&w=600&q=80", Description = "Trending apparel for Men & Women" },
                new Category { Name = "Shoes", Slug = "shoes", Icon = "fa-shoe-prints", ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=600&q=80", Description = "Athletic sneakers, boots & luxury footwear" },
                new Category { Name = "Watches", Slug = "watches", Icon = "fa-clock", ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=600&q=80", Description = "Classic chronographs & smartwatches" },
                new Category { Name = "Beauty", Slug = "beauty", Icon = "fa-wand-magic-sparkles", ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=600&q=80", Description = "Skincare, cosmetics & luxury fragrances" },
                new Category { Name = "Home & Kitchen", Slug = "home-kitchen", Icon = "fa-kitchen-set", ImageUrl = "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?auto=format&fit=crop&w=600&q=80", Description = "Modern appliances & culinary tools" },
                new Category { Name = "Gaming", Slug = "gaming", Icon = "fa-gamepad", ImageUrl = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=600&q=80", Description = "Consoles, RGB gear & peripherals" },
                new Category { Name = "Furniture", Slug = "furniture", Icon = "fa-couch", ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=600&q=80", Description = "Ergonomic chairs & luxury home decor" },
                new Category { Name = "Grocery", Slug = "grocery", Icon = "fa-basket-shopping", ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=600&q=80", Description = "Organic produce & gourmet snacks" },
                new Category { Name = "Sports", Slug = "sports", Icon = "fa-dumbbell", ImageUrl = "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=600&q=80", Description = "Fitness equipment, yoga & outdoors" },
                new Category { Name = "Books", Slug = "books", Icon = "fa-book-open", ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=600&q=80", Description = "Best-selling fiction & technical guides" },
                new Category { Name = "Automotive", Slug = "automotive", Icon = "fa-car-side", ImageUrl = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=600&q=80", Description = "Car accessories, electronics & care" },
                new Category { Name = "Pet Supplies", Slug = "pet-supplies", Icon = "fa-paw", ImageUrl = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=600&q=80", Description = "Premium pet food & interactive toys" }
            };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var catDict = context.Categories.ToDictionary(c => c.Name.ToLower(), c => c.Id);

            // 2. SEED 60+ PRODUCTS
            if (!context.Products.Any())
            {
                var products = new List<Product>();

            // --- Category 1: Electronics (4 Products) ---
            products.Add(new Product {
                Title = "Sony WH-1000XM5 Wireless Headphones", SKU = "ELEC-SONY-001", CategoryId = 1, CategoryName = "Electronics", Brand = "Sony",
                Price = 349.99m, OldPrice = 399.99m, DiscountPercentage = 12.5, Rating = 4.9, ReviewCount = 342, Stock = 45,
                MainImage = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1546435770-a3e426bf472b?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Industry-leading noise canceling headphones with dual processors and 8 microphones.",
                FullDescription = "Experience unprecedented silence and crystal clear sound with the Sony WH-1000XM5. Designed with lightweight leather and precision drivers for audiophiles.",
                IsFeatured = true, IsTrending = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Bose SoundLink Flex Bluetooth Speaker", SKU = "ELEC-BOSE-002", CategoryId = 1, CategoryName = "Electronics", Brand = "Bose",
                Price = 129.99m, OldPrice = 149.99m, DiscountPercentage = 13.3, Rating = 4.7, ReviewCount = 188, Stock = 60,
                MainImage = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1545454675-3531b543be5d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Waterproof rugged outdoor speaker with PositionIQ technology for optimal acoustic clarity.",
                FullDescription = "Take deep, immersive sound wherever adventure calls. Built to withstand water, dust, and rust.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(2)
            });
            products.Add(new Product {
                Title = "Anker 737 Power Bank (PowerCore 24K)", SKU = "ELEC-ANKER-003", CategoryId = 1, CategoryName = "Electronics", Brand = "Anker",
                Price = 109.99m, OldPrice = 149.99m, DiscountPercentage = 26.6, Rating = 4.8, ReviewCount = 512, Stock = 85,
                MainImage = "https://images.unsplash.com/photo-1609592424009-8809477e74eb?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Ultra-powerful 140W bi-directional charging power bank with smart digital display.",
                FullDescription = "Charge your MacBook Pro, iPhone, and iPad simultaneously at maximum speed with 24,000mAh capacity.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Samsung 49\" Odyssey G9 Curved Gaming Monitor", SKU = "ELEC-SAMS-004", CategoryId = 1, CategoryName = "Electronics", Brand = "Samsung",
                Price = 1199.99m, OldPrice = 1499.99m, DiscountPercentage = 20.0, Rating = 4.8, ReviewCount = 94, Stock = 12,
                MainImage = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Super ultrawide 240Hz 1ms OLED gaming monitor with 1000R curvature.",
                FullDescription = "Wrap your view in stunning QLED colors and ultra-fast 240Hz refresh rate for ultimate gaming immersion.",
                IsFeatured = true
            });

            // --- Category 2: Mobile Phones (4 Products) ---
            products.Add(new Product {
                Title = "Apple iPhone 15 Pro Max 256GB Titanium", SKU = "MOB-APPL-005", CategoryId = 2, CategoryName = "Mobile Phones", Brand = "Apple",
                Price = 1199.00m, OldPrice = 1299.00m, DiscountPercentage = 7.7, Rating = 4.9, ReviewCount = 890, Stock = 30,
                MainImage = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Forged in titanium with A17 Pro chip, customizable Action button, and 5x Telephoto camera.",
                FullDescription = "The strongest iPhone ever built. Groundbreaking graphics performance and pro camera system.",
                IsFeatured = true, IsBestSeller = true, IsTrending = true
            });
            products.Add(new Product {
                Title = "Samsung Galaxy S24 Ultra 512GB Titanium Gray", SKU = "MOB-SAMS-006", CategoryId = 2, CategoryName = "Mobile Phones", Brand = "Samsung",
                Price = 1299.99m, OldPrice = 1419.99m, DiscountPercentage = 8.5, Rating = 4.8, ReviewCount = 420, Stock = 25,
                MainImage = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1580910051074-3eb694886505?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Welcome to the era of Galaxy AI. Live Translate, Circle to Search, and 200MP camera system.",
                FullDescription = "Empower your creativity with S Pen precision and Snapdragon 8 Gen 3 for Galaxy processor.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Google Pixel 8 Pro 128GB Obsidian", SKU = "MOB-GOOG-007", CategoryId = 2, CategoryName = "Mobile Phones", Brand = "Google",
                Price = 799.00m, OldPrice = 999.00m, DiscountPercentage = 20.0, Rating = 4.6, ReviewCount = 210, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Google Tensor G3 powers advanced AI photo editing, Magic Eraser, and best-in-class security.",
                FullDescription = "Super Actua display, fully upgraded triple camera system, and 7 years of OS support.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(1)
            });
            products.Add(new Product {
                Title = "OnePlus 12 5G 16GB RAM 512GB Emerald", SKU = "MOB-ONEP-008", CategoryId = 2, CategoryName = "Mobile Phones", Brand = "OnePlus",
                Price = 799.99m, OldPrice = 899.99m, DiscountPercentage = 11.1, Rating = 4.7, ReviewCount = 135, Stock = 18,
                MainImage = "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Smooth Beyond Belief with 4th Gen Hasselblad Camera and 100W SUPERVOOC charging.",
                FullDescription = "Dual Vapor Chamber cooling, Snapdragon 8 Gen 3 CPU, and 2K 120Hz ProXDR display.",
                IsNewArrival = true
            });

            // --- Category 3: Laptops (4 Products) ---
            products.Add(new Product {
                Title = "Apple MacBook Pro 16\" M3 Max (36GB RAM, 1TB SSD)", SKU = "LAP-APPL-009", CategoryId = 3, CategoryName = "Laptops", Brand = "Apple",
                Price = 3499.00m, OldPrice = 3699.00m, DiscountPercentage = 5.4, Rating = 4.9, ReviewCount = 610, Stock = 15,
                MainImage = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Mind-blowing performance with Liquid Retina XDR display and 22 hours battery life.",
                FullDescription = "Built for developers, 3D artists, and video editors demanding extreme computational speed.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Dell XPS 15 9530 OLED Touch Intel i9 32GB RAM", SKU = "LAP-DELL-010", CategoryId = 3, CategoryName = "Laptops", Brand = "Dell",
                Price = 2199.99m, OldPrice = 2499.99m, DiscountPercentage = 12.0, Rating = 4.7, ReviewCount = 180, Stock = 20,
                MainImage = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Precision CNC machined aluminum chassis with 3.5K OLED touchscreen display.",
                FullDescription = "NVIDIA GeForce RTX 4060 graphics paired with 13th Gen Intel Core i9 processor for high productivity.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "ASUS ROG Zephyrus G16 Gaming Laptop RTX 4080", SKU = "LAP-ASUS-011", CategoryId = 3, CategoryName = "Laptops", Brand = "ASUS",
                Price = 2699.99m, OldPrice = 2899.99m, DiscountPercentage = 6.9, Rating = 4.8, ReviewCount = 145, Stock = 10,
                MainImage = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Ultra-slim OLED gaming laptop with Intel Core Ultra 9 and ROG Nebula 240Hz display.",
                FullDescription = "Uncompromising mobile power packed into a sleek CNC aluminum chassis with dynamic RGB keyboard.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Lenovo ThinkPad X1 Carbon Gen 11 Ultrabook", SKU = "LAP-LENV-012", CategoryId = 3, CategoryName = "Laptops", Brand = "Lenovo",
                Price = 1499.00m, OldPrice = 1799.00m, DiscountPercentage = 16.6, Rating = 4.6, ReviewCount = 220, Stock = 35,
                MainImage = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Ultra-lightweight business notebook tested against 12 military-grade durability standards.",
                FullDescription = "Legendary TrackPoint keyboard, 14-inch 2.8K OLED anti-glare display, and Intel Evo certification.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(3)
            });

            // --- Category 4: Fashion (4 Products) ---
            products.Add(new Product {
                Title = "Men's Italian Wool Slim-Fit Blazer Jacket", SKU = "FASH-MEN-013", CategoryId = 4, CategoryName = "Fashion", Brand = "Hugo Boss",
                Price = 299.99m, OldPrice = 450.00m, DiscountPercentage = 33.3, Rating = 4.8, ReviewCount = 112, Stock = 50,
                MainImage = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1594938298603-c8148c4dae35?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Tailored from premium Super 120s virgin Italian wool with double-vented back.",
                FullDescription = "Sophisticated corporate and evening style featuring notch lapels, interior jet pockets, and silk lining.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Women's Cashmere Trench Coat - Camel", SKU = "FASH-WOM-014", CategoryId = 4, CategoryName = "Fashion", Brand = "Burberry",
                Price = 599.00m, OldPrice = 799.00m, DiscountPercentage = 25.0, Rating = 4.9, ReviewCount = 86, Stock = 18,
                MainImage = "https://images.unsplash.com/photo-1539533018447-63fcce2678e3?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Timeless double-breasted silhouette woven from 100% pure Mongolian cashmere.",
                FullDescription = "Iconic vintage check lining, horn buttons, and waist belt for an effortless elegant profile.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Unisex Designer Heavyweight Hooded Sweatshirt", SKU = "FASH-UNI-015", CategoryId = 4, CategoryName = "Fashion", Brand = "Essentials",
                Price = 85.00m, OldPrice = 110.00m, DiscountPercentage = 22.7, Rating = 4.7, ReviewCount = 450, Stock = 120,
                MainImage = "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1509967419530-da38b4704bc6?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "480GSM fleece construction with relaxed dropped shoulders and minimalist branding.",
                FullDescription = "Maximum warmth and ultra-soft plush touch. Pre-shrunk cotton blend for long-lasting wear.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Women's Silk Floral Wrap Maxi Dress", SKU = "FASH-WOM-016", CategoryId = 4, CategoryName = "Fashion", Brand = "ZARA",
                Price = 129.99m, OldPrice = 179.99m, DiscountPercentage = 27.7, Rating = 4.6, ReviewCount = 74, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Ethereal mulberry silk maxi dress featuring hand-painted botanical motifs.",
                FullDescription = "Adjustable waist tie, ruffle trim hemline, and breathable V-neck design for summer elegance.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(2)
            });

            // --- Category 5: Shoes (4 Products) ---
            products.Add(new Product {
                Title = "Nike Air Max 270 React Running Shoes", SKU = "SHOE-NIKE-017", CategoryId = 5, CategoryName = "Shoes", Brand = "Nike",
                Price = 159.99m, OldPrice = 189.99m, DiscountPercentage = 15.7, Rating = 4.8, ReviewCount = 980, Stock = 75,
                MainImage = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Nike's biggest heel Air unit yet delivers unrivaled all-day bounce and responsive cushioning.",
                FullDescription = "Combining light React foam with bold aesthetic layering for ultimate street style performance.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Adidas Ultraboost Light Running Shoes", SKU = "SHOE-ADID-018", CategoryId = 5, CategoryName = "Shoes", Brand = "Adidas",
                Price = 189.99m, OldPrice = 210.00m, DiscountPercentage = 9.5, Rating = 4.7, ReviewCount = 530, Stock = 60,
                MainImage = "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1608231387042-66d1773070a5?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "30% lighter Light BOOST material engineered with Primeknit+ textile upper.",
                FullDescription = "Continental Rubber outsole guarantees supreme traction across wet and dry urban terrain.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Jordan 1 Retro High OG Chicago Lost & Found", SKU = "SHOE-JORD-019", CategoryId = 5, CategoryName = "Shoes", Brand = "Jordan",
                Price = 249.99m, OldPrice = 299.99m, DiscountPercentage = 16.6, Rating = 4.9, ReviewCount = 1420, Stock = 22,
                MainImage = "https://images.unsplash.com/photo-1552346154-21d32810aba3?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1514989940723-e8e51635b782?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "The legendary 1985 basketball icon restored with vintage cracked leather finish.",
                FullDescription = "A holy grail sneaker release featuring original red, white, and black leather paneling.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Timberland 6-Inch Premium Waterproof Boot", SKU = "SHOE-TIMB-020", CategoryId = 5, CategoryName = "Shoes", Brand = "Timberland",
                Price = 198.00m, OldPrice = 220.00m, DiscountPercentage = 10.0, Rating = 4.7, ReviewCount = 380, Stock = 45,
                MainImage = "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Direct-attach seam-sealed waterproof construction in nubuck wheat leather.",
                FullDescription = "Anti-fatigue technology insoles and 400g PrimaLoft insulation keep feet comfortable in all conditions.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(4)
            });

            // --- Category 6: Watches (4 Products) ---
            products.Add(new Product {
                Title = "Apple Watch Ultra 2 GPS + Cellular 49mm Titanium", SKU = "WATCH-APPL-021", CategoryId = 6, CategoryName = "Watches", Brand = "Apple",
                Price = 799.00m, OldPrice = 849.00m, DiscountPercentage = 5.8, Rating = 4.9, ReviewCount = 490, Stock = 35,
                MainImage = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1434493789847-2f02dc6ca35d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "The ultimate sports & adventure watch with 3000 nits display and dual-frequency GPS.",
                FullDescription = "Customizable Action button, 100m water resistance, EN13319 scuba certification, and 36h battery life.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Seiko Prospex Speedtimer Solar Chronograph", SKU = "WATCH-SEIK-022", CategoryId = 6, CategoryName = "Watches", Brand = "Seiko",
                Price = 675.00m, OldPrice = 775.00m, DiscountPercentage = 12.9, Rating = 4.8, ReviewCount = 145, Stock = 20,
                MainImage = "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Panda dial solar-powered precision watch charging from any light source.",
                FullDescription = "Curved sapphire crystal with anti-reflective coating, 6-month power reserve, and stainless steel bracelet.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Tissot PRX Powermatic 80 Automatic Blue Dial", SKU = "WATCH-TISS-023", CategoryId = 6, CategoryName = "Watches", Brand = "Tissot",
                Price = 725.00m, OldPrice = 825.00m, DiscountPercentage = 12.1, Rating = 4.9, ReviewCount = 310, Stock = 18,
                MainImage = "https://images.unsplash.com/photo-1539185441755-769473a23570?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "1970s integrated bracelet design powered by Swiss Powermatic 80 caliber with 80h power reserve.",
                FullDescription = "Waffle textured blue dial, Nivachron balance spring, and transparent sapphire case back.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Fossil Gen 6 Smartwatch Touchscreen Stainless Steel", SKU = "WATCH-FOSS-024", CategoryId = 6, CategoryName = "Watches", Brand = "Fossil",
                Price = 179.99m, OldPrice = 299.99m, DiscountPercentage = 40.0, Rating = 4.5, ReviewCount = 195, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Powered by Wear OS by Google with SpO2 sensor, continuous heart rate tracking and fast charging.",
                FullDescription = "Charges to 80% in just 30 minutes. Customizable watch faces and contactless payments via Google Wallet.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(1)
            });

            // --- Category 7: Beauty (4 Products) ---
            products.Add(new Product {
                Title = "Estée Lauder Advanced Night Repair Serum 50ml", SKU = "BEAU-ESTE-025", CategoryId = 7, CategoryName = "Beauty", Brand = "Estée Lauder",
                Price = 115.00m, OldPrice = 135.00m, DiscountPercentage = 14.8, Rating = 4.8, ReviewCount = 760, Stock = 80,
                MainImage = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1571781926291-c477ebfd024b?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Patented Chronolux Power Signal Technology for anti-aging nightly skin renewal.",
                FullDescription = "Deep hydration reduces appearance of fine lines, pores, and uneven texture for radiant glowing skin.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Chanel Coco Mademoiselle Eau De Parfum 100ml", SKU = "BEAU-CHAN-026", CategoryId = 7, CategoryName = "Beauty", Brand = "Chanel",
                Price = 165.00m, OldPrice = 185.00m, DiscountPercentage = 10.8, Rating = 4.9, ReviewCount = 890, Stock = 50,
                MainImage = "https://images.unsplash.com/photo-1541643600914-78b084683601?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1592945403244-b3fbafd7f539?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "An amber woody floral fragrance with vibrant notes of orange, patchouli, and May rose.",
                FullDescription = "Embodying independent grace and audacity in a luxurious glass bottle sprayer.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Dyson Airwrap Multi-Styler Complete Long", SKU = "BEAU-DYSO-027", CategoryId = 7, CategoryName = "Beauty", Brand = "Dyson",
                Price = 599.99m, OldPrice = 649.99m, DiscountPercentage = 7.6, Rating = 4.8, ReviewCount = 610, Stock = 15,
                MainImage = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1527799820374-dcf8d9d4a388?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Curl, shape, smooth and hide flyaways using Coanda airflow technology without extreme heat.",
                FullDescription = "Includes re-engineered barrels that harness airflow in both directions for faster styling.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "La Mer Crème de la Mer Moisturizing Cream 60ml", SKU = "BEAU-LAME-028", CategoryId = 7, CategoryName = "Beauty", Brand = "La Mer",
                Price = 380.00m, OldPrice = 420.00m, DiscountPercentage = 9.5, Rating = 4.7, ReviewCount = 320, Stock = 25,
                MainImage = "https://images.unsplash.com/photo-1571781926291-c477ebfd024b?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Infused with cell-renewing Miracle Broth for soothing dry skin and restoring firmness.",
                FullDescription = "Ultra-rich formula transforms dryness so skin looks supple, smooth, and rejuvenated.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(3)
            });

            // --- Category 8: Home & Kitchen (4 Products) ---
            products.Add(new Product {
                Title = "Nespresso VertuoPlus Coffee & Espresso Machine", SKU = "HOME-NESP-029", CategoryId = 8, CategoryName = "Home & Kitchen", Brand = "Nespresso",
                Price = 159.00m, OldPrice = 199.00m, DiscountPercentage = 20.1, Rating = 4.8, ReviewCount = 1120, Stock = 65,
                MainImage = "https://images.unsplash.com/photo-1517668808822-9efe02eae258?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Centrifusion technology reads barcode parameters to brew perfect single-serve coffee and crema.",
                FullDescription = "Brews 5 cup sizes at the touch of a button with rapid 20-second heating and auto shut-off.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Ninja Foodi 6-in-1 8-qt. 2-Basket Air Fryer", SKU = "HOME-NINJ-030", CategoryId = 8, CategoryName = "Home & Kitchen", Brand = "Ninja",
                Price = 179.99m, OldPrice = 219.99m, DiscountPercentage = 18.1, Rating = 4.9, ReviewCount = 840, Stock = 45,
                MainImage = "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1585515320310-259814833e62?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "DualZone technology allows cooking 2 different foods 2 ways, finishing simultaneously.",
                FullDescription = "Air fry, air broil, roast, bake, reheat, and dehydrate with 75% less fat than traditional frying.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Le Creuset Enameled Cast Iron Dutch Oven 5.5 qt", SKU = "HOME-LECR-031", CategoryId = 8, CategoryName = "Home & Kitchen", Brand = "Le Creuset",
                Price = 419.95m, OldPrice = 460.00m, DiscountPercentage = 8.7, Rating = 4.9, ReviewCount = 490, Stock = 30,
                MainImage = "https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "French culinary masterpiece offering superior heat distribution for slow-cooking and searing.",
                FullDescription = "Durable porcelain enamel resists chipping and cracking. Oven safe up to 500°F.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "iRobot Roomba j7+ Self-Emptying Robot Vacuum", SKU = "HOME-IROB-032", CategoryId = 8, CategoryName = "Home & Kitchen", Brand = "iRobot",
                Price = 599.00m, OldPrice = 799.00m, DiscountPercentage = 25.0, Rating = 4.7, ReviewCount = 380, Stock = 25,
                MainImage = "https://images.unsplash.com/photo-1518640467707-6811f4a6ab73?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1556911220-e15b29be8c8f?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "PrecisionVision Navigation avoids pet waste & cords while emptying itself for up to 60 days.",
                FullDescription = "Learns your cleaning habits to offer personalized schedules and localized room cleaning via app.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(2)
            });

            // --- Category 9: Gaming (4 Products) ---
            products.Add(new Product {
                Title = "PlayStation 5 Console (Slim Digital Edition)", SKU = "GAM-SONY-033", CategoryId = 9, CategoryName = "Gaming", Brand = "Sony",
                Price = 449.99m, OldPrice = 499.99m, DiscountPercentage = 10.0, Rating = 4.9, ReviewCount = 1480, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1507457379470-08b800bebc67?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Harness ultra-high speed SSD loading, haptic feedback, adaptive triggers and 3D Audio.",
                FullDescription = "1TB SSD storage in a sleeker, compact form factor capable of 4K 120Hz gaming output.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Xbox Series X 1TB Console - Black", SKU = "GAM-MICR-034", CategoryId = 9, CategoryName = "Gaming", Brand = "Microsoft",
                Price = 469.99m, OldPrice = 499.99m, DiscountPercentage = 6.0, Rating = 4.8, ReviewCount = 920, Stock = 35,
                MainImage = "https://images.unsplash.com/photo-1621259182978-fbf93132d53d?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "12 teraflops of raw graphic processing power with Quick Resume across thousands of games.",
                FullDescription = "Experience true 4K gaming and high dynamic range with custom Zen 2 and RDNA 2 architectures.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Logitech G Pro X Superlight 2 Wireless Gaming Mouse", SKU = "GAM-LOGI-035", CategoryId = 9, CategoryName = "Gaming", Brand = "Logitech",
                Price = 159.99m, OldPrice = 179.99m, DiscountPercentage = 11.1, Rating = 4.9, ReviewCount = 670, Stock = 90,
                MainImage = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "60g ultra-lightweight esports icon with LIGHTFORCE hybrid switches and HERO 2 sensor.",
                FullDescription = "32,000 DPI precision tracking, 95 hours of battery life, and zero-additive PTFE feet.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Razer BlackWidow V4 Pro Mechanical Gaming Keyboard", SKU = "GAM-RAZE-036", CategoryId = 9, CategoryName = "Gaming", Brand = "Razer",
                Price = 229.99m, OldPrice = 249.99m, DiscountPercentage = 8.0, Rating = 4.7, ReviewCount = 280, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1595225476474-87563907a212?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Full-blown battlestation keyboard featuring Razer Command Dial and 8 dedicated macro keys.",
                FullDescription = "Per-key Razer Chroma RGB with 3-side underglow, magnetic plush wrist rest, and sound dampening foam.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(3)
            });

            // --- Category 10: Furniture (4 Products) ---
            products.Add(new Product {
                Title = "Herman Miller Aeron Ergonomic Office Chair", SKU = "FURN-HERM-037", CategoryId = 10, CategoryName = "Furniture", Brand = "Herman Miller",
                Price = 1295.00m, OldPrice = 1495.00m, DiscountPercentage = 13.3, Rating = 4.9, ReviewCount = 740, Stock = 15,
                MainImage = "https://images.unsplash.com/photo-1580481072645-022f9a6d83d0?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1505797149-43b0069ec26b?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Pellicle suspension mesh and PostureFit SL sacral support for optimal spinal alignment.",
                FullDescription = "The golden standard of ergonomic seating. Fully adjustable arms, tilt limiter, and durable aluminum frame.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "West Elm Harmony Leather Sectional Sofa", SKU = "FURN-WEST-038", CategoryId = 10, CategoryName = "Furniture", Brand = "West Elm",
                Price = 2499.00m, OldPrice = 2899.00m, DiscountPercentage = 13.7, Rating = 4.8, ReviewCount = 130, Stock = 8,
                MainImage = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Deep plush sofa upholstered in top-grain cognac saddle leather.",
                FullDescription = "Handcrafted wooden frame with down-blend cushions for deep, sink-in comfort during lounge sessions.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Article Seno Solid Oak Dining Table", SKU = "FURN-ARTI-039", CategoryId = 10, CategoryName = "Furniture", Brand = "Article",
                Price = 799.00m, OldPrice = 949.00m, DiscountPercentage = 15.8, Rating = 4.7, ReviewCount = 95, Stock = 14,
                MainImage = "https://images.unsplash.com/photo-1615066390971-03e4e1c36ddf?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1530018607912-eff2daa1bac4?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Scandinavian minimalist dining table constructed from 100% solid White American Oak.",
                FullDescription = "Comfortably seats up to 8 guests with clear protective lacquer topcoat for easy maintenance.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Mid-Century Modern Velvet Accent Armchair", SKU = "FURN-MOD-040", CategoryId = 10, CategoryName = "Furniture", Brand = "Modway",
                Price = 329.99m, OldPrice = 429.99m, DiscountPercentage = 23.2, Rating = 4.6, ReviewCount = 210, Stock = 30,
                MainImage = "https://images.unsplash.com/photo-1567538096630-e0c55bd6374c?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1580481072645-022f9a6d83d0?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Emerald green velvet armchair featuring splayed gold stainless steel legs.",
                FullDescription = "Dense foam padding and channel tufting add retro charm and superior cozy seating to any living space.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(5)
            });

            // --- Category 11: Grocery (4 Products) ---
            products.Add(new Product {
                Title = "Organic Italian Extra Virgin Olive Oil 1L", SKU = "GROC-OIL-041", CategoryId = 11, CategoryName = "Grocery", Brand = "California Olive Ranch",
                Price = 24.99m, OldPrice = 29.99m, DiscountPercentage = 16.6, Rating = 4.9, ReviewCount = 520, Stock = 150,
                MainImage = "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Cold-extracted 100% estate-grown olives with peppery finish and high antioxidant polyphenols.",
                FullDescription = "Non-GMO project verified, USDA Organic certified for gourmet salad dressings and light pan searing.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Manuka Health MGO 400+ Raw Manuka Honey 500g", SKU = "GROC-HON-042", CategoryId = 11, CategoryName = "Grocery", Brand = "Manuka Health",
                Price = 59.99m, OldPrice = 74.99m, DiscountPercentage = 20.0, Rating = 4.8, ReviewCount = 310, Stock = 80,
                MainImage = "https://images.unsplash.com/photo-1587049352847-4a222e784d38?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Superfood honey harvested directly from pristine remote New Zealand native bush.",
                FullDescription = "100% natural superfood with guaranteed minimum 400mg/kg methylglyoxal content for immune vitality.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Artisanal Whole Bean Espresso Coffee Roast 1kg", SKU = "GROC-COF-043", CategoryId = 11, CategoryName = "Grocery", Brand = "Stumptown",
                Price = 32.00m, OldPrice = 38.00m, DiscountPercentage = 15.7, Rating = 4.9, ReviewCount = 440, Stock = 110,
                MainImage = "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Direct trade 100% Arabica dark roast featuring notes of dark chocolate and dark cherry.",
                FullDescription = "Roasted in small batches to preserve natural essential oils for rich crema and velvet espresso body.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Gourmet Swiss Dark Chocolate Gift Box (24 Pcs)", SKU = "GROC-CHO-044", CategoryId = 11, CategoryName = "Grocery", Brand = "Lindt",
                Price = 28.50m, OldPrice = 35.00m, DiscountPercentage = 18.5, Rating = 4.8, ReviewCount = 280, Stock = 90,
                MainImage = "https://images.unsplash.com/photo-1549007994-cb92caebd54b?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "70% to 85% cocoa truffles infused with sea salt, orange peel, and roasted hazelnuts.",
                FullDescription = "Master Swiss chocolatier selection wrapped in luxury foil presentation box.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(2)
            });

            // --- Category 12: Sports (4 Products) ---
            products.Add(new Product {
                Title = "Peloton Bike+ Interactive Fitness Exercise Bike", SKU = "SPOR-PELO-045", CategoryId = 12, CategoryName = "Sports", Brand = "Peloton",
                Price = 2195.00m, OldPrice = 2495.00m, DiscountPercentage = 12.0, Rating = 4.9, ReviewCount = 810, Stock = 12,
                MainImage = "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "23.8\" rotating HD touchscreen with Auto-Follow resistance and studio sound speakers.",
                FullDescription = "Stream live studio cardio, strength, and yoga workouts with real-time metrics and instructor feedback.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Bowflex SelectTech 552 Adjustable Dumbbells Pair", SKU = "SPOR-BOWF-046", CategoryId = 12, CategoryName = "Sports", Brand = "Bowflex",
                Price = 429.00m, OldPrice = 549.00m, DiscountPercentage = 21.8, Rating = 4.8, ReviewCount = 1250, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Replaces 15 sets of weights in one compact system. Adjusts from 5 to 52.5 lbs per dumbbell.",
                FullDescription = "Unique dial system rapidly clicks to your target resistance weight for seamless home strength training.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Lululemon Align High-Rise Yoga Pant 25\"", SKU = "SPOR-LULU-047", CategoryId = 12, CategoryName = "Sports", Brand = "Lululemon",
                Price = 98.00m, OldPrice = 118.00m, DiscountPercentage = 16.9, Rating = 4.9, ReviewCount = 2100, Stock = 100,
                MainImage = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Weightless Nulu fabric so buttery soft it feels like a second skin.",
                FullDescription = "Sweat-wicking, four-way stretch leggings engineered to minimize distraction during yoga and fitness.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Garmin Forerunner 965 GPS Running Smartwatch", SKU = "SPOR-GARM-048", CategoryId = 12, CategoryName = "Sports", Brand = "Garmin",
                Price = 599.99m, OldPrice = 649.99m, DiscountPercentage = 7.6, Rating = 4.8, ReviewCount = 310, Stock = 25,
                MainImage = "https://images.unsplash.com/photo-1510017803434-a899398421b3?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Vibrant AMOLED display with titanium bezel, full-color mapping, and training readiness metrics.",
                FullDescription = "23 days of battery life in smartwatch mode with multi-band GNSS satIQ technology.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(3)
            });

            // --- Category 13: Books (4 Products) ---
            products.Add(new Product {
                Title = "Atomic Habits by James Clear (Hardcover)", SKU = "BOOK-ATOM-049", CategoryId = 13, CategoryName = "Books", Brand = "Penguin Random House",
                Price = 18.99m, OldPrice = 27.00m, DiscountPercentage = 29.6, Rating = 4.9, ReviewCount = 4800, Stock = 200,
                MainImage = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "An Easy & Proven Way to Build Good Habits & Break Bad Ones. Over 15 million copies sold.",
                FullDescription = "Tiny changes, remarkable results. James Clear reveals practical strategies to master habit formation.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Designing Data-Intensive Applications by Martin Kleppmann", SKU = "BOOK-DATA-050", CategoryId = 13, CategoryName = "Books", Brand = "O'Reilly Media",
                Price = 42.50m, OldPrice = 54.99m, DiscountPercentage = 22.7, Rating = 4.9, ReviewCount = 1350, Stock = 85,
                MainImage = "https://images.unsplash.com/photo-1532012164546-f43778862363?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "The definitive guide to the architecture of distributed storage systems and data processing.",
                FullDescription = "Deeply explores reliability, scalability, maintainability, transactions, and consensus protocols.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Clean Code: A Handbook of Agile Software Craftsmanship", SKU = "BOOK-CLEA-051", CategoryId = 13, CategoryName = "Books", Brand = "Pearson",
                Price = 39.99m, OldPrice = 49.99m, DiscountPercentage = 20.0, Rating = 4.8, ReviewCount = 920, Stock = 70,
                MainImage = "https://images.unsplash.com/photo-1512820790803-83ca734da794?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Robert C. Martin (Uncle Bob) reveals principles of writing maintainable, readable code.",
                FullDescription = "Packed with real-world refactoring case studies, smell tests, and object-oriented design patterns.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "The Psychology of Money by Morgan Housel", SKU = "BOOK-PSYC-052", CategoryId = 13, CategoryName = "Books", Brand = "Harriman House",
                Price = 16.99m, OldPrice = 22.00m, DiscountPercentage = 22.7, Rating = 4.8, ReviewCount = 2900, Stock = 140,
                MainImage = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Timeless lessons on wealth, greed, and happiness exploring how people think about money.",
                FullDescription = "19 short stories demonstrating how behavior beats intelligence when it comes to financial success.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(2)
            });

            // --- Category 14: Automotive (4 Products) ---
            products.Add(new Product {
                Title = "NOCO Boost HD GB70 2000A 12V Car Jump Starter", SKU = "AUTO-NOCO-053", CategoryId = 14, CategoryName = "Automotive", Brand = "NOCO",
                Price = 199.95m, OldPrice = 249.95m, DiscountPercentage = 20.0, Rating = 4.8, ReviewCount = 890, Stock = 50,
                MainImage = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1486006920555-c77dce18193b?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Safely jump start a dead battery in seconds up to 40 times on a single charge.",
                FullDescription = "UltraSafe spark-proof lithium power bank with built-in 400 lumen LED flashlight and USB charging.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Chemical Guys HOL148 16-Piece Car Wash Bucket Kit", SKU = "AUTO-CHEM-054", CategoryId = 14, CategoryName = "Automotive", Brand = "Chemical Guys",
                Price = 99.99m, OldPrice = 129.99m, DiscountPercentage = 23.0, Rating = 4.7, ReviewCount = 610, Stock = 65,
                MainImage = "https://images.unsplash.com/photo-1607860108855-64acf2078ed9?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Everything needed for pristine exterior paint foam washing, wheel cleaning, and interior detailing.",
                FullDescription = "Includes Citrus Wash & Gloss, Butter Wet Wax, Diablo Wheel Cleaner, foam cannon, and microfiber towels.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Rexing V1P 4K Dual Dash Cam Front and Rear", SKU = "AUTO-REXI-055", CategoryId = 14, CategoryName = "Automotive", Brand = "Rexing",
                Price = 149.99m, OldPrice = 189.99m, DiscountPercentage = 21.0, Rating = 4.6, ReviewCount = 420, Stock = 35,
                MainImage = "https://images.unsplash.com/photo-1508974239320-0a029497e820?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "4K Ultra HD front camera with 1080p rear recording and supercapacitor night vision.",
                FullDescription = "Wi-Fi mobile app connection, built-in GPS logger, G-sensor collision detection, and loop recording.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Armor All Complete Car Care Microfiber Gift Pack", SKU = "AUTO-ARMO-056", CategoryId = 14, CategoryName = "Automotive", Brand = "Armor All",
                Price = 34.99m, OldPrice = 45.00m, DiscountPercentage = 22.2, Rating = 4.6, ReviewCount = 780, Stock = 120,
                MainImage = "https://images.unsplash.com/photo-1520340356584-f9917d1eea6f?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Protectant wipes, glass cleaner, tire shine foam, and wash pads for showroom finish.",
                FullDescription = "UV blocking formula prevents dashboard cracking while leaving non-greasy natural shine.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(4)
            });

            // --- Category 15: Pet Supplies (4 Products) ---
            products.Add(new Product {
                Title = "Furbo 360° Dog Camera with Treat Tossing & Barking Alert", SKU = "PET-FURB-057", CategoryId = 15, CategoryName = "Pet Supplies", Brand = "Furbo",
                Price = 149.00m, OldPrice = 209.00m, DiscountPercentage = 28.7, Rating = 4.8, ReviewCount = 1140, Stock = 45,
                MainImage = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Full HD rotating pet camera with color night vision, 2-way audio, and remote treat dispenser.",
                FullDescription = "Smart AI alerts detect barking, pet movement, and emergency home alarms right to your smartphone.",
                IsFeatured = true, IsBestSeller = true
            });
            products.Add(new Product {
                Title = "Bespoke Orthopedic Memory Foam Dog Bed (Extra Large)", SKU = "PET-BED-058", CategoryId = 15, CategoryName = "Pet Supplies", Brand = "PetFusion",
                Price = 119.95m, OldPrice = 149.95m, DiscountPercentage = 20.0, Rating = 4.9, ReviewCount = 890, Stock = 40,
                MainImage = "https://images.unsplash.com/photo-1541599540903-216a46ca1dc0?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "4-inch solid memory foam base relieves joint pain and improves mobility for senior dogs.",
                FullDescription = "Tear-resistant cover with waterproof inner liner and non-skid bottom in luxury slate gray.",
                IsTrending = true
            });
            products.Add(new Product {
                Title = "Catit Flower Automatic Water Fountain for Cats", SKU = "PET-CATI-059", CategoryId = 15, CategoryName = "Pet Supplies", Brand = "Catit",
                Price = 29.99m, OldPrice = 39.99m, DiscountPercentage = 25.0, Rating = 4.7, ReviewCount = 1420, Stock = 90,
                MainImage = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Triple-action filtration fountain encouraging pets to drink fresh oxygenated water.",
                FullDescription = "Compact 3L reservoir with low-voltage whisper quiet energy efficient pump.",
                IsNewArrival = true
            });
            products.Add(new Product {
                Title = "Royal Canin Size Health Small Adult Dry Dog Food 14 lb", SKU = "PET-ROYA-060", CategoryId = 15, CategoryName = "Pet Supplies", Brand = "Royal Canin",
                Price = 46.99m, OldPrice = 54.99m, DiscountPercentage = 14.5, Rating = 4.8, ReviewCount = 670, Stock = 110,
                MainImage = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=800&q=80",
                AdditionalImages = new() { "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=800&q=80" },
                ShortDescription = "Formulated for small dogs 9-22 lbs to maintain ideal weight and digestive health.",
                FullDescription = "Enriched with L-carnitine and EPA/DHA fatty acids for healthy coat skin and coat shiny vitality.",
                IsFlashDeal = true, FlashDealEnd = DateTime.Now.AddDays(3)
            });

                foreach (var p in products)
                {
                    if (catDict.TryGetValue(p.CategoryName.ToLower(), out int cId))
                    {
                        p.CategoryId = cId;
                    }
                }

                context.Products.AddRange(products);
                context.SaveChanges();
            }

            // 3. SEED REVIEWS FOR PRODUCTS
            if (!context.Reviews.Any())
            {
                var sonyProduct = context.Products.FirstOrDefault(p => p.SKU == "ELEC-SONY-001");
                var iphoneProduct = context.Products.FirstOrDefault(p => p.SKU == "MOB-APPL-005");
                var macbookProduct = context.Products.FirstOrDefault(p => p.SKU == "LAP-APPL-009");
                var ultraboostProduct = context.Products.FirstOrDefault(p => p.SKU == "SHOE-ADID-017");
            var sampleReviews = new List<Review>
            {
                new Review { ProductId = 1, UserName = "Sarah Jenkins", Rating = 5.0, Title = "Absolute silence on flights!", Comment = "These headphones completely eliminated airplane engine noise on my 14-hour flight. Worth every single penny!", Date = DateTime.Now.AddDays(-12), HelpfulCount = 34 },
                new Review { ProductId = 1, UserName = "David Miller", Rating = 5.0, Title = "Best microphone quality for Zoom calls", Comment = "My coworkers noticed the difference immediately. Crisp mic clarity and super comfortable padding.", Date = DateTime.Now.AddDays(-5), HelpfulCount = 19 },
                new Review { ProductId = 5, UserName = "Marcus Vance", Rating = 5.0, Title = "Titanium is so light!", Comment = "The upgrade from iPhone 12 to 15 Pro Max is incredible. The camera zoom is mind-blowing.", Date = DateTime.Now.AddDays(-2), HelpfulCount = 42 },
                new Review { ProductId = 9, UserName = "Elena Rostova", Rating = 5.0, Title = "MacBook speed demon", Comment = "Renders 4K 60fps video projects in seconds. Battery easily lasts 2 full workdays without charging.", Date = DateTime.Now.AddDays(-20), HelpfulCount = 56 },
                new Review { ProductId = 17, UserName = "Jake Coleman", Rating = 4.8, Title = "Super comfortable sneakers", Comment = "Ran 10k in these fresh out of the box with zero blisters. Fits true to size.", Date = DateTime.Now.AddDays(-8), HelpfulCount = 14 }
            };
                if (sonyProduct != null)
                {
                    sampleReviews[0].ProductId = sonyProduct.Id;
                    sampleReviews[1].ProductId = sonyProduct.Id;
                }
                if (iphoneProduct != null) sampleReviews[2].ProductId = iphoneProduct.Id;
                if (macbookProduct != null) sampleReviews[3].ProductId = macbookProduct.Id;
                if (ultraboostProduct != null) sampleReviews[4].ProductId = ultraboostProduct.Id;

                context.Reviews.AddRange(sampleReviews);
                context.SaveChanges();
            }

            // 4. SEED Q&A
            if (!context.QuestionAnswers.Any())
            {
                var sonyProduct = context.Products.FirstOrDefault(p => p.SKU == "ELEC-SONY-001");
                var iphoneProduct = context.Products.FirstOrDefault(p => p.SKU == "MOB-APPL-005");
            var sampleQA = new List<QuestionAnswer>
            {
                new QuestionAnswer { ProductId = 1, Question = "Can this connect to two devices simultaneously?", Answer = "Yes! The Sony WH-1000XM5 features Multipoint connection so you can switch seamlessly between your laptop and phone.", AskedBy = "Tom K.", AnsweredBy = "Hamara Support" },
                new QuestionAnswer { ProductId = 5, Question = "Does it include a USB-C charging cable in the box?", Answer = "Yes, a high-durability braided USB-C charging cable is included in the box.", AskedBy = "Rachel G.", AnsweredBy = "Hamara Support" }
            };
                if (sonyProduct != null) sampleQA[0].ProductId = sonyProduct.Id;
                if (iphoneProduct != null) sampleQA[1].ProductId = iphoneProduct.Id;

                context.QuestionAnswers.AddRange(sampleQA);
                context.SaveChanges();
            }

            // 5. SEED COUPONS
            if (!context.Coupons.Any())
            {
                var coupons = new List<Coupon>
                {
                    new Coupon { Code = "SAVE10", Description = "10% off your entire order", DiscountPercentage = 10, MinimumSpend = 50, IsActive = true },
                    new Coupon { Code = "HAMARA20", Description = "20% special discount on orders over $100", DiscountPercentage = 20, MinimumSpend = 100, IsActive = true },
                    new Coupon { Code = "FREESHIP", Description = "Free Express Shipping on any order", DiscountPercentage = 0, FixedDiscountAmount = 0m, FreeShipping = true, MinimumSpend = 0, IsActive = true },
                    new Coupon { Code = "WELCOME50", Description = "$50 Flat discount on purchases above $300", FixedDiscountAmount = 50.00m, MinimumSpend = 300, IsActive = true }
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

            // 6. SEED ROLES AND USERS (ASP.NET Core Identity)
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

                // Seed Admin User (configured via appsettings / env var, with safe fallback)
                string adminEmail = config?["AdminSeed:Email"] ?? config?["AdminUser:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@hamaracommerce.pk";
                string adminPassword = config?["AdminSeed:Password"] ?? config?["AdminUser:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123!";
                string adminFullName = config?["AdminUser:FullName"] ?? "Hamara Administrator";

                var admin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = adminFullName,
                        PhoneNumber = "+92 300 0000000",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    userManager.CreateAsync(admin, adminPassword).GetAwaiter().GetResult();
                }
                else
                {
                    // In Development only: reset password to configured password (default: Admin@123!)
                    if (isDevelopment)
                    {
                        var resetToken = userManager.GeneratePasswordResetTokenAsync(admin).GetAwaiter().GetResult();
                        userManager.ResetPasswordAsync(admin, resetToken, adminPassword).GetAwaiter().GetResult();
                    }
                }

                // Ensure Admin properties, role, and clear lockout/access-failed counts
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

                    // Clear lockout and access failed count
                    userManager.SetLockoutEndDateAsync(admin, null).GetAwaiter().GetResult();
                    userManager.ResetAccessFailedCountAsync(admin).GetAwaiter().GetResult();

                    // Assign Admin role
                    if (!userManager.IsInRoleAsync(admin, "Admin").GetAwaiter().GetResult())
                    {
                        userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
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

            // 7. SEED DEMO ORDERS (For Order Tracking & Admin Analytics)
            if (!context.Orders.Any())
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
                        new OrderItem { ProductId = 1, ProductTitle = "Sony WH-1000XM5 Wireless Headphones", ProductImage = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80", SKU = "AUD-SONY-001", UnitPrice = 349.99m, Quantity = 1 }
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
                    PaymentMethod = "Sandbox Test Card",
                    PaymentStatus = PaymentStatus.Paid,
                    Status = OrderStatus.Delivered,
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(-1),
                    Subtotal = 1199.00m,
                    DiscountAmount = 0m,
                    TaxAmount = 95.92m,
                    ShippingFee = 0.00m,
                    TotalAmount = 1294.92m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 5, ProductTitle = "Apple iPhone 15 Pro Max 256GB Titanium", ProductImage = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=800&q=80", SKU = "MOB-APPL-005", UnitPrice = 1199.00m, Quantity = 1 }
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
                        new OrderItem { ProductId = 29, ProductTitle = "Nespresso VertuoPlus Coffee & Espresso Machine", ProductImage = "https://images.unsplash.com/photo-1517668808822-9efe02eae258?auto=format&fit=crop&w=800&q=80", SKU = "HOME-NESP-029", UnitPrice = 159.00m, Quantity = 1 }
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

            // 8. BACKFILL HISTORICAL COUPON REDEMPTIONS (Safe migration from legacy CustomerNotes markers)
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



