namespace BookAtrium.PluginContracts;

/// <summary>Ebook retailer or catalogue provider plugin.</summary>
public interface IStorePlugin : IBookAtriumPlugin
{
    StoreDescriptor Store { get; }

    Task<PluginOperationResult<StoreSearchPage>> SearchAsync(
        StoreSearchRequest request,
        CancellationToken cancellationToken);
}

public sealed record StoreDescriptor(
    string Id,
    string Name,
    string? HomepageUrl = null,
    string? IconPath = null,
    string? IconUrl = null);

public sealed record StoreSearchRequest(
    string? Title,
    string? Author,
    string? Series,
    string? Keywords,
    int MaxResults,
    string? PageToken = null,
    string? Language = null,
    string? Country = null,
    string? Currency = null)
{
    public const int AbsoluteMaxResults = 100;
    public const int DefaultMaxResults = 25;

    /// <summary>True when at least one search term is present.</summary>
    public bool HasSearchTerm =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Author) ||
        !string.IsNullOrWhiteSpace(Series) ||
        !string.IsNullOrWhiteSpace(Keywords);

    /// <summary>
    /// Creates a validated request. Throws when no search term is provided.
    /// <paramref name="maxResults"/> is clamped to [1, <see cref="AbsoluteMaxResults"/>].
    /// </summary>
    public static StoreSearchRequest Create(
        string? title = null,
        string? author = null,
        string? series = null,
        string? keywords = null,
        int maxResults = DefaultMaxResults,
        string? pageToken = null,
        string? language = null,
        string? country = null,
        string? currency = null)
    {
        var request = new StoreSearchRequest(
            title, author, series, keywords,
            Math.Clamp(maxResults, 1, AbsoluteMaxResults),
            pageToken, language, country, currency);

        if (!request.HasSearchTerm)
            throw new ArgumentException(
                "At least one of Title, Author, Series, or Keywords is required.",
                nameof(title));

        return request;
    }
}

public sealed record StoreSearchPage(
    IReadOnlyList<StoreSearchItem> Items,
    string? NextPageToken = null,
    int? TotalEstimated = null);

/// <summary>
/// Normalised store catalogue hit. <see cref="HasDrm"/> is null when DRM status is unknown —
/// never infer DRM-free from a missing value.
/// </summary>
public sealed record StoreSearchItem(
    string StoreId,
    string ExternalItemId,
    string Title,
    IReadOnlyList<string>? Authors = null,
    string? Series = null,
    double? SeriesIndex = null,
    decimal? Price = null,
    string? Currency = null,
    decimal? SalePrice = null,
    decimal? OriginalPrice = null,
    IReadOnlyList<string>? Formats = null,
    bool? HasDrm = null,
    string? Availability = null,
    string? CoverUrl = null,
    string? ProductUrl = null,
    string? StoreName = null,
    string? ShortDescription = null,
    string? Language = null,
    DateTime? PublicationDate = null);
