using System.Runtime.CompilerServices;
using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Metadata source plugin. Implement <see cref="SearchAsync"/>; optionally <see cref="GetCoverAsync"/>.</summary>
public abstract class MetadataSourcePlugin : BookAtriumPlugin, IMetadataSourcePlugin, IBookAtriumPlugin
{
    PluginDescriptor IMetadataSourcePlugin.Descriptor => BuildDescriptor();
    PluginDescriptor IBookAtriumPlugin.Descriptor => BuildDescriptor();

    private PluginDescriptor BuildDescriptor()
    {
        var caps = PluginCapabilities.MetadataLookup | PluginCapabilities.CoverDownload;
        var legacy = MetadataSourceCapabilities.SearchByTitleAuthor |
                     MetadataSourceCapabilities.SearchByIsbn |
                     MetadataSourceCapabilities.CoverSearch |
                     MetadataSourceCapabilities.Description |
                     MetadataSourceCapabilities.Identifiers;
        var hosts = NetworkHosts?.ToArray() ?? Array.Empty<string>();
        if (hosts.Length > 0)
            caps |= PluginCapabilities.NetworkAccess;

        var info = Info;
        return new PluginDescriptor(
            Id: info.Id,
            Name: info.Name,
            Version: info.Version,
            Author: info.Publisher,
            License: info.License,
            Description: info.Description ?? string.Empty,
            Capabilities: legacy,
            Homepage: info.Homepage,
            NetworkHosts: hosts.Length > 0 ? hosts : null,
            PluginType: PluginType.MetadataSource,
            Publisher: info.Publisher,
            PluginApiVersion: PluginApiVersion.Current,
            DeclaredCapabilities: caps,
            RequiresRestart: false,
            Configurable: SettingsType is not null);
    }

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    public abstract IAsyncEnumerable<BookMetadata> SearchAsync(
        MetadataQuery query,
        CancellationToken cancellationToken);

    public virtual Task<BookCover?> GetCoverAsync(BookMetadata metadata, CancellationToken cancellationToken) =>
        Task.FromResult<BookCover?>(null);

    async Task<IReadOnlyList<PluginMetadataResult>> IMetadataSourcePlugin.SearchAsync(
        PluginMetadataSearchRequest request,
        IPluginExecutionContext context,
        CancellationToken cancellationToken)
    {
        EnsureContext(context);
        string? isbn = null;
        request.Identifiers?.TryGetValue("isbn", out isbn);
        var query = new MetadataQuery
        {
            Title = request.Title ?? request.Query,
            Author = request.Authors?.FirstOrDefault(),
            Isbn = isbn,
            Identifiers = request.Identifiers is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(request.Identifiers)
        };

        var list = new List<PluginMetadataResult>();
        await foreach (var meta in SearchAsync(query, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            list.Add(Map(meta));
            if (list.Count >= Math.Max(1, request.MaxResults))
                break;
        }

        return list;
    }

    async Task<IReadOnlyList<PluginCoverResult>> IMetadataSourcePlugin.SearchCoversAsync(
        PluginCoverSearchRequest request,
        IPluginExecutionContext context,
        CancellationToken cancellationToken)
    {
        EnsureContext(context);
        var meta = new BookMetadata
        {
            Title = request.Title,
            Authors = request.Authors?.ToList() ?? new List<string>()
        };
        if (request.Identifiers is not null)
        {
            foreach (var kv in request.Identifiers)
                meta.Identifiers[kv.Key] = kv.Value;
        }

        var cover = await GetCoverAsync(meta, cancellationToken).ConfigureAwait(false);
        if (cover is null)
            return Array.Empty<PluginCoverResult>();

        return new[]
        {
            new PluginCoverResult(
                ImageUrl: cover.Url,
                ImageBytes: cover.Bytes.Length == 0 ? null : cover.Bytes,
                MimeType: cover.ContentType,
                Attribution: Info.Name)
        };
    }

    private void EnsureContext(IPluginExecutionContext context)
    {
        if (HasContext)
            return;

        var init = new PluginInitialisationContext
        {
            DataDirectory = Path.Combine(Path.GetTempPath(), "bookatrium-plugin", Info.Id),
            Settings = context.Settings,
            Logger = context.Logger,
            ApplicationVersion = "0.0.0",
            PluginApiVersion = PluginApiVersion.Current,
            Http = context.Http
        };
        Directory.CreateDirectory(init.DataDirectory);
        AttachContext(PluginContextFactory.FromContracts(init, this));
    }

    private PluginMetadataResult Map(BookMetadata meta)
    {
        Helpers.Metadata.Normalize(meta);
        meta.Identifiers.TryGetValue("isbn", out var isbn);
        return new PluginMetadataResult(
            Title: meta.Title ?? string.Empty,
            Authors: meta.Authors.ToArray(),
            Series: meta.Series,
            SeriesNumber: meta.SeriesIndex,
            Publisher: meta.Publisher,
            PublicationDate: meta.Published,
            Language: meta.Language,
            Description: meta.Description,
            Tags: meta.Tags.ToArray(),
            Isbn: isbn,
            Identifiers: new Dictionary<string, string>(meta.Identifiers),
            CoverUrl: meta.CoverUrl,
            CoverImage: meta.CoverBytes,
            Attribution: Info.Name);
    }
}
