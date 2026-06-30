using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class MetadataReportJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(MetadataReportResult result)
    {
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
            ["command"] = result.Command,
            ["target"] = result.Target,
            ["dryRun"] = result.DryRun,
            ["status"] = result.Status,
            ["summary"] = new JsonObject
            {
                ["errors"] = result.Diagnostics.ErrorCount,
                ["warnings"] = result.Diagnostics.WarningCount,
                ["notes"] = result.Diagnostics.NoteCount,
                ["sources"] = result.SourceDigests.Count,
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

        if (result.Outputs is not null)
        {
            root["outputs"] = ToJson(result.Outputs);
        }

        root["sourceDigests"] = new JsonArray(result.SourceDigests.Select(ToJson).ToArray());
        root["outputDigests"] = new JsonArray(result.OutputDigests.Select(ToJson).ToArray());

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToJson(MetadataReportOutputs outputs)
    {
        var json = new JsonObject
        {
            ["root"] = outputs.Root,
            ["validationReport"] = outputs.ValidationReport,
            ["dependencyReport"] = outputs.DependencyReport,
            ["capabilityReport"] = outputs.CapabilityReport,
            ["runReport"] = outputs.RunReport,
            ["manifest"] = outputs.Manifest
        };
        if (outputs.Checksums is not null)
        {
            json["checksums"] = outputs.Checksums;
        }

        return json;
    }

    private static JsonObject ToJson(FileDigest digest) =>
        new()
        {
            ["path"] = digest.Path,
            ["sha256"] = digest.Sha256,
            ["length"] = digest.Length
        };
}
