using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Metadata writer. Implement <see cref="CanWrite"/> and <see cref="WriteAsync"/>.</summary>
public abstract class MetadataWriterPlugin : BookAtriumPlugin, IMetadataWriterPlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.MetadataWriter,
            PluginCapabilities.WriteBookMetadata | PluginCapabilities.TemporaryFileAccess);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    IReadOnlySet<string> IMetadataWriterPlugin.SupportedFormats =>
        new HashSet<string>(GetSupportedFormats(), StringComparer.OrdinalIgnoreCase);

    public abstract bool CanWrite(BookFile file);

    public abstract Task WriteAsync(BookFile file, BookMetadata metadata, CancellationToken cancellationToken);

    protected virtual IEnumerable<string> GetSupportedFormats() => Array.Empty<string>();

    async Task<PluginOperationResult<MetadataWriteResult>> IMetadataWriterPlugin.WriteMetadataAsync(
        MetadataWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(request.SourcePath))
                throw new PluginFormatException("Source file was not found.");

            File.Copy(request.SourcePath, request.OutputTempPath, overwrite: true);
            var file = new BookFile { Path = request.OutputTempPath };
            if (!CanWrite(file))
            {
                return PluginOperationResult<MetadataWriteResult>.Failure(
                    PluginErrorCodes.TypeUnsupported,
                    "Format is not supported by this writer.");
            }

            var meta = FromSnapshot(request.Metadata);
            await WriteAsync(file, meta, cancellationToken).ConfigureAwait(false);
            return PluginOperationResult<MetadataWriteResult>.Success(new MetadataWriteResult(request.OutputTempPath));
        }
        catch (Exception ex)
        {
            return AuthoringHostBridge.FailFromException<MetadataWriteResult>(ex);
        }
    }

    private static BookMetadata FromSnapshot(PluginBookMetadataSnapshot s)
    {
        var meta = new BookMetadata
        {
            Title = s.Title,
            Authors = s.Authors?.ToList() ?? new List<string>(),
            Series = s.Series,
            SeriesIndex = s.SeriesIndex,
            Publisher = s.Publisher,
            Published = s.PublicationDate,
            Language = s.Language,
            Description = s.Description ?? s.Comments,
            Tags = s.Tags?.ToList() ?? new List<string>(),
            CoverBytes = s.CoverBytes
        };
        if (s.Identifiers is not null)
        {
            foreach (var kv in s.Identifiers)
                meta.Identifiers[kv.Key] = kv.Value;
        }

        if (!string.IsNullOrWhiteSpace(s.Isbn))
            meta.Identifiers["isbn"] = s.Isbn!;
        return meta;
    }
}
