using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class JipScriptBuildJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(JipScriptBuildResult result)
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
            ["command"] = "build",
            ["target"] = result.Target,
            ["dryRun"] = result.DryRun,
            ["status"] = result.Status,
            ["summary"] = new JsonObject
            {
                ["errors"] = result.Diagnostics.ErrorCount,
                ["warnings"] = result.Diagnostics.WarningCount,
                ["notes"] = result.Diagnostics.NoteCount,
                ["plannedScripts"] = result.PlanEntries.Count,
                ["builtScripts"] = result.BuiltFiles.Count,
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

        root["scripts"] = new JsonArray(result.BuiltFiles.Select(ToScriptJson).ToArray());
        root["sourceDigests"] = new JsonArray(result.SourceDigests.Select(ToDigestJson).ToArray());
        root["outputDigests"] = new JsonArray(result.OutputDigests.Select(ToDigestJson).ToArray());

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToOutputsJson(JipScriptBuildOutputs outputs) =>
        new()
        {
            ["root"] = outputs.Root,
            ["scripts"] = new JsonArray(outputs.Scripts.Select(script => JsonValue.Create(script)).ToArray()),
            ["manifest"] = outputs.BuildManifest,
            ["checksums"] = outputs.Checksums
        };

    private static JsonObject ToScriptJson(JipScriptBuildFile file) =>
        new()
        {
            ["id"] = file.ScriptId,
            ["outputFile"] = file.OutputFile,
            ["outputPath"] = file.OutputPath,
            ["dataPath"] = file.DataPath,
            ["installPath"] = file.InstallPath,
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
