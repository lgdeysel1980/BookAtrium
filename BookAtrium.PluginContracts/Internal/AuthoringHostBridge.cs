using BookAtrium.PluginContracts;

namespace BookAtrium.PluginContracts.Internal;

/// <summary>Shared host-facing bridge. Authors never call this type.</summary>
internal static class AuthoringHostBridge
{
    public static PluginDescriptor BuildDescriptor(
        BookAtriumPlugin plugin,
        PluginType pluginType,
        PluginCapabilities capabilities)
    {
        var info = plugin.Info;
        var hosts = plugin.NetworkHosts?
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? Array.Empty<string>();
        if (hosts.Length > 0)
            capabilities |= PluginCapabilities.NetworkAccess;

        if (plugin.SettingsType is not null)
            capabilities |= PluginCapabilities.PluginSettingsStorage;

        return new PluginDescriptor(
            Id: info.Id,
            Name: info.Name,
            Version: info.Version,
            Author: info.Publisher,
            License: info.License,
            Description: info.Description ?? string.Empty,
            Capabilities: MetadataSourceCapabilities.None,
            Homepage: info.Homepage,
            NetworkHosts: hosts.Length > 0 ? hosts : null,
            PluginType: pluginType,
            Publisher: info.Publisher,
            PluginApiVersion: PluginApiVersion.Current,
            DeclaredCapabilities: capabilities,
            RequiresRestart: false,
            Configurable: plugin.SettingsType is not null);
    }

    public static ValueTask<PluginInitialisationResult> InitialiseAsync(
        BookAtriumPlugin plugin,
        PluginInitialisationContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        plugin.AttachContext(PluginContextFactory.FromContracts(context, plugin));
        return ValueTask.FromResult(PluginInitialisationResult.Success());
    }

    public static ValueTask ShutdownAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public static PluginOperationResult<T> FailFromException<T>(Exception ex)
    {
        var code = ex switch
        {
            OperationCanceledException => PluginErrorCodes.Failed,
            PluginRateLimitException => PluginErrorCodes.OperationTimedOut,
            PluginNetworkException => PluginErrorCodes.ExecutionFailed,
            PluginAuthenticationException => PluginErrorCodes.ConfigureFailed,
            PluginFormatException => PluginErrorCodes.ResultInvalid,
            PluginException => PluginErrorCodes.ResultInvalid,
            _ => PluginErrorCodes.ExecutionFailed
        };
        return PluginOperationResult<T>.Failure(code, ex.Message);
    }
}

internal static class PluginContextFactory
{
    public static PluginContext FromContracts(PluginInitialisationContext context, BookAtriumPlugin plugin)
    {
        var log = new ContractsLog(context.Logger);
        var http = new ContractsHttp(context.Http, plugin.NetworkHosts);
        var browser = new ProcessBrowser();
        var files = new SystemFiles();
        var settings = new ContractsSettings(context.Settings, context.DataDirectory);
        var progress = new NullProgress();
        var temp = new SystemTemp(context.DataDirectory);
        return new PluginContext(log, http, browser, files, settings, progress, temp);
    }

    private sealed class ContractsLog(IPluginLogger logger) : IPluginLog
    {
        public void Info(string message) => logger.Information(message);
        public void Warning(string message) => logger.Warning(message);
        public void Error(string message, Exception? exception = null) => logger.Error(message, exception);
        public void Debug(string message) => logger.Debug(message);
    }

    private sealed class ContractsHttp(IPluginHttpClient? http, IReadOnlyCollection<string> hosts) : IPluginHttp
    {
        public async Task<string> GetStringAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var bytes = await GetBytesAsync(uri, cancellationToken).ConfigureAwait(false);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public async Task<AngleSharp.Html.Dom.IHtmlDocument> GetHtmlAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var html = await GetStringAsync(uri, cancellationToken).ConfigureAwait(false);
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            return parser.ParseDocument(html);
        }

        public async Task<byte[]> GetBytesAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            EnsureHttp();
            EnsureHost(uri);
            var response = await http!.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode is < 200 or >= 300)
                throw new PluginNetworkException($"HTTP {response.StatusCode} for {uri}");
            return response.Body ?? Array.Empty<byte>();
        }

        private void EnsureHttp()
        {
            if (http is null)
                throw new PluginNetworkException("Network access is not available. Declare NetworkHosts on the plugin.");
        }

        private void EnsureHost(Uri uri)
        {
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                throw new PluginNetworkException("Only HTTPS URLs are allowed.");
            if (hosts.Count == 0)
                return;
            if (!hosts.Any(h => string.Equals(h, uri.Host, StringComparison.OrdinalIgnoreCase)))
                throw new PluginNetworkException($"Host '{uri.Host}' is not in the plugin allowlist.");
        }
    }

    private sealed class ProcessBrowser : IPluginBrowser
    {
        public Task OpenAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
            return Task.CompletedTask;
        }
    }

    private sealed class SystemFiles : IPluginFiles
    {
        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stream stream = File.OpenRead(path);
            return Task.FromResult(stream);
        }

        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return File.WriteAllBytesAsync(path, bytes, cancellationToken);
        }

        public bool Exists(string path) => File.Exists(path);
    }

    private sealed class ContractsSettings(IPluginSettings settings, string dataDirectory) : IPluginSettingsStore
    {
        public T Get<T>() where T : class, new()
        {
            var instance = new T();
            ApplyDefaults(instance);
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanWrite)
                    continue;
                var raw = settings.GetText(prop.Name);
                if (raw is null)
                    continue;
                try
                {
                    var target = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    prop.SetValue(instance, Convert.ChangeType(raw, target));
                }
                catch
                {
                    // keep default
                }
            }

            return instance;
        }

        public void Save<T>(T value) where T : class
        {
            // Contracts surface is read-oriented; persist a sidecar for SDK settings in the plugin data folder.
            var path = Path.Combine(dataDirectory, "sdk-settings.json");
            var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead)
                    continue;
                map[prop.Name] = prop.GetValue(value)?.ToString();
            }

            File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(map));
        }

        private static void ApplyDefaults<T>(T instance) where T : class
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var attr = prop.GetCustomAttributes(typeof(PluginSettingAttribute), false)
                    .OfType<PluginSettingAttribute>()
                    .FirstOrDefault();
                if (attr?.Default is null || !prop.CanWrite)
                    continue;
                try
                {
                    var target = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    prop.SetValue(instance, Convert.ChangeType(attr.Default, target));
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    private sealed class NullProgress : IPluginProgress
    {
        public void Report(double percent, string? message = null) { }
    }

    private sealed class SystemTemp(string dataDirectory) : IPluginTemp
    {
        public string CreateDirectory(string? prefix = null)
        {
            var path = Path.Combine(dataDirectory, "tmp", (prefix ?? "plugin") + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        public string CreateFile(string? extension = null)
        {
            var dir = CreateDirectory("file");
            var path = Path.Combine(dir, "temp" + (extension ?? ".tmp"));
            File.WriteAllBytes(path, Array.Empty<byte>());
            return path;
        }
    }
}
