namespace BookAtrium.PluginContracts;

/// <summary>
/// Writes selected metadata into an application-controlled temporary copy of a supported ebook file.
/// Must never mutate the managed original path; final replacement remains application-controlled.
/// </summary>
public interface IMetadataWriterPlugin : IBookAtriumPlugin
{
    IReadOnlySet<string> SupportedFormats { get; }

    Task<PluginOperationResult<MetadataWriteResult>> WriteMetadataAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken);
}

public sealed record MetadataWriteRequest(
    string SourcePath,
    string OutputTempPath,
    PluginBookMetadataSnapshot Metadata,
    IReadOnlyDictionary<string, string>? Options = null);

public sealed record MetadataWriteResult(
    string OutputTempPath,
    IReadOnlyList<string>? UnsupportedFields = null,
    IReadOnlyList<string>? Warnings = null);
