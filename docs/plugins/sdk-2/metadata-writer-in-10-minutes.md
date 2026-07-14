# Metadata writer in 10 minutes

```powershell
bookatrium-plugin new metadata-writer --name "My Writer" --publisher "Me"
```

## One class

```csharp
using BookAtrium.PluginContracts;

public sealed class MyWriterPlugin : MetadataWriterPlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-writer", "My Writer", "1.0.0", "Me");

    protected override IEnumerable<string> GetSupportedFormats() => ["txt"];

    public override bool CanWrite(BookFile file) =>
        string.Equals(file.Extension, ".txt", StringComparison.OrdinalIgnoreCase);

    public override async Task WriteAsync(
        BookFile file, BookMetadata metadata, CancellationToken cancellationToken)
    {
        if (!CanWrite(file))
            throw new PluginFormatException("Unsupported format.");

        // Host already copied the source to file.Path (temp). Write into that path only.
        var sidecar = file.Path + ".meta.txt";
        await File.WriteAllTextAsync(
            sidecar,
            $"{metadata.Title}\n{string.Join("; ", metadata.Authors)}",
            cancellationToken);
    }
}
```

Never write to the user's original path — always use `file.Path` (temp copy).

## Pack

```powershell
bookatrium-plugin pack
```

See `samples/BookAtrium.PluginSamples.MetadataWriter`.
