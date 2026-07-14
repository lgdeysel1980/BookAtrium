namespace BookAtrium.PluginContracts;

/// <summary>Reads metadata from a supported ebook file without modifying it.</summary>
public interface IMetadataReaderPlugin : IBookAtriumPlugin
{
    IReadOnlySet<string> SupportedFormats { get; }

    Task<PluginOperationResult<MetadataReadResult>> ReadMetadataAsync(
        MetadataReadRequest request,
        CancellationToken cancellationToken);
}

public sealed record MetadataReadRequest(
    string SourcePath,
    IReadOnlyDictionary<string, string>? Options = null);

public sealed record MetadataReadResult(
    PluginBookMetadataSnapshot Metadata,
    IReadOnlyList<string>? Warnings = null);
