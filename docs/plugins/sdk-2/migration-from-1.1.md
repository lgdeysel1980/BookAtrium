# Migrate from Plugin API 1.1 to API 2.0

API 1.1 (hand-written type interfaces / `PluginDescriptor` / `plugin.json` / `pack.ps1`) still loads in the host for compatibility, but **new authoring uses Plugin API 2.0 inside `BookAtrium.PluginContracts`**.

Third-party plugin projects should reference only `BookAtrium.PluginContracts`:

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.0.0" />
```

## Steps

1. Ensure your package reference is `BookAtrium.PluginContracts` **2.0.0** (not any other BookAtrium package).
2. Delete (or stop editing) hand-maintained `plugin.json` and `pack.ps1`.
3. Change your class to inherit the matching base:

| 1.1 interface | API 2.0 base |
|---------------|--------------|
| `IStorePlugin` | `StorePlugin` |
| `IMetadataSourcePlugin` | `MetadataSourcePlugin` |
| `IMetadataReaderPlugin` | `MetadataReaderPlugin` |
| `IMetadataWriterPlugin` | `MetadataWriterPlugin` |
| `IConversionInputPlugin` | `InputConverterPlugin` |
| `IConversionOutputPlugin` | `OutputConverterPlugin` |
| `IDeviceInterfacePlugin` | `DevicePlugin` |

4. Replace `PluginDescriptor` with `PluginInfo` (+ optional `NetworkHosts` / `SettingsType`).
5. Remove empty `InitialiseAsync` / `ShutdownAsync` — the base handles host attach.
6. Stop returning `PluginOperationResult<>`; implement domain methods and throw `PluginException` subclasses on failure.
7. Map DTOs to authoring models (`StoreBook`, `BookMetadata`, `ConversionDocument`, `Device`, …).
8. Pack with `bookatrium-plugin pack`.

## Keep

- Stable plugin **id** (so installs/updates continue).
- Behavioural contracts (formats, network hosts, capabilities intent).

## Samples comparison

See [../../../samples/SDK2-MIGRATION.md](../../../samples/SDK2-MIGRATION.md). Legacy 1.1 authoring docs: [../legacy-api-1.1/](../legacy-api-1.1/).
