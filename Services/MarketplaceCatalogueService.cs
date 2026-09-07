using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HamaraCommerce.Services
{
    public class MarketplaceCatalogueService
    {
        private readonly ICatalogueImporter _importer;
        private readonly ILogger<MarketplaceCatalogueService> _logger;

        public MarketplaceCatalogueService(
            ICatalogueImporter importer,
            ILogger<MarketplaceCatalogueService> logger)
        {
            _importer = importer;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Delegating marketplace catalogue transformation to canonical 342-product catalogue importer...");
            var report = await _importer.ImportCatalogueAsync(null, cancellationToken);
            _logger.LogInformation("Canonical catalogue sync completed: {Published} published, {Archived} archived across {Categories} categories.",
                report.PublishedProductCount, report.TotalArchived, report.CategoryCounts.Count);
        }
    }
}
