#!/usr/bin/env python3
"""Build a 300-500 item portfolio catalogue from source-paired API records.

Each published row is created only after its own API-provided image downloads,
decodes and passes duplicate-hash validation. Prices are source USD values
converted to PKR for portfolio display; they are not live Pakistani quotes.
"""

from __future__ import annotations

import concurrent.futures
import hashlib
import html
import json
import re
import urllib.request
from datetime import date
from io import BytesIO
from pathlib import Path

from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "wwwroot/images/products"
CATALOG_PATH = ROOT / "Data/Catalog/pakistan-products-2026.json"
PROVENANCE_PATH = ROOT / "Data/Catalog/source-paired-image-audit.csv"
USD_TO_PKR = 280
TARGET_MAKEUP = 150


DUMMY_CATEGORY_MAP = {
    "beauty": "beauty-personal-care", "fragrances": "beauty-personal-care",
    "skin-care": "beauty-personal-care", "furniture": "furniture-decor",
    "groceries": "grocery-beverages", "home-decoration": "home-living",
    "kitchen-accessories": "kitchen-appliances", "laptops": "laptops-computers",
    "tablets": "laptops-computers", "mens-shirts": "mens-fashion",
    "tops": "womens-fashion", "womens-dresses": "womens-fashion",
    "womens-bags": "womens-fashion", "mens-shoes": "shoes-footwear",
    "womens-shoes": "shoes-footwear", "mens-watches": "watches-jewellery",
    "womens-watches": "watches-jewellery", "womens-jewellery": "watches-jewellery",
    "sunglasses": "watches-jewellery", "mobile-accessories": "mobile-accessories",
    "smartphones": "mobile-phones", "sports-accessories": "sports-fitness",
    "motorcycle": "motorcycle-accessories", "vehicle": "car-accessories",
}


def get_json(url: str):
    request = urllib.request.Request(url, headers={"User-Agent": "HamaraCommerce-Portfolio-Catalogue/1.0"})
    with urllib.request.urlopen(request, timeout=90) as response:
        return json.load(response)


def clean_text(value, limit=420):
    text = html.unescape(re.sub(r"<[^>]+>", " ", str(value or "")))
    return re.sub(r"\s+", " ", text).strip()[:limit]


def slugify(value: str):
    return re.sub(r"-+", "-", re.sub(r"[^a-z0-9]+", "-", value.lower())).strip("-")


def pkr(usd):
    value = max(1.0, float(usd)) * USD_TO_PKR
    return int(round(value / 50.0) * 50)


def dummy_candidates():
    payload = get_json("https://dummyjson.com/products?limit=0")
    for item in payload["products"]:
        category = DUMMY_CATEGORY_MAP.get(item.get("category"))
        if not category:
            continue
        image_url = item.get("thumbnail") or (item.get("images") or [None])[0]
        if not image_url:
            continue
        sku = f"PK-DJ-{int(item['id']):04d}"
        yield {
            "Title": clean_text(item.get("title"), 180),
            "Brand": clean_text(item.get("brand") or "Independent", 80),
            "Category": category,
            "Price": pkr(item.get("price") or 1),
            "OldPrice": 0,
            "SKU": sku,
            "Slug": f"{slugify(item.get('title') or sku)}-{item['id']}",
            "SourceRetailer": "DummyJSON portfolio dataset",
            "SourceProductUrl": f"https://dummyjson.com/products/{item['id']}",
            "ImageSourceUrl": image_url,
            "MainImage": f"/images/products/{sku.lower()}.webp",
            "PriceCheckedAt": date.today().isoformat(),
            "ShortDescription": clean_text(item.get("description") or item.get("title")),
            "Stock": max(1, min(80, int(item.get("stock") or 20))),
            "IsFeatured": False,
            "IsFlashDeal": False,
            "_source": "DummyJSON",
        }


