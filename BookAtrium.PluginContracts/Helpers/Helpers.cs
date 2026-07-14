using System.Globalization;
using System.Text.RegularExpressions;

namespace BookAtrium.PluginContracts.Helpers;

/// <summary>Lightweight query token helpers.</summary>
public static class Query
{
    public static IReadOnlyList<string> TitleTokens(string? title) => Tokenize(title);
    public static IReadOnlyList<string> AuthorTokens(string? author) => Tokenize(author);

    public static string? Isbn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var digits = Regex.Replace(value, "[^0-9Xx]", "");
        return Identifiers.IsValidIsbn(digits) ? digits.ToUpperInvariant() : null;
    }

    private static IReadOnlyList<string> Tokenize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Array.Empty<string>();
        return value.Split([' ', '\t', ',', ';', ':', '-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

public static class Identifiers
{
    public static bool IsValidIsbn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var s = Regex.Replace(value, "[^0-9Xx]", "").ToUpperInvariant();
        return s.Length is 10 or 13;
    }
}

public static class Prices
{
    public static decimal? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var cleaned = Regex.Replace(text, @"[^\d\.,]", "");
        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            return v;
        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.CurrentCulture, out v))
            return v;
        return null;
    }
}

public static class Dates
{
    public static DateTime? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            return dt.Date;
        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out dt))
            return dt.Date;
        return null;
    }
}

public static class Urls
{
    public static Uri? Resolve(Uri? baseUri, string? relativeOrAbsolute)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            return null;
        if (Uri.TryCreate(relativeOrAbsolute, UriKind.Absolute, out var abs))
            return abs;
        if (baseUri is not null && Uri.TryCreate(baseUri, relativeOrAbsolute, out var resolved))
            return resolved;
        return null;
    }
}

public static class Html
{
    public static string? Text(AngleSharp.Dom.IParentNode document, string selector)
    {
        var el = document.QuerySelector(selector);
        return string.IsNullOrWhiteSpace(el?.TextContent) ? null : el!.TextContent.Trim();
    }

    public static string? Attribute(AngleSharp.Dom.IParentNode document, string selector, string attributeName)
    {
        var el = document.QuerySelector(selector);
        var value = el?.GetAttribute(attributeName);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public static class Metadata
{
    public static BookMetadata Normalize(BookMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        if (!string.IsNullOrWhiteSpace(metadata.Title))
            metadata.Title = metadata.Title.Trim();
        metadata.Authors = metadata.Authors
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        metadata.Tags = metadata.Tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (!string.IsNullOrWhiteSpace(metadata.Language))
            metadata.Language = metadata.Language.Trim().ToLowerInvariant();
        return metadata;
    }
}
