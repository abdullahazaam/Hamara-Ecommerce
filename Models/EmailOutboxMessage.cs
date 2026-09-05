using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HamaraCommerce.Models
{
    public enum EmailOutboxStatus
    {
        Queued = 0,
        Processing = 1,
        Sent = 2,
        Failed = 3,
        Blocked = 4
    }

    [Table("EmailOutboxMessages")]
    public class EmailOutboxMessage
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(150)]
        public string? EventKey { get; set; }

        [Required]
        [MaxLength(255)]
        public string ToEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string HtmlBody { get; set; } = string.Empty;

        public string? PlainTextBody { get; set; }

        public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Queued;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public DateTime? NextAttemptAt { get; set; }

        public int AttemptCount { get; set; } = 0;

        public int MaxAttempts { get; set; } = 3;

        public string? LastError { get; set; }

        public string? LockToken { get; set; }
        public DateTime? LockExpiresAt { get; set; }

        public string? MetadataJson { get; set; }
    }
}
