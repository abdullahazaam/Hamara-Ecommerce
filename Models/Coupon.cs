using System;

namespace HamaraCommerce.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double DiscountPercentage { get; set; }
        public decimal FixedDiscountAmount { get; set; }
        public decimal MinimumSpend { get; set; } = 0;
        public decimal? MaxDiscountAmount { get; set; }
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-1);
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public bool IsActive { get; set; } = true;
        
        public int UsageCount { get; set; } = 0;
        public int UsageLimit { get; set; } = 500;
        public int PerUserLimit { get; set; } = 0;
        
        public int? ApplicableCategoryId { get; set; }
        public Category? ApplicableCategory { get; set; }
        
        public int? ApplicableProductId { get; set; }
        public Product? ApplicableProduct { get; set; }
        
        public bool FreeShipping { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
