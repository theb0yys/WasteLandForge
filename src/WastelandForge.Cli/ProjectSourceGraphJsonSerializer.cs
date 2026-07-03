using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class ProjectSourceGraphJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(ProjectSourceGraphResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

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
            ["summary"] = ToJson(result.Summary, result),
            ["issues"] = new JsonArray(result.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray()),
            ["nodes"] = new JsonArray(result.Nodes.Select(ToJson).ToArray()),
            ["edges"] = new JsonArray(result.Edges.Select(ToJson).ToArray()),
            ["sourceDigests"] = new JsonArray(result.SourceDigests.Select(ToJson).ToArray()),
            ["outputDigests"] = new JsonArray(result.OutputDigests.Select(ToJson).ToArray())
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

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToJson(ProjectSourceGraphSummary summary, ProjectSourceGraphResult result) =>
        new()
        {
            ["errors"] = result.Diagnostics.ErrorCount,
            ["warnings"] = result.Diagnostics.WarningCount,
            ["notes"] = result.Diagnostics.NoteCount,
            ["nodes"] = summary.Nodes,
            ["edges"] = summary.Edges,
            ["sourceDocuments"] = summary.SourceDocuments,
            ["manifestDocuments"] = summary.ManifestDocuments,
            ["registryDocuments"] = summary.RegistryDocuments,
            ["outputBoundaries"] = summary.OutputBoundaries,
            ["capabilityRequirements"] = summary.CapabilityRequirements,
            ["requiredCapabilityRequirements"] = summary.RequiredCapabilityRequirements,
            ["optionalCapabilityRequirements"] = summary.OptionalCapabilityRequirements,
            ["referencedCapabilities"] = summary.ReferencedCapabilities,
            ["referencedProviders"] = summary.ReferencedProviders,
            ["catalogueCapabilities"] = summary.CatalogueCapabilities,
            ["catalogueProviders"] = summary.CatalogueProviders,
            ["generatorTargets"] = summary.GeneratorTargets,
            ["generatorTargetInputEdges"] = summary.GeneratorTargetInputEdges,
            ["generatorTargetOutputEdges"] = summary.GeneratorTargetOutputEdges,
            ["sources"] = result.SourceDigests.Count,
            ["outputs"] = result.OutputDigests.Count
        };

    private static JsonObject ToJson(ProjectSourceGraphOutputs outputs) =>
        new()
        {
            ["root"] = outputs.Root,
            ["graphJson"] = outputs.GraphJson,
            ["graphMarkdown"] = outputs.GraphMarkdown,
            ["manifest"] = outputs.Manifest,
            ["checksums"] = outputs.Checksums
        };

    private static JsonObject ToJson(ProjectSourceGraphNode node)
    {
        var json = new JsonObject
        {
            ["id"] = node.Id,
            ["kind"] = node.Kind,
            ["label"] = node.Label
        };
        if (node.Path is not null)
        {
            json["path"] = node.Path;
        }

        if (node.Boundary is not null)
        {
            json["boundary"] = node.Boundary;
        }

        return json;
    }

    private static JsonObject ToJson(ProjectSourceGraphEdge edge) =>
        new()
        {
            ["from"] = edge.From,
            ["to"] = edge.To,
            ["kind"] = edge.Kind
        };

    private static JsonObject ToJson(FileDigest digest) =>
        new()
        {
            ["path"] = digest.Path,
            ["sha256"] = digest.Sha256,
            ["length"] = digest.Length
        };
}
