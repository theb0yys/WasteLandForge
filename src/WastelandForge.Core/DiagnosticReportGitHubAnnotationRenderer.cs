using System.Text;

namespace WastelandForge.Core;

public static class DiagnosticReportGitHubAnnotationRenderer
{
    public static string Render(DiagnosticReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.Issues.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var issue in report.Issues)
        {
            builder.Append("::");
            builder.Append(ToCommand(issue.Severity));
            builder.Append(" file=");
            builder.Append(EscapeProperty(ToGitHubPath(issue.PrimaryLocation.File)));
            if (issue.PrimaryLocation.Line is not null)
            {
                builder.Append(",line=");
                builder.Append(issue.PrimaryLocation.Line.Value);
            }

            if (issue.PrimaryLocation.Column is not null)
            {
                builder.Append(",col=");
                builder.Append(issue.PrimaryLocation.Column.Value);
            }

            builder.Append(",title=");
            builder.Append(EscapeProperty($"{issue.RuleId} {issue.Title}"));
            builder.Append("::");
            builder.AppendLine(EscapeMessage(CreateMessage(issue)));
        }

        return builder.ToString();
    }

    private static string ToCommand(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Error => "error",
        DiagnosticSeverity.Warning => "warning",
        DiagnosticSeverity.Note => "notice",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private static string CreateMessage(DiagnosticIssue issue)
    {
        var builder = new StringBuilder(issue.Message);
        if (issue.PrimaryLocation.Pointer is not null)
        {
            builder.Append(" Location: ");
            builder.Append(issue.PrimaryLocation.File);
            builder.Append('#');
            builder.Append(issue.PrimaryLocation.Pointer);
        }

        if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
        {
            builder.Append(" Fix: ");
            builder.Append(issue.SuggestedFix);
        }

        if (issue.DocsUri is not null)
        {
            builder.Append(" Docs: ");
            builder.Append(issue.DocsUri);
        }

        return builder.ToString();
    }

    private static string ToGitHubPath(string file)
    {
        var normalized = file.Replace('\\', '/');
        if (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            return normalized[2..];
        }

        return normalized.TrimStart('/');
    }

    private static string EscapeProperty(string value)
    {
        return EscapeCommandData(value)
            .Replace(":", "%3A", StringComparison.Ordinal)
            .Replace(",", "%2C", StringComparison.Ordinal);
    }

    private static string EscapeMessage(string value)
    {
        return EscapeCommandData(value);
    }

    private static string EscapeCommandData(string value)
    {
        return value
            .Replace("%", "%25", StringComparison.Ordinal)
            .Replace("\r", "%0D", StringComparison.Ordinal)
            .Replace("\n", "%0A", StringComparison.Ordinal);
    }
}
