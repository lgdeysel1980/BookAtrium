namespace BookAtrium.PluginContracts;

/// <summary>Concise code-first plugin identity. Packaging generates the manifest from this.</summary>
public sealed class PluginInfo
{
    public PluginInfo(string id, string name, string version, string publisher)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public string Publisher { get; }

    public string? Description { get; init; }
    public string? Homepage { get; init; }
    public string? SupportUrl { get; init; }
    public string License { get; init; } = "MIT";
    public string MinimumAppVersion { get; init; } = "1.0.0";
}
