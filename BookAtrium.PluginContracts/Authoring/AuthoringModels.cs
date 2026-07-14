namespace BookAtrium.PluginContracts;

/// <summary>SDK-owned book metadata model (authors never use Core DB types).</summary>
public sealed class BookMetadata
{
    public string? Title { get; set; }
    public IList<string> Authors { get; set; } = new List<string>();
    public string? Publisher { get; set; }
    public DateTime? Published { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public string? Series { get; set; }
    public double? SeriesIndex { get; set; }
    public IList<string> Tags { get; set; } = new List<string>();
    public IDictionary<string, string> Identifiers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public string? CoverUrl { get; set; }
    public byte[]? CoverBytes { get; set; }
}

public sealed class BookCover
{
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public string? ContentType { get; set; }
    public string? Url { get; set; }
}

public sealed class BookFile
{
    public required string Path { get; init; }
    public string? Extension => System.IO.Path.GetExtension(Path);
    public string? FileName => System.IO.Path.GetFileName(Path);
}

public sealed class MetadataQuery
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public string? Isbn { get; init; }
    public IDictionary<string, string> Identifiers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> TitleTokens => Helpers.Query.TitleTokens(Title);
    public IReadOnlyList<string> AuthorTokens => Helpers.Query.AuthorTokens(Author);
}

public sealed class StoreSearch
{
    public StoreSearch(
        string? title = null,
        string? author = null,
        string? series = null,
        string? keywords = null,
        int maxResults = 25)
    {
        Title = title;
        Author = author;
        Series = series;
        Keywords = keywords;
        MaxResults = Math.Clamp(maxResults, 1, 100);
    }

    public string? Title { get; }
    public string? Author { get; }
    public string? Series { get; }
    public string? Keywords { get; }
    public int MaxResults { get; }

    public bool HasSearchTerm =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Author) ||
        !string.IsNullOrWhiteSpace(Series) ||
        !string.IsNullOrWhiteSpace(Keywords);
}

public sealed class StoreBook
{
    public string? Title { get; set; }
    public IList<string> Authors { get; set; } = new List<string>();
    public string? Series { get; set; }
    public double? SeriesIndex { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? Format { get; set; }
    public IList<string> Formats { get; set; } = new List<string>();
    public bool? HasDrm { get; set; }
    public string? Availability { get; set; }
    public string? CoverUrl { get; set; }
    public string? ProductId { get; set; }
    public Uri? Url { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
}

public sealed class ConversionInput
{
    public required string Path { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } =
        new Dictionary<string, string>();
}

public sealed class ConversionOutput
{
    public required string Path { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } =
        new Dictionary<string, string>();
}

public sealed class ConversionDocument
{
    public string? Title { get; set; }
    public IList<string> Authors { get; set; } = new List<string>();
    public string? HtmlBody { get; set; }
    public string? PlainText { get; set; }
    public BookMetadata Metadata { get; set; } = new();
    public IList<ConversionResource> Resources { get; set; } = new List<ConversionResource>();
}

public sealed class ConversionResource
{
    public required string RelativePath { get; init; }
    public required byte[] Bytes { get; init; }
    public string? ContentType { get; init; }
}

public sealed class Device
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? MountPath { get; init; }
}

public sealed class DeviceBook
{
    public required string Path { get; init; }
    public string? Title { get; init; }
    public IList<string> Authors { get; set; } = new List<string>();
}
