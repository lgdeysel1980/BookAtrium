using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace BookAtrium.PluginContracts.Packaging;

/// <summary>Stages, zips, and hashes a plugin package. Used by the CLI and PluginPackager.</summary>
public static class PluginPackageBuilder
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void WritePluginJson(string stageRoot, PluginPackageManifest manifest)
    {
        Directory.CreateDirectory(stageRoot);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(Path.Combine(stageRoot, "plugin.json"), json);
    }

    public static void StageLibFromBuildOutput(
        string gatherRoot,
        string stageRoot,
        string entryAssemblyFileName,
        bool includePdb = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gatherRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(stageRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryAssemblyFileName);

        var libDir = Path.Combine(stageRoot, "lib");
        Directory.CreateDirectory(libDir);

        var files = PluginPackageDependencyFilter.SelectFiles(gatherRoot, entryAssemblyFileName, includePdb);
        if (files.Count == 0)
            throw new InvalidOperationException($"No packageable assemblies found in '{gatherRoot}'.");

        foreach (var file in files)
            File.Copy(file, Path.Combine(libDir, Path.GetFileName(file)), overwrite: true);
    }

    public static void StageOptionalExtras(string sourceDir, string stageRoot)
    {
        if (string.IsNullOrWhiteSpace(sourceDir) || !Directory.Exists(sourceDir))
            return;

        foreach (var extra in new[] { "README.md", "LICENSE.txt", "LICENSE", "icon.png" })
        {
            var src = Path.Combine(sourceDir, extra);
            if (File.Exists(src))
                File.Copy(src, Path.Combine(stageRoot, extra), overwrite: true);
        }
    }

    public static void ValidateStagedPackage(string stageRoot, PluginPackageManifest manifest)
    {
        if (!File.Exists(Path.Combine(stageRoot, "plugin.json")))
            throw new InvalidOperationException("Package is missing plugin.json.");

        var entryPath = PluginPackageDependencyFilter.ResolveEntryAssemblyPath(stageRoot, manifest.EntryAssembly);
        if (entryPath is null)
            throw new InvalidOperationException($"Package is missing entry assembly '{manifest.EntryAssembly}'.");

        foreach (var noise in new[] { "obj", "bin", ".git" })
        {
            if (Directory.Exists(Path.Combine(stageRoot, noise)))
                throw new InvalidOperationException($"Package stage unexpectedly contains '{noise}'.");
        }
    }

    public static PluginPackageBuildResult CreatePackage(string stageRoot, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stageRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDir))
            Directory.CreateDirectory(outputDir);

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        CreateDeterministicZip(stageRoot, outputPath);
        var hex = WriteSha256Sidecar(outputPath);
        return new PluginPackageBuildResult(outputPath, hex, outputPath + ".sha256");
    }

    public static void CreateDeterministicZip(string stageRoot, string outputPath)
    {
        var files = Directory.EnumerateFiles(stageRoot, "*", SearchOption.AllDirectories)
            .Select(path => new
            {
                FullPath = path,
                EntryName = Path.GetRelativePath(stageRoot, path).Replace('\\', '/')
            })
            .OrderBy(x => x.EntryName, StringComparer.Ordinal)
            .ToList();

        using var archiveStream = File.Create(outputPath);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: false);
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(file.EntryName, CompressionLevel.Optimal);
            entry.LastWriteTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            using var entryStream = entry.Open();
            using var source = File.OpenRead(file.FullPath);
            source.CopyTo(entryStream);
        }
    }

    public static string WriteSha256Sidecar(string outputPath)
    {
        using var stream = File.OpenRead(outputPath);
        var hash = SHA256.HashData(stream);
        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        var sidecar = outputPath + ".sha256";
        var line = $"{hex}  {Path.GetFileName(outputPath)}\n";
        File.WriteAllText(sidecar, line, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return hex;
    }

    public static string FindBuiltDll(string projectPath, string? configuration = null)
    {
        var name = ResolveAssemblyName(projectPath);
        var dir = Path.GetDirectoryName(Path.GetFullPath(projectPath))!;
        var bin = Path.Combine(dir, "bin");
        if (!Directory.Exists(bin))
            throw new FileNotFoundException("Built plugin DLL not found. Build the project first.", projectPath);

        var candidates = Directory.GetFiles(bin, name + ".dll", SearchOption.AllDirectories)
            .Where(p =>
                p.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
                p.Contains($"{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(configuration))
        {
            candidates = candidates.Where(p =>
                p.Contains($"{Path.DirectorySeparatorChar}{configuration}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        }

        return candidates.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()
               ?? throw new FileNotFoundException("Built plugin DLL not found. Build the project first.", projectPath);
    }

    public static string ResolveAssemblyName(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var assemblyName = document
            .Descendants("AssemblyName")
            .Select(e => e.Value.Trim())
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        return assemblyName ?? Path.GetFileNameWithoutExtension(projectPath);
    }

    public static string? ExtractTargetFramework(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var tfm = document.Descendants("TargetFramework").Select(e => e.Value.Trim()).FirstOrDefault()
                  ?? document.Descendants("TargetFrameworks").Select(e => e.Value.Trim()).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tfm))
            return null;
        if (tfm.Contains(';'))
        {
            var parts = tfm.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 1)
            {
                throw new InvalidOperationException(
                    "Project specifies multiple TargetFrameworks. Plugin packaging requires a single target framework.");
            }

            return parts[0];
        }

        return tfm;
    }
}

/// <summary>Result of creating a <c>.bookplugin</c> archive and SHA-256 sidecar.</summary>
public sealed record PluginPackageBuildResult(string PackagePath, string Sha256Hex, string Sha256SidecarPath);
