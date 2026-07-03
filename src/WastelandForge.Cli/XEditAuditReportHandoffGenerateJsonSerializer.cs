using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class XEditAuditReportHandoffGenerateJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(string command, XEditAuditReportHandoffEmissionResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(result);

        var issues = new JsonArray();
        foreach (var issue in result.Diagnostics.Issues)
        {
            issues.Add(DiagnosticIssueJsonSerializer.ToJsonNode(issue));
        }

        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = command,
            ["target"] = XEditAuditReportHandoffEmitter.CommandTarget,
            ["auditTarget"] = result.Target,
            ["status"] = result.HasErrors ? "failed" : "passed",
            ["summary"] = ToSummaryJson(result),
            ["issues"] = issues
        };

        if (result.ProjectId is not null)
        {
            root["project"] = new JsonObject
            {
                ["id"] = result.ProjectId.ToString(),
                ["root"] = result.ProjectRoot
            };
        }

        if (!result.HasErrors)
        {
            root["outputs"] = ToOutputsJson(result);
        }

        root["handoff"] = ToHandoffJson(result.Projection);
        root["files"] = new JsonArray(result.GeneratedFiles.Select(ToGeneratedFileJson).ToArray());
        root["outputDigests"] = new JsonArray(result.OutputDigests.Select(ToDigestJson).ToArray());

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToSummaryJson(XEditAuditReportHandoffEmissionResult result) =>
        new()
        {
            ["errors"] = result.Diagnostics.ErrorCount,
            ["warnings"] = result.Diagnostics.WarningCount,
            ["notes"] = result.Diagnostics.NoteCount,
            ["plannedAudits"] = result.Projection.Summary.PlannedAudits,
            ["parsedReports"] = result.Projection.Summary.ParsedReports,
            ["records"] = result.Projection.Summary.Records,
            ["findings"] = result.Projection.Summary.Findings,
            ["generatedFiles"] = result.GeneratedFiles.Count,
            ["outputs"] = result.OutputDigests.Count
        };

    private static JsonObject ToOutputsJson(XEditAuditReportHandoffEmissionResult result)
    {
        var json = new JsonObject
        {
            ["root"] = $"generated/{result.Target}"
        };

        foreach (var file in result.GeneratedFiles)
        {
            if (StringComparer.Ordinal.Equals(file.ContentKind, "json"))
            {
                json["handoffJson"] = file.OutputPath;
            }
            else if (StringComparer.Ordinal.Equals(file.ContentKind, "text"))
            {
                json["handoffText"] = file.OutputPath;
            }
        }

        if (result.ManifestPath is not null)
        {
            json["manifest"] = result.ManifestPath;
        }

        if (result.ChecksumsPath is not null)
        {
            json["checksums"] = result.ChecksumsPath;
        }

        return json;
    }

    private static JsonObject ToHandoffJson(XEditAuditReportEvidenceProjection projection) =>
        new()
        {
            ["status"] = projection.Status,
            ["plannedAudits"] = projection.Summary.PlannedAudits,
            ["parsedReports"] = projection.Summary.ParsedReports,
            ["records"] = projection.Summary.Records,
            ["findings"] = projection.Summary.Findings,
            ["errorFindings"] = projection.Summary.ErrorFindings,
            ["warningFindings"] = projection.Summary.WarningFindings,
            ["noteFindings"] = projection.Summary.NoteFindings,
            ["reports"] = new JsonArray(projection.Reports.Select(ToReportJson).ToArray())
        };

    private static JsonObject ToReportJson(XEditAuditReport report) =>
        new()
        {
            ["auditId"] = report.AuditId,
            ["reportPath"] = report.ReportPath,
            ["records"] = report.Summary.Records,
            ["findings"] = report.Summary.Findings,
            ["errorFindings"] = report.Summary.ErrorFindings,
            ["warningFindings"] = report.Summary.WarningFindings,
            ["noteFindings"] = report.Summary.NoteFindings
        };

    private static JsonObject ToGeneratedFileJson(XEditAuditReportHandoffFile file) =>
        new()
        {
            ["path"] = file.OutputPath,
            ["contentKind"] = file.ContentKind,
            ["lineEnding"] = file.LineEnding,
            ["encoding"] = file.Encoding,
            ["contentBytes"] = file.ContentBytes
        };

    private static JsonObject ToDigestJson(FileDigest digest) =>
        new()
        {
            ["path"] = digest.Path,
            ["sha256"] = digest.Sha256,
            ["length"] = digest.Length
        };
}
