namespace BookAtrium.PluginContracts;

/// <summary>
/// Immutable shared metadata fields used across conversion, metadata reader/writer, and related contracts.
/// Path-based; does not reference conversion IR types from BookAtrium.Conversion.
/// </summary>
/// <param name="Title">Book title.</param>
/// <param name="Subtitle">Book subtitle.</param>
/// <param name="Authors">Author display names.</param>
/// <param name="AuthorSorts">Optional author sort / file-as values aligned by index with <paramref name="Authors"/>.</param>
/// <param name="AuthorRoles">Optional author roles aligned by index with <paramref name="Authors"/> (e.g. Author, Translator).</param>
/// <param name="Series">Series name.</param>
/// <param name="SeriesIndex">Series index.</param>
/// <param name="Publisher">Publisher name.</param>
/// <param name="PublicationDate">Publication date.</param>
/// <param name="Language">Language code or name.</param>
/// <param name="Identifiers">Identifier map (e.g. ISBN, ASIN).</param>
/// <param name="Tags">Tags / subjects.</param>
/// <param name="Comments">Comments / notes.</param>
/// <param name="Description">Description / synopsis.</param>
/// <param name="Rating">Rating value.</param>
/// <param name="Isbn">ISBN when known.</param>
/// <param name="CoverBytes">Cover image bytes.</param>
/// <param name="CoverMimeType">Cover MIME type.</param>
/// <param name="PageCount">Page count when known.</param>
/// <param name="SortTitle">Sort title.</param>
/// <param name="Duration">Audiobook duration when known.</param>
/// <param name="Narrators">Narrator names.</param>
/// <param name="SupplementalFields">Format-specific supplemental key/value metadata (non-canonical fields).</param>
public sealed record PluginBookMetadataSnapshot(
    string? Title = null,
    string? Subtitle = null,
    IReadOnlyList<string>? Authors = null,
    IReadOnlyList<string>? AuthorSorts = null,
    IReadOnlyList<string>? AuthorRoles = null,
    string? Series = null,
    double? SeriesIndex = null,
    string? Publisher = null,
    DateTime? PublicationDate = null,
    string? Language = null,
    IReadOnlyDictionary<string, string>? Identifiers = null,
    IReadOnlyList<string>? Tags = null,
    string? Comments = null,
    string? Description = null,
    double? Rating = null,
    string? Isbn = null,
    byte[]? CoverBytes = null,
    string? CoverMimeType = null,
    int? PageCount = null,
    string? SortTitle = null,
    TimeSpan? Duration = null,
    IReadOnlyList<string>? Narrators = null,
    IReadOnlyDictionary<string, string>? SupplementalFields = null);
