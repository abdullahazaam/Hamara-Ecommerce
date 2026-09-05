using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HamaraCommerce.Models
{
    // ==========================================
    // 1. ADMIN AUDIT LOG ENTITY
    // ==========================================
    public class AdminAuditLog
    {
        public int Id { get; set; }

        [MaxLength(450)]
        public string AdminUserId { get; set; } = string.Empty;

        [MaxLength(150)]
        public string AdminUserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EntityId { get; set; }

        [MaxLength(2000)]
        public string Details { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ==========================================
    // 2. PERSISTENT STORE SETTINGS ENTITY
    // ==========================================
    public class StoreSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Store Name")]
        public string StoreName { get; set; } = "Hamara Commerce";

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        [Display(Name = "Support Email")]
        public string StoreEmail { get; set; } = "support@hamaracommerce.pk";

        [Required]
        [Phone]
        [MaxLength(50)]
        [Display(Name = "Support Phone")]
        public string StorePhone { get; set; } = "+92 300 1234567";

        [Required]
        [MaxLength(250)]
        [Display(Name = "Store Head Office Address")]
        public string StoreAddress { get; set; } = "Plaza 45, Main Boulevard, Gulberg III, Lahore, Pakistan";

        [Required]
        [MaxLength(10)]
        [Display(Name = "Currency Code")]
        public string CurrencyCode { get; set; } = "PKR";

        [Required]
        [MaxLength(10)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "Rs. ";

        [Range(0, 100)]
        [Display(Name = "Tax Rate (%)")]
        public decimal TaxRatePercent { get; set; } = 5.0m;

        [Range(0, 1000000)]
        [Display(Name = "Free Shipping Threshold")]
        public decimal FreeShippingThreshold { get; set; } = 5000.0m;

        [Range(0, 50000)]
        [Display(Name = "Standard Courier Shipping Fee")]
        public decimal StandardShippingFee { get; set; } = 250.0m;

        [Range(0, 50000)]
        [Display(Name = "Express Courier Shipping Fee")]
        public decimal ExpressShippingFee { get; set; } = 500.0m;

        [Range(1, 1000)]
        [Display(Name = "Low Stock Alert Threshold")]
        public int LowStockThreshold { get; set; } = 5;

        [Display(Name = "Enable Guest Checkout")]
        public bool EnableGuestCheckout { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ==========================================
    // 3. REAL DATABASE-DRIVEN ADMIN VIEWMODELS
    // ==========================================
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRefunds { get; set; }
        public int LowStockCount { get; set; }
        public decimal AverageOrderValue { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
        public List<AdminAuditLog> RecentAuditLogs { get; set; } = new();

        // Real Monthly Chart Data
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartRevenue { get; set; } = new();
        public List<int> ChartOrders { get; set; } = new();
    }

    public class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public string? SearchTerm { get; set; }
        public OrderStatus? StatusFilter { get; set; }
        public PaymentStatus? PaymentStatusFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class AdminCustomerListViewModel
    {
        public List<CustomerAdminViewModel> Customers { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class AdminAuditLogListViewModel
    {
        public List<AdminAuditLog> Logs { get; set; } = new();
        public string? EntityFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 15;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class AdminReviewModerationViewModel
    {
        public List<Review> Reviews { get; set; } = new();
        public bool? ApprovedFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
