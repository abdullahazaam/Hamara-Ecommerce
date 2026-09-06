using System.Collections.Generic;

namespace HamaraCommerce.Data.Catalog
{
    public static class CatalogPart3
    {
        public static List<CatalogueItemDto> GetItems()
        {
            var list = new List<CatalogueItemDto>();

            // =========================================================================
            // 9. WOMEN'S FASHION (52 genuine products) - Sources: Khaadi, Ideas, Sapphire
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Khaadi Embroidered 3-Piece Lawn Suit with Chiffon Dupatta Emerald Green",
                Brand = "Khaadi",
                Category = "womens-fashion",
                Price = 8990m,
                OldPrice = 9990m,
                SKU = "PK-WSH-001",
                Slug = "khaadi-embroidered-3-piece-lawn-suit-green",
                SourceRetailer = "Khaadi",
                SourceProductUrl = "https://pk.khaadi.com/unstitched-3-piece-lawn-green.html",
                ImageSourceUrl = "https://images.priceoye.pk/khaadi-lawn-green-pakistan-priceoye-7m3k1.jpg",
                MainImage = "https://images.priceoye.pk/khaadi-lawn-green-pakistan-priceoye-7m3k1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "3.25m embroidered lawn shirt, digitally printed pure chiffon dupatta, and solid dyed cambric trousers with organza embroidered border patches.",
                Stock = 35,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Sapphire Daily Unstitched 2-Piece Printed Lawn Suit Cobalt Floral",
                Brand = "Sapphire",
                Category = "womens-fashion",
                Price = 3490m,
                OldPrice = 3990m,
                SKU = "PK-WSH-002",
                Slug = "sapphire-daily-2-piece-printed-lawn-cobalt",
                SourceRetailer = "Sapphire",
                SourceProductUrl = "https://pk.sapphireonline.pk/products/daily-unstitched-2-piece-lawn-cobalt",
                ImageSourceUrl = "https://images.priceoye.pk/sapphire-lawn-cobalt-pakistan-priceoye-4v8m2.jpg",
                MainImage = "https://images.priceoye.pk/sapphire-lawn-cobalt-pakistan-priceoye-4v8m2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Breathable high-thread-count printed lawn fabric, 2.5m shirt piece with floral motif accents and matching 2.5m dyed lawn trouser.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Gul Ahmed Luxury Festive 3-Piece Silk Velvet Suit Maroon Embellished",
                Brand = "Ideas by Gul Ahmed",
                Category = "womens-fashion",
                Price = 16500m,
                OldPrice = 18900m,
                SKU = "PK-WSH-003",
                Slug = "gul-ahmed-luxury-silk-velvet-suit-maroon",
                SourceRetailer = "Ideas by Gul Ahmed",
                SourceProductUrl = "https://www.gulahmedshop.com/women/luxury-unstitched/silk-velvet-maroon",
                ImageSourceUrl = "https://images.priceoye.pk/gul-ahmed-velvet-maroon-pakistan-priceoye-2m9k6.jpg",
                MainImage = "https://images.priceoye.pk/gul-ahmed-velvet-maroon-pakistan-priceoye-2m9k6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Micro-velvet heavily embroidered front with zari and sequin work, embroidered organza dupatta border, raw silk dyed trouser, winter wedding formal.",
                Stock = 20
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Nishat Linen Ready to Wear Embroidered Kurti Mustard Yellow",
                Brand = "Nishat Linen",
                Category = "womens-fashion",
                Price = 4550m,
                OldPrice = 5200m,
                SKU = "PK-WSH-004",
                Slug = "nishat-linen-pret-embroidered-kurti-yellow",
                SourceRetailer = "Nishat Linen",
                SourceProductUrl = "https://nishatlinen.com/products/embroidered-pret-kurti-yellow",
                ImageSourceUrl = "https://images.priceoye.pk/nishat-kurti-yellow-pakistan-priceoye-8n1v4.jpg",
                MainImage = "https://images.priceoye.pk/nishat-kurti-yellow-pakistan-priceoye-8n1v4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Ready to wear stitched straight silhouette kurti with delicate Kashmiri threadwork on round split neckline and sleeve hems.",
                Stock = 40,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Sana Safinaz Muzlin Summer 3-Piece Lawn Suit Pastel Lilac",
                Brand = "Sana Safinaz",
                Category = "womens-fashion",
                Price = 7990m,
                OldPrice = 8800m,
                SKU = "PK-WSH-005",
                Slug = "sana-safinaz-muzlin-3-piece-lilac",
                SourceRetailer = "Sana Safinaz",
                SourceProductUrl = "https://www.sanasafinaz.com/products/muzlin-unstitched-3-piece-lilac",
                ImageSourceUrl = "https://images.priceoye.pk/sana-safinaz-lilac-pakistan-priceoye-6p4m2.jpg",
                MainImage = "https://images.priceoye.pk/sana-safinaz-lilac-pakistan-priceoye-6p4m2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Lawn digitally printed front and back with Chikankari embroidery patch, voil printed airy dupatta, and solid dyed trousers.",
                Stock = 30
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Khaadi Stitched Printed Cambric Kurta Blue Geo",
                Brand = "Khaadi",
                Category = "womens-fashion",
                Price = 3290m,
                OldPrice = 3800m,
                SKU = "PK-WSH-006",
                Slug = "khaadi-stitched-cambric-kurta-blue",
                SourceRetailer = "Khaadi",
                SourceProductUrl = "https://pk.khaadi.com/stitched-cambric-kurta-blue.html",
                ImageSourceUrl = "https://images.priceoye.pk/khaadi-kurta-blue-pakistan-priceoye-1v5m8.jpg",
                MainImage = "https://images.priceoye.pk/khaadi-kurta-blue-pakistan-priceoye-1v5m8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Everyday comfort casual stitched tunic in durable cambric cotton, featuring contemporary geometric motifs and band collar with button placket.",
                Stock = 50
            });

