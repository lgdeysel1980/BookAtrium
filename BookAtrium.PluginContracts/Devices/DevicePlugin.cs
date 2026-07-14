using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Device plugin. Implement <see cref="DetectAsync"/> and <see cref="SendAsync"/>.</summary>
public abstract class DevicePlugin : BookAtriumPlugin, IDeviceInterfacePlugin
{
    private readonly Dictionary<string, Device> _devices = new(StringComparer.OrdinalIgnoreCase);

    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.DeviceInterface,
            PluginCapabilities.DetectDevice | PluginCapabilities.TransferToDevice);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    public abstract Task<Device?> DetectAsync(CancellationToken cancellationToken);

    public abstract Task SendAsync(Device device, IReadOnlyList<BookFile> books, CancellationToken cancellationToken);

    public virtual Task<IReadOnlyList<DeviceBook>> ListBooksAsync(Device device, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DeviceBook>>(Array.Empty<DeviceBook>());

    async Task<PluginOperationResult<IReadOnlyList<DeviceDescriptor>>> IDeviceInterfacePlugin.DetectAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var device = await DetectAsync(cancellationToken).ConfigureAwait(false);
            if (device is null)
                return PluginOperationResult<IReadOnlyList<DeviceDescriptor>>.Success(Array.Empty<DeviceDescriptor>());

            _devices[device.Id] = device;
            var descriptor = new DeviceDescriptor(
                device.Id,
                device.Name,
                device.Manufacturer,
                device.Model,
                StorageLocations: device.MountPath is null
                    ? null
                    : new[] { new DeviceStorageLocation("default", "Default", device.MountPath) });
            return PluginOperationResult<IReadOnlyList<DeviceDescriptor>>.Success(new[] { descriptor });
        }
        catch (Exception ex)
        {
            return AuthoringHostBridge.FailFromException<IReadOnlyList<DeviceDescriptor>>(ex);
        }
    }

    Task<PluginOperationResult<IDeviceSession>> IDeviceInterfacePlugin.OpenSessionAsync(
        string deviceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_devices.TryGetValue(deviceId, out var device))
        {
            return Task.FromResult(PluginOperationResult<IDeviceSession>.Failure(
                PluginErrorCodes.ResultInvalid,
                "Device was not found. Call DetectAsync first."));
        }

        IDeviceSession session = new SdkDeviceSession(this, device);
        return Task.FromResult(PluginOperationResult<IDeviceSession>.Success(session));
    }

    private sealed class SdkDeviceSession(DevicePlugin plugin, Device device) : IDeviceSession
    {
        public DeviceDescriptor Device { get; } = new(
            device.Id,
            device.Name,
            device.Manufacturer,
            device.Model,
            StorageLocations: device.MountPath is null
                ? new[] { new DeviceStorageLocation("default", "Default", null) }
                : new[] { new DeviceStorageLocation("default", "Default", device.MountPath) });

        public Task<PluginOperationResult<IReadOnlyList<DeviceStorageLocation>>> GetStorageLocationsAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(PluginOperationResult<IReadOnlyList<DeviceStorageLocation>>.Success(Device.StorageLocations ?? Array.Empty<DeviceStorageLocation>()));

        public Task<PluginOperationResult<DeviceCapacityInfo>> GetCapacityAsync(
            string storageLocationId,
            CancellationToken cancellationToken)
        {
            long free = 0;
            long total = 0;
            if (!string.IsNullOrWhiteSpace(device.MountPath) && Directory.Exists(device.MountPath))
            {
                try
                {
                    var info = new DriveInfo(Path.GetPathRoot(device.MountPath)!);
                    free = info.AvailableFreeSpace;
                    total = info.TotalSize;
                }
                catch
                {
                    // ignore
                }
            }

            return Task.FromResult(PluginOperationResult<DeviceCapacityInfo>.Success(
                new DeviceCapacityInfo(storageLocationId, total, free)));
        }

        public async Task<PluginOperationResult<DeviceTransferResult>> TransferAsync(
            DeviceTransferRequest request,
            IProgress<DeviceTransferProgress>? progress,
            CancellationToken cancellationToken)
        {
            try
            {
                var books = new[] { new BookFile { Path = request.SourcePath } };
                progress?.Report(new DeviceTransferProgress(0, null, "Sending"));
                await plugin.SendAsync(device, books, cancellationToken).ConfigureAwait(false);
                var dest = Path.Combine(device.MountPath ?? Path.GetTempPath(),
                    request.DestinationFileName ?? Path.GetFileName(request.SourcePath));
                progress?.Report(new DeviceTransferProgress(1, 1, "Complete"));
                return PluginOperationResult<DeviceTransferResult>.Success(
                    new DeviceTransferResult(dest, new FileInfo(request.SourcePath).Length));
            }
            catch (Exception ex)
            {
                return AuthoringHostBridge.FailFromException<DeviceTransferResult>(ex);
            }
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
