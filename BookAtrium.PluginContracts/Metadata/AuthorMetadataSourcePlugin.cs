using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Author metadata source plugin. Implement <see cref="SearchAsync"/>.</summary>
public abstract class AuthorMetadataSourcePlugin : BookAtriumPlugin, IAuthorMetadataSourcePlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.AuthorMetadataSource,
            PluginCapabilities.MetadataLookup | PluginCapabilities.CoverDownload);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    public abstract Task<PluginOperationResult<IReadOnlyList<PluginAuthorMetadataResult>>> SearchAsync(
        PluginAuthorMetadataSearchRequest request,
        CancellationToken cancellationToken);
}
