using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportTriageIndexRenderer
{
    private const int OperatorHandoffItemLimit = 5;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var triage = DoctorExportTriageProjection.Create(report);
        var reviewPaths = BundleReviewPaths(triage).ToArray();
        var prioritySummaries = DoctorExportTriageProjection.CreatePrioritySummaries(triage.Worklist).ToArray();
        var sourceSummaries = DoctorExportTriageProjection.CreateBundleSourceSummaries(triage.Worklist).ToArray();
        var remediation = DoctorExportTriageProjection.CreateRemediationHeader(triage);
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-triage-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = new JsonObject
            {
                ["status"] = triage.Status,
                ["blockingItems"] = triage.Blocking.Count,
                ["reviewItems"] = triage.Review.Count,
                ["actions"] = triage.Actions.Count,
                ["commandHints"] = triage.Commands.Count,
                ["workItems"] = triage.Worklist.Count,
                ["worklistPriorityGroups"] = prioritySummaries.Length,
                ["worklistSourceGroups"] = sourceSummaries.Length,
                ["reviewPaths"] = reviewPaths.Length,
                ["requiredUnavailable"] = triage.RequiredUnavailable,
                ["optionalUnavailable"] = triage.OptionalUnavailable,
                ["diagnosticErrors"] = triage.DiagnosticErrors,
                ["diagnosticWarnings"] = triage.DiagnosticWarnings,
                ["openQuestions"] = triage.OpenQuestions,
                ["releaseReadinessBlockingChecks"] = triage.ReleaseReadinessBlockingChecks
            },
            ["blocking"] = new JsonArray(triage.Blocking.Select(ToBundleJson).ToArray()),
            ["review"] = new JsonArray(triage.Review.Select(ToBundleJson).ToArray()),
            ["actions"] = new JsonArray(triage.Actions.Select(ToBundleJson).ToArray()),
            ["commands"] = new JsonArray(triage.Commands.Select(ToBundleJson).ToArray()),
            ["remediation"] = ToBundleJson(remediation),
            ["worklist"] = new JsonArray(triage.Worklist.Select(ToBundleJson).ToArray()),
            ["worklistSummary"] = new JsonObject
            {
                ["priorities"] = new JsonArray(prioritySummaries.Select(ToBundleJson).ToArray()),
                ["sources"] = new JsonArray(sourceSummaries.Select(ToBundleJson).ToArray())
            },
            ["reviewPaths"] = new JsonArray(reviewPaths.Select(ToBundleJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var triage = DoctorExportTriageProjection.Create(report);
        var reviewPaths = BundleReviewPaths(triage).ToArray();
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Triage");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this triage index");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{triage.Status}`");
        builder.AppendLine($"- Blocking items: {triage.Blocking.Count}");
        builder.AppendLine($"- Review items: {triage.Review.Count}");
        builder.AppendLine($"- Actions: {triage.Actions.Count}");
        builder.AppendLine($"- Command hints: {triage.Commands.Count}");
        builder.AppendLine($"- Work items: {triage.Worklist.Count}");
        builder.AppendLine($"- Review paths: {reviewPaths.Length}");
        builder.AppendLine();

        AppendItems(builder, "Blocking", triage.Blocking, "No blocking items.");
        AppendItems(builder, "Review", triage.Review, "No review items.");
        AppendActions(builder, triage.Actions);
        AppendCommands(builder, triage.Commands);
        AppendRemediationHeader(builder, DoctorExportTriageProjection.CreateRemediationHeader(triage));
        AppendOperatorHandoff(builder, triage);
        AppendWorklistSummary(builder, triage.Worklist);
        AppendWorklist(builder, triage.Worklist);
        AppendReviewPaths(builder, reviewPaths);

        return builder.ToString();
    }

    private static IEnumerable<DoctorExportTriageReviewTarget> BundleReviewPaths(DoctorExportTriageReport triage) =>
        triage.ReviewTargets.Where(target => !string.IsNullOrWhiteSpace(target.BundlePath));

    private static JsonObject ToBundleJson(DoctorExportTriageItem item) =>
        new()
        {
            ["id"] = item.Id,
            ["severity"] = item.Severity,
            ["count"] = item.Count,
            ["title"] = item.Title,
            ["paths"] = new JsonArray(item.BundlePaths.Select(path => JsonValue.Create(path)).ToArray())
        };

    private static JsonObject ToBundleJson(DoctorExportTriageAction action) =>
        new()
        {
            ["area"] = new JsonObject
            {
                ["id"] = action.AreaId,
                ["title"] = action.AreaTitle,
                ["status"] = action.AreaStatus
            },
            ["sourceType"] = action.SourceType,
            ["text"] = action.Text,
            ["path"] = action.BundlePath
        };

    private static JsonObject ToBundleJson(DoctorExportTriageCommandHint command) =>
        new()
        {
            ["id"] = command.Id,
            ["command"] = command.Command,
            ["purpose"] = command.Purpose,
            ["path"] = command.BundlePath
        };

    private static JsonObject ToBundleJson(DoctorExportTriageRemediationHeader remediation)
    {
        var json = new JsonObject
        {
            ["status"] = remediation.Status,
            ["headline"] = remediation.Headline,
            ["workItems"] = remediation.WorkItems,
            ["blockerItems"] = remediation.BlockerItems,
            ["reviewItems"] = remediation.ReviewItems
        };

        if (!string.IsNullOrWhiteSpace(remediation.FirstWorkItem))
        {
            json["firstWorkItem"] = remediation.FirstWorkItem;
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommandHint))
        {
            json["firstCommandHint"] = remediation.FirstCommandHint;
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommand))
        {
            json["firstCommand"] = remediation.FirstCommand;
        }

        if (!string.IsNullOrWhiteSpace(remediation.BundlePath))
        {
            json["path"] = remediation.BundlePath;
        }

        return json;
    }

    private static JsonObject ToBundleJson(DoctorExportTriageWorkItem workItem) =>
        new()
        {
            ["order"] = workItem.Order,
            ["id"] = workItem.Id,
            ["priority"] = workItem.Priority,
            ["title"] = workItem.Title,
            ["reason"] = workItem.Reason,
            ["commandHint"] = workItem.CommandHint,
            ["path"] = workItem.BundlePath
        };

    private static JsonObject ToBundleJson(DoctorExportTriageWorklistPrioritySummary summary) =>
        new()
        {
            ["priority"] = summary.Priority,
            ["count"] = summary.Count,
            ["workItems"] = new JsonArray(summary.WorkItems.Select(item => JsonValue.Create(item)).ToArray())
        };

    private static JsonObject ToBundleJson(DoctorExportTriageWorklistSourceSummary summary) =>
        new()
        {
            ["path"] = summary.Source,
            ["count"] = summary.Count,
            ["workItems"] = new JsonArray(summary.WorkItems.Select(item => JsonValue.Create(item)).ToArray())
        };

    private static JsonObject ToBundleJson(DoctorExportTriageReviewTarget path) =>
        new()
        {
            ["path"] = path.BundlePath,
            ["purpose"] = path.Purpose
        };

    private static void AppendItems(
        StringBuilder builder,
        string heading,
        IReadOnlyList<DoctorExportTriageItem> items,
        string emptyText)
    {
        builder.AppendLine($"## {heading}");
        builder.AppendLine();
        if (items.Count == 0)
        {
            builder.AppendLine(emptyText);
            builder.AppendLine();
            return;
        }

        foreach (var item in items)
        {
            builder.AppendLine($"- `{EscapeInline(item.Id)}` ({EscapeInline(item.Severity)}, {item.Count}): {EscapeParagraph(item.Title)}");
            builder.AppendLine($"  Paths: {JoinInline(item.BundlePaths)}");
        }

        builder.AppendLine();
    }

    private static void AppendActions(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageAction> actions)
    {
        builder.AppendLine("## Actions");
        builder.AppendLine();
        if (actions.Count == 0)
        {
            builder.AppendLine("No next actions.");
            builder.AppendLine();
            return;
        }

        foreach (var action in actions)
        {
            builder.AppendLine($"- `{EscapeInline(action.AreaId)}` ({EscapeInline(action.SourceType)}, {EscapeInline(action.AreaStatus)}): {EscapeParagraph(action.Text)}");
        }

        builder.AppendLine();
    }

    private static void AppendCommands(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageCommandHint> commands)
    {
        builder.AppendLine("## Command Hints");
        builder.AppendLine();
        if (commands.Count == 0)
        {
            builder.AppendLine("No command hints.");
            builder.AppendLine();
            return;
        }

        foreach (var command in commands)
        {
            builder.AppendLine($"- `{EscapeInline(command.Id)}`: `{EscapeInline(command.Command)}`");
            builder.AppendLine($"  Path: `{EscapeInline(command.BundlePath)}`");
            builder.AppendLine($"  Purpose: {EscapeParagraph(command.Purpose)}");
        }

        builder.AppendLine();
    }

    private static void AppendRemediationHeader(
        StringBuilder builder,
        DoctorExportTriageRemediationHeader remediation)
    {
        builder.AppendLine("## Remediation");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(remediation.Status)}`");
        builder.AppendLine($"- Headline: {EscapeParagraph(remediation.Headline)}");
        builder.AppendLine($"- Work items: {remediation.WorkItems}");
        builder.AppendLine($"- Blocker items: {remediation.BlockerItems}");
        builder.AppendLine($"- Review items: {remediation.ReviewItems}");
        if (!string.IsNullOrWhiteSpace(remediation.FirstWorkItem))
        {
            builder.AppendLine($"- First work item: `{EscapeInline(remediation.FirstWorkItem)}`");
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommandHint))
        {
            builder.AppendLine($"- First command hint: `{EscapeInline(remediation.FirstCommandHint)}`");
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommand))
        {
            builder.AppendLine($"- First command: `{EscapeInline(remediation.FirstCommand)}`");
        }

        if (!string.IsNullOrWhiteSpace(remediation.BundlePath))
        {
            builder.AppendLine($"- Path: `{EscapeInline(remediation.BundlePath)}`");
        }

        builder.AppendLine();
    }

    private static void AppendOperatorHandoff(
        StringBuilder builder,
        DoctorExportTriageReport triage)
    {
        var remediation = DoctorExportTriageProjection.CreateRemediationHeader(triage);
        builder.AppendLine("## Operator Handoff");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(remediation.Status)}`");
        builder.AppendLine($"- Headline: {EscapeParagraph(remediation.Headline)}");
        builder.AppendLine($"- Priorities: {EscapeParagraph(FormatPrioritySummary(triage.Worklist))}");
        builder.AppendLine($"- Sources: {EscapeParagraph(FormatSourceSummary(triage.Worklist))}");
        builder.AppendLine();
        builder.AppendLine("Checklist:");
        if (triage.Worklist.Count == 0)
        {
            builder.AppendLine("- [ ] No remediation work items.");
            builder.AppendLine();
            return;
        }

        foreach (var workItem in triage.Worklist.OrderBy(item => item.Order).Take(OperatorHandoffItemLimit))
        {
            builder.AppendLine($"- [ ] `{EscapeInline(workItem.Id)}` ({EscapeInline(workItem.Priority)}): {EscapeParagraph(workItem.Title)}");
            builder.AppendLine($"  Command: `{EscapeInline(ResolveCommand(triage.Commands, workItem.CommandHint))}`");
            builder.AppendLine($"  Path: `{EscapeInline(workItem.BundlePath)}`");
        }

        var omitted = triage.Worklist.Count - OperatorHandoffItemLimit;
        if (omitted > 0)
        {
            builder.AppendLine($"- [ ] Review {omitted} additional work item(s) in the full worklist.");
        }

        builder.AppendLine();
    }

    private static void AppendWorklistSummary(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        builder.AppendLine("## Worklist Summary");
        builder.AppendLine();
        if (worklist.Count == 0)
        {
            builder.AppendLine("No worklist summary.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("Priorities:");
        foreach (var summary in DoctorExportTriageProjection.CreatePrioritySummaries(worklist))
        {
            builder.AppendLine($"- `{EscapeInline(summary.Priority)}`: {summary.Count} item(s) - {JoinInline(summary.WorkItems)}");
        }

        builder.AppendLine();
        builder.AppendLine("Sources:");
        foreach (var summary in DoctorExportTriageProjection.CreateBundleSourceSummaries(worklist))
        {
            builder.AppendLine($"- `{EscapeInline(summary.Source)}`: {summary.Count} item(s) - {JoinInline(summary.WorkItems)}");
        }

        builder.AppendLine();
    }

    private static void AppendWorklist(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        builder.AppendLine("## Worklist");
        builder.AppendLine();
        if (worklist.Count == 0)
        {
            builder.AppendLine("No work items.");
            builder.AppendLine();
            return;
        }

        foreach (var workItem in worklist)
        {
            builder.AppendLine($"- {workItem.Order}. `{EscapeInline(workItem.Id)}` ({EscapeInline(workItem.Priority)}): {EscapeParagraph(workItem.Title)}");
            builder.AppendLine($"  Path: `{EscapeInline(workItem.BundlePath)}`");
            builder.AppendLine($"  Command hint: `{EscapeInline(workItem.CommandHint)}`");
            builder.AppendLine($"  Reason: {EscapeParagraph(workItem.Reason)}");
        }

        builder.AppendLine();
    }

    private static void AppendReviewPaths(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageReviewTarget> paths)
    {
        builder.AppendLine("## Review Paths");
        builder.AppendLine();
        foreach (var path in paths)
        {
            builder.AppendLine($"- `{EscapeInline(path.BundlePath ?? string.Empty)}` - {EscapeParagraph(path.Purpose)}");
        }
    }

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string FormatPrioritySummary(IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        var summaries = DoctorExportTriageProjection.CreatePrioritySummaries(worklist);
        return summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Priority}={summary.Count}"));
    }

    private static string FormatSourceSummary(IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        var summaries = DoctorExportTriageProjection.CreateBundleSourceSummaries(worklist);
        return summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Source}={summary.Count}"));
    }

    private static string ResolveCommand(
        IReadOnlyList<DoctorExportTriageCommandHint> commands,
        string commandHint) =>
        commands.FirstOrDefault(command => StringComparer.Ordinal.Equals(command.Id, commandHint))?.Command ??
        commandHint;

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeParagraph(string text) =>
        Normalize(text);

    private static string Normalize(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
}
