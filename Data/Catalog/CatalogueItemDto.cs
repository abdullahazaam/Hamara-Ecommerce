namespace HamaraCommerce.Data.Catalog
{
    public class CatalogueItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SourceRetailer { get; set; } = string.Empty;
        public string SourceProductUrl { get; set; } = string.Empty;
        public string ImageSourceUrl { get; set; } = string.Empty;
        public string MainImage { get; set; } = string.Empty;
        public string PriceCheckedAt { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public int Stock { get; set; } = 40;
        public bool IsFeatured { get; set; } = false;
        public bool IsFlashDeal { get; set; } = false;
        public int? StorefrontRank { get; set; }
    }
}
