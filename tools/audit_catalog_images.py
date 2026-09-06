#!/usr/bin/env python3
"""Audit local WebP assets and emit a reproducible SKU-level CSV."""

import csv
import hashlib
import json
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "Data/Catalog/pakistan-products-2026.json"
OUTPUT = ROOT / "Data/Catalog/product-image-audit.csv"


def main():
    items = json.loads(CATALOG.read_text(encoding="utf-8"))
    rows = []
    hashes = {}
    errors = []
    for item in items:
        sku = item["SKU"]
        path = ROOT / "wwwroot/images/products" / f"{sku.lower().replace(' ', '-').replace('/', '-')}.webp"
        try:
            raw = path.read_bytes()
            digest = hashlib.sha256(raw).hexdigest()
            with Image.open(path) as image:
                image.verify()
            with Image.open(path) as image:
                width, height = image.size
                fmt = image.format
            if fmt != "WEBP" or width < 80 or height < 80 or width > 600 or height > 600:
                raise ValueError(f"unexpected {fmt} {width}x{height}")
            if digest in hashes:
                errors.append(f"duplicate hash: {sku} and {hashes[digest]}")
            hashes[digest] = sku
            kind = "source-record-paired-photograph"
            rows.append([sku, item["Title"], item["Category"], path.relative_to(ROOT).as_posix(), kind, width, height, len(raw), digest, "valid"])
        except Exception as exc:
            rows.append([sku, item["Title"], item["Category"], path.relative_to(ROOT).as_posix(), "unknown", "", "", "", "", f"ERROR: {exc}"])
            errors.append(f"{sku}: {exc}")

    with OUTPUT.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(["SKU", "Title", "Category", "LocalPath", "AssetKind", "Width", "Height", "Bytes", "SHA256", "Validation"])
        writer.writerows(rows)

    print(f"catalogue={len(items)} valid={len(items)-len(errors)} unique_hashes={len(hashes)} errors={len(errors)}")
    if errors:
        for error in errors[:20]:
            print(error)
        raise SystemExit(1)


if __name__ == "__main__":
    main()
