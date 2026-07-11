using System.IO;

namespace WastelandForge.Desktop;

internal sealed record SetupPathIssue(string Message, bool IsInvalid);

internal sealed record SetupPathValidationResult(
    int ReadyCorePaths,
    int ReadyToolPaths,
    IReadOnlyList<SetupPathIssue> Issues,
    IReadOnlyList<string> Warnings)
{
    public bool CanScan => Issues.Count == 0;

    public bool HasInvalidPaths => Issues.Any(issue => issue.IsInvalid);

    public bool HasWarnings => Warnings.Count > 0;
}

internal static class SetupPathValidator
{
    private const int PathPressureWarningLength = 240;

    public static SetupPathValidationResult Validate(LocalAppSettings settings)
    {
        var issues = new List<SetupPathIssue>();
        var warnings = new List<string>();
        var readyCore = 0;
        var readyTools = 0;

        ValidateDirectory("Project root", settings.ProjectRoot, issues, ref readyCore);
        ValidateDirectory("Game root", settings.GameRoot, issues, ref readyCore);
        ValidateDirectory("Data root", settings.DataRoot, issues, ref readyCore);

        ValidateOptionalFile("MO2", settings.Mo2Path, issues, ref readyTools);
        ValidateOptionalDirectory("MO2 mods root", settings.Mo2ModsRoot, settings.DataRoot, issues);
        ValidateOptionalFile("GECK", settings.ToolPaths.GetValueOrDefault("geck", string.Empty), issues, ref readyTools);
        ValidateOptionalFile("xEdit", settings.ToolPaths.GetValueOrDefault("xedit", string.Empty), issues, ref readyTools);

        AddProgramFilesWarning(settings.GameRoot, warnings);
        AddPathPressureWarnings(settings, warnings);

        return new SetupPathValidationResult(readyCore, readyTools, issues, warnings);
    }

    private static void AddProgramFilesWarning(string? gameRoot, ICollection<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(gameRoot))
        {
            return;
        }

        var programRoots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        if (programRoots.Any(root => !string.IsNullOrWhiteSpace(root) && IsUnderDirectory(gameRoot, root)))
        {
            warnings.Add("Game root is under Program Files; Windows protection can interfere with modding tools.");
        }
    }

    private static void AddPathPressureWarnings(LocalAppSettings settings, ICollection<string> warnings)
    {
        var paths = new[]
        {
            ("Project root", settings.ProjectRoot),
            ("Game root", settings.GameRoot),
            ("Data root", settings.DataRoot),
            ("MO2", settings.Mo2Path),
            ("MO2 mods root", settings.Mo2ModsRoot),
            ("GECK", settings.ToolPaths.GetValueOrDefault("geck", string.Empty)),
            ("xEdit", settings.ToolPaths.GetValueOrDefault("xedit", string.Empty))
        };

        foreach (var (label, path) in paths)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.Length >= PathPressureWarningLength)
            {
                warnings.Add($"{label} is {path.Length} characters long and is approaching the legacy 260-character path limit.");
            }
        }
    }

    private static bool IsUnderDirectory(string path, string parent)
    {
        try
        {
            var normalizedPath = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var normalizedParent = Path.GetFullPath(parent)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return normalizedPath.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith(normalizedParent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static void ValidateDirectory(
        string label,
        string? path,
        ICollection<SetupPathIssue> issues,
        ref int ready)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            issues.Add(new SetupPathIssue(label + " is required.", IsInvalid: false));
        }
        else if (!Directory.Exists(path))
        {
            issues.Add(new SetupPathIssue(label + " must be an existing folder.", IsInvalid: true));
        }
        else
        {
            ready++;
        }
    }

    private static void ValidateOptionalFile(
        string label,
        string? path,
        ICollection<SetupPathIssue> issues,
        ref int ready)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!File.Exists(path))
        {
            issues.Add(new SetupPathIssue(label + " must be an existing file.", IsInvalid: true));
        }
        else
        {
            ready++;
        }
    }

    private static void ValidateOptionalDirectory(string label, string? path, string? gameDataRoot, ICollection<SetupPathIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        var reason = Mo2InstanceDiscovery.ValidateModsRoot(path, gameDataRoot);
        if (reason is not null) issues.Add(new SetupPathIssue($"{label} is invalid: {reason}", IsInvalid: true));
    }
}
