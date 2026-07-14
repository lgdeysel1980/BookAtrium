namespace BookAtrium.PluginContracts;

/// <summary>Well-known platform tokens used in package manifests.</summary>
public static class PluginPlatform
{
    public const string WindowsX64 = "windows-x64";
    public const string WindowsX86 = "windows-x86";
    public const string WindowsArm64 = "windows-arm64";
    public const string Windows = "windows";
    public const string Any = "any";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        WindowsX64,
        WindowsX86,
        WindowsArm64,
        Windows,
        Any
    };

    public static bool IsKnown(string? platform) =>
        !string.IsNullOrWhiteSpace(platform) && Known.Contains(platform.Trim());
}
