#!/usr/bin/env python3
"""Canonical official-registry contract derived from BookAtrium.PluginContracts.

Parses C# sources so categories, platforms, capabilities, and plugin API versions
cannot drift from the host contracts without an intentional code change.
"""

from __future__ import annotations

import json
import re
from functools import lru_cache
from pathlib import Path
from typing import Any

METADATA_PATH_PREFIX = "Plugins/Official/"
# Source-tree layout used by OfficialPluginInventory / OfficialPluginRepositoryBuilder.
METADATA_PATH_PATTERN = (
    r"^Plugins/Official/[A-Za-z0-9._\-]+/[A-Za-z0-9._\-]+/src/[A-Za-z0-9._\-]+/plugin\.json$"
)
METADATA_PATH_RE = re.compile(METADATA_PATH_PATTERN)

EXCLUDED_METADATA_SEGMENTS = (
    "/bin/",
    "/obj/",
    "/packaging/tmp/",
    "/artifacts/",
    "/bundle-publish/",
)

# Legacy kebab published layout that must not coexist with the source-tree contract.
LEGACY_PUBLISHED_METADATA_RE = re.compile(
    r"^plugins/official/[a-z0-9\-]+/[a-z0-9\-]+/plugin\.json$"
)

ID_RE = re.compile(r"^[a-z0-9]([a-z0-9.\-]{0,126}[a-z0-9])?$")
SEMVER_RE = re.compile(r"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$")
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
RELEASE_URL_RE = re.compile(
    r"^https://github\.com/[A-Za-z0-9](?:[A-Za-z0-9\-]*[A-Za-z0-9])?/"
    r"[A-Za-z0-9._\-]+/releases/download/[^/]+/[^/?#]+$"
)
LATEST_URL_RE = re.compile(r"/releases/latest(?:/|$)", re.IGNORECASE)
DEV_VERSION_RE = re.compile(
    r"(?:^0\.\d+\.\d+$)|(?:-(?:dev|alpha|beta|rc|preview|snapshot)(?:\.|$))",
    re.IGNORECASE,
)


def _repo_root_from(path: Path) -> Path:
    path = path.resolve()
    if path.name == "scripts":
        return path.parent
    return path


def _contracts_dir(repo_root: Path) -> Path:
    return repo_root / "BookAtrium.PluginContracts"


def _parse_csharp_enum_names(source: str, enum_name: str) -> list[str]:
    match = re.search(
        rf"\benum\s+{re.escape(enum_name)}\s*\{{(.*?)\}}",
        source,
        flags=re.DOTALL,
    )
    if not match:
        raise ValueError(f"enum {enum_name} not found")

    names: list[str] = []
    for line in match.group(1).splitlines():
        stripped = line.split("//", 1)[0].strip().rstrip(",")
        if not stripped or stripped.startswith("#"):
            continue
        token = re.match(r"^([A-Za-z_][A-Za-z0-9_]*)\s*(?:=|$)", stripped)
        if not token:
            continue
        name = token.group(1)
        if name == "None":
            continue
        names.append(name)
    if not names:
        raise ValueError(f"enum {enum_name} produced no names")
    return names


def _parse_const_string(source: str, const_name: str) -> str:
    match = re.search(
        rf'public\s+const\s+string\s+{re.escape(const_name)}\s*=\s*"([^"]+)"\s*;',
        source,
    )
    if not match:
        raise ValueError(f"const string {const_name} not found")
    return match.group(1)


def _parse_public_string_constants(source: str) -> list[str]:
    values = re.findall(
        r'public\s+const\s+string\s+[A-Za-z_][A-Za-z0-9_]*\s*=\s*"([^"]+)"\s*;',
        source,
    )
    if not values:
        raise ValueError("no public const string values found")
    # Preserve order, unique.
    seen: set[str] = set()
    ordered: list[str] = []
    for value in values:
        if value not in seen:
            seen.add(value)
            ordered.append(value)
    return ordered


# Official public publication requires plugin API 2.0 for all catalogue entries.
# Host ContractApiVersion.Current may be newer and remains compatible with 2.0 packages.
OFFICIAL_PLUGIN_API_VERSIONS = ["2.0"]


