using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Conversion output. Declare <see cref="Extension"/> and implement <see cref="WriteAsync"/>.</summary>
public abstract class OutputConverterPlugin : BookAtriumPlugin, IConversionOutputPlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.ConversionOutput,
            PluginCapabilities.ProduceOutputFormat | PluginCapabilities.TemporaryFileAccess);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    /// <summary>Output extension including leading dot, e.g. ".xyz".</summary>
    public abstract string Extension { get; }

    public abstract Task WriteAsync(ConversionDocument document, ConversionOutput output, CancellationToken cancellationToken);

    string IConversionOutputPlugin.OutputFormat => Extension.TrimStart('.').ToLowerInvariant();

    async Task<PluginOperationResult<ConversionOutputResult>> IConversionOutputPlugin.WriteAsync(
        ConversionOutputRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var contentPath = Path.Combine(request.IntermediateDirectory, "content.txt");
            var htmlPath = Path.Combine(request.IntermediateDirectory, "content.html");
            var doc = new ConversionDocument
            {
                PlainText = File.Exists(contentPath) ? await File.ReadAllTextAsync(contentPath, cancellationToken).ConfigureAwait(false) : null,
                HtmlBody = File.Exists(htmlPath) ? await File.ReadAllTextAsync(htmlPath, cancellationToken).ConfigureAwait(false) : null
            };

            await WriteAsync(doc, new ConversionOutput { Path = request.OutputPath, Options = request.Options }, cancellationToken)
                .ConfigureAwait(false);

            return PluginOperationResult<ConversionOutputResult>.Success(new ConversionOutputResult(request.OutputPath));
        }
        catch (Exception ex)
        {
            return AuthoringHostBridge.FailFromException<ConversionOutputResult>(ex);
        }
    }
}
