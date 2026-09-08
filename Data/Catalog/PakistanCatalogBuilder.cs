using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HamaraCommerce.Models;

namespace HamaraCommerce.Data.Catalog
{
    public static class PakistanCatalogBuilder
    {
        public static string ResolveJsonFilePath()
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "Catalog", "pakistan-products-2026.json"),
                Path.Combine(AppContext.BaseDirectory, "Data", "Catalog", "pakistan-products-2026.json"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "Catalog", "pakistan-products-2026.json")
            };

            foreach (var path in candidates)
            {
                var full = Path.GetFullPath(path);
                if (File.Exists(full))
                {
                    return full;
                }
            }

            return Path.GetFullPath(candidates[0]);
        }

        public static List<CatalogueItemDto> GetAllProducts()
        {
            string path = ResolveJsonFilePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var items = JsonSerializer.Deserialize<List<CatalogueItemDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null && items.Count == 163)
                {
                    return items;
                }
            }

            // Fallback to project root generator if file missing or corrupt
            string baseDir = Directory.GetCurrentDirectory();
            return CatalogueSourceGenerator.GenerateCanonicalCatalogueAsync(baseDir).GetAwaiter().GetResult();
        }

        public static List<Category> GetOfficialCategories()
        {
            return MarketplaceTaxonomy.GetOfficialCategories();
        }

        public static string EnsureJsonFileGenerated(string? outputPath = null)
        {
            var target = outputPath ?? ResolveJsonFilePath();
            var dir = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(target) || new FileInfo(target).Length < 1000)
            {
                string baseDir = Directory.GetCurrentDirectory();
                CatalogueSourceGenerator.GenerateCanonicalCatalogueAsync(baseDir).GetAwaiter().GetResult();
            }

            return target;
        }
    }
}
