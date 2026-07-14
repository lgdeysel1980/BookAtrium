using BookAtrium.PluginContracts;

namespace BookAtrium.PluginContracts.Packaging;

/// <summary>Generates <c>plugin.json</c> from a compiled SDK plugin instance.</summary>
public static class PluginManifestGenerator
{
    public static PluginPackageManifest FromPlugin(BookAtriumPlugin plugin, string entryAssemblyFileName, string entryTypeFullName)
    {
        var info = plugin.Info;
        var pluginType = ResolveType(plugin);
        var capabilities = InferCapabilities(plugin, pluginType);
        var hosts = plugin.NetworkHosts?.Where(h => !string.IsNullOrWhiteSpace(h)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    ?? new List<string>();

        return new PluginPackageManifest
        {
            ManifestVersion = 1,
            Id = info.Id,
            Name = info.Name,
            Description = info.Description ?? string.Empty,
            Version = info.Version,
            Publisher = info.Publisher,
            Author = info.Publisher,
            Homepage = info.Homepage,
            SupportUrl = info.SupportUrl,
            License = info.License,
            PluginType = pluginType.ToString(),
            PluginApiVersion = PluginApiVersion.Current,
            MinimumAppVersion = info.MinimumAppVersion,
            TargetFramework = "net8.0",
            SupportedPlatforms = new List<string> { "windows" },
            EntryAssembly = entryAssemblyFileName,
            EntryType = entryTypeFullName,
            Capabilities = PluginCapabilityNames.ToNames(capabilities).ToList(),
            RequiresRestart = false,
            Configurable = plugin.SettingsType is not null,
            DefaultEnabled = false,
            NetworkHosts = hosts,
            ContentTypes = InferContentTypes(plugin)
        };
    }

    public static PluginType ResolveType(BookAtriumPlugin plugin) => plugin switch
    {
        StorePlugin => PluginType.Store,
        MetadataSourcePlugin => PluginType.MetadataSource,
        MetadataReaderPlugin => PluginType.MetadataReader,
        MetadataWriterPlugin => PluginType.MetadataWriter,
        InputConverterPlugin => PluginType.ConversionInput,
        OutputConverterPlugin => PluginType.ConversionOutput,
        DevicePlugin => PluginType.DeviceInterface,
        _ => throw new PluginException($"Unknown plugin base type: {plugin.GetType().FullName}")
    };

    private static PluginCapabilities InferCapabilities(BookAtriumPlugin plugin, PluginType type)
    {
        var caps = type switch
        {
            PluginType.Store => PluginCapabilities.StoreSearch | PluginCapabilities.CoverDownload,
            PluginType.MetadataSource => PluginCapabilities.MetadataLookup | PluginCapabilities.CoverDownload,
            PluginType.MetadataReader => PluginCapabilities.ReadBookMetadata,
            PluginType.MetadataWriter => PluginCapabilities.WriteBookMetadata | PluginCapabilities.TemporaryFileAccess,
            PluginType.ConversionInput => PluginCapabilities.ReadInputFormat | PluginCapabilities.TemporaryFileAccess,
            PluginType.ConversionOutput => PluginCapabilities.ProduceOutputFormat | PluginCapabilities.TemporaryFileAccess,
            PluginType.DeviceInterface => PluginCapabilities.DetectDevice | PluginCapabilities.TransferToDevice,
            _ => PluginCapabilities.None
        };

        if (plugin.NetworkHosts.Count > 0)
            caps |= PluginCapabilities.NetworkAccess;
        if (plugin.SettingsType is not null)
            caps |= PluginCapabilities.PluginSettingsStorage;
        return caps;
    }

    private static List<string> InferContentTypes(BookAtriumPlugin plugin) => plugin switch
    {
        InputConverterPlugin input => input.Extensions.Select(e => e.TrimStart('.').ToLowerInvariant()).ToList(),
        OutputConverterPlugin output => new List<string> { output.Extension.TrimStart('.').ToLowerInvariant() },
        MetadataReaderPlugin => new List<string>(),
        MetadataWriterPlugin => new List<string>(),
        _ => new List<string>()
    };
}
