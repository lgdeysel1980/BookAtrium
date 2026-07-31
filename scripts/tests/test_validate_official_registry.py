#!/usr/bin/env python3
"""Unit tests for official registry validation (schema + filesystem contract)."""

from __future__ import annotations

import json
import shutil
import sys
from pathlib import Path

import pytest

SCRIPTS = Path(__file__).resolve().parents[1]
ROOT = SCRIPTS.parent
if str(SCRIPTS) not in sys.path:
    sys.path.insert(0, str(SCRIPTS))

from official_registry_contract import (  # noqa: E402
    build_index_schema,
    build_manifest_schema,
    is_canonical_metadata_path,
    load_contract,
)
from validate_official_registry import main as validate_main  # noqa: E402

try:
    import jsonschema
except ImportError:  # pragma: no cover
    jsonschema = None


pytestmark = pytest.mark.skipif(jsonschema is None, reason="jsonschema required")


CANONICAL_META = (
    "Plugins/Official/Stores/DemoStore/src/BookAtrium.Plugins.DemoStore/plugin.json"
)


def _minimal_package() -> dict:
    return {
        "downloadUrl": (
            "https://github.com/lgdeysel1980/BookAtrium/releases/download/"
            "official-plugins-baseline-v1.0.0/com.example.plugin-1.0.0.bookplugin"
        ),
        "fileName": "com.example.plugin-1.0.0.bookplugin",
        "sizeBytes": 4096,
        "sha256": "a" * 64,
    }


def _registry_entry(**overrides) -> dict:
    entry = {
        "id": "com.practicore.bookatrium.store.demo",
        "name": "Demo Store",
        "publisher": "BookAtrium",
        "official": True,
        "ownership": "first-party",
        "category": "Store",
        "version": "1.0.0",
        "pluginApiVersion": "2.0",
        "summary": "Demo",
        "description": "Demo",
        "license": "MIT",
        "minimumAppVersion": "1.0.0",
        "supportedPlatforms": ["windows", "any"],
        "capabilities": ["StoreSearch"],
        "networkHosts": [],
        "networkDisclosure": None,
        "defaultEnabled": False,
        "desktopSupport": True,
        "embeddedWebSupport": False,
        "standaloneWebSupport": False,
        "configurationRequired": False,
        "secretsRequired": False,
        "requiresRestart": False,
        "deprecated": False,
        "withdrawn": False,
        "metadataPath": CANONICAL_META,
        "releaseRepository": "lgdeysel1980/BookAtrium",
        "releaseTag": "official-plugins-baseline-v1.0.0",
        "packageHosting": "bookatrium",
        "package": _minimal_package(),
    }
    entry.update(overrides)
    return entry


def _index(plugins: list[dict]) -> dict:
    ordered = sorted(plugins, key=lambda p: (p["category"], p["id"]))
    return {
        "schemaVersion": 1,
        "generatedAtUtc": "2026-07-31T00:00:00+00:00",
        "minimumClientVersion": "1.0.0",
        "trustSource": "official-bookatrium",
        "description": "test",
        "plugins": ordered,
    }


def _write_manifest(path: Path, **overrides) -> None:
    manifest = {
        "manifestVersion": 1,
        "id": "com.practicore.bookatrium.store.demo",
        "name": "Demo Store",
        "description": "Demo",
        "version": "1.0.0",
        "publisher": "BookAtrium",
        "license": "MIT",
        "pluginType": "Store",
        "pluginApiVersion": "2.0",
        "minimumAppVersion": "1.0.0",
        "targetFramework": "net10.0",
        "supportedPlatforms": ["windows", "any"],
        "entryAssembly": "BookAtrium.Plugins.DemoStore.dll",
        "entryType": "BookAtrium.Plugins.DemoStore.Plugin",
        "capabilities": ["StoreSearch"],
        "networkHosts": [],
        "requiresRestart": False,
        "defaultEnabled": False,
    }
    manifest.update(overrides)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


@pytest.fixture
def contract():
    return load_contract(ROOT)


