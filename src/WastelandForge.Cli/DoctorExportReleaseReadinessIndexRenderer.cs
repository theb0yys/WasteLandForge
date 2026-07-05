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
    IReadOnlyList<DoctorExportReleaseReadinessEvidence> Evidence,
    IReadOnlyList<string> Boundaries);

internal sealed record DoctorExportReleaseReadinessEvidence(
    string Id,
    string Title,
    string Source,
    string Status,
    bool Required);

internal static class DoctorExportReleaseReadinessProjection
{
    private const string SourceCommand = "forge release publish <project-root> --dry-run --format json --no-input";

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

    private static JsonArray ToStringArray(IReadOnlyList<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeParagraph(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
}
