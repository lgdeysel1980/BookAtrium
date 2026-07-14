using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Metadata reader. Implement <see cref="CanRead"/> and <see cref="ReadAsync"/>.</summary>
public abstract class MetadataReaderPlugin : BookAtriumPlugin, IMetadataReaderPlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(this, PluginType.MetadataReader, PluginCapabilities.ReadBookMetadata);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    IReadOnlySet<string> IMetadataReaderPlugin.SupportedFormats =>
        new HashSet<string>(GetSupportedFormats(), StringComparer.OrdinalIgnoreCase);

    public abstract bool CanRead(BookFile file);

    public abstract Task<BookMetadata> ReadAsync(BookFile file, CancellationToken cancellationToken);

    /// <summary>Extensions without dots, e.g. "epub". Defaults from <see cref="CanRead"/> usage via override.</summary>
    protected virtual IEnumerable<string> GetSupportedFormats() => Array.Empty<string>();

    async Task<PluginOperationResult<MetadataReadResult>> IMetadataReaderPlugin.ReadMetadataAsync(
        MetadataReadRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var file = new BookFile { Path = request.SourcePath };
            if (!CanRead(file))
            {
                return PluginOperationResult<MetadataReadResult>.Failure(
                    PluginErrorCodes.TypeUnsupported,
                    "Format is not supported by this reader.");
            }

            var meta = await ReadAsync(file, cancellationToken).ConfigureAwait(false);
            Helpers.Metadata.Normalize(meta);
            return PluginOperationResult<MetadataReadResult>.Success(new MetadataReadResult(ToSnapshot(meta)));
        }
        catch (Exception ex)
        {
            return AuthoringHostBridge.FailFromException<MetadataReadResult>(ex);
        }
    }

    private static PluginBookMetadataSnapshot ToSnapshot(BookMetadata meta) =>
        new(
            Title: meta.Title,
            Authors: meta.Authors.ToArray(),
            Series: meta.Series,
            SeriesIndex: meta.SeriesIndex,
            Publisher: meta.Publisher,
            PublicationDate: meta.Published,
            Language: meta.Language,
            Identifiers: new Dictionary<string, string>(meta.Identifiers),
            Tags: meta.Tags.ToArray(),
            Description: meta.Description,
            Isbn: meta.Identifiers.TryGetValue("isbn", out var isbn) ? isbn : null,
            CoverBytes: meta.CoverBytes);
}
