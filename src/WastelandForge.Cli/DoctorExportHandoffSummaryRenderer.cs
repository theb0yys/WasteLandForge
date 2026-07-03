using System.Text;

namespace WastelandForge.Cli;

internal static class DoctorExportHandoffSummaryRenderer
{
    private const int WorkItemLimit = 5;

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var triage = DoctorExportTriageProjection.Create(report);
        var remediation = DoctorExportTriageProjection.CreateRemediationHeader(triage);
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Handoff Summary");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this handoff summary");
        builder.AppendLine();

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
            builder.AppendLine($"- First path: `{EscapeInline(remediation.BundlePath)}`");
        }

        builder.AppendLine();

        builder.AppendLine("## Immediate Worklist");
        builder.AppendLine();
        builder.AppendLine($"- Priorities: {EscapeParagraph(FormatPrioritySummary(triage.Worklist))}");
        builder.AppendLine($"- Sources: {EscapeParagraph(FormatSourceSummary(triage.Worklist))}");
        builder.AppendLine();
        if (triage.Worklist.Count == 0)
        {
            builder.AppendLine("- [ ] No remediation work items.");
        }
        else
        {
            foreach (var workItem in triage.Worklist.OrderBy(item => item.Order).Take(WorkItemLimit))
            {
                builder.AppendLine($"- [ ] `{EscapeInline(workItem.Id)}` ({EscapeInline(workItem.Priority)}): {EscapeParagraph(workItem.Title)}");
                builder.AppendLine($"  Command: `{EscapeInline(ResolveCommand(triage.Commands, workItem.CommandHint))}`");
                builder.AppendLine($"  Path: `{EscapeInline(workItem.BundlePath)}`");
            }

            var omitted = triage.Worklist.Count - WorkItemLimit;
            if (omitted > 0)
            {
                builder.AppendLine($"- [ ] Review {omitted} additional work item(s) in `triage/index.md`.");
            }
        }

        builder.AppendLine();

        builder.AppendLine("## Command Hints");
        builder.AppendLine();
        if (triage.Commands.Count == 0)
        {
            builder.AppendLine("No command hints.");
        }
        else
        {
            foreach (var command in triage.Commands.OrderBy(command => command.Id, StringComparer.Ordinal))
            {
                builder.AppendLine($"- `{EscapeInline(command.Id)}`: `{EscapeInline(command.Command)}`");
                builder.AppendLine($"  Path: `{EscapeInline(command.BundlePath)}`");
                builder.AppendLine($"  Purpose: {EscapeParagraph(command.Purpose)}");
            }
        }

        builder.AppendLine();

        builder.AppendLine("## Key Archive Paths");
        builder.AppendLine();
        builder.AppendLine("- `doctor-export.md` - full redacted human-readable Doctor report.");
        builder.AppendLine("- `doctor-export.json` - full redacted machine-readable Doctor report.");
        foreach (var target in triage.ReviewTargets.Where(target => !string.IsNullOrWhiteSpace(target.BundlePath)))
        {
            builder.AppendLine($"- `{EscapeInline(target.BundlePath ?? string.Empty)}` - {EscapeParagraph(target.Purpose)}");
        }

        builder.AppendLine();

        builder.AppendLine("## Boundaries");
        builder.AppendLine();
        builder.AppendLine("- Derived from redacted Doctor export metadata already in this archive.");
        builder.AppendLine("- Does not run provider detectors, runtime probes, MO2 VFS checks, GECK automation, network checks, provider version checks, or AI calls.");

        return builder.ToString();
    }

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
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
}
