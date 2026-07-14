using System.Text.RegularExpressions;

namespace BookAtrium.PluginContracts;

/// <summary>
/// SemVer 2.0 identity used for plugin and application version comparisons.
/// Build metadata is ignored for precedence. Invalid input displays as <c>Unknown</c>.
/// </summary>
public readonly struct PluginSemanticVersion : IComparable<PluginSemanticVersion>, IEquatable<PluginSemanticVersion>
{
    private static readonly Regex CoreRegex = new(
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<pre>[0-9A-Za-z\-\.]+))?(?:\+(?<build>[0-9A-Za-z\-\.]+))?$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static PluginSemanticVersion Unknown { get; } = new(false, 0, 0, 0, Array.Empty<string>(), string.Empty);

    public bool IsValid { get; }
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public IReadOnlyList<string> PreRelease { get; }
    public string Original { get; }

    private PluginSemanticVersion(bool isValid, int major, int minor, int patch, IReadOnlyList<string> preRelease, string original)
    {
        IsValid = isValid;
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        Original = original;
    }

    public static PluginSemanticVersion Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Unknown;

        var trimmed = value.Trim();
        var match = CoreRegex.Match(trimmed);
        if (!match.Success)
            return Unknown;

        if (!int.TryParse(match.Groups["major"].Value, out var major) ||
            !int.TryParse(match.Groups["minor"].Value, out var minor) ||
            !int.TryParse(match.Groups["patch"].Value, out var patch))
            return Unknown;

        var pre = match.Groups["pre"].Success
            ? match.Groups["pre"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        foreach (var part in pre)
        {
            if (part.Length == 0)
                return Unknown;
            if (part.All(char.IsDigit) && part.Length > 1 && part[0] == '0')
                return Unknown;
        }

        return new PluginSemanticVersion(true, major, minor, patch, pre, trimmed);
    }

    public static bool TryParse(string? value, out PluginSemanticVersion version)
    {
        version = Parse(value);
        return version.IsValid;
    }

    public int CompareTo(PluginSemanticVersion other)
    {
        if (!IsValid && !other.IsValid)
            return 0;
        if (!IsValid)
            return -1;
        if (!other.IsValid)
            return 1;

        var core = Major.CompareTo(other.Major);
        if (core != 0) return core;
        core = Minor.CompareTo(other.Minor);
        if (core != 0) return core;
        core = Patch.CompareTo(other.Patch);
        if (core != 0) return core;

        if (PreRelease.Count == 0 && other.PreRelease.Count == 0)
            return 0;
        if (PreRelease.Count == 0)
            return 1;
        if (other.PreRelease.Count == 0)
            return -1;

        var len = Math.Min(PreRelease.Count, other.PreRelease.Count);
        for (var i = 0; i < len; i++)
        {
            var left = PreRelease[i];
            var right = other.PreRelease[i];
            var leftNumeric = left.All(char.IsDigit);
            var rightNumeric = right.All(char.IsDigit);
            if (leftNumeric && rightNumeric)
            {
                var cmp = int.Parse(left).CompareTo(int.Parse(right));
                if (cmp != 0) return cmp;
            }
            else if (leftNumeric)
            {
                return -1;
            }
            else if (rightNumeric)
            {
                return 1;
            }
            else
            {
                var cmp = string.CompareOrdinal(left, right);
                if (cmp != 0) return cmp;
            }
        }

        return PreRelease.Count.CompareTo(other.PreRelease.Count);
    }

    public bool Equals(PluginSemanticVersion other) => CompareTo(other) == 0 && IsValid == other.IsValid;

    public override bool Equals(object? obj) => obj is PluginSemanticVersion other && Equals(other);

    public override int GetHashCode()
    {
        if (!IsValid)
            return 0;
        var hash = HashCode.Combine(Major, Minor, Patch);
        foreach (var part in PreRelease)
            hash = HashCode.Combine(hash, part);
        return hash;
    }

    public override string ToString() => IsValid ? Original : "Unknown";

    public static bool operator ==(PluginSemanticVersion left, PluginSemanticVersion right) => left.Equals(right);
    public static bool operator !=(PluginSemanticVersion left, PluginSemanticVersion right) => !left.Equals(right);
    public static bool operator <(PluginSemanticVersion left, PluginSemanticVersion right) => left.CompareTo(right) < 0;
    public static bool operator >(PluginSemanticVersion left, PluginSemanticVersion right) => left.CompareTo(right) > 0;
    public static bool operator <=(PluginSemanticVersion left, PluginSemanticVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PluginSemanticVersion left, PluginSemanticVersion right) => left.CompareTo(right) >= 0;

    /// <summary>True when <paramref name="appVersion"/> is within [minimum, maximum] (inclusive).</summary>
    public static bool IsInRange(string? appVersion, string? minimum, string? maximum)
    {
        var app = Parse(appVersion);
        var min = Parse(minimum);
        if (!app.IsValid || !min.IsValid)
            return false;
        if (app < min)
            return false;
        if (!string.IsNullOrWhiteSpace(maximum))
        {
            var max = Parse(maximum);
            if (!max.IsValid || app > max)
                return false;
        }
        return true;
    }
}
