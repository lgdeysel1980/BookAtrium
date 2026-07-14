namespace BookAtrium.PluginContracts;

/// <summary>Converts a source ebook file into an application-owned path-based intermediate representation.</summary>
public interface IConversionInputPlugin : IBookAtriumPlugin
{
    /// <summary>Supported source extensions without leading dots (e.g. "epub", "mobi").</summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    /// <summary>Optional probe to check whether a file can be read without producing an artifact.</summary>
    Task<PluginOperationResult<ConversionInputProbeResult>> ProbeAsync(
        ConversionInputRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the source into intermediates under <see cref="ConversionInputRequest.IntermediateDirectory"/>.
    /// Must not mutate the source file. Do not return Conversion-layer document types.
    /// </summary>
    Task<PluginOperationResult<ConversionInputResult>> ReadAsync(
        ConversionInputRequest request,
        CancellationToken cancellationToken);
}

public sealed record ConversionInputRequest(
    string SourcePath,
    IReadOnlyDictionary<string, string> Options,
    string IntermediateDirectory);

public sealed record ConversionInputProbeResult(
    bool IsSupported,
    string? DetectedFormat = null,
    IReadOnlyList<string>? Warnings = null);

public sealed record ConversionInputResult(
    string IntermediateDirectory,
    string? DetectedFormat = null,
    PluginBookMetadataSnapshot? Metadata = null,
    IReadOnlyList<string>? Warnings = null);
