namespace BookAtrium.PluginContracts;

/// <summary>
/// Plugin-local operation result (no dependency on BookAtrium.Core).
/// </summary>
public sealed record PluginOperationResult(
    bool Succeeded,
    bool Cancelled = false,
    bool PartialSuccess = false,
    string? Code = null,
    string? Message = null,
    IReadOnlyList<string>? Warnings = null)
{
    public static PluginOperationResult Success(
        string? message = null,
        IReadOnlyList<string>? warnings = null) =>
        new(true, false, false, null, message, warnings);

    public static PluginOperationResult Partial(
        string? message = null,
        IReadOnlyList<string>? warnings = null,
        string? code = null) =>
        new(true, false, true, code, message, warnings);

    public static PluginOperationResult Failure(string code, string message) =>
        new(false, false, false, code, message);

    public static PluginOperationResult Cancel(string? message = null) =>
        new(false, true, false, PluginErrorCodes.Failed, message ?? "The operation was cancelled.");
}

/// <summary>
/// Plugin-local operation result with a typed value (no dependency on BookAtrium.Core).
/// </summary>
public sealed record PluginOperationResult<T>(
    bool Succeeded,
    T? Value = default,
    bool Cancelled = false,
    bool PartialSuccess = false,
    string? Code = null,
    string? Message = null,
    IReadOnlyList<string>? Warnings = null)
{
    public static PluginOperationResult<T> Success(
        T value,
        string? message = null,
        IReadOnlyList<string>? warnings = null) =>
        new(true, value, false, false, null, message, warnings);

    public static PluginOperationResult<T> Partial(
        T value,
        string? message = null,
        IReadOnlyList<string>? warnings = null,
        string? code = null) =>
        new(true, value, false, true, code, message, warnings);

    public static PluginOperationResult<T> Failure(string code, string message) =>
        new(false, default, false, false, code, message);

    public static PluginOperationResult<T> Cancel(string? message = null) =>
        new(false, default, true, false, PluginErrorCodes.Failed, message ?? "The operation was cancelled.");
}
