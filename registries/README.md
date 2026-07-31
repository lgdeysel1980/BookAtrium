# Official plugin registries

Generated and checked-in catalogue artifacts for the BookAtrium official plugin feed.

| File | Tracked | Purpose |
| --- | ---: | --- |
| `official-plugins.json` | yes | Production catalogue loaded by the desktop client |
| `official-plugin-inventory.json` | yes | Machine-readable inventory derived from manifests |
| `legacy-builtin-id-map.json` | yes | Legacy `builtin.*` → official ID migrations |
| `official-plugin-publication-report.md` | yes | Last generator report |
| `packages/*.bookplugin` | no | Release binaries (CI artifact / local dry-run) |

Regenerate with `BookAtrium.OfficialPluginRepositoryBuilder`. See
`docs/plugins/official-plugin-repository-publishing.md`.
