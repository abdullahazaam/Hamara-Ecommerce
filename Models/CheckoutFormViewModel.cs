using System.ComponentModel.DataAnnotations;

namespace HamaraCommerce.Models
{
    public class CheckoutFormViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Phone number is required.")]
        [RegularExpression(@"^(\+92|0092|0)?3[0-9]{9}$|^\+?[1-9]\d{7,14}$", 
            ErrorMessage = "Please enter a valid Pakistani mobile number (e.g. 03001234567 or +923001234567) or valid international phone.")]
        [Display(Name = "Mobile Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street Address is required.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Street address must be at least 5 characters.")]
        [Display(Name = "Street Address / House / Suite")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province / State is required.")]
        [StringLength(100)]
        [Display(Name = "Province / Region")]
        public string State { get; set; } = "Punjab";

        [Required(ErrorMessage = "Postal / ZIP Code is required.")]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; } = "Pakistan";

        [Required(ErrorMessage = "Please select a shipping delivery method.")]
        [Display(Name = "Shipping Method")]
        public string ShippingMethod { get; set; } = "Standard";

        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        [StringLength(500, ErrorMessage = "Customer notes cannot exceed 500 characters.")]
        [Display(Name = "Order Notes / Special Delivery Instructions")]
        public string? CustomerNotes { get; set; }

        // Anti-Duplicate Idempotency Token
        [Required]
        public string IdempotencyToken { get; set; } = string.Empty;

        // =========================================================================
        // EPHEMERAL DEVELOPMENT SANDBOX CARD FIELDS (NEVER PERSISTED TO DATABASE OR LOGS)
        // =========================================================================
        public string? SandboxCardholderName { get; set; }
        public string? SandboxCardNumber { get; set; }
        public string? SandboxExpiry { get; set; }
        public string? SandboxCvc { get; set; }
        public bool SimulatePaymentFailure { get; set; } = false;

        // Dynamic Total Revalidation & Transparent Charging
        public decimal? ExpectedGrandTotal { get; set; }
        public string? PriceChangeWarning { get; set; }

        // Computed View Models for Checkout Sidebar
        public ShoppingCartViewModel Cart { get; set; } = new();
        public bool IsDevelopmentSandboxEnabled { get; set; } = false;
    }

    public class CheckoutRecalculateRequest
    {
        public string ShippingMethod { get; set; } = "Standard";
    }
}
