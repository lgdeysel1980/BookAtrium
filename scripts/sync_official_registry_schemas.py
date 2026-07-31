#!/usr/bin/env python3
"""Regenerate official registry JSON Schemas from BookAtrium.PluginContracts."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from official_registry_contract import (
    _build_contract_from_sources,
    build_index_schema,
    build_manifest_schema,
    load_contract,
)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--root",
        type=Path,
        default=None,
        help="Repository root (default: parent of scripts/)",
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="Exit 1 if generated schemas differ from files on disk",
    )
    args = parser.parse_args(argv)

    root = (args.root or Path(__file__).resolve().parents[1]).resolve()
    load_contract.cache_clear()
    contracts_dir = root / "BookAtrium.PluginContracts"
    if contracts_dir.is_dir():
        contract = _build_contract_from_sources(root)
    else:
        contract = load_contract(root)

    schemas_dir = root / "registries" / "schemas"
    schemas_dir.mkdir(parents=True, exist_ok=True)

    targets = {
        schemas_dir / "official-contract.json": contract,
        schemas_dir / "official-index.schema.json": build_index_schema(contract),
        schemas_dir
        / "official-plugin-manifest.schema.json": build_manifest_schema(contract),
    }

    dirty = False
    for path, payload in targets.items():
        text = json.dumps(payload, indent=2) + "\n"
        if args.check:
            if not path.is_file() or path.read_text(encoding="utf-8") != text:
                print(f"schema drift: {path.relative_to(root).as_posix()}", file=sys.stderr)
                dirty = True
            continue
        path.write_text(text, encoding="utf-8", newline="\n")
        print(f"wrote {path.relative_to(root).as_posix()}")

    # Remove obsolete published-catalogue schema if present.
    obsolete = schemas_dir / "official-plugin.schema.json"
    if obsolete.is_file() and not args.check:
        obsolete.unlink()
        print(f"removed {obsolete.relative_to(root).as_posix()}")

    if args.check and dirty:
        return 1
    if args.check:
        print("Official registry schemas are up to date.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
