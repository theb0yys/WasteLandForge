using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class DocsReferenceIndexJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(DocsReferenceIndexResult result)
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
            ["sections"] = new JsonArray(result.Sections.Select(ToJson).ToArray()),
            ["schemaReferences"] = new JsonArray(result.SchemaReferences.Select(ToJson).ToArray()),
            ["registryReferences"] = new JsonArray(result.RegistryReferences.Select(ToJson).ToArray()),
            ["ruleReferences"] = new JsonArray(result.RuleReferences.Select(ToJson).ToArray()),
            ["capabilityReferences"] = new JsonArray(result.CapabilityReferences.Select(ToJson).ToArray()),
            ["providerReferences"] = new JsonArray(result.ProviderReferences.Select(ToJson).ToArray()),
            ["commandReferences"] = new JsonArray(result.CommandReferences.Select(ToJson).ToArray()),
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

    private static JsonObject ToJson(DocsReferenceIndexSummary summary, DocsReferenceIndexResult result) =>
        new()
        {
            ["errors"] = result.Diagnostics.ErrorCount,
            ["warnings"] = result.Diagnostics.WarningCount,
            ["notes"] = result.Diagnostics.NoteCount,
            ["schemas"] = summary.Schemas,
            ["schemaReferences"] = summary.SchemaReferences,
            ["registries"] = summary.Registries,
            ["registryReferences"] = summary.RegistryReferences,
            ["ruleFamilies"] = summary.RuleFamilies,
            ["ruleReferences"] = summary.RuleReferences,
            ["capabilities"] = summary.Capabilities,
            ["capabilityReferences"] = summary.CapabilityReferences,
            ["providers"] = summary.Providers,
            ["providerReferences"] = summary.ProviderReferences,
            ["commands"] = summary.Commands,
            ["commandReferences"] = summary.CommandReferences,
            ["sources"] = result.SourceDigests.Count,
            ["outputs"] = result.OutputDigests.Count
        };

    private static JsonObject ToJson(DocsReferenceIndexOutputs outputs) =>
        new()
        {
            ["root"] = outputs.Root,
            ["referenceIndexJson"] = outputs.ReferenceIndexJson,
            ["referenceIndexMarkdown"] = outputs.ReferenceIndexMarkdown,
            ["schemaReferenceJson"] = ToJsonArray(outputs.SchemaReferenceJson),
            ["schemaReferenceMarkdown"] = ToJsonArray(outputs.SchemaReferenceMarkdown),
            ["registryReferenceJson"] = ToJsonArray(outputs.RegistryReferenceJson),
            ["registryReferenceMarkdown"] = ToJsonArray(outputs.RegistryReferenceMarkdown),
            ["ruleReferenceJson"] = ToJsonArray(outputs.RuleReferenceJson),
            ["ruleReferenceMarkdown"] = ToJsonArray(outputs.RuleReferenceMarkdown),
            ["capabilityReferenceJson"] = ToJsonArray(outputs.CapabilityReferenceJson),
            ["capabilityReferenceMarkdown"] = ToJsonArray(outputs.CapabilityReferenceMarkdown),
            ["providerReferenceJson"] = ToJsonArray(outputs.ProviderReferenceJson),
            ["providerReferenceMarkdown"] = ToJsonArray(outputs.ProviderReferenceMarkdown),
            ["commandReferenceJson"] = ToJsonArray(outputs.CommandReferenceJson),
            ["commandReferenceMarkdown"] = ToJsonArray(outputs.CommandReferenceMarkdown),
            ["manifest"] = outputs.Manifest,
            ["checksums"] = outputs.Checksums
        };

    private static JsonObject ToJson(DocsReferenceIndexSection section) =>
        new()
        {
            ["id"] = section.Id,
            ["title"] = section.Title,
            ["entries"] = new JsonArray(section.Entries.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(DocsReferenceIndexEntry entry)
    {
        var json = new JsonObject
        {
            ["id"] = entry.Id,
            ["title"] = entry.Title,
            ["kind"] = entry.Kind,
            ["source"] = entry.Source
        };
        if (entry.Version is not null)
        {
            json["version"] = entry.Version;
        }

        if (entry.Description is not null)
        {
            json["description"] = entry.Description;
        }

        return json;
    }

    private static JsonObject ToJson(DocsSchemaReferencePage page)
    {
        var json = new JsonObject
        {
            ["schemaId"] = page.SchemaId,
            ["kind"] = page.Kind,
            ["version"] = page.Version,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["schemaSha256"] = page.SchemaSha256,
            ["schemaLength"] = page.SchemaLength,
            ["required"] = ToJsonArray(page.Required),
            ["topLevelProperties"] = ToJsonArray(page.TopLevelProperties)
        };

        if (page.Title is not null)
        {
            json["title"] = page.Title;
        }

        if (page.Description is not null)
        {
            json["description"] = page.Description;
        }

        if (page.Type is not null)
        {
            json["type"] = page.Type;
        }

        return json;
    }

    private static JsonObject ToJson(DocsRegistryReferencePage page)
    {
        var json = new JsonObject
        {
            ["registryId"] = page.RegistryId,
            ["title"] = page.Title,
            ["group"] = page.RegistryGroup,
            ["source"] = page.Source,
            ["format"] = page.Format,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["sourceSha256"] = page.SourceSha256,
            ["sourceLength"] = page.SourceLength,
            ["parseStatus"] = page.ParseStatus,
            ["topLevelProperties"] = ToJsonArray(page.TopLevelProperties)
        };
        if (page.ItemCount is not null)
        {
            json["itemCount"] = page.ItemCount.Value;
        }

        return json;
    }

    private static JsonObject ToJson(DocsRuleReferencePage page) =>
        new()
        {
            ["ruleFamilyId"] = page.RuleFamilyId,
            ["prefix"] = page.Prefix,
            ["title"] = page.Title,
            ["scope"] = page.Scope,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["knownDiagnostics"] = ToJsonArray(page.KnownDiagnosticIds),
            ["knownDiagnosticCount"] = page.KnownDiagnosticIds.Count
        };

    private static JsonObject ToJson(DocsCapabilityReferencePage page) =>
        new()
        {
            ["capabilityId"] = page.CapabilityId,
            ["title"] = page.Title,
            ["description"] = page.Description,
            ["catalogId"] = page.CatalogId,
            ["catalogVersion"] = page.CatalogVersion,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["satisfiedBy"] = ToJsonArray(page.SatisfiedBy),
            ["satisfiedByCount"] = page.SatisfiedBy.Count
        };

    private static JsonObject ToJson(DocsProviderReferencePage page) =>
        new()
        {
            ["providerId"] = page.ProviderId,
            ["title"] = page.Title,
            ["providerType"] = page.ProviderType,
            ["installScope"] = page.InstallScope,
            ["catalogId"] = page.CatalogId,
            ["catalogVersion"] = page.CatalogVersion,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["capabilities"] = ToJsonArray(page.Capabilities),
            ["capabilityCount"] = page.Capabilities.Count,
            ["detectorKinds"] = ToJsonArray(page.DetectorKinds),
            ["detectorKindCount"] = page.DetectorKinds.Count,
            ["notes"] = ToJsonArray(page.Notes),
            ["noteCount"] = page.Notes.Count,
            ["version"] = ProviderVersionDeclarationProjection.ToJson(page.Version)
        };

    private static JsonObject ToJson(DocsCommandReferencePage page) =>
        new()
        {
            ["commandId"] = page.CommandId,
            ["command"] = page.CommandText,
            ["title"] = page.Title,
            ["commandGroup"] = page.CommandGroup,
            ["surfaceStatus"] = page.SurfaceStatus,
            ["source"] = page.Source,
            ["researchSource"] = page.ResearchSource,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["notes"] = ToJsonArray(page.Notes),
            ["noteCount"] = page.Notes.Count
        };

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static JsonObject ToJson(FileDigest digest) =>
        new()
        {
            ["path"] = digest.Path,
            ["sha256"] = digest.Sha256,
            ["length"] = digest.Length
        };
}
