namespace BookAtrium.PluginContracts;

/// <summary>Declares reusable input-side conversion profiles.</summary>
public interface IInputProfilePlugin : IBookAtriumPlugin
{
    IReadOnlyList<PluginInputProfileDefinition> InputProfiles { get; }
}

public sealed record PluginInputProfileDefinition(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string> EnabledInputFormats,
    IReadOnlyDictionary<string, string>? Options = null);
