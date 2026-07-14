using System.Text.Json.Serialization;

namespace BookAtrium.PluginContracts;

/// <summary>
/// Canonical package manifest for <c>.bookapp-plugin</c> packages.
/// Field names remain compatible with Phase 3D <c>plugin.json</c> (author, contractApiVersion, etc.).
/// </summary>
public sealed class PluginPackageManifest
{
    public const int SupportedManifestVersion = 1;
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxPublisherLength = 200;
    public const int MaxPathLength = 260;
    public const int MaxPlatforms = 16;
    public const int MaxCapabilities = 64;
    public const int MaxSettings = 64;
    public const int MaxHosts = 32;

    [JsonPropertyName("manifestVersion")]
    public int ManifestVersion { get; set; } = SupportedManifestVersion;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Legacy Phase 3D author field. Prefer <see cref="Publisher"/> for new packages.</summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>Publisher; when absent, hosts may fall back to <see cref="Author"/>.</summary>
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("homepage")]
    public string? Homepage { get; set; }

    [JsonPropertyName("supportUrl")]
    public string? SupportUrl { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("pluginType")]
    public string? PluginType { get; set; }

    /// <summary>Unified API version field for new packages (preferred).</summary>
    [JsonPropertyName("pluginApiVersion")]
    public string? PluginApiVersion { get; set; }

    /// <summary>Legacy Phase 3D contract API field; still accepted.</summary>
    [JsonPropertyName("contractApiVersion")]
    public string? ContractApiVersion { get; set; }

    [JsonPropertyName("minimumAppVersion")]
    public string MinimumAppVersion { get; set; } = "1.0.0";

    [JsonPropertyName("maximumAppVersion")]
    public string? MaximumAppVersion { get; set; }

    [JsonPropertyName("targetFramework")]
    public string? TargetFramework { get; set; } = "net8.0";

    [JsonPropertyName("supportedPlatforms")]
    public List<string> SupportedPlatforms { get; set; } = new();

    [JsonPropertyName("entryAssembly")]
    public string EntryAssembly { get; set; } = string.Empty;

    [JsonPropertyName("entryType")]
    public string EntryType { get; set; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; set; } = new();

    [JsonPropertyName("requiresRestart")]
    public bool RequiresRestart { get; set; } = true;

    [JsonPropertyName("configurable")]
    public bool? Configurable { get; set; }

    [JsonPropertyName("settingsSchema")]
    public List<PluginSettingDefinition>? SettingsSchema { get; set; }

    [JsonPropertyName("requiredSecrets")]
    public List<string> RequiredSecrets { get; set; } = new();

    [JsonPropertyName("defaultEnabled")]
    public bool DefaultEnabled { get; set; }

    [JsonPropertyName("networkHosts")]
    public List<string> NetworkHosts { get; set; } = new();

    [JsonPropertyName("contentTypes")]
    public List<string> ContentTypes { get; set; } = new();

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("releaseNotes")]
    public string? ReleaseNotes { get; set; }

    /// <summary>Effective publisher with author fallback.</summary>
    [JsonIgnore]
    public string EffectivePublisher =>
        !string.IsNullOrWhiteSpace(Publisher) ? Publisher! :
        !string.IsNullOrWhiteSpace(Author) ? Author! :
        string.Empty;

    /// <summary>Effective API version: pluginApiVersion, else contractApiVersion, else 1.0.</summary>
    [JsonIgnore]
    public string EffectivePluginApiVersion =>
        !string.IsNullOrWhiteSpace(PluginApiVersion) ? PluginApiVersion! :
        !string.IsNullOrWhiteSpace(ContractApiVersion) ? ContractApiVersion! :
        "1.0";

    /// <summary>Resolved plugin type; defaults to MetadataSource for legacy packages.</summary>
    [JsonIgnore]
    public PluginType EffectivePluginType
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(PluginType) &&
                Enum.TryParse<PluginType>(PluginType, ignoreCase: true, out var parsed))
                return parsed;
            return BookAtrium.PluginContracts.PluginType.MetadataSource;
        }
    }

    [JsonIgnore]
    public bool EffectiveConfigurable =>
        Configurable ?? (SettingsSchema is { Count: > 0 });
}

/// <summary>Known package file extensions.</summary>
public static class PluginPackageExtensions
{
    /// <summary>Canonical Plugin API 2.0 package extension (`bookatrium-plugin pack`).</summary>
    public const string BookPlugin = ".bookplugin";

    /// <summary>Legacy unified package extension (Plugin API 1.x).</summary>
    public const string BookAppPlugin = ".bookapp-plugin";

    /// <summary>Accepted alias for MetadataSource packages (Phase 3D).</summary>
    public const string LegacyMetadataPlugin = ".bookmetadata-plugin";

    public static bool IsRecognisedPackagePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        return path.EndsWith(BookPlugin, StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(BookAppPlugin, StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(LegacyMetadataPlugin, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsLegacyMetadataPackagePath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        path.EndsWith(LegacyMetadataPlugin, StringComparison.OrdinalIgnoreCase);

    public static bool IsLegacyBookAppPackagePath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        path.EndsWith(BookAppPlugin, StringComparison.OrdinalIgnoreCase);
}
