#!/usr/bin/env python3
"""Validate the official BookAtrium plugin registry and per-plugin catalogue metadata.

Validates:
  - registries/official-plugins.json structure and uniqueness
  - per-plugin plugin.json identity, versions, package URLs, and SHA-256
  - registry ↔ plugin metadata consistency
  - required public documents (README, CHANGELOG, LICENSE, PRIVACY, SECURITY)
  - prohibition of /releases/latest download URLs
  - official publisher must be BookAtrium
  - rejection of development-style versions in the published registry

Does not download or execute plugin packages. Use verify_official_package_remote.py
for an explicit remote checksum audit.

Exit codes: 0 success, 1 validation failure, 2 usage/environment error.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
REGISTRY_PATH = ROOT / "registries" / "official-plugins.json"
PLUGIN_SCHEMA_PATH = ROOT / "registries" / "schemas" / "official-plugin.schema.json"
INDEX_SCHEMA_PATH = ROOT / "registries" / "schemas" / "official-index.schema.json"

PLUGIN_TYPES = {
    "ConversionInput",
    "ConversionOutput",
    "DeviceInterface",
    "MetadataReader",
    "MetadataSource",
    "MetadataWriter",
    "Store",
}
CATEGORIES = {
    "ConversionInput",
    "ConversionOutput",
    "Store",
    "MetadataReader",
    "MetadataWriter",
    "MetadataSource",
    "Importer",
    "Exporter",
    "Device",
    "Reader",
    "Utility",
}
CAPABILITIES = {
    "NetworkAccess",
    "PluginSettingsStorage",
    "TemporaryFileAccess",
    "ReadBookMetadata",
    "WriteBookMetadata",
    "ReadInputFormat",
    "ProduceOutputFormat",
    "DetectDevice",
    "TransferToDevice",
    "StoreSearch",
    "CoverDownload",
    "MetadataLookup",
}
PLATFORMS = {"windows-x64", "windows-x86", "windows-arm64", "windows", "any"}
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
ID_RE = re.compile(r"^[a-z0-9]([a-z0-9.\-]{0,126}[a-z0-9])?$")
SEMVER_RE = re.compile(r"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$")
RELEASE_URL_RE = re.compile(
    r"^https://github\.com/[A-Za-z0-9](?:[A-Za-z0-9\-]*[A-Za-z0-9])?/"
    r"[A-Za-z0-9._\-]+/releases/download/[^/]+/[^/?#]+$"
)
LATEST_URL_RE = re.compile(r"/releases/latest(?:/|$)", re.IGNORECASE)
DEV_VERSION_RE = re.compile(
    r"(?:^0\.\d+\.\d+$)|(?:-(?:dev|alpha|beta|rc|preview|snapshot)(?:\.|$))",
    re.IGNORECASE,
)
REQUIRED_DOCS = ("README.md", "CHANGELOG.md", "LICENSE", "PRIVACY.md", "SECURITY.md")
CATEGORY_DIR = {
    "ConversionInput": "conversion-input",
    "ConversionOutput": "conversion-output",
    "Store": "stores",
    "MetadataReader": "metadata-readers",
    "MetadataWriter": "metadata-writers",
    "MetadataSource": "metadata-sources",
    "Importer": "importers",
    "Exporter": "exporters",
    "Device": "devices",
    "Reader": "readers",
    "Utility": "utilities",
}


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


def is_dev_version(version: str) -> bool:
    if version.startswith("0."):
        return True
    return bool(DEV_VERSION_RE.search(version))


def validate_package(prefix: str, package: dict[str, Any], errors: list[str]) -> None:
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
        if isinstance(file_name, str) and file_name and not download_url.endswith("/" + file_name):
            errors.append(
                f"{prefix}: package.downloadUrl file name must match package.fileName"
            )

    if not isinstance(file_name, str) or not file_name.endswith(".bookplugin"):
        errors.append(f"{prefix}: package.fileName must end with .bookplugin")

    if not isinstance(size_bytes, int) or size_bytes < 1:
        errors.append(f"{prefix}: package.sizeBytes must be a positive integer")

    if not isinstance(sha256, str) or not SHA256_RE.match(sha256):
        errors.append(f"{prefix}: package.sha256 must be a 64-character hex digest")


def validate_plugin_entry(path: Path, data: dict[str, Any], errors: list[str]) -> None:
    rel = path.relative_to(ROOT).as_posix()
    prefix = rel

    if data.get("schemaVersion") != 1:
        errors.append(f"{prefix}: schemaVersion must be 1")

    plugin_id = data.get("id")
    if not isinstance(plugin_id, str) or not ID_RE.match(plugin_id):
        errors.append(f"{prefix}: invalid plugin id")

    if data.get("official") is not True:
        errors.append(f"{prefix}: official must be true")
    if data.get("ownership") != "first-party":
        errors.append(f"{prefix}: ownership must be 'first-party'")

    publisher = data.get("publisher")
    if not isinstance(publisher, dict):
        errors.append(f"{prefix}: publisher must be an object")
    else:
        if publisher.get("name") != "BookAtrium":
            errors.append(
                f"{prefix}: official plugins must have publisher.name 'BookAtrium' "
                f"(got {publisher.get('name')!r})"
            )
        if publisher.get("verified") is not True:
            errors.append(f"{prefix}: publisher.verified must be true for official plugins")

    category = data.get("category")
    if category not in CATEGORIES:
        errors.append(f"{prefix}: invalid category {category!r}")

    plugin_type = data.get("pluginType")
    if plugin_type not in PLUGIN_TYPES:
        errors.append(f"{prefix}: invalid pluginType {plugin_type!r}")

    version = data.get("version")
    if not isinstance(version, str) or not SEMVER_RE.match(version):
        errors.append(f"{prefix}: version must be a semantic version")
    elif is_dev_version(version):
        errors.append(
            f"{prefix}: development versions are not allowed in the published official "
            f"registry (got {version})"
        )

    if data.get("pluginApiVersion") != "2.0":
        errors.append(f"{prefix}: pluginApiVersion must be '2.0'")

    min_app = data.get("minimumAppVersion")
    if not isinstance(min_app, str) or not SEMVER_RE.match(min_app):
        errors.append(f"{prefix}: minimumAppVersion must be a semantic version")

    platforms = data.get("supportedPlatforms")
    if not isinstance(platforms, list) or not platforms:
        errors.append(f"{prefix}: supportedPlatforms must be a non-empty array")
    else:
        for platform in platforms:
            if platform not in PLATFORMS:
                errors.append(f"{prefix}: unsupported platform {platform!r}")

    capabilities = data.get("capabilities")
    if not isinstance(capabilities, list) or not capabilities:
        errors.append(f"{prefix}: capabilities must be a non-empty array")
    else:
        for capability in capabilities:
            if capability not in CAPABILITIES:
                errors.append(f"{prefix}: unknown capability {capability!r}")

    network_hosts = data.get("networkHosts")
    if not isinstance(network_hosts, list):
        errors.append(f"{prefix}: networkHosts must be an array")

    for key in ("privacyPath", "securityPath", "licensePath"):
        value = data.get(key)
        if not isinstance(value, str) or not value:
            errors.append(f"{prefix}: {key} is required")
            continue
        doc_path = ROOT / value
        if not doc_path.is_file():
            errors.append(f"{prefix}: {key} does not exist: {value}")

    validate_package(prefix, data.get("package") or {}, errors)

    download_url = (data.get("package") or {}).get("downloadUrl")
    if isinstance(download_url, str) and LATEST_URL_RE.search(download_url):
        errors.append(f"{prefix}: forbidden latest-download URL")

    # Required sibling documents
    plugin_dir = path.parent
    for doc in REQUIRED_DOCS:
        if not (plugin_dir / doc).is_file():
            errors.append(f"{prefix}: missing required document {doc}")

    # Category/path consistency
    if isinstance(category, str) and category in CATEGORY_DIR:
        expected_parent = CATEGORY_DIR[category]
        parts = path.relative_to(ROOT / "plugins" / "official").parts
        if not parts or parts[0] != expected_parent:
            errors.append(
                f"{prefix}: metadata path category folder must be "
                f"plugins/official/{expected_parent}/... for category {category}"
            )


def validate_registry(registry: dict[str, Any], errors: list[str]) -> list[Path]:
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

    # Deterministic ordering: sort by id
    ids = [p.get("id") for p in plugins if isinstance(p, dict)]
    if ids != sorted(x for x in ids if isinstance(x, str)):
        errors.append(
            "registries/official-plugins.json: plugins must be ordered by id ascending"
        )

    seen_ids: set[str] = set()
    seen_paths: set[str] = set()
    seen_category_paths: set[str] = set()
    metadata_paths: list[Path] = []

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

        if entry.get("publisher") != "BookAtrium":
            errors.append(f"{prefix}: publisher must be 'BookAtrium'")
        if entry.get("official") is not True:
            errors.append(f"{prefix}: official must be true")
        if entry.get("ownership") != "first-party":
            errors.append(f"{prefix}: ownership must be 'first-party'")

        category = entry.get("category")
        if category not in CATEGORIES:
            errors.append(f"{prefix}: invalid category")

        version = entry.get("version")
        if not isinstance(version, str) or not SEMVER_RE.match(version):
            errors.append(f"{prefix}: invalid version")
        elif is_dev_version(version):
            errors.append(f"{prefix}: development version not allowed ({version})")

        if entry.get("pluginApiVersion") != "2.0":
            errors.append(f"{prefix}: pluginApiVersion must be '2.0'")

        metadata_path = entry.get("metadataPath")
        if not isinstance(metadata_path, str) or not metadata_path.startswith(
            "plugins/official/"
        ):
            errors.append(f"{prefix}: metadataPath must be under plugins/official/")
            continue

        if metadata_path in seen_paths:
            errors.append(f"{prefix}: duplicate metadataPath {metadata_path}")
        else:
            seen_paths.add(metadata_path)

        category_path_key = f"{category}:{metadata_path}"
        if category_path_key in seen_category_paths:
            errors.append(f"{prefix}: duplicate category/path combination")
        else:
            seen_category_paths.add(category_path_key)

        abs_meta = ROOT / metadata_path
        if not abs_meta.is_file():
            errors.append(f"{prefix}: metadataPath does not exist: {metadata_path}")
        else:
            metadata_paths.append(abs_meta)

        validate_package(prefix, entry.get("package") or {}, errors)

    return metadata_paths


def validate_consistency(
    registry_entry: dict[str, Any], plugin: dict[str, Any], errors: list[str]
) -> None:
    plugin_id = registry_entry.get("id")
    prefix = f"consistency:{plugin_id}"

    checks = [
        ("id", registry_entry.get("id"), plugin.get("id")),
        ("name", registry_entry.get("name"), plugin.get("name")),
        ("version", registry_entry.get("version"), plugin.get("version")),
        ("category", registry_entry.get("category"), plugin.get("category")),
        (
            "pluginApiVersion",
            registry_entry.get("pluginApiVersion"),
            plugin.get("pluginApiVersion"),
        ),
        (
            "releaseRepository",
            registry_entry.get("releaseRepository"),
            plugin.get("releaseRepository"),
        ),
        ("releaseTag", registry_entry.get("releaseTag"), plugin.get("releaseTag")),
        (
            "packageHosting",
            registry_entry.get("packageHosting"),
            plugin.get("packageHosting"),
        ),
    ]
    for field, left, right in checks:
        if left is not None and right is not None and left != right:
            errors.append(f"{prefix}: registry.{field} ({left!r}) != plugin.{field} ({right!r})")

    reg_pkg = registry_entry.get("package") or {}
    plug_pkg = plugin.get("package") or {}
    for field in ("downloadUrl", "fileName", "sizeBytes", "sha256"):
        if reg_pkg.get(field) != plug_pkg.get(field):
            errors.append(
                f"{prefix}: registry.package.{field} differs from plugin.package.{field}"
            )

    publisher_name = (plugin.get("publisher") or {}).get("name")
    if registry_entry.get("publisher") != publisher_name:
        errors.append(
            f"{prefix}: registry.publisher ({registry_entry.get('publisher')!r}) "
            f"!= plugin.publisher.name ({publisher_name!r})"
        )


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


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--root",
        type=Path,
        default=None,
        help="Repository root (default: parent of scripts/)",
    )
    args = parser.parse_args()

    root = (args.root or Path(__file__).resolve().parents[1]).resolve()
    registry_path = root / "registries" / "official-plugins.json"
    plugin_schema_path = root / "registries" / "schemas" / "official-plugin.schema.json"
    index_schema_path = root / "registries" / "schemas" / "official-index.schema.json"

    # Bind module-level ROOT used by helpers for relative paths.
    global ROOT
    ROOT = root

    errors: list[str] = []

    if not registry_path.is_file():
        fail(f"missing registry: {registry_path}")
        return 2

    try:
        registry = load_json(registry_path)
    except (OSError, json.JSONDecodeError):
        return 2

    if not isinstance(registry, dict):
        fail("registry root must be an object")
        return 1

    try_jsonschema(registry, index_schema_path, "official-index", errors)
    metadata_paths = validate_registry(registry, errors)

    registry_by_path = {
        entry["metadataPath"]: entry
        for entry in registry.get("plugins", [])
        if isinstance(entry, dict) and isinstance(entry.get("metadataPath"), str)
    }

    for meta_path in metadata_paths:
        try:
            plugin = load_json(meta_path)
        except (OSError, json.JSONDecodeError):
            errors.append(f"failed to load {meta_path.relative_to(root).as_posix()}")
            continue
        if not isinstance(plugin, dict):
            errors.append(f"{meta_path.relative_to(root).as_posix()}: root must be an object")
            continue
        try_jsonschema(plugin, plugin_schema_path, meta_path.name, errors)
        validate_plugin_entry(meta_path, plugin, errors)
        reg_entry = registry_by_path.get(meta_path.relative_to(root).as_posix())
        if reg_entry:
            validate_consistency(reg_entry, plugin, errors)

    # Ensure every plugins/official/**/plugin.json is registered
    official_root = root / "plugins" / "official"
    if official_root.is_dir():
        for orphan in sorted(official_root.rglob("plugin.json")):
            rel = orphan.relative_to(root).as_posix()
            if rel not in registry_by_path:
                errors.append(f"unregistered official plugin metadata: {rel}")

    if errors:
        for message in errors:
            fail(message)
        print(f"Official registry validation failed with {len(errors)} error(s).", file=sys.stderr)
        return 1

    print(f"Official registry OK ({len(metadata_paths)} plugin(s)).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
