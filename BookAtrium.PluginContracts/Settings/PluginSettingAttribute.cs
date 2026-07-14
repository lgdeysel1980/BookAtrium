namespace BookAtrium.PluginContracts;

/// <summary>Marks a property as a host-rendered plugin setting.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PluginSettingAttribute : Attribute
{
    public PluginSettingAttribute(string displayName) => DisplayName = displayName;

    public string DisplayName { get; }
    public object? Default { get; set; }
    public double Min { get; set; } = double.NaN;
    public double Max { get; set; } = double.NaN;
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
    public string[]? Choices { get; set; }
}

/// <summary>Setting value kinds inferred from property type / attributes.</summary>
public enum PluginSettingKind
{
    Text,
    Integer,
    Decimal,
    Boolean,
    Choice,
    Password,
    Url,
    Folder,
    File
}
