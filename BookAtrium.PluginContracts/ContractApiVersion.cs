namespace BookAtrium.PluginContracts;

/// <summary>
/// Independent version of the plugin contract surface. Additive minor changes remain compatible;
/// breaking changes increment the major component.
/// </summary>
public static class ContractApiVersion
{
    /// <summary>Host supports Plugin API 2.0 and retains compatibility for older package metadata.</summary>
    public const string Current = "2.0";
    public const int Major = 2;
    public const int Minor = 0;

    /// <summary>
    /// Returns true when <paramref name="requiredVersion"/> is compatible with this host API.
    /// Empty/null is treated as compatible. API 1.0 and 1.1 remain accepted for compatibility.
    /// </summary>
    public static bool IsCompatible(string? requiredVersion)
    {
        if (string.IsNullOrWhiteSpace(requiredVersion))
            return true;

        if (!Version.TryParse(Normalize(requiredVersion), out var required))
            return false;

        if (required.Major == 1 && required.Minor <= 1)
            return true;

        return required.Major == Major && required.Minor <= Minor;
    }

    private static string Normalize(string version)
    {
        var parts = version.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
            return $"{parts[0]}.0";
        return string.Join('.', parts.Take(Math.Min(parts.Length, 4)));
    }
}
