using System;

namespace HamaraCommerce.Models
{
    public class CouponRedemption
    {
        public int Id { get; set; }
        
        public int CouponId { get; set; }
        public Coupon? Coupon { get; set; }
        
        public string CouponCode { get; set; } = string.Empty;
        
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        public string? UserId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        
        public decimal DiscountAmount { get; set; }
        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRestored { get; set; } = false;
        public DateTime? RestoredAt { get; set; }
        public string? RestoreReason { get; set; }
    }
}
