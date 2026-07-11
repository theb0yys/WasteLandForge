using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WastelandForge.Desktop;

internal sealed record Mo2InstanceCandidate(
    string CandidateId,
    string InstanceKind,
    string InstanceName,
    string? ExecutablePath,
    string ConfigurationPath,
    string? BaseDirectory,
    string? ModsRoot,
    string Status,
    string Reason,
    IReadOnlyList<string> Evidence)
{
    public string Display => $"{InstanceName} [{InstanceKind}] - {Status}: {ModsRoot ?? Reason}";
}

internal sealed record Mo2DiscoveryResult(IReadOnlyList<Mo2InstanceCandidate> Candidates);

internal static partial class Mo2InstanceDiscovery
{
    private const string IniName = "ModOrganizer.ini";

    public static Mo2DiscoveryResult Discover(string? executablePath, string? globalRoot = null, string? gameDataRoot = null)
    {
        var inputs = new List<(string Kind, string Name, string? Executable, string Ini)>();
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            try
            {
                var executable = Path.GetFullPath(executablePath);
                var directory = Path.GetDirectoryName(executable);
                if (directory is not null) inputs.Add(("portable", "Portable", executable, Path.Combine(directory, IniName)));
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException) { }
        }

        var boundedGlobalRoot = globalRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ModOrganizer");
        if (Directory.Exists(boundedGlobalRoot) && !IsReparsePoint(boundedGlobalRoot))
        {
            foreach (var directory in Directory.GetDirectories(boundedGlobalRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ThenBy(path => path, StringComparer.Ordinal))
            {
                if (!IsReparsePoint(directory)) inputs.Add(("global", Path.GetFileName(directory), null, Path.Combine(directory, IniName)));
            }
        }

        var candidates = inputs.Where(input => File.Exists(input.Ini))
            .Select(input => Parse(input.Ini, input.Kind, input.Name, input.Executable, gameDataRoot))
            .GroupBy(candidate => candidate.ConfigurationPath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(candidate => candidate.InstanceKind == "portable" ? 0 : 1).First())
            .OrderBy(candidate => candidate.ConfigurationPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.ConfigurationPath, StringComparer.Ordinal)
            .ToArray();
        return new(candidates);
    }

    internal static Mo2InstanceCandidate Parse(string iniPath, string kind, string name, string? executablePath = null, string? gameDataRoot = null)
    {
        var fullIni = Path.GetFullPath(iniPath);
        var evidence = new List<string> { $"configuration={fullIni}", $"instanceKind={kind}" };
        try
        {
            var values = ReadSettings(fullIni);
            var iniRoot = Path.GetDirectoryName(fullIni)!;
            var baseValue = values.GetValueOrDefault("base_directory", iniRoot);
            var baseDirectory = ResolvePath(baseValue, iniRoot, iniRoot);
            var modsValue = values.GetValueOrDefault("mod_directory", "%BASE_DIR%/mods");
            if (UnknownToken().IsMatch(modsValue.Replace("%BASE_DIR%", string.Empty, StringComparison.Ordinal)))
                return Candidate("unsupported", "The mods directory contains an unsupported expansion token.", null, null);
            var modsRoot = ResolvePath(modsValue.Replace("%BASE_DIR%", baseDirectory, StringComparison.Ordinal), iniRoot, baseDirectory);
            evidence.Add($"baseDirectory={baseDirectory}");
            evidence.Add($"modsRoot={modsRoot}");
            var invalidReason = ValidateModsRoot(modsRoot, gameDataRoot);
            return invalidReason is null ? Candidate("candidate", "Candidate requires explicit user confirmation.", baseDirectory, modsRoot) : Candidate("invalid", invalidReason, baseDirectory, modsRoot);

            Mo2InstanceCandidate Candidate(string status, string reason, string? baseDir, string? mods) =>
                new(fullIni, kind, name, executablePath, fullIni, baseDir, mods, status, reason, evidence);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DecoderFallbackException or InvalidDataException or ArgumentException or NotSupportedException or PathTooLongException)
        {
            return new(fullIni, kind, name, executablePath, fullIni, null, null, "invalid", ex.Message, evidence);
        }
    }

    internal static string? ValidateModsRoot(string? path, string? gameDataRoot = null)
    {
        if (string.IsNullOrWhiteSpace(path)) return "The mods directory is empty.";
        if (!Directory.Exists(path)) return "The mods directory does not exist.";
        if (IsReparsePoint(path)) return "The mods directory is a reparse point.";
        if (path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains("overwrite", StringComparer.OrdinalIgnoreCase)) return "The mods directory resolves through an Overwrite path.";
        if (!string.IsNullOrWhiteSpace(gameDataRoot) && IsWithin(Path.GetFullPath(path), Path.GetFullPath(gameDataRoot))) return "The mods directory resolves inside game Data.";
        return null;
    }

    private static Dictionary<string, string> ReadSettings(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Contains((byte)0)) throw new InvalidDataException("The MO2 INI contains NUL content.");
        var text = new UTF8Encoding(false, true).GetString(bytes);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var inSettings = false;
        foreach (var raw in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#')) continue;
            if (line.StartsWith('[') && line.EndsWith(']')) { inSettings = line[1..^1].Equals("Settings", StringComparison.OrdinalIgnoreCase); continue; }
            if (!inSettings) continue;
            var equals = line.IndexOf('=');
            if (equals <= 0) throw new InvalidDataException("Malformed line in MO2 [Settings] section.");
            var key = line[..equals].Trim();
            if (!key.Equals("base_directory", StringComparison.OrdinalIgnoreCase) && !key.Equals("mod_directory", StringComparison.OrdinalIgnoreCase)) continue;
            if (!values.TryAdd(key, line[(equals + 1)..].Trim())) throw new InvalidDataException($"Duplicate MO2 setting '{key}'.");
        }
        return values;
    }

    private static string ResolvePath(string value, string relativeRoot, string baseDirectory)
    {
        var expanded = value.Replace("%BASE_DIR%", baseDirectory, StringComparison.Ordinal);
        return Path.GetFullPath(Path.IsPathRooted(expanded) ? expanded : Path.Combine(relativeRoot, expanded));
    }

    private static bool IsReparsePoint(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    private static bool IsWithin(string path, string parent) { var prefix = parent.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; return path.Equals(parent, StringComparison.OrdinalIgnoreCase) || path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase); }

    [GeneratedRegex("%[^%]+%", RegexOptions.CultureInvariant)]
    private static partial Regex UnknownToken();
}
