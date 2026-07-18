# Publishing to the community registry

This guide is for **independently published third-party plugins**.

BookAtrium-owned official plugins are **not** submitted here. Official catalogue
metadata and release references are published through the public BookAtrium
repository (`plugins/official/` and `registries/official-plugins.json`). See
[Official plugin release process](../official/release-process.md).

Third-party packages must target **Plugin API 2.0** (`BookAtrium.PluginContracts`).
Third-party developers host their own repositories and release assets.

## Prepare release artifacts

```powershell
bookatrium-plugin prepare-release --project .\MyPlugin.csproj --out .\artifacts
```

Produces:

- `{id}-{version}.bookplugin` + `.sha256`
- `registry-entry.json` (edit download URL)
- `release-notes.md`
- `publication-checklist.md`

## Checklist

1. Tag a GitHub Release in **your** plugin repository and upload the `.bookplugin` (immutable asset URL required by the registry).
2. Confirm SHA-256 matches.
3. Set license, homepage, publisher name — do **not** claim BookAtrium support for third-party plugins, and do **not** claim official BookAtrium plugin status.
4. Open a PR against [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins) with the registry entry.
5. Wait for review/merge; the generated index must list `pluginApiVersion: "2.0"`.

## Rules of thumb

- Search/open catalogue plugins only — no DRM bypass, login scraping, or purchase automation.
- Declare accurate `NetworkHosts`.
- Ship `LICENSE` and a short `README.md` in the project (copied into the package when present).
- Use version-specific release download URLs. Do not use `/releases/latest/download/...`.

Community registry repository: https://github.com/lgdeysel1980/BookAtrium-Community-Plugins
