using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WastelandForge.FakeFNVEdit;

internal static partial class Program
{
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    private static int Main(string[] args)
    {
        try
        {
            var paths = ValidateArguments(args);
            var script = File.ReadAllText(paths.Script, Encoding.UTF8);
            if (!script.Contains("wastelandforge.fnv-game-knowledge-export/0.2.0", StringComparison.Ordinal) ||
                !script.Contains("\"forgeExecutedXEdit\":true", StringComparison.Ordinal))
                throw new InvalidOperationException("The automated 0.2.0 export script was not supplied.");
            var outputMatch = RawOutputRegex().Match(script);
            if (!outputMatch.Success) throw new InvalidOperationException("The script did not declare RawOutputFile.");
            var rawOutput = Path.GetFullPath(outputMatch.Groups[1].Value.Replace("''", "'", StringComparison.Ordinal));
            if (!StringComparer.OrdinalIgnoreCase.Equals(rawOutput, Path.Combine(paths.Run, "raw-export.json")))
                throw new InvalidOperationException("RawOutputFile escaped the approved private run.");

            var plugins = File.ReadAllBytes(paths.PluginList);
            if (!plugins.AsSpan().SequenceEqual(Utf8NoBom.GetBytes("FalloutNV.esm\r\n")))
                throw new InvalidOperationException("The private plugin list was not the exact single-master allowlist.");
            if (!File.Exists(Path.Combine(paths.Data, "FalloutNV.esm")))
                throw new FileNotFoundException("The synthetic FalloutNV.esm was not present in the approved Data directory.");
            if (Directory.EnumerateFileSystemEntries(paths.Backups).Any())
                throw new InvalidOperationException("The private backups directory was not empty before execution.");

            WriteText(rawOutput, CreateSyntheticExport());
            WriteText(paths.Log, "Synthetic Gate 547 FNVEdit provider completed the approved read-only export.\n");
            WriteText(Path.ChangeExtension(paths.PluginList, ".fnvviewsettings"), "synthetic private view settings\n");
            WriteText(Path.Combine(paths.Cache, "synthetic-cache.txt"), "private cache byproduct\n");
            WriteText(Path.Combine(paths.Temp, "synthetic-temp.txt"), "private temp byproduct\n");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static ApprovedPaths ValidateArguments(string[] args)
    {
        if (args.Length != 12) throw new InvalidOperationException($"Expected exactly 12 approved arguments; received {args.Length}.");
        if (!StringComparer.Ordinal.Equals(args[0], "-FNV") ||
            !StringComparer.Ordinal.Equals(args[1], "-view") ||
            !StringComparer.Ordinal.Equals(args[2], "-autoload") ||
            !args[3].StartsWith("-script:", StringComparison.Ordinal) ||
            !StringComparer.Ordinal.Equals(args[4], "-autoexit") ||
            !args[5].StartsWith("-D:", StringComparison.Ordinal) ||
            !args[6].StartsWith("-P:", StringComparison.Ordinal) ||
            !args[7].StartsWith("-S:", StringComparison.Ordinal) ||
            !args[8].StartsWith("-C:", StringComparison.Ordinal) ||
            !args[9].StartsWith("-T:", StringComparison.Ordinal) ||
            !args[10].StartsWith("-B:", StringComparison.Ordinal) ||
            !args[11].StartsWith("-R:", StringComparison.Ordinal))
            throw new InvalidOperationException("The approved argument allowlist or order was not supplied.");

        return new(
            FilePath(args[3][8..]),
            DirectoryPath(args[5][3..]),
            FilePath(args[6][3..]),
            DirectoryPath(args[7][3..]),
            DirectoryPath(args[8][3..]),
            DirectoryPath(args[9][3..]),
            DirectoryPath(args[10][3..]),
            Path.GetFullPath(args[11][3..]));
    }

    private static string FilePath(string path)
    {
        var full = Path.GetFullPath(path);
        if (!File.Exists(full)) throw new FileNotFoundException("Approved input was not found.", full);
        return full;
    }

    private static string DirectoryPath(string path)
    {
        var full = Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (!Directory.Exists(full)) throw new DirectoryNotFoundException("Approved directory was not found: " + full);
        return full;
    }

    private static void WriteText(string path, string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents, Utf8NoBom);
    }

    private static string CreateSyntheticExport()
    {
        var export = new
        {
            formatVersion = "0.2.0",
            kind = "wastelandforge.fnv-game-knowledge-export",
            producer = new { name = "xEdit", gameMode = "FNV", version = "gate-547-synthetic-provider", scriptId = "wastelandforge.fnv-game-knowledge-export/0.2.0" },
            source = new { fileName = "FalloutNV.esm", synthetic = true, usesRealPluginBytes = false },
            completion = new { complete = true, recordsVisited = 1, recordsEmitted = 1, omissions = Array.Empty<string>(), refusals = Array.Empty<string>() },
            records = new[]
            {
                new { sourceFile = "FalloutNV.esm", signature = "CELL", fixedFormId = "00000100", loadOrderFormId = "00000100", editorId = "SyntheticRoadCell", displayName = "Synthetic Road", isDeleted = false,
                    context = new { kind = "cell", interior = false, gridX = 1, gridY = -2,
                        worldspace = new { sourceFile = "FalloutNV.esm", signature = "WRLD", fixedFormId = "00000001", editorId = "SyntheticWorld" } } }
            },
            safety = new { readOnly = true, forgeExecutedXEdit = true, mutatedPlugin = false, wrotePlugin = false, changedLoadOrder = false, wroteGameData = false }
        };
        return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
    }

    [GeneratedRegex("RawOutputFile\\s*=\\s*'((?:''|[^'])+)'\\s*;", RegexOptions.CultureInvariant)]
    private static partial Regex RawOutputRegex();

    private sealed record ApprovedPaths(string Script, string Data, string PluginList, string Run, string Cache, string Temp, string Backups, string Log);
}
