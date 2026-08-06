namespace BookAtrium.PluginContracts;

/// <summary>Declares file types understood by the library and import workflows.</summary>
public interface IFileTypePlugin : IBookAtriumPlugin
{
    IReadOnlyList<PluginFileTypeDefinition> FileTypes { get; }
}

/// <param name="Name">User-facing format name.</param>
/// <param name="Extension">Extension including or excluding the leading dot.</param>
/// <param name="MediaType">Usually <c>Ebook</c> or <c>Audiobook</c>.</param>
/// <param name="MimeType">Optional MIME type.</param>
public sealed record PluginFileTypeDefinition(
    string Name,
    string Extension,
    string MediaType = "Ebook",
    string? MimeType = null);
