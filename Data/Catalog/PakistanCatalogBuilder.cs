using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HamaraCommerce.Models;

namespace HamaraCommerce.Data.Catalog
{
    public static class PakistanCatalogBuilder
    {
        public static List<CatalogueItemDto> GetAllProducts()
        {
            var list = new List<CatalogueItemDto>();
            list.AddRange(CatalogPart1.GetItems());
            list.AddRange(CatalogPart2.GetItems());
            list.AddRange(CatalogPart3.GetItems());
            list.AddRange(CatalogPart4.GetItems());
            list.AddRange(CatalogPart5.GetItems());
            list.AddRange(CatalogPart6.GetItems());
            return list;
        }

        public static List<Category> GetOfficialCategories()
        {
            return new List<Category>
            {
                new Category { Name = "Mobile Phones", Slug = "mobile-phones", Icon = "fa-mobile-screen-button", Description = "Smartphones, flagship devices & folding phones", DisplayOrder = 1, IsFeatured = true },
                new Category { Name = "Laptops & Computers", Slug = "laptops-computers", Icon = "fa-laptop", Description = "Workstations, gaming rigs & ultrabooks", DisplayOrder = 2, IsFeatured = true },
                new Category { Name = "Mobile Accessories", Slug = "mobile-accessories", Icon = "fa-headphones", Description = "Fast chargers, power banks, cases & cables", DisplayOrder = 3, IsFeatured = true },
                new Category { Name = "Computer Accessories", Slug = "computer-accessories", Icon = "fa-keyboard", Description = "Mechanical keyboards, gaming mice & monitors", DisplayOrder = 4, IsFeatured = true },
                new Category { Name = "TVs & Entertainment", Slug = "tvs-entertainment", Icon = "fa-tv", Description = "4K QLED smart TVs, home theaters & soundbars", DisplayOrder = 5, IsFeatured = true },
                new Category { Name = "Home Appliances", Slug = "home-appliances", Icon = "fa-plug", Description = "Inverter air conditioners, refrigerators & washing machines", DisplayOrder = 6, IsFeatured = true },
                new Category { Name = "Kitchen Appliances", Slug = "kitchen-appliances", Icon = "fa-blender", Description = "Air fryers, food processors, blenders & microwave ovens", DisplayOrder = 7, IsFeatured = true },
                new Category { Name = "Men's Fashion", Slug = "mens-fashion", Icon = "fa-shirt", Description = "Kurtas, unstitched wash & wear fabrics, formal suits & jeans", DisplayOrder = 8, IsFeatured = true },
                new Category { Name = "Women's Fashion", Slug = "womens-fashion", Icon = "fa-person-dress", Description = "Designer 3-piece lawn suits, pret kurtis & festive wear", DisplayOrder = 9, IsFeatured = true },
                new Category { Name = "Shoes & Footwear", Slug = "shoes-footwear", Icon = "fa-shoe-prints", Description = "Athletic running sneakers, formal oxfords & Peshawari chappals", DisplayOrder = 10, IsFeatured = true },
                new Category { Name = "Watches & Jewellery", Slug = "watches-jewellery", Icon = "fa-clock", Description = "Chronographs, digital watches & Austrian zirconia jewellery", DisplayOrder = 11, IsFeatured = true },
                new Category { Name = "Beauty & Personal Care", Slug = "beauty-personal-care", Icon = "fa-wand-magic-sparkles", Description = "Herbal skincare, luxury Pakistani fragrances & hair care", DisplayOrder = 12, IsFeatured = true },
                new Category { Name = "Health & Wellness", Slug = "health-wellness", Icon = "fa-heart-pulse", Description = "Digital BP monitors, glucometers, multivitamins & supplements", DisplayOrder = 13, IsFeatured = true },
                new Category { Name = "Grocery & Beverages", Slug = "grocery-beverages", Icon = "fa-basket-shopping", Description = "Basmati rice, pure cooking oils, spices & premium black teas", DisplayOrder = 14, IsFeatured = true },
                new Category { Name = "Home & Living", Slug = "home-living", Icon = "fa-couch", Description = "Luxury bed sheet sets, comforters, bath towels & cushions", DisplayOrder = 15, IsFeatured = true },
                new Category { Name = "Furniture & Decor", Slug = "furniture-decor", Icon = "fa-chair", Description = "Ergonomic mesh chairs, study desks & minimalist coffee tables", DisplayOrder = 16, IsFeatured = true },
                new Category { Name = "Kids & Babies", Slug = "kids-babies", Icon = "fa-baby", Description = "Foldable strollers, anti-colic feeding bottles & organic rompers", DisplayOrder = 17, IsFeatured = true },
                new Category { Name = "Toys & Games", Slug = "toys-games", Icon = "fa-gamepad", Description = "Diecast model cars, Lego building sets & family board games", DisplayOrder = 18, IsFeatured = true },
                new Category { Name = "Sports & Fitness", Slug = "sports-fitness", Icon = "fa-dumbbell", Description = "English Willow cricket bats, leather balls & gym gear", DisplayOrder = 19, IsFeatured = true },
                new Category { Name = "Books & Stationery", Slug = "books-stationery", Icon = "fa-book", Description = "Bestselling Urdu & English literature, pens & hardbound journals", DisplayOrder = 20, IsFeatured = true },
                new Category { Name = "Car Accessories", Slug = "car-accessories", Icon = "fa-car", Description = "Car covers, floor mats, tyre inflators & emergency tools", DisplayOrder = 21, IsFeatured = true },
                new Category { Name = "Oils & Car Care", Slug = "car-care-oils", Icon = "fa-oil-can", Description = "Synthetic engine oils, waxes, washes & lubricants", DisplayOrder = 22, IsFeatured = true },
                new Category { Name = "Car Electronics", Slug = "car-electronics", Icon = "fa-car-battery", Description = "Touchscreen multimedia head units, dash cams & vacuums", DisplayOrder = 23, IsFeatured = true },
                new Category { Name = "Motorcycle Accessories", Slug = "motorcycle-accessories", Icon = "fa-motorcycle", Description = "Helmets, riding gloves, locks, chains & sprockets", DisplayOrder = 24, IsFeatured = true }
            };
        }

        public static string EnsureJsonFileGenerated(string? outputPath = null)
        {
            var target = outputPath ?? Path.Combine(AppContext.BaseDirectory, "Data", "Catalog", "pakistan-products-2026.json");
            var dir = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var items = GetAllProducts();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(target, json);
            return target;
        }
    }
}
