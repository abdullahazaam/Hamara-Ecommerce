using System;

namespace HamaraCommerce.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }
        
        public InventoryMovementType MovementType { get; set; }
        
        // Positive for addition/restock/cancellation, negative for sale/deduction
        public int QuantityChange { get; set; }
        public int OldStock { get; set; }
        public int NewStock { get; set; }
        
        public string Reason { get; set; } = string.Empty;
        
        public string? AdminUserId { get; set; }
        public string? AdminUserName { get; set; }
        
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
