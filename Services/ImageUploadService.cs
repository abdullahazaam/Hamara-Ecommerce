using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HamaraCommerce.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageUploadService> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public ImageUploadService(IWebHostEnvironment env, ILogger<ImageUploadService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<(bool Success, string? ImageUrl, string? ErrorMessage)> UploadProductImageAsync(IFormFile file, string subFolder = "products")
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, "No image file provided.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, null, "Image exceeds maximum allowed size of 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return (false, null, $"Invalid file extension '{extension}'. Allowed extensions: {string.Join(", ", AllowedExtensions)}.");
            }

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, null, $"Invalid file content type '{file.ContentType}'.");
            }

            // Verify Magic Bytes header to prevent malicious disguised files
            using (var stream = file.OpenReadStream())
            {
                byte[] header = new byte[8];
                int read = await stream.ReadAsync(header, 0, 8);
                if (read < 4 || !IsValidImageHeader(header, extension))
                {
                    return (false, null, "File content is corrupted or not a valid image format.");
                }
            }

            try
            {
                // Generate secure collision-free filename
                var safeFileName = $"{Guid.NewGuid():N}{extension}";
                
                // Sanitize folder name
                var sanitizedSubFolder = Path.GetFileName(subFolder);
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", sanitizedSubFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fullPath = Path.Combine(uploadsFolder, safeFileName);
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var relativeUrl = $"/uploads/{sanitizedSubFolder}/{safeFileName}";
                _logger.LogInformation("Product image uploaded successfully to {Path}", relativeUrl);

                return (true, relativeUrl, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save uploaded image file.");
                return (false, null, "An error occurred while saving the image to disk.");
            }
        }

        public async Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files, string subFolder = "products")
        {
            var results = new List<string>();
            if (files == null) return results;

            foreach (var file in files)
            {
                var (success, url, _) = await UploadProductImageAsync(file, subFolder);
                if (success && !string.IsNullOrEmpty(url))
                {
                    results.Add(url);
                }
            }

            return results;
        }

        public bool DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/"))
            {
                return false; // Do not delete external URLs or non-uploads
            }

            try
            {
                // Prevent directory traversal
                var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted local image file: {Path}", fullPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete image file: {Url}", imageUrl);
            }

            return false;
        }

        private static bool IsValidImageHeader(byte[] header, string extension)
        {
            // JPEG: FF D8 FF
            if ((extension == ".jpg" || extension == ".jpeg") &&
                header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47
            if (extension == ".png" &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return true;

            // GIF: 47 49 46 38
            if (extension == ".gif" &&
                header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                return true;

            // WEBP: 52 49 46 46 (RIFF)
            if (extension == ".webp" &&
                header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
                return true;

            return false;
        }
    }
}
