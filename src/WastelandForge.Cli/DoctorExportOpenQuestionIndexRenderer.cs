using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportOpenQuestionIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var openQuestions = report.Index.CataloguePolicy;
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-open-question-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = new JsonObject
            {
                ["sourceTypes"] = openQuestions.SourceTypeIndex.Count,
                ["openQuestions"] = openQuestions.OpenQuestions.Count,
                ["openQuestionDetails"] = openQuestions.OpenQuestionDetails.Count,
                ["diagnosticHandoffItems"] = openQuestions.DiagnosticHandoff.Count
            },
            ["sourceTypeIndex"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToSourceTypeIndexJson(openQuestions),
            ["openQuestionDetails"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToOpenQuestionDetailsJson(openQuestions),
            ["diagnosticHandoff"] = CapabilityCataloguePolicyHandoffRenderer.ToJson(openQuestions),
            ["openQuestions"] = new JsonArray(openQuestions.OpenQuestions.Select(question => JsonValue.Create(question)).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var openQuestions = report.Index.CataloguePolicy;
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Open Questions");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this open-question index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Source types: {openQuestions.SourceTypeIndex.Count}");
        builder.AppendLine($"- Open questions: {openQuestions.OpenQuestions.Count}");
        builder.AppendLine($"- Open-question details: {openQuestions.OpenQuestionDetails.Count}");
        builder.AppendLine($"- Diagnostic handoff items: {openQuestions.DiagnosticHandoff.Count}");
        builder.AppendLine();

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendSourceTypeIndexText(
            builder,
            openQuestions,
            string.Empty,
            "- ",
            "  ",
            "## Source Types");
        builder.AppendLine();

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendOpenQuestionDetailsText(
            builder,
            openQuestions,
            string.Empty,
            "- ",
            "## Open Question Details");
        builder.AppendLine();

        CapabilityCataloguePolicyHandoffRenderer.AppendText(
            builder,
            openQuestions,
            "## ",
            "- ",
            "  ");
        builder.AppendLine();

        AppendOpenQuestions(builder, openQuestions);

        return builder.ToString();
    }

    private static void AppendOpenQuestions(StringBuilder builder, CapabilityCataloguePolicyView openQuestions)
    {
        builder.AppendLine("## Open Questions");
        builder.AppendLine();
        if (openQuestions.OpenQuestions.Count == 0)
        {
            builder.AppendLine("No open questions.");
            return;
        }

        foreach (var question in openQuestions.OpenQuestions)
        {
            builder.AppendLine($"- {question}");
        }
    }
}
