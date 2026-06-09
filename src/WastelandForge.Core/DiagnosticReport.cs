namespace WastelandForge.Core;

public sealed record DiagnosticReport
{
    public DiagnosticReport(LogicalId? projectId, IEnumerable<DiagnosticIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        ProjectId = projectId;
        Issues = issues
            .OrderBy(issue => issue.RuleId.ToString(), StringComparer.Ordinal)
            .ThenBy(issue => issue.PrimaryLocation.File, StringComparer.Ordinal)
            .ThenBy(issue => issue.PrimaryLocation.Pointer?.ToString(), StringComparer.Ordinal)
            .ThenBy(issue => issue.Title, StringComparer.Ordinal)
            .ToArray();
    }

    public LogicalId? ProjectId { get; }

    public IReadOnlyList<DiagnosticIssue> Issues { get; }

    public int ErrorCount => Issues.Count(issue => issue.Severity == DiagnosticSeverity.Error);

    public int WarningCount => Issues.Count(issue => issue.Severity == DiagnosticSeverity.Warning);

    public int NoteCount => Issues.Count(issue => issue.Severity == DiagnosticSeverity.Note);

    public bool HasErrors => Issues.Any(issue => issue.Severity == DiagnosticSeverity.Error);
}
