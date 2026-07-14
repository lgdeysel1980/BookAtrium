namespace BookAtrium.PluginContracts;

/// <summary>
/// Immutable descriptor returned by a loaded plugin instance.
/// Identity comes from the manifest plugin id, not the display name.
/// </summary>
/// <remarks>
/// Parameters after <see cref="PreferredSearchTimeout"/> are additive (contract API 1.1).
/// Existing call sites that stop at <c>Capabilities</c> / timeout / network hosts continue to compile.
/// </remarks>
public sealed record PluginDescriptor(
    string Id,
    string Name,
    string Version,
    string Author,
    string License,
    string Description,
    MetadataSourceCapabilities Capabilities,
    string? Homepage = null,
    string? SupportUrl = null,
    IReadOnlyList<string>? NetworkHosts = null,
    IReadOnlyList<string>? RequiredSecrets = null,
    TimeSpan? PreferredSearchTimeout = null,
    PluginType PluginType = PluginType.MetadataSource,
    string? Publisher = null,
    string PluginApiVersion = "1.0",
    string? MinimumAppVersion = null,
    string? MaximumAppVersion = null,
    IReadOnlyList<string>? SupportedPlatforms = null,
    PluginCapabilities DeclaredCapabilities = PluginCapabilities.None,
    bool RequiresRestart = true,
    bool Configurable = false)
{
    /// <summary>Effective publisher; falls back to <see cref="Author"/> when unset.</summary>
    public string EffectivePublisher =>
        string.IsNullOrWhiteSpace(Publisher) ? Author : Publisher;
}
