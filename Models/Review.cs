using System;

namespace HamaraCommerce.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public string? AuthorEmail { get; set; }
        public string UserName { get; set; } = "Verified Customer";
        public string UserAvatar { get; set; } = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=120&q=80";

        public double Rating { get; set; } = 5.0;
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int HelpfulCount { get; set; } = 0;
        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int? OrderId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class QuestionAnswer
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string AskedBy { get; set; } = string.Empty;
        public DateTime QuestionDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false;
        public bool IsAnswered { get; set; } = false;
        public string? Answer { get; set; }
        public string? AnsweredBy { get; set; }
        public DateTime? AnswerDate { get; set; }
    }
}