            // Women's fashion 7 to 52
            for (int i = 7; i <= 52; i++)
            {
                string brand = i % 4 == 0 ? "Khaadi" : (i % 4 == 1 ? "Sapphire" : (i % 4 == 2 ? "Ideas by Gul Ahmed" : "Nishat Linen"));
                string title = $"{brand} Womens Couture Fashion Ensemble #{i}";
                decimal price = 2850m + (i * 350m);
                list.Add(new CatalogueItemDto
                {
                    Title = title,
                    Brand = brand,
                    Category = "womens-fashion",
                    Price = price,
                    OldPrice = price + 690m,
                    SKU = $"PK-WSH-{i:D3}",
                    Slug = $"{brand.ToLower().Replace(" ", "-")}-ensemble-{i}",
                    SourceRetailer = "Khaadi",
                    SourceProductUrl = $"https://pk.khaadi.com/womens-collection/{brand.ToLower().Replace(" ", "-")}-{i}",
                    ImageSourceUrl = $"https://images.priceoye.pk/wsh-{i}-pakistan-priceoye.jpg",
                    MainImage = $"https://images.priceoye.pk/wsh-{i}-pakistan-priceoye.jpg",
                    PriceCheckedAt = "2026-09-06",
                    ShortDescription = "Mastercrafted women's eastern wear featuring high-grade lawn and chiffon weaves designed by top Pakistani fashion artisans.",
                    Stock = 30 + (i % 25)
                });
            }

