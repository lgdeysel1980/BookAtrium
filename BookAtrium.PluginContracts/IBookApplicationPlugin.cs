namespace BookAtrium.PluginContracts;

/// <summary>Minimal shared base contract for all BookAtrium plugins.</summary>
/// <remarks>
/// Existing <see cref="IMetadataSourcePlugin"/> implementations are not required to implement this interface.
/// New packages (conversion, device, store, metadata reader/writer, and new metadata sources) should implement it.
/// </remarks>
public interface IBookAtriumPlugin
{
    PluginDescriptor Descriptor { get; }

    ValueTask<PluginInitialisationResult> InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken);

    ValueTask ShutdownAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Bounded initialisation context. Does not expose DI (<c>IServiceProvider</c>), database, or host UI types.
/// </summary>
public sealed class PluginInitialisationContext
{
    public required string DataDirectory { get; init; }
    public required IPluginSettings Settings { get; init; }
    public required IPluginLogger Logger { get; init; }
    public required string ApplicationVersion { get; init; }
    public required string PluginApiVersion { get; init; }

    /// <summary>
    /// Present only when the plugin declared <see cref="PluginCapabilities.NetworkAccess"/>.
    /// </summary>
    public IPluginHttpClient? Http { get; init; }

    /// <summary>Optional clock for deterministic tests; defaults to UTC wall clock when null.</summary>
    public IPluginClock? Clock { get; init; }

    public DateTimeOffset UtcNow => Clock?.UtcNow ?? DateTimeOffset.UtcNow;
}

/// <summary>Optional host clock abstraction for plugins.</summary>
public interface IPluginClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>Result of plugin initialisation.</summary>
public sealed record PluginInitialisationResult(
    bool Succeeded,
    string? Code = null,
    string? Message = null,
    IReadOnlyList<string>? Warnings = null)
{
    public static PluginInitialisationResult Success(
        string? message = null,
        IReadOnlyList<string>? warnings = null) =>
        new(true, null, message, warnings);

    public static PluginInitialisationResult Failure(string code, string message) =>
        new(false, code, message);
}
