namespace BookAtrium.PluginContracts;

/// <summary>Canonical plugin base. Authors subclass a type-specific class, not this type directly.</summary>
public abstract class BookAtriumPlugin
{
    private PluginContext? _context;

    /// <summary>Code-first plugin identity.</summary>
    public abstract PluginInfo Info { get; }

    /// <summary>Optional explicit network allowlist. HTTPS only; enforced by the host.</summary>
    public virtual IReadOnlyCollection<string> NetworkHosts =>
        Permissions.NetworkHosts;

    /// <summary>Optional permissions object. Prefer <see cref="NetworkHosts"/>.</summary>
    public virtual PluginPermissions Permissions { get; } = new();

    /// <summary>Optional settings model type decorated with <see cref="PluginSettingAttribute"/>.</summary>
    public virtual Type? SettingsType => null;

    /// <summary>Host services for this plugin instance. Available after the host attaches.</summary>
    protected PluginContext Context =>
        _context ?? throw new InvalidOperationException(
            "Plugin context is not attached. Use PluginTestContext in unit tests or run under BookAtrium.");

    /// <summary>Host/test infrastructure attaches the context. Authors do not call this.</summary>
    public void AttachContext(PluginContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    internal bool HasContext => _context is not null;
}