def _build_contract_from_sources(root: Path) -> dict[str, Any]:
    contracts = _contracts_dir(root)
    plugin_type_src = (contracts / "PluginType.cs").read_text(encoding="utf-8")
    capabilities_src = (contracts / "PluginCapabilities.cs").read_text(encoding="utf-8")
    metadata_caps_src = (contracts / "MetadataSourceCapabilities.cs").read_text(
        encoding="utf-8"
    )
    platform_src = (contracts / "PluginPlatform.cs").read_text(encoding="utf-8")
    api_src = (contracts / "ContractApiVersion.cs").read_text(encoding="utf-8")

    categories = _parse_csharp_enum_names(plugin_type_src, "PluginType")
    plugin_capabilities = _parse_csharp_enum_names(capabilities_src, "PluginCapabilities")
    metadata_capabilities = _parse_csharp_enum_names(
        metadata_caps_src, "MetadataSourceCapabilities"
    )
    capabilities: list[str] = []
    seen: set[str] = set()
    for name in [*plugin_capabilities, *metadata_capabilities]:
        if name not in seen:
            seen.add(name)
            capabilities.append(name)

    platforms = _parse_public_string_constants(platform_src)
    current_api = _parse_const_string(api_src, "Current")

    return {
        "categories": categories,
        "pluginTypes": categories,  # category == PluginType name for official plugins
        "capabilities": capabilities,
        "platforms": platforms,
        "pluginApiVersions": list(OFFICIAL_PLUGIN_API_VERSIONS),
        "currentPluginApiVersion": current_api,
        "officialPluginApiVersion": OFFICIAL_PLUGIN_API_VERSIONS[0],
        "metadataPathPrefix": METADATA_PATH_PREFIX,
        "metadataPathPattern": METADATA_PATH_PATTERN,
    }


@lru_cache(maxsize=4)
def load_contract(repo_root: str | Path) -> dict[str, Any]:
    root = Path(repo_root).resolve()
    baked = root / "registries" / "schemas" / "official-contract.json"
    if baked.is_file():
        data = json.loads(baked.read_text(encoding="utf-8"))
        # Ensure publication pin cannot drift if an older bake slips through.
        data["pluginApiVersions"] = list(OFFICIAL_PLUGIN_API_VERSIONS)
        data["officialPluginApiVersion"] = OFFICIAL_PLUGIN_API_VERSIONS[0]
        data["metadataPathPrefix"] = METADATA_PATH_PREFIX
        data["metadataPathPattern"] = METADATA_PATH_PATTERN
        return data

    contracts_dir = _contracts_dir(root)
    if not contracts_dir.is_dir():
        raise FileNotFoundError(
            "Missing registries/schemas/official-contract.json and "
            "BookAtrium.PluginContracts/; cannot load official registry contract."
        )
    return _build_contract_from_sources(root)


def is_excluded_metadata_path(relative_posix: str) -> bool:
    normalized = "/" + relative_posix.replace("\\", "/").lstrip("/").lower() + "/"
    # Check segment presence with surrounding slashes.
    path = relative_posix.replace("\\", "/")
    lower = f"/{path.lower()}"
    return any(seg in lower for seg in EXCLUDED_METADATA_SEGMENTS)


def canonicalize_official_metadata_path(relative_posix: str) -> str:
    """Force Plugins/Official/ prefix casing while preserving the remainder."""
    path = relative_posix.replace("\\", "/")
    lower = path.lower()
    prefix = "plugins/official/"
    if lower.startswith(prefix):
        return METADATA_PATH_PREFIX + path[len(prefix) :]
    return path


def is_canonical_metadata_path(relative_posix: str) -> bool:
    return bool(METADATA_PATH_RE.match(relative_posix.replace("\\", "/")))


def is_dev_version(version: str) -> bool:
    if version.startswith("0."):
        return True
    return bool(DEV_VERSION_RE.search(version))


def discover_official_metadata_files(repo_root: Path) -> list[str]:
    """Return canonical repo-relative metadata paths under Plugins/Official."""
    official_root = repo_root / "Plugins" / "Official"
    if not official_root.is_dir():
        return []

    found: list[str] = []
    for path in sorted(official_root.rglob("plugin.json")):
        rel = canonicalize_official_metadata_path(
            path.relative_to(repo_root).as_posix()
        )
        if is_excluded_metadata_path(rel):
            continue
        if not is_canonical_metadata_path(rel):
            # Non-src copies (docs fixtures, packaging leftovers) are ignored for
            # registration only when excluded; otherwise reported by callers.
            continue
        found.append(rel)
    return found


