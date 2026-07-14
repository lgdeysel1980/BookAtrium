namespace BookAtrium.PluginContracts;

/// <summary>
/// Plugin-local structured error code constants (no dependency on BookAtrium.Core).
/// Host code may mirror these into application error catalogues.
/// </summary>
public static class PluginErrorCodes
{
    public const string PackageInvalid = "PLUGIN_PACKAGE_INVALID";
    public const string ManifestInvalid = "PLUGIN_MANIFEST_INVALID";
    public const string IdConflict = "PLUGIN_ID_CONFLICT";
    public const string TypeUnsupported = "PLUGIN_TYPE_UNSUPPORTED";
    public const string ApiIncompatible = "PLUGIN_API_INCOMPATIBLE";
    public const string AppVersionIncompatible = "PLUGIN_APP_VERSION_INCOMPATIBLE";
    public const string PlatformIncompatible = "PLUGIN_PLATFORM_INCOMPATIBLE";
    public const string InstallFailed = "PLUGIN_INSTALL_FAILED";
    public const string UpdateFailed = "PLUGIN_UPDATE_FAILED";
    public const string RollbackFailed = "PLUGIN_ROLLBACK_FAILED";
    public const string LoadFailed = "PLUGIN_LOAD_FAILED";
    public const string InitialiseFailed = "PLUGIN_INITIALISE_FAILED";
    public const string ExecutionFailed = "PLUGIN_EXECUTION_FAILED";
    public const string ConfigureFailed = "PLUGIN_CONFIGURE_FAILED";
    public const string DisableFailed = "PLUGIN_DISABLE_FAILED";
    public const string RemoveFailed = "PLUGIN_REMOVE_FAILED";
    public const string SafeModeActive = "PLUGIN_SAFE_MODE_ACTIVE";
    public const string ResultInvalid = "PLUGIN_RESULT_INVALID";
    public const string OperationTimedOut = "PLUGIN_OPERATION_TIMED_OUT";

    /// <summary>Legacy alias retained for compatibility with earlier metadata-plugin messaging.</summary>
    public const string TimedOut = "PLUGIN_TIMED_OUT";

    /// <summary>Legacy alias retained for compatibility with earlier metadata-plugin messaging.</summary>
    public const string Failed = "PLUGIN_FAILED";
}
