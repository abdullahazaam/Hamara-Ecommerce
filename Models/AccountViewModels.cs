using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HamaraCommerce.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResendEmailConfirmationViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Avatar Image URL")]
        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int RewardPoints { get; set; }
        public int WishlistCount { get; set; }

        public List<Address> SavedAddresses { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
    }

    public class AddressViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Address title/label is required.")]
        [StringLength(50)]
        [Display(Name = "Address Label (e.g. Home, Office)")]
        public string Title { get; set; } = "Home";

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100)]
        [Display(Name = "Recipient Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street Address is required.")]
        [StringLength(250)]
        [Display(Name = "Street Address / House")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province / State is required.")]
        [StringLength(100)]
        [Display(Name = "Province / Region")]
        public string State { get; set; } = "Punjab";

        [Required(ErrorMessage = "Postal Code is required.")]
        [StringLength(20)]
        [Display(Name = "Postal / ZIP Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; } = "Pakistan";

        [Display(Name = "Set as Default Shipping Address")]
        public bool IsDefault { get; set; } = false;
    }

    public class CustomerOrderListViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 8;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class CustomerWishlistViewModel
    {
        public List<Product> Products { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 8;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class CancelOrderViewModel
    {
        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a reason for cancellation.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Cancellation reason must be at least 5 characters.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReturnOrderViewModel
    {
        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe the reason for return/refund.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Return reason must be at least 5 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
