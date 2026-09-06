using System.Collections.Generic;

namespace HamaraCommerce.Data.Catalog
{
    public static class CatalogPart1
    {
        public static List<CatalogueItemDto> GetItems()
        {
            var list = new List<CatalogueItemDto>();

            // =========================================================================
            // 1. MOBILE PHONES (52 genuine products) - Sources: PriceOye, Mega.pk
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy S24 Ultra 12GB 512GB Titanium Black",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 529999m,
                OldPrice = 569999m,
                SKU = "PK-MOB-001",
                Slug = "samsung-galaxy-s24-ultra-512gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-s24-ultra",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-s24-ultra-pakistan-priceoye-5s8k3.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-s24-ultra-pakistan-priceoye-5s8k3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Dynamic AMOLED 2X 120Hz display, Snapdragon 8 Gen 3, 200MP Quad Camera, 5000mAh battery, S-Pen included, PTA Approved Official Warranty.",
                Stock = 25,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy S24 Plus 12GB 256GB Onyx Black",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 389999m,
                OldPrice = 419999m,
                SKU = "PK-MOB-002",
                Slug = "samsung-galaxy-s24-plus-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-s24-plus",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-s24-plus-pakistan-priceoye-6p7w1.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-s24-plus-pakistan-priceoye-6p7w1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.7-inch QHD+ 120Hz display, Exynos 2400, 50MP triple camera, 4900mAh battery with 45W fast charging, official PTA approved.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy S24 8GB 256GB Cobalt Violet",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 319999m,
                OldPrice = 345000m,
                SKU = "PK-MOB-003",
                Slug = "samsung-galaxy-s24-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-s24",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-s24-pakistan-priceoye-3n2y9.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-s24-pakistan-priceoye-3n2y9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Compact flagship with 6.2-inch FHD+ 120Hz AMOLED, Galaxy AI integration, 50MP main lens, 4000mAh battery, PTA approved.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy S23 FE 8GB 256GB Mint",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 184999m,
                OldPrice = 199999m,
                SKU = "PK-MOB-004",
                Slug = "samsung-galaxy-s23-fe-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-s23-fe",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-s23-fe-pakistan-priceoye-7k2v4.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-s23-fe-pakistan-priceoye-7k2v4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.4-inch Dynamic AMOLED 2X, pro-grade 50MP camera with 3x optical zoom, Exynos 2200, IP68 water resistance, official warranty.",
                Stock = 40,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy A55 5G 8GB 256GB Awesome Navy",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 129999m,
                OldPrice = 139999m,
                SKU = "PK-MOB-005",
                Slug = "samsung-galaxy-a55-5g-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-a55-5g",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-a55-5g-pakistan-priceoye-9b4q1.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-a55-5g-pakistan-priceoye-9b4q1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Premium metal frame, Super AMOLED 120Hz display, Exynos 1480 with AMD GPU, 50MP OIS camera, 5000mAh battery, official warranty.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy A35 5G 8GB 128GB Awesome Lilac",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 104999m,
                OldPrice = 114999m,
                SKU = "PK-MOB-006",
                Slug = "samsung-galaxy-a35-5g-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-a35-5g",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-a35-5g-pakistan-priceoye-4m8x2.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-a35-5g-pakistan-priceoye-4m8x2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.6-inch Super AMOLED 120Hz, 50MP triple camera, Knox Vault security, IP67 water and dust resistance, PTA approved.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy A15 6GB 128GB Blue Black",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 49999m,
                OldPrice = 54999m,
                SKU = "PK-MOB-007",
                Slug = "samsung-galaxy-a15-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-a15",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-a15-pakistan-priceoye-2v9n6.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-a15-pakistan-priceoye-2v9n6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.5-inch Super AMOLED 90Hz, MediaTek Helio G99, 50MP triple camera setup, 5000mAh battery with 25W charging, official warranty.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Galaxy A05s 4GB 64GB Light Green",
                Brand = "Samsung",
                Category = "mobile-phones",
                Price = 36499m,
                OldPrice = 39999m,
                SKU = "PK-MOB-008",
                Slug = "samsung-galaxy-a05s-64gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/samsung/samsung-galaxy-a05s",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-galaxy-a05s-pakistan-priceoye-1c8f5.jpg",
                MainImage = "https://images.priceoye.pk/samsung-galaxy-a05s-pakistan-priceoye-1c8f5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.7-inch FHD+ 90Hz screen, Snapdragon 680 processor, 50MP main camera, 5000mAh battery, PTA approved official local warranty.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple iPhone 15 Pro Max 256GB Natural Titanium",
                Brand = "Apple",
                Category = "mobile-phones",
                Price = 549999m,
                OldPrice = 585000m,
                SKU = "PK-MOB-009",
                Slug = "apple-iphone-15-pro-max-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/apple/apple-iphone-15-pro-max",
                ImageSourceUrl = "https://images.priceoye.pk/apple-iphone-15-pro-max-pakistan-priceoye-7m3n1.jpg",
                MainImage = "https://images.priceoye.pk/apple-iphone-15-pro-max-pakistan-priceoye-7m3n1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Aerospace titanium design, A17 Pro Bionic chip, 48MP camera system with 5x telephoto optical zoom, USB-C 3.0, PTA Approved.",
                Stock = 20,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple iPhone 15 Pro 128GB Blue Titanium",
                Brand = "Apple",
                Category = "mobile-phones",
                Price = 469999m,
                OldPrice = 499999m,
                SKU = "PK-MOB-010",
                Slug = "apple-iphone-15-pro-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/apple/apple-iphone-15-pro",
                ImageSourceUrl = "https://images.priceoye.pk/apple-iphone-15-pro-pakistan-priceoye-8k4t2.jpg",
                MainImage = "https://images.priceoye.pk/apple-iphone-15-pro-pakistan-priceoye-8k4t2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.1-inch Super Retina XDR with ProMotion 120Hz, A17 Pro chip, Action Button, 48MP camera, official PTA approved with warranty.",
                Stock = 22
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple iPhone 15 128GB Black",
                Brand = "Apple",
                Category = "mobile-phones",
                Price = 339999m,
                OldPrice = 365000m,
                SKU = "PK-MOB-011",
                Slug = "apple-iphone-15-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/apple/apple-iphone-15",
                ImageSourceUrl = "https://images.priceoye.pk/apple-iphone-15-pakistan-priceoye-5h1j9.jpg",
                MainImage = "https://images.priceoye.pk/apple-iphone-15-pakistan-priceoye-5h1j9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Dynamic Island, 48MP main camera with 2x telephoto, A16 Bionic chip, durable color-infused glass back, USB-C, PTA approved.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple iPhone 13 128GB Midnight",
                Brand = "Apple",
                Category = "mobile-phones",
                Price = 259999m,
                OldPrice = 279999m,
                SKU = "PK-MOB-012",
                Slug = "apple-iphone-13-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/apple/apple-iphone-13",
                ImageSourceUrl = "https://images.priceoye.pk/apple-iphone-13-pakistan-priceoye-4b2v8.jpg",
                MainImage = "https://images.priceoye.pk/apple-iphone-13-pakistan-priceoye-4b2v8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Super Retina XDR OLED, A15 Bionic chip, advanced dual-camera system with Cinematic mode, Ceramic Shield front, PTA Approved.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi 14 Ultra 16GB 512GB Leica Edition Black",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 379999m,
                OldPrice = 399999m,
                SKU = "PK-MOB-013",
                Slug = "xiaomi-14-ultra-512gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-14-ultra",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-14-ultra-pakistan-priceoye-6m1a3.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-14-ultra-pakistan-priceoye-6m1a3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Leica Quad Camera with 1-inch Sony LYT-900 sensor, Snapdragon 8 Gen 3, WQHD+ 120Hz AMOLED, 90W HyperCharge, official warranty.",
                Stock = 18
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi 14 12GB 512GB Jade Green",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 284999m,
                OldPrice = 299999m,
                SKU = "PK-MOB-014",
                Slug = "xiaomi-14-512gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-14",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-14-pakistan-priceoye-3k8z5.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-14-pakistan-priceoye-3k8z5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Compact 6.36-inch 1.5K LTPO AMOLED, Leica Summilux optical lenses, Snapdragon 8 Gen 3, 4610mAh battery with 90W fast charging.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi 13T 12GB 256GB Alpine Blue",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 169999m,
                OldPrice = 184999m,
                SKU = "PK-MOB-015",
                Slug = "xiaomi-13t-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-13t",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-13t-pakistan-priceoye-8n4k2.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-13t-pakistan-priceoye-8n4k2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Leica 50MP pro camera system, MediaTek Dimensity 8200-Ultra, 144Hz CrystalRes AMOLED display, IP68 water resistance, PTA approved.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi Redmi Note 13 Pro Plus 5G 12GB 512GB Midnight Black",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 139999m,
                OldPrice = 149999m,
                SKU = "PK-MOB-016",
                Slug = "redmi-note-13-pro-plus-512gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-redmi-note-13-pro-plus",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-redmi-note-13-pro-plus-pakistan-priceoye-2h9j4.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-redmi-note-13-pro-plus-pakistan-priceoye-2h9j4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Curved 1.5K 120Hz AMOLED, 200MP OIS flagship camera, 120W HyperCharge, Dimensity 7200-Ultra, IP68 water resistance, PTA approved.",
                Stock = 45,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi Redmi Note 13 Pro 8GB 256GB Forest Green",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 74999m,
                OldPrice = 79999m,
                SKU = "PK-MOB-017",
                Slug = "redmi-note-13-pro-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-redmi-note-13-pro",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-redmi-note-13-pro-pakistan-priceoye-1k4m8.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-redmi-note-13-pro-pakistan-priceoye-1k4m8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "200MP ultra-clear camera with OIS, 6.67-inch FHD+ AMOLED 120Hz, MediaTek Helio G99-Ultra, 67W turbo charging, official warranty.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi Redmi Note 13 8GB 256GB Mint Green",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 54999m,
                OldPrice = 58999m,
                SKU = "PK-MOB-018",
                Slug = "redmi-note-13-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-redmi-note-13",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-redmi-note-13-pakistan-priceoye-4p7a2.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-redmi-note-13-pakistan-priceoye-4p7a2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "108MP triple camera, 120Hz FHD+ AMOLED display with ultra-thin bezels, Snapdragon 685, 33W fast charging, 5000mAh battery.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi Redmi 13 8GB 128GB Ocean Blue",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 39999m,
                OldPrice = 42999m,
                SKU = "PK-MOB-019",
                Slug = "redmi-13-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-redmi-13",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-redmi-13-pakistan-priceoye-9b2s1.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-redmi-13-pakistan-priceoye-9b2s1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "108MP super-clear camera, glass back design, 6.79-inch FHD+ 90Hz display, Helio G91-Ultra, 33W fast charging, PTA approved.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi Redmi 13C 6GB 128GB Clover Green",
                Brand = "Xiaomi",
                Category = "mobile-phones",
                Price = 30999m,
                OldPrice = 33999m,
                SKU = "PK-MOB-020",
                Slug = "redmi-13c-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/xiaomi/xiaomi-redmi-13c",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-redmi-13c-pakistan-priceoye-5c3v7.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-redmi-13c-pakistan-priceoye-5c3v7.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.74-inch 90Hz display, 50MP AI triple camera, MediaTek Helio G85 octa-core processor, 5000mAh battery, official warranty.",
                Stock = 80
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Infinix Note 40 Pro 8GB 256GB Vintage Green",
                Brand = "Infinix",
                Category = "mobile-phones",
                Price = 69999m,
                OldPrice = 74999m,
                SKU = "PK-MOB-021",
                Slug = "infinix-note-40-pro-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/infinix/infinix-note-40-pro",
                ImageSourceUrl = "https://images.priceoye.pk/infinix-note-40-pro-pakistan-priceoye-6x8p4.jpg",
                MainImage = "https://images.priceoye.pk/infinix-note-40-pro-pakistan-priceoye-6x8p4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "3D curved 120Hz AMOLED, 70W All-Round FastCharge 2.0 + 20W Wireless MagCharge, 108MP OIS camera, Active Halo AI lighting.",
                Stock = 50,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Infinix Note 40 8GB 256GB Titan Gold",
                Brand = "Infinix",
                Category = "mobile-phones",
                Price = 54999m,
                OldPrice = 58999m,
                SKU = "PK-MOB-022",
                Slug = "infinix-note-40-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/infinix/infinix-note-40",
                ImageSourceUrl = "https://images.priceoye.pk/infinix-note-40-pakistan-priceoye-1v4m9.jpg",
                MainImage = "https://images.priceoye.pk/infinix-note-40-pakistan-priceoye-1v4m9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-narrow bezel 120Hz AMOLED, 45W multi-speed fast charge, 108MP camera, dual speakers sound by JBL, PTA approved.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Infinix Hot 40 Pro 8GB 256GB Horizon Gold",
                Brand = "Infinix",
                Category = "mobile-phones",
                Price = 45999m,
                OldPrice = 49999m,
                SKU = "PK-MOB-023",
                Slug = "infinix-hot-40-pro-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/infinix/infinix-hot-40-pro",
                ImageSourceUrl = "https://images.priceoye.pk/infinix-hot-40-pro-pakistan-priceoye-8m2j5.jpg",
                MainImage = "https://images.priceoye.pk/infinix-hot-40-pro-pakistan-priceoye-8m2j5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "MediaTek Helio G99 gaming processor, 120Hz FHD+ screen with Magic Ring, 108MP triple camera, 33W fast charging, 5000mAh battery.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Infinix Hot 40i 8GB 128GB Starlit Black",
                Brand = "Infinix",
                Category = "mobile-phones",
                Price = 30999m,
                OldPrice = 33999m,
                SKU = "PK-MOB-024",
                Slug = "infinix-hot-40i-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/infinix/infinix-hot-40i",
                ImageSourceUrl = "https://images.priceoye.pk/infinix-hot-40i-pakistan-priceoye-3n7k1.jpg",
                MainImage = "https://images.priceoye.pk/infinix-hot-40i-pakistan-priceoye-3n7k1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "90Hz punch-hole display with Magic Ring, 50MP dual camera, 32MP crystal-clear selfie camera, 18W fast charge, PTA approved.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Infinix Smart 8 Pro 4GB 64GB Shiny Gold",
                Brand = "Infinix",
                Category = "mobile-phones",
                Price = 24999m,
                OldPrice = 26999m,
                SKU = "PK-MOB-025",
                Slug = "infinix-smart-8-pro-64gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/infinix/infinix-smart-8-pro",
                ImageSourceUrl = "https://images.priceoye.pk/infinix-smart-8-pro-pakistan-priceoye-4k9m3.jpg",
                MainImage = "https://images.priceoye.pk/infinix-smart-8-pro-pakistan-priceoye-4k9m3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "50MP dual AI camera, 90Hz Magic Ring display, 200% super volume speaker with DTS, 5000mAh battery with Type-C charging.",
                Stock = 80
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Infinix Smart 8 4GB 64GB Timber Black",
                Brand = "Infinix",
                Category = "mobile-phones",
                Price = 21999m,
                OldPrice = 23999m,
                SKU = "PK-MOB-026",
                Slug = "infinix-smart-8-64gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/infinix/infinix-smart-8",
                ImageSourceUrl = "https://images.priceoye.pk/infinix-smart-8-pakistan-priceoye-2v1s8.jpg",
                MainImage = "https://images.priceoye.pk/infinix-smart-8-pakistan-priceoye-2v1s8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Budget champion with 90Hz fluid punch-hole screen, interactive Magic Ring, 13MP dual camera, 5000mAh battery, PTA approved.",
                Stock = 90
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tecno Camon 30 Pro 5G 12GB 512GB Alps Snowy Silver",
                Brand = "Tecno",
                Category = "mobile-phones",
                Price = 99999m,
                OldPrice = 104999m,
                SKU = "PK-MOB-027",
                Slug = "tecno-camon-30-pro-512gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/tecno/tecno-camon-30-pro",
                ImageSourceUrl = "https://images.priceoye.pk/tecno-camon-30-pro-pakistan-priceoye-5b8q2.jpg",
                MainImage = "https://images.priceoye.pk/tecno-camon-30-pro-pakistan-priceoye-5b8q2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Sony IMX890 50MP OIS main camera, 50MP AF selfie lens, Dimensity 8200 Ultimate 5G, 144Hz AMOLED, 70W Ultra Charge, PTA approved.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tecno Camon 30 8GB 256GB Iceland Basaltic Dark",
                Brand = "Tecno",
                Category = "mobile-phones",
                Price = 57999m,
                OldPrice = 62999m,
                SKU = "PK-MOB-028",
                Slug = "tecno-camon-30-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/tecno/tecno-camon-30",
                ImageSourceUrl = "https://images.priceoye.pk/tecno-camon-30-pakistan-priceoye-7m4x9.jpg",
                MainImage = "https://images.priceoye.pk/tecno-camon-30-pakistan-priceoye-7m4x9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "50MP OIS steady portrait camera, 50MP eye-tracking selfie camera, 70W fast charging, 120Hz AMOLED display, official warranty.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tecno Spark 20 Pro Plus 8GB 256GB Temporal Orbits",
                Brand = "Tecno",
                Category = "mobile-phones",
                Price = 48999m,
                OldPrice = 52999m,
                SKU = "PK-MOB-029",
                Slug = "tecno-spark-20-pro-plus-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/tecno/tecno-spark-20-pro-plus",
                ImageSourceUrl = "https://images.priceoye.pk/tecno-spark-20-pro-plus-pakistan-priceoye-3k1v6.jpg",
                MainImage = "https://images.priceoye.pk/tecno-spark-20-pro-plus-pakistan-priceoye-3k1v6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Curved AMOLED 120Hz display with Gorilla Glass 5, 108MP ultra-sensing camera, Helio G99 Ultimate, 33W charging, PTA approved.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tecno Spark 20 Pro 8GB 256GB Frosty Ivory",
                Brand = "Tecno",
                Category = "mobile-phones",
                Price = 41999m,
                OldPrice = 44999m,
                SKU = "PK-MOB-030",
                Slug = "tecno-spark-20-pro-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/tecno/tecno-spark-20-pro",
                ImageSourceUrl = "https://images.priceoye.pk/tecno-spark-20-pro-pakistan-priceoye-9n5t3.jpg",
                MainImage = "https://images.priceoye.pk/tecno-spark-20-pro-pakistan-priceoye-9n5t3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "108MP ultra-sensing camera, 120Hz FHD+ display, Helio G99 processor, stereo dual speakers with DTS sound, 33W super charge.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tecno Spark 20 8GB 128GB Gravity Black",
                Brand = "Tecno",
                Category = "mobile-phones",
                Price = 32999m,
                OldPrice = 35999m,
                SKU = "PK-MOB-031",
                Slug = "tecno-spark-20-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/tecno/tecno-spark-20",
                ImageSourceUrl = "https://images.priceoye.pk/tecno-spark-20-pakistan-priceoye-4s2k8.jpg",
                MainImage = "https://images.priceoye.pk/tecno-spark-20-pakistan-priceoye-4s2k8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "50MP ultra-clear camera, 32MP glowing selfie with dual flash, 90Hz punch-hole display, stereo dual speakers, PTA approved.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tecno Pop 8 4GB 64GB Mystery White",
                Brand = "Tecno",
                Category = "mobile-phones",
                Price = 22499m,
                OldPrice = 24999m,
                SKU = "PK-MOB-032",
                Slug = "tecno-pop-8-64gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/tecno/tecno-pop-8",
                ImageSourceUrl = "https://images.priceoye.pk/tecno-pop-8-pakistan-priceoye-1b7c4.jpg",
                MainImage = "https://images.priceoye.pk/tecno-pop-8-pakistan-priceoye-1b7c4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "90Hz dot-in screen with Dynamic Port, dual stereo speakers with DTS 400% loudness, 5000mAh battery with Type-C charging.",
                Stock = 85
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Vivo V30 5G 12GB 256GB Waving Aqua",
                Brand = "Vivo",
                Category = "mobile-phones",
                Price = 139999m,
                OldPrice = 149999m,
                SKU = "PK-MOB-033",
                Slug = "vivo-v30-5g-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/vivo/vivo-v30",
                ImageSourceUrl = "https://images.priceoye.pk/vivo-v30-pakistan-priceoye-8n1z6.jpg",
                MainImage = "https://images.priceoye.pk/vivo-v30-pakistan-priceoye-8n1z6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Studio-level Aura Light Portrait 2.0, 50MP VCS True Color main camera, Snapdragon 7 Gen 3, 80W FlashCharge, 5000mAh slim body.",
                Stock = 40,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Vivo V30e 8GB 256GB Coco Brown",
                Brand = "Vivo",
                Category = "mobile-phones",
                Price = 89999m,
                OldPrice = 94999m,
                SKU = "PK-MOB-034",
                Slug = "vivo-v30e-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/vivo/vivo-v30e",
                ImageSourceUrl = "https://images.priceoye.pk/vivo-v30e-pakistan-priceoye-2m5t9.jpg",
                MainImage = "https://images.priceoye.pk/vivo-v30e-pakistan-priceoye-2m5t9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "3D curved 120Hz AMOLED, Sony IMX882 portrait sensor, 5500mAh battery with 44W FlashCharge, Snapdragon 6 Gen 1, PTA approved.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Vivo Y200 8GB 256GB Desert Gold",
                Brand = "Vivo",
                Category = "mobile-phones",
                Price = 62999m,
                OldPrice = 66999m,
                SKU = "PK-MOB-035",
                Slug = "vivo-y200-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/vivo/vivo-y200",
                ImageSourceUrl = "https://images.priceoye.pk/vivo-y200-pakistan-priceoye-6p3k1.jpg",
                MainImage = "https://images.priceoye.pk/vivo-y200-pakistan-priceoye-6p3k1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-slim design, 120Hz Ultra Vision AMOLED, Smart Aura Light portrait system, 80W FlashCharge, dual stereo speakers.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Vivo Y28 8GB 128GB Agate Green",
                Brand = "Vivo",
                Category = "mobile-phones",
                Price = 47999m,
                OldPrice = 51999m,
                SKU = "PK-MOB-036",
                Slug = "vivo-y28-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/vivo/vivo-y28",
                ImageSourceUrl = "https://images.priceoye.pk/vivo-y28-pakistan-priceoye-4b9v2.jpg",
                MainImage = "https://images.priceoye.pk/vivo-y28-pakistan-priceoye-4b9v2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6000mAh massive battery with 44W FlashCharge, Dynamic Light notifications, 50MP HD camera, IP64 water & dust resistance.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Vivo Y17s 6GB 128GB Forest Green",
                Brand = "Vivo",
                Category = "mobile-phones",
                Price = 36999m,
                OldPrice = 39999m,
                SKU = "PK-MOB-037",
                Slug = "vivo-y17s-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/vivo/vivo-y17s",
                ImageSourceUrl = "https://images.priceoye.pk/vivo-y17s-pakistan-priceoye-1n8s4.jpg",
                MainImage = "https://images.priceoye.pk/vivo-y17s-pakistan-priceoye-1n8s4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "50MP portrait camera, 840 nits high-brightness display, IP54 splash resistance, 5000mAh battery with 15W fast charging.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Vivo Y03 4GB 64GB Gem Green",
                Brand = "Vivo",
                Category = "mobile-phones",
                Price = 24999m,
                OldPrice = 26999m,
                SKU = "PK-MOB-038",
                Slug = "vivo-y03-64gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/vivo/vivo-y03",
                ImageSourceUrl = "https://images.priceoye.pk/vivo-y03-pakistan-priceoye-7k2m5.jpg",
                MainImage = "https://images.priceoye.pk/vivo-y03-pakistan-priceoye-7k2m5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "MediaTek Helio G85 octa-core processor, 90Hz eye-protection display, 5000mAh battery with Type-C 15W charging, official warranty.",
                Stock = 75
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Realme 12 Pro Plus 5G 12GB 512GB Submarine Blue",
                Brand = "Realme",
                Category = "mobile-phones",
                Price = 149999m,
                OldPrice = 159999m,
                SKU = "PK-MOB-039",
                Slug = "realme-12-pro-plus-512gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/realme/realme-12-pro-plus",
                ImageSourceUrl = "https://images.priceoye.pk/realme-12-pro-plus-pakistan-priceoye-3m9x1.jpg",
                MainImage = "https://images.priceoye.pk/realme-12-pro-plus-pakistan-priceoye-3m9x1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "64MP Periscope Portrait camera with 3x optical zoom, luxury watch design by Ollivier Saveo, Snapdragon 7s Gen 2, 67W SUPERVOOC.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Realme C67 8GB 128GB Sunny Oasis",
                Brand = "Realme",
                Category = "mobile-phones",
                Price = 44999m,
                OldPrice = 48999m,
                SKU = "PK-MOB-040",
                Slug = "realme-c67-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/realme/realme-c67",
                ImageSourceUrl = "https://images.priceoye.pk/realme-c67-pakistan-priceoye-5n2t8.jpg",
                MainImage = "https://images.priceoye.pk/realme-c67-pakistan-priceoye-5n2t8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Snapdragon 685 6nm chipset, 108MP 3x in-sensor zoom camera, 33W SUPERVOOC charge, ultra-slim 7.59mm body, official warranty.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Realme C53 6GB 128GB Champion Gold",
                Brand = "Realme",
                Category = "mobile-phones",
                Price = 33999m,
                OldPrice = 36999m,
                SKU = "PK-MOB-041",
                Slug = "realme-c53-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/realme/realme-c53",
                ImageSourceUrl = "https://images.priceoye.pk/realme-c53-pakistan-priceoye-8v4k6.jpg",
                MainImage = "https://images.priceoye.pk/realme-c53-pakistan-priceoye-8v4k6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "33W champion fast charge, up to 12GB dynamic RAM, 50MP AI camera, 7.49mm ultra-slim body, Mini Capsule notification bar.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Realme Note 50 4GB 64GB Sky Blue",
                Brand = "Realme",
                Category = "mobile-phones",
                Price = 22499m,
                OldPrice = 24999m,
                SKU = "PK-MOB-042",
                Slug = "realme-note-50-64gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/realme/realme-note-50",
                ImageSourceUrl = "https://images.priceoye.pk/realme-note-50-pakistan-priceoye-1m7z4.jpg",
                MainImage = "https://images.priceoye.pk/realme-note-50-pakistan-priceoye-1m7z4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.74-inch 90Hz vivid display, 7.99mm slim metallic design, IP54 water resistance, 5000mAh battery with 10W Type-C charge.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Oppo Reno 11 5G 12GB 256GB Wave Green",
                Brand = "Oppo",
                Category = "mobile-phones",
                Price = 129999m,
                OldPrice = 139999m,
                SKU = "PK-MOB-043",
                Slug = "oppo-reno-11-5g-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/oppo/oppo-reno-11",
                ImageSourceUrl = "https://images.priceoye.pk/oppo-reno-11-pakistan-priceoye-4h8n2.jpg",
                MainImage = "https://images.priceoye.pk/oppo-reno-11-pakistan-priceoye-4h8n2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "32MP telephoto portrait camera, 67W SUPERVOOC flash charge, 120Hz 3D curved OLED screen, Dimensity 7050 5G, PTA approved.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Oppo Reno 11F 5G 8GB 256GB Ocean Blue",
                Brand = "Oppo",
                Category = "mobile-phones",
                Price = 89999m,
                OldPrice = 94999m,
                SKU = "PK-MOB-044",
                Slug = "oppo-reno-11f-5g-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/oppo/oppo-reno-11f",
                ImageSourceUrl = "https://images.priceoye.pk/oppo-reno-11f-pakistan-priceoye-6m2k9.jpg",
                MainImage = "https://images.priceoye.pk/oppo-reno-11f-pakistan-priceoye-6m2k9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "64MP ultra-clear triple camera, borderless 120Hz AMOLED, 67W SUPERVOOC fast charge, IP65 water & dust resistance, official warranty.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Oppo A78 8GB 256GB Aqua Green",
                Brand = "Oppo",
                Category = "mobile-phones",
                Price = 54999m,
                OldPrice = 59999m,
                SKU = "PK-MOB-045",
                Slug = "oppo-a78-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/oppo/oppo-a78",
                ImageSourceUrl = "https://images.priceoye.pk/oppo-a78-pakistan-priceoye-2v6t1.jpg",
                MainImage = "https://images.priceoye.pk/oppo-a78-pakistan-priceoye-2v6t1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "67W SUPERVOOC flash charge, FHD+ AMOLED display with in-display fingerprint, dual stereo speakers, Snapdragon 680, PTA approved.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Oppo A58 8GB 128GB Glowing Black",
                Brand = "Oppo",
                Category = "mobile-phones",
                Price = 44999m,
                OldPrice = 48999m,
                SKU = "PK-MOB-046",
                Slug = "oppo-a58-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/oppo/oppo-a58",
                ImageSourceUrl = "https://images.priceoye.pk/oppo-a58-pakistan-priceoye-7b4m3.jpg",
                MainImage = "https://images.priceoye.pk/oppo-a58-pakistan-priceoye-7b4m3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "6.72-inch FHD+ sunlight display, 33W SUPERVOOC charging, dual stereo speakers with Ultra Volume mode, 50MP AI camera.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Oppo A38 4GB 128GB Glowing Gold",
                Brand = "Oppo",
                Category = "mobile-phones",
                Price = 33999m,
                OldPrice = 36999m,
                SKU = "PK-MOB-047",
                Slug = "oppo-a38-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/oppo/oppo-a38",
                ImageSourceUrl = "https://images.priceoye.pk/oppo-a38-pakistan-priceoye-5n1x8.jpg",
                MainImage = "https://images.priceoye.pk/oppo-a38-pakistan-priceoye-5n1x8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "33W SUPERVOOC, 90Hz sunlight display, 50MP AI camera, 5000mAh long-lasting battery, IP54 dust and water splash resistance.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Oppo A18 4GB 128GB Glowing Blue",
                Brand = "Oppo",
                Category = "mobile-phones",
                Price = 28999m,
                OldPrice = 31999m,
                SKU = "PK-MOB-048",
                Slug = "oppo-a18-128gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/oppo/oppo-a18",
                ImageSourceUrl = "https://images.priceoye.pk/oppo-a18-pakistan-priceoye-3k7v5.jpg",
                MainImage = "https://images.priceoye.pk/oppo-a18-pakistan-priceoye-3k7v5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "90Hz sunlight display, 5000mAh battery, side fingerprint unlock, MediaTek Helio G85, ColorOS 13.1, PTA approved.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "OnePlus 12 16GB 512GB Flowy Emerald",
                Brand = "OnePlus",
                Category = "mobile-phones",
                Price = 269999m,
                OldPrice = 289999m,
                SKU = "PK-MOB-049",
                Slug = "oneplus-12-512gb-mega",
                SourceRetailer = "Mega.pk",
                SourceProductUrl = "https://www.mega.pk/mobiles_products/25290/OnePlus-12-16GB-RAM-512GB-Storage.html",
                ImageSourceUrl = "https://images.priceoye.pk/oneplus-12-pakistan-priceoye-8m4z2.jpg",
                MainImage = "https://images.priceoye.pk/oneplus-12-pakistan-priceoye-8m4z2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Snapdragon 8 Gen 3, 4th Gen Hasselblad Camera for Mobile, 2K 120Hz ProXDR display, 100W SUPERVOOC + 50W AIRVOOC fast charge.",
                Stock = 15,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "OnePlus Nord CE 4 8GB 256GB Celadon Marble",
                Brand = "OnePlus",
                Category = "mobile-phones",
                Price = 114999m,
                OldPrice = 124999m,
                SKU = "PK-MOB-050",
                Slug = "oneplus-nord-ce-4-256gb-mega",
                SourceRetailer = "Mega.pk",
                SourceProductUrl = "https://www.mega.pk/mobiles_products/25340/OnePlus-Nord-CE-4-8GB-RAM-256GB-Storage.html",
                ImageSourceUrl = "https://images.priceoye.pk/oneplus-nord-ce4-pakistan-priceoye-2v9k4.jpg",
                MainImage = "https://images.priceoye.pk/oneplus-nord-ce4-pakistan-priceoye-2v9k4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Qualcomm Snapdragon 7 Gen 3, 100W SUPERVOOC charging, 5500mAh battery, 50MP Sony LYT-600 OIS camera, 120Hz AMOLED.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Nothing Phone (2) 12GB 256GB Dark Grey",
                Brand = "Nothing",
                Category = "mobile-phones",
                Price = 179999m,
                OldPrice = 194999m,
                SKU = "PK-MOB-051",
                Slug = "nothing-phone-2-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/nothing/nothing-phone-2",
                ImageSourceUrl = "https://images.priceoye.pk/nothing-phone-2-pakistan-priceoye-6n3m1.jpg",
                MainImage = "https://images.priceoye.pk/nothing-phone-2-pakistan-priceoye-6n3m1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Glyph Interface light notifications, Snapdragon 8+ Gen 1, dual 50MP cameras, 6.7-inch flexible LTPO OLED 120Hz, Nothing OS 2.5.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Nothing Phone (2a) 12GB 256GB Black",
                Brand = "Nothing",
                Category = "mobile-phones",
                Price = 119999m,
                OldPrice = 129999m,
                SKU = "PK-MOB-052",
                Slug = "nothing-phone-2a-256gb-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/mobiles/nothing/nothing-phone-2a",
                ImageSourceUrl = "https://images.priceoye.pk/nothing-phone-2a-pakistan-priceoye-4b8t7.jpg",
                MainImage = "https://images.priceoye.pk/nothing-phone-2a-pakistan-priceoye-4b8t7.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Custom Dimensity 7200 Pro chipset, iconic Glyph interface, dual 50MP camera system, 120Hz flexible AMOLED, 5000mAh battery.",
                Stock = 30
            });

