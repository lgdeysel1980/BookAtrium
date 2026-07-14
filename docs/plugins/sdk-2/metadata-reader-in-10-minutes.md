# Metadata reader in 10 minutes

```powershell
bookatrium-plugin new metadata-reader --name "My Reader" --publisher "Me"
```

## One class

```csharp
using BookAtrium.PluginContracts;

public sealed class MyReaderPlugin : MetadataReaderPlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-reader", "My Reader", "1.0.0", "Me");

    protected override IEnumerable<string> GetSupportedFormats() => ["txt"];

    public override bool CanRead(BookFile file) =>
        string.Equals(file.Extension, ".txt", StringComparison.OrdinalIgnoreCase)
        && File.Exists(file.Path);

    public override Task<BookMetadata> ReadAsync(BookFile file, CancellationToken cancellationToken)
    {
        if (!CanRead(file))
            throw new PluginFormatException("Unsupported format.");

        return Task.FromResult(new BookMetadata
        {
            Title = Path.GetFileNameWithoutExtension(file.Path),
            Authors = ["Unknown"]
        });
    }
}
```

The host maps `BookMetadata` to its internal snapshot. Do not mutate the source file.

## Pack

```powershell
bookatrium-plugin pack
```

See `samples/BookAtrium.PluginSamples.MetadataReader`.
