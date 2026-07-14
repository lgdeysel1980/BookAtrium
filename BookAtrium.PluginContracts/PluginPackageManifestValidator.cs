using System.Text.Json;

namespace BookAtrium.PluginContracts;

/// <summary>
/// Pure validation helpers for <see cref="PluginPackageManifest"/> (no file I/O).
/// </summary>
public static class PluginPackageManifestValidator
{
    public static PluginPackageManifestValidationResult Validate(PluginPackageManifest? manifest)
    {
        if (manifest is null)
            return Fail(PluginErrorCodes.ManifestInvalid, "Manifest is missing.");

        if (manifest.ManifestVersion != PluginPackageManifest.SupportedManifestVersion)
            return Fail(PluginErrorCodes.ApiIncompatible,
                $"Unsupported manifest version {manifest.ManifestVersion}. Host supports {PluginPackageManifest.SupportedManifestVersion}.");

        if (!PluginId.IsValid(manifest.Id))
            return Fail(PluginErrorCodes.ManifestInvalid,
                "Plugin id must be a lowercase reverse-domain token (letters, numbers, dots, hyphens), max 128 characters, and cannot use reserved namespaces.");

        if (string.IsNullOrWhiteSpace(manifest.Name) || manifest.Name.Length > PluginPackageManifest.MaxNameLength)
            return Fail(PluginErrorCodes.ManifestInvalid, "Plugin name is missing or too long.");

        if (manifest.Description.Length > PluginPackageManifest.MaxDescriptionLength)
            return Fail(PluginErrorCodes.ManifestInvalid, "Plugin description exceeds the maximum length.");

        if (string.IsNullOrWhiteSpace(manifest.EffectivePublisher) ||
            manifest.EffectivePublisher.Length > PluginPackageManifest.MaxPublisherLength)
            return Fail(PluginErrorCodes.ManifestInvalid, "Publisher (or author) is required.");

        if (!PluginSemanticVersion.TryParse(manifest.Version, out _))
            return Fail(PluginErrorCodes.ManifestInvalid, "Plugin version must be a valid semantic version.");

        if (!string.IsNullOrWhiteSpace(manifest.PluginType) &&
            !Enum.TryParse<PluginType>(manifest.PluginType, ignoreCase: true, out _))
            return Fail(PluginErrorCodes.TypeUnsupported, $"Unknown plugin type '{manifest.PluginType}'.");

        var pluginType = manifest.EffectivePluginType;

        if (!ContractApiVersion.IsCompatible(manifest.EffectivePluginApiVersion))
            return Fail(PluginErrorCodes.ApiIncompatible,
                $"Requires plugin API {manifest.EffectivePluginApiVersion}; host supports {ContractApiVersion.Current}.");

        if (!PluginSemanticVersion.TryParse(manifest.MinimumAppVersion, out _))
            return Fail(PluginErrorCodes.ManifestInvalid, "minimumAppVersion is invalid.");

        if (!string.IsNullOrWhiteSpace(manifest.MaximumAppVersion) &&
            !PluginSemanticVersion.TryParse(manifest.MaximumAppVersion, out _))
            return Fail(PluginErrorCodes.ManifestInvalid, "maximumAppVersion is invalid.");

        if (!IsSafeRelativePath(manifest.EntryAssembly) ||
            !manifest.EntryAssembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            return Fail(PluginErrorCodes.ManifestInvalid, "entryAssembly must be a safe relative path ending in .dll.");

        if (string.IsNullOrWhiteSpace(manifest.EntryType) || manifest.EntryType.Length > 512)
            return Fail(PluginErrorCodes.ManifestInvalid, "entryType is required.");

        if (manifest.SupportedPlatforms.Count > PluginPackageManifest.MaxPlatforms)
            return Fail(PluginErrorCodes.ManifestInvalid, "Too many supportedPlatforms entries.");

        foreach (var platform in manifest.SupportedPlatforms)
        {
            if (!PluginPlatform.IsKnown(platform))
                return Fail(PluginErrorCodes.PlatformIncompatible, $"Unsupported platform '{platform}'.");
        }

        if (manifest.Capabilities.Count == 0)
            return Fail(PluginErrorCodes.ManifestInvalid, "At least one capability is required.");

        if (manifest.Capabilities.Count > PluginPackageManifest.MaxCapabilities)
            return Fail(PluginErrorCodes.ManifestInvalid, "Too many capabilities.");

        if (!PluginCapabilityRules.TryValidate(pluginType, manifest.Capabilities, out var invalidCaps))
            return Fail(PluginErrorCodes.ManifestInvalid,
                $"Capabilities not allowed for {pluginType}: {string.Join(", ", invalidCaps)}.");

        if (manifest.NetworkHosts.Count > PluginPackageManifest.MaxHosts)
            return Fail(PluginErrorCodes.ManifestInvalid, "Too many networkHosts entries.");

        if (manifest.Icon is not null && !IsSafeRelativePath(manifest.Icon))
            return Fail(PluginErrorCodes.ManifestInvalid, "icon path is unsafe.");

        if (manifest.SettingsSchema is not null)
        {
            var schemaResult = PluginSettingSchemaValidator.Validate(manifest.SettingsSchema);
            if (!schemaResult.IsValid)
                return Fail(PluginErrorCodes.ManifestInvalid, schemaResult.Message ?? "Settings schema is invalid.");
        }

        if (string.IsNullOrWhiteSpace(manifest.License))
            return Fail(PluginErrorCodes.ManifestInvalid, "Licence information is required.");

        return PluginPackageManifestValidationResult.Ok(pluginType);
    }

    public static PluginPackageManifest ParseTolerant(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<PluginPackageManifest>(json, options)
               ?? throw new InvalidOperationException("Manifest JSON deserialized to null.");
    }

    public static bool IsSafeRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > PluginPackageManifest.MaxPathLength)
            return false;
        if (Path.IsPathRooted(path))
            return false;

        var normalized = path.Replace('\\', '/');
        if (normalized.StartsWith('/') ||
            normalized.Contains("../", StringComparison.Ordinal) ||
            normalized.Contains("/..", StringComparison.Ordinal))
            return false;

        if (normalized.Split('/').Any(segment => segment is "." or ".."))
            return false;

        return true;
    }

    private static PluginPackageManifestValidationResult Fail(string code, string message) =>
        new(false, code, message, null);
}

/// <summary>Result of manifest field validation.</summary>
public sealed record PluginPackageManifestValidationResult(
    bool IsValid,
    string? ErrorCode,
    string? Message,
    PluginType? PluginType)
{
    public static PluginPackageManifestValidationResult Ok(PluginType pluginType) =>
        new(true, null, null, pluginType);
}
