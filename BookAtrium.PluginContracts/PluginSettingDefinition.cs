using System.Text.Json.Serialization;

namespace BookAtrium.PluginContracts;

/// <summary>Supported plugin setting field types for host-rendered configuration UI.</summary>
public enum PluginSettingFieldType
{
    Text = 0,
    Secret = 1,
    Boolean = 2,
    Integer = 3,
    Decimal = 4,
    Choice = 5,
    MultipleChoice = 6,
    Url = 7,
    Folder = 8,
    File = 9
}

/// <summary>UI-neutral setting definition declared by a plugin package.</summary>
public sealed record PluginSettingDefinition(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("type")] string Type = "text",
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("required")] bool Required = false,
    [property: JsonPropertyName("default")] string? Default = null,
    [property: JsonPropertyName("minimum")] decimal? Minimum = null,
    [property: JsonPropertyName("maximum")] decimal? Maximum = null,
    [property: JsonPropertyName("choices")] IReadOnlyList<string>? Choices = null,
    [property: JsonPropertyName("secret")] bool Secret = false)
{
    public bool TryGetFieldType(out PluginSettingFieldType fieldType)
    {
        fieldType = PluginSettingFieldType.Text;
        var normalized = Type?.Trim();
        if (string.IsNullOrEmpty(normalized))
            return false;

        if (normalized.Equals("password", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("secret", StringComparison.OrdinalIgnoreCase))
        {
            fieldType = PluginSettingFieldType.Secret;
            return true;
        }

        if (normalized.Equals("multichoice", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("multi-choice", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("multiplechoice", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("multipleChoice", StringComparison.OrdinalIgnoreCase))
        {
            fieldType = PluginSettingFieldType.MultipleChoice;
            return true;
        }

        // Legacy metadata plugin "info" labels are treated as non-editable text.
        if (normalized.Equals("info", StringComparison.OrdinalIgnoreCase))
        {
            fieldType = PluginSettingFieldType.Text;
            return true;
        }

        return Enum.TryParse(normalized, ignoreCase: true, out fieldType);
    }
}

/// <summary>UI-neutral configuration schema for a plugin package.</summary>
public sealed record PluginConfigurationSchema(
    IReadOnlyList<PluginSettingDefinition> Definitions)
{
    public static PluginConfigurationSchema Empty { get; } =
        new(Array.Empty<PluginSettingDefinition>());
}

/// <summary>Validates plugin settings schema definitions without I/O.</summary>
public static class PluginSettingSchemaValidator
{
    public const int MaxSettings = 64;
    public const int MaxKeyLength = 64;
    public const int MaxLabelLength = 200;

    public static PluginSettingSchemaValidationResult Validate(IReadOnlyList<PluginSettingDefinition>? schema)
    {
        if (schema is null)
            return PluginSettingSchemaValidationResult.Ok();

        if (schema.Count > MaxSettings)
            return Fail("Too many settings schema entries.");

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var setting in schema)
        {
            if (string.IsNullOrWhiteSpace(setting.Key) || setting.Key.Length > MaxKeyLength || !keys.Add(setting.Key))
                return Fail("Settings keys must be unique, stable, and within length limits.");

            if (string.IsNullOrWhiteSpace(setting.Label) || setting.Label.Length > MaxLabelLength)
                return Fail($"Setting '{setting.Key}' requires a label.");

            if (!setting.TryGetFieldType(out var fieldType))
                return Fail($"Unsupported setting type '{setting.Type}'.");

            if (fieldType is PluginSettingFieldType.Choice or PluginSettingFieldType.MultipleChoice &&
                (setting.Choices is null || setting.Choices.Count == 0))
                return Fail($"Choice setting '{setting.Key}' requires options.");

            if (fieldType is PluginSettingFieldType.Integer or PluginSettingFieldType.Decimal &&
                setting.Minimum is not null && setting.Maximum is not null &&
                setting.Minimum > setting.Maximum)
                return Fail($"Setting '{setting.Key}' has invalid min/max bounds.");
        }

        return PluginSettingSchemaValidationResult.Ok();
    }

    private static PluginSettingSchemaValidationResult Fail(string message) => new(false, message);
}

public sealed record PluginSettingSchemaValidationResult(bool IsValid, string? Message)
{
    public static PluginSettingSchemaValidationResult Ok() => new(true, null);
}
