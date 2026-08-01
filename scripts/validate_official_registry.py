#!/usr/bin/env python3
"""Validate the official BookAtrium plugin registry against source manifests.

Canonical contract:
  - registries/official-plugins.json is the repository / packaging registry
  - metadataPath points at source manifests:
      Plugins/Official/<CategoryDir>/<PluginDir>/src/<Project>/plugin.json
  - Categories and plugin API versions are derived from BookAtrium.PluginContracts
  - Structural rules live in JSON Schema; this script adds filesystem checks

Exit codes: 0 success, 1 validation failure, 2 usage/environment error.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from official_registry_contract import (
    ID_RE,
    LATEST_URL_RE,
    LEGACY_PUBLISHED_METADATA_RE,
    METADATA_PATH_PREFIX,
    METADATA_PATH_RE,
    RELEASE_URL_RE,
    SEMVER_RE,
    SHA256_RE,
    canonicalize_official_metadata_path,
    discover_official_metadata_files,
    is_canonical_metadata_path,
    is_dev_version,
    is_excluded_metadata_path,
    load_contract,
)

ROOT = Path(__file__).resolve().parents[1]
REGISTRY_PATH = ROOT / "registries" / "official-plugins.json"
INDEX_SCHEMA_PATH = ROOT / "registries" / "schemas" / "official-index.schema.json"
MANIFEST_SCHEMA_PATH = (
    ROOT / "registries" / "schemas" / "official-plugin-manifest.schema.json"
)


def fail(msg: str) -> None:
    print(f"error: {msg}", file=sys.stderr)


def load_json(path: Path) -> Any:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        fail(f"missing file: {path.relative_to(ROOT).as_posix()}")
        raise
    except json.JSONDecodeError as exc:
        fail(f"invalid JSON in {path.relative_to(ROOT).as_posix()}: {exc}")
        raise


def try_jsonschema(data: Any, schema_path: Path, label: str, errors: list[str]) -> None:
    try:
        import jsonschema  # type: ignore
    except ImportError:
        return

    schema = load_json(schema_path)
    validator = jsonschema.Draft202012Validator(schema)
    for error in sorted(validator.iter_errors(data), key=lambda e: list(e.path)):
        path = ".".join(str(p) for p in error.path) or "(root)"
        errors.append(f"{label} schema: {path}: {error.message}")


def validate_package(prefix: str, package: Any, errors: list[str]) -> None:
    if not isinstance(package, dict):
        errors.append(f"{prefix}: package must be an object")
        return

    download_url = package.get("downloadUrl")
    file_name = package.get("fileName")
    size_bytes = package.get("sizeBytes")
    sha256 = package.get("sha256")

    if not isinstance(download_url, str) or not download_url:
        errors.append(f"{prefix}: package.downloadUrl is required")
    else:
        if LATEST_URL_RE.search(download_url):
            errors.append(
                f"{prefix}: package.downloadUrl must not use /releases/latest "
                f"(got {download_url})"
            )
        elif not RELEASE_URL_RE.match(download_url):
            errors.append(
                f"{prefix}: package.downloadUrl must be an immutable version-specific "
                f"GitHub release asset URL (got {download_url})"
            )
        if isinstance(file_name, str) and file_name and not download_url.endswith(
            "/" + file_name
        ):
            errors.append(
                f"{prefix}: package.downloadUrl file name must match package.fileName"
            )

    if not isinstance(file_name, str) or not file_name.endswith(".bookplugin"):
        errors.append(f"{prefix}: package.fileName must end with .bookplugin")

    if not isinstance(size_bytes, int) or size_bytes < 1:
        errors.append(f"{prefix}: package.sizeBytes must be a positive integer")

    if not isinstance(sha256, str) or not SHA256_RE.match(sha256):
        errors.append(f"{prefix}: package.sha256 must be a 64-character hex digest")


def validate_registry(
    registry: dict[str, Any],
    contract: dict[str, Any],
    errors: list[str],
) -> list[str]:
    """Validate registry FS/uniqueness rules. Returns metadataPath values."""
    if registry.get("schemaVersion") != 1:
        errors.append("registries/official-plugins.json: schemaVersion must be 1")
    if registry.get("trustSource") != "official-bookatrium":
        errors.append(
            "registries/official-plugins.json: trustSource must be 'official-bookatrium'"
        )

    plugins = registry.get("plugins")
    if not isinstance(plugins, list):
        errors.append("registries/official-plugins.json: plugins must be an array")
        return []

    # Deterministic ordering: category, then id (matches OfficialPluginRepositoryBuilder).
    order_keys = [
        (p.get("category"), p.get("id"))
        for p in plugins
        if isinstance(p, dict)
    ]
    expected = sorted(
        [(c, i) for c, i in order_keys if isinstance(c, str) and isinstance(i, str)],
        key=lambda item: (item[0], item[1]),
    )
    actual = [(c, i) for c, i in order_keys if isinstance(c, str) and isinstance(i, str)]
    if actual != expected:
        errors.append(
            "registries/official-plugins.json: plugins must be ordered by "
            "category ascending, then id ascending"
        )

    categories = set(contract["categories"])
    api_versions = set(contract["pluginApiVersions"])
    seen_ids: set[str] = set()
    seen_paths: set[str] = set()
    metadata_paths: list[str] = []

    for index, entry in enumerate(plugins):
        prefix = f"registries/official-plugins.json[{index}]"
        if not isinstance(entry, dict):
            errors.append(f"{prefix}: entry must be an object")
            continue

        plugin_id = entry.get("id")
        if not isinstance(plugin_id, str) or not ID_RE.match(plugin_id):
            errors.append(f"{prefix}: invalid id")
        elif plugin_id in seen_ids:
            errors.append(f"{prefix}: duplicate plugin id {plugin_id}")
        else:
            seen_ids.add(plugin_id)

        publisher = entry.get("publisher")
        if not isinstance(publisher, str) or not publisher.strip():
            errors.append(f"{prefix}: publisher is required")
        if entry.get("official") is not True:
            errors.append(f"{prefix}: official must be true")
        if entry.get("ownership") != "first-party":
            errors.append(f"{prefix}: ownership must be 'first-party'")

        category = entry.get("category")
        if category not in categories:
            errors.append(f"{prefix}: invalid category {category!r}")

        version = entry.get("version")
        if not isinstance(version, str) or not SEMVER_RE.match(version):
            errors.append(f"{prefix}: invalid version")
        elif is_dev_version(version):
            errors.append(f"{prefix}: development version not allowed ({version})")

        api = entry.get("pluginApiVersion")
        if api not in api_versions:
            errors.append(
                f"{prefix}: pluginApiVersion must be one of "
                f"{sorted(api_versions)} (got {api!r})"
            )

        metadata_path = entry.get("metadataPath")
        if not isinstance(metadata_path, str):
            errors.append(f"{prefix}: metadataPath is required")
            continue

        if metadata_path != canonicalize_official_metadata_path(metadata_path):
            errors.append(
                f"{prefix}: metadataPath must use canonical casing "
                f"{METADATA_PATH_PREFIX!r} (got {metadata_path!r})"
            )

        if not is_canonical_metadata_path(metadata_path):
            errors.append(
                f"{prefix}: metadataPath must match "
                f"{METADATA_PATH_PREFIX}<Category>/<Plugin>/src/<Project>/plugin.json"
            )
            continue

        if ".." in metadata_path.split("/"):
            errors.append(f"{prefix}: metadataPath must not contain '..'")
            continue

        if metadata_path in seen_paths:
            errors.append(f"{prefix}: duplicate metadataPath {metadata_path}")
        else:
            seen_paths.add(metadata_path)

        abs_meta = ROOT / metadata_path
        if not abs_meta.is_file():
            errors.append(f"{prefix}: metadataPath does not exist: {metadata_path}")
        else:
            metadata_paths.append(metadata_path)

        validate_package(prefix, entry.get("package"), errors)

    return metadata_paths


def validate_manifest_consistency(
    registry_entry: dict[str, Any],
    manifest: dict[str, Any],
    metadata_path: str,
    contract: dict[str, Any],
    errors: list[str],
) -> None:
    plugin_id = registry_entry.get("id")
    prefix = f"consistency:{plugin_id}"

    checks = [
        ("id", registry_entry.get("id"), manifest.get("id")),
        ("name", registry_entry.get("name"), manifest.get("name")),
        ("version", registry_entry.get("version"), manifest.get("version")),
        (
            "pluginApiVersion",
            registry_entry.get("pluginApiVersion"),
            manifest.get("pluginApiVersion") or manifest.get("contractApiVersion"),
        ),
    ]
    for field, left, right in checks:
        if left is not None and right is not None and left != right:
            errors.append(
                f"{prefix}: registry.{field} ({left!r}) != manifest.{field} ({right!r})"
            )

    # The desktop installer rejects a package whose manifest publisher differs from the catalogue
    # entry's, so a divergence here is not cosmetic: it makes the plugin impossible to install.
    # Mirror the host's comparison, which is case-insensitive and falls back to the manifest author.
    registry_publisher = registry_entry.get("publisher")
    manifest_publisher = manifest.get("publisher") or manifest.get("author")
    if (
        isinstance(registry_publisher, str)
        and isinstance(manifest_publisher, str)
        and registry_publisher.strip().casefold() != manifest_publisher.strip().casefold()
    ):
        errors.append(
            f"{prefix}: registry.publisher ({registry_publisher!r}) != "
            f"manifest.publisher ({manifest_publisher!r})"
        )

    category = registry_entry.get("category")
    plugin_type = manifest.get("pluginType")
    if category != plugin_type:
        errors.append(
            f"{prefix}: registry.category ({category!r}) != manifest.pluginType ({plugin_type!r})"
        )

    if plugin_type not in contract["pluginTypes"]:
        errors.append(f"{prefix}: unsupported manifest.pluginType {plugin_type!r}")

    # Path traversal / unsafe relative path already gated by canonical pattern.
    if not METADATA_PATH_RE.match(metadata_path):
        errors.append(f"{prefix}: non-canonical metadataPath {metadata_path!r}")


def reject_legacy_published_layout(repo_root: Path, errors: list[str]) -> None:
    """Fail if the obsolete kebab published layout is still present."""
    official_lower = repo_root / "plugins" / "official"
    # On case-insensitive filesystems this is the same tree as Plugins/Official.
    # Only flag leaf paths that match the old published pattern and are not
    # canonical source-tree paths.
    if not official_lower.is_dir():
        return

    for path in sorted(official_lower.rglob("plugin.json")):
        rel = path.relative_to(repo_root).as_posix()
        canonical = canonicalize_official_metadata_path(rel)
        if LEGACY_PUBLISHED_METADATA_RE.match(rel.replace("\\", "/")) or (
            LEGACY_PUBLISHED_METADATA_RE.match(canonical.lower())
            and not is_canonical_metadata_path(canonical)
        ):
            # Exact old layout: plugins/official/<kebab-category>/<kebab-plugin>/plugin.json
            parts = rel.replace("\\", "/").split("/")
            if (
                len(parts) == 5
                and parts[0].lower() == "plugins"
                and parts[1].lower() == "official"
                and parts[4] == "plugin.json"
                and "/src/" not in rel.replace("\\", "/")
            ):
                errors.append(
                    "obsolete published official metadata layout must be removed "
                    f"(use {METADATA_PATH_PREFIX}.../src/.../plugin.json): {rel}"
                )


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--root",
        type=Path,
        default=None,
        help="Repository root (default: parent of scripts/)",
    )
    args = parser.parse_args(argv)

    root = (args.root or Path(__file__).resolve().parents[1]).resolve()
    registry_path = root / "registries" / "official-plugins.json"
    index_schema_path = root / "registries" / "schemas" / "official-index.schema.json"
    manifest_schema_path = (
        root / "registries" / "schemas" / "official-plugin-manifest.schema.json"
    )

    global ROOT
    ROOT = root

    errors: list[str] = []

    if not registry_path.is_file():
        fail(f"missing registry: {registry_path}")
        return 2

    try:
        contract = load_contract(root)
        registry = load_json(registry_path)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        fail(str(exc))
        return 2

    if not isinstance(registry, dict):
        fail("registry root must be an object")
        return 1

    if not index_schema_path.is_file() or not manifest_schema_path.is_file():
        fail(
            "missing schemas; run: python scripts/sync_official_registry_schemas.py"
        )
        return 2

    try_jsonschema(registry, index_schema_path, "official-index", errors)
    metadata_paths = validate_registry(registry, contract, errors)

    registry_by_path = {
        entry["metadataPath"]: entry
        for entry in registry.get("plugins", [])
        if isinstance(entry, dict) and isinstance(entry.get("metadataPath"), str)
    }

    for metadata_path in metadata_paths:
        abs_meta = root / metadata_path
        try:
            manifest = load_json(abs_meta)
        except (OSError, json.JSONDecodeError):
            errors.append(f"failed to load {metadata_path}")
            continue
        if not isinstance(manifest, dict):
            errors.append(f"{metadata_path}: root must be an object")
            continue
        try_jsonschema(manifest, manifest_schema_path, metadata_path, errors)
        reg_entry = registry_by_path.get(metadata_path)
        if reg_entry:
            validate_manifest_consistency(
                reg_entry, manifest, metadata_path, contract, errors
            )

    # Every canonical source manifest must be registered (except inventory exclusions).
    excluded_ids_path = root / "registries" / "official-plugin-inventory.json"
    excluded_ids: set[str] = set()
    # Soft-read ExcludedPluginIds from inventory is not stored; hardcode sync with C#.
    # Smashwords remains the only inventory exclusion today.
    excluded_ids.add("com.practicore.bookatrium.store.smashwords")

    discovered = discover_official_metadata_files(root)
    for rel in discovered:
        if rel in registry_by_path:
            continue
        # Allow intentionally excluded packages to remain on disk unregistered.
        try:
            manifest = load_json(root / rel)
            plugin_id = manifest.get("id") if isinstance(manifest, dict) else None
        except (OSError, json.JSONDecodeError):
            plugin_id = None
        if isinstance(plugin_id, str) and plugin_id in excluded_ids:
            continue
        errors.append(f"unregistered official plugin metadata: {rel}")

    for metadata_path in registry_by_path:
        if metadata_path not in discovered and (root / metadata_path).is_file():
            # Registered but discovery skipped it (shouldn't happen for canonical paths).
            if is_excluded_metadata_path(metadata_path):
                errors.append(
                    f"registry metadataPath points at excluded build artifact: {metadata_path}"
                )

    reject_legacy_published_layout(root, errors)

    # Amazon US Kindle Store must be present exactly once under the canonical path
    # whenever that official plugin exists in the repository tree.
    amazon_id = "com.practicore.bookatrium.store.amazon-us-kindle"
    amazon_source = (
        root
        / "Plugins"
        / "Official"
        / "Stores"
        / "AmazonUSKindleStore"
        / "src"
        / "BookAtrium.Plugins.AmazonUSKindleStore"
        / "plugin.json"
    )
    amazon_entries = [
        e
        for e in registry.get("plugins", [])
        if isinstance(e, dict) and e.get("id") == amazon_id
    ]
    if amazon_source.is_file() or amazon_entries:
        if len(amazon_entries) != 1:
            errors.append(
                f"Amazon US Kindle Store must appear exactly once in the registry "
                f"(found {len(amazon_entries)})"
            )
        elif not is_canonical_metadata_path(str(amazon_entries[0].get("metadataPath"))):
            errors.append(
                "Amazon US Kindle Store metadataPath must use the canonical "
                f"{METADATA_PATH_PREFIX} source-tree layout"
            )

    if errors:
        for message in errors:
            fail(message)
        print(
            f"Official registry validation failed with {len(errors)} error(s).",
            file=sys.stderr,
        )
        return 1

    print(f"Official registry OK ({len(metadata_paths)} plugin(s)).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
