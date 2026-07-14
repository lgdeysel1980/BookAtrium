using System.Runtime.CompilerServices;
using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Store plugin. Implement <see cref="SearchAsync"/>; optionally <see cref="GetBookPage"/>.</summary>
public abstract class StorePlugin : BookAtriumPlugin, IStorePlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(this, PluginType.Store, PluginCapabilities.StoreSearch | PluginCapabilities.CoverDownload);

    StoreDescriptor IStorePlugin.Store =>
        new(Info.Id, Info.Name, Info.Homepage);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    /// <summary>Search the store catalogue.</summary>
    public abstract IAsyncEnumerable<StoreBook> SearchAsync(
        StoreSearch search,
        CancellationToken cancellationToken);

    /// <summary>Optional product page URL. Defaults to <see cref="StoreBook.Url"/>.</summary>
    public virtual Uri? GetBookPage(StoreBook book) => book.Url;

    /// <summary>Fetch and parse HTML via host HTTP + AngleSharp.</summary>
    protected Task<AngleSharp.Html.Dom.IHtmlDocument> GetHtmlAsync(Uri uri, CancellationToken cancellationToken = default) =>
        Context.Http.GetHtmlAsync(uri, cancellationToken);

    async Task<PluginOperationResult<StoreSearchPage>> IStorePlugin.SearchAsync(
        StoreSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!request.HasSearchTerm)
            {
                return PluginOperationResult<StoreSearchPage>.Failure(
                    PluginErrorCodes.ResultInvalid,
                    "At least one search term is required.");
            }

            var search = new StoreSearch(request.Title, request.Author, request.Series, request.Keywords, request.MaxResults);
            var items = new List<StoreSearchItem>();
            await foreach (var book in SearchAsync(search, cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                items.Add(Map(book));
                if (items.Count >= search.MaxResults)
                    break;
            }

            return PluginOperationResult<StoreSearchPage>.Success(new StoreSearchPage(items, null, items.Count));
        }
        catch (Exception ex)
        {
            return AuthoringHostBridge.FailFromException<StoreSearchPage>(ex);
        }
    }

    private StoreSearchItem Map(StoreBook book)
    {
        var formats = book.Formats.Count > 0
            ? book.Formats.ToArray()
            : string.IsNullOrWhiteSpace(book.Format) ? null : new[] { book.Format };

        return new StoreSearchItem(
            StoreId: Info.Id,
            ExternalItemId: book.ProductId ?? book.Url?.ToString() ?? Guid.NewGuid().ToString("N"),
            Title: book.Title ?? string.Empty,
            Authors: book.Authors.ToArray(),
            Series: book.Series,
            SeriesIndex: book.SeriesIndex,
            Price: book.Price,
            Currency: book.Currency,
            Formats: formats,
            HasDrm: book.HasDrm,
            Availability: book.Availability,
            CoverUrl: book.CoverUrl,
            ProductUrl: GetBookPage(book)?.AbsoluteUri ?? book.Url?.AbsoluteUri,
            StoreName: Info.Name,
            ShortDescription: book.Description,
            Language: book.Language);
    }
}
