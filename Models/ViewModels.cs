using System;
using System.Collections.Generic;

namespace HamaraCommerce.Models
{
    public class ShopFilterViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        
        // Filter criteria
        public string? SearchQuery { get; set; }
        public string? Category { get; set; }
        public string? SelectedBrand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "rating", "newest", "popular"
        public bool InStockOnly { get; set; }
        public bool OnSaleOnly { get; set; }
        public bool FlashDealOnly { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new();
        public List<Product> RelatedProducts { get; set; } = new();
        public List<Product> FrequentlyBoughtTogether { get; set; } = new();
    }

    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        
        public decimal RevenueGrowthPercentage { get; set; } = 14.8m;
        public decimal OrdersGrowthPercentage { get; set; } = 8.2m;
        
        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
        public List<Product> TopSellingProducts { get; set; } = new();
        
        // Monthly Sales Data for Chart.js
        public List<string> ChartMonths { get; set; } = new() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug" };
        public List<decimal> ChartRevenueData { get; set; } = new() { 12400, 18500, 22100, 19800, 27400, 31200, 36800, 42500 };
        public List<int> ChartOrdersData { get; set; } = new() { 140, 210, 280, 230, 340, 390, 450, 520 };
    }

    public class CustomerAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class CheckoutViewModel
    {
        public ShoppingCartViewModel Cart { get; set; } = new();
        
        // Customer Step
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        
        // Address Step
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "United States";
        
        // Shipping Step
        public string ShippingMethod { get; set; } = "Standard Ground";
        public decimal ShippingCost { get; set; } = 15.00m;
        
        // Payment Step
        public string PaymentMethod { get; set; } = "CreditCard"; // CreditCard, ApplePay, GooglePay, PayPal, COD
        public string CardNumber { get; set; } = string.Empty;
        public string CardExpiry { get; set; } = string.Empty;
        public string CardCVC { get; set; } = string.Empty;
        
        public string? PromoCode { get; set; }
    }
}
