using BookAtrium.PluginContracts;

namespace BookAtrium.PluginContracts.Testing;

/// <summary>Lightweight test harness for SDK 2.0 plugins.</summary>
public sealed class PluginTestContext
{
    private readonly Dictionary<string, byte[]> _httpBodies = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _logs = new();
    private readonly List<(double Percent, string? Message)> _progress = new();
    private readonly List<Uri> _openedUrls = new();
    private readonly Dictionary<string, object> _settings = new(StringComparer.OrdinalIgnoreCase);
    private string _tempRoot = Path.Combine(Path.GetTempPath(), "bookatrium-plugin-tests", Guid.NewGuid().ToString("N"));

    public IReadOnlyList<string> Logs => _logs;
    public IReadOnlyList<(double Percent, string? Message)> Progress => _progress;
    public IReadOnlyList<Uri> OpenedUrls => _openedUrls;

    public static PluginTestContext Create() => new();

    public T Create<T>() where T : BookAtriumPlugin, new()
    {
        Directory.CreateDirectory(_tempRoot);
        var plugin = new T();
        plugin.AttachContext(new PluginContext(
            new CaptureLog(_logs),
            new FixtureHttp(_httpBodies),
            new CaptureBrowser(_openedUrls),
            new SystemFiles(),
            new MemorySettings(_settings),
            new CaptureProgress(_progress),
            new TempRoot(_tempRoot)));
        return plugin;
    }

    public void RespondWith(string host, string body, string pathPrefix = "/")
    {
        _httpBodies[host + "|" + pathPrefix] = System.Text.Encoding.UTF8.GetBytes(body);
    }

    public void RespondWithFile(string host, string filePath, string pathPrefix = "/")
    {
        _httpBodies[host + "|" + pathPrefix] = File.ReadAllBytes(filePath);
    }

    private sealed class CaptureLog(List<string> logs) : IPluginLog
    {
        public void Info(string message) => logs.Add("INFO " + message);
        public void Warning(string message) => logs.Add("WARN " + message);
        public void Error(string message, Exception? exception = null) =>
            logs.Add("ERROR " + message + (exception is null ? "" : " " + exception.Message));
        public void Debug(string message) => logs.Add("DEBUG " + message);
    }

    private sealed class FixtureHttp(Dictionary<string, byte[]> bodies) : IPluginHttp
    {
        public Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var bytes = Resolve(uri);
            return Task.FromResult(System.Text.Encoding.UTF8.GetString(bytes));
        }

        public async Task<AngleSharp.Html.Dom.IHtmlDocument> GetHtmlAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var html = await GetStringAsync(uri, cancellationToken).ConfigureAwait(false);
            return new AngleSharp.Html.Parser.HtmlParser().ParseDocument(html);
        }

        public Task<byte[]> GetBytesAsync(Uri uri, CancellationToken cancellationToken = default) =>
            Task.FromResult(Resolve(uri));

        private byte[] Resolve(Uri uri)
        {
            foreach (var kv in bodies)
            {
                var parts = kv.Key.Split('|', 2);
                if (parts.Length != 2)
                    continue;
                if (!string.Equals(parts[0], uri.Host, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (uri.AbsolutePath.StartsWith(parts[1], StringComparison.OrdinalIgnoreCase) || parts[1] == "/")
                    return kv.Value;
            }

            throw new PluginNetworkException($"No fixture response for {uri}");
        }
    }

    private sealed class CaptureBrowser(List<Uri> opened) : IPluginBrowser
    {
        public Task OpenAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            opened.Add(uri);
            return Task.CompletedTask;
        }
    }

    private sealed class SystemFiles : IPluginFiles
    {
        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default) =>
            Task.FromResult<Stream>(File.OpenRead(path));

        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) =>
            File.WriteAllBytesAsync(path, bytes, cancellationToken);

        public bool Exists(string path) => File.Exists(path);
    }

    private sealed class MemorySettings(Dictionary<string, object> store) : IPluginSettingsStore
    {
        public T Get<T>() where T : class, new()
        {
            if (store.TryGetValue(typeof(T).FullName!, out var existing) && existing is T typed)
                return typed;
            var created = new T();
            store[typeof(T).FullName!] = created;
            return created;
        }

        public void Save<T>(T settings) where T : class =>
            store[typeof(T).FullName!] = settings;
    }

    private sealed class CaptureProgress(List<(double, string?)> items) : IPluginProgress
    {
        public void Report(double percent, string? message = null) => items.Add((percent, message));
    }

    private sealed class TempRoot(string root) : IPluginTemp
    {
        public string CreateDirectory(string? prefix = null)
        {
            var path = Path.Combine(root, (prefix ?? "d") + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        public string CreateFile(string? extension = null)
        {
            var path = Path.Combine(CreateDirectory("f"), "temp" + (extension ?? ".tmp"));
            File.WriteAllBytes(path, Array.Empty<byte>());
            return path;
        }
    }
}
