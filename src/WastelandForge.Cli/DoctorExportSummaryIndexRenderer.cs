using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportSummaryIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-summary-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = ToJson(report.Summary),
            ["indexSummaries"] = new JsonObject
            {
                ["doctorAreaCapabilitySummary"] = CapabilityDoctorAreaCapabilitySummaryIndex.ToJson(report.Index.DoctorAreaCapabilitySummary),
                ["providerInventorySummary"] = CapabilityProviderInventorySummaryIndex.ToJson(report.Index.ProviderInventorySummary),
                ["evidenceSummary"] = CapabilityScanEvidenceSummaryIndex.ToJson(report.Index.EvidenceSummary),
                ["actionSummary"] = CapabilityDoctorActionSummaryIndex.ToJson(report.Index.ActionSummary),
                ["requirementSummary"] = CapabilityRequirementSummaryIndex.ToJson(report.Index.RequirementSummary),
                ["diagnosticSummary"] = CapabilityDiagnosticSummaryIndex.ToJson(report.Index.DiagnosticSummary),
                ["cataloguePolicySummary"] = ToJson(report.Index.CataloguePolicy)
            }
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Summary");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this summary index");
        builder.AppendLine();

        AppendReportSummary(builder, report);
        AppendDerivedSummary(builder, report);

        return builder.ToString();
    }

    private static void AppendReportSummary(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Report Summary");
        builder.AppendLine();
        builder.AppendLine($"- Catalog: `{EscapeInline(report.Summary.Catalog.Id)} {EscapeInline(report.Summary.Catalog.Version)}`");
        builder.AppendLine($"- Offline: `{report.Offline.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- AI optional: `{report.AiOptional.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"- Providers: {report.Summary.Providers.Total} total; {report.Summary.Providers.Probable} probable; {report.Summary.Providers.Missing} missing; {report.Summary.Providers.Unknown} unknown; {report.Summary.Providers.WrongScope} wrong-scope");
        builder.AppendLine($"- Capabilities: {report.Summary.Capabilities.Total} total; {report.Summary.Capabilities.Probable} probable; {report.Summary.Capabilities.Missing} missing; {report.Summary.Capabilities.Unknown} unknown; {report.Summary.Capabilities.WrongScope} wrong-scope");
        builder.AppendLine($"- Doctor areas: {report.Summary.Doctor.Areas} area(s); {report.Summary.Doctor.Ready} ready; {report.Summary.Doctor.ActionNeeded} action-needed; {report.Summary.Doctor.Unknown} unknown; {report.Summary.Doctor.Actions} action(s)");
        if (report.Summary.Requirements is null)
        {
            builder.AppendLine("- Requirements: not included");
        }
        else
        {
            builder.AppendLine($"- Requirements: {report.Summary.Requirements.Total} total; {report.Summary.Requirements.Satisfied} satisfied; {report.Summary.Requirements.Missing} missing; {report.Summary.Requirements.Unknown} unknown; {report.Summary.Requirements.WrongScope} wrong-scope; {report.Summary.Requirements.RequiredUnavailable} required unavailable; {report.Summary.Requirements.OptionalUnavailable} optional unavailable");
        }

        builder.AppendLine($"- Diagnostics: {report.Summary.Diagnostics.Issues} issue(s); {report.Summary.Diagnostics.Errors} error(s); {report.Summary.Diagnostics.Warnings} warning(s); {report.Summary.Diagnostics.Notes} note(s)");
        builder.AppendLine($"- Catalogue-policy: {report.Index.CataloguePolicy.OpenQuestions.Count} open question(s); {report.Index.CataloguePolicy.DiagnosticHandoff.Count} handoff item(s)");
        builder.AppendLine();
    }

    private static void AppendDerivedSummary(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Derived Index Summaries");
        builder.AppendLine();
        builder.AppendLine($"- Doctor area capability summaries: {report.Index.DoctorAreaCapabilitySummary.Areas} area(s)");
        builder.AppendLine($"- Provider inventory summaries: {report.Index.ProviderInventorySummary.Providers} provider(s)");
        builder.AppendLine($"- Evidence summaries: {report.Index.EvidenceSummary.EvidenceEntries} evidence item(s) across {report.Index.EvidenceSummary.ProvidersWithEvidence} provider(s)");
        builder.AppendLine($"- Action summaries: {report.Index.ActionSummary.Actions} action(s) across {report.Index.ActionSummary.AreasWithActions} area(s)");
        builder.AppendLine($"- Requirement summaries: {report.Index.RequirementSummary.Requirements} requirement(s); {report.Index.RequirementSummary.Unavailable} unavailable");
        builder.AppendLine($"- Diagnostic summaries: {report.Index.DiagnosticSummary.Issues} issue(s)");
        builder.AppendLine($"- Catalogue-policy summaries: {report.Index.CataloguePolicy.SourceTypeIndex.Count} source type(s); {report.Index.CataloguePolicy.OpenQuestionDetails.Count} open-question detail(s)");
    }

    private static JsonObject ToJson(DoctorExportSummary summary) =>
        new()
        {
            ["catalog"] = new JsonObject
            {
                ["id"] = summary.Catalog.Id,
                ["version"] = summary.Catalog.Version
            },
            ["providers"] = new JsonObject
            {
                ["total"] = summary.Providers.Total,
                ["probable"] = summary.Providers.Probable,
                ["missing"] = summary.Providers.Missing,
                ["unknown"] = summary.Providers.Unknown,
                ["wrongScope"] = summary.Providers.WrongScope
            },
            ["capabilities"] = new JsonObject
            {
                ["total"] = summary.Capabilities.Total,
                ["probable"] = summary.Capabilities.Probable,
                ["missing"] = summary.Capabilities.Missing,
                ["unknown"] = summary.Capabilities.Unknown,
                ["wrongScope"] = summary.Capabilities.WrongScope
            },
            ["doctor"] = new JsonObject
            {
                ["areas"] = summary.Doctor.Areas,
                ["ready"] = summary.Doctor.Ready,
                ["actionNeeded"] = summary.Doctor.ActionNeeded,
                ["unknown"] = summary.Doctor.Unknown,
                ["actions"] = summary.Doctor.Actions
            },
            ["requirements"] = summary.Requirements is null ? null : new JsonObject
            {
                ["total"] = summary.Requirements.Total,
                ["satisfied"] = summary.Requirements.Satisfied,
                ["missing"] = summary.Requirements.Missing,
                ["unknown"] = summary.Requirements.Unknown,
                ["wrongScope"] = summary.Requirements.WrongScope,
                ["requiredUnavailable"] = summary.Requirements.RequiredUnavailable,
                ["optionalUnavailable"] = summary.Requirements.OptionalUnavailable
            },
            ["diagnostics"] = new JsonObject
            {
                ["issues"] = summary.Diagnostics.Issues,
                ["errors"] = summary.Diagnostics.Errors,
                ["warnings"] = summary.Diagnostics.Warnings,
                ["notes"] = summary.Diagnostics.Notes
            }
        };

    private static JsonObject ToJson(CapabilityCataloguePolicyView cataloguePolicy) =>
        new()
        {
            ["sourceTypes"] = cataloguePolicy.SourceTypeIndex.Count,
            ["openQuestions"] = cataloguePolicy.OpenQuestions.Count,
            ["openQuestionDetails"] = cataloguePolicy.OpenQuestionDetails.Count,
            ["diagnosticHandoffItems"] = cataloguePolicy.DiagnosticHandoff.Count
        };

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);
}
