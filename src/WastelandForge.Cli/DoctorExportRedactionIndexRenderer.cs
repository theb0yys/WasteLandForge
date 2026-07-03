using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportRedactionIndexRenderer
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
            ["kind"] = "wastelandforge/doctor-redaction-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = new JsonObject
            {
                ["tokens"] = report.Redaction.Tokens.Count,
                ["notes"] = report.Redaction.Notes.Count
            },
            ["tokens"] = new JsonArray(report.Redaction.Tokens.Select(token => JsonValue.Create(token)).ToArray()),
            ["notes"] = new JsonArray(report.Redaction.Notes.Select(note => JsonValue.Create(note)).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Redaction");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this redaction index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Mode: `{EscapeInline(report.Redaction.Mode)}`");
        builder.AppendLine($"- Paths: `{EscapeInline(report.Redaction.Paths)}`");
        builder.AppendLine($"- Tokens: {report.Redaction.Tokens.Count}");
        builder.AppendLine($"- Notes: {report.Redaction.Notes.Count}");
        builder.AppendLine();

        AppendList(builder, "Tokens", report.Redaction.Tokens);
        AppendList(builder, "Notes", report.Redaction.Notes);

        return builder.ToString();
    }

    private static void AppendList(StringBuilder builder, string title, IReadOnlyList<string> values)
    {
        builder.Append("## ");
        builder.AppendLine(title);
        builder.AppendLine();
        if (values.Count == 0)
        {
            builder.AppendLine("None.");
            builder.AppendLine();
            return;
        }

        foreach (var value in values)
        {
            builder.Append("- `");
            builder.Append(EscapeInline(value));
            builder.AppendLine("`");
        }

        builder.AppendLine();
    }

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);
}
