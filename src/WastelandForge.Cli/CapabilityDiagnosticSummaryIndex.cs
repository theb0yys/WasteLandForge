using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Cli;

internal sealed record CapabilityDiagnosticSummary(
    int Issues,
    int Errors,
    int Warnings,
    int Notes,
    IReadOnlyList<CapabilityDiagnosticSeveritySummary> Severities,
    IReadOnlyList<CapabilityDiagnosticRuleSummary> Rules,
    IReadOnlyList<CapabilityDiagnosticCategorySummary> Categories);

internal sealed record CapabilityDiagnosticSeveritySummary(
    string Severity,
    int Count,
    IReadOnlyList<string> RuleIds);

internal sealed record CapabilityDiagnosticRuleSummary(
    string RuleId,
    int Count,
    IReadOnlyList<string> Severities,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> SourceFiles);

internal sealed record CapabilityDiagnosticCategorySummary(
    string Category,
    int Count,
    IReadOnlyList<string> RuleIds);

internal static class CapabilityDiagnosticSummaryIndex
{
    public static CapabilityDiagnosticSummary Create(DiagnosticReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return new CapabilityDiagnosticSummary(
            report.Issues.Count,
            report.ErrorCount,
            report.WarningCount,
            report.NoteCount,
            report.Issues
                .GroupBy(issue => FormatSeverity(issue.Severity))
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityDiagnosticSeveritySummary(
                    group.Key,
                    group.Count(),
                    RuleIds(group)))
                .ToArray(),
            report.Issues
                .GroupBy(issue => issue.RuleId.ToString())
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityDiagnosticRuleSummary(
                    group.Key,
                    group.Count(),
                    Severities(group),
                    Categories(group),
                    SourceFiles(group)))
                .ToArray(),
            report.Issues
                .GroupBy(issue => issue.Category)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityDiagnosticCategorySummary(
                    group.Key,
                    group.Count(),
                    RuleIds(group)))
                .ToArray());
    }

    public static JsonObject ToJson(CapabilityDiagnosticSummary summary) =>
        new()
        {
            ["issues"] = summary.Issues,
            ["errors"] = summary.Errors,
            ["warnings"] = summary.Warnings,
            ["notes"] = summary.Notes,
            ["severities"] = new JsonArray(summary.Severities.Select(ToJson).ToArray()),
            ["rules"] = new JsonArray(summary.Rules.Select(ToJson).ToArray()),
            ["categories"] = new JsonArray(summary.Categories.Select(ToJson).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityDiagnosticSummary summary,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (summary.Issues == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Diagnostic summary:");
        builder.AppendLine(
            $"{itemIndent}Diagnostics: {summary.Issues} issue(s); {summary.Errors} error(s), {summary.Warnings} warning(s), {summary.Notes} note(s)");
        foreach (var severity in summary.Severities)
        {
            builder.AppendLine($"{itemIndent}Severity {severity.Severity}: {severity.Count} issue(s)");
            builder.AppendLine($"{detailIndent}Rules: {JoinOrNone(severity.RuleIds)}");
        }

        foreach (var rule in summary.Rules)
        {
            builder.AppendLine($"{itemIndent}Rule {rule.RuleId}: {rule.Count} issue(s)");
            builder.AppendLine($"{detailIndent}Severities: {JoinOrNone(rule.Severities)}");
            builder.AppendLine($"{detailIndent}Categories: {JoinOrNone(rule.Categories)}");
            builder.AppendLine($"{detailIndent}Files: {JoinOrNone(rule.SourceFiles)}");
        }

        foreach (var category in summary.Categories)
        {
            builder.AppendLine($"{itemIndent}Category {category.Category}: {category.Count} issue(s)");
            builder.AppendLine($"{detailIndent}Rules: {JoinOrNone(category.RuleIds)}");
        }
    }

    private static JsonObject ToJson(CapabilityDiagnosticSeveritySummary severity) =>
        new()
        {
            ["severity"] = severity.Severity,
            ["count"] = severity.Count,
            ["ruleIds"] = new JsonArray(severity.RuleIds.Select(ruleId => JsonValue.Create(ruleId)).ToArray())
        };

    private static JsonObject ToJson(CapabilityDiagnosticRuleSummary rule) =>
        new()
        {
            ["ruleId"] = rule.RuleId,
            ["count"] = rule.Count,
            ["severities"] = new JsonArray(rule.Severities.Select(severity => JsonValue.Create(severity)).ToArray()),
            ["categories"] = new JsonArray(rule.Categories.Select(category => JsonValue.Create(category)).ToArray()),
            ["sourceFiles"] = new JsonArray(rule.SourceFiles.Select(file => JsonValue.Create(file)).ToArray())
        };

    private static JsonObject ToJson(CapabilityDiagnosticCategorySummary category) =>
        new()
        {
            ["category"] = category.Category,
            ["count"] = category.Count,
            ["ruleIds"] = new JsonArray(category.RuleIds.Select(ruleId => JsonValue.Create(ruleId)).ToArray())
        };

    private static IReadOnlyList<string> RuleIds(IEnumerable<DiagnosticIssue> issues) =>
        issues
            .Select(issue => issue.RuleId.ToString())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> Severities(IEnumerable<DiagnosticIssue> issues) =>
        issues
            .Select(issue => FormatSeverity(issue.Severity))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> Categories(IEnumerable<DiagnosticIssue> issues) =>
        issues
            .Select(issue => issue.Category)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> SourceFiles(IEnumerable<DiagnosticIssue> issues) =>
        issues
            .Select(issue => issue.PrimaryLocation.File)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
