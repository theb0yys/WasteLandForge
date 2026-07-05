using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record DoctorExportReleaseReadiness(
    string Kind,
    bool Included,
    bool EvaluatedInCurrentGate,
    string SourceCommand,
    string Status,
    string Detail,
    string? EvidenceRoot,
    string? ReleasePrepareEvidenceStatus,
    string? PublishReadinessStatus,
    bool EvidenceSatisfied,
    bool GovernanceSatisfied,
    bool ApprovalSatisfied,
    bool LocalPreconditionsSatisfied,
    bool ReadyForRealPublish,
    int RequiredEvidence,
    int RequiredChecks,
    int SatisfiedChecks,
    int BlockingChecks,
    IReadOnlyList<string> BlockingCheckIds,
    DoctorExportReleaseDryRunEvidenceRemediation DryRunEvidenceRemediation,
    IReadOnlyList<DoctorExportReleaseReadinessEvidence> Evidence,
    IReadOnlyList<string> Boundaries);

internal sealed record DoctorExportReleaseReadinessEvidence(
    string Id,
    string Title,
    string Source,
    string Status,
    bool Required);

internal sealed record DoctorExportReleaseDryRunEvidenceRemediation(
    string Status,
    bool CheckedInCurrentGate,
    bool RequiresOperatorAction,
    string SourceStatus,
    int ActionItems,
    int CommandHints,
    int AffectedPaths,
    int BlockingIssues,
    string RecommendedCommand,
    string Detail,
    IReadOnlyList<string> TargetPaths,
    IReadOnlyList<DoctorExportReleaseDryRunEvidenceRemediationItem> Items);

internal sealed record DoctorExportReleaseDryRunEvidenceRemediationItem(
    string Id,
    string Priority,
    string Category,
    string Status,
    string Reason,
    string CommandHint,
    string Execution,
    bool BlocksPublishReadiness,
    IReadOnlyList<string> TargetPaths);

internal static class DoctorExportReleaseReadinessProjection
{
    private const string SourceCommand = "forge release publish <project-root> --dry-run --format json --no-input";
    private const string ReleaseVerifyEvidenceCommandHint = "forge release verify <project-root> --format json --no-input";

    public static DoctorExportReleaseReadiness NotIncluded() =>
        new(
            "wastelandforge/doctor-release-readiness/v1",
            Included: false,
            EvaluatedInCurrentGate: false,
            SourceCommand,
            Status: "not-included",
            Detail: "project-root-not-provided",
            EvidenceRoot: null,
            ReleasePrepareEvidenceStatus: null,
            PublishReadinessStatus: null,
            EvidenceSatisfied: false,
            GovernanceSatisfied: false,
            ApprovalSatisfied: false,
            LocalPreconditionsSatisfied: false,
            ReadyForRealPublish: false,
            RequiredEvidence: 0,
            RequiredChecks: 0,
            SatisfiedChecks: 0,
            BlockingChecks: 0,
            BlockingCheckIds: [],
            DryRunEvidenceRemediation: new DoctorExportReleaseDryRunEvidenceRemediation(
                "not-included",
                CheckedInCurrentGate: false,
                RequiresOperatorAction: false,
                SourceStatus: "not-included",
                ActionItems: 0,
                CommandHints: 0,
                AffectedPaths: 0,
                BlockingIssues: 0,
                RecommendedCommand: ReleaseVerifyEvidenceCommandHint,
                Detail: "project-root-not-provided",
                TargetPaths: [],
                Items: []),
            Evidence: [],
            Boundaries:
            [
                "Release-readiness handoff is included only when doctor export has a project root.",
                "No release is published.",
                "Remote repositories are not called.",
                "Release assets are not uploaded.",
                "Attestations and signing are not performed.",
                "External tools, plugin mutation, MO2 automation, GECK automation, runtime probes, and AI calls are not used."
            ]);

    public static DoctorExportReleaseReadiness Create(string projectRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);

        var preflight = ReleasePublishPreflightPlanner.Plan(new ReleasePublishPreflightOptions(
            projectRoot,
            DryRun: true,
            Yes: false,
            Confirm: null));

