namespace BookAtrium.PluginContracts;

/// <summary>
/// Immutable shared metadata fields used across conversion, metadata reader/writer, and related contracts.
/// Path-based; does not reference conversion IR types from BookAtrium.Conversion.
/// </summary>
public sealed record PluginBookMetadataSnapshot(
    string? Title = null,
    string? Subtitle = null,
    IReadOnlyList<string>? Authors = null,
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
    string? CoverMimeType = null);
