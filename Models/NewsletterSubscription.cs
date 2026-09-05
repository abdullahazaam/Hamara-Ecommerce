using System;
using System.ComponentModel.DataAnnotations;

namespace HamaraCommerce.Models
{
    public class NewsletterSubscription
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? UnsubscribedAt { get; set; }

        [Required]
        [StringLength(100)]
        public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");

        public bool ConsentGiven { get; set; } = true;

        [StringLength(50)]
        public string? IpAddress { get; set; }
    }
}