def build_index_schema(contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "$id": "https://github.com/lgdeysel1980/BookAtrium/registries/schemas/official-index.schema.json",
        "title": "Official BookAtrium Plugin Registry Index",
        "description": (
            "Index of official BookAtrium first-party plugins. "
            "metadataPath points at source manifests under Plugins/Official/.../src/.../plugin.json."
        ),
        "type": "object",
        "additionalProperties": False,
        "required": ["schemaVersion", "minimumClientVersion", "trustSource", "plugins"],
        "properties": {
            "schemaVersion": {"const": 1},
            "generatedAtUtc": {"type": ["string", "null"], "format": "date-time"},
            "minimumClientVersion": {"type": "string", "minLength": 1},
            "trustSource": {"const": "official-bookatrium"},
            "description": {"type": ["string", "null"], "maxLength": 1000},
            "plugins": {
                "type": "array",
                "maxItems": 500,
                "items": build_registry_entry_schema(contract),
            },
        },
    }


def build_registry_entry_schema(contract: dict[str, Any]) -> dict[str, Any]:
    return {
        "type": "object",
        "additionalProperties": False,
        "required": [
            "id",
            "name",
            "publisher",
            "official",
            "ownership",
            "category",
            "version",
            "pluginApiVersion",
            "metadataPath",
            "package",
        ],
        "properties": {
            "id": {
                "type": "string",
                "pattern": "^[a-z0-9]([a-z0-9.\\-]{0,126}[a-z0-9])?$",
            },
            "name": {"type": "string", "minLength": 1, "maxLength": 200},
            # The publisher is whatever the plugin's own manifest declares. The installer cross-checks
            # the catalogue entry against the packaged manifest, so pinning this to a product name that
            # no package declares made every official install fail as a registry/manifest mismatch.
            "publisher": {"type": "string", "minLength": 1, "maxLength": 200},
            "official": {"const": True},
            "ownership": {"const": "first-party"},
            "category": {"type": "string", "enum": contract["categories"]},
            "version": {
                "type": "string",
                "pattern": "^\\d+\\.\\d+\\.\\d+(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$",
            },
            "pluginApiVersion": {
                "type": "string",
                "enum": contract["pluginApiVersions"],
            },
            "summary": {"type": ["string", "null"], "maxLength": 500},
            "description": {"type": ["string", "null"], "maxLength": 4000},
            "license": {"type": ["string", "null"], "maxLength": 128},
            "minimumAppVersion": {
                "type": ["string", "null"],
                "pattern": "^\\d+\\.\\d+\\.\\d+(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$",
            },
            "maximumAppVersion": {"type": ["string", "null"]},
            "supportedPlatforms": {
                "type": "array",
                "maxItems": 16,
                "items": {"type": "string", "enum": contract["platforms"]},
            },
            "capabilities": {
                "type": "array",
                "maxItems": 64,
                "items": {"type": "string", "enum": contract["capabilities"]},
            },
            "networkHosts": {
                "type": "array",
                "maxItems": 32,
                "items": {"type": "string", "minLength": 1, "maxLength": 253},
            },
            "networkDisclosure": {"type": ["string", "null"], "maxLength": 4000},
            "defaultEnabled": {"type": ["boolean", "null"]},
            "desktopSupport": {"type": "boolean"},
            "embeddedWebSupport": {"type": "boolean"},
            "standaloneWebSupport": {"type": "boolean"},
            "configurationRequired": {"type": "boolean"},
            "secretsRequired": {"type": "boolean"},
            "requiresRestart": {"type": "boolean"},
            "legacyBuiltInId": {"type": ["string", "null"], "maxLength": 128},
            "homepageUrl": {"type": ["string", "null"]},
            "documentationUrl": {"type": ["string", "null"]},
            "supportUrl": {"type": ["string", "null"]},
            "releaseNotes": {"type": ["string", "null"]},
            "deprecated": {"type": "boolean"},
            "withdrawn": {"type": "boolean"},
            "metadataPath": {
                "type": "string",
                "pattern": contract["metadataPathPattern"],
            },
            "releaseRepository": {
                "type": "string",
                "pattern": "^[A-Za-z0-9]([A-Za-z0-9\\-]*[A-Za-z0-9])?/[A-Za-z0-9._\\-]+$",
            },
            "releaseTag": {"type": "string", "minLength": 1, "maxLength": 128},
            "packageHosting": {
                "type": "string",
                "enum": ["bookatrium", "standalone-repository-transitional"],
            },
            "package": {
                "type": "object",
                "additionalProperties": False,
                "required": ["downloadUrl", "fileName", "sizeBytes", "sha256"],
                "properties": {
                    "downloadUrl": {
                        "type": "string",
                        "pattern": (
                            "^https://github\\.com/[A-Za-z0-9]([A-Za-z0-9\\-]*[A-Za-z0-9])?/"
                            "[A-Za-z0-9._\\-]+/releases/download/[^/]+/[^/?#]+$"
                        ),
                    },
                    "fileName": {
                        "type": "string",
                        "pattern": "^.+\\.bookplugin$",
                    },
                    "sizeBytes": {
                        "type": "integer",
                        "minimum": 1,
                        "maximum": 41943040,
                    },
                    "sha256": {"type": "string", "pattern": "^[0-9a-fA-F]{64}$"},
                },
            },
        },
    }


