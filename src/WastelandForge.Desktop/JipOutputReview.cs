using System.IO;
using System.Text;

namespace WastelandForge.Desktop;

internal sealed record JipReviewedScript(string Name, string Path, string Content);
internal sealed record JipOutputReviewResult(bool Success, string Message, IReadOnlyList<JipReviewedScript> Scripts, string? PackageFolder);

internal static class JipOutputReview
{
    public static JipOutputReviewResult Read(string projectRoot)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot);
            var scriptRoot = Path.GetFullPath(Path.Combine(root, "generated", "jip-scripts", "nvse", "plugins", "scripts"));
            var package = Path.GetFullPath(Path.Combine(root, "dist", "jip-scripts", "package"));
            if (!IsUnder(scriptRoot, root) || !IsUnder(package, root)) return new(false, "JIP output paths escaped the project root.", [], null);
            if (!Directory.Exists(scriptRoot)) return new(false, "No generated JIP scripts were found. Generate or package the project first.", [], Directory.Exists(package) ? package : null);

            var scripts = Directory.EnumerateFiles(scriptRoot, "*.txt", SearchOption.TopDirectoryOnly)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .Select(path => new JipReviewedScript(Path.GetFileName(path), path, File.ReadAllText(path)))
                .ToArray();
            return scripts.Length == 0
                ? new(false, "The generated JIP script folder contains no .txt scripts.", [], Directory.Exists(package) ? package : null)
                : new(true, $"Loaded {scripts.Length} generated JIP script(s).", scripts, Directory.Exists(package) ? package : null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new(false, "JIP output review failed: " + ex.Message, [], null);
        }
    }

    public static string Render(JipOutputReviewResult result)
    {
        var builder = new StringBuilder();
        foreach (var script in result.Scripts)
        {
            if (builder.Length > 0) builder.AppendLine().AppendLine();
            builder.Append("== ").Append(script.Name).AppendLine(" ==").Append(script.Content);
        }
        return builder.ToString();
    }

    private static bool IsUnder(string path, string parent)
    {
        var normalizedParent = parent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return path.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith(normalizedParent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
