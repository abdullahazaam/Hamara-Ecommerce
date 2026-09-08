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
                new Category { Name = "Mobile Phones", Slug = "mobile-phones", Icon = "fa-mobile-screen-button", Description = "Smartphones, flagship devices & folding phones", DisplayOrder = 1, IsFeatured = true, IsActive = true },
                new Category { Name = "Laptops & Computers", Slug = "laptops-computers", Icon = "fa-laptop", Description = "Workstations, laptops, ultrabooks & tablets", DisplayOrder = 2, IsFeatured = true, IsActive = true },
                new Category { Name = "Headphones & Audio", Slug = "headphones-audio", Icon = "fa-headphones", Description = "Wireless headphones, earbuds, speakers & soundbars", DisplayOrder = 3, IsFeatured = true, IsActive = true },
                new Category { Name = "Smart Watches", Slug = "smart-watches", Icon = "fa-clock", Description = "Smartwatches, fitness bands & connected wearables", DisplayOrder = 4, IsFeatured = true, IsActive = true },
                new Category { Name = "Home Appliances", Slug = "home-appliances", Icon = "fa-plug", Description = "Inverter ACs, refrigerators, washing machines, vacuum cleaners & water dispensers", DisplayOrder = 5, IsFeatured = true, IsActive = true },
                new Category { Name = "Kitchen Appliances", Slug = "kitchen-appliances", Icon = "fa-blender", Description = "Microwave ovens, blenders, electric stoves, cooktops & cookware", DisplayOrder = 6, IsFeatured = true, IsActive = true },
                new Category { Name = "Beauty & Personal Care", Slug = "beauty-personal-care", Icon = "fa-wand-magic-sparkles", Description = "Skincare, luxury fragrances, cosmetics & personal grooming", DisplayOrder = 7, IsFeatured = true, IsActive = true },
                new Category { Name = "Gaming", Slug = "gaming", Icon = "fa-gamepad", Description = "Gaming consoles, controllers, mechanical keyboards & gaming accessories", DisplayOrder = 8, IsFeatured = true, IsActive = true },
                new Category { Name = "Cameras & Accessories", Slug = "cameras-accessories", Icon = "fa-camera", Description = "DSLR cameras, mirrorless cameras, action cameras, drones & tripods", DisplayOrder = 9, IsFeatured = true, IsActive = true },
                new Category { Name = "Televisions", Slug = "televisions", Icon = "fa-tv", Description = "Smart 4K UHD TVs, QLED, OLED & Android LED televisions", DisplayOrder = 10, IsFeatured = true, IsActive = true },
                new Category { Name = "Home & Living", Slug = "home-living", Icon = "fa-couch", Description = "Luxury bedding, comforters, furniture, lamps & home decor", DisplayOrder = 11, IsFeatured = true, IsActive = true },
                new Category { Name = "Sports & Fitness", Slug = "sports-fitness", Icon = "fa-dumbbell", Description = "Cricket gear, footballs, rackets, gym & fitness equipment", DisplayOrder = 12, IsFeatured = true, IsActive = true }
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
