# Official plugin release process

Maintainer guide for BookAtrium first-party (official) plugins.

Official plugins are developed in BookAtrium’s private development repository and
published as public catalogue metadata, documentation, checksums, and release
assets through this public repository (or, during migration, a transitional
standalone release host).

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
- Preferred future release tag conventions (do not create these tags until migration):

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

## Migration of an existing plugin release

When an official plugin already has a public release on a standalone repository:

1. Do **not** rebuild, replace, rename, or republish the existing package.
2. Do **not** create a new version solely for repository reorganisation.
3. Add official catalogue metadata that points at the **exact** existing
   version-specific asset URL and checksum.
4. Keep `packageHosting` as `standalone-repository-transitional` until hosting
   migration is complete and end-to-end updater validation succeeds.
5. Only after updater validation, copy/host the **same** package bytes under a
   BookAtrium release tag, update catalogue URLs to the new immutable URL, and
   set `packageHosting` to `bookatrium`.
6. Keep the old standalone repository available until all links are verified.
7. Archive or redirect the standalone repository only after a separate, explicit
   migration task confirms consumers have moved.

### Amazon US Kindle Store v1.0.4 (hosting migration complete)

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

Facts for the completed hosting migration:

- Source development remains in BookAtrium’s private development repository.
- The BookAtrium-hosted package is byte-for-byte identical to the original standalone v1.0.4 asset.
- Official catalogue metadata now points to the BookAtrium release tag and immutable asset URL.
- No new Amazon plugin version was created for the hosting migration.
- The immutable package retains its original embedded `PractiCore` publisher metadata; the catalogue identifies BookAtrium as the official first-party publisher.
- The original standalone v1.0.4 release remains temporarily available.
- The community catalogue entry and standalone-repository retirement remain deferred follow-up work.
- The standalone repository has **not** been archived in this step.

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
