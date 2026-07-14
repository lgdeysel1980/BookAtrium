namespace BookAtrium.PluginContracts;

/// <summary>Detects connected devices and opens transfer sessions.</summary>
public interface IDeviceInterfacePlugin : IBookAtriumPlugin
{
    Task<PluginOperationResult<IReadOnlyList<DeviceDescriptor>>> DetectAsync(
        CancellationToken cancellationToken);

    Task<PluginOperationResult<IDeviceSession>> OpenSessionAsync(
        string deviceId,
        CancellationToken cancellationToken);
}

/// <summary>Active device session. Dispose to release device resources.</summary>
public interface IDeviceSession : IAsyncDisposable
{
    DeviceDescriptor Device { get; }

    Task<PluginOperationResult<IReadOnlyList<DeviceStorageLocation>>> GetStorageLocationsAsync(
        CancellationToken cancellationToken);

    Task<PluginOperationResult<DeviceCapacityInfo>> GetCapacityAsync(
        string storageLocationId,
        CancellationToken cancellationToken);

    Task<PluginOperationResult<DeviceTransferResult>> TransferAsync(
        DeviceTransferRequest request,
        IProgress<DeviceTransferProgress>? progress,
        CancellationToken cancellationToken);
}

public sealed record DeviceDescriptor(
    string DeviceId,
    string DisplayName,
    string? Manufacturer = null,
    string? Model = null,
    IReadOnlyList<string>? SupportedFormats = null,
    IReadOnlyList<DeviceStorageLocation>? StorageLocations = null);

public sealed record DeviceStorageLocation(
    string Id,
    string DisplayName,
    string? Path = null,
    bool IsRemovable = false);

public sealed record DeviceCapacityInfo(
    string StorageLocationId,
    long TotalBytes,
    long FreeBytes);

public sealed record DeviceTransferRequest(
    string SourcePath,
    string DestinationStorageLocationId,
    string? DestinationFileName = null,
    IReadOnlyDictionary<string, string>? Options = null);

public sealed record DeviceTransferProgress(
    long BytesTransferred,
    long? TotalBytes = null,
    string? Stage = null);

public sealed record DeviceTransferResult(
    string DestinationPath,
    long BytesTransferred,
    IReadOnlyList<string>? Warnings = null);
