using BookAtrium.PluginContracts.Internal;

namespace BookAtrium.PluginContracts;

/// <summary>Input-profile definition plugin.</summary>
public abstract class InputProfilePlugin : BookAtriumPlugin, IInputProfilePlugin
{
    PluginDescriptor IBookAtriumPlugin.Descriptor =>
        AuthoringHostBridge.BuildDescriptor(
            this,
            PluginType.InputProfile,
            PluginCapabilities.DeclareInputProfiles);

    ValueTask<PluginInitialisationResult> IBookAtriumPlugin.InitialiseAsync(
        PluginInitialisationContext context,
        CancellationToken cancellationToken) =>
        AuthoringHostBridge.InitialiseAsync(this, context, cancellationToken);

    ValueTask IBookAtriumPlugin.ShutdownAsync(CancellationToken cancellationToken) =>
        AuthoringHostBridge.ShutdownAsync(cancellationToken);

    public abstract IReadOnlyList<PluginInputProfileDefinition> InputProfiles { get; }
}