        return new DoctorExportReleaseReadiness(
            "wastelandforge/doctor-release-readiness/v1",
            Included: true,
            EvaluatedInCurrentGate: true,
            SourceCommand,
            preflight.PublishReadiness.Status,
            preflight.PublishReadiness.Detail,
            preflight.ReleasePrepareEvidenceRoot,
            preflight.ReleasePrepareEvidenceStatus,
            preflight.PublishReadiness.Status,
            preflight.PublishReadiness.EvidenceSatisfied,
            preflight.PublishReadiness.GovernanceSatisfied,
            preflight.PublishReadiness.ApprovalSatisfied,
            preflight.PublishReadiness.LocalPreconditionsSatisfied,
            preflight.PublishReadiness.ReadyForRealPublish,
            preflight.RequiredEvidence.Count,
            preflight.PublishReadiness.RequiredChecks,
            preflight.PublishReadiness.SatisfiedChecks,
            preflight.PublishReadiness.BlockingChecks,
            preflight.PublishReadiness.BlockingCheckIds,
            CreateDryRunEvidenceRemediation(preflight.DryRunEvidenceRemediation),
            preflight.RequiredEvidence
                .Select(evidence => new DoctorExportReleaseReadinessEvidence(
                    evidence.Id,
                    evidence.Title,
                    evidence.Source,
                    evidence.Status,
                    evidence.Required))
                .ToArray(),
            preflight.Boundaries);
    }

    private static DoctorExportReleaseDryRunEvidenceRemediation CreateDryRunEvidenceRemediation(
        ReleasePublishDryRunEvidenceRemediation remediation) =>
        new(
            remediation.Status,
            remediation.CheckedInCurrentGate,
            remediation.RequiresOperatorAction,
            remediation.SourceStatus,
            remediation.ActionItems,
            remediation.CommandHints,
            remediation.AffectedPaths,
            remediation.BlockingIssues,
            remediation.RecommendedCommand,
            remediation.Detail,
            remediation.TargetPaths,
            remediation.Items
                .Select(item => new DoctorExportReleaseDryRunEvidenceRemediationItem(
                    item.Id,
                    item.Priority,
                    item.Category,
                    item.Status,
                    item.Reason,
                    item.CommandHint,
                    item.Execution,
                    item.BlocksPublishReadiness,
                    item.TargetPaths))
                .ToArray());
}

internal static class DoctorExportReleaseReadinessIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReleaseReadiness readiness)
    {
        ArgumentNullException.ThrowIfNull(readiness);

        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = readiness.Kind,
            ["releaseReadiness"] = ToJson(readiness)
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static JsonObject ToJson(DoctorExportReleaseReadiness readiness) =>
        new()
        {
            ["kind"] = readiness.Kind,
            ["included"] = readiness.Included,
            ["evaluatedInCurrentGate"] = readiness.EvaluatedInCurrentGate,
            ["sourceCommand"] = readiness.SourceCommand,
            ["status"] = readiness.Status,
            ["detail"] = readiness.Detail,
            ["evidenceRoot"] = readiness.EvidenceRoot,
            ["releasePrepareEvidenceStatus"] = readiness.ReleasePrepareEvidenceStatus,
            ["publishReadinessStatus"] = readiness.PublishReadinessStatus,
            ["evidenceSatisfied"] = readiness.EvidenceSatisfied,
            ["governanceSatisfied"] = readiness.GovernanceSatisfied,
            ["approvalSatisfied"] = readiness.ApprovalSatisfied,
            ["localPreconditionsSatisfied"] = readiness.LocalPreconditionsSatisfied,
            ["readyForRealPublish"] = readiness.ReadyForRealPublish,
            ["requiredEvidence"] = readiness.RequiredEvidence,
            ["requiredChecks"] = readiness.RequiredChecks,
            ["satisfiedChecks"] = readiness.SatisfiedChecks,
            ["blockingChecks"] = readiness.BlockingChecks,
            ["blockingCheckIds"] = ToStringArray(readiness.BlockingCheckIds),
            ["dryRunEvidenceRemediation"] = ToDryRunEvidenceRemediation(readiness.DryRunEvidenceRemediation),
            ["evidence"] = ToEvidenceArray(readiness.Evidence),
            ["boundaries"] = ToStringArray(readiness.Boundaries)
        };

    public static void AppendText(StringBuilder builder, DoctorExportReleaseReadiness readiness)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(readiness);

        builder.AppendLine("Release readiness:");
        builder.AppendLine($"  status: {readiness.Status}");
        builder.AppendLine($"  included: {readiness.Included.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  evaluated in current gate: {readiness.EvaluatedInCurrentGate.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  source command: {readiness.SourceCommand}");
        builder.AppendLine($"  evidence root: {readiness.EvidenceRoot ?? "(not included)"}");
        builder.AppendLine($"  release prepare evidence: {readiness.ReleasePrepareEvidenceStatus ?? "(not included)"}");
        builder.AppendLine($"  publish readiness: {readiness.PublishReadinessStatus ?? "(not included)"}");
        builder.AppendLine($"  evidence satisfied: {readiness.EvidenceSatisfied.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  governance satisfied: {readiness.GovernanceSatisfied.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  approval satisfied: {readiness.ApprovalSatisfied.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  local preconditions satisfied: {readiness.LocalPreconditionsSatisfied.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  ready for real publish: {readiness.ReadyForRealPublish.ToString().ToLowerInvariant()}");
        builder.AppendLine($"  checks: {readiness.SatisfiedChecks}/{readiness.RequiredChecks} satisfied; {readiness.BlockingChecks} blocking");
        builder.AppendLine($"  detail: {readiness.Detail}");
        AppendTextDryRunEvidenceRemediation(builder, readiness.DryRunEvidenceRemediation);
        foreach (var evidence in readiness.Evidence)
        {
            builder.AppendLine($"  {evidence.Id}: {evidence.Status} ({evidence.Source})");
        }
    }

    public static string RenderMarkdown(DoctorExportReleaseReadiness readiness)
    {
        ArgumentNullException.ThrowIfNull(readiness);

        var builder = new StringBuilder();
        AppendMarkdown(builder, readiness, "# WastelandForge Doctor Release Readiness");
        return builder.ToString();
    }

    public static void AppendMarkdown(
        StringBuilder builder,
        DoctorExportReleaseReadiness readiness,
        string heading = "## Release Readiness")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(readiness);

        builder.AppendLine(heading);
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(readiness.Status)}`");
        builder.AppendLine($"- Included: `{readiness.Included.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Source command: `{EscapeInline(readiness.SourceCommand)}`");
        builder.AppendLine($"- Evidence root: `{EscapeInline(readiness.EvidenceRoot ?? "(not included)")}`");
        builder.AppendLine($"- Release prepare evidence: `{EscapeInline(readiness.ReleasePrepareEvidenceStatus ?? "(not included)")}`");
        builder.AppendLine($"- Publish readiness: `{EscapeInline(readiness.PublishReadinessStatus ?? "(not included)")}`");
        builder.AppendLine($"- Evidence satisfied: `{readiness.EvidenceSatisfied.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Governance satisfied: `{readiness.GovernanceSatisfied.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Approval satisfied: `{readiness.ApprovalSatisfied.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Local preconditions satisfied: `{readiness.LocalPreconditionsSatisfied.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Ready for real publish: `{readiness.ReadyForRealPublish.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Checks: `{readiness.SatisfiedChecks}/{readiness.RequiredChecks}` satisfied; `{readiness.BlockingChecks}` blocking");
        builder.AppendLine($"- Detail: `{EscapeInline(readiness.Detail)}`");
        builder.AppendLine();

        AppendMarkdownDryRunEvidenceRemediation(
            builder,
            readiness.DryRunEvidenceRemediation,
            StringComparer.Ordinal.Equals(heading, "# WastelandForge Doctor Release Readiness")
                ? "## Dry-Run Evidence Remediation"
                : "### Dry-Run Evidence Remediation");

        if (readiness.Evidence.Count == 0)
        {
            builder.AppendLine("No release-readiness evidence is included.");
        }
        else
        {
            builder.AppendLine("| Evidence | Status | Source | Required |");
            builder.AppendLine("|---|---|---|---|");
            foreach (var evidence in readiness.Evidence)
            {
                builder.Append("| `");
                builder.Append(EscapeInline(evidence.Id));
                builder.Append("` | `");
                builder.Append(EscapeInline(evidence.Status));
                builder.Append("` | `");
                builder.Append(EscapeInline(evidence.Source));
                builder.Append("` | `");
                builder.Append(evidence.Required.ToString().ToLowerInvariant());
                builder.AppendLine("` |");
            }
        }

        builder.AppendLine();
        builder.AppendLine(StringComparer.Ordinal.Equals(heading, "# WastelandForge Doctor Release Readiness")
            ? "## Boundaries"
            : "### Boundaries");
        builder.AppendLine();
        foreach (var boundary in readiness.Boundaries)
        {
            builder.Append("- ");
            builder.AppendLine(EscapeParagraph(boundary));
        }
    }

    private static JsonObject ToDryRunEvidenceRemediation(
        DoctorExportReleaseDryRunEvidenceRemediation remediation) =>
        new()
        {
            ["status"] = remediation.Status,
            ["checkedInCurrentGate"] = remediation.CheckedInCurrentGate,
            ["requiresOperatorAction"] = remediation.RequiresOperatorAction,
            ["sourceStatus"] = remediation.SourceStatus,
            ["actionItems"] = remediation.ActionItems,
            ["commandHints"] = remediation.CommandHints,
            ["affectedPaths"] = remediation.AffectedPaths,
            ["blockingIssues"] = remediation.BlockingIssues,
            ["recommendedCommand"] = remediation.RecommendedCommand,
            ["detail"] = remediation.Detail,
            ["targetPaths"] = ToStringArray(remediation.TargetPaths),
            ["items"] = ToDryRunEvidenceRemediationItems(remediation.Items)
        };

    private static JsonArray ToDryRunEvidenceRemediationItems(
        IReadOnlyList<DoctorExportReleaseDryRunEvidenceRemediationItem> items)
    {
        var array = new JsonArray();
        foreach (var item in items)
        {
            array.Add(new JsonObject
            {
                ["id"] = item.Id,
                ["priority"] = item.Priority,
                ["category"] = item.Category,
                ["status"] = item.Status,
                ["reason"] = item.Reason,
                ["commandHint"] = item.CommandHint,
                ["execution"] = item.Execution,
                ["blocksPublishReadiness"] = item.BlocksPublishReadiness,
                ["targetPaths"] = ToStringArray(item.TargetPaths)
            });
        }

        return array;
    }

    private static JsonArray ToEvidenceArray(IReadOnlyList<DoctorExportReleaseReadinessEvidence> evidence)
    {
        var array = new JsonArray();
        foreach (var item in evidence)
        {
            array.Add(new JsonObject
            {
                ["id"] = item.Id,
                ["title"] = item.Title,
                ["source"] = item.Source,
                ["status"] = item.Status,
                ["required"] = item.Required
            });
        }

        return array;
    }

    private static void AppendTextDryRunEvidenceRemediation(
        StringBuilder builder,
        DoctorExportReleaseDryRunEvidenceRemediation remediation)
    {
        builder.AppendLine("  dry-run evidence remediation:");
        builder.AppendLine($"    status: {remediation.Status}");
        builder.AppendLine($"    checked in current gate: {remediation.CheckedInCurrentGate.ToString().ToLowerInvariant()}");
        builder.AppendLine($"    requires operator action: {remediation.RequiresOperatorAction.ToString().ToLowerInvariant()}");
        builder.AppendLine($"    source status: {remediation.SourceStatus}");
        builder.AppendLine($"    action items: {remediation.ActionItems}");
        builder.AppendLine($"    command hints: {remediation.CommandHints}");
        builder.AppendLine($"    affected paths: {remediation.AffectedPaths}");
        builder.AppendLine($"    blocking issues: {remediation.BlockingIssues}");
        builder.AppendLine($"    recommended command: {remediation.RecommendedCommand}");
        builder.AppendLine($"    detail: {remediation.Detail}");
        foreach (var item in remediation.Items)
        {
            builder.AppendLine($"    [{item.Priority}] {item.Id} ({item.Category}, {item.Execution}): {Normalize(item.Reason)}");
            builder.AppendLine($"      command: {item.CommandHint}");
            builder.AppendLine($"      paths: {string.Join(", ", item.TargetPaths)}");
        }
    }

    private static void AppendMarkdownDryRunEvidenceRemediation(
        StringBuilder builder,
        DoctorExportReleaseDryRunEvidenceRemediation remediation,
        string heading)
    {
        builder.AppendLine(heading);
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(remediation.Status)}`");
        builder.AppendLine($"- Checked in current gate: `{remediation.CheckedInCurrentGate.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Requires operator action: `{remediation.RequiresOperatorAction.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Source status: `{EscapeInline(remediation.SourceStatus)}`");
        builder.AppendLine($"- Action items: {remediation.ActionItems}");
        builder.AppendLine($"- Command hints: {remediation.CommandHints}");
        builder.AppendLine($"- Affected paths: {remediation.AffectedPaths}");
        builder.AppendLine($"- Blocking issues: {remediation.BlockingIssues}");
        builder.AppendLine($"- Recommended command: `{EscapeInline(remediation.RecommendedCommand)}`");
        builder.AppendLine($"- Detail: `{EscapeInline(remediation.Detail)}`");
        builder.AppendLine();

        if (remediation.Items.Count == 0)
        {
            builder.AppendLine("No release dry-run evidence remediation items.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Item | Priority | Category | Execution | Blocks publish | Command |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var item in remediation.Items)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(item.Id));
            builder.Append("` | `");
            builder.Append(EscapeInline(item.Priority));
            builder.Append("` | `");
            builder.Append(EscapeInline(item.Category));
            builder.Append("` | `");
            builder.Append(EscapeInline(item.Execution));
            builder.Append("` | `");
            builder.Append(item.BlocksPublishReadiness.ToString().ToLowerInvariant());
            builder.Append("` | `");
            builder.Append(EscapeInline(item.CommandHint));
            builder.AppendLine("` |");
        }

        builder.AppendLine();
    }

    private static JsonArray ToStringArray(IReadOnlyList<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeParagraph(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');

    private static string Normalize(string text) =>
        EscapeParagraph(text);
}
