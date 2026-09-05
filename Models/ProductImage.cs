using System;

namespace HamaraCommerce.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int SortOrder { get; set; } = 0;
        public bool IsMain { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
