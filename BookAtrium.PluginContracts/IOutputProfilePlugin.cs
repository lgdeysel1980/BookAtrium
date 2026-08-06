namespace BookAtrium.PluginContracts;

/// <summary>Declares reusable output-side conversion profiles.</summary>
public interface IOutputProfilePlugin : IBookAtriumPlugin
{
    IReadOnlyList<PluginOutputProfileDefinition> OutputProfiles { get; }
}

public sealed record PluginOutputProfileDefinition(
    string Id,
    string Name,
    string Description,
    string OutputFormat,
    IReadOnlyDictionary<string, string>? Options = null);
