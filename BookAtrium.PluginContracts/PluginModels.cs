namespace BookAtrium.PluginContracts;

/// <param name="SearchMode">Search mode requested by the host.</param>
/// <param name="Query">Free-text query.</param>
/// <param name="Title">Title filter when provided.</param>
/// <param name="Authors">Author filters.</param>
/// <param name="Identifiers">Identifier filters.</param>
/// <param name="Language">Preferred language.</param>
/// <param name="Series">Series filter.</param>
/// <param name="Publisher">Publisher filter.</param>
/// <param name="MaxResults">Maximum results to return.</param>
/// <param name="PreferAudiobookResults">When true, prefer audiobook editions where the provider distinguishes them; audiobook-only identity fields remain audiobook-scoped for the host.</param>
public sealed record PluginMetadataSearchRequest(
    string SearchMode,
    string Query,
    string? Title,
    IReadOnlyList<string> Authors,
    IReadOnlyDictionary<string, string> Identifiers,
    string? Language,
    string? Series,
    string? Publisher,
    int MaxResults,
    bool PreferAudiobookResults = false);

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
    string? Attribution = null,
    string? EditionType = null,
    string? AudiobookAsin = null,
    IReadOnlyList<string>? Narrators = null,
    TimeSpan? ListeningLength = null,
    DateTime? AudiobookPublicationDate = null,
    string? AudiobookVersion = null,
    string? AudiobookLanguage = null);

public sealed record PluginCoverResult(
    string? ImageUrl = null,
    byte[]? ImageBytes = null,
    string? MimeType = null,
    string? Attribution = null,
    string? SourceUrl = null,
    int? Width = null,
    int? Height = null);
