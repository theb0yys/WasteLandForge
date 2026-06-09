namespace WastelandForge.Core;

public sealed record DiagnosticIssue
{
    public DiagnosticIssue(
        RuleId ruleId,
        DiagnosticSeverity severity,
        string category,
        string title,
        string message,
        SourceLocation primaryLocation,
        LogicalId? projectId = null,
        IReadOnlyList<SourceLocation>? relatedLocations = null,
        string? suggestedFix = null,
        Uri? docsUri = null,
        string? fingerprint = null)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Diagnostics require a category.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Diagnostics require a title.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Diagnostics require a message.", nameof(message));
        }

        RuleId = ruleId;
        Severity = severity;
        Category = category;
        Title = title;
        Message = message;
        ProjectId = projectId;
        PrimaryLocation = primaryLocation;
        RelatedLocations = relatedLocations ?? [];
        SuggestedFix = suggestedFix;
        DocsUri = docsUri;
        Fingerprint = fingerprint;
    }

    public RuleId RuleId { get; }

    public DiagnosticSeverity Severity { get; }

    public string Category { get; }

    public string Title { get; }

    public string Message { get; }

    public LogicalId? ProjectId { get; }

    public SourceLocation PrimaryLocation { get; }

    public IReadOnlyList<SourceLocation> RelatedLocations { get; }

    public string? SuggestedFix { get; }

    public Uri? DocsUri { get; }

    public string? Fingerprint { get; }
}
