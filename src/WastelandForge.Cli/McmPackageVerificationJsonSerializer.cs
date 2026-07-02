using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Cli;

internal static class McmPackageVerificationJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(McmPackageVerificationCliResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var issues = new JsonArray();
        foreach (var issue in result.Diagnostics.Issues)
        {
            issues.Add(DiagnosticIssueJsonSerializer.ToJsonNode(issue));
        }

        var outputs = new JsonObject
        {
            ["root"] = result.Root,
            ["packageManifest"] = result.PackageManifest,
            ["installPreview"] = result.InstallPreview,
            ["installPreviewSummary"] = result.InstallPreviewSummary,
            ["installPlan"] = result.InstallPlan,
            ["installPlanSummary"] = result.InstallPlanSummary,
            ["packageVerification"] = result.PackageVerification,
            ["packageVerificationSummary"] = result.PackageVerificationSummary,
            ["checksums"] = result.Checksums,
            ["buildManifest"] = result.BuildManifest
        };
        if (result.PackageArchive is not null)
        {
            outputs["packageArchive"] = result.PackageArchive;
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
            ["mode"] = result.Mode,
            ["status"] = result.Status,
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["summary"] = new JsonObject
            {
                ["errors"] = result.Diagnostics.ErrorCount,
                ["warnings"] = result.Diagnostics.WarningCount,
                ["notes"] = result.Diagnostics.NoteCount
            },
            ["outputs"] = outputs,
            ["issues"] = issues
        };

        return root.ToJsonString(SerializerOptions);
    }
}
