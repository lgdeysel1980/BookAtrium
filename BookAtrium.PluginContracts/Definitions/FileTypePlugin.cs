using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>File-type definition plugin. Declare one or more <see cref="FileTypes"/>.</summary>
public abstract class FileTypePlugin : BookAtriumPlugin, IFileTypePlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.FileType,
            PluginCapabilities.DeclareFileTypes);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    public abstract IReadOnlyList<PluginFileTypeDefinition> FileTypes { get; }
}
