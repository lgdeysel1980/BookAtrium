# BookAtrium.PluginContracts 2.0

Canonical public package for BookAtrium plugins. Reference this package only:

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.0.1" />
```

Inherit a type base (`StorePlugin`, `MetadataSourcePlugin`, `MetadataReaderPlugin`,
`MetadataWriterPlugin`, `InputConverterPlugin`, `OutputConverterPlugin`, or `DevicePlugin`),
implement the domain method(s), then pack with `bookatrium-plugin pack` when the CLI is available.

Third-party plugin projects should reference only `BookAtrium.PluginContracts`.

Developer guides: [docs/plugins/sdk-2](https://github.com/lgdeysel1980/BookAtrium/tree/main/docs/plugins/sdk-2)

Official plugin catalogue (first-party): [`plugins/official/`](../plugins/official/) and [`registries/official-plugins.json`](../registries/official-plugins.json)  
Community catalogue (third-party): [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins)
