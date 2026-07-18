# Store plugin in 10 minutes

```powershell
bookatrium-plugin new store --name "My Store" --publisher "Me"
```

## One class

```csharp
using System.Runtime.CompilerServices;
using BookAtrium.PluginContracts;

public sealed class MyStorePlugin : StorePlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-store", "My Store", "1.0.0", "Me")
    {
        Description = "Search my catalogue."
    };

    // Required when calling Context.Http / GetHtmlAsync
    public override IReadOnlyCollection<string> NetworkHosts =>
        ["www.example.com"];

    public override async IAsyncEnumerable<StoreBook> SearchAsync(
        StoreSearch search,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!search.HasSearchTerm)
            yield break;

        var doc = await GetHtmlAsync(
            new Uri($"https://www.example.com/search?q={Uri.EscapeDataString(search.Title ?? search.Keywords ?? "")}"),
            cancellationToken);

        // parse → yield StoreBook { Title, Authors, ProductId, Url, Price, ... }
        yield return new StoreBook
        {
            Title = search.Title ?? "Example",
            ProductId = "sku-1",
            Url = new Uri("https://www.example.com/books/sku-1")
        };
        await Task.Yield();
    }

    public override Uri? GetBookPage(StoreBook book) => book.Url;
}
```

Use `Context.Log`, `Context.Browser.OpenAsync`, and helpers under `BookAtrium.PluginContracts.Helpers` as needed. Throw `PluginNetworkException` / `PluginException` instead of returning contract error wrappers.

## Pack

```powershell
bookatrium-plugin pack
```

See sample: `samples/BookAtrium.PluginSamples.Store` when published.

For an example of a published official store plugin’s **public catalogue** page (not implementation source), see [`plugins/official/stores/amazon-us-kindle-store`](../../../plugins/official/stores/amazon-us-kindle-store/). Official plugin implementation source is private; third-party store plugins continue to live in their own repositories.
