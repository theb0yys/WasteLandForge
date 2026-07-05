using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class ReportsPackageJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(ReportsPackageResult result)
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
            ["command"] = "package",
            ["target"] = result.Target,
            ["dryRun"] = result.DryRun,
            ["status"] = result.Status,
            ["summary"] = new JsonObject
            {
                ["errors"] = result.Diagnostics.ErrorCount,
                ["warnings"] = result.Diagnostics.WarningCount,
                ["notes"] = result.Diagnostics.NoteCount,
                ["entries"] = result.Entries.Count,
                ["presentInputs"] = result.Entries.Count(entry => entry.SourceExists),
                ["missingInputs"] = result.Entries.Count(entry => !entry.SourceExists),
                ["stagedInputs"] = result.Entries.Count(entry => entry.Staged),
                ["unstagedInputs"] = result.Entries.Count(entry => !entry.Staged),
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
            root["outputs"] = ToOutputsJson(result.Outputs);
        }

        root["entries"] = new JsonArray(result.Entries.Select(ToEntryJson).ToArray());
        root["sourceDigests"] = new JsonArray(result.SourceDigests.Select(ToDigestJson).ToArray());
        root["outputDigests"] = new JsonArray(result.OutputDigests.Select(ToDigestJson).ToArray());

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToOutputsJson(ReportsPackageOutputs outputs) =>
        new()
        {
            ["root"] = outputs.Root,
            ["stagingRoot"] = outputs.StagingRoot,
            ["packagePlan"] = outputs.PackagePlan,
            ["stagingLayout"] = outputs.StagingLayout,
            ["packageArchive"] = outputs.PackageArchive,
            ["packageArchiveEvidence"] = outputs.PackageArchiveEvidence,
            ["manifest"] = outputs.BuildManifest,
            ["checksums"] = outputs.Checksums
        };

    private static JsonObject ToEntryJson(ReportsPackageEntry entry) =>
        new()
        {
            ["id"] = entry.Id,
            ["sourcePath"] = entry.SourcePath,
            ["packagePath"] = entry.PackagePath,
            ["plannedStagedPath"] = entry.PlannedStagedPath,
            ["role"] = entry.Role,
            ["mediaType"] = entry.MediaType,
            ["required"] = entry.Required,
            ["sourceExists"] = entry.SourceExists,
            ["inputStatus"] = entry.InputStatus,
            ["staged"] = entry.Staged,
            ["stageStatus"] = entry.StageStatus
        };

    private static JsonObject ToDigestJson(FileDigest digest) =>
        new()
        {
            ["path"] = digest.Path,
            ["sha256"] = digest.Sha256,
            ["length"] = digest.Length
        };
}
