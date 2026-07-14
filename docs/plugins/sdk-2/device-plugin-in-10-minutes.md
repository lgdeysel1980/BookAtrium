# Device plugin in 10 minutes

```powershell
bookatrium-plugin new device --name "My Device" --publisher "Me"
```

## One class

```csharp
using BookAtrium.PluginContracts;

public sealed class MyDevicePlugin : DevicePlugin
{
    public override PluginInfo Info { get; } = new(
        "com.me.my-device", "My Device", "1.0.0", "Me");

    public override Task<Device?> DetectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Scan for a mount / path; return null when nothing is connected
        var mount = @"E:\Books";
        if (!Directory.Exists(mount))
            return Task.FromResult<Device?>(null);

        return Task.FromResult<Device?>(new Device
        {
            Id = "my-device-1",
            Name = "My Reader",
            Manufacturer = "Acme",
            Model = "X1",
            MountPath = mount
        });
    }

    public override async Task SendAsync(
        Device device, IReadOnlyList<BookFile> books, CancellationToken cancellationToken)
    {
        var destRoot = device.MountPath
            ?? throw new PluginException("Device has no mount path.");
        foreach (var book in books)
        {
            var dest = Path.Combine(destRoot, Path.GetFileName(book.Path));
            await using var src = await Context.Files.OpenReadAsync(book.Path, cancellationToken);
            await using var dst = File.Create(dest);
            await src.CopyToAsync(dst, cancellationToken);
        }
    }
}
```

Optional: override `ListBooksAsync`. Session / transfer ceremony is handled by the SDK base.

## Pack

```powershell
bookatrium-plugin pack
```

See `samples/BookAtrium.PluginSamples.DeviceInterface`.