@pytest.fixture
def sandbox(tmp_path: Path):
    """Minimal repo tree with contracts + schemas + one official plugin."""
    # Contracts needed by load_contract
    for name in (
        "PluginType.cs",
        "PluginCapabilities.cs",
        "MetadataSourceCapabilities.cs",
        "PluginPlatform.cs",
        "ContractApiVersion.cs",
    ):
        src = ROOT / "BookAtrium.PluginContracts" / name
        dest = tmp_path / "BookAtrium.PluginContracts" / name
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dest)

    schemas_dir = tmp_path / "registries" / "schemas"
    schemas_dir.mkdir(parents=True, exist_ok=True)
    contract = load_contract(tmp_path)
    (schemas_dir / "official-index.schema.json").write_text(
        json.dumps(build_index_schema(contract), indent=2) + "\n", encoding="utf-8"
    )
    (schemas_dir / "official-plugin-manifest.schema.json").write_text(
        json.dumps(build_manifest_schema(contract), indent=2) + "\n", encoding="utf-8"
    )

    meta = tmp_path.joinpath(*CANONICAL_META.split("/"))
    _write_manifest(meta)
    return tmp_path


def _write_registry(root: Path, plugins: list[dict]) -> None:
    path = root / "registries" / "official-plugins.json"
    path.write_text(json.dumps(_index(plugins), indent=2) + "\n", encoding="utf-8")


def test_valid_plugin_api_2_0_registry_entry(sandbox: Path, contract: dict):
    entry = _registry_entry(pluginApiVersion="2.0")
    schema = build_index_schema(contract)
    jsonschema.Draft202012Validator(schema).validate(_index([entry]))
    _write_registry(sandbox, [entry])
    assert validate_main(["--root", str(sandbox)]) == 0


@pytest.mark.parametrize(
    "category",
    [
        "AuthorMetadataSource",
        "FileType",
        "InputProfile",
        "OutputProfile",
        "ConversionInput",
        "ConversionOutput",
        "MetadataReader",
        "MetadataWriter",
        "MetadataSource",
        "Store",
        "DeviceInterface",
    ],
)
def test_supported_categories_accepted(contract: dict, category: str):
    entry = _registry_entry(
        id=f"com.practicore.bookatrium.test.{category.lower()}",
        category=category,
        capabilities=["TemporaryFileAccess"]
        if category
        in {
            "ConversionInput",
            "ConversionOutput",
            "MetadataReader",
            "MetadataWriter",
            "DeviceInterface",
        }
        else ["StoreSearch"]
        if category == "Store"
        else ["MetadataLookup"]
        if category in {"MetadataSource", "AuthorMetadataSource"}
        else ["DeclareFileTypes"]
        if category == "FileType"
        else ["DeclareInputProfiles"]
        if category == "InputProfile"
        else ["DeclareOutputProfiles"]
        if category == "OutputProfile"
        else ["NetworkAccess"],
        metadataPath=(
            f"Plugins/Official/Test/{category}/src/BookAtrium.Plugins.Test/plugin.json"
        ),
    )
    # Only schema-level acceptance here (filesystem checked elsewhere).
    jsonschema.Draft202012Validator(build_index_schema(contract)).validate(
        _index([entry])
    )


def test_extended_metadata_fields_accepted(contract: dict):
    entry = _registry_entry(
        networkDisclosure="Sends queries to example.com",
        networkHosts=["example.com"],
        configurationRequired=True,
        secretsRequired=True,
        requiresRestart=True,
        defaultEnabled=True,
        deprecated=False,
        withdrawn=False,
        license="MIT",
        legacyBuiltInId="builtin.store.demo",
        desktopSupport=True,
        embeddedWebSupport=False,
        standaloneWebSupport=False,
        summary="Summary",
        description="Description",
        capabilities=["StoreSearch", "NetworkAccess", "CoverDownload"],
        supportedPlatforms=["windows-x64", "any"],
        minimumAppVersion="1.0.0",
    )
    jsonschema.Draft202012Validator(build_index_schema(contract)).validate(
        _index([entry])
    )


def test_unknown_property_rejected(contract: dict):
    entry = _registry_entry()
    entry["notARealField"] = True
    with pytest.raises(jsonschema.ValidationError):
        jsonschema.Draft202012Validator(build_index_schema(contract)).validate(
            _index([entry])
        )


def test_unsupported_category_rejected(contract: dict):
    entry = _registry_entry(category="Importer")
    with pytest.raises(jsonschema.ValidationError):
        jsonschema.Draft202012Validator(build_index_schema(contract)).validate(
            _index([entry])
        )


def test_unsupported_plugin_api_version_rejected(contract: dict):
    for bad in ("2.1", "3.0", "1.0"):
        entry = _registry_entry(pluginApiVersion=bad)
        with pytest.raises(jsonschema.ValidationError):
            jsonschema.Draft202012Validator(build_index_schema(contract)).validate(
                _index([entry])
            )


