namespace BookAtrium.PluginContracts;

/// <summary>
/// Compact permission declaration. Prefer inference from plugin type and
/// <see cref="BookAtriumPlugin.NetworkHosts"/>; override only when needed.
/// </summary>
public sealed class PluginPermissions
{
    public IReadOnlyCollection<string> NetworkHosts { get; init; } = Array.Empty<string>();
}
