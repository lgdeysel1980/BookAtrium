namespace BookAtrium.PluginContracts;

/// <summary>Supported external plugin type categories.</summary>
public enum PluginType
{
    ConversionInput = 0,
    ConversionOutput = 1,
    /// <summary>Third-party device transfer. No official implementation ships; the type remains supported.</summary>
    DeviceInterface = 2,
    MetadataReader = 3,
    MetadataSource = 4,
    MetadataWriter = 5,
    Store = 6,
    FileType = 7,
    InputProfile = 8,
    OutputProfile = 9,
    /// <summary>Author-domain metadata lookup (biography, sort name, author identifiers).</summary>
    AuthorMetadataSource = 10
}
