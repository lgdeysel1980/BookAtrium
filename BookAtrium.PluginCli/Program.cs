using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using BookAtrium.PluginContracts;
using BookAtrium.PluginContracts.Packaging;

namespace BookAtrium.PluginCli;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            PrintHelp();
            return 0;
        }

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "new" => await CmdNew(args.Skip(1).ToArray()),
                "run" => CmdRun(args.Skip(1).ToArray()),
                "test" => CmdTest(args.Skip(1).ToArray()),
                "validate" => CmdValidate(args.Skip(1).ToArray()),
                "pack" => await CmdPack(args.Skip(1).ToArray()),
                "prepare-release" => await CmdPrepareRelease(args.Skip(1).ToArray()),
                _ => Fail($"Unknown command '{args[0]}'. Run bookatrium-plugin help.")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("error: " + ex.Message);
            return 1;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            bookatrium-plugin — BookAtrium Plugin SDK 2.0 tooling

            Commands:
              new <type> [--name ...] [--publisher ...] [--id ...] [--dir ...]
              run [--project <path>]
              test [--project <path>]
              validate [--project <path>]
              pack [--project <path>] [--out <dir>]
              prepare-release [--project <path>] [--out <dir>]

            Types: store, metadata-source, metadata-reader, metadata-writer,
                   input-converter, output-converter, device
            """);
    }

    private static async Task<int> CmdNew(string[] args)
    {
        if (args.Length == 0)
            return Fail("Usage: bookatrium-plugin new <type> --name \"My Plugin\" --publisher \"Me\"");

        var type = args[0].ToLowerInvariant();
        var name = GetOpt(args, "--name") ?? "My Plugin";
        var publisher = GetOpt(args, "--publisher") ?? "Example Developer";
        var id = GetOpt(args, "--id") ?? $"com.example.{Slug(name)}";
        var dir = GetOpt(args, "--dir") ?? Path.Combine(Environment.CurrentDirectory, Slug(name));

        var templateRoot = FindTemplatesRoot();
        var templateName = type switch
        {
            "store" => "store",
            "metadata-source" => "metadata-source",
            "metadata-reader" => "metadata-reader",
            "metadata-writer" => "metadata-writer",
            "input-converter" or "conversion-input" => "conversion-input",
            "output-converter" or "conversion-output" => "conversion-output",
            "device" => "device",
            _ => null
        };
        if (templateName is null)
            return Fail($"Unknown plugin type '{type}'.");

        var source = Path.Combine(templateRoot, templateName);
        if (!Directory.Exists(source))
            return Fail($"Template not found: {source}");

        Directory.CreateDirectory(dir);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(source, file);
            var destRel = rel
                .Replace("Plugin.cs", SlugPascal(name) + "Plugin.cs", StringComparison.OrdinalIgnoreCase)
                .Replace("Plugin.csproj", Path.GetFileName(dir) + ".csproj", StringComparison.OrdinalIgnoreCase);
            var dest = Path.Combine(dir, destRel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            var text = await File.ReadAllTextAsync(file);
            text = text
                .Replace("{{PluginName}}", name)
                .Replace("{{Publisher}}", publisher)
                .Replace("{{PluginId}}", id)
                .Replace("{{ClassName}}", SpanPascal(name) + "Plugin");
            await File.WriteAllTextAsync(dest, text);
        }

        Console.WriteLine($"Created {type} plugin at {dir}");
        Console.WriteLine("Next: edit the plugin class, then: bookatrium-plugin pack");
        return 0;
    }

    private static int CmdRun(string[] args)
    {
        var project = ResolveProject(GetOpt(args, "--project"));
        Console.WriteLine("Plugin development mode:");
        Console.WriteLine($"  1. Build: dotnet build \"{project}\"");
        Console.WriteLine("  2. Launch BookAtrium with:");
        Console.WriteLine($"     BOOKATRIUM_PLUGIN_DEV_PATH=\"{Path.GetDirectoryName(project)}\"");
        Console.WriteLine("Dev load skips packaging; restart the app after rebuilds (hot reload when available).");
        return 0;
    }

    private static int CmdTest(string[] args)
    {
        var project = ResolveProject(GetOpt(args, "--project"));
        var dir = Path.GetDirectoryName(project)!;
        var testProj = Directory.GetFiles(dir, "*.Tests.csproj", SearchOption.AllDirectories).FirstOrDefault()
                       ?? Directory.GetFiles(Path.GetDirectoryName(dir)!, Path.GetFileNameWithoutExtension(project) + ".Tests.csproj", SearchOption.AllDirectories).FirstOrDefault();
        if (testProj is null)
        {
            Console.WriteLine("No test project found. Running validate instead.");
            return CmdValidate(args);
        }

        return RunDotNet("test", testProj);
    }

    private static int CmdValidate(string[] args)
    {
        var project = ResolveProject(GetOpt(args, "--project"));
        var build = RunDotNet("build", project, "-c", "Release");
        if (build != 0)
            return build;

        var plugin = LoadPluginFromProject(project);
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(plugin.Info.Id))
            issues.Add("BA1001: Plugin ID is missing");
        if (!PluginId.IsValid(plugin.Info.Id))
            issues.Add("BA1001: Plugin ID is invalid");
        if (!PluginSemanticVersion.TryParse(plugin.Info.Version, out _))
            issues.Add("BA1003: Plugin version must use semantic versioning");

        var type = PluginManifestGenerator.ResolveType(plugin);
        if (type is PluginType.Store or PluginType.MetadataSource)
        {
            // networked types should declare hosts when they intend remote calls; warn only if empty for store with "amazon" in id
            if (plugin.Info.Id.Contains("amazon", StringComparison.OrdinalIgnoreCase) && plugin.NetworkHosts.Count == 0)
                issues.Add("BA1002: Amazon store must declare allowed network hosts");
        }

        if (issues.Count > 0)
        {
            foreach (var issue in issues)
                Console.Error.WriteLine(issue);
            return 1;
        }

        Console.WriteLine("::notice::validate OK — " + plugin.Info.Id + " " + plugin.Info.Version);
        return 0;
    }

    private static async Task<int> CmdPack(string[] args)
    {
        var project = ResolveProject(GetOpt(args, "--project"));
        var outDir = GetOpt(args, "--out") ?? Path.Combine(Path.GetDirectoryName(project)!, "artifacts");
        Directory.CreateDirectory(outDir);

        if (RunDotNet("build", project, "-c", "Release") != 0)
            return 1;

        var plugin = LoadPluginFromProject(project);
        var dll = FindBuiltDll(project);
        var entryAsm = Path.GetFileName(dll);
        var entryType = plugin.GetType().FullName!;
        var manifest = PluginManifestGenerator.FromPlugin(plugin, entryAsm, entryType);

        var stage = Path.Combine(Path.GetTempPath(), "ba-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(stage, "plugin.json"),
                JsonSerializer.Serialize(manifest, JsonOptions));

            Directory.CreateDirectory(Path.Combine(stage, "lib"));
            File.Copy(dll, Path.Combine(stage, "lib", entryAsm), overwrite: true);

            // Copy non-shared dependencies alongside the plugin (not PluginContracts — host shares it).
            var buildDir = Path.GetDirectoryName(dll)!;
            foreach (var dep in Directory.GetFiles(buildDir, "*.dll"))
            {
                var name = Path.GetFileName(dep);
                if (name.Equals("BookAtrium.PluginContracts.dll", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(entryAsm, StringComparison.OrdinalIgnoreCase))
                    continue;
                File.Copy(dep, Path.Combine(stage, "lib", name), overwrite: true);
            }

            CopyIfExists(Path.Combine(Path.GetDirectoryName(project)!, "README.md"), Path.Combine(stage, "README.md"));
            CopyIfExists(Path.Combine(Path.GetDirectoryName(project)!, "LICENSE"), Path.Combine(stage, "LICENSE"));

            var packageName = $"{plugin.Info.Id}-{plugin.Info.Version}.bookplugin";
            var packagePath = Path.Combine(outDir, packageName);
            if (File.Exists(packagePath))
                File.Delete(packagePath);
            ZipFile.CreateFromDirectory(stage, packagePath, CompressionLevel.Optimal, includeBaseDirectory: false);

            var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(packagePath))).ToLowerInvariant();
            await File.WriteAllTextAsync(packagePath + ".sha256", hash + "  " + packageName + Environment.NewLine);

            Console.WriteLine("Packed: " + packagePath);
            Console.WriteLine("SHA-256: " + hash);
            return 0;
        }
        finally
        {
            try { Directory.Delete(stage, recursive: true); } catch { /* ignore */ }
        }
    }

    private static async Task<int> CmdPrepareRelease(string[] args)
    {
        var project = ResolveProject(GetOpt(args, "--project"));
        var outDir = GetOpt(args, "--out") ?? Path.Combine(Path.GetDirectoryName(project)!, "artifacts");
        if (await CmdPack(new[] { "--project", project, "--out", outDir }) != 0)
            return 1;

        var plugin = LoadPluginFromProject(project);
        var package = Directory.GetFiles(outDir, "*.bookplugin").OrderByDescending(File.GetLastWriteTimeUtc).First();
        var bytes = await File.ReadAllBytesAsync(package);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var fileName = Path.GetFileName(package);

        var entry = new Dictionary<string, object?>
        {
            ["id"] = plugin.Info.Id,
            ["name"] = plugin.Info.Name,
            ["summary"] = plugin.Info.Description ?? plugin.Info.Name,
            ["description"] = plugin.Info.Description ?? plugin.Info.Name,
            ["publisher"] = new Dictionary<string, object?>
            {
                ["name"] = plugin.Info.Publisher,
                ["githubLogin"] = null,
                ["verified"] = false
            },
            ["pluginType"] = PluginManifestGenerator.ResolveType(plugin).ToString(),
            ["version"] = plugin.Info.Version,
            ["releaseDateUtc"] = null,
            ["minimumAppVersion"] = plugin.Info.MinimumAppVersion,
            ["maximumAppVersion"] = null,
            ["pluginApiVersion"] = PluginApiVersion.Current,
            ["supportedPlatforms"] = new[] { "windows-x64", "any" },
            ["repositoryUrl"] = "https://github.com/OWNER/REPO",
            ["supportUrl"] = plugin.Info.SupportUrl ?? "https://github.com/OWNER/REPO/issues",
            ["documentationUrl"] = plugin.Info.Homepage,
            ["license"] = new Dictionary<string, object?>
            {
                ["spdxId"] = plugin.Info.License,
                ["url"] = null
            },
            ["package"] = new Dictionary<string, object?>
            {
                ["downloadUrl"] = "https://github.com/OWNER/REPO/releases/download/v" + plugin.Info.Version + "/" + fileName,
                ["fileName"] = fileName,
                ["sizeBytes"] = bytes.LongLength,
                ["sha256"] = hash
            },
            ["capabilities"] = PluginManifestGenerator.FromPlugin(plugin, "x.dll", "x").Capabilities,
            ["networkHosts"] = plugin.NetworkHosts.ToArray(),
            ["requiresRestart"] = false,
            ["homepageUrl"] = plugin.Info.Homepage,
            ["changelogUrl"] = null,
            ["deprecated"] = false,
            ["blocked"] = false,
            ["blockedVersions"] = Array.Empty<string>(),
            ["uninstallPluginIds"] = Array.Empty<string>(),
            ["tags"] = Array.Empty<string>()
        };

        await File.WriteAllTextAsync(Path.Combine(outDir, "registry-entry.json"), JsonSerializer.Serialize(entry, JsonOptions));
        await File.WriteAllTextAsync(Path.Combine(outDir, "release-notes.md"),
            $"# {plugin.Info.Name} {plugin.Info.Version}\n\n- \n");
        await File.WriteAllTextAsync(Path.Combine(outDir, "publication-checklist.md"),
            """
            # Publication checklist

            - [ ] Review registry-entry.json download URL
            - [ ] Confirm SHA-256 matches the package
            - [ ] Confirm license and homepage
            - [ ] Open PR against BookAtrium-Community-Plugins
            - [ ] Do not claim BookAtrium support for third-party plugins
            """);

        Console.WriteLine("Release artifacts ready in " + outDir);
        return 0;
    }

    private static BookAtriumPlugin LoadPluginFromProject(string project)
    {
        var dll = FindBuiltDll(project);
        var asm = Assembly.LoadFrom(dll);
        var type = asm.GetTypes().FirstOrDefault(t =>
            t is { IsPublic: true, IsAbstract: false } &&
            typeof(BookAtriumPlugin).IsAssignableFrom(t) &&
            t.GetConstructor(Type.EmptyTypes) is not null)
            ?? throw new InvalidOperationException("No public BookAtriumPlugin subclass with a parameterless constructor was found.");
        return (BookAtriumPlugin)Activator.CreateInstance(type)!;
    }

    private static string FindBuiltDll(string project)
    {
        var name = ResolveAssemblyName(project);
        var dir = Path.GetDirectoryName(project)!;
        var candidates = Directory.GetFiles(Path.Combine(dir, "bin"), name + ".dll", SearchOption.AllDirectories)
            .Where(p => p.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        || p.Contains($"{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();
        return candidates.FirstOrDefault()
               ?? throw new FileNotFoundException("Built plugin DLL not found. Build the project first.");
    }

    private static string ResolveAssemblyName(string project)
    {
        var document = XDocument.Load(project);
        var assemblyName = document
            .Descendants("AssemblyName")
            .Select(e => e.Value.Trim())
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        return assemblyName ?? Path.GetFileNameWithoutExtension(project);
    }

    private static string ResolveProject(string? explicitPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
            return Path.GetFullPath(explicitPath);
        var here = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj");
        if (here.Length == 1)
            return here[0];
        throw new InvalidOperationException("Specify --project <path.csproj>");
    }

    private static string FindTemplatesRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "samples", "templates", "sdk2");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate samples/templates/sdk2");
    }

    private static int RunDotNet(params string[] args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in args)
            psi.ArgumentList.Add(a);
        using var p = System.Diagnostics.Process.Start(psi)!;
        Console.Write(p.StandardOutput.ReadToEnd());
        Console.Error.Write(p.StandardError.ReadToEnd());
        p.WaitForExit();
        return p.ExitCode;
    }

    private static string? GetOpt(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("error: " + message);
        return 1;
    }

    private static void CopyIfExists(string src, string dest)
    {
        if (File.Exists(src))
            File.Copy(src, dest, overwrite: true);
    }

    private static string Slug(string name)
    {
        var sb = new StringBuilder();
        foreach (var ch in name.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (ch is ' ' or '-' or '_')
                sb.Append('-');
        }

        return sb.ToString().Trim('-');
    }

    private static string SlugPascal(string name) => SpanPascal(name);

    private static string SpanPascal(string name)
    {
        var parts = name.Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }
}