def build_manifest_schema(contract: dict[str, Any]) -> dict[str, Any]:
    """Schema for source-tree plugin.json package manifests (not published catalogue rows)."""
    return {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "$id": "https://github.com/lgdeysel1980/BookAtrium/registries/schemas/official-plugin-manifest.schema.json",
        "title": "Official BookAtrium Plugin Package Manifest",
        "description": (
            "Source package manifest under Plugins/Official/.../src/.../plugin.json. "
            "Distinct from the registry index entry in registries/official-plugins.json."
        ),
        "type": "object",
        "additionalProperties": False,
        "required": [
            "manifestVersion",
            "id",
            "name",
            "version",
            "pluginType",
            "pluginApiVersion",
            "entryAssembly",
            "entryType",
            "capabilities",
            "license",
        ],
        "properties": {
            "manifestVersion": {"const": 1},
            "id": {
                "type": "string",
                "pattern": "^[a-z0-9]([a-z0-9.\\-]{0,126}[a-z0-9])?$",
            },
            "name": {"type": "string", "minLength": 1, "maxLength": 200},
            "description": {"type": "string", "maxLength": 4000},
            "version": {
                "type": "string",
                "pattern": "^\\d+\\.\\d+\\.\\d+(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$",
            },
            "author": {"type": ["string", "null"], "maxLength": 200},
            "publisher": {"type": ["string", "null"], "maxLength": 200},
            "homepage": {"type": ["string", "null"]},
            "supportUrl": {"type": ["string", "null"]},
            "license": {"type": "string", "minLength": 1, "maxLength": 128},
            "pluginType": {"type": "string", "enum": contract["pluginTypes"]},
            "pluginApiVersion": {
                "type": "string",
                "enum": contract["pluginApiVersions"],
            },
            "contractApiVersion": {
                "anyOf": [
                    {
                        "type": "string",
                        "enum": [*contract["pluginApiVersions"], "1.0", "1.1"],
                    },
                    {"type": "null"},
                ]
            },
            "minimumAppVersion": {
                "type": "string",
                "pattern": "^\\d+\\.\\d+\\.\\d+(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$",
            },
            "maximumAppVersion": {"type": ["string", "null"]},
            "targetFramework": {"type": ["string", "null"]},
            "supportedPlatforms": {
                "type": "array",
                "maxItems": 16,
                "items": {"type": "string", "enum": contract["platforms"]},
            },
            "entryAssembly": {
                "type": "string",
                "minLength": 1,
                "maxLength": 260,
                "pattern": r"^(?!.*\.\.)(?!/).+\.dll$",
            },
            "entryType": {"type": "string", "minLength": 1, "maxLength": 512},
            "capabilities": {
                "type": "array",
                "minItems": 1,
                "maxItems": 64,
                "items": {"type": "string", "enum": contract["capabilities"]},
            },
            "requiresRestart": {"type": "boolean"},
            "configurable": {"type": ["boolean", "null"]},
            "settingsSchema": {"type": ["array", "null"]},
            "requiredSecrets": {
                "type": "array",
                "items": {"type": "string"},
            },
            "defaultEnabled": {"type": "boolean"},
            "networkHosts": {
                "type": "array",
                "maxItems": 32,
                "items": {"type": "string", "minLength": 1, "maxLength": 253},
            },
            "contentTypes": {
                "type": "array",
                "items": {"type": "string"},
            },
            "icon": {"type": ["string", "null"]},
            "releaseNotes": {"type": ["string", "null"]},
        },
    }
