namespace BookAtrium.PluginContracts;

/// <summary>Supported external plugin type categories.</summary>
public enum PluginType
{
    ConversionInput = 0,
    ConversionOutput = 1,
    DeviceInterface = 2,
    MetadataReader = 3,
    MetadataSource = 4,
    MetadataWriter = 5,
    Store = 6
}
