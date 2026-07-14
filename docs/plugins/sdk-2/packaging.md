# Packaging (Plugin API 2.0)

Do **not** hand-author `plugin.json` or maintain `pack.ps1`.

```powershell
bookatrium-plugin validate --project .\MyPlugin.csproj
bookatrium-plugin pack --project .\MyPlugin.csproj --out .\artifacts
```

## What pack does

1. `dotnet build -c Release`
2. Loads the public `BookAtriumPlugin` subclass
3. Builds `plugin.json` via `PluginManifestGenerator` from `PluginInfo`, type, `NetworkHosts`, `SettingsType`
4. Zips `plugin.json` + `lib/*.dll` (+ optional `README.md` / `LICENSE`) as `{id}-{version}.bookplugin`
5. Writes `{package}.sha256`

`BookAtrium.PluginContracts` is **not** copied into the package — the host shares that assembly.

## Dev loop without packaging

```powershell
bookatrium-plugin run --project .\MyPlugin.csproj
```

Use `BOOKATRIUM_PLUGIN_DEV_PATH` as printed; rebuild and restart (hot reload when available).

## Manifest fields (code-first)

| Code | Manifest |
|------|----------|
| `PluginInfo.Id/Name/Version/Publisher` | identity |
| Base class type | `pluginType` |
| `NetworkHosts` | `networkHosts` + network capability |
| `SettingsType` | `configurable` |
| `PluginApiVersion.Current` | `"2.0"` |

CI: see `.github/workflows/plugin-build.yml` (reusable `workflow_call`).
