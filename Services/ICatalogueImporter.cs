using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HamaraCommerce.Services
{
    public class CatalogueImportReport
    {
        public int TotalProcessed { get; set; }
        public int TotalInserted { get; set; }
        public int TotalUpdated { get; set; }
        public int TotalArchived { get; set; }
        public int TotalDeleted { get; set; }
        public int PublishedProductCount { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
        public Dictionary<string, (decimal Min, decimal Max)> CategoryPriceRanges { get; set; } = new();
        public Dictionary<string, int> SourceCounts { get; set; } = new();
        public int DuplicateSkus { get; set; }
        public int DuplicateSlugs { get; set; }
        public int MissingImages { get; set; }
        public int MissingSourceUrls { get; set; }
        public int InvalidPrices { get; set; }
        public int LegacyPublishedProducts { get; set; }
        public int FakeSeededReviews { get; set; }
    }

    public interface ICatalogueImporter
    {
        Task<int> CleanLegacyDemoDataAsync(CancellationToken cancellationToken = default);
        Task SyncCategoriesAsync(CancellationToken cancellationToken = default);
        Task<CatalogueImportReport> ImportCatalogueAsync(string? jsonFilePath = null, CancellationToken cancellationToken = default);
        Task<CatalogueImportReport> GenerateVerificationReportAsync(CancellationToken cancellationToken = default);
    }
}
