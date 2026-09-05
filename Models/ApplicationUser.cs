using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace HamaraCommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? AvatarUrl { get; set; }
        
        // Persisted shopping cart for logged-in users
        public string? CartJson { get; set; }

        public List<Address> SavedAddresses { get; set; } = new();
        public List<int> WishlistProductIds { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
    }
}
