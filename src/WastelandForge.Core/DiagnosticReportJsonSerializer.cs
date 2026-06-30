using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace WastelandForge.Core;

public static class DiagnosticReportJsonSerializer
{
    private const string FormatVersion = "1.0";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(DiagnosticReport report, string? toolVersion = null, string command = "validate")
    {
        ArgumentNullException.ThrowIfNull(report);

        var issues = new JsonArray();
        foreach (var issue in report.Issues)
        {
            issues.Add(DiagnosticIssueJsonSerializer.ToJsonNode(issue));
        }

        var root = new JsonObject
        {
            ["formatVersion"] = FormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = "WastelandForge"
            },
            ["command"] = command
        };
        if (!string.IsNullOrWhiteSpace(toolVersion))
        {
            root["tool"]!["version"] = toolVersion;
        }

        if (report.ProjectId is not null)
        {
            root["project"] = new JsonObject
            {
                ["id"] = report.ProjectId.ToString()
            };
        }

        root["summary"] = new JsonObject
        {
            ["errors"] = report.ErrorCount,
            ["warnings"] = report.WarningCount,
            ["notes"] = report.NoteCount
        };
        root["issues"] = issues;

        return root.ToJsonString(SerializerOptions);
    }
}
