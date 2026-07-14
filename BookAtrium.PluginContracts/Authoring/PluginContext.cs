namespace BookAtrium.PluginContracts;

/// <summary>Host-provided services. Authors use this instead of DI or raw HttpClient.</summary>
public sealed class PluginContext
{
    public PluginContext(
        IPluginLog log,
        IPluginHttp http,
        IPluginBrowser browser,
        IPluginFiles files,
        IPluginSettingsStore settings,
        IPluginProgress progress,
        IPluginTemp temp)
    {
        Log = log;
        Http = http;
        Browser = browser;
        Files = files;
        Settings = settings;
        Progress = progress;
        Temp = temp;
    }

    public IPluginLog Log { get; }
    public IPluginHttp Http { get; }
    public IPluginBrowser Browser { get; }
    public IPluginFiles Files { get; }
    public IPluginSettingsStore Settings { get; }
    public IPluginProgress Progress { get; }
    public IPluginTemp Temp { get; }
}

public interface IPluginLog
{
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
    void Debug(string message);
}

public interface IPluginHttp
{
    Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default);
    Task<AngleSharp.Html.Dom.IHtmlDocument> GetHtmlAsync(Uri uri, CancellationToken cancellationToken = default);
    Task<byte[]> GetBytesAsync(Uri uri, CancellationToken cancellationToken = default);
}

public interface IPluginBrowser
{
    Task OpenAsync(Uri uri, CancellationToken cancellationToken = default);
}

public interface IPluginFiles
{
    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);
    bool Exists(string path);
}

public interface IPluginSettingsStore
{
    T Get<T>() where T : class, new();
    void Save<T>(T settings) where T : class;
}

public interface IPluginProgress
{
    void Report(double percent, string? message = null);
}

public interface IPluginTemp
{
    string CreateDirectory(string? prefix = null);
    string CreateFile(string? extension = null);
}
