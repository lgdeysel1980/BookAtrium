using System.Text.RegularExpressions;

namespace BookAtrium.PluginContracts;

/// <summary>Reverse-domain style plugin identity helpers.</summary>
public static class PluginId
{
    public const int MaxLength = 128;

    private static readonly Regex IdRegex = new(
        @"^[a-z0-9]([a-z0-9.\-]{0,126}[a-z0-9])?$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Validates a plugin id: lowercase ASCII letters/digits, dots, hyphens; max 128;
    /// rejects empty/whitespace and reserved <c>bookatrium.*</c>, legacy <c>bookapplication.*</c>,
    /// and <c>builtin.*</c> prefixes.
    /// </summary>
    public static bool IsValid(string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > MaxLength)
            return false;
        if (id.Any(char.IsWhiteSpace))
            return false;
        if (!IdRegex.IsMatch(id))
            return false;
        if (id.StartsWith("bookatrium.", StringComparison.Ordinal) ||
            id.StartsWith("bookapplication.", StringComparison.Ordinal) ||
            id.StartsWith("builtin.", StringComparison.Ordinal))
            return false;
        return true;
    }

    /// <summary>Returns a normalised lowercase id, or null when invalid.</summary>
    public static string? Normalize(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;
        var trimmed = id.Trim().ToLowerInvariant();
        return IsValid(trimmed) ? trimmed : null;
    }
}
