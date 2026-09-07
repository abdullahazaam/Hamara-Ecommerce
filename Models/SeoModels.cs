using System.Collections.Generic;

namespace HamaraCommerce.Models
{
    public class PageSeoMetadata
    {
        public string Title { get; set; } = "Hamara Commerce | Online Shopping in Pakistan";
        public string Description { get; set; } = "Shop electronics, fashion, lifestyle, and home goods with nationwide cash on delivery and easy returns.";
        public string Keywords { get; set; } = "ecommerce pakistan, online shopping lahore, karachi electronics, buy online pakistan, cash on delivery";
        public string? CanonicalUrl { get; set; }
        public string Robots { get; set; } = "index, follow";

        // Open Graph
        public string OgType { get; set; } = "website";
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImage { get; set; }
        public string? OgUrl { get; set; }
        public string OgSiteName { get; set; } = "Hamara Commerce";

        // Twitter Card
        public string TwitterCard { get; set; } = "summary_large_image";
        public string? TwitterTitle { get; set; }
        public string? TwitterDescription { get; set; }
        public string? TwitterImage { get; set; }

        // JSON-LD Structured Data
        public List<string> JsonLdScripts { get; set; } = new();
    }

    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
