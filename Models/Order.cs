using System;
using System.Collections.Generic;

namespace HamaraCommerce.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }
        public string? VariantName { get; set; }
        
        // Immutable historical snapshot of purchased item details
        public string ProductTitle { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class Order
    {
        // Internal database primary key (Private to system)
        public int Id { get; set; }

        // Public Cryptographically Random Order Reference (e.g. HC-PK-892184920)
        public string OrderNumber { get; set; } = string.Empty;

        // Public Shipping Tracking Reference
        public string TrackingNumber { get; set; } = string.Empty;
        
        // Associated User (Optional for Guest checkout, filled for authenticated customers)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        // Customer Details
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        
        // Shipping Address
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty; // Province / State
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Pakistan";
        
        // Shipping & Payment Configuration
        public string ShippingMethod { get; set; } = "Standard Courier (3-5 Days)";
        public string PaymentMethod { get; set; } = "Cash on Delivery";
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public OrderStatus Status { get; set; } = OrderStatus.Confirmed;
        
        public string Currency { get; set; } = "PKR";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime EstimatedDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(4);
        
        // Order Financial Summary (Calculated Authoritatively)
        public List<OrderItem> Items { get; set; } = new();
        public List<PaymentTransaction> Payments { get; set; } = new();

        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        
        public string? CouponCode { get; set; }
        public string? CustomerNotes { get; set; }
        
        // Secure, order-scoped expiring token for anonymous guest confirmation, tracking & invoices
        public string? GuestAccessToken { get; set; }
        public DateTime? GuestAccessExpiry { get; set; }
    }
}
