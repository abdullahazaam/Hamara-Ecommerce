using System.Collections.Generic;

namespace HamaraCommerce.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-box";
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int ProductCount { get; set; }
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public List<Category> SubCategories { get; set; } = new();

        public List<Product> Products { get; set; } = new();
    }
}
