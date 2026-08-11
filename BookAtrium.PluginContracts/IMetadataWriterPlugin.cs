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

/// <param name="OutputTempPath">Path of the temporary output file.</param>
/// <param name="UnsupportedFields">Fields the writer could not apply.</param>
/// <param name="Warnings">Non-fatal writer warnings.</param>
/// <param name="FileChanged">True when the temporary output file was modified relative to the source copy.</param>
public sealed record MetadataWriteResult(
    string OutputTempPath,
    IReadOnlyList<string>? UnsupportedFields = null,
    IReadOnlyList<string>? Warnings = null,
    bool FileChanged = true);
