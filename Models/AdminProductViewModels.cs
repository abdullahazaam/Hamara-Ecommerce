using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HamaraCommerce.Models
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product title is required.")]
        [StringLength(250, ErrorMessage = "Title cannot exceed 250 characters.")]
        [Display(Name = "Product Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "SEO URL Slug is required.")]
        [StringLength(250, ErrorMessage = "Slug cannot exceed 250 characters.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens.")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters.")]
        [Display(Name = "SKU / Item Code")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters.")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Selling Price must be greater than $0.00.")]
        [Display(Name = "Selling Price ($)")]
        public decimal Price { get; set; }

        [Range(0.00, 1000000.00, ErrorMessage = "Original Price must be non-negative.")]
        [Display(Name = "Compare-at / Original Price ($)")]
        public decimal OldPrice { get; set; }

        [Range(0.00, 1000000.00, ErrorMessage = "Cost Price must be non-negative.")]
        [Display(Name = "Cost per Item ($)")]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, 100000, ErrorMessage = "Stock must be between 0 and 100,000.")]
        [Display(Name = "Inventory Stock Quantity")]
        public int Stock { get; set; } = 50;

        [Display(Name = "Publishing Status")]
        public ProductStatus Status { get; set; } = ProductStatus.Published;

        [Required(ErrorMessage = "Short summary description is required.")]
        [StringLength(1000, ErrorMessage = "Short description cannot exceed 1000 characters.")]
        [Display(Name = "Short Summary")]
        public string ShortDescription { get; set; } = string.Empty;

        [Display(Name = "Detailed Description")]
        public string FullDescription { get; set; } = string.Empty;

        // Physical Specs
        [Range(0.00, 1000.00, ErrorMessage = "Weight must be positive.")]
        [Display(Name = "Weight (kg)")]
        public decimal WeightKg { get; set; } = 0.5m;

        [StringLength(100)]
        [Display(Name = "Dimensions (L x W x H)")]
        public string Dimensions { get; set; } = "15 x 10 x 5 cm";

        [StringLength(100)]
        [Display(Name = "Estimated Delivery")]
        public string DeliveryEstimate { get; set; } = "2-4 Business Days";

        [StringLength(250)]
        [Display(Name = "Return Policy")]
        public string ReturnPolicy { get; set; } = "30-Day Hassle-Free Returns & Money Back Guarantee";

        [StringLength(100)]
        [Display(Name = "Warranty")]
        public string Warranty { get; set; } = "1 Year Official Warranty";

        // Badges & Promotions
        public bool IsFeatured { get; set; }
        public bool IsTrending { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsFlashDeal { get; set; }
        public DateTime? FlashDealEnd { get; set; }

        // Concurrency token
        public byte[]? RowVersion { get; set; }

        // Images Upload & Existing
        [Display(Name = "Upload Main Image")]
        public IFormFile? MainImageFile { get; set; }
        public string? ExistingMainImage { get; set; }

        [Display(Name = "Upload Additional Gallery Images")]
        public List<IFormFile>? AdditionalImageFiles { get; set; }
        public List<ProductImageViewModel> ExistingImages { get; set; } = new();
        public List<int> DeletedImageIds { get; set; } = new();

        // Variants
        public List<ProductVariantFormModel> Variants { get; set; } = new();

        // Specifications
        public string? SpecificationsJson { get; set; }
        public Dictionary<string, string> Specifications { get; set; } = new();

        // Selection List
        public List<Category> AvailableCategories { get; set; } = new();
    }

    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsMain { get; set; }
    }

    public class ProductVariantFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Variant SKU is required.")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Variant Name is required (e.g. Black / XL).")]
        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Storage { get; set; }

        [Range(-10000.00, 100000.00)]
        public decimal PriceAdjustment { get; set; } = 0;

        [Range(0, 100000)]
        public int Stock { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class ProductCreateViewModel : ProductFormViewModel
    {
        public string? MainImageUrl { get; set; }
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Categories { get; set; } = new();
    }

    public class ProductEditViewModel : ProductFormViewModel
    {
        public string? ExistingMainImageUrl { get; set; }
        public string? MainImageUrl { get; set; }
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Categories { get; set; } = new();
        public new List<ProductImage> ExistingImages { get; set; } = new();
        public List<ProductVariant> ExistingVariants { get; set; } = new();
    }
}