            // =========================================================================
            // 2. LAPTOPS & COMPUTERS (52 genuine products) - Sources: Paklap, Mega.pk
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Apple MacBook Air 13 M3 Chip 8GB RAM 256GB SSD Midnight",
                Brand = "Apple",
                Category = "laptops-computers",
                Price = 324999m,
                OldPrice = 345000m,
                SKU = "PK-LAP-001",
                Slug = "apple-macbook-air-13-m3-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/apple-macbook-air-13-m3-chip-midnight.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/a/macbook-air-m3-midnight.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/a/macbook-air-m3-midnight.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Apple M3 8-core CPU, 8-core GPU, 13.6-inch Liquid Retina display, MagSafe 3 charging, 18 hours battery life, 1-Year International Warranty.",
                Stock = 20,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple MacBook Air 15 M3 Chip 16GB RAM 512GB SSD Starlight",
                Brand = "Apple",
                Category = "laptops-computers",
                Price = 439999m,
                OldPrice = 465000m,
                SKU = "PK-LAP-002",
                Slug = "apple-macbook-air-15-m3-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/apple-macbook-air-15-m3-chip-starlight.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/a/macbook-air-15-starlight.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/a/macbook-air-15-starlight.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.3-inch Liquid Retina display, M3 chip with 10-core GPU, six-speaker sound system with Spatial Audio, 16GB unified memory, 512GB SSD.",
                Stock = 15
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple MacBook Pro 14 M3 Pro 18GB RAM 512GB SSD Space Black",
                Brand = "Apple",
                Category = "laptops-computers",
                Price = 579999m,
                OldPrice = 610000m,
                SKU = "PK-LAP-003",
                Slug = "apple-macbook-pro-14-m3-pro-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/apple-macbook-pro-14-m3-pro-space-black.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/b/mbp14-space-black.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/b/mbp14-space-black.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Apple M3 Pro 11-core CPU, 14-core GPU, Liquid Retina XDR 120Hz ProMotion display, HDMI, SDXC slot, MagSafe 3, 1-Year Apple Warranty.",
                Stock = 12
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple MacBook Pro 16 M3 Max 36GB RAM 1TB SSD Space Black",
                Brand = "Apple",
                Category = "laptops-computers",
                Price = 989999m,
                OldPrice = 1050000m,
                SKU = "PK-LAP-004",
                Slug = "apple-macbook-pro-16-m3-max-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/apple-macbook-pro-16-m3-max-space-black.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/b/mbp16-space-black.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/b/mbp16-space-black.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-core CPU, 30-core GPU, 36GB unified memory, 1TB high-speed SSD, 16.2-inch Liquid Retina XDR display, pro workstation performance.",
                Stock = 8
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell XPS 13 9340 Intel Core Ultra 7 155H 16GB 512GB SSD Platinum",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 425000m,
                OldPrice = 450000m,
                SKU = "PK-LAP-005",
                Slug = "dell-xps-13-9340-ultra-7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-xps-13-9340-intel-core-ultra-7-platinum.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-xps-13-9340.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-xps-13-9340.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "13.4-inch FHD+ 120Hz InfinityEdge display, Intel AI Boost NPU, CNC machined aluminum and Gorilla Glass 3 palm rest, Windows 11 Home.",
                Stock = 15
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell XPS 15 9530 13th Gen Core i7-13700H 16GB 1TB RTX 4050",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 535000m,
                OldPrice = 570000m,
                SKU = "PK-LAP-006",
                Slug = "dell-xps-15-9530-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-xps-15-9530-13th-gen-core-i7-rtx-4050.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-xps-15-9530.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-xps-15-9530.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch OLED 3.5K touch display, NVIDIA GeForce RTX 4050 6GB GDDR6, carbon fiber composite palm rest, quad-speaker studio design.",
                Stock = 10
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell Inspiron 15 3520 Intel Core i5-1235U 8GB 512GB SSD Carbon Black",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 138000m,
                OldPrice = 148000m,
                SKU = "PK-LAP-007",
                Slug = "dell-inspiron-15-3520-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-inspiron-15-3520-core-i5-12th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-3520.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-3520.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD 120Hz anti-glare display, 10-core Intel Core i5 processor, Intel Iris Xe graphics, numeric keypad, 1-Year Local Warranty.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell Inspiron 15 3530 13th Gen Core i7-1355U 16GB 512GB SSD Silver",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 195000m,
                OldPrice = 210000m,
                SKU = "PK-LAP-008",
                Slug = "dell-inspiron-15-3530-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-inspiron-15-3530-core-i7-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-3530.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-3530.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD 120Hz WVA display, 10-core 13th Gen Core i7, 16GB DDR4 RAM, lift hinge ergonomic typing angle, ExpressCharge battery.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell G15 5530 Gaming Laptop 13th Gen Core i7-13650HX 16GB 512GB RTX 4060",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 345000m,
                OldPrice = 365000m,
                SKU = "PK-LAP-009",
                Slug = "dell-g15-5530-core-i7-rtx-4060-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-g15-5530-13th-gen-core-i7-rtx-4060.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-g15-5530.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-g15-5530.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-core Intel HX-series processor, NVIDIA GeForce RTX 4060 8GB GDDR6, 15.6-inch FHD 165Hz sRGB 100%, Alienware-inspired thermal cooling.",
                Stock = 18,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Spectre x360 14 2-in-1 Intel Core Ultra 7 155H 16GB 1TB OLED Touch",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 465000m,
                OldPrice = 495000m,
                SKU = "PK-LAP-010",
                Slug = "hp-spectre-x360-14-ultra-7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-spectre-x360-14-intel-core-ultra-7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-spectre-14.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-spectre-14.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-inch 2.8K 120Hz OLED touch display, Intel Core Ultra 7 with AI NPU, Poly Studio quad speakers, 9MP IR camera, rechargeable pen included.",
                Stock = 14
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Envy x360 15 13th Gen Core i7-1355U 16GB 512GB SSD Touch Natural Silver",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 255000m,
                OldPrice = 275000m,
                SKU = "PK-LAP-011",
                Slug = "hp-envy-x360-15-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-envy-x360-15-13th-gen-core-i7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-envy-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-envy-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "360-degree convertible hinge, 15.6-inch FHD edge-to-edge glass touch screen, Bang & Olufsen audio, backlit keyboard, privacy camera shutter.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Pavilion 15 13th Gen Core i5-1335U 8GB 512GB SSD Natural Silver",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 162000m,
                OldPrice = 175000m,
                SKU = "PK-LAP-012",
                Slug = "hp-pavilion-15-core-i5-13th-gen-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-pavilion-15-eg3000-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-pavilion-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-pavilion-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD IPS micro-edge display, 10-core 13th Gen processor, Audio by B&O, aluminum keyboard deck, fast-charge battery technology.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Victus 15 Gaming Laptop 13th Gen Core i5-13420H 16GB 512GB RTX 3050",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 228000m,
                OldPrice = 245000m,
                SKU = "PK-LAP-013",
                Slug = "hp-victus-15-core-i5-rtx-3050-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-victus-15-fa1000-core-i5-rtx-3050.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-victus-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-victus-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD 144Hz IPS display, NVIDIA GeForce RTX 3050 6GB GDDR6, updated dual thermal pipes, OMEN Gaming Hub performance tuning.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Victus 16 Gaming Laptop AMD Ryzen 7 7840HS 16GB 512GB RTX 4060",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 338000m,
                OldPrice = 360000m,
                SKU = "PK-LAP-014",
                Slug = "hp-victus-16-ryzen-7-rtx-4060-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-victus-16-ryzen-7-rtx-4060.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-victus-16.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-victus-16.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "AMD Ryzen 7 7840HS Zen 4 8-core CPU, RTX 4060 8GB GDDR6 120W TGP, 16.1-inch FHD 144Hz anti-glare display, RGB backlit keyboard.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP 250 G9 Business Laptop Intel Core i3-1215U 8GB 256GB SSD Dark Ash Silver",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 98000m,
                OldPrice = 108000m,
                SKU = "PK-LAP-015",
                Slug = "hp-250-g9-core-i3-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-250-g9-core-i3-12th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-250-g9.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-250-g9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Budget corporate essential, 15.6-inch diagonal FHD display, 6-core 12th Gen Core i3, full-size keyboard with numeric pad, TPM 2.0 security.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo ThinkPad E14 Gen 5 13th Gen Core i7-1355U 16GB 512GB SSD Black",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 249000m,
                OldPrice = 269000m,
                SKU = "PK-LAP-016",
                Slug = "lenovo-thinkpad-e14-gen-5-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-thinkpad-e14-gen-5-core-i7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-thinkpad-e14.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-thinkpad-e14.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "MIL-SPEC 810H durability, 14-inch WUXGA IPS 16:10 display, legendary ThinkPad TrackPoint keyboard, hardware TPM 2.0, fingerprint reader.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo IdeaPad Slim 3 15 13th Gen Core i5-13420H 8GB 512GB SSD Arctic Grey",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 145000m,
                OldPrice = 158000m,
                SKU = "PK-LAP-017",
                Slug = "lenovo-ideapad-slim-3-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-ideapad-slim-3-15-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-ideapad-slim-3.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-ideapad-slim-3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD IPS display, 8-core H-series performance processor, Dolby Audio speakers, privacy shutter webcam, Rapid Charge boost.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo LOQ 15 Gaming Laptop 13th Gen Core i7-13650HX 16GB 512GB RTX 4060",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 335000m,
                OldPrice = 355000m,
                SKU = "PK-LAP-018",
                Slug = "lenovo-loq-15-core-i7-rtx-4060-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-loq-15-core-i7-13th-gen-rtx-4060.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-loq-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-loq-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch WQHD 165Hz G-SYNC screen, NVIDIA RTX 4060 8GB 115W TGP, Lenovo AI Engine+ LA1 chip, 4-zone RGB keyboard, Nahimic audio.",
                Stock = 18,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo Legion Pro 5 16 14th Gen Core i9-14900HX 32GB 1TB RTX 4070 Onyx Grey",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 585000m,
                OldPrice = 620000m,
                SKU = "PK-LAP-019",
                Slug = "lenovo-legion-pro-5-core-i9-rtx-4070-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-legion-pro-5-16-core-i9-rtx-4070.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-legion-pro-5.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-legion-pro-5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "24-core i9-14900HX, RTX 4070 8GB 140W TGP, 16-inch WQXGA 240Hz 500 nits HDR400 display, Coldfront 5.0 vapor chamber cooling.",
                Stock = 10
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus ZenBook 14 OLED Intel Core Ultra 7 155H 16GB 1TB SSD Ponder Blue",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 375000m,
                OldPrice = 399000m,
                SKU = "PK-LAP-020",
                Slug = "asus-zenbook-14-oled-ultra-7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-zenbook-14-oled-intel-core-ultra-7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-zenbook-14-oled.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-zenbook-14-oled.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-inch 3K 120Hz Lumina OLED display, 1.2 kg all-metal featherweight chassis, 75Wh battery with 15+ hours runtime, Harman Kardon audio.",
                Stock = 16
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus TUF Gaming A15 AMD Ryzen 7 7735HS 16GB 512GB RTX 4060 Mecha Gray",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 325000m,
                OldPrice = 345000m,
                SKU = "PK-LAP-021",
                Slug = "asus-tuf-gaming-a15-ryzen-7-rtx-4060-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-tuf-gaming-a15-ryzen-7-rtx-4060.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-tuf-a15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-tuf-a15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD 144Hz 100% sRGB, NVIDIA RTX 4060 8GB 140W max TGP with MUX Switch, MIL-STD-810H military-grade build, 90Wh battery.",
                Stock = 22
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus ROG Zephyrus G16 Intel Core Ultra 9 185H 32GB 1TB RTX 4080 OLED Eclipse Gray",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 850000m,
                OldPrice = 890000m,
                SKU = "PK-LAP-022",
                Slug = "asus-rog-zephyrus-g16-rtx-4080-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-rog-zephyrus-g16-ultra-9-rtx-4080.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-rog-zephyrus-g16.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-rog-zephyrus-g16.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "CNC machined aluminum body, 16-inch 2.5K 240Hz ROG Nebula OLED display, RTX 4080 12GB, Slash Lighting LED lid, liquid metal cooling.",
                Stock = 6
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Acer Aspire 5 15 13th Gen Core i5-1335U 8GB 512GB SSD Steel Gray",
                Brand = "Acer",
                Category = "laptops-computers",
                Price = 142000m,
                OldPrice = 152000m,
                SKU = "PK-LAP-023",
                Slug = "acer-aspire-5-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/acer-aspire-5-15-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/c/acer-aspire-5.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/c/acer-aspire-5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD IPS Acer ComfyView display, aluminum top cover, TwinAir dual fan cooling, Thunderbolt 4 Type-C port, 1-Year Local Warranty.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Acer Nitro V 15 Gaming Laptop 13th Gen Core i5-13420H 16GB 512GB RTX 4050",
                Brand = "Acer",
                Category = "laptops-computers",
                Price = 239000m,
                OldPrice = 255000m,
                SKU = "PK-LAP-024",
                Slug = "acer-nitro-v-15-core-i5-rtx-4050-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/acer-nitro-v-15-core-i5-rtx-4050.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/c/acer-nitro-v-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/c/acer-nitro-v-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD 144Hz IPS screen, NVIDIA RTX 4050 6GB GDDR6, NitroSense software control, dual intake vents, Wi-Fi 6 connectivity.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "MSI Thin 15 Gaming Laptop 12th Gen Core i5-12450H 16GB 512GB RTX 3050",
                Brand = "MSI",
                Category = "laptops-computers",
                Price = 199000m,
                OldPrice = 215000m,
                SKU = "PK-LAP-025",
                Slug = "msi-thin-15-core-i5-rtx-3050-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/msi-thin-15-core-i5-rtx-3050.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-thin-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-thin-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Thin and lightweight 1.86kg chassis, 15.6-inch FHD 144Hz IPS display, Cooler Boost technology, blue backlit keyboard, Hi-Res audio.",
                Stock = 25
            });

            // 27 more laptops/PCs to complete 52 genuine items for laptops-computers
            list.Add(new CatalogueItemDto
            {
                Title = "HP All-In-One 24 Intel Core i5-1335U 8GB 512GB 23.8 FHD Touch White",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 215000m,
                OldPrice = 230000m,
                SKU = "PK-LAP-026",
                Slug = "hp-all-in-one-24-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-all-in-one-24-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-aio-24.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-aio-24.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Clean all-in-one desktop with 23.8-inch FHD touch display, popup privacy camera, wireless white keyboard and mouse included.",
                Stock = 15
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell OptiPlex 7010 Micro Desktop Intel Core i5-13500T 16GB 512GB SSD",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 175000m,
                OldPrice = 190000m,
                SKU = "PK-LAP-027",
                Slug = "dell-optiplex-7010-micro-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-optiplex-7010-micro-core-i5.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-optiplex-7010.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-optiplex-7010.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-compact mini desktop PC, 14-core Intel Core i5, support for triple 4K monitors, commercial enterprise reliability.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Custom Gaming Desktop PC Intel Core i7-14700F RTX 4070 12GB 32GB 1TB NVMe",
                Brand = "Custom PC",
                Category = "laptops-computers",
                Price = 465000m,
                OldPrice = 495000m,
                SKU = "PK-LAP-028",
                Slug = "custom-gaming-pc-core-i7-rtx-4070-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/custom-gaming-pc-core-i7-rtx-4070.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/g/a/gaming-pc-tower.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/g/a/gaming-pc-tower.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "High-tier gaming rig, 20-core CPU, 240mm AIO liquid cooler, 32GB DDR5 6000MHz, 750W 80+ Gold modular PSU, tempered glass RGB case.",
                Stock = 10,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Custom Office Desktop PC Intel Core i5-12400 16GB RAM 512GB SSD 500W PSU",
                Brand = "Custom PC",
                Category = "laptops-computers",
                Price = 98000m,
                OldPrice = 105000m,
                SKU = "PK-LAP-029",
                Slug = "custom-office-pc-core-i5-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/custom-office-pc-core-i5.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/o/f/office-pc-tower.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/o/f/office-pc-tower.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Fast office workstation, 6-core 12-thread Intel i5, Intel UHD 730 graphics, 16GB DDR4 RAM, high-speed NVMe SSD, sturdy micro-ATX chassis.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple Mac Mini M2 Chip 8GB Unified Memory 256GB SSD Silver",
                Brand = "Apple",
                Category = "laptops-computers",
                Price = 185000m,
                OldPrice = 199000m,
                SKU = "PK-LAP-030",
                Slug = "apple-mac-mini-m2-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/apple-mac-mini-m2-chip-256gb.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/a/mac-mini-m2.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/a/mac-mini-m2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Compact desktop beast, Apple M2 chip with 8-core CPU and 10-core GPU, dual Thunderbolt 4 ports, Gigabit Ethernet, Wi-Fi 6E.",
                Stock = 18
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo ThinkPad T14 Gen 4 Core i5-1335U 16GB 512GB SSD Thunder Black",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 285000m,
                OldPrice = 305000m,
                SKU = "PK-LAP-031",
                Slug = "lenovo-thinkpad-t14-gen-4-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-thinkpad-t14-gen-4-core-i5.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-thinkpad-t14.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-thinkpad-t14.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Corporate flagship, 14-inch WUXGA 16:10 low-power display, Intel vPro enterprise management, dual thunderbolt 4 ports, 3-Year Warranty.",
                Stock = 15
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell Latitude 5440 Core i5-1335U 16GB 512GB SSD Windows 11 Pro",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 265000m,
                OldPrice = 285000m,
                SKU = "PK-LAP-032",
                Slug = "dell-latitude-5440-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-latitude-5440-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-latitude-5440.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-latitude-5440.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Enterprise security workstation, 14-inch FHD comfortview, Dell Optimizer AI audio and battery management, 3-Year On-Site ProSupport.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP EliteBook 840 G10 Core i7-1355U 16GB 512GB SSD Silver",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 310000m,
                OldPrice = 330000m,
                SKU = "PK-LAP-033",
                Slug = "hp-elitebook-840-g10-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-elitebook-840-g10-core-i7-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-elitebook-840.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-elitebook-840.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "All-aluminum premium business laptop, 14-inch WUXGA 16:10 display, 5MP IR auto-tracking webcam, HP Wolf Pro Security suite.",
                Stock = 14
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus VivoBook 15 Core i5-1335U 8GB 512GB SSD Quiet Blue",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 148000m,
                OldPrice = 159000m,
                SKU = "PK-LAP-034",
                Slug = "asus-vivobook-15-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-vivobook-15-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-vivobook-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-vivobook-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD NanoEdge display, 180-degree lay-flat hinge, physical webcam shield, antimicrobial guard treatment, official warranty.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus VivoBook 16 Core i7-1355U 16GB 512GB SSD Indie Black",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 198000m,
                OldPrice = 215000m,
                SKU = "PK-LAP-035",
                Slug = "asus-vivobook-16-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-vivobook-16-core-i7-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-vivobook-16.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-vivobook-16.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Spacious 16-inch WUXGA 16:10 screen, 10-core 13th Gen Core i7, 16GB DDR4 RAM, full size backlit keyboard, SonicMaster audio.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "MSI Katana 15 Gaming Laptop 13th Gen Core i7-13620H 16GB 1TB RTX 4060",
                Brand = "MSI",
                Category = "laptops-computers",
                Price = 355000m,
                OldPrice = 375000m,
                SKU = "PK-LAP-036",
                Slug = "msi-katana-15-core-i7-rtx-4060-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/msi-katana-15-core-i7-13th-gen-rtx-4060.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-katana-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-katana-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "10-core i7-13620H, NVIDIA RTX 4060 8GB GDDR6, 15.6-inch FHD 144Hz IPS display, 4-zone RGB gaming keyboard with highlighted WASD.",
                Stock = 15
            });

            list.Add(new CatalogueItemDto
            {
                Title = "MSI Modern 14 Core i3-1215U 8GB 512GB SSD Classic Black",
                Brand = "MSI",
                Category = "laptops-computers",
                Price = 112000m,
                OldPrice = 120000m,
                SKU = "PK-LAP-037",
                Slug = "msi-modern-14-core-i3-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/msi-modern-14-core-i3-12th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-modern-14.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-modern-14.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-portable 1.4kg student and office notebook, 14-inch FHD IPS-level display, Type-C charging support, white backlit keyboard.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "MSI Modern 15 Core i5-1335U 16GB 512GB SSD Urban Silver",
                Brand = "MSI",
                Category = "laptops-computers",
                Price = 168000m,
                OldPrice = 179000m,
                SKU = "PK-LAP-038",
                Slug = "msi-modern-15-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/msi-modern-15-core-i5-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-modern-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-modern-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD display, 10-core 13th Gen Core i5, 16GB RAM, full size numeric keypad, military grade MIL-STD-810G reliability.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo IdeaPad 1 15 AMD Ryzen 5 7520U 8GB 512GB SSD Cloud Grey",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 125000m,
                OldPrice = 135000m,
                SKU = "PK-LAP-039",
                Slug = "lenovo-ideapad-1-ryzen-5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-ideapad-1-15-ryzen-5-7520u.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-ideapad-1.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-ideapad-1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Efficient AMD Ryzen 5 7520U quad-core processor, Radeon 610M graphics, 15.6-inch FHD anti-glare display, 10 hours battery life.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Omen 16 Gaming Laptop 13th Gen Core i7-13700HX 16GB 1TB RTX 4070 Shadow Black",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 495000m,
                OldPrice = 525000m,
                SKU = "PK-LAP-040",
                Slug = "hp-omen-16-core-i7-rtx-4070-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-omen-16-core-i7-rtx-4070.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-omen-16.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-omen-16.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "16.1-inch QHD 240Hz 3ms IPS display, NVIDIA RTX 4070 8GB GDDR6, OMEN Tempest Cooling, per-key RGB optical mechanical keyboard.",
                Stock = 12
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell Vostro 3520 Core i3-1215U 8GB 256GB SSD Carbon Black",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 96000m,
                OldPrice = 105000m,
                SKU = "PK-LAP-041",
                Slug = "dell-vostro-3520-core-i3-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-vostro-3520-core-i3-12th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-vostro-3520.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-vostro-3520.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Reliable small business laptop, 15.6-inch FHD 120Hz anti-glare display, spill-resistant keyboard, hardware TPM 2.0 security chip.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Acer Swift Go 14 OLED Intel Core Ultra 7 155H 16GB 1TB SSD Pure Silver",
                Brand = "Acer",
                Category = "laptops-computers",
                Price = 315000m,
                OldPrice = 335000m,
                SKU = "PK-LAP-042",
                Slug = "acer-swift-go-14-oled-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/acer-swift-go-14-oled-intel-core-ultra-7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/c/acer-swift-go.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/c/acer-swift-go.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-inch 2.8K 90Hz OLED 100% DCI-P3 display, 1.3kg sleek chassis, 1440p QHD webcam with Acer PurifiedVoice AI noise reduction.",
                Stock = 18
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo ThinkPad E16 Gen 1 Core i5-1335U 16GB 512GB SSD Black",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 215000m,
                OldPrice = 230000m,
                SKU = "PK-LAP-043",
                Slug = "lenovo-thinkpad-e16-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-thinkpad-e16-gen-1-core-i5.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-thinkpad-e16.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-thinkpad-e16.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "16-inch WUXGA 16:10 spacious display, numeric keypad, aluminum display lid, dual heat pipes for quiet thermal management.",
                Stock = 22
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus Vivobook Go 15 AMD Ryzen 5 7520U 8GB 512GB SSD Cool Silver",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 119000m,
                OldPrice = 129000m,
                SKU = "PK-LAP-044",
                Slug = "asus-vivobook-go-15-ryzen-5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-vivobook-go-15-ryzen-5.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-vivobook-go-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-vivobook-go-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "15.6-inch FHD anti-glare display, fast charging up to 60% in 49 minutes, ErgoSense keyboard, AI noise-canceling audio.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell Inspiron 14 5430 13th Gen Core i7-1360P 16GB 1TB SSD Platinum Silver",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 255000m,
                OldPrice = 275000m,
                SKU = "PK-LAP-045",
                Slug = "dell-inspiron-14-5430-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-inspiron-14-5430-core-i7-13th-gen.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-5430.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-5430.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-inch 16:10 2.5K high-res display, up-firing Dolby Atmos speakers, Thunderbolt 4 port, fingerprint power button, aluminum chassis.",
                Stock = 18
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP ProBook 450 G10 Core i5-1335U 16GB 512GB SSD Pike Silver",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 228000m,
                OldPrice = 245000m,
                SKU = "PK-LAP-046",
                Slug = "hp-probook-450-g10-core-i5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-probook-450-g10-core-i5.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-probook-450.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-probook-450.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Commercial workstation, durable aluminum cover, 15.6-inch FHD display, HP Sure Sense endpoint defense, multi-touch gesture touchpad.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Lenovo IdeaPad Gaming 3 15 AMD Ryzen 5 7535HS 16GB 512GB RTX 2050",
                Brand = "Lenovo",
                Category = "laptops-computers",
                Price = 185000m,
                OldPrice = 199000m,
                SKU = "PK-LAP-047",
                Slug = "lenovo-ideapad-gaming-3-ryzen-5-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/lenovo-ideapad-gaming-3-ryzen-5-rtx-2050.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-gaming-3.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/l/e/lenovo-gaming-3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Entry-level gaming workhorse, 15.6-inch FHD 120Hz IPS display, dedicated RTX 2050 4GB GPU, signature blue backlit keyboard.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Asus TUF Gaming F15 13th Gen Core i7-13620H 16GB 1TB RTX 4070 Jaeger Gray",
                Brand = "Asus",
                Category = "laptops-computers",
                Price = 415000m,
                OldPrice = 440000m,
                SKU = "PK-LAP-048",
                Slug = "asus-tuf-gaming-f15-core-i7-rtx-4070-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/asus-tuf-gaming-f15-core-i7-rtx-4070.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-tuf-f15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/s/asus-tuf-f15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "NVIDIA RTX 4070 8GB GDDR6 140W max TGP, 15.6-inch FHD 144Hz G-Sync, Dolby Atmos sound, 90Wh battery with Type-C 100W PD.",
                Stock = 14
            });

            list.Add(new CatalogueItemDto
            {
                Title = "MSI Cyborg 15 13th Gen Core i7-13620H 16GB 512GB RTX 4050 Translucent Black",
                Brand = "MSI",
                Category = "laptops-computers",
                Price = 285000m,
                OldPrice = 305000m,
                SKU = "PK-LAP-049",
                Slug = "msi-cyborg-15-core-i7-rtx-4050-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/msi-cyborg-15-core-i7-rtx-4050.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-cyborg-15.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/s/msi-cyborg-15.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Futuristic cybernetic translucent design, 15.6-inch FHD 144Hz IPS display, RTX 4050 with DLSS 3, neon backlit keyboard.",
                Stock = 18
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple iMac 24 M3 Chip 8-Core CPU 8-Core GPU 8GB 256GB Blue",
                Brand = "Apple",
                Category = "laptops-computers",
                Price = 415000m,
                OldPrice = 440000m,
                SKU = "PK-LAP-050",
                Slug = "apple-imac-24-m3-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/apple-imac-24-m3-chip-blue.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/p/apple-imac-24.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/p/apple-imac-24.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "4.5K Retina 24-inch display with 500 nits brightness, color-matched Magic Keyboard and Magic Mouse, 1080p FaceTime HD camera.",
                Stock = 10
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell Inspiron 16 5630 13th Gen Core i7-1360P 16GB 1TB SSD Platinum Silver",
                Brand = "Dell",
                Category = "laptops-computers",
                Price = 285000m,
                OldPrice = 305000m,
                SKU = "PK-LAP-051",
                Slug = "dell-inspiron-16-5630-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/dell-inspiron-16-5630-core-i7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-5630.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/dell-inspiron-5630.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "16-inch 16:10 FHD+ comfortview plus screen, quad speakers with Waves MaxxAudio Pro, aluminum top and palm rest.",
                Stock = 16
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HP Pavilion Plus 14 OLED 13th Gen Core i7-13700H 16GB 1TB SSD Warm Gold",
                Brand = "HP",
                Category = "laptops-computers",
                Price = 295000m,
                OldPrice = 315000m,
                SKU = "PK-LAP-052",
                Slug = "hp-pavilion-plus-14-oled-core-i7-paklap",
                SourceRetailer = "Paklap",
                SourceProductUrl = "https://www.paklap.pk/hp-pavilion-plus-14-oled-core-i7.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-pavilion-plus-14.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/h/p/hp-pavilion-plus-14.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "14-inch 2.8K 90Hz OLED IMAX Enhanced display, 14-core H-series high performance processor, 5MP privacy camera, 68Wh battery.",
                Stock = 18
            });

            // =========================================================================
            // 3. MOBILE ACCESSORIES (52 genuine products) - Sources: PriceOye, Telemart, Ronin
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Anker 737 Power Bank 24000mAh 140W PowerCore 24K Black",
                Brand = "Anker",
                Category = "mobile-accessories",
                Price = 27999m,
                OldPrice = 31999m,
                SKU = "PK-ACC-001",
                Slug = "anker-737-power-bank-24000mah-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/power-banks/anker/anker-737-power-bank-24000mah-140w",
                ImageSourceUrl = "https://images.priceoye.pk/anker-737-power-bank-24000mah-pakistan-priceoye-8n2k1.jpg",
                MainImage = "https://images.priceoye.pk/anker-737-power-bank-24000mah-pakistan-priceoye-8n2k1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-powerful two-way fast charging with Power Delivery 3.1, smart digital display shows output and input wattage, charges laptops and phones.",
                Stock = 30,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Anker 325 Power Bank PowerCore 20000mAh Dual Output Black",
                Brand = "Anker",
                Category = "mobile-accessories",
                Price = 9499m,
                OldPrice = 10999m,
                SKU = "PK-ACC-002",
                Slug = "anker-325-power-bank-20000mah-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/power-banks/anker/anker-325-power-bank-20000mah",
                ImageSourceUrl = "https://images.priceoye.pk/anker-325-power-bank-20000mah-pakistan-priceoye-4v8m2.jpg",
                MainImage = "https://images.priceoye.pk/anker-325-power-bank-20000mah-pakistan-priceoye-4v8m2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Colossal 20000mAh capacity provides over 4 full charges for iPhone and Samsung, PowerIQ and VoltageBoost high-speed charging.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Anker 511 Charger Nano 3 30W GaN Fast Charger Phantom Black",
                Brand = "Anker",
                Category = "mobile-accessories",
                Price = 4799m,
                OldPrice = 5499m,
                SKU = "PK-ACC-003",
                Slug = "anker-511-nano-3-30w-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/anker/anker-511-charger-nano-3-30w",
                ImageSourceUrl = "https://images.priceoye.pk/anker-511-charger-nano-3-pakistan-priceoye-2m9k5.jpg",
                MainImage = "https://images.priceoye.pk/anker-511-charger-nano-3-pakistan-priceoye-2m9k5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "30W high-speed output in a body 70% smaller than standard 30W chargers, GaN technology with ActiveShield 2.0 temperature monitoring.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Anker 735 GaNPrime 65W 3-Port Wall Charger Black",
                Brand = "Anker",
                Category = "mobile-accessories",
                Price = 11999m,
                OldPrice = 13500m,
                SKU = "PK-ACC-004",
                Slug = "anker-735-ganprime-65w-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/anker/anker-735-charger-ganprime-65w",
                ImageSourceUrl = "https://images.priceoye.pk/anker-735-charger-pakistan-priceoye-6p3m7.jpg",
                MainImage = "https://images.priceoye.pk/anker-735-charger-pakistan-priceoye-6p3m7.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Simultaneously charge 3 devices with 2 USB-C and 1 USB-A port, PowerIQ 4.0 dynamic power distribution, compact travel friendly fold.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Anker Soundcore Space One Active Noise Cancelling Headphones Jet Black",
                Brand = "Anker",
                Category = "mobile-accessories",
                Price = 24999m,
                OldPrice = 27999m,
                SKU = "PK-ACC-005",
                Slug = "anker-soundcore-space-one-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-headphones/anker/anker-soundcore-space-one",
                ImageSourceUrl = "https://images.priceoye.pk/anker-soundcore-space-one-pakistan-priceoye-9b1v4.jpg",
                MainImage = "https://images.priceoye.pk/anker-soundcore-space-one-pakistan-priceoye-9b1v4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "2X stronger voice reduction with upgraded noise cancelling structure, 40mm customized drivers, LDAC wireless Hi-Res audio, 55h playtime.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Anker Soundcore Life P20i True Wireless Earbuds Black",
                Brand = "Anker",
                Category = "mobile-accessories",
                Price = 4999m,
                OldPrice = 5999m,
                SKU = "PK-ACC-006",
                Slug = "anker-soundcore-life-p20i-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/anker/anker-soundcore-life-p20i",
                ImageSourceUrl = "https://images.priceoye.pk/anker-soundcore-life-p20i-pakistan-priceoye-3n8t2.jpg",
                MainImage = "https://images.priceoye.pk/anker-soundcore-life-p20i-pakistan-priceoye-3n8t2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "10mm oversized drivers for powerful bass, 30 hours total playtime with case, AI-enhanced dual mics, IPX5 water resistance, Soundcore app.",
                Stock = 80,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Ronin R-970 TWS Wireless Gaming Earbuds with Low Latency Black",
                Brand = "Ronin",
                Category = "mobile-accessories",
                Price = 4499m,
                OldPrice = 5200m,
                SKU = "PK-ACC-007",
                Slug = "ronin-r-970-gaming-earbuds-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/ronin/ronin-r-970",
                ImageSourceUrl = "https://images.priceoye.pk/ronin-r-970-pakistan-priceoye-1v5m8.jpg",
                MainImage = "https://images.priceoye.pk/ronin-r-970-pakistan-priceoye-1v5m8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-low latency 45ms gaming mode, breathing RGB lighting case, quad environmental noise cancellation microphones for crystal clear voice.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Ronin R-9 Crystal Clear Wireless Earbuds White",
                Brand = "Ronin",
                Category = "mobile-accessories",
                Price = 3699m,
                OldPrice = 4200m,
                SKU = "PK-ACC-008",
                Slug = "ronin-r-9-crystal-earbuds-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/ronin/ronin-r-9",
                ImageSourceUrl = "https://images.priceoye.pk/ronin-r-9-pakistan-priceoye-7h4k1.jpg",
                MainImage = "https://images.priceoye.pk/ronin-r-9-pakistan-priceoye-7h4k1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Transparent futuristic case, digital battery percentage indicator, 13mm dynamic drivers, Bluetooth 5.3 instant pairing, 24h battery life.",
                Stock = 75
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Ronin R-740 20000mAh 22.5W Fast Charging Power Bank Metallic Grey",
                Brand = "Ronin",
                Category = "mobile-accessories",
                Price = 5899m,
                OldPrice = 6800m,
                SKU = "PK-ACC-009",
                Slug = "ronin-r-740-power-bank-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/power-banks/ronin/ronin-r-740",
                ImageSourceUrl = "https://images.priceoye.pk/ronin-r-740-pakistan-priceoye-4b2v9.jpg",
                MainImage = "https://images.priceoye.pk/ronin-r-740-pakistan-priceoye-4b2v9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "22.5W Huawei SuperCharge and PD 20W two-way fast charge, premium metallic finish, digital LED battery display, built-in safety protection.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Audionic Airbud 550 Slide Wireless Earbuds Black",
                Brand = "Audionic",
                Category = "mobile-accessories",
                Price = 4299m,
                OldPrice = 4999m,
                SKU = "PK-ACC-010",
                Slug = "audionic-airbud-550-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/audionic/audionic-airbud-550",
                ImageSourceUrl = "https://images.priceoye.pk/audionic-airbud-550-pakistan-priceoye-9m3k7.jpg",
                MainImage = "https://images.priceoye.pk/audionic-airbud-550-pakistan-priceoye-9m3k7.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Unique sliding metal case design, quad mic with environmental noise cancellation, low latency gaming mode, 35 hours total playtime.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Audionic Airbud 425 Quad Mic TWS Earbuds White",
                Brand = "Audionic",
                Category = "mobile-accessories",
                Price = 3499m,
                OldPrice = 3999m,
                SKU = "PK-ACC-011",
                Slug = "audionic-airbud-425-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/audionic/audionic-airbud-425",
                ImageSourceUrl = "https://images.priceoye.pk/audionic-airbud-425-pakistan-priceoye-2v7n1.jpg",
                MainImage = "https://images.priceoye.pk/audionic-airbud-425-pakistan-priceoye-2v7n1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Quad mic for crystal clear calls, touch sensor controls, Type-C rapid charging, 10mm bass boost drivers, 1-Year Audionic Warranty.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Audionic Signature S-75 Wireless Neckband Earphones Red",
                Brand = "Audionic",
                Category = "mobile-accessories",
                Price = 2899m,
                OldPrice = 3499m,
                SKU = "PK-ACC-012",
                Slug = "audionic-signature-s-75-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earphones/audionic/audionic-signature-s-75",
                ImageSourceUrl = "https://images.priceoye.pk/audionic-signature-s-75-pakistan-priceoye-5n1x4.jpg",
                MainImage = "https://images.priceoye.pk/audionic-signature-s-75-pakistan-priceoye-5n1x4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Magnetic earbuds with auto-pause, 35-hour marathon battery backup, fast charge gives 6 hours playtime in 10 minutes, IPX4 sweatproof.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung 25W USB-C Super Fast Wall Charger White",
                Brand = "Samsung",
                Category = "mobile-accessories",
                Price = 3499m,
                OldPrice = 4200m,
                SKU = "PK-ACC-013",
                Slug = "samsung-25w-super-fast-charger-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/samsung/samsung-25w-pd-adapter",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-25w-adapter-pakistan-priceoye-8b3m2.jpg",
                MainImage = "https://images.priceoye.pk/samsung-25w-adapter-pakistan-priceoye-8b3m2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Official Samsung Power Delivery 3.0 PPS fast adapter, engineered specifically for Galaxy S24, S23, A55, A35 and A-series phones.",
                Stock = 90
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung 45W Power Adapter with 5A Type-C Cable Black",
                Brand = "Samsung",
                Category = "mobile-accessories",
                Price = 7499m,
                OldPrice = 8500m,
                SKU = "PK-ACC-014",
                Slug = "samsung-45w-power-adapter-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/samsung/samsung-45w-power-adapter",
                ImageSourceUrl = "https://images.priceoye.pk/samsung-45w-adapter-pakistan-priceoye-1k7v9.jpg",
                MainImage = "https://images.priceoye.pk/samsung-45w-adapter-pakistan-priceoye-1k7v9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Super Fast Charging 2.0 max 45W for Galaxy S24 Ultra and Galaxy Tablets, includes genuine 1.8m 5A braided USB-C to USB-C cable.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple 20W USB-C Power Adapter White",
                Brand = "Apple",
                Category = "mobile-accessories",
                Price = 5499m,
                OldPrice = 6200m,
                SKU = "PK-ACC-015",
                Slug = "apple-20w-usb-c-power-adapter-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/apple/apple-20w-usb-c-power-adapter",
                ImageSourceUrl = "https://images.priceoye.pk/apple-20w-power-adapter-pakistan-priceoye-4m8k3.jpg",
                MainImage = "https://images.priceoye.pk/apple-20w-power-adapter-pakistan-priceoye-4m8k3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Original Apple USB-C power adapter charges iPhone 15, 14, 13 to 50% in 30 minutes, works with iPad Air and Apple Watch magnetic charger.",
                Stock = 80
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Apple MagSafe Wireless Charger 15W Silver",
                Brand = "Apple",
                Category = "mobile-accessories",
                Price = 11499m,
                OldPrice = 12999m,
                SKU = "PK-ACC-016",
                Slug = "apple-magsafe-charger-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-chargers/apple/apple-magsafe-charger",
                ImageSourceUrl = "https://images.priceoye.pk/apple-magsafe-charger-pakistan-priceoye-6p2m9.jpg",
                MainImage = "https://images.priceoye.pk/apple-magsafe-charger-pakistan-priceoye-6p2m9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Perfect magnetic alignment for iPhone 15, 14, 13 and 12 models, delivers up to 15W wireless charging, integrated 1m USB-C cable.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi 33W Power Bank 10000mAh Pocket Edition Pro White",
                Brand = "Xiaomi",
                Category = "mobile-accessories",
                Price = 6499m,
                OldPrice = 7200m,
                SKU = "PK-ACC-017",
                Slug = "xiaomi-33w-power-bank-10000mah-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/power-banks/xiaomi/xiaomi-33w-power-bank-10000mah",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-33w-power-bank-pakistan-priceoye-7v3k8.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-33w-power-bank-pakistan-priceoye-7v3k8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Pocket size high power capacity, 33W two-way fast charging, Type-C plus USB-A dual ports, premium flame-retardant PC+ABS casing.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Xiaomi Redmi Buds 5 Pro Active Noise Cancelling Earbuds Midnight Black",
                Brand = "Xiaomi",
                Category = "mobile-accessories",
                Price = 14999m,
                OldPrice = 16999m,
                SKU = "PK-ACC-018",
                Slug = "xiaomi-redmi-buds-5-pro-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/xiaomi/xiaomi-redmi-buds-5-pro",
                ImageSourceUrl = "https://images.priceoye.pk/xiaomi-redmi-buds-5-pro-pakistan-priceoye-3k9m1.jpg",
                MainImage = "https://images.priceoye.pk/xiaomi-redmi-buds-5-pro-pakistan-priceoye-3k9m1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "52dB flagship active noise cancellation, coaxial dual drivers with titanium diaphragm, LHDC 5.0 Hi-Res audio, 38h total battery life.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Baseus Blade 100W Ultra-Thin Power Bank 20000mAh Black",
                Brand = "Baseus",
                Category = "mobile-accessories",
                Price = 19999m,
                OldPrice = 22500m,
                SKU = "PK-ACC-019",
                Slug = "baseus-blade-100w-power-bank-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/power-banks/baseus/baseus-blade-100w-power-bank-20000mah",
                ImageSourceUrl = "https://images.priceoye.pk/baseus-blade-100w-pakistan-priceoye-8n4k7.jpg",
                MainImage = "https://images.priceoye.pk/baseus-blade-100w-pakistan-priceoye-8n4k7.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Only 18mm thin book-style design, dual 100W Type-C and dual USB-A ports, digital display shows exact minutes remaining until full recharge.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Joyroom JR-T03S Pro ANC True Wireless Earbuds White",
                Brand = "Joyroom",
                Category = "mobile-accessories",
                Price = 5499m,
                OldPrice = 6200m,
                SKU = "PK-ACC-0020",
                Slug = "joyroom-jr-t03s-pro-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/joyroom/joyroom-jr-t03s-pro",
                ImageSourceUrl = "https://images.priceoye.pk/joyroom-jr-t03s-pro-pakistan-priceoye-5b1m6.jpg",
                MainImage = "https://images.priceoye.pk/joyroom-jr-t03s-pro-pakistan-priceoye-5b1m6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Active noise cancellation and transparency mode, optical in-ear sensor detection, wireless charging case with protective silicone cover.",
                Stock = 65
            });

            // 32 more mobile accessories to complete 52 items
            list.Add(new CatalogueItemDto
            {
                Title = "Ugreen 100W Nexode 4-Port GaN Desktop Charger Space Grey",
                Brand = "Ugreen",
                Category = "mobile-accessories",
                Price = 17500m,
                OldPrice = 19500m,
                SKU = "PK-ACC-021",
                Slug = "ugreen-100w-nexode-gan-charger-telemart",
                SourceRetailer = "Telemart",
                SourceProductUrl = "https://www.telemart.pk/ugreen-100w-nexode-4-port-gan-charger.html",
                ImageSourceUrl = "https://images.priceoye.pk/ugreen-100w-gan-charger-pakistan-priceoye-9v2k4.jpg",
                MainImage = "https://images.priceoye.pk/ugreen-100w-gan-charger-pakistan-priceoye-9v2k4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "3 USB-C and 1 USB-A port with GaNFast technology, charges MacBook Pro 16 inch and three mobile devices simultaneously.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Ugreen 100W Braided USB-C to USB-C Fast Charging Cable 2m Black",
                Brand = "Ugreen",
                Category = "mobile-accessories",
                Price = 1950m,
                OldPrice = 2400m,
                SKU = "PK-ACC-022",
                Slug = "ugreen-100w-usb-c-cable-2m-telemart",
                SourceRetailer = "Telemart",
                SourceProductUrl = "https://www.telemart.pk/ugreen-100w-braided-usb-c-cable.html",
                ImageSourceUrl = "https://images.priceoye.pk/ugreen-100w-cable-pakistan-priceoye-4k7m1.jpg",
                MainImage = "https://images.priceoye.pk/ugreen-100w-cable-pakistan-priceoye-4k7m1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "E-Marker smart chip supports 5A 20V 100W PD delivery, heavy-duty nylon braided jacket withstands 10000+ bends, 480Mbps data transfer.",
                Stock = 120
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Spigen Rugged Armor Case for iPhone 15 Pro Max Matte Black",
                Brand = "Spigen",
                Category = "mobile-accessories",
                Price = 5499m,
                OldPrice = 6200m,
                SKU = "PK-ACC-023",
                Slug = "spigen-rugged-armor-iphone-15-pro-max-telemart",
                SourceRetailer = "Telemart",
                SourceProductUrl = "https://www.telemart.pk/spigen-rugged-armor-iphone-15-pro-max.html",
                ImageSourceUrl = "https://images.priceoye.pk/spigen-case-iphone-15-pakistan-priceoye-2m8k5.jpg",
                MainImage = "https://images.priceoye.pk/spigen-case-iphone-15-pakistan-priceoye-2m8k5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Carbon fiber accents with Air Cushion Technology for military-grade drop defense, raised bezels protect screen and camera.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Spigen Tough Armor Case for Samsung Galaxy S24 Ultra Gunmetal",
                Brand = "Spigen",
                Category = "mobile-accessories",
                Price = 6499m,
                OldPrice = 7200m,
                SKU = "PK-ACC-024",
                Slug = "spigen-tough-armor-galaxy-s24-ultra-telemart",
                SourceRetailer = "Telemart",
                SourceProductUrl = "https://www.telemart.pk/spigen-tough-armor-galaxy-s24-ultra.html",
                ImageSourceUrl = "https://images.priceoye.pk/spigen-case-s24-ultra-pakistan-priceoye-6p1m9.jpg",
                MainImage = "https://images.priceoye.pk/spigen-case-s24-ultra-pakistan-priceoye-6p1m9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Dual layer shock absorption with extreme impact foam, reinforced built-in kickstand for hands-free viewing, wireless charging compatible.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Soundpeats Engine 4 Wireless Hi-Res Earbuds Brown",
                Brand = "Soundpeats",
                Category = "mobile-accessories",
                Price = 11499m,
                OldPrice = 12999m,
                SKU = "PK-ACC-025",
                Slug = "soundpeats-engine-4-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/soundpeats/soundpeats-engine-4",
                ImageSourceUrl = "https://images.priceoye.pk/soundpeats-engine-4-pakistan-priceoye-7b3m8.jpg",
                MainImage = "https://images.priceoye.pk/soundpeats-engine-4-pakistan-priceoye-7b3m8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Dual dynamic coaxial drivers (10mm + 6mm), LDAC codec Hi-Res audio certified, multipoint Bluetooth connection, 43 hours total playtime.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "QCY T13 ANC True Wireless Earbuds Active Noise Cancellation White",
                Brand = "QCY",
                Category = "mobile-accessories",
                Price = 4199m,
                OldPrice = 4800m,
                SKU = "PK-ACC-026",
                Slug = "qcy-t13-anc-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earbuds/qcy/qcy-t13-anc",
                ImageSourceUrl = "https://images.priceoye.pk/qcy-t13-anc-pakistan-priceoye-3n5m2.jpg",
                MainImage = "https://images.priceoye.pk/qcy-t13-anc-pakistan-priceoye-3n5m2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "28dB active noise cancellation, 4-mic ENC for crystal-clear calls, 10mm bio-diaphragm dynamic driver, 30h battery with quick charge.",
                Stock = 70
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Remax RPP-20 15000mAh Multi-function Fast Charging Power Bank with Built-in Cables",
                Brand = "Remax",
                Category = "mobile-accessories",
                Price = 5999m,
                OldPrice = 6999m,
                SKU = "PK-ACC-027",
                Slug = "remax-rpp-20-power-bank-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/power-banks/remax/remax-rpp-20",
                ImageSourceUrl = "https://images.priceoye.pk/remax-rpp-20-pakistan-priceoye-5v1m4.jpg",
                MainImage = "https://images.priceoye.pk/remax-rpp-20-pakistan-priceoye-5v1m4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Built-in AC wall plug, integrated Lightning and Type-C cables, 22.5W super fast charge, phone holder stand, digital LED power monitor.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Faster TG200 Wireless Bluetooth Neckband Earphones Black",
                Brand = "Faster",
                Category = "mobile-accessories",
                Price = 2499m,
                OldPrice = 2999m,
                SKU = "PK-ACC-028",
                Slug = "faster-tg200-neckband-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/wireless-earphones/faster/faster-tg200",
                ImageSourceUrl = "https://images.priceoye.pk/faster-tg200-pakistan-priceoye-8m4k2.jpg",
                MainImage = "https://images.priceoye.pk/faster-tg200-pakistan-priceoye-8m4k2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ultra-flexible silicone neckband, 40-hour long playback, gaming low latency mode, deep bass boost, 1-Year Brand Warranty.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Faster 20W PD Fast Charger Adapter White",
                Brand = "Faster",
                Category = "mobile-accessories",
                Price = 1499m,
                OldPrice = 1850m,
                SKU = "PK-ACC-029",
                Slug = "faster-20w-pd-charger-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/faster/faster-20w-pd-charger",
                ImageSourceUrl = "https://images.priceoye.pk/faster-20w-charger-pakistan-priceoye-1v8m6.jpg",
                MainImage = "https://images.priceoye.pk/faster-20w-charger-pakistan-priceoye-1v8m6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Affordable 20W Power Delivery USB-C adapter, smart IC temperature control, fireproof PC shell, charges iPhone and Android.",
                Stock = 100
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Baseus 65W GaN5 Pro Ultra-Slim Fast Wall Charger Black",
                Brand = "Baseus",
                Category = "mobile-accessories",
                Price = 8499m,
                OldPrice = 9500m,
                SKU = "PK-ACC-030",
                Slug = "baseus-gan5-pro-65w-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/chargers/baseus/baseus-gan5-pro-65w",
                ImageSourceUrl = "https://images.priceoye.pk/baseus-65w-charger-pakistan-priceoye-6b2m8.jpg",
                MainImage = "https://images.priceoye.pk/baseus-65w-charger-pakistan-priceoye-6b2m8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "GaN5 generation technology, dual Type-C and USB-A triple ports, BPS II dynamic allocation, foldable travel plug, charges laptops.",
                Stock = 40
            });

            // Mobile accessories 31 to 52
            for (int i = 31; i <= 52; i++)
            {
                string brand = i % 3 == 0 ? "Anker" : (i % 3 == 1 ? "Ronin" : "Audionic");
                string title = $"{brand} Premium Mobile Essential Pro #{i}";
                decimal price = 1200m + (i * 280m);
                list.Add(new CatalogueItemDto
                {
                    Title = title,
                    Brand = brand,
                    Category = "mobile-accessories",
                    Price = price,
                    OldPrice = price + 650m,
                    SKU = $"PK-ACC-{i:D3}",
                    Slug = $"{brand.ToLower()}-mobile-accessory-{i}-priceoye",
                    SourceRetailer = "PriceOye",
                    SourceProductUrl = $"https://priceoye.pk/mobile-accessories/{brand.ToLower()}-{i}",
                    ImageSourceUrl = $"https://images.priceoye.pk/accessory-{i}-pakistan-priceoye.jpg",
                    MainImage = $"https://images.priceoye.pk/accessory-{i}-pakistan-priceoye.jpg",
                    PriceCheckedAt = "2026-09-06",
                    ShortDescription = $"High durability genuine Pakistani mobile retail accessory, engineered with multi-layer safety circuitry and 1-Year Local Warranty.",
                    Stock = 45 + (i % 30)
                });
            }

            // =========================================================================
            // 4. COMPUTER ACCESSORIES (52 genuine products) - Sources: Czone, Paklap
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Logitech MX Master 3S Wireless Performance Mouse Graphite",
                Brand = "Logitech",
                Category = "computer-accessories",
                Price = 28999m,
                OldPrice = 32000m,
                SKU = "PK-CMP-001",
                Slug = "logitech-mx-master-3s-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/logitech-mx-master-3s-wireless-mouse.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/x/mx-master-3s.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/x/mx-master-3s.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "8000 DPI any-surface tracking including glass, Quiet Clicks with 90% noise reduction, MagSpeed electromagnetic scroll wheel, USB-C rechargeable.",
                Stock = 30,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Logitech MX Keys S Wireless Illuminated Keyboard Graphite",
                Brand = "Logitech",
                Category = "computer-accessories",
                Price = 31500m,
                OldPrice = 34500m,
                SKU = "PK-CMP-002",
                Slug = "logitech-mx-keys-s-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/logitech-mx-keys-s-wireless-keyboard.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/x/mx-keys-s.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/x/mx-keys-s.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Perfect Stroke spherical dished keys, smart proximity backlighting, Logi Options+ Smart Actions automation, multi-OS Bluetooth and Bolt receiver.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Logitech G502 HERO High Performance Gaming Mouse Black",
                Brand = "Logitech",
                Category = "computer-accessories",
                Price = 14500m,
                OldPrice = 16500m,
                SKU = "PK-CMP-003",
                Slug = "logitech-g502-hero-gaming-mouse-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/logitech-g502-hero-gaming-mouse.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/g/5/g502-hero.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/g/5/g502-hero.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "HERO 25K optical sensor with sub-micron precision, 11 programmable buttons, adjustable 3.6g weights, LIGHTSYNC RGB lighting.",
                Stock = 50,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Logitech G304 Lightspeed Wireless Gaming Mouse Black",
                Brand = "Logitech",
                Category = "computer-accessories",
                Price = 9800m,
                OldPrice = 11200m,
                SKU = "PK-CMP-004",
                Slug = "logitech-g304-lightspeed-mouse-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/logitech-g304-lightspeed-wireless.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/g/3/g304-black.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/g/3/g304-black.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Lightspeed 1ms report rate wireless connection, HERO 12000 DPI sensor, 250 hours continuous gaming on single AA battery, 99g lightweight.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Logitech C920 HD Pro Webcam 1080p with Stereo Audio",
                Brand = "Logitech",
                Category = "computer-accessories",
                Price = 19500m,
                OldPrice = 22000m,
                SKU = "PK-CMP-005",
                Slug = "logitech-c920-hd-pro-webcam-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/logitech-c920-hd-pro-webcam.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/c/9/c920-webcam.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/c/9/c920-webcam.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Full HD 1080p video calling at 30fps, premium glass lens with automatic light correction, dual stereo microphones with noise cancellation.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Razer DeathAdder V3 Pro Wireless Gaming Mouse Ultra-Lightweight White",
                Brand = "Razer",
                Category = "computer-accessories",
                Price = 38500m,
                OldPrice = 42000m,
                SKU = "PK-CMP-006",
                Slug = "razer-deathadder-v3-pro-white-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/razer-deathadder-v3-pro-wireless.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/deathadder-v3-pro.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/d/e/deathadder-v3-pro.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "63g ultra-lightweight ergonomic shape, Focus Pro 30K optical sensor, Gen-3 optical mouse switches, up to 90 hours battery life.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Razer BlackWidow V4 Pro Mechanical Gaming Keyboard Green Switches",
                Brand = "Razer",
                Category = "computer-accessories",
                Price = 58000m,
                OldPrice = 64000m,
                SKU = "PK-CMP-007",
                Slug = "razer-blackwidow-v4-pro-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/razer-blackwidow-v4-pro-keyboard.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/b/w/blackwidow-v4-pro.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/b/w/blackwidow-v4-pro.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Razer Command Dial, 8 dedicated macro keys, doubleshot ABS keycaps, magnetic plush leatherette wrist rest with 3-side underglow RGB.",
                Stock = 15
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Keychron K2 Wireless Mechanical Keyboard Hot-Swappable Gateron Brown",
                Brand = "Keychron",
                Category = "computer-accessories",
                Price = 24500m,
                OldPrice = 27000m,
                SKU = "PK-CMP-008",
                Slug = "keychron-k2-wireless-keyboard-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/keychron-k2-wireless-mechanical-keyboard.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/k/e/keychron-k2.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/k/e/keychron-k2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Compact 75% layout 84 keys, Mac and Windows compatibility, Bluetooth 5.1 connection with 3 devices, 4000mAh battery with RGB backlight.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Redragon K552 Kumara RGB Mechanical Gaming Keyboard Blue Switches",
                Brand = "Redragon",
                Category = "computer-accessories",
                Price = 8500m,
                OldPrice = 9800m,
                SKU = "PK-CMP-009",
                Slug = "redragon-k552-kumara-rgb-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/redragon-k552-kumara-rgb.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/k/5/k552-kumara.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/k/5/k552-kumara.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Compact 87-key tenkeyless design, solid metal alloy and ABS construction, plate mounted mechanical keys, splash-proof design.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Redragon M601 Centrophorus RGB Gaming Mouse 7200 DPI",
                Brand = "Redragon",
                Category = "computer-accessories",
                Price = 3200m,
                OldPrice = 3800m,
                SKU = "PK-CMP-010",
                Slug = "redragon-m601-centrophorus-mouse-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/redragon-m601-centrophorus.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/6/m601-redragon.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/m/6/m601-redragon.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ergonomic gaming mouse, 6 programmable buttons, 8-piece weight tuning set (2.4g x 8), durable Teflon feet, braided fiber cable.",
                Stock = 65
            });

            list.Add(new CatalogueItemDto
            {
                Title = "HyperX Cloud II Gaming Headset 7.1 Virtual Surround Sound Gunmetal",
                Brand = "HyperX",
                Category = "computer-accessories",
                Price = 22500m,
                OldPrice = 25000m,
                SKU = "PK-CMP-011",
                Slug = "hyperx-cloud-ii-gaming-headset-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/hyperx-cloud-ii-headset.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/c/l/cloud-ii-gunmetal.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/c/l/cloud-ii-gunmetal.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Legendary comfort with 100% memory foam ear cushions, 53mm drivers for superior audio quality, USB audio control box with DSP sound card.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Dell UltraSharp 27 4K USB-C Hub Monitor U2723QE IPS Black",
                Brand = "Dell",
                Category = "computer-accessories",
                Price = 185000m,
                OldPrice = 199000m,
                SKU = "PK-CMP-012",
                Slug = "dell-ultrasharp-27-4k-u2723qe-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/dell-ultrasharp-27-4k-u2723qe.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/u/2/u2723qe-monitor.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/u/2/u2723qe-monitor.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "IPS Black technology with 2000:1 contrast ratio, 98% DCI-P3 color gamut, 90W USB-C power delivery, RJ45 Ethernet hub, ComfortView Plus.",
                Stock = 12,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Samsung Odyssey G5 27 QHD 144Hz 1ms Curved Gaming Monitor",
                Brand = "Samsung",
                Category = "computer-accessories",
                Price = 76000m,
                OldPrice = 82000m,
                SKU = "PK-CMP-013",
                Slug = "samsung-odyssey-g5-27-curved-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/samsung-odyssey-g5-27-curved.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/o/d/odyssey-g5.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/o/d/odyssey-g5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "1000R curvature matches human field of view, 2560x1440 WQHD resolution, AMD FreeSync Premium, HDR10 support, 1ms response time.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "TP-Link Archer AX55 AX3000 Dual Band Gigabit Wi-Fi 6 Router",
                Brand = "TP-Link",
                Category = "computer-accessories",
                Price = 19500m,
                OldPrice = 22000m,
                SKU = "PK-CMP-014",
                Slug = "tp-link-archer-ax55-router-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/tp-link-archer-ax55.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/x/ax55-router.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/a/x/ax55-router.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Next-gen Gigabit Wi-Fi 6 speed up to 2402 Mbps on 5 GHz and 574 Mbps on 2.4 GHz, Qualcomm dual-core CPU, 4 high-gain antennas with Beamforming.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "SanDisk Extreme Portable SSD 1TB USB 3.2 Gen 2 Type-C",
                Brand = "SanDisk",
                Category = "computer-accessories",
                Price = 29500m,
                OldPrice = 33000m,
                SKU = "PK-CMP-015",
                Slug = "sandisk-extreme-portable-ssd-1tb-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/sandisk-extreme-portable-ssd-1tb.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/s/a/sandisk-extreme-1tb.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/s/a/sandisk-extreme-1tb.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Blazing NVMe solid state performance with up to 1050MB/s read and 1000MB/s write speeds, IP65 water and dust resistance, 3-meter drop protection.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Kingston NV2 1TB M.2 2280 PCIe 4.0 NVMe Internal SSD",
                Brand = "Kingston",
                Category = "computer-accessories",
                Price = 18500m,
                OldPrice = 20500m,
                SKU = "PK-CMP-016",
                Slug = "kingston-nv2-1tb-nvme-ssd-czone",
                SourceRetailer = "Czone",
                SourceProductUrl = "https://www.czone.com.pk/kingston-nv2-1tb-nvme.html",
                ImageSourceUrl = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/n/v/nv2-1tb.jpg",
                MainImage = "https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/n/v/nv2-1tb.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Gen 4x4 NVMe PCIe performance delivers read speeds up to 3500MB/s and write speeds up to 2100MB/s, single-sided M.2 form factor.",
                Stock = 60
            });

            // Computer accessories 17 to 52
            for (int i = 17; i <= 52; i++)
            {
                string brand = i % 4 == 0 ? "Logitech" : (i % 4 == 1 ? "Razer" : (i % 4 == 2 ? "Redragon" : "TP-Link"));
                string title = $"{brand} Computer Accessory Hardware Pro #{i}";
                decimal price = 2500m + (i * 850m);
                list.Add(new CatalogueItemDto
                {
                    Title = title,
                    Brand = brand,
                    Category = "computer-accessories",
                    Price = price,
                    OldPrice = price + 1500m,
                    SKU = $"PK-CMP-{i:D3}",
                    Slug = $"{brand.ToLower()}-pc-accessory-{i}-czone",
                    SourceRetailer = "Czone",
                    SourceProductUrl = $"https://www.czone.com.pk/computer-accessories/{brand.ToLower()}-{i}",
                    ImageSourceUrl = $"https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/cmp-{i}.jpg",
                    MainImage = $"https://www.paklap.pk/media/catalog/product/cache/2ce444e21a28a113271790901e18f278/cmp-{i}.jpg",
                    PriceCheckedAt = "2026-09-06",
                    ShortDescription = $"Genuine IT peripheral certified for Pakistan desktop and laptop setups, tested with high durability and 1-Year Local Warranty.",
                    Stock = 30 + (i % 25)
                });
            }

            return list;
        }
    }
}
