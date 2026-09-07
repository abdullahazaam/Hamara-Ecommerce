using System.Collections.Generic;
using HamaraCommerce.Models;

namespace HamaraCommerce.Data.Catalog
{
    public class DepartmentDef
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-box";
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<SubcategoryDef> Subcategories { get; set; } = new();
    }

    public class SubcategoryDef
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-tag";
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int? ExistingId { get; set; }
    }

    public static class MarketplaceTaxonomy
    {
        public static List<Category> GetOfficialCategories()
        {
            return new List<Category>
            {
                new Category { Name = "Beauty & Personal Care", Slug = "beauty-personal-care", Icon = "fa-wand-magic-sparkles", Description = "Herbal skincare, luxury Pakistani fragrances, cosmetics & hair care", DisplayOrder = 1, IsFeatured = true, IsActive = true },
                new Category { Name = "Kitchen Appliances", Slug = "kitchen-appliances", Icon = "fa-blender", Description = "Air fryers, food processors, blenders & microwave ovens", DisplayOrder = 2, IsFeatured = true, IsActive = true },
                new Category { Name = "Grocery & Beverages", Slug = "grocery-beverages", Icon = "fa-basket-shopping", Description = "Basmati rice, cooking oils, spices & premium black teas", DisplayOrder = 3, IsFeatured = true, IsActive = true },
                new Category { Name = "Sports & Fitness", Slug = "sports-fitness", Icon = "fa-dumbbell", Description = "Sports gear, training essentials, fitness & gym equipment", DisplayOrder = 4, IsFeatured = true, IsActive = true },
                new Category { Name = "Watches & Jewellery", Slug = "watches-jewellery", Icon = "fa-clock", Description = "Chronographs, digital watches & fine jewellery", DisplayOrder = 5, IsFeatured = true, IsActive = true },
                new Category { Name = "Mobile Phones", Slug = "mobile-phones", Icon = "fa-mobile-screen-button", Description = "Smartphones, flagship devices & folding phones", DisplayOrder = 6, IsFeatured = true, IsActive = true },
                new Category { Name = "Women's Fashion", Slug = "womens-fashion", Icon = "fa-person-dress", Description = "Designer lawn, stitched pret kurtis & festive wear", DisplayOrder = 7, IsFeatured = true, IsActive = true },
                new Category { Name = "Mobile Accessories", Slug = "mobile-accessories", Icon = "fa-headphones", Description = "Fast chargers, power banks, cases & cables", DisplayOrder = 8, IsFeatured = true, IsActive = true },
                new Category { Name = "Shoes & Footwear", Slug = "shoes-footwear", Icon = "fa-shoe-prints", Description = "Running sneakers, formal footwear & Peshawari chappals", DisplayOrder = 9, IsFeatured = true, IsActive = true },
                new Category { Name = "Laptops & Computers", Slug = "laptops-computers", Icon = "fa-laptop", Description = "Workstations, laptops, ultrabooks & computing devices", DisplayOrder = 10, IsFeatured = true, IsActive = true },
                new Category { Name = "Car Accessories", Slug = "car-accessories", Icon = "fa-car", Description = "Car covers, floor mats, tyre inflators & accessories", DisplayOrder = 11, IsFeatured = true, IsActive = true },
                new Category { Name = "Furniture & Decor", Slug = "furniture-decor", Icon = "fa-chair", Description = "Ergonomic mesh chairs, study desks & minimalist decor", DisplayOrder = 12, IsFeatured = true, IsActive = true },
                new Category { Name = "Home & Living", Slug = "home-living", Icon = "fa-couch", Description = "Luxury bed sheet sets, comforters, bath towels & cushions", DisplayOrder = 13, IsFeatured = true, IsActive = true },
                new Category { Name = "Men's Fashion", Slug = "mens-fashion", Icon = "fa-shirt", Description = "Kurtas, unstitched wash & wear fabrics, formal shirts & jeans", DisplayOrder = 14, IsFeatured = true, IsActive = true },
                new Category { Name = "Motorcycle Accessories", Slug = "motorcycle-accessories", Icon = "fa-motorcycle", Description = "Helmets, riding gloves, locks, chains & bike accessories", DisplayOrder = 15, IsFeatured = true, IsActive = true }
            };
        }

        public static List<DepartmentDef> GetDepartments()
        {
            var official = GetOfficialCategories();
            var list = new List<DepartmentDef>();
            foreach (var c in official)
            {
                list.Add(new DepartmentDef
                {
                    Name = c.Name,
                    Slug = c.Slug,
                    Icon = c.Icon,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder
                });
            }
            return list;
        }
    }
}
