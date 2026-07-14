namespace BookAtrium.PluginContracts;

public sealed record PluginMetadataSearchRequest(
    string SearchMode,
    string Query,
    string? Title,
    IReadOnlyList<string> Authors,
    IReadOnlyDictionary<string, string> Identifiers,
    string? Language,
    string? Series,
    string? Publisher,
    int MaxResults);

public sealed record PluginCoverSearchRequest(
    string? Title,
    IReadOnlyList<string> Authors,
    IReadOnlyDictionary<string, string> Identifiers,
    string? PreferredLanguage,
    int? MinimumWidth,
    int? MinimumHeight,
    int MaxResults);

public sealed record PluginMetadataResult(
    string Title,
    string? Subtitle = null,
    IReadOnlyList<string>? Authors = null,
    string? Series = null,
    double? SeriesNumber = null,
    string? Publisher = null,
    DateTime? PublicationDate = null,
    string? Language = null,
    string? Description = null,
    IReadOnlyList<string>? Tags = null,
    double? Rating = null,
    string? Isbn = null,
    IReadOnlyDictionary<string, string>? Identifiers = null,
    string? CoverUrl = null,
    byte[]? CoverImage = null,
    string? SourceUrl = null,
    double? Confidence = null,
    string? Attribution = null);

public sealed record PluginCoverResult(
    string? ImageUrl = null,
    byte[]? ImageBytes = null,
    string? MimeType = null,
    string? Attribution = null,
    string? SourceUrl = null,
    int? Width = null,
    int? Height = null);
