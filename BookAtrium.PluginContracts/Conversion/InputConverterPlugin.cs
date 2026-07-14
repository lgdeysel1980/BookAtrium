using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Conversion input. Declare <see cref="Extensions"/> and implement <see cref="ReadAsync"/>.</summary>
public abstract class InputConverterPlugin : BookAtriumPlugin, IConversionInputPlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.ConversionInput,
            PluginCapabilities.ReadInputFormat | PluginCapabilities.TemporaryFileAccess);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    /// <summary>Extensions including leading dots, e.g. ".xyz".</summary>
    public abstract IReadOnlyCollection<string> Extensions { get; }

    public abstract Task<ConversionDocument> ReadAsync(ConversionInput input, CancellationToken cancellationToken);

    IReadOnlySet<string> IConversionInputPlugin.SupportedExtensions =>
        new HashSet<string>(
            Extensions.Select(e => e.TrimStart('.').ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

    Task<PluginOperationResult<ConversionInputProbeResult>> IConversionInputPlugin.ProbeAsync(
        ConversionInputRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ext = Path.GetExtension(request.SourcePath);
        var ok = Extensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals("." + e.TrimStart('.'), ext, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(PluginOperationResult<ConversionInputProbeResult>.Success(
            new ConversionInputProbeResult(ok, ok ? ext.TrimStart('.') : null)));
    }

    async Task<PluginOperationResult<ConversionInputResult>> IConversionInputPlugin.ReadAsync(
        ConversionInputRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var doc = await ReadAsync(
                new ConversionInput { Path = request.SourcePath, Options = request.Options },
                cancellationToken).ConfigureAwait(false);

            Directory.CreateDirectory(request.IntermediateDirectory);
            var contentPath = Path.Combine(request.IntermediateDirectory, "content.txt");
            await File.WriteAllTextAsync(contentPath, doc.PlainText ?? doc.HtmlBody ?? string.Empty, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(doc.HtmlBody))
            {
                await File.WriteAllTextAsync(
                    Path.Combine(request.IntermediateDirectory, "content.html"),
                    doc.HtmlBody,
                    cancellationToken).ConfigureAwait(false);
            }

            foreach (var resource in doc.Resources)
            {
                var path = Path.Combine(request.IntermediateDirectory, resource.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllBytesAsync(path, resource.Bytes, cancellationToken).ConfigureAwait(false);
            }

            var snap = new PluginBookMetadataSnapshot(
                Title: doc.Title ?? doc.Metadata.Title,
                Authors: (doc.Authors.Count > 0 ? doc.Authors : doc.Metadata.Authors).ToArray(),
                Description: doc.Metadata.Description);

            return PluginOperationResult<ConversionInputResult>.Success(
                new ConversionInputResult(request.IntermediateDirectory, Path.GetExtension(request.SourcePath).TrimStart('.'), snap));
        }
        catch (Exception ex)
        {
            return AuthoringHostBridge.FailFromException<ConversionInputResult>(ex);
        }
    }
}
