using System.IO;
using System.Text;

namespace WastelandForge.Desktop;

internal sealed record XEditAuditReviewedFile(string RelativePath, string FullPath, string Content);
internal sealed record XEditAuditReviewResult(bool Success, string Message, string? OutputRoot, IReadOnlyList<XEditAuditReviewedFile> Files);

internal static class XEditAuditOutputReview
{
    public static XEditAuditReviewResult Read(string projectRoot)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot);
            var outputRoot = Path.GetFullPath(Path.Combine(root, "generated", "xedit-audit"));
            if (!outputRoot.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return new(false, "Generated audit root escaped the project.", null, []);
            if (!Directory.Exists(outputRoot)) return new(false, "No generated xEdit audit evidence exists.", null, []);
            var files = Directory.EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
                .Where(IsReviewable)
                .Select(path => new XEditAuditReviewedFile(Path.GetRelativePath(outputRoot, path).Replace('\\', '/'), path, File.ReadAllText(path)))
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray();
            return new(true, $"Loaded {files.Length} generated evidence file(s).", outputRoot, files);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new(false, "xEdit audit review failed: " + ex.Message, null, []);
        }
    }

    public static string Render(XEditAuditReviewResult result)
    {
        var builder = new StringBuilder();
        foreach (var file in result.Files)
        {
            if (builder.Length > 0) builder.AppendLine().AppendLine();
            builder.Append("== ").Append(file.RelativePath).AppendLine(" ==").Append(file.Content);
        }
        return builder.ToString();
    }

    private static bool IsReviewable(string path) => path.EndsWith(".pas", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
}
