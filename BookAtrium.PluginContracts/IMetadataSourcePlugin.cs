namespace BookAtrium.PluginContracts;

/// <summary>
/// Entry interface for an external metadata-source plugin.
/// Implement only the operations your capabilities declare; the host will not call unsupported methods.
/// </summary>
/// <remarks>
/// Existing implementations need not implement <see cref="IBookAtriumPlugin"/>.
/// New metadata-source packages may optionally also implement <see cref="IBookAtriumPlugin"/>
/// for unified lifecycle (initialise/shutdown) without changing these search methods.
/// </remarks>
public interface IMetadataSourcePlugin
{
    PluginDescriptor Descriptor { get; }

    Task<IReadOnlyList<PluginMetadataResult>> SearchAsync(
        PluginMetadataSearchRequest request,
        IPluginExecutionContext context,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PluginCoverResult>> SearchCoversAsync(
        PluginCoverSearchRequest request,
        IPluginExecutionContext context,
        CancellationToken cancellationToken);
}

/// <summary>
/// Optional enrichment for a known book when the plugin declares <see cref="MetadataSourceCapabilities.MetadataEnrichment"/>.
/// </summary>
public interface IMetadataEnrichmentPlugin
{
    Task<PluginMetadataResult?> EnrichAsync(
        PluginMetadataSearchRequest request,
        IPluginExecutionContext context,
        CancellationToken cancellationToken);
}
