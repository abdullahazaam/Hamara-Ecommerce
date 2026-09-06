using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HamaraCommerce.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal CostPrice { get; set; } = 0;
        public double DiscountPercentage { get; set; }
        
        public double Rating { get; set; } = 4.5;
        public int ReviewCount { get; set; } = 0;
        public int Stock { get; set; } = 50;
        
        public ProductStatus Status { get; set; } = ProductStatus.Published;
        
        public string ShortDescription { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty;
        
        // Storage for image URLs
        private string _mainImage = string.Empty;
        public string MainImage
        {
            get
            {
                var mainFromList = Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl;
                if (!string.IsNullOrEmpty(mainFromList)) return mainFromList;
                if (!string.IsNullOrEmpty(_mainImage)) return _mainImage;
                return Images?.FirstOrDefault()?.ImageUrl ?? "/images/placeholder-product.svg";
            }
            set => _mainImage = value;
        }

        private List<string> _additionalImages = new();
        public List<string> AdditionalImages
        {
            get
            {
                if (Images != null && Images.Any())
                {
                    return Images.Where(i => !i.IsMain).OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList();
                }
                return _additionalImages;
            }
            set => _additionalImages = value ?? new();
        }

        // Key Value specs stored cleanly via JSON conversion in SQL Server
        public Dictionary<string, string> Specifications { get; set; } = new();
        
        // Shipping & Physical Attributes
        public decimal WeightKg { get; set; } = 0.5m;
        public string Dimensions { get; set; } = "15 x 10 x 5 cm";
        public string DeliveryEstimate { get; set; } = "2-4 Business Days";
        public string ReturnPolicy { get; set; } = "30-Day Hassle-Free Returns & Money Back Guarantee";
        public string Warranty { get; set; } = "1 Year Official Warranty";
        
        // Product Badges & Flags
        public bool IsFeatured { get; set; }
        public bool IsTrending { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsFlashDeal { get; set; }
        public DateTime? FlashDealEnd { get; set; }

        // Source & Provenance Metadata
        public string? SourceRetailer { get; set; }
        public string? SourceProductUrl { get; set; }
        public string? ImageSourceUrl { get; set; }
        public DateTime? PriceCheckedAt { get; set; }
        public int? StorefrontRank { get; set; }
        
        // Audit Timestamps & Concurrency Token
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Associated Relations
        public List<ProductImage> Images { get; set; } = new();
        public List<ProductVariant> Variants { get; set; } = new();
        public List<InventoryMovement> InventoryMovements { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<QuestionAnswer> Questions { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
