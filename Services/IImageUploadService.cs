using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HamaraCommerce.Services
{
    public interface IImageUploadService
    {
        Task<(bool Success, string? ImageUrl, string? ErrorMessage)> UploadProductImageAsync(IFormFile file, string subFolder = "products");
        Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files, string subFolder = "products");
        bool DeleteImageFile(string? imageUrl);
    }
}
