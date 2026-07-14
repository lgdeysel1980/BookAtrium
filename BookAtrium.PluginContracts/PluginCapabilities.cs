namespace BookAtrium.PluginContracts;

/// <summary>
/// Declarative cross-type plugin capabilities. Informational for host validation;
/// they do not create a process sandbox.
/// </summary>
/// <remarks>
/// Mapping from <see cref="MetadataSourceCapabilities"/> (metadata-source specific):
/// <list type="bullet">
/// <item><see cref="MetadataSourceCapabilities.CoverSearch"/> → typically declare <see cref="CoverDownload"/></item>
/// <item><see cref="MetadataSourceCapabilities.SearchByTitleAuthor"/> / ISBN / enrichment → <see cref="MetadataLookup"/></item>
/// <item>Any remote lookup → also declare <see cref="NetworkAccess"/></item>
/// <item>Plugin settings / secrets → <see cref="PluginSettingsStorage"/></item>
/// </list>
/// Keep the existing <see cref="MetadataSourceCapabilities"/> flags on <see cref="PluginDescriptor.Capabilities"/>
/// for metadata-source detail; use <see cref="PluginDescriptor.DeclaredCapabilities"/> for the unified set.
/// </remarks>
[Flags]
public enum PluginCapabilities
{
    None = 0,
    NetworkAccess = 1 << 0,
    PluginSettingsStorage = 1 << 1,
    TemporaryFileAccess = 1 << 2,
    ReadBookMetadata = 1 << 3,
    WriteBookMetadata = 1 << 4,
    ReadInputFormat = 1 << 5,
    ProduceOutputFormat = 1 << 6,
    DetectDevice = 1 << 7,
    TransferToDevice = 1 << 8,
    StoreSearch = 1 << 9,
    CoverDownload = 1 << 10,
    MetadataLookup = 1 << 11
}

/// <summary>Well-known capability name strings for manifests and serialisation.</summary>
public static class PluginCapabilityNames
{
    public const string NetworkAccess = nameof(PluginCapabilities.NetworkAccess);
    public const string PluginSettingsStorage = nameof(PluginCapabilities.PluginSettingsStorage);
    public const string TemporaryFileAccess = nameof(PluginCapabilities.TemporaryFileAccess);
    public const string ReadBookMetadata = nameof(PluginCapabilities.ReadBookMetadata);
    public const string WriteBookMetadata = nameof(PluginCapabilities.WriteBookMetadata);
    public const string ReadInputFormat = nameof(PluginCapabilities.ReadInputFormat);
    public const string ProduceOutputFormat = nameof(PluginCapabilities.ProduceOutputFormat);
    public const string DetectDevice = nameof(PluginCapabilities.DetectDevice);
    public const string TransferToDevice = nameof(PluginCapabilities.TransferToDevice);
    public const string StoreSearch = nameof(PluginCapabilities.StoreSearch);
    public const string CoverDownload = nameof(PluginCapabilities.CoverDownload);
    public const string MetadataLookup = nameof(PluginCapabilities.MetadataLookup);

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        NetworkAccess,
        PluginSettingsStorage,
        TemporaryFileAccess,
        ReadBookMetadata,
        WriteBookMetadata,
        ReadInputFormat,
        ProduceOutputFormat,
        DetectDevice,
        TransferToDevice,
        StoreSearch,
        CoverDownload,
        MetadataLookup
    };

    public static bool TryParse(string? name, out PluginCapabilities capability)
    {
        capability = PluginCapabilities.None;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (Enum.TryParse(name.Trim(), ignoreCase: true, out PluginCapabilities parsed) &&
            parsed != PluginCapabilities.None)
        {
            capability = parsed;
            return true;
        }
        return false;
    }

    public static PluginCapabilities ParseMany(IEnumerable<string>? names)
    {
        var flags = PluginCapabilities.None;
        if (names is null)
            return flags;
        foreach (var name in names)
        {
            if (TryParse(name, out var capability))
                flags |= capability;
        }
        return flags;
    }

    public static IReadOnlyList<string> ToNames(PluginCapabilities capabilities)
    {
        if (capabilities == PluginCapabilities.None)
            return Array.Empty<string>();

        var list = new List<string>();
        foreach (PluginCapabilities flag in Enum.GetValues<PluginCapabilities>())
        {
            if (flag == PluginCapabilities.None)
                continue;
            if (capabilities.HasFlag(flag))
                list.Add(flag.ToString());
        }
        return list;
    }
}

/// <summary>
/// Maps which declarative capabilities are allowed for each <see cref="PluginType"/>.
/// </summary>
public static class PluginCapabilityRules
{
    private static readonly IReadOnlyDictionary<PluginType, PluginCapabilities> Allowed =
        new Dictionary<PluginType, PluginCapabilities>
        {
            [PluginType.ConversionInput] =
                PluginCapabilities.TemporaryFileAccess |
                PluginCapabilities.ReadInputFormat |
                PluginCapabilities.ReadBookMetadata |
                PluginCapabilities.PluginSettingsStorage,

            [PluginType.ConversionOutput] =
                PluginCapabilities.TemporaryFileAccess |
                PluginCapabilities.ProduceOutputFormat |
                PluginCapabilities.PluginSettingsStorage,

            [PluginType.DeviceInterface] =
                PluginCapabilities.DetectDevice |
                PluginCapabilities.TransferToDevice |
                PluginCapabilities.TemporaryFileAccess |
                PluginCapabilities.PluginSettingsStorage,

            [PluginType.MetadataReader] =
                PluginCapabilities.ReadBookMetadata |
                PluginCapabilities.TemporaryFileAccess |
                PluginCapabilities.PluginSettingsStorage,

            [PluginType.MetadataSource] =
                PluginCapabilities.MetadataLookup |
                PluginCapabilities.CoverDownload |
                PluginCapabilities.NetworkAccess |
                PluginCapabilities.PluginSettingsStorage,

            [PluginType.MetadataWriter] =
                PluginCapabilities.WriteBookMetadata |
                PluginCapabilities.TemporaryFileAccess |
                PluginCapabilities.PluginSettingsStorage,

            [PluginType.Store] =
                PluginCapabilities.StoreSearch |
                PluginCapabilities.CoverDownload |
                PluginCapabilities.NetworkAccess |
                PluginCapabilities.PluginSettingsStorage
        };

    public static PluginCapabilities GetAllowed(PluginType pluginType) =>
        Allowed.TryGetValue(pluginType, out var caps) ? caps : PluginCapabilities.None;

    public static bool IsAllowed(PluginType pluginType, PluginCapabilities capability)
    {
        if (capability == PluginCapabilities.None)
            return true;
        var allowed = GetAllowed(pluginType);
        return (capability & ~allowed) == PluginCapabilities.None;
    }

    public static bool TryValidate(
        PluginType pluginType,
        IEnumerable<string>? capabilityNames,
        out IReadOnlyList<string> invalid)
    {
        var bad = new List<string>();
        if (capabilityNames is null)
        {
            invalid = bad;
            return true;
        }

        var allowed = GetAllowed(pluginType);
        foreach (var name in capabilityNames)
        {
            if (!PluginCapabilityNames.TryParse(name, out var capability))
            {
                // Legacy metadata-source capability names are accepted for MetadataSource packages.
                if (pluginType == PluginType.MetadataSource &&
                    Enum.TryParse<MetadataSourceCapabilities>(name, ignoreCase: true, out _))
                    continue;

                bad.Add(name);
                continue;
            }

            if ((capability & ~allowed) != PluginCapabilities.None)
                bad.Add(name);
        }

        invalid = bad;
        return bad.Count == 0;
    }
}
