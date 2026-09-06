# Source-paired catalogue asset report

Generated: 2026-09-07

## Result

- 342 published catalogue records in the JSON import.
- 15 active product categories with deliberately uneven quantities.
- 342 readable local WebP photographs.
- 342 distinct SHA-256 hashes; no published product shares an image file.
- Every record uses the image supplied by that same product API record.
- Broken, undersized, non-image, and duplicate downloads were rejected.
- No published product uses a generated illustration or generic fallback.
- The storefront has no runtime dependency on remote image URLs.

## Category distribution

| Category | Products |
|---|---:|
| Beauty & Personal Care | 163 |
| Kitchen Appliances | 30 |
| Grocery & Beverages | 27 |
| Sports & Fitness | 17 |
| Watches & Jewellery | 17 |
| Mobile Phones | 16 |
| Women's Fashion | 15 |
| Mobile Accessories | 14 |
| Shoes & Footwear | 10 |
| Laptops & Computers | 8 |
| Car Accessories | 5 |
| Furniture & Decor | 5 |
| Home & Living | 5 |
| Men's Fashion | 5 |
| Motorcycle Accessories | 5 |

## Provenance and accuracy boundary

The title and image for each item come from the same source record. This is a
source-pairing guarantee, not a claim that a human independently compared all
342 images with manufacturer catalogues. Sources are DummyJSON's product
dataset and the public Makeup API. `Data/Catalog/source-paired-image-audit.csv`
records the source image URL and local hash for every item.

Prices are source USD figures converted at a fixed portfolio display rate of
PKR 280 per USD and rounded to PKR 50. They are not live Pakistani retailer
quotes. No artificial old price or discount is added.

## Import and verification

```bash
dotnet run -- --import-catalogue
python tools/audit_catalog_images.py
```

The importer archives prior `PK-*` catalogue products that are absent from the
new JSON. It does not delete historical product records. Both legacy CLI aliases
now route to this JSON importer so they cannot regenerate the old 1,040-item
synthetic catalogue.

In Development, `Catalogue:AutoSyncOnStartup` is enabled. On first startup the
application compares the JSON count with the published source-paired rows and
automatically imports when the old database is still active. Production never
auto-imports.
