# Plugin settings (SDK 2.0)

Declare a POCO, mark properties with `[PluginSetting]`, point `SettingsType` at it. The host renders Settings UI.

```csharp
using BookAtrium.PluginContracts;

public sealed class MySettings
{
    [PluginSetting("Region", Description = "Catalogue region", Default = "us")]
    public string Region { get; set; } = "us";

    [PluginSetting("API Key", IsSecret = true)]
    public string? ApiKey { get; set; }

    [PluginSetting("Max results", Min = 1, Max = 50, Default = 20)]
    public int MaxResults { get; set; } = 20;

    [PluginSetting("Format", Choices = ["epub", "pdf", "any"])]
    public string PreferredFormat { get; set; } = "any";
}

public sealed class MyPlugin : StorePlugin // any BookAtriumPlugin subclass
{
    public override PluginInfo Info { get; } = new("com.me.x", "X", "1.0.0", "Me");
    public override Type? SettingsType => typeof(MySettings);

    public override async IAsyncEnumerable<StoreBook> SearchAsync(
        StoreSearch search,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var s = Context.Settings.Get<MySettings>();
        Context.Log.Info($"region={s.Region}");
        // ...
        yield break;
    }
}
```

## Notes

- Property type drives kind (string, int, decimal, bool). `IsSecret` → password; `Choices` → dropdown; `Uri`/`Directory`/`File` kinds when host supports them.
- Setting `SettingsType` marks the plugin configurable in the generated manifest.
- Persist with `Context.Settings.Save(settings)` only if you mutate and need to write back from plugin code (UI normally saves).

See `MetadataPlugin.Sample` (`SampleSettings`).
