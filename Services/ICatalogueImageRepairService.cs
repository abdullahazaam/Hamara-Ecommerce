using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HamaraCommerce.Services
{
    public class ImageRepairReport
    {
        public int TotalProcessed { get; set; }
        public int RepairedCount { get; set; }
        public int ArchivedCount { get; set; }
        public int DuplicateHashesCount { get; set; }
        public List<UnrepairedProductRecord> UnrepairedProducts { get; set; } = new();
    }

    public class UnrepairedProductRecord
    {
        public string SKU { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public interface ICatalogueImageRepairService
    {
        Task<ImageRepairReport> RepairImagesAndSignalsAsync(CancellationToken cancellationToken = default);
    }
}
