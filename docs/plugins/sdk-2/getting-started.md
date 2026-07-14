# Getting started (Plugin API 2.0)

Build a working plugin in minutes: reference the public package → one class → pack.

## Prerequisites

- .NET 8 SDK
- NuGet package **`BookAtrium.PluginContracts` 2.0.0**

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.0.0" />
```

Optional CLI: `bookatrium-plugin` (when published). Until then you can author a small class library that inherits an API 2.0 base type and validate/pack with the CLI when available.

## Create

With the CLI:

```powershell
bookatrium-plugin new store --name "Demo Store" --publisher "You" --id "com.you.demo-store"
cd demo-store
```

Types: `store`, `metadata-source`, `metadata-reader`, `metadata-writer`, `input-converter`, `output-converter`, `device`.

Or create a `net8.0` class library manually that references **only** `BookAtrium.PluginContracts`. Manifests are generated at pack time.

## Implement

Edit the plugin class. Override `Info` and the domain method(s) for your type (for example `SearchAsync` on `StorePlugin`). Use `Context.Http`, `Context.Log`, etc. — never raw `HttpClient` to unknown hosts without declaring `NetworkHosts`.

## Validate, test, pack

```powershell
bookatrium-plugin validate
bookatrium-plugin test          # runs *.Tests if present
bookatrium-plugin pack          # → artifacts/*.bookplugin + .sha256
```

## Load in BookAtrium

Install the `.bookplugin` from Settings → Plugins, or use `bookatrium-plugin run` when the CLI is available.

## Next

- Type guides in this folder (`*-in-10-minutes.md`)
- [settings.md](settings.md) · [testing.md](testing.md) · [packaging.md](packaging.md) · [publishing.md](publishing.md)
