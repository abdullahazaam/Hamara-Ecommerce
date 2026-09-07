using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HamaraCommerce.Data.Catalog
{
    public static class CatalogueSourceGenerator
    {
        private const int UsdToPkr = 280;

        public static decimal UsdToPkrPrice(double usd)
        {
            double val = Math.Max(1.0, usd) * UsdToPkr;
            return (decimal)(Math.Round(val / 50.0) * 50);
        }

        public static string CleanText(string? text, int limit = 420)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string unescaped = System.Net.WebUtility.HtmlDecode(text);
            string noHtml = Regex.Replace(unescaped, "<[^>]+>", " ");
            string clean = Regex.Replace(noHtml, @"\s+", " ").Trim();
            return clean.Length > limit ? clean.Substring(0, limit) : clean;
        }

        public static string Slugify(string text)
        {
            string lower = text.ToLowerInvariant();
            string clean = Regex.Replace(lower, @"[^a-z0-9]+", "-");
            clean = Regex.Replace(clean, @"-+", "-");
            return clean.Trim('-');
        }

        private static string[] ParseCsvLine(string line)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            parts.Add(current.ToString());
            return parts.ToArray();
        }

        public static async Task<List<CatalogueItemDto>> GenerateCanonicalCatalogueAsync(string basePath)
        {
            string auditCsvPath = Path.Combine(basePath, "Data", "Catalog", "source-paired-image-audit.csv");
            if (!File.Exists(auditCsvPath))
            {
                throw new FileNotFoundException("source-paired-image-audit.csv not found at: " + auditCsvPath);
            }

            var auditRows = new Dictionary<string, (string Title, string Category, string Dataset, string SourceImageUrl, string LocalImage)>(StringComparer.OrdinalIgnoreCase);
            var lines = await File.ReadAllLinesAsync(auditCsvPath);
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                var parts = ParseCsvLine(line);
                if (parts.Length >= 6)
                {
                    string sku = parts[0].Trim();
                    string title = parts[1].Trim();
                    string category = parts[2].Trim();
                    string dataset = parts[3].Trim();
                    string sourceImg = parts[4].Trim();
                    string localImg = parts[5].Trim();
                    auditRows[sku] = (title, category, dataset, sourceImg, localImg);
                }
            }

            string djCache = Path.Combine(basePath, "tools", ".cache", "dummyjson.json");
            string muCache = Path.Combine(basePath, "tools", ".cache", "makeup.json");

            string djJson;
            if (File.Exists(djCache))
            {
                djJson = await File.ReadAllTextAsync(djCache);
            }
            else
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HamaraCommerce-Portfolio-Catalogue/1.0");
                djJson = await httpClient.GetStringAsync("https://dummyjson.com/products?limit=0");
            }

            using var djDoc = JsonDocument.Parse(djJson);
            var djDict = new Dictionary<string, JsonElement>();
            foreach (var elem in djDoc.RootElement.GetProperty("products").EnumerateArray())
            {
                int id = elem.GetProperty("id").GetInt32();
                string sku = $"PK-DJ-{id:D4}";
                djDict[sku] = elem.Clone();
            }

            string muJson;
            if (File.Exists(muCache))
            {
                muJson = await File.ReadAllTextAsync(muCache);
            }
            else
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HamaraCommerce-Portfolio-Catalogue/1.0");
                muJson = await httpClient.GetStringAsync("https://makeup-api.herokuapp.com/api/v1/products.json");
            }

            using var muDoc = JsonDocument.Parse(muJson);
            var muDict = new Dictionary<string, JsonElement>();
            foreach (var elem in muDoc.RootElement.EnumerateArray())
            {
                string idStr = elem.GetProperty("id").ToString();
                string rawId = Regex.Replace(idStr, @"\D", "");
                string sku = $"PK-MU-{rawId}";
                if (sku.Length > 48) sku = sku.Substring(0, 48);
                if (!muDict.ContainsKey(sku))
                {
                    muDict[sku] = elem.Clone();
                }
            }

            var items = new List<CatalogueItemDto>();
            using var sha1 = SHA1.Create();

            foreach (var kvp in auditRows)
            {
                string sku = kvp.Key;
                var audit = kvp.Value;

                if (audit.Dataset.Equals("DummyJSON", StringComparison.OrdinalIgnoreCase))
                {
                    if (djDict.TryGetValue(sku, out var p))
                    {
                        int id = p.GetProperty("id").GetInt32();
                        string rawTitle = p.GetProperty("title").GetString() ?? audit.Title;
                        string title = CleanText(rawTitle, 180);
                        string brand = "Independent";
                        if (p.TryGetProperty("brand", out var bProp) && bProp.ValueKind == JsonValueKind.String)
                        {
                            brand = CleanText(bProp.GetString(), 80);
                        }
                        if (string.IsNullOrWhiteSpace(brand)) brand = "Independent";

                        double priceUsd = p.GetProperty("price").GetDouble();
                        decimal price = UsdToPkrPrice(priceUsd);
                        string slug = $"{Slugify(string.IsNullOrEmpty(title) ? sku : title)}-{id}";

                        string desc = title;
                        if (p.TryGetProperty("description", out var dProp) && dProp.ValueKind == JsonValueKind.String)
                        {
                            desc = CleanText(dProp.GetString());
                        }

                        int stock = 20;
                        if (p.TryGetProperty("stock", out var sProp) && sProp.ValueKind == JsonValueKind.Number)
                        {
                            stock = Math.Max(1, Math.Min(80, sProp.GetInt32()));
                        }

                        items.Add(new CatalogueItemDto
                        {
                            Title = title,
                            Brand = brand,
                            Category = audit.Category,
                            Price = price,
                            OldPrice = 0m,
                            SKU = sku,
                            Slug = slug,
                            SourceRetailer = "DummyJSON portfolio dataset",
                            SourceProductUrl = $"https://dummyjson.com/products/{id}",
                            ImageSourceUrl = audit.SourceImageUrl,
                            MainImage = audit.LocalImage,
                            PriceCheckedAt = "2026-09-07",
                            ShortDescription = desc,
                            Stock = stock,
                            IsFeatured = false,
                            IsFlashDeal = false
                        });
                    }
                }
                else if (audit.Dataset.Equals("Makeup API", StringComparison.OrdinalIgnoreCase))
                {
                    if (muDict.TryGetValue(sku, out var p))
                    {
                        string idStr = p.GetProperty("id").ToString();
                        string rawId = Regex.Replace(idStr, @"\D", "");
                        string brandRaw = "Independent";
                        if (p.TryGetProperty("brand", out var bProp) && bProp.ValueKind == JsonValueKind.String)
                        {
                            brandRaw = CleanText(bProp.GetString(), 80);
                        }
                        if (string.IsNullOrWhiteSpace(brandRaw)) brandRaw = "Independent";
                        string brand = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(brandRaw);

                        string name = p.GetProperty("name").GetString() ?? audit.Title;
                        string rawTitle = CleanText(name, 180);
                        string title = rawTitle.StartsWith(brand, StringComparison.OrdinalIgnoreCase) ? rawTitle : $"{brand} {rawTitle}";

                        double priceUsd = 0.0;
                        if (p.TryGetProperty("price", out var prProp) && prProp.ValueKind == JsonValueKind.String)
                        {
                            double.TryParse(prProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out priceUsd);
                        }
                        decimal price = UsdToPkrPrice(priceUsd);

                        string slug = $"{Slugify($"{brand}-{rawTitle}")}-{rawId}";
                        string prodUrl = $"https://makeup-api.herokuapp.com/api/v1/products/{idStr}.json";
                        if (p.TryGetProperty("product_link", out var plProp) && plProp.ValueKind == JsonValueKind.String)
                        {
                            prodUrl = plProp.GetString() ?? prodUrl;
                        }

                        string desc = $"{brand} {rawTitle}";
                        if (p.TryGetProperty("description", out var dProp) && dProp.ValueKind == JsonValueKind.String)
                        {
                            desc = CleanText(dProp.GetString());
                        }

                        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(sku));
                        int stock = 8 + (Convert.ToInt32(Convert.ToHexString(hashBytes).Substring(0, 2), 16) % 48);

                        items.Add(new CatalogueItemDto
                        {
                            Title = title,
                            Brand = brand,
                            Category = "beauty-personal-care",
                            Price = price,
                            OldPrice = 0m,
                            SKU = sku,
                            Slug = slug,
                            SourceRetailer = "Makeup API catalogue",
                            SourceProductUrl = prodUrl,
                            ImageSourceUrl = audit.SourceImageUrl,
                            MainImage = audit.LocalImage,
                            PriceCheckedAt = "2026-09-07",
                            ShortDescription = desc,
                            Stock = stock,
                            IsFeatured = false,
                            IsFlashDeal = false
                        });
                    }
                }
            }

            // Sort exactly like python script: Category, Title.ToLower(), SKU
            items = items.OrderBy(x => x.Category)
                         .ThenBy(x => x.Title.ToLowerInvariant())
                         .ThenBy(x => x.SKU)
                         .ToList();

            // Assign IsFeatured
            int featuredCount = 0;
            int step = Math.Max(1, items.Count / 14);
            for (int i = 0; i < items.Count; i++)
            {
                if (i % step == 0 && featuredCount < 14)
                {
                    items[i].IsFeatured = true;
                    featuredCount++;
                }
            }

            string targetJson = Path.Combine(basePath, "Data", "Catalog", "pakistan-products-2026.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonOut = JsonSerializer.Serialize(items, options);
            await File.WriteAllTextAsync(targetJson, jsonOut, Encoding.UTF8);

            return items;
        }
    }
}
