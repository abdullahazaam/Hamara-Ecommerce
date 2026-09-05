using System;
using System.Collections.Generic;

namespace HamaraCommerce.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Storage { get; set; }
        
        public decimal PriceAdjustment { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        
        public List<InventoryMovement> InventoryMovements { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