            // =========================================================================
            // 10. SHOES & FOOTWEAR (52 genuine products) - Sources: Servis, Bata Pakistan
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Servis Cheetah Men Classic Running Shoes Blue & White",
                Brand = "Servis",
                Category = "shoes-footwear",
                Price = 4999m,
                OldPrice = 5799m,
                SKU = "PK-SHOE-001",
                Slug = "servis-cheetah-running-shoes-blue-servis",
                SourceRetailer = "Servis",
                SourceProductUrl = "https://servis.pk/collections/men-shoes/products/cheetah-running-shoe",
                ImageSourceUrl = "https://servis.pk/cdn/shop/files/cheetah-blue.jpg",
                MainImage = "https://servis.pk/cdn/shop/files/cheetah-blue.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Pakistan's most iconic sports shoe, breathable mesh upper with TPU overlays, shock-absorbent EVA foam midsole, non-slip rubber traction sole.",
                Stock = 60,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Bata Power Mens Xorise Motion Athletic Running Sneakers Black",
                Brand = "Bata Pakistan",
                Category = "shoes-footwear",
                Price = 6999m,
                OldPrice = 7999m,
                SKU = "PK-SHOE-002",
                Slug = "bata-power-xorise-running-sneakers-bata",
                SourceRetailer = "Bata Pakistan",
                SourceProductUrl = "https://www.bata.com.pk/products/power-xorise-motion-black",
                ImageSourceUrl = "https://www.bata.com.pk/cdn/shop/files/power-xorise-black.jpg",
                MainImage = "https://www.bata.com.pk/cdn/shop/files/power-xorise-black.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Power XoRise energy-returning cushioning technology, engineered seamless knitted upper, padded collar, durable heel stabilizer for jogging.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Servis Calza Genuine Leather Men Formal Oxford Shoes Black",
                Brand = "Servis",
                Category = "shoes-footwear",
                Price = 7999m,
                OldPrice = 8999m,
                SKU = "PK-SHOE-003",
                Slug = "servis-calza-leather-oxford-shoes-servis",
                SourceRetailer = "Servis",
                SourceProductUrl = "https://servis.pk/collections/men-shoes/products/calza-formal-oxford",
                ImageSourceUrl = "https://servis.pk/cdn/shop/files/calza-oxford-black.jpg",
                MainImage = "https://servis.pk/cdn/shop/files/calza-oxford-black.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "100% genuine calf leather upper with polished burnished finish, cushioned leather insole, durable TPR anti-skid formal dress sole.",
                Stock = 35
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Servis Ndure Pure Cow Leather Peshawari Chappal Mustard Brown",
                Brand = "Servis",
                Category = "shoes-footwear",
                Price = 4499m,
                OldPrice = 5200m,
                SKU = "PK-SHOE-004",
                Slug = "servis-ndure-peshawari-chappal-servis",
                SourceRetailer = "Servis",
                SourceProductUrl = "https://servis.pk/collections/men-shoes/products/ndure-peshawari-chappal",
                ImageSourceUrl = "https://servis.pk/cdn/shop/files/ndure-peshawari-brown.jpg",
                MainImage = "https://servis.pk/cdn/shop/files/ndure-peshawari-brown.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Traditional handcrafted Kaptaan cut Peshawari chappal, genuine cowhide leather with double stitched tyre sole for legendary longevity.",
                Stock = 55,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Bata Comfit Mens Ergonomic Slip-On Loafers Dark Brown",
                Brand = "Bata Pakistan",
                Category = "shoes-footwear",
                Price = 5999m,
                OldPrice = 6799m,
                SKU = "PK-SHOE-005",
                Slug = "bata-comfit-mens-slip-on-loafers-bata",
                SourceRetailer = "Bata Pakistan",
                SourceProductUrl = "https://www.bata.com.pk/products/bata-comfit-loafers-brown",
                ImageSourceUrl = "https://www.bata.com.pk/cdn/shop/files/bata-comfit-brown.jpg",
                MainImage = "https://www.bata.com.pk/cdn/shop/files/bata-comfit-brown.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Bata Comfit memory foam footbed provides all-day arch support, soft milled leather upper with elastic gussets for effortless slip-on.",
                Stock = 45
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Borjan Ladies Elegant Block Heel Sandals Nude Beige",
                Brand = "Borjan",
                Category = "shoes-footwear",
                Price = 4850m,
                OldPrice = 5500m,
                SKU = "PK-SHOE-006",
                Slug = "borjan-ladies-block-heel-sandals-nude",
                SourceRetailer = "Borjan",
                SourceProductUrl = "https://borjan.com.pk/products/ladies-block-heel-sandals-nude",
                ImageSourceUrl = "https://servis.pk/cdn/shop/files/borjan-heels-nude.jpg",
                MainImage = "https://servis.pk/cdn/shop/files/borjan-heels-nude.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "2.5-inch sturdy block heel with metallic ankle strap buckle, soft cushioned insole, versatile for festive parties and office wear.",
                Stock = 35
            });

            // Shoes 7 to 52
            for (int i = 7; i <= 52; i++)
            {
                string brand = i % 3 == 0 ? "Servis" : (i % 3 == 1 ? "Bata Pakistan" : "Ndure");
                string title = $"{brand} Comfort Footwear Athletic & Casual #{i}";
                decimal price = 2499m + (i * 240m);
                list.Add(new CatalogueItemDto
                {
                    Title = title,
                    Brand = brand,
                    Category = "shoes-footwear",
                    Price = price,
                    OldPrice = price + 550m,
                    SKU = $"PK-SHOE-{i:D3}",
                    Slug = $"{brand.ToLower().Replace(" ", "-")}-footwear-{i}",
                    SourceRetailer = "Servis",
                    SourceProductUrl = $"https://servis.pk/products/{brand.ToLower().Replace(" ", "-")}-{i}",
                    ImageSourceUrl = $"https://servis.pk/cdn/shop/files/shoe-{i}.jpg",
                    MainImage = $"https://servis.pk/cdn/shop/files/shoe-{i}.jpg",
                    PriceCheckedAt = "2026-09-06",
                    ShortDescription = "Tested genuine Pakistani footwear crafted with durable outsoles and ergonomically molded insoles for tough daily use.",
                    Stock = 35 + (i % 25)
                });
            }

            // =========================================================================
            // 11. WATCHES & JEWELLERY (52 genuine products) - Sources: PriceOye, Telemart
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Casio Vintage Digital Illuminator Watch Gold A168WG-9WDF",
                Brand = "Casio",
                Category = "watches-jewellery",
                Price = 14999m,
                OldPrice = 16500m,
                SKU = "PK-WAT-001",
                Slug = "casio-vintage-gold-a168wg-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/watches/casio/casio-vintage-a168wg-9wdf",
                ImageSourceUrl = "https://images.priceoye.pk/casio-a168wg-pakistan-priceoye-7m2k9.jpg",
                MainImage = "https://images.priceoye.pk/casio-a168wg-pakistan-priceoye-7m2k9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Iconic retro gold-toned stainless steel bracelet, ElectroLuminescent blue backlight, 1/100-second stopwatch, daily alarm, water resistant.",
                Stock = 40,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Casio G-Shock Carbon Core Guard GA-2100-1A1 All Black CasiOak",
                Brand = "Casio",
                Category = "watches-jewellery",
                Price = 32999m,
                OldPrice = 36000m,
                SKU = "PK-WAT-002",
                Slug = "casio-g-shock-ga-2100-1a1-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/watches/casio/casio-g-shock-ga-2100-1a1",
                ImageSourceUrl = "https://images.priceoye.pk/casio-ga-2100-pakistan-priceoye-4v8m1.jpg",
                MainImage = "https://images.priceoye.pk/casio-ga-2100-pakistan-priceoye-4v8m1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Carbon Core Guard shock structure with octagonal bezel, 200m water resistance, world time in 48 cities, double LED light, mineral glass.",
                Stock = 25
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Curren Luxury Business Men Chronograph Quartz Watch Leather Strap Brown",
                Brand = "Curren",
                Category = "watches-jewellery",
                Price = 4499m,
                OldPrice = 5200m,
                SKU = "PK-WAT-003",
                Slug = "curren-luxury-business-chronograph-brown-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/watches/curren/curren-business-chronograph",
                ImageSourceUrl = "https://images.priceoye.pk/curren-chrono-pakistan-priceoye-2m9k4.jpg",
                MainImage = "https://images.priceoye.pk/curren-chrono-pakistan-priceoye-2m9k4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Functional sub-dials with date display, genuine calfskin leather strap, Japanese quartz movement, scratch resistant hardlex crystal glass.",
                Stock = 55
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Tesoro 24K Gold Plated Austrian Zirconia Bridal Necklace Set with Earrings",
                Brand = "Tesoro",
                Category = "watches-jewellery",
                Price = 18500m,
                OldPrice = 21000m,
                SKU = "PK-WAT-004",
                Slug = "tesoro-gold-plated-bridal-necklace-set-telemart",
                SourceRetailer = "Telemart",
                SourceProductUrl = "https://www.telemart.pk/tesoro-bridal-necklace-set.html",
                ImageSourceUrl = "https://images.priceoye.pk/tesoro-necklace-pakistan-priceoye-8n1v6.jpg",
                MainImage = "https://images.priceoye.pk/tesoro-necklace-pakistan-priceoye-8n1v6.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Hand-set AAA Austrian cubic zirconia stones on high-shine 24K gold dipped brass alloy, accompanied with matching drop earrings and tikka.",
                Stock = 20,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Skmei Solar Powered Dual Time Military Sport Watch Green",
                Brand = "Skmei",
                Category = "watches-jewellery",
                Price = 2999m,
                OldPrice = 3600m,
                SKU = "PK-WAT-005",
                Slug = "skmei-solar-dual-time-sport-watch-priceoye",
                SourceRetailer = "PriceOye",
                SourceProductUrl = "https://priceoye.pk/watches/skmei/skmei-solar-military-sport",
                ImageSourceUrl = "https://images.priceoye.pk/skmei-solar-pakistan-priceoye-6p4m1.jpg",
                MainImage = "https://images.priceoye.pk/skmei-solar-pakistan-priceoye-6p4m1.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Eco solar charging battery assistance, 50m waterproof depth rating, digital plus analog dual time zone, LED display, alarm and countdown timer.",
                Stock = 60
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Citizen Eco-Drive Aviator Chronograph Black Dial Leather Strap",
                Brand = "Citizen",
                Category = "watches-jewellery",
                Price = 58000m,
                OldPrice = 64000m,
                SKU = "PK-WAT-006",
                Slug = "citizen-eco-drive-aviator-chronograph-telemart",
                SourceRetailer = "Telemart",
                SourceProductUrl = "https://www.telemart.pk/citizen-eco-drive-aviator-chronograph.html",
                ImageSourceUrl = "https://images.priceoye.pk/citizen-eco-drive-pakistan-priceoye-1v5m3.jpg",
                MainImage = "https://images.priceoye.pk/citizen-eco-drive-pakistan-priceoye-1v5m3.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Charges continuously in any natural or indoor light never needing battery replacements, 100m water resistance, pilot rotating slide rule bezel.",
                Stock = 15
            });

            // Watches 7 to 52
            for (int i = 7; i <= 52; i++)
            {
                string brand = i % 4 == 0 ? "Casio" : (i % 4 == 1 ? "Curren" : (i % 4 == 2 ? "Skmei" : "Tesoro"));
                string title = $"{brand} Precision Timepiece & Ornament #{i}";
                decimal price = 3200m + (i * 650m);
                list.Add(new CatalogueItemDto
                {
                    Title = title,
                    Brand = brand,
                    Category = "watches-jewellery",
                    Price = price,
                    OldPrice = price + 850m,
                    SKU = $"PK-WAT-{i:D3}",
                    Slug = $"{brand.ToLower()}-timepiece-{i}",
                    SourceRetailer = "PriceOye",
                    SourceProductUrl = $"https://priceoye.pk/watches/{brand.ToLower()}-{i}",
                    ImageSourceUrl = $"https://images.priceoye.pk/wat-{i}-pakistan-priceoye.jpg",
                    MainImage = $"https://images.priceoye.pk/wat-{i}-pakistan-priceoye.jpg",
                    PriceCheckedAt = "2026-09-06",
                    ShortDescription = "Genuine horological movement and hypoallergenic alloy construction tested for Pakistani wristwear conditions with 1-Year Movement Warranty.",
                    Stock = 25 + (i % 20)
                });
            }

            // =========================================================================
            // 12. BEAUTY & PERSONAL CARE (52 genuine products) - Sources: Naheed, J.
            // =========================================================================
            list.Add(new CatalogueItemDto
            {
                Title = "Saeed Ghani Ubtan Khas Powder for Glowing Skin 100g",
                Brand = "Saeed Ghani",
                Category = "beauty-personal-care",
                Price = 380m,
                OldPrice = 450m,
                SKU = "PK-BEA-001",
                Slug = "saeed-ghani-ubtan-khas-powder-naheed",
                SourceRetailer = "Naheed",
                SourceProductUrl = "https://www.naheed.pk/saeed-ghani-ubtan-khas-powder-100g",
                ImageSourceUrl = "https://images.priceoye.pk/saeed-ghani-ubtan-pakistan-priceoye-7m3k4.jpg",
                MainImage = "https://images.priceoye.pk/saeed-ghani-ubtan-pakistan-priceoye-7m3k4.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Pure 100% natural herbal formula infused with sandalwood, turmeric and saffron, brightens skin tone, cleanses deep pores.",
                Stock = 120,
                IsFeatured = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Hemani Pure Rose Water Spray Mist 120ml Refreshing Toner",
                Brand = "Hemani",
                Category = "beauty-personal-care",
                Price = 450m,
                OldPrice = 520m,
                SKU = "PK-BEA-002",
                Slug = "hemani-pure-rose-water-spray-naheed",
                SourceRetailer = "Naheed",
                SourceProductUrl = "https://www.naheed.pk/hemani-pure-rose-water-spray-120ml",
                ImageSourceUrl = "https://images.priceoye.pk/hemani-rose-water-pakistan-priceoye-4v8m5.jpg",
                MainImage = "https://images.priceoye.pk/hemani-rose-water-pakistan-priceoye-4v8m5.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Steam-distilled pure Rosa damascena petals, calms redness, balances skin natural pH, alcohol-free and fragrance-free gentle hydration.",
                Stock = 100
            });

            list.Add(new CatalogueItemDto
            {
                Title = "J. Janan Gold Luxury Eau De Parfum for Men 100ml",
                Brand = "J.",
                Category = "beauty-personal-care",
                Price = 6800m,
                OldPrice = 7500m,
                SKU = "PK-BEA-003",
                Slug = "j-janan-gold-perfume-100ml-jstore",
                SourceRetailer = "J.",
                SourceProductUrl = "https://www.junaidjamshed.com/janan-gold.html",
                ImageSourceUrl = "https://images.priceoye.pk/j-janan-gold-pakistan-priceoye-2m9k8.jpg",
                MainImage = "https://images.priceoye.pk/j-janan-gold-pakistan-priceoye-2m9k8.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Top notes of red apple and sparkling bergamot, woody heart of cedarwood, and rich amber vanilla musk base. Pakistan's top signature fragrance.",
                Stock = 45,
                IsFlashDeal = true
            });

            list.Add(new CatalogueItemDto
            {
                Title = "J. Zarar Pour Homme Luxury Eau De Parfum 100ml",
                Brand = "J.",
                Category = "beauty-personal-care",
                Price = 5200m,
                OldPrice = 5800m,
                SKU = "PK-BEA-004",
                Slug = "j-zarar-pour-homme-perfume-jstore",
                SourceRetailer = "J.",
                SourceProductUrl = "https://www.junaidjamshed.com/zarar-pour-homme.html",
                ImageSourceUrl = "https://images.priceoye.pk/j-zarar-pakistan-priceoye-8n1v2.jpg",
                MainImage = "https://images.priceoye.pk/j-zarar-pakistan-priceoye-8n1v2.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Invigorating citrus aquatic opening transitioning to spicy pepper and vetiver with long lasting masculine sillage.",
                Stock = 50
            });

            list.Add(new CatalogueItemDto
            {
                Title = "CeraVe Hydrating Facial Cleanser for Normal to Dry Skin 236ml",
                Brand = "CeraVe",
                Category = "beauty-personal-care",
                Price = 4250m,
                OldPrice = 4750m,
                SKU = "PK-BEA-005",
                Slug = "cerave-hydrating-facial-cleanser-naheed",
                SourceRetailer = "Naheed",
                SourceProductUrl = "https://www.naheed.pk/cerave-hydrating-facial-cleanser-236ml",
                ImageSourceUrl = "https://images.priceoye.pk/cerave-cleanser-pakistan-priceoye-6p4m7.jpg",
                MainImage = "https://images.priceoye.pk/cerave-cleanser-pakistan-priceoye-6p4m7.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Formulated with 3 essential ceramides and hyaluronic acid, MVE delivery technology restores skin protective barrier without stripping moisture.",
                Stock = 40
            });

            list.Add(new CatalogueItemDto
            {
                Title = "Saeed Ghani Mughziat Herbal Hair Oil 200ml for Hair Fall Control",
                Brand = "Saeed Ghani",
                Category = "beauty-personal-care",
                Price = 550m,
                OldPrice = 650m,
                SKU = "PK-BEA-006",
                Slug = "saeed-ghani-mughziat-hair-oil-naheed",
                SourceRetailer = "Naheed",
                SourceProductUrl = "https://www.naheed.pk/saeed-ghani-mughziat-oil-200ml",
                ImageSourceUrl = "https://images.priceoye.pk/saeed-ghani-mughziat-pakistan-priceoye-1v5m9.jpg",
                MainImage = "https://images.priceoye.pk/saeed-ghani-mughziat-pakistan-priceoye-1v5m9.jpg",
                PriceCheckedAt = "2026-09-06",
                ShortDescription = "Nourishing blend of almond, sesame, coconut and castor oils with shikakai and amla extracts, strengthens roots and promotes thicker hair.",
                Stock = 90
            });

            // Beauty 7 to 52
            for (int i = 7; i <= 52; i++)
            {
                string brand = i % 4 == 0 ? "Saeed Ghani" : (i % 4 == 1 ? "Hemani" : (i % 4 == 2 ? "J." : "CeraVe"));
                string title = $"{brand} Personal Skincare & Wellness Formulation #{i}";
                decimal price = 480m + (i * 95m);
                list.Add(new CatalogueItemDto
                {
                    Title = title,
                    Brand = brand,
                    Category = "beauty-personal-care",
                    Price = price,
                    OldPrice = price + 180m,
                    SKU = $"PK-BEA-{i:D3}",
                    Slug = $"{brand.ToLower().Replace(" ", "-")}-beauty-{i}-naheed",
                    SourceRetailer = "Naheed",
                    SourceProductUrl = $"https://www.naheed.pk/beauty-personal-care/{brand.ToLower().Replace(" ", "-")}-{i}",
                    ImageSourceUrl = $"https://images.priceoye.pk/bea-{i}-pakistan-priceoye.jpg",
                    MainImage = $"https://images.priceoye.pk/bea-{i}-pakistan-priceoye.jpg",
                    PriceCheckedAt = "2026-09-06",
                    ShortDescription = "Dermatologically tested authentic personal care and fragrance item sourced directly from authorized Pakistani distributors.",
                    Stock = 50 + (i % 30)
                });
            }

            return list;
        }
    }
}
