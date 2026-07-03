using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class XEditAuditGenerateJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(string command, XEditAuditScriptScaffoldResult result)
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
            ["target"] = result.Target,
            ["status"] = result.HasErrors ? "failed" : "passed",
            ["summary"] = new JsonObject
            {
                ["errors"] = result.Diagnostics.ErrorCount,
                ["warnings"] = result.Diagnostics.WarningCount,
                ["notes"] = result.Diagnostics.NoteCount,
                ["plannedAudits"] = result.PlanEntries.Count,
                ["generatedScaffolds"] = result.GeneratedFiles.Count,
                ["outputs"] = result.OutputDigests.Count
            },
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

        root["scaffolds"] = new JsonArray(result.GeneratedFiles.Select(ToScaffoldJson).ToArray());
        root["outputDigests"] = new JsonArray(result.OutputDigests.Select(ToDigestJson).ToArray());

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToOutputsJson(XEditAuditScriptScaffoldResult result)
    {
        var json = new JsonObject
        {
            ["root"] = $"generated/{result.Target}",
            ["scaffolds"] = new JsonArray(result.GeneratedFiles.Select(file => JsonValue.Create(file.OutputPath)).ToArray())
        };

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

    private static JsonObject ToScaffoldJson(XEditAuditScriptScaffoldFile file) =>
        new()
        {
            ["id"] = file.AuditId,
            ["outputPath"] = file.OutputPath,
            ["expectedReportPath"] = file.ExpectedReportPath,
            ["contentBytes"] = file.ContentBytes,
            ["source"] = new JsonObject
            {
                ["file"] = file.Source.File,
                ["pointer"] = file.Source.Pointer?.ToString()
            }
        };

    private static JsonObject ToDigestJson(FileDigest digest) =>
        new()
        {
            ["path"] = digest.Path,
            ["sha256"] = digest.Sha256,
            ["length"] = digest.Length
        };
}
