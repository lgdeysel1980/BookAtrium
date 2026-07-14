namespace BookAtrium.PluginContracts;

public interface IPluginExecutionContext
{
    IPluginLogger Logger { get; }
    IPluginHttpClient Http { get; }
    IPluginSettings Settings { get; }
    string CorrelationId { get; }
    TimeSpan OperationTimeout { get; }
}

public interface IPluginLogger
{
    void Debug(string message);
    void Information(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}

public interface IPluginSettings
{
    string? GetText(string key);
    string? GetSecret(string key);
    bool GetBoolean(string key, bool defaultValue = false);
    int GetInteger(string key, int defaultValue = 0);
    bool TryGetChoice(string key, out string? value);
}

public interface IPluginHttpClient
{
    Task<PluginHttpResponse> GetAsync(Uri uri, CancellationToken cancellationToken = default);
    Task<PluginHttpResponse> SendAsync(PluginHttpRequest request, CancellationToken cancellationToken = default);
}

public sealed record PluginHttpRequest(
    Uri Uri,
    string Method = "GET",
    IReadOnlyDictionary<string, string>? Headers = null,
    byte[]? Body = null,
    string? ContentType = null);

public sealed record PluginHttpResponse(
    int StatusCode,
    string? ContentType,
    byte[] Body,
    Uri? FinalUri);
