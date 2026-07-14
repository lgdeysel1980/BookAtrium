# Metadata source in 10 minutes

```powershell
bookatrium-plugin new metadata-source --name "My Source" --publisher "Me"
```

## One class

```csharp
using System.Runtime.CompilerServices;
using BookAtrium.PluginContracts;

public sealed class MySourcePlugin : MetadataSourcePlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-source", "My Source", "1.0.0", "Me");

    public override IReadOnlyCollection<string> NetworkHosts =>
        ["api.example.com"]; // omit if offline / fixture-only

    public override async IAsyncEnumerable<BookMetadata> SearchAsync(
        MetadataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // query.Title, query.Author, query.Isbn, query.Identifiers
        yield return new BookMetadata
        {
            Title = query.Title ?? "Unknown",
            Authors = string.IsNullOrWhiteSpace(query.Author) ? [] : [query.Author],
            Identifiers = string.IsNullOrWhiteSpace(query.Isbn)
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["isbn"] = query.Isbn }
        };
        await Task.Yield();
    }

    public override Task<BookCover?> GetCoverAsync(
        BookMetadata metadata, CancellationToken cancellationToken) =>
        Task.FromResult<BookCover?>(null); // or return Bytes / Url
}
```

Optional: set `SettingsType` and use `Context.Settings.Get<T>()` — see [settings.md](settings.md).

## Pack

```powershell
bookatrium-plugin pack
```

See `samples/MetadataPlugin.Sample`.
