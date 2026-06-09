using System.Text.Json;
using System.Text.Json.Serialization;

namespace WastelandForge.Core;

public static class DiagnosticIssueJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(DiagnosticIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return JsonSerializer.Serialize(ToJsonModel(issue), SerializerOptions);
    }

    private static DiagnosticIssueJsonModel ToJsonModel(DiagnosticIssue issue)
    {
        return new DiagnosticIssueJsonModel(
            issue.RuleId.ToString(),
            ToJsonSeverity(issue.Severity),
            issue.Category,
            issue.Title,
            issue.Message,
            issue.ProjectId?.ToString(),
            ToJsonLocation(issue.PrimaryLocation),
            issue.RelatedLocations.Count == 0 ? null : issue.RelatedLocations.Select(ToJsonLocation).ToArray(),
            issue.SuggestedFix,
            issue.DocsUri?.ToString(),
            issue.Fingerprint);
    }

    private static DiagnosticLocationJsonModel ToJsonLocation(SourceLocation location)
    {
        return new DiagnosticLocationJsonModel(
            location.File,
            location.Pointer?.ToString(),
            location.Line,
            location.Column);
    }

    private static string ToJsonSeverity(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Note => "note",
        DiagnosticSeverity.Warning => "warning",
        DiagnosticSeverity.Error => "error",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private sealed record DiagnosticIssueJsonModel(
        string RuleId,
        string Severity,
        string Category,
        string Title,
        string Message,
        string? ProjectId,
        DiagnosticLocationJsonModel PrimaryLocation,
        IReadOnlyList<DiagnosticLocationJsonModel>? RelatedLocations,
        string? SuggestedFix,
        string? DocsUri,
        string? Fingerprint);

    private sealed record DiagnosticLocationJsonModel(
        string File,
        string? Pointer,
        int? Line,
        int? Column);
}
