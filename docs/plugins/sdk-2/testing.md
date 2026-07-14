# Testing plugins (SDK 2.0)

Use `BookAtrium.PluginContracts.Testing.PluginTestContext` — no BookAtrium process required.

```csharp
using BookAtrium.PluginContracts.Testing;
using Xunit;

public class MyStoreTests
{
    [Fact]
    public async Task Search_returns_items()
    {
        var ctx = PluginTestContext.Create();
        ctx.RespondWith("www.example.com", "<html><body>…fixture…</body></html>");

        var plugin = ctx.Create<MyStorePlugin>();
        var results = new List<StoreBook>();
        await foreach (var book in plugin.SearchAsync(new StoreSearch(title: "Dune"), CancellationToken.None))
            results.Add(book);

        Assert.NotEmpty(results);
    }
}
```

## Harness features

| API | Use |
|-----|-----|
| `Create<T>()` | Instantiates plugin + attaches fake `PluginContext` |
| `RespondWith(host, body)` | Fixture HTTP for `GetStringAsync` / `GetHtmlAsync` |
| `RespondWithFile(host, path)` | Load fixture from disk |
| `Logs` / `Progress` / `OpenedUrls` | Assert side effects |

## CLI

```powershell
bookatrium-plugin test --project .\MyPlugin.csproj
```

Looks for a sibling `*.Tests` project; otherwise falls back to `validate`.

Example: `samples/AmazonUsKindleStore.Tests`.
