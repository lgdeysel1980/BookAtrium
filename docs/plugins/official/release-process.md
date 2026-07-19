# Official plugin release process

Maintainer guide for BookAtrium first-party (official) plugins.

Official plugins are developed in BookAtrium’s private development repository and
published as public catalogue metadata, documentation, checksums, and release
assets through this public repository.

This document does not cover third-party/community plugins. Community listings
remain in [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins).

## Trust sources

BookAtrium will eventually consume two separate plugin trust sources:

| Source | Location | Meaning |
|--------|----------|---------|
| Official BookAtrium plugins | `registries/official-plugins.json` and `plugins/official/` in this repository | First-party plugins developed and maintained by BookAtrium |
| Community plugins | [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins) | Independently published third-party plugins |

Do not submit official plugins to the community catalogue.

## Version independence

- Each official plugin has its own version line.
- A plugin update occurs only when its installed `.bookplugin` package changes.
- Documentation-only catalogue edits do **not** bump the plugin version.
- Registry-only corrections (typos, URL path fixes that still point at the same
  immutable asset, wording) do **not** bump the plugin version.
- Never republish or replace an already published version’s package bytes.
- Never reuse a version number for a different package.

## Immutable packages and URLs

- Every published package must have a version-specific GitHub Release asset URL.
- Forbidden for plugin downloads: `/releases/latest` and `/releases/latest/download/...`.
- Every published package must record SHA-256 and byte size in catalogue metadata.
- Preferred release tag conventions:

```text
bookatrium-v1.0.0
plugin-store-amazon-us-kindle-v1.0.4
```

## Catalogue layout

```text
plugins/official/<category-folder>/<plugin-slug>/
  plugin.json
  README.md
  CHANGELOG.md
  LICENSE
  PRIVACY.md
  SECURITY.md
  icon.png          # optional, only when an approved icon exists

registries/
  official-plugins.json
  schemas/
    official-plugin.schema.json
    official-index.schema.json
```

Create category folders only when a plugin exists in that category.

## First public release rules

1. Package identity (plugin id, assembly name, API version) must be final.
2. Upload the `.bookplugin` and `.sha256` to a version-specific GitHub Release.
3. Record exact `downloadUrl`, `fileName`, `sizeBytes`, and `sha256` in
   `plugin.json` and `registries/official-plugins.json`.
4. Ensure required public documents exist.
5. Run `python scripts/validate_official_registry.py`.
6. Optionally run `python scripts/verify_official_package_remote.py` to confirm
   the remote asset matches the recorded checksum.
7. Do not open a community-catalogue PR for official plugins.

## Amazon US Kindle Store v1.0.4 reference

| Item | Value |
|------|-------|
| Plugin id | `com.practicore.bookatrium.store.amazon-us-kindle` |
| Version | `1.0.4` |
| Release host | `lgdeysel1980/BookAtrium` |
| Release tag | `plugin-store-amazon-us-kindle-v1.0.4` |
| Package file | `com.practicore.bookatrium.store.amazon-us-kindle-1.0.4.bookplugin` |
| Package hosting | `bookatrium` |
| SHA-256 | `65040330d195f98597f8a0484c559020cd64b3669b75fc89befd09e6a9a65719` |
| Size | `24256` bytes |

Reference notes:

- Source development remains in BookAtrium’s private development repository.
- Official catalogue metadata points to the BookAtrium release tag and immutable asset URL.
- The immutable package retains its embedded `PractiCore` publisher metadata; the catalogue identifies BookAtrium as the official first-party publisher.

## Validation

```powershell
python scripts/validate_official_registry.py
python scripts/verify_official_package_remote.py   # explicit remote audit only
```

CI workflow: `.github/workflows/validate-official-plugins.yml`

## What this process must never do

- Upload unsigned or unreviewed packages from private trees without release review
- Put private C# implementation source into this public repository
- Put credentials, signing keys, or tokens into catalogue documentation
- Make application update checking depend on repository-wide `/releases/latest`
  once plugin releases share this repository
