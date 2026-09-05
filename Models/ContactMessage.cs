using System;
using System.ComponentModel.DataAnnotations;

namespace HamaraCommerce.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Subject must be between 3 and 150 characters.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(2500, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2500 characters.")]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string? AdminNotes { get; set; }
    }
}
