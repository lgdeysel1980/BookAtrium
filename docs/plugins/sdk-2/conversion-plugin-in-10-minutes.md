# Conversion plugins in 10 minutes

CLI types: `input-converter` and `output-converter`.

```powershell
bookatrium-plugin new input-converter --name "My Input" --publisher "Me"
bookatrium-plugin new output-converter --name "My Output" --publisher "Me"
```

## Input — `InputConverterPlugin`

```csharp
using BookAtrium.PluginContracts;

public sealed class MyInputPlugin : InputConverterPlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-input", "My Input", "1.0.0", "Me");

    public override IReadOnlyCollection<string> Extensions { get; } = [".xyz"];

    public override async Task<ConversionDocument> ReadAsync(
        ConversionInput input, CancellationToken cancellationToken)
    {
        var text = await File.ReadAllTextAsync(input.Path, cancellationToken);
        return new ConversionDocument
        {
            Title = Path.GetFileNameWithoutExtension(input.Path),
            PlainText = text,
            // HtmlBody / Resources optional
        };
    }
}
```

## Output — `OutputConverterPlugin`

```csharp
using BookAtrium.PluginContracts;

public sealed class MyOutputPlugin : OutputConverterPlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-output", "My Output", "1.0.0", "Me");

    public override string Extension => ".xyz";

    public override async Task WriteAsync(
        ConversionDocument document, ConversionOutput output, CancellationToken cancellationToken)
    {
        var body = document.PlainText ?? document.HtmlBody ?? "";
        await File.WriteAllTextAsync(output.Path, body, cancellationToken);
    }
}
```

The SDK persists intermediate `content.txt` / `content.html` for the host pipeline.

## Pack

```powershell
bookatrium-plugin pack
```

Samples: `BookAtrium.PluginSamples.ConversionInput`, `BookAtrium.PluginSamples.ConversionOutput`.
