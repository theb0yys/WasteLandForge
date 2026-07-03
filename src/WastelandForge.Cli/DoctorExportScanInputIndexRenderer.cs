using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class DoctorExportScanInputIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var inputs = report.Capabilities.Inputs;
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-scan-input-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = new JsonObject
            {
                ["gameRootProvided"] = inputs.GameRoot is not null,
                ["dataRootProvided"] = inputs.DataRoot is not null,
                ["toolPaths"] = inputs.ToolPaths.Count,
                ["detectorFamilies"] = inputs.DetectorFamilies.Count,
                ["runtimeProbesEnabled"] = inputs.RuntimeProbesEnabled,
                ["mo2VfsEnabled"] = inputs.Mo2VfsEnabled
            },
            ["inputs"] = ToJson(inputs)
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var inputs = report.Capabilities.Inputs;
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Scan Inputs");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this scan-input index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Game root provided: {FormatBool(inputs.GameRoot is not null)}");
        builder.AppendLine($"- Data root provided: {FormatBool(inputs.DataRoot is not null)}");
        builder.AppendLine($"- Tool paths: {inputs.ToolPaths.Count}");
        builder.AppendLine($"- Detector families: {inputs.DetectorFamilies.Count}");
        builder.AppendLine($"- Runtime probes enabled: {FormatBool(inputs.RuntimeProbesEnabled)}");
        builder.AppendLine($"- MO2 VFS enabled: {FormatBool(inputs.Mo2VfsEnabled)}");
        builder.AppendLine();

        builder.AppendLine("## Redacted Inputs");
        builder.AppendLine();
        builder.AppendLine($"- Game root: `{EscapeInline(inputs.GameRoot ?? "(not provided)")}`");
        builder.AppendLine($"- Data root: `{EscapeInline(inputs.DataRoot ?? "(not provided)")}`");
        builder.AppendLine($"- Tool paths: {JoinInline(inputs.ToolPaths)}");
        builder.AppendLine($"- Detector families: {JoinInline(inputs.DetectorFamilies)}");

        return builder.ToString();
    }

    private static JsonObject ToJson(CapabilityScanInputs inputs) =>
        new()
        {
            ["gameRoot"] = inputs.GameRoot,
            ["dataRoot"] = inputs.DataRoot,
            ["toolPaths"] = new JsonArray(inputs.ToolPaths.Select(path => JsonValue.Create(path)).ToArray()),
            ["detectorFamilies"] = new JsonArray(inputs.DetectorFamilies.Select(family => JsonValue.Create(family)).ToArray()),
            ["runtimeProbesEnabled"] = inputs.RuntimeProbesEnabled,
            ["mo2VfsEnabled"] = inputs.Mo2VfsEnabled
        };

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string FormatBool(bool value) =>
        value.ToString().ToLowerInvariant();

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);
}
