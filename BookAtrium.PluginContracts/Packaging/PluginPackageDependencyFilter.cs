namespace BookAtrium.PluginContracts.Packaging;

/// <summary>
/// Decides which build-output files belong in an external plugin package.
/// Host-owned BookAtrium assemblies, PluginContracts, FirstParty shared DLLs, and
/// host native runtimes are never packaged. Genuine third-party plugin dependencies are.
/// </summary>
public static class PluginPackageDependencyFilter
{
    private static readonly HashSet<string> NativeRuntimeFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome.exe",
        "chrome.dll",
        "chrome_elf.dll",
        "libEGL.dll",
        "libGLESv2.dll",
        "d3dcompiler_47.dll",
        "vk_swiftshader.dll",
        "vulkan-1.dll",
        "icudtl.dat",
        "v8_context_snapshot.bin",
        "snapshot_blob.bin",
        "resources.pak",
        "chrome_100_percent.pak",
        "chrome_200_percent.pak"
    };

    /// <summary>
    /// Returns whether <paramref name="filePath"/> should be copied into <c>lib/</c>.
    /// Only the top-level build directory should be scanned — never <c>runtimes/</c>.
    /// </summary>
    public static bool ShouldInclude(string filePath, string? entryAssemblyFileName, bool includePdb = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var name = Path.GetFileName(filePath);
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (IsEntryAssembly(name, entryAssemblyFileName))
            return true;

        if (includePdb &&
            !string.IsNullOrWhiteSpace(entryAssemblyFileName) &&
            name.Equals(Path.ChangeExtension(entryAssemblyFileName, ".pdb"), StringComparison.OrdinalIgnoreCase))
            return true;

        if (name.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase))
            return false;

        if (IsHostOwnedNativeRuntime(name))
            return false;

        if (IsForbiddenHostOrContractsAssembly(name, entryAssemblyFileName))
            return false;

        var ext = Path.GetExtension(name);
        if (ext.Equals(".dll", StringComparison.OrdinalIgnoreCase))
            return true;

        if (includePdb && ext.Equals(".pdb", StringComparison.OrdinalIgnoreCase))
            return !IsForbiddenHostOrContractsAssembly(Path.ChangeExtension(name, ".dll"), entryAssemblyFileName);

        return false;
    }

    /// <summary>
    /// Selects packageable files from a project build output directory (top-level only).
    /// </summary>
    public static IReadOnlyList<string> SelectFiles(
        string gatherRoot,
        string? entryAssemblyFileName,
        bool includePdb = false)
    {
        if (string.IsNullOrWhiteSpace(gatherRoot) || !Directory.Exists(gatherRoot))
            return Array.Empty<string>();

        return Directory.EnumerateFiles(gatherRoot, "*", SearchOption.TopDirectoryOnly)
            .Where(path => ShouldInclude(path, entryAssemblyFileName, includePdb))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool IsHostOwnedNativeRuntime(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (NativeRuntimeFileNames.Contains(name))
            return true;
        if (name.StartsWith("chrome", StringComparison.OrdinalIgnoreCase))
            return true;
        if (name.StartsWith("pdfium", StringComparison.OrdinalIgnoreCase))
            return true;
        if (name.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public static bool IsForbiddenHostOrContractsAssembly(string fileName, string? entryAssemblyFileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (IsEntryAssembly(name, entryAssemblyFileName))
            return false;

        var assemblyName = Path.GetFileNameWithoutExtension(name);
        if (assemblyName.Equals("BookAtrium.PluginContracts", StringComparison.OrdinalIgnoreCase))
            return true;
        if (assemblyName.Equals("BookAtrium", StringComparison.OrdinalIgnoreCase) ||
            assemblyName.StartsWith("BookAtrium.", StringComparison.OrdinalIgnoreCase))
            return true;
        if (assemblyName.Equals("BookApplication", StringComparison.OrdinalIgnoreCase) ||
            assemblyName.StartsWith("BookApplication.", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    /// <summary>Resolves <c>plugin.json</c> <c>entryAssembly</c> against package root or <c>lib/</c>.</summary>
    public static string? ResolveEntryAssemblyPath(string packageRoot, string? entryAssembly)
    {
        if (string.IsNullOrWhiteSpace(packageRoot) || string.IsNullOrWhiteSpace(entryAssembly))
            return null;

        var candidates = new[]
        {
            Path.Combine(packageRoot, entryAssembly),
            Path.Combine(packageRoot, "lib", Path.GetFileName(entryAssembly)),
            Path.Combine(packageRoot, Path.GetFileName(entryAssembly))
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static bool IsEntryAssembly(string fileName, string? entryAssemblyFileName) =>
        !string.IsNullOrWhiteSpace(entryAssemblyFileName) &&
        fileName.Equals(Path.GetFileName(entryAssemblyFileName), StringComparison.OrdinalIgnoreCase);
}
