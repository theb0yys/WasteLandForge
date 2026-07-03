using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportCataloguePolicyIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var cataloguePolicy = report.Index.CataloguePolicy;
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-catalogue-policy-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = new JsonObject
            {
                ["sourceTypes"] = cataloguePolicy.SourceTypeIndex.Count,
                ["openQuestions"] = cataloguePolicy.OpenQuestions.Count,
                ["openQuestionDetails"] = cataloguePolicy.OpenQuestionDetails.Count,
                ["diagnosticHandoffItems"] = cataloguePolicy.DiagnosticHandoff.Count
            },
            ["sourceTypeIndex"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToSourceTypeIndexJson(cataloguePolicy),
            ["openQuestionDetails"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToOpenQuestionDetailsJson(cataloguePolicy),
            ["diagnosticHandoff"] = CapabilityCataloguePolicyHandoffRenderer.ToJson(cataloguePolicy),
            ["openQuestions"] = new JsonArray(cataloguePolicy.OpenQuestions.Select(question => JsonValue.Create(question)).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var cataloguePolicy = report.Index.CataloguePolicy;
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Catalogue Policy");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this catalogue-policy index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Source types: {cataloguePolicy.SourceTypeIndex.Count}");
        builder.AppendLine($"- Open questions: {cataloguePolicy.OpenQuestions.Count}");
        builder.AppendLine($"- Open-question details: {cataloguePolicy.OpenQuestionDetails.Count}");
        builder.AppendLine($"- Diagnostic handoff items: {cataloguePolicy.DiagnosticHandoff.Count}");
        builder.AppendLine();

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendSourceTypeIndexText(
            builder,
            cataloguePolicy,
            string.Empty,
            "- ",
            "  ",
            "## Source Types");
        builder.AppendLine();

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendOpenQuestionDetailsText(
            builder,
            cataloguePolicy,
            string.Empty,
            "- ",
            "## Open Question Details");
        builder.AppendLine();

        CapabilityCataloguePolicyHandoffRenderer.AppendText(
            builder,
            cataloguePolicy,
            "## ",
            "- ",
            "  ");
        builder.AppendLine();

        AppendOpenQuestions(builder, cataloguePolicy);

        return builder.ToString();
    }

    private static void AppendOpenQuestions(StringBuilder builder, CapabilityCataloguePolicyView cataloguePolicy)
    {
        builder.AppendLine("## Open Questions");
        builder.AppendLine();
        if (cataloguePolicy.OpenQuestions.Count == 0)
        {
            builder.AppendLine("No catalogue-policy open questions.");
            return;
        }

        foreach (var question in cataloguePolicy.OpenQuestions)
        {
            builder.AppendLine($"- {question}");
        }
    }
}
