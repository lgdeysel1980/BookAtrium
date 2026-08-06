namespace BookAtrium.PluginContracts;

/// <summary>
/// External author metadata source. Returns author-domain results only; do not use book
/// metadata result models or overwrite book identity fields through this interface.
/// </summary>
public interface IAuthorMetadataSourcePlugin : IBookAtriumPlugin
{
    Task<PluginOperationResult<IReadOnlyList<PluginAuthorMetadataResult>>> SearchAsync(
        PluginAuthorMetadataSearchRequest request,
        CancellationToken cancellationToken);
}

public sealed record PluginAuthorMetadataSearchRequest(
    string SearchMode,
    string Query,
    string? Name,
    IReadOnlyDictionary<string, string>? Identifiers,
    int MaxResults);

public sealed record PluginAuthorMetadataResult(
    string Name,
    string? SortName = null,
    string? FirstName = null,
    string? LastName = null,
    string? About = null,
    string? Link = null,
    string? SourceUrl = null,
    DateTime? BirthDate = null,
    DateTime? DeathDate = null,
    string? CoverUrl = null,
    byte[]? CoverImage = null,
    IReadOnlyDictionary<string, string>? Identifiers = null,
    string? Attribution = null);
