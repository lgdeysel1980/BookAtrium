namespace BookAtrium.PluginContracts;

/// <summary>Writes a conversion intermediate directory to a concrete output format.</summary>
public interface IConversionOutputPlugin : IBookAtriumPlugin
{
    /// <summary>Output format identifier (e.g. "epub", "pdf").</summary>
    string OutputFormat { get; }

    /// <summary>
    /// Writes output to an application-controlled temporary <see cref="ConversionOutputRequest.OutputPath"/>.
    /// The application owns final destination placement and library import.
    /// </summary>
    Task<PluginOperationResult<ConversionOutputResult>> WriteAsync(
        ConversionOutputRequest request,
        CancellationToken cancellationToken);
}

public sealed record ConversionOutputRequest(
    string IntermediateDirectory,
    string OutputPath,
    IReadOnlyDictionary<string, string> Options);

public sealed record ConversionOutputResult(
    string OutputPath,
    IReadOnlyList<string>? Warnings = null);