def test_canonical_metadata_path_accepted():
    assert is_canonical_metadata_path(CANONICAL_META)


def test_noncanonical_path_rejected(sandbox: Path):
    entry = _registry_entry(
        metadataPath="plugins/official/stores/demo-store/plugin.json"
    )
    _write_registry(sandbox, [entry])
    assert validate_main(["--root", str(sandbox)]) == 1


def test_missing_metadata_file_detected(sandbox: Path):
    entry = _registry_entry(
        metadataPath=(
            "Plugins/Official/Stores/MissingStore/src/BookAtrium.Plugins.Missing/plugin.json"
        )
    )
    _write_registry(sandbox, [entry])
    assert validate_main(["--root", str(sandbox)]) == 1


def test_unregistered_metadata_detected(sandbox: Path):
    _write_registry(sandbox, [_registry_entry()])
    extra = sandbox.joinpath(
        *(
            "Plugins/Official/Stores/ExtraStore/src/BookAtrium.Plugins.ExtraStore/"
            "plugin.json".split("/")
        )
    )
    _write_manifest(
        extra,
        id="com.practicore.bookatrium.store.extra",
        name="Extra",
        entryAssembly="BookAtrium.Plugins.ExtraStore.dll",
        entryType="BookAtrium.Plugins.ExtraStore.Plugin",
    )
    assert validate_main(["--root", str(sandbox)]) == 1


def test_duplicate_plugin_id_detected(sandbox: Path):
    a = _registry_entry()
    b = _registry_entry(
        metadataPath=(
            "Plugins/Official/Stores/Other/src/BookAtrium.Plugins.Other/plugin.json"
        )
    )
    _write_manifest(
        sandbox.joinpath(*b["metadataPath"].split("/")),
        id=a["id"],
        entryAssembly="BookAtrium.Plugins.Other.dll",
        entryType="BookAtrium.Plugins.Other.Plugin",
    )
    _write_registry(sandbox, [a, b])
    assert validate_main(["--root", str(sandbox)]) == 1


def test_duplicate_metadata_path_detected(sandbox: Path):
    a = _registry_entry(id="com.practicore.bookatrium.store.demo")
    b = _registry_entry(id="com.practicore.bookatrium.store.demo-2", name="Demo 2")
    _write_registry(sandbox, [a, b])
    assert validate_main(["--root", str(sandbox)]) == 1


def test_registry_manifest_consistency(sandbox: Path):
    entry = _registry_entry(version="9.9.9")
    _write_registry(sandbox, [entry])
    assert validate_main(["--root", str(sandbox)]) == 1


def test_amazon_us_kindle_store_registration_in_repo():
    """Live repository must register Amazon under the canonical source-tree path."""
    registry = json.loads(
        (ROOT / "registries" / "official-plugins.json").read_text(encoding="utf-8")
    )
    matches = [
        p
        for p in registry["plugins"]
        if p["id"] == "com.practicore.bookatrium.store.amazon-us-kindle"
    ]
    assert len(matches) == 1
    path = matches[0]["metadataPath"]
    assert path == (
        "Plugins/Official/Stores/AmazonUSKindleStore/src/"
        "BookAtrium.Plugins.AmazonUSKindleStore/plugin.json"
    )
    assert is_canonical_metadata_path(path)
    assert (ROOT / path).is_file()
    # Old published kebab layout must not remain as a competing metadata root.
    legacy = ROOT / "plugins" / "official" / "stores" / "amazon-us-kindle-store" / "plugin.json"
    if legacy.exists():
        # On case-insensitive FS this may resolve into Plugins/Official; only fail
        # when the path is the shallow published layout (no /src/).
        rel = legacy.resolve().relative_to(ROOT.resolve()).as_posix()
        assert "/src/" in rel


def test_live_repo_validate_official_registry():
    assert validate_main(["--root", str(ROOT)]) == 0


def test_schema_sync_check_is_clean():
    from sync_official_registry_schemas import main as sync_main

    assert sync_main(["--root", str(ROOT), "--check"]) == 0


def test_checked_in_registry_uses_only_canonical_metadata_paths():
    registry = json.loads(
        (ROOT / "registries" / "official-plugins.json").read_text(encoding="utf-8")
    )
    paths = [p["metadataPath"] for p in registry["plugins"]]
    assert paths
    assert all(is_canonical_metadata_path(path) for path in paths)
    assert len(paths) == len(set(paths))