def makeup_candidates():
    items = get_json("https://makeup-api.herokuapp.com/api/v1/products.json")
    seen = set()
    for item in items:
        title = clean_text(item.get("name"), 180)
        brand = clean_text(item.get("brand") or "Independent", 80).title()
        image_url = item.get("image_link")
        try:
            source_price = float(item.get("price"))
        except (TypeError, ValueError):
            continue
        key = (brand.lower(), title.lower())
        if not title or not image_url or source_price <= 0 or key in seen:
            continue
        seen.add(key)
        raw_id = re.sub(r"\D", "", str(item.get("id"))) or hashlib.sha1((brand+title).encode()).hexdigest()[:7]
        sku = f"PK-MU-{raw_id}"[:48]
        yield {
            "Title": f"{brand} {title}" if not title.lower().startswith(brand.lower()) else title,
            "Brand": brand,
            "Category": "beauty-personal-care",
            "Price": pkr(source_price),
            "OldPrice": 0,
            "SKU": sku,
            "Slug": f"{slugify(brand+'-'+title)}-{raw_id}",
            "SourceRetailer": "Makeup API catalogue",
            "SourceProductUrl": item.get("product_link") or f"https://makeup-api.herokuapp.com/api/v1/products/{item.get('id')}.json",
            "ImageSourceUrl": image_url.replace("http://", "https://"),
            "MainImage": f"/images/products/{sku.lower()}.webp",
            "PriceCheckedAt": date.today().isoformat(),
            "ShortDescription": clean_text(item.get("description") or f"{brand} {title}"),
            "Stock": 8 + int(hashlib.sha1(sku.encode()).hexdigest()[:2], 16) % 48,
            "IsFeatured": False,
            "IsFlashDeal": False,
            "_source": "Makeup API",
        }


def download(item):
    request = urllib.request.Request(item["ImageSourceUrl"], headers={"User-Agent": "Mozilla/5.0", "Accept": "image/*"})
    with urllib.request.urlopen(request, timeout=30) as response:
        mime = (response.headers.get_content_type() or "").lower()
        raw = response.read(8_000_000)
    if not mime.startswith("image/") or len(raw) < 1200:
        raise ValueError(f"invalid response {mime} ({len(raw)} bytes)")
    with Image.open(BytesIO(raw)) as source:
        source.load()
        if source.width < 100 or source.height < 100:
            raise ValueError(f"image too small: {source.width}x{source.height}")
        oriented = ImageOps.exif_transpose(source)
        if oriented.mode in ("RGBA", "LA", "P"):
            rgba = oriented.convert("RGBA")
            clean = Image.new("RGBA", rgba.size, (255, 255, 255, 255))
            clean.alpha_composite(rgba)
            image = clean.convert("RGB")
        else:
            image = oriented.convert("RGB")
    image.thumbnail((600, 600), Image.Resampling.LANCZOS)
    canvas = Image.new("RGB", (600, 600), "white")
    canvas.paste(image, ((600-image.width)//2, (600-image.height)//2))
    buffer = BytesIO()
    canvas.save(buffer, "WEBP", quality=82, method=6)
    webp = buffer.getvalue()
    return item, webp, hashlib.sha256(webp).hexdigest(), mime


def main():
    # A bounded candidate pool avoids unnecessary requests; failed images are
    # simply skipped and never enter the published catalogue.
    candidates = list(dummy_candidates()) + list(makeup_candidates())[:420]
    accepted, hashes, rows = [], set(), []
    makeup_count = 0
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=12) as pool:
        futures = {pool.submit(download, item): item for item in candidates}
        for future in concurrent.futures.as_completed(futures):
            item = futures[future]
            if item["_source"] == "Makeup API" and makeup_count >= TARGET_MAKEUP:
                continue
            try:
                item, webp, digest, mime = future.result()
                if digest in hashes:
                    continue
                hashes.add(digest)
                if item["_source"] == "Makeup API":
                    makeup_count += 1
                path = ROOT / "wwwroot" / item["MainImage"].lstrip("/")
                path.write_bytes(webp)
                public_item = {k: v for k, v in item.items() if not k.startswith("_")}
                accepted.append(public_item)
                rows.append((item["SKU"], item["Title"], item["Category"], item["_source"], item["ImageSourceUrl"], item["MainImage"], digest, "source-record-paired"))
            except Exception:
                continue

    accepted.sort(key=lambda x: (x["Category"], x["Title"].lower(), x["SKU"]))
    # Curate a small, honest featured set. There are no fabricated discounts.
    for index, item in enumerate(accepted):
        item["IsFeatured"] = index % max(1, len(accepted)//14) == 0 and sum(x["IsFeatured"] for x in accepted[:index]) < 14
    CATALOG_PATH.write_text(json.dumps(accepted, indent=2, ensure_ascii=False), encoding="utf-8")

    import csv
    with PROVENANCE_PATH.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(["SKU", "Title", "Category", "Dataset", "SourceImageUrl", "LocalImage", "SHA256", "PairingStatus"])
        writer.writerows(sorted(rows))

    from collections import Counter
    counts = Counter(x["Category"] for x in accepted)
    print(f"accepted={len(accepted)} unique_images={len(hashes)} dummy={len(accepted)-makeup_count} makeup={makeup_count}")
    for category, count in sorted(counts.items()):
        print(f"{category}: {count}")
    if not 300 <= len(accepted) <= 500:
        raise SystemExit("catalogue did not reach requested 300-500 range")


if __name__ == "__main__":
    main()
