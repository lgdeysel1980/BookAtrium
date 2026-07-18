#!/usr/bin/env python3
"""Explicit remote checksum audit for official plugin packages.

Downloads each official catalogue package to a temporary file, verifies size and
SHA-256 against catalogue metadata, then deletes the temporary file.

Does not execute plugin packages.

Usage:
  python scripts/verify_official_package_remote.py
  python scripts/verify_official_package_remote.py --id com.practicore.bookatrium.store.amazon-us-kindle

Exit codes: 0 success, 1 mismatch/failure, 2 usage/environment error.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import sys
import tempfile
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REGISTRY_PATH = ROOT / "registries" / "official-plugins.json"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--id", help="Validate a single plugin id")
    args = parser.parse_args()

    try:
        registry = json.loads(REGISTRY_PATH.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        print(f"error: failed to read registry: {exc}", file=sys.stderr)
        return 2

    plugins = registry.get("plugins") or []
    if args.id:
        plugins = [p for p in plugins if p.get("id") == args.id]
        if not plugins:
            print(f"error: plugin id not found: {args.id}", file=sys.stderr)
            return 2

    failures = 0
    for entry in plugins:
        plugin_id = entry.get("id")
        package = entry.get("package") or {}
        url = package.get("downloadUrl")
        expected_sha = (package.get("sha256") or "").lower()
        expected_size = package.get("sizeBytes")
        file_name = package.get("fileName")

        print(f"Auditing {plugin_id} …")
        print(f"  URL: {url}")

        try:
            with urllib.request.urlopen(url, timeout=60) as response:
                data = response.read()
        except Exception as exc:  # noqa: BLE001 — report any network failure
            print(f"error: download failed for {plugin_id}: {exc}", file=sys.stderr)
            failures += 1
            continue

        with tempfile.NamedTemporaryFile(prefix="ba-official-", suffix=".bookplugin", delete=True) as tmp:
            tmp.write(data)
            tmp.flush()
            digest = hashlib.sha256(data).hexdigest()

        ok = True
        if len(data) != expected_size:
            print(
                f"error: size mismatch for {plugin_id}: "
                f"expected {expected_size}, got {len(data)}",
                file=sys.stderr,
            )
            ok = False
        if digest != expected_sha:
            print(
                f"error: sha256 mismatch for {plugin_id}: "
                f"expected {expected_sha}, got {digest}",
                file=sys.stderr,
            )
            ok = False
        if not url.endswith("/" + str(file_name)):
            print(
                f"error: download URL does not end with fileName {file_name}",
                file=sys.stderr,
            )
            ok = False

        if ok:
            print(f"  OK size={len(data)} sha256={digest}")
        else:
            failures += 1

    if failures:
        print(f"Remote package audit failed ({failures} plugin(s)).", file=sys.stderr)
        return 1

    print("Remote package audit OK.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
