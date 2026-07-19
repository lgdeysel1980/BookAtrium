# BookAtrium Plugin API 2.0

**Canonical public package:** `BookAtrium.PluginContracts` **2.0.0** only.

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.0.0" />
```

Use the thin base classes (`StorePlugin`, `MetadataSourcePlugin`, `MetadataReaderPlugin`,
`MetadataWriterPlugin`, `InputConverterPlugin`, `OutputConverterPlugin`, `DevicePlugin`).
Do not hand-write `plugin.json`. Third-party plugin projects should reference only `BookAtrium.PluginContracts`.

```powershell
bookatrium-plugin new store --name "My Store" --publisher "Me"
# edit one class →
bookatrium-plugin pack
```

## Guides

| Guide | Purpose |
|-------|---------|
| [getting-started.md](getting-started.md) | Package reference, create, pack, load |
| [store-plugin-in-10-minutes.md](store-plugin-in-10-minutes.md) | `StorePlugin` |
| [metadata-source-in-10-minutes.md](metadata-source-in-10-minutes.md) | `MetadataSourcePlugin` |
| [metadata-reader-in-10-minutes.md](metadata-reader-in-10-minutes.md) | `MetadataReaderPlugin` |
| [metadata-writer-in-10-minutes.md](metadata-writer-in-10-minutes.md) | `MetadataWriterPlugin` |
| [conversion-plugin-in-10-minutes.md](conversion-plugin-in-10-minutes.md) | `InputConverterPlugin` / `OutputConverterPlugin` |
| [device-plugin-in-10-minutes.md](device-plugin-in-10-minutes.md) | `DevicePlugin` |
| [settings.md](settings.md) | `[PluginSetting]` models |
| [testing.md](testing.md) | `PluginTestContext` |
| [packaging.md](packaging.md) | `bookatrium-plugin pack` |
| [publishing.md](publishing.md) | Third-party community registry release |
| [usability-checklist.md](usability-checklist.md) | Authoring checklist |
| [../official/release-process.md](../official/release-process.md) | Official first-party plugin release process (maintainers) |

### Official vs third-party plugins

| Kind | Who develops it | Public metadata | Packages |
|------|-----------------|-----------------|----------|
| Official BookAtrium plugin | BookAtrium (private development repository) | [`plugins/official/`](../../../plugins/official/) and [`registries/official-plugins.json`](../../../registries/official-plugins.json) | Version-specific GitHub Releases in this repository |
| Third-party / community plugin | Independent publisher | [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins) | Publisher’s own repository releases |

Official plugins are **not** submitted to the community catalogue. Third-party developers continue to use their own repositories and the community registry.

Community catalogue: https://github.com/lgdeysel1980/BookAtrium-Community-Plugins
