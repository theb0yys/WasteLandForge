using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class ReleaseDryRunJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(ReleaseDryRunResult result)
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
            ["command"] = "release verify",
            ["dryRun"] = result.DryRun,
            ["status"] = result.Status,
            ["summary"] = new JsonObject
            {
                ["errors"] = result.Diagnostics.ErrorCount,
                ["warnings"] = result.Diagnostics.WarningCount,
                ["notes"] = result.Diagnostics.NoteCount
            },
            ["issues"] = issues
        };

        var project = CreateProjectJson(result);
        if (project.Count > 0)
        {
            root["project"] = project;
        }

        if (result.Outputs is not null)
        {
            root["outputs"] = new JsonObject
            {
                ["root"] = result.Outputs.Root,
                ["stagingRoot"] = result.Outputs.StagingRoot,
                ["validationReport"] = result.Outputs.ValidationReport,
                ["releaseSummary"] = result.Outputs.ReleaseSummary,
                ["buildManifest"] = result.Outputs.BuildManifest,
                ["checksums"] = result.Outputs.Checksums
            };
        }

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject CreateProjectJson(ReleaseDryRunResult result)
    {
        var project = new JsonObject();
        if (result.ProjectId is not null)
        {
            project["id"] = result.ProjectId.ToString();
        }

        if (!string.IsNullOrWhiteSpace(result.ProjectName))
        {
            project["name"] = result.ProjectName;
        }

        if (!string.IsNullOrWhiteSpace(result.ProjectVersion))
        {
            project["version"] = result.ProjectVersion;
        }

        return project;
    }
}
