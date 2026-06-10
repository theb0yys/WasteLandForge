using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Schema;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace WastelandForge.Validation;

public sealed class ProjectValidationPipeline
{
    private static readonly IReadOnlyDictionary<string, string[]> AssetTargetExtensions = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["mesh"] = [".nif"],
        ["texture"] = [".dds"],
        ["audio"] = [".wav", ".ogg"],
        ["voice"] = [".wav", ".ogg"],
        ["lip"] = [".lip"],
        ["animation"] = [".kf", ".kfm", ".rdt"],
        ["interface"] = [".xml"],
        ["config"] = [".json", ".ini"],
        ["script"] = [".txt"],
        ["documentation"] = [".md", ".txt"]
    };

    private static readonly IReadOnlyDictionary<string, string[]> AssetTargetPrefixes = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["mesh"] = ["meshes/"],
        ["texture"] = ["textures/"],
        ["audio"] = ["sound/"],
        ["voice"] = ["sound/voice/"],
        ["lip"] = ["sound/voice/"],
        ["animation"] = ["meshes/"],
        ["interface"] = ["menus/", "textures/interface/"],
        ["config"] = ["config/"]
    };

    private static readonly Lazy<JsonSchema> ManifestSchema = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Manifest010,
        "Manifest schema 0.1.0"));
    private static readonly Lazy<JsonSchema> DependencyRegistrySchema = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Dependency010,
        "Dependency registry schema 0.1.0"));
    private static readonly Lazy<JsonSchema> CapabilityRegistrySchema = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Capability010,
        "Capability registry schema 0.1.0"));
    private static readonly Lazy<JsonSchema> AssetRegistrySchema = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Asset010,
        "Asset registry schema 0.1.0"));
    private static readonly Lazy<JsonSchema> QuestRegistrySchema = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Quest010,
        "Quest registry schema 0.1.0"));
    private static readonly Lazy<JsonSchema> QuestRegistrySchema020 = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Quest020,
        "Quest registry schema 0.2.0"));
    private static readonly Lazy<JsonSchema> QuestRegistrySchema030 = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Quest030,
        "Quest registry schema 0.3.0"));
    private static readonly Lazy<JsonSchema> QuestRegistrySchema040 = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Quest040,
        "Quest registry schema 0.4.0"));
    private static readonly Lazy<JsonSchema> DialogueRegistrySchema = new(() => LoadBuiltInSchema(
        WastelandForgeSchemaIds.Dialogue010,
        "Dialogue registry schema 0.1.0"));

    private static JsonSchema LoadBuiltInSchema(string schemaId, string label)
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(schemaId, out var resource) ||
            resource is null)
        {
            throw new InvalidOperationException($"{label} is not registered in the built-in schema catalog.");
        }

        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource));
    }

    public DiagnosticReport Validate(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var projectRoot = Path.GetFullPath(projectPath);
        var issues = new List<DiagnosticIssue>();
        var manifestLoad = LoadManifest(projectRoot, issues);
        LogicalId? projectId = null;

        if (manifestLoad is not null)
        {
            projectId = ReadProjectId(manifestLoad.Manifest);
            var issueCountBeforeSchemaValidation = issues.Count;
            ValidateManifestSchema(manifestLoad, issues, projectId);
            if (!HasErrorSince(issues, issueCountBeforeSchemaValidation))
            {
                RunSemanticPlaceholder(projectId, manifestLoad, issues);
                RunCapabilityPlaceholder();
            }
        }

        return new DiagnosticReport(projectId, issues);
    }

    private static LoadedManifest? LoadManifest(string projectRoot, List<DiagnosticIssue> issues)
    {
        if (!Directory.Exists(projectRoot))
        {
            issues.Add(CreateIssue(
                "WF-LOAD-001",
                DiagnosticSeverity.Error,
                "load",
                "Project root not found",
                $"Project root '{projectRoot}' does not exist.",
                new SourceLocation(projectRoot),
                docsRule: "WF-LOAD-001"));
            return null;
        }

        var candidates = new[]
            {
                Path.Combine(projectRoot, "wastelandforge.json"),
                Path.Combine(projectRoot, "wastelandforge.yaml"),
                Path.Combine(projectRoot, "wastelandforge.yml")
            }
            .Where(File.Exists)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (candidates.Length == 0)
        {
            issues.Add(CreateIssue(
                "WF-LOAD-001",
                DiagnosticSeverity.Error,
                "load",
                "Manifest not found",
                "Expected wastelandforge.json, wastelandforge.yaml, or wastelandforge.yml in the project root.",
                new SourceLocation("."),
                docsRule: "WF-LOAD-001"));
            return null;
        }

        if (candidates.Length > 1)
        {
            issues.Add(CreateIssue(
                "WF-LOAD-002",
                DiagnosticSeverity.Error,
                "load",
                "Multiple manifests found",
                "Only one WastelandForge root manifest is allowed.",
                new SourceLocation(ToDisplayPath(projectRoot, candidates[0])),
                relatedLocations: candidates.Skip(1).Select(path => new SourceLocation(ToDisplayPath(projectRoot, path))).ToArray(),
                docsRule: "WF-LOAD-002"));
            return null;
        }

        var document = LoadSourceDocument(projectRoot, candidates[0], "Manifest", issues);
        return document is null
            ? null
            : new LoadedManifest(
                projectRoot,
                document.Path,
                document.DisplayPath,
                document.Root,
                document.SourceLocations);
    }

    private static LoadedSourceDocument? LoadSourceDocument(
        string projectRoot,
        string path,
        string label,
        List<DiagnosticIssue> issues,
        LogicalId? projectId = null)
    {
        var displayPath = ToDisplayPath(projectRoot, path);
        var extension = Path.GetExtension(path);
        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            return LoadJsonSourceDocument(path, displayPath, label, issues, projectId);
        }

        if (extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yml", StringComparison.OrdinalIgnoreCase))
        {
            return LoadYamlSourceDocument(path, displayPath, label, issues, projectId);
        }

        issues.Add(CreateIssue(
            "WF-LOAD-003",
            DiagnosticSeverity.Error,
            "load",
            "Unsupported source contract extension",
            $"{label} source contract '{displayPath}' must be JSON or YAML.",
            new SourceLocation(displayPath),
            projectId,
            docsRule: "WF-LOAD-003"));
        return null;
    }

    private static LoadedSourceDocument? LoadJsonSourceDocument(
        string path,
        string displayPath,
        string label,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        try
        {
            var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
            if (root is null)
            {
                issues.Add(CreateIssue(
                    "WF-LOAD-004",
                    DiagnosticSeverity.Error,
                    "load",
                    $"{label} root is not an object",
                    $"{label} source contract must be a JSON object.",
                    new SourceLocation(displayPath),
                    projectId,
                    docsRule: "WF-LOAD-004"));
                return null;
            }

            return new LoadedSourceDocument(path, displayPath, root, new Dictionary<string, SourcePosition>());
        }
        catch (JsonException ex)
        {
            issues.Add(CreateIssue(
                "WF-LOAD-004",
                DiagnosticSeverity.Error,
                "load",
                $"{label} JSON could not be parsed",
                ex.Message,
                new SourceLocation(displayPath),
                projectId,
                docsRule: "WF-LOAD-004"));
            return null;
        }
    }

    private static LoadedSourceDocument? LoadYamlSourceDocument(
        string path,
        string displayPath,
        string label,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        try
        {
            var stream = new YamlStream();
            using var reader = File.OpenText(path);
            stream.Load(reader);

            if (stream.Documents.Count != 1)
            {
                issues.Add(CreateIssue(
                    "WF-LOAD-007",
                    DiagnosticSeverity.Error,
                    "load",
                    "Unsupported YAML feature",
                    "YAML source contracts must contain exactly one document.",
                    new SourceLocation(displayPath),
                    projectId,
                    docsRule: "WF-LOAD-007"));
                return null;
            }

            var sourceLocations = new Dictionary<string, SourcePosition>(StringComparer.Ordinal);
            var root = ConvertYamlNode(stream.Documents[0].RootNode, displayPath, string.Empty, sourceLocations, issues, projectId);
            if (root is not JsonObject rootObject)
            {
                if (!HasErrorForFile(issues, displayPath))
                {
                    issues.Add(CreateIssue(
                        "WF-LOAD-004",
                        DiagnosticSeverity.Error,
                        "load",
                        $"{label} root is not an object",
                        $"{label} source contract must be a YAML mapping that normalizes to a JSON object.",
                        new SourceLocation(displayPath),
                        projectId,
                        docsRule: "WF-LOAD-004"));
                }

                return null;
            }

            return new LoadedSourceDocument(path, displayPath, rootObject, sourceLocations);
        }
        catch (YamlException ex)
        {
            issues.Add(CreateIssue(
                "WF-LOAD-004",
                DiagnosticSeverity.Error,
                "load",
                $"{label} YAML could not be parsed",
                ex.Message,
                CreateYamlExceptionLocation(displayPath, ex),
                projectId,
                docsRule: "WF-LOAD-004"));
            return null;
        }
    }

    private static JsonNode? ConvertYamlNode(
        YamlNode node,
        string displayPath,
        string pointer,
        Dictionary<string, SourcePosition> sourceLocations,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        RecordSourceLocation(node, pointer, sourceLocations);
        if (!IsSupportedYamlNode(node, displayPath, pointer, issues, projectId))
        {
            return null;
        }

        return node switch
        {
            YamlMappingNode mapping => ConvertYamlMapping(mapping, displayPath, pointer, sourceLocations, issues, projectId),
            YamlSequenceNode sequence => ConvertYamlSequence(sequence, displayPath, pointer, sourceLocations, issues, projectId),
            YamlScalarNode scalar => ConvertYamlScalar(scalar, displayPath, pointer, issues, projectId),
            _ => AddUnsupportedYamlFeature(displayPath, pointer, node, issues, projectId, $"YAML node type '{node.NodeType}' is not supported in source contracts.")
        };
    }

    private static JsonObject? ConvertYamlMapping(
        YamlMappingNode mapping,
        string displayPath,
        string pointer,
        Dictionary<string, SourcePosition> sourceLocations,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var json = new JsonObject();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in mapping.Children)
        {
            if (pair.Key is not YamlScalarNode keyNode || string.IsNullOrWhiteSpace(keyNode.Value))
            {
                AddUnsupportedYamlFeature(displayPath, pointer, pair.Key, issues, projectId, "YAML mapping keys must be non-empty scalar strings.");
                return null;
            }

            var key = keyNode.Value;
            if (StringComparer.Ordinal.Equals(key, "<<"))
            {
                AddUnsupportedYamlFeature(displayPath, pointer, pair.Key, issues, projectId, "YAML merge keys are not supported in source contracts.");
                return null;
            }

            if (!keys.Add(key))
            {
                issues.Add(CreateIssue(
                    "WF-LOAD-008",
                    DiagnosticSeverity.Error,
                    "load",
                    "Duplicate YAML mapping key",
                    $"YAML mapping key '{key}' appears more than once.",
                    CreateYamlNodeLocation(displayPath, BuildPointer(pointer, key), pair.Key),
                    projectId,
                    docsRule: "WF-LOAD-008"));
                return null;
            }

            var childPointer = BuildPointer(pointer, key);
            json[key] = ConvertYamlNode(pair.Value, displayPath, childPointer, sourceLocations, issues, projectId);
            if (HasErrorForFile(issues, displayPath))
            {
                return null;
            }
        }

        return json;
    }

    private static JsonArray? ConvertYamlSequence(
        YamlSequenceNode sequence,
        string displayPath,
        string pointer,
        Dictionary<string, SourcePosition> sourceLocations,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var json = new JsonArray();
        for (var index = 0; index < sequence.Children.Count; index++)
        {
            json.Add(ConvertYamlNode(sequence.Children[index], displayPath, BuildPointer(pointer, index.ToString(CultureInfo.InvariantCulture)), sourceLocations, issues, projectId));
            if (HasErrorForFile(issues, displayPath))
            {
                return null;
            }
        }

        return json;
    }

    private static JsonNode? ConvertYamlScalar(
        YamlScalarNode scalar,
        string displayPath,
        string pointer,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var value = scalar.Value;
        var tag = scalar.Tag.ToString();
        if (string.IsNullOrEmpty(value) && IsYamlNullTag(tag))
        {
            return null;
        }

        if (IsYamlNullTag(tag))
        {
            return null;
        }

        if (IsYamlBoolTag(tag))
        {
            if (bool.TryParse(value, out var boolean))
            {
                return JsonValue.Create(boolean);
            }

            AddUnsupportedYamlFeature(displayPath, pointer, scalar, issues, projectId, "YAML booleans must use JSON-compatible true or false values.");
            return null;
        }

        if (IsYamlIntTag(tag))
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
            {
                return JsonValue.Create(integer);
            }

            AddUnsupportedYamlFeature(displayPath, pointer, scalar, issues, projectId, "YAML integers must use JSON-compatible integer values.");
            return null;
        }

        if (IsYamlFloatTag(tag))
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
                double.IsFinite(number))
            {
                return JsonValue.Create(number);
            }

            AddUnsupportedYamlFeature(displayPath, pointer, scalar, issues, projectId, "YAML numbers must use finite JSON-compatible numeric values.");
            return null;
        }

        if (IsYamlNonSpecificTag(tag) && TryConvertJsonCompatibleScalar(value, out var jsonValue))
        {
            return jsonValue;
        }

        return JsonValue.Create(value ?? string.Empty);
    }

    private static JsonNode? AddUnsupportedYamlFeature(
        string displayPath,
        string pointer,
        YamlNode node,
        List<DiagnosticIssue> issues,
        LogicalId? projectId,
        string message)
    {
        issues.Add(CreateIssue(
            "WF-LOAD-007",
            DiagnosticSeverity.Error,
            "load",
            "Unsupported YAML feature",
            message,
            CreateYamlNodeLocation(displayPath, pointer, node),
            projectId,
            docsRule: "WF-LOAD-007"));
        return null;
    }

    private static bool IsSupportedYamlNode(
        YamlNode node,
        string displayPath,
        string pointer,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (StringComparer.Ordinal.Equals(node.NodeType.ToString(), "Alias"))
        {
            AddUnsupportedYamlFeature(displayPath, pointer, node, issues, projectId, "YAML aliases are not supported in source contracts.");
            return false;
        }

        if (!node.Anchor.IsEmpty)
        {
            AddUnsupportedYamlFeature(displayPath, pointer, node, issues, projectId, "YAML anchors are not supported in source contracts.");
            return false;
        }

        var tag = node.Tag.ToString();
        if (string.IsNullOrWhiteSpace(tag) ||
            StringComparer.Ordinal.Equals(tag, "!") ||
            StringComparer.Ordinal.Equals(tag, "?") ||
            tag.StartsWith("tag:yaml.org,2002:", StringComparison.Ordinal))
        {
            return true;
        }

        AddUnsupportedYamlFeature(displayPath, pointer, node, issues, projectId, $"YAML custom tag '{tag}' is not supported in source contracts.");
        return false;
    }

    private static void RecordSourceLocation(
        YamlNode node,
        string pointer,
        Dictionary<string, SourcePosition> sourceLocations)
    {
        var line = node.Start.Line;
        var column = node.Start.Column;
        if (line > 0 && column > 0)
        {
            sourceLocations[pointer] = new SourcePosition(ToSourcePosition(line), ToSourcePosition(column));
        }
    }

    private static void ValidateManifestSchema(LoadedManifest manifestLoad, List<DiagnosticIssue> issues, LogicalId? projectId)
    {
        ValidateSourceSchema(
            "Manifest",
            manifestLoad.DisplayPath,
            manifestLoad.Manifest,
            manifestLoad.SourceLocations,
            ManifestSchema.Value,
            issues,
            projectId);
    }

    private static void ValidateRegistrySchema(LoadedSourceDocument source, string expectedKind, List<DiagnosticIssue> issues, LogicalId? projectId)
    {
        var schema = ResolveRegistrySchema(source, expectedKind);
        var label = expectedKind switch
        {
            "dependency" => "Dependency registry",
            "capability" => "Capability registry",
            "asset" => "Asset registry",
            "quest" => "Quest registry",
            "dialogue" => "Dialogue registry",
            _ => "Registry"
        };

        ValidateSourceSchema(
            label,
            source.DisplayPath,
            source.Root,
            source.SourceLocations,
            schema,
            issues,
            projectId);
    }

    private static JsonSchema ResolveRegistrySchema(LoadedSourceDocument source, string expectedKind)
    {
        return expectedKind switch
        {
            "dependency" => DependencyRegistrySchema.Value,
            "capability" => CapabilityRegistrySchema.Value,
            "asset" => AssetRegistrySchema.Value,
            "quest" when StringComparer.Ordinal.Equals(GetString(source.Root, "schemaVersion"), "0.4.0") => QuestRegistrySchema040.Value,
            "quest" when StringComparer.Ordinal.Equals(GetString(source.Root, "schemaVersion"), "0.3.0") => QuestRegistrySchema030.Value,
            "quest" when StringComparer.Ordinal.Equals(GetString(source.Root, "schemaVersion"), "0.2.0") => QuestRegistrySchema020.Value,
            "quest" => QuestRegistrySchema.Value,
            "dialogue" => DialogueRegistrySchema.Value,
            _ => throw new InvalidOperationException($"No built-in registry schema is registered for kind '{expectedKind}'.")
        };
    }

    private static void ValidateSourceSchema(
        string label,
        string displayPath,
        JsonObject root,
        IReadOnlyDictionary<string, SourcePosition> sourceLocations,
        JsonSchema schema,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        using var document = JsonDocument.Parse(root.ToJsonString());
        var results = schema.Evaluate(
            document.RootElement,
            new EvaluationOptions
            {
                OutputFormat = OutputFormat.Hierarchical
            });

        if (results.IsValid)
        {
            return;
        }

        var emitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var failure in EnumerateSchemaFailures(results))
        {
            var pointer = NormalizeJsonPointer(failure.InstanceLocation.ToString());
            var message = FormatSchemaErrors(failure);
            var key = $"{pointer}|{message}";
            if (!emitted.Add(key))
            {
                continue;
            }

            var ruleId = ToSchemaRuleId(pointer);
            issues.Add(CreateIssue(
                ruleId,
                DiagnosticSeverity.Error,
                "schema",
                ToSchemaTitle(ruleId, label),
                message,
                CreateSourceLocation(displayPath, pointer, sourceLocations),
                projectId,
                docsRule: ruleId));
        }
    }

    private static IEnumerable<EvaluationResults> EnumerateSchemaFailures(EvaluationResults results)
    {
        if (results.IsValid)
        {
            yield break;
        }

        if (results.Errors is { Count: > 0 })
        {
            yield return results;
        }

        foreach (var detail in results.Details ?? [])
        {
            foreach (var failure in EnumerateSchemaFailures(detail))
            {
                yield return failure;
            }
        }
    }

    private static string FormatSchemaErrors(EvaluationResults result)
    {
        if (result.Errors is not { Count: > 0 })
        {
            return "Source contract failed JSON Schema Draft 2020-12 validation.";
        }

        return string.Join(
            " ",
            result.Errors
                .OrderBy(error => error.Key, StringComparer.Ordinal)
                .Select(error => $"{error.Key}: {error.Value}"));
    }

    private static string ToSchemaRuleId(string pointer)
    {
        return pointer switch
        {
            "/schemaVersion" => "WF-SCHEMA-002",
            "/id" => "WF-SCHEMA-003",
            _ => "WF-SCHEMA-001"
        };
    }

    private static string ToSchemaTitle(string ruleId, string label)
    {
        return ruleId switch
        {
            "WF-SCHEMA-002" => "Unsupported schema version",
            "WF-SCHEMA-003" => "Invalid logical id",
            _ => $"{label} schema validation failed"
        };
    }

    private static LogicalId? ReadProjectId(JsonObject manifest)
    {
        var rawId = GetString(manifest, "id");
        return LogicalId.TryParse(rawId, out var parsedProjectId)
            ? parsedProjectId
            : null;
    }

    private static void RunSemanticPlaceholder(LogicalId? projectId, LoadedManifest manifestLoad, List<DiagnosticIssue> issues)
    {
        var registries = manifestLoad.Manifest["registries"] as JsonObject;
        var dependencyPath = GetString(registries, "dependencies");
        var capabilityPath = GetString(registries, "capabilities");
        var assetPath = GetString(registries, "assets");
        var questPath = GetString(registries, "quests");
        var dialoguePath = GetString(registries, "dialogue");
        if (dependencyPath is null || capabilityPath is null)
        {
            return;
        }

        var registryIssueStart = issues.Count;
        var dependencyDocuments = LoadRegistryDocuments(
            manifestLoad.ProjectRoot,
            dependencyPath,
            "dependency",
            "dependencies",
            issues,
            projectId);
        var capabilityDocuments = LoadRegistryDocuments(
            manifestLoad.ProjectRoot,
            capabilityPath,
            "capability",
            "capabilities",
            issues,
            projectId);
        IReadOnlyList<RegistryDocument> assetDocuments = [];
        if (assetPath is not null)
        {
            assetDocuments = LoadRegistryDocuments(
                manifestLoad.ProjectRoot,
                assetPath,
                "asset",
                "assets",
                issues,
                projectId);
        }
        IReadOnlyList<RegistryDocument> questDocuments = [];
        if (questPath is not null)
        {
            questDocuments = LoadRegistryDocuments(
                manifestLoad.ProjectRoot,
                questPath,
                "quest",
                "quests",
                issues,
                projectId);
        }
        IReadOnlyList<RegistryDocument> dialogueDocuments = [];
        if (dialoguePath is not null)
        {
            dialogueDocuments = LoadRegistryDocuments(
                manifestLoad.ProjectRoot,
                dialoguePath,
                "dialogue",
                "dialogue",
                issues,
                projectId);
        }

        if (HasErrorSince(issues, registryIssueStart))
        {
            return;
        }

        if (dependencyDocuments.Count == 0 || capabilityDocuments.Count == 0)
        {
            return;
        }

        var assetRecords = ReadAssetRecords(assetDocuments);
        RunAssetSemanticValidation(manifestLoad.ProjectRoot, assetRecords, issues, projectId);
        RunQuestSemanticValidation(questDocuments, issues, projectId);
        RunDialogueSemanticValidation(dialogueDocuments, questDocuments, assetRecords, issues, projectId);

        var capabilityIds = capabilityDocuments
            .SelectMany(document => ReadCapabilityIds(document.Root))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var document in dependencyDocuments)
        {
            foreach (var requiredCapability in ReadRequiredCapabilityIds(document.Root))
            {
                if (capabilityIds.Contains(requiredCapability.Id))
                {
                    continue;
                }

                var pointer = $"/requires/capabilities/{requiredCapability.Index}/id";
                issues.Add(CreateIssue(
                    "WF-SEM-014",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Unknown capability reference",
                    $"Dependency registry references capability '{requiredCapability.Id}' which is not defined.",
                    CreateSourceLocation(document.DisplayPath, pointer, document.SourceLocations),
                    projectId,
                    capabilityDocuments.Select(capabilityDocument => ToCapabilityRelatedLocation(capabilityDocument)).ToArray(),
                    "Declare the capability in the capability registry or remove the dependency.",
                    "WF-SEM-014",
                    $"wf:sem:014:{requiredCapability.Id}"));
            }
        }
    }

    private static void RunAssetSemanticValidation(
        string projectRoot,
        IReadOnlyList<AssetRecord> assets,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        foreach (var assetRecord in assets)
        {
            ValidateAssetSourcePath(projectRoot, assetRecord.Document, assetRecord.Asset, issues, projectId);
            ValidateAssetTargetPath(assetRecord.Document, assetRecord.Asset, issues, projectId);
            ValidateAssetTargetExtension(assetRecord.Document, assetRecord.Asset, issues, projectId);
            ValidateAssetSourceSignature(projectRoot, assetRecord.Document, assetRecord.Asset, issues, projectId);
            ValidateAssetTargetRoot(assetRecord.Document, assetRecord.Asset, issues, projectId);
            ValidateVoiceTargetShape(assetRecord.Document, assetRecord.Asset, issues, projectId);
        }

        ValidateVoiceAssetPairs(assets, issues, projectId);
    }

    private static void RunDialogueSemanticValidation(
        IReadOnlyList<RegistryDocument> dialogueDocuments,
        IReadOnlyList<RegistryDocument> questDocuments,
        IReadOnlyList<AssetRecord> assetRecords,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (dialogueDocuments.Count == 0)
        {
            return;
        }

        var voiceExtensionsByStem = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var assetRecord in assetRecords)
        {
            if (!TryReadVoiceTarget(assetRecord, out var voiceTarget))
            {
                continue;
            }

            if (!voiceExtensionsByStem.TryGetValue(voiceTarget.Stem, out var extensions))
            {
                extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                voiceExtensionsByStem[voiceTarget.Stem] = extensions;
            }

            extensions.Add(voiceTarget.Extension);
        }

        foreach (var document in dialogueDocuments)
        {
            foreach (var voiceWorkItem in ReadDialogueVoiceWorkItems(document))
            {
                var expectedStem = BuildVoiceTargetStem(
                    voiceWorkItem.Plugin,
                    voiceWorkItem.VoiceType,
                    voiceWorkItem.FileStem);
                voiceExtensionsByStem.TryGetValue(expectedStem, out var declaredExtensions);
                declaredExtensions ??= [];

                var missingExtensions = new[] { ".wav", ".ogg", ".lip" }
                    .Where(extension => !declaredExtensions.Contains(extension))
                    .ToArray();
                if (missingExtensions.Length == 0)
                {
                    continue;
                }

                issues.Add(CreateIssue(
                    "WF-SEM-015",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Dialogue voice worklist assets are missing",
                    $"Dialogue line '{voiceWorkItem.LineId}' expects voice target '{expectedStem}' but is missing {FormatAllowedExtensions(missingExtensions)}.",
                    CreateSourceLocation(document.DisplayPath, $"/lines/{voiceWorkItem.Index}/voice", document.SourceLocations),
                    projectId,
                    suggestedFix: "Declare matching voice .wav, voice .ogg, and .lip assets for the dialogue voice work item.",
                    docsRule: "WF-SEM-015",
                    fingerprint: $"wf:sem:015:{voiceWorkItem.LineId}"));
            }
        }

        if (questDocuments.Count == 0)
        {
            return;
        }

        var questIds = questDocuments
            .SelectMany(ReadQuestIds)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var document in dialogueDocuments)
        {
            foreach (var questReference in ReadDialogueQuestReferences(document))
            {
                if (questIds.Contains(questReference.QuestId))
                {
                    continue;
                }

                issues.Add(CreateIssue(
                    "WF-SEM-016",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Dialogue references unknown quest",
                    $"Dialogue line '{questReference.LineId}' references quest '{questReference.QuestId}' which is not declared.",
                    CreateSourceLocation(document.DisplayPath, $"/lines/{questReference.Index}/questId", document.SourceLocations),
                    projectId,
                    questDocuments.Select(ToQuestRelatedLocation).ToArray(),
                    "Declare the quest in the quest registry or update the dialogue questId.",
                    "WF-SEM-016",
                    $"wf:sem:016:{questReference.LineId}"));
            }
        }
    }

    private static void RunQuestSemanticValidation(
        IReadOnlyList<RegistryDocument> questDocuments,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        foreach (var document in questDocuments)
        {
            foreach (var stageReference in ReadQuestObjectiveStageReferences(document))
            {
                if (stageReference.StageIds.Contains(stageReference.StageId))
                {
                    continue;
                }

                issues.Add(CreateIssue(
                    "WF-SEM-017",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Quest objective references unknown stage",
                    $"Quest objective '{stageReference.ObjectiveId}' references stage '{stageReference.StageId}' through '{stageReference.PropertyName}' but that stage is not declared in quest '{stageReference.QuestId}'.",
                    CreateSourceLocation(document.DisplayPath, $"/quests/{stageReference.QuestIndex}/objectives/{stageReference.ObjectiveIndex}/{stageReference.PropertyName}", document.SourceLocations),
                    projectId,
                    [CreateSourceLocation(document.DisplayPath, $"/quests/{stageReference.QuestIndex}/stages", document.SourceLocations)],
                    "Declare the stage in the same quest or update the objective stage reference.",
                    "WF-SEM-017",
                    $"wf:sem:017:{stageReference.ObjectiveId}:{stageReference.PropertyName}"));
            }

            foreach (var stageReference in ReadQuestTransitionStageReferences(document))
            {
                if (stageReference.StageIds.Contains(stageReference.StageId))
                {
                    continue;
                }

                issues.Add(CreateIssue(
                    "WF-SEM-018",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Quest transition references unknown stage",
                    $"Quest transition '{stageReference.TransitionId}' references stage '{stageReference.StageId}' through '{stageReference.PropertyName}' but that stage is not declared in quest '{stageReference.QuestId}'.",
                    CreateSourceLocation(document.DisplayPath, $"/quests/{stageReference.QuestIndex}/transitions/{stageReference.TransitionIndex}/{stageReference.PropertyName}", document.SourceLocations),
                    projectId,
                    [CreateSourceLocation(document.DisplayPath, $"/quests/{stageReference.QuestIndex}/stages", document.SourceLocations)],
                    "Declare the stage in the same quest or update the transition stage reference.",
                    "WF-SEM-018",
                    $"wf:sem:018:{stageReference.TransitionId}:{stageReference.PropertyName}"));
            }

            foreach (var stageReference in ReadQuestConditionStageReferences(document))
            {
                if (stageReference.StageIds.Contains(stageReference.StageId))
                {
                    continue;
                }

                issues.Add(CreateIssue(
                    "WF-SEM-019",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Quest condition references unknown stage",
                    $"Quest condition '{stageReference.ConditionId}' references stage '{stageReference.StageId}' but that stage is not declared in quest '{stageReference.QuestId}'.",
                    CreateSourceLocation(document.DisplayPath, $"/quests/{stageReference.QuestIndex}/conditions/{stageReference.ConditionIndex}/stageId", document.SourceLocations),
                    projectId,
                    [CreateSourceLocation(document.DisplayPath, $"/quests/{stageReference.QuestIndex}/stages", document.SourceLocations)],
                    "Declare the stage in the same quest or update the condition stage reference.",
                    "WF-SEM-019",
                    $"wf:sem:019:{stageReference.ConditionId}:stageId"));
            }
        }
    }

    private static void ValidateAssetSourcePath(
        string projectRoot,
        RegistryDocument document,
        AssetEntry asset,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var pointer = $"/assets/{asset.Index}/source";
        var absoluteSource = Path.GetFullPath(Path.Combine(projectRoot, asset.Source));
        if (!IsInside(projectRoot, absoluteSource))
        {
            issues.Add(CreateIssue(
                "WF-ASSET-001",
                DiagnosticSeverity.Error,
                "asset",
                "Asset source path escapes project root",
                $"Asset '{asset.Id}' source path must stay inside the project root.",
                CreateSourceLocation(document.DisplayPath, pointer, document.SourceLocations),
                projectId,
                suggestedFix: "Use a project-relative source path under the WastelandForge project root.",
                docsRule: "WF-ASSET-001",
                fingerprint: $"wf:asset:001:{asset.Id}"));
            return;
        }

        if (asset.Required && !File.Exists(absoluteSource))
        {
            issues.Add(CreateIssue(
                "WF-ASSET-002",
                DiagnosticSeverity.Error,
                "asset",
                "Required asset source file not found",
                $"Asset '{asset.Id}' source file '{asset.Source}' was not found.",
                CreateSourceLocation(document.DisplayPath, pointer, document.SourceLocations),
                projectId,
                suggestedFix: "Add the source file or mark the asset as required: false if it is optional.",
                docsRule: "WF-ASSET-002",
                fingerprint: $"wf:asset:002:{asset.Id}"));
        }
    }

    private static void ValidateAssetTargetPath(
        RegistryDocument document,
        AssetEntry asset,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (IsSafeRelativeAssetPath(asset.Target))
        {
            return;
        }

        issues.Add(CreateIssue(
            "WF-ASSET-003",
            DiagnosticSeverity.Error,
            "asset",
            "Asset target path is not game-relative",
            $"Asset '{asset.Id}' target path must be a relative game-data path without traversal segments.",
            CreateSourceLocation(document.DisplayPath, $"/assets/{asset.Index}/target", document.SourceLocations),
            projectId,
            suggestedFix: "Use a target path relative to the game Data directory, such as meshes/example/item.nif.",
            docsRule: "WF-ASSET-003",
            fingerprint: $"wf:asset:003:{asset.Id}"));
    }

    private static void ValidateAssetTargetExtension(
        RegistryDocument document,
        AssetEntry asset,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (!AssetTargetExtensions.TryGetValue(asset.AssetType, out var allowedExtensions))
        {
            return;
        }

        var extension = Path.GetExtension(asset.Target);
        if (allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        issues.Add(CreateIssue(
            "WF-ASSET-004",
            DiagnosticSeverity.Error,
            "asset",
            "Asset target extension does not match asset type",
            $"Asset '{asset.Id}' type '{asset.AssetType}' target must use {FormatAllowedExtensions(allowedExtensions)}.",
            CreateSourceLocation(document.DisplayPath, $"/assets/{asset.Index}/target", document.SourceLocations),
            projectId,
            suggestedFix: "Use a target file extension that matches the declared asset type.",
            docsRule: "WF-ASSET-004",
            fingerprint: $"wf:asset:004:{asset.Id}"));
    }

    private static void ValidateAssetSourceSignature(
        string projectRoot,
        RegistryDocument document,
        AssetEntry asset,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var absoluteSource = Path.GetFullPath(Path.Combine(projectRoot, asset.Source));
        if (!IsInside(projectRoot, absoluteSource) || !File.Exists(absoluteSource))
        {
            return;
        }

        if (!TryGetAssetSignature(asset.Target, out var expectedSignature, out var isMatch))
        {
            return;
        }

        var bytes = File.ReadAllBytes(absoluteSource);
        if (isMatch(bytes))
        {
            return;
        }

        issues.Add(CreateIssue(
            "WF-ASSET-005",
            DiagnosticSeverity.Error,
            "asset",
            "Asset source signature does not match target type",
            $"Asset '{asset.Id}' source file must look like {expectedSignature}.",
            CreateSourceLocation(document.DisplayPath, $"/assets/{asset.Index}/source", document.SourceLocations),
            projectId,
            suggestedFix: "Use a source file whose header matches the declared asset target type.",
            docsRule: "WF-ASSET-005",
            fingerprint: $"wf:asset:005:{asset.Id}"));
    }

    private static void ValidateAssetTargetRoot(
        RegistryDocument document,
        AssetEntry asset,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (!IsSafeRelativeAssetPath(asset.Target) ||
            !AssetTargetPrefixes.TryGetValue(asset.AssetType, out var allowedPrefixes))
        {
            return;
        }

        var normalizedTarget = NormalizeAssetPath(asset.Target);
        if (allowedPrefixes.Any(prefix => normalizedTarget.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        issues.Add(CreateIssue(
            "WF-ASSET-006",
            DiagnosticSeverity.Error,
            "asset",
            "Asset target root does not match asset type",
            $"Asset '{asset.Id}' type '{asset.AssetType}' target must start with {FormatAllowedPrefixes(allowedPrefixes)}.",
            CreateSourceLocation(document.DisplayPath, $"/assets/{asset.Index}/target", document.SourceLocations),
            projectId,
            suggestedFix: "Use the game-data folder convention that matches the declared asset type.",
            docsRule: "WF-ASSET-006",
            fingerprint: $"wf:asset:006:{asset.Id}"));
    }

    private static void ValidateVoiceTargetShape(
        RegistryDocument document,
        AssetEntry asset,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (!IsVoiceOrLipAsset(asset) ||
            !IsSafeRelativeAssetPath(asset.Target) ||
            !NormalizeAssetPath(asset.Target).StartsWith("sound/voice/", StringComparison.OrdinalIgnoreCase) ||
            HasVoiceTargetShape(asset.Target))
        {
            return;
        }

        issues.Add(CreateIssue(
            "WF-ASSET-007",
            DiagnosticSeverity.Error,
            "asset",
            "Voice target path is missing plugin or voice type",
            $"Asset '{asset.Id}' target must use sound/voice/<PluginName>/<VoiceType>/<FileName>.",
            CreateSourceLocation(document.DisplayPath, $"/assets/{asset.Index}/target", document.SourceLocations),
            projectId,
            suggestedFix: "Place voice and lip targets under sound/voice/<PluginName>/<VoiceType>/ with a dialogue-line file name.",
            docsRule: "WF-ASSET-007",
            fingerprint: $"wf:asset:007:{asset.Id}"));
    }

    private static void ValidateVoiceAssetPairs(
        IReadOnlyList<AssetRecord> assets,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var voiceTargets = assets
            .Select(assetRecord => TryReadVoiceTarget(assetRecord, out var voiceTarget) ? voiceTarget : null)
            .OfType<VoiceTarget>()
            .ToArray();

        var voiceExtensionsByStem = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var lipStems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var voiceTarget in voiceTargets)
        {
            if (voiceTarget.Extension.Equals(".lip", StringComparison.OrdinalIgnoreCase))
            {
                lipStems.Add(voiceTarget.Stem);
                continue;
            }

            if (!voiceExtensionsByStem.TryGetValue(voiceTarget.Stem, out var extensions))
            {
                extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                voiceExtensionsByStem[voiceTarget.Stem] = extensions;
            }

            extensions.Add(voiceTarget.Extension);
        }

        var reportedVoicePairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var reportedLipPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var voiceTarget in voiceTargets)
        {
            if (!voiceTarget.Record.Asset.Required ||
                !voiceTarget.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase) &&
                !voiceTarget.Extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var voiceExtensions = voiceExtensionsByStem[voiceTarget.Stem];
            if ((!voiceExtensions.Contains(".wav") || !voiceExtensions.Contains(".ogg")) &&
                reportedVoicePairs.Add(voiceTarget.Stem))
            {
                issues.Add(CreateIssue(
                    "WF-ASSET-008",
                    DiagnosticSeverity.Error,
                    "asset",
                    "Voice WAV/OGG pair is incomplete",
                    $"Voice asset target '{voiceTarget.Stem}' must declare both .wav and .ogg assets.",
                    CreateSourceLocation(voiceTarget.Record.Document.DisplayPath, $"/assets/{voiceTarget.Record.Asset.Index}/target", voiceTarget.Record.Document.SourceLocations),
                    projectId,
                    suggestedFix: "Declare the missing WAV or OGG voice asset with the same sound/voice plugin, voice type, and file stem.",
                    docsRule: "WF-ASSET-008",
                    fingerprint: $"wf:asset:008:{voiceTarget.Record.Asset.Id}"));
            }

            if (!lipStems.Contains(voiceTarget.Stem) &&
                reportedLipPairs.Add(voiceTarget.Stem))
            {
                issues.Add(CreateIssue(
                    "WF-ASSET-009",
                    DiagnosticSeverity.Error,
                    "asset",
                    "Voice LIP pair is missing",
                    $"Voice asset target '{voiceTarget.Stem}' must declare a matching .lip asset.",
                    CreateSourceLocation(voiceTarget.Record.Document.DisplayPath, $"/assets/{voiceTarget.Record.Asset.Index}/target", voiceTarget.Record.Document.SourceLocations),
                    projectId,
                    suggestedFix: "Declare the matching LIP asset under the same sound/voice plugin and voice type path.",
                    docsRule: "WF-ASSET-009",
                    fingerprint: $"wf:asset:009:{voiceTarget.Record.Asset.Id}"));
            }
        }
    }

    private static void RunCapabilityPlaceholder()
    {
        // Environment/provider probing starts after the deterministic source pipeline exists.
    }

    private static IReadOnlyList<RegistryDocument> LoadRegistryDocuments(
        string projectRoot,
        string declaredPath,
        string expectedKind,
        string label,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var absolutePath = Path.GetFullPath(Path.Combine(projectRoot, declaredPath));
        if (!IsInside(projectRoot, absolutePath))
        {
            issues.Add(CreateIssue(
                "WF-LOAD-005",
                DiagnosticSeverity.Error,
                "load",
                "Registry path escapes project root",
                $"The {label} registry path must stay inside the project root.",
                new SourceLocation(declaredPath),
                projectId,
                docsRule: "WF-LOAD-005"));
            return [];
        }

        var files = ResolveRegistryFiles(absolutePath);
        if (files.Count == 0)
        {
            issues.Add(CreateIssue(
                "WF-LOAD-006",
                DiagnosticSeverity.Error,
                "load",
                "Registry root not found",
                $"The {label} registry root '{declaredPath}' did not resolve to any JSON or YAML files.",
                new SourceLocation(declaredPath),
                projectId,
                docsRule: "WF-LOAD-006"));
            return [];
        }

        var documents = new List<RegistryDocument>();
        foreach (var file in files)
        {
            var source = LoadSourceDocument(projectRoot, file, "Registry document", issues, projectId);
            if (source is null)
            {
                continue;
            }

            var schemaIssueStart = issues.Count;
            ValidateRegistrySchema(source, expectedKind, issues, projectId);
            if (HasErrorSince(issues, schemaIssueStart))
            {
                continue;
            }

            documents.Add(new RegistryDocument(source.Path, source.DisplayPath, source.Root, source.SourceLocations));
        }

        return documents;
    }

    private static IReadOnlyList<string> ResolveRegistryFiles(string absolutePath)
    {
        if (File.Exists(absolutePath))
        {
            return IsSourceContractExtension(Path.GetExtension(absolutePath))
                ? [absolutePath]
                : [];
        }

        if (!Directory.Exists(absolutePath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(absolutePath, "*", SearchOption.TopDirectoryOnly)
            .Where(path => IsSourceContractExtension(Path.GetExtension(path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsSourceContractExtension(string extension)
    {
        return extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ReadCapabilityIds(JsonObject root)
    {
        var topLevelId = GetString(root, "id");
        if (topLevelId is not null)
        {
            yield return topLevelId;
        }

        if (root["capabilities"] is not JsonArray capabilities)
        {
            yield break;
        }

        foreach (var item in capabilities.OfType<JsonObject>())
        {
            var id = GetString(item, "id");
            if (id is not null)
            {
                yield return id;
            }
        }
    }

    private static IEnumerable<RequiredCapability> ReadRequiredCapabilityIds(JsonObject root)
    {
        if (root["requires"] is not JsonObject requires ||
            requires["capabilities"] is not JsonArray capabilities)
        {
            yield break;
        }

        for (var index = 0; index < capabilities.Count; index++)
        {
            if (capabilities[index] is not JsonObject capability)
            {
                continue;
            }

            var id = GetString(capability, "id");
            if (id is not null)
            {
                yield return new RequiredCapability(index, id);
            }
        }
    }

    private static IReadOnlyList<AssetRecord> ReadAssetRecords(IReadOnlyList<RegistryDocument> assetDocuments)
    {
        return assetDocuments
            .SelectMany(document => ReadAssetEntries(document).Select(asset => new AssetRecord(document, asset)))
            .ToArray();
    }

    private static IEnumerable<string> ReadQuestIds(RegistryDocument document)
    {
        if (document.Root["quests"] is not JsonArray quests)
        {
            yield break;
        }

        foreach (var item in quests.OfType<JsonObject>())
        {
            var id = GetString(item, "id");
            if (id is not null)
            {
                yield return id;
            }
        }
    }

    private static IEnumerable<QuestObjectiveStageReference> ReadQuestObjectiveStageReferences(RegistryDocument document)
    {
        if (document.Root["quests"] is not JsonArray quests)
        {
            yield break;
        }

        for (var questIndex = 0; questIndex < quests.Count; questIndex++)
        {
            if (quests[questIndex] is not JsonObject quest)
            {
                continue;
            }

            var questId = GetString(quest, "id");
            if (questId is null)
            {
                continue;
            }

            var stageIds = ReadQuestStageIds(quest);

            if (quest["objectives"] is not JsonArray objectives)
            {
                continue;
            }

            for (var objectiveIndex = 0; objectiveIndex < objectives.Count; objectiveIndex++)
            {
                if (objectives[objectiveIndex] is not JsonObject objective)
                {
                    continue;
                }

                var objectiveId = GetString(objective, "id");
                if (objectiveId is null)
                {
                    continue;
                }

                foreach (var propertyName in new[] { "startStageId", "completionStageId" })
                {
                    var stageId = GetString(objective, propertyName);
                    if (stageId is null)
                    {
                        continue;
                    }

                    yield return new QuestObjectiveStageReference(
                        questIndex,
                        objectiveIndex,
                        questId,
                        objectiveId,
                        propertyName,
                        stageId,
                        stageIds);
                }
            }
        }
    }

    private static IEnumerable<QuestTransitionStageReference> ReadQuestTransitionStageReferences(RegistryDocument document)
    {
        if (document.Root["quests"] is not JsonArray quests)
        {
            yield break;
        }

        for (var questIndex = 0; questIndex < quests.Count; questIndex++)
        {
            if (quests[questIndex] is not JsonObject quest)
            {
                continue;
            }

            var questId = GetString(quest, "id");
            if (questId is null)
            {
                continue;
            }

            var stageIds = ReadQuestStageIds(quest);

            if (quest["transitions"] is not JsonArray transitions)
            {
                continue;
            }

            for (var transitionIndex = 0; transitionIndex < transitions.Count; transitionIndex++)
            {
                if (transitions[transitionIndex] is not JsonObject transition)
                {
                    continue;
                }

                var transitionId = GetString(transition, "id");
                if (transitionId is null)
                {
                    continue;
                }

                foreach (var propertyName in new[] { "fromStageId", "toStageId" })
                {
                    var stageId = GetString(transition, propertyName);
                    if (stageId is null)
                    {
                        continue;
                    }

                    yield return new QuestTransitionStageReference(
                        questIndex,
                        transitionIndex,
                        questId,
                        transitionId,
                        propertyName,
                        stageId,
                        stageIds);
                }
            }
        }
    }

    private static IEnumerable<QuestConditionStageReference> ReadQuestConditionStageReferences(RegistryDocument document)
    {
        if (document.Root["quests"] is not JsonArray quests)
        {
            yield break;
        }

        for (var questIndex = 0; questIndex < quests.Count; questIndex++)
        {
            if (quests[questIndex] is not JsonObject quest)
            {
                continue;
            }

            var questId = GetString(quest, "id");
            if (questId is null)
            {
                continue;
            }

            var stageIds = ReadQuestStageIds(quest);

            if (quest["conditions"] is not JsonArray conditions)
            {
                continue;
            }

            for (var conditionIndex = 0; conditionIndex < conditions.Count; conditionIndex++)
            {
                if (conditions[conditionIndex] is not JsonObject condition)
                {
                    continue;
                }

                var conditionId = GetString(condition, "id");
                var stageId = GetString(condition, "stageId");
                if (conditionId is null || stageId is null)
                {
                    continue;
                }

                yield return new QuestConditionStageReference(
                    questIndex,
                    conditionIndex,
                    questId,
                    conditionId,
                    stageId,
                    stageIds);
            }
        }
    }

    private static HashSet<string> ReadQuestStageIds(JsonObject quest)
    {
        return quest["stages"] is JsonArray stages
            ? stages
                .OfType<JsonObject>()
                .Select(stage => GetString(stage, "id"))
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
    }

    private static IEnumerable<AssetEntry> ReadAssetEntries(RegistryDocument document)
    {
        if (document.Root["assets"] is not JsonArray assets)
        {
            yield break;
        }

        for (var index = 0; index < assets.Count; index++)
        {
            if (assets[index] is not JsonObject asset)
            {
                continue;
            }

            var id = GetString(asset, "id");
            var assetType = GetString(asset, "assetType");
            var source = GetString(asset, "source");
            var target = GetString(asset, "target");
            if (id is null || assetType is null || source is null || target is null)
            {
                continue;
            }

            yield return new AssetEntry(
                index,
                id,
                assetType,
                source,
                target,
                GetBoolean(asset, "required") is not false);
        }
    }

    private static IEnumerable<DialogueQuestReference> ReadDialogueQuestReferences(RegistryDocument document)
    {
        if (document.Root["lines"] is not JsonArray lines)
        {
            yield break;
        }

        for (var index = 0; index < lines.Count; index++)
        {
            if (lines[index] is not JsonObject line)
            {
                continue;
            }

            var lineId = GetString(line, "id");
            var questId = GetString(line, "questId");
            if (lineId is null || questId is null)
            {
                continue;
            }

            yield return new DialogueQuestReference(index, lineId, questId);
        }
    }

    private static IEnumerable<DialogueVoiceWorkItem> ReadDialogueVoiceWorkItems(RegistryDocument document)
    {
        if (document.Root["lines"] is not JsonArray lines)
        {
            yield break;
        }

        for (var index = 0; index < lines.Count; index++)
        {
            if (lines[index] is not JsonObject line ||
                line["voice"] is not JsonObject voice)
            {
                continue;
            }

            var lineId = GetString(line, "id");
            var plugin = GetString(voice, "plugin");
            var voiceType = GetString(voice, "voiceType");
            var fileStem = GetString(voice, "fileStem");
            if (lineId is null ||
                plugin is null ||
                voiceType is null ||
                fileStem is null)
            {
                continue;
            }

            yield return new DialogueVoiceWorkItem(index, lineId, plugin, voiceType, fileStem);
        }
    }

    private static SourceLocation ToCapabilityRelatedLocation(RegistryDocument document)
    {
        var pointer = GetString(document.Root, "id") is not null
            ? "/id"
            : "/capabilities";

        return CreateSourceLocation(document.DisplayPath, pointer, document.SourceLocations);
    }

    private static SourceLocation ToQuestRelatedLocation(RegistryDocument document)
    {
        return CreateSourceLocation(document.DisplayPath, "/quests", document.SourceLocations);
    }

    private static string? GetString(JsonObject? root, string propertyName)
    {
        if (root?[propertyName]?.GetValueKind() != JsonValueKind.String)
        {
            return null;
        }

        var value = root[propertyName]?.GetValue<string>();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool? GetBoolean(JsonObject? root, string propertyName)
    {
        return root?[propertyName]?.GetValueKind() switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static bool HasErrorSince(IReadOnlyList<DiagnosticIssue> issues, int issueStart)
    {
        for (var index = issueStart; index < issues.Count; index++)
        {
            if (issues[index].Severity == DiagnosticSeverity.Error)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasErrorForFile(IEnumerable<DiagnosticIssue> issues, string displayPath)
    {
        return issues.Any(issue =>
            issue.Severity == DiagnosticSeverity.Error &&
            StringComparer.Ordinal.Equals(issue.PrimaryLocation.File, displayPath));
    }

    private static DiagnosticIssue CreateIssue(
        string ruleId,
        DiagnosticSeverity severity,
        string category,
        string title,
        string message,
        SourceLocation primaryLocation,
        LogicalId? projectId = null,
        IReadOnlyList<SourceLocation>? relatedLocations = null,
        string? suggestedFix = null,
        string? docsRule = null,
        string? fingerprint = null)
    {
        return new DiagnosticIssue(
            RuleId.Parse(ruleId),
            severity,
            category,
            title,
            message,
            primaryLocation,
            projectId,
            relatedLocations,
            suggestedFix,
            docsRule is null ? null : new Uri($"https://docs.wastelandforge.dev/rules/{docsRule}"),
            fingerprint);
    }

    private static SourceLocation CreateSourceLocation(
        string displayPath,
        string pointer,
        IReadOnlyDictionary<string, SourcePosition> sourceLocations)
    {
        var normalizedPointer = NormalizeJsonPointer(pointer);
        var jsonPointer = JsonPointer.Parse(normalizedPointer);
        if (sourceLocations.TryGetValue(normalizedPointer, out var position) ||
            sourceLocations.TryGetValue(string.Empty, out position))
        {
            return new SourceLocation(displayPath, jsonPointer, position.Line, position.Column);
        }

        return new SourceLocation(displayPath, jsonPointer);
    }

    private static SourceLocation CreateYamlNodeLocation(string displayPath, string pointer, YamlNode node)
    {
        var normalizedPointer = NormalizeJsonPointer(pointer);
        var jsonPointer = JsonPointer.Parse(normalizedPointer);
        return node.Start.Line > 0 && node.Start.Column > 0
            ? new SourceLocation(displayPath, jsonPointer, ToSourcePosition(node.Start.Line), ToSourcePosition(node.Start.Column))
            : new SourceLocation(displayPath, jsonPointer);
    }

    private static SourceLocation CreateYamlExceptionLocation(string displayPath, YamlException exception)
    {
        return exception.Start.Line > 0 && exception.Start.Column > 0
            ? new SourceLocation(displayPath, line: ToSourcePosition(exception.Start.Line), column: ToSourcePosition(exception.Start.Column))
            : new SourceLocation(displayPath);
    }

    private static int ToSourcePosition(long value)
    {
        return value > int.MaxValue
            ? int.MaxValue
            : (int)value;
    }

    private static string NormalizeJsonPointer(string pointer)
    {
        if (string.IsNullOrWhiteSpace(pointer) || StringComparer.Ordinal.Equals(pointer, "#"))
        {
            return string.Empty;
        }

        if (pointer.StartsWith('#'))
        {
            pointer = pointer[1..];
        }

        return pointer.StartsWith('/')
            ? pointer
            : "/" + pointer;
    }

    private static string BuildPointer(string parent, string segment)
    {
        return string.IsNullOrEmpty(parent)
            ? "/" + JsonPointer.EscapeSegment(segment)
            : parent + "/" + JsonPointer.EscapeSegment(segment);
    }

    private static bool IsYamlNullTag(string tag) => tag.EndsWith(":null", StringComparison.Ordinal);

    private static bool IsYamlBoolTag(string tag) => tag.EndsWith(":bool", StringComparison.Ordinal);

    private static bool IsYamlIntTag(string tag) => tag.EndsWith(":int", StringComparison.Ordinal);

    private static bool IsYamlFloatTag(string tag) => tag.EndsWith(":float", StringComparison.Ordinal);

    private static bool IsSafeRelativeAssetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
        {
            return false;
        }

        var segments = path.Replace('\\', '/').Split('/');
        return segments.All(segment =>
            !string.IsNullOrWhiteSpace(segment) &&
            !StringComparer.Ordinal.Equals(segment, ".") &&
            !StringComparer.Ordinal.Equals(segment, ".."));
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    private static bool IsVoiceOrLipAsset(AssetEntry asset)
    {
        return asset.AssetType.Equals("voice", StringComparison.Ordinal) ||
            asset.AssetType.Equals("lip", StringComparison.Ordinal);
    }

    private static bool HasVoiceTargetShape(string target)
    {
        var normalizedTarget = NormalizeAssetPath(target);
        var segments = normalizedTarget.Split('/');
        return segments.Length == 5 &&
            segments[0].Equals("sound", StringComparison.OrdinalIgnoreCase) &&
            segments[1].Equals("voice", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(segments[2]) &&
            !string.IsNullOrWhiteSpace(segments[3]) &&
            !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(segments[4]));
    }

    private static bool TryReadVoiceTarget(AssetRecord record, out VoiceTarget voiceTarget)
    {
        voiceTarget = default!;
        if (!IsVoiceOrLipAsset(record.Asset) ||
            !IsSafeRelativeAssetPath(record.Asset.Target) ||
            !HasVoiceTargetShape(record.Asset.Target))
        {
            return false;
        }

        var extension = Path.GetExtension(record.Asset.Target).ToLowerInvariant();
        if (record.Asset.AssetType.Equals("voice", StringComparison.Ordinal) &&
            !extension.Equals(".wav", StringComparison.Ordinal) &&
            !extension.Equals(".ogg", StringComparison.Ordinal))
        {
            return false;
        }

        if (record.Asset.AssetType.Equals("lip", StringComparison.Ordinal) &&
            !extension.Equals(".lip", StringComparison.Ordinal))
        {
            return false;
        }

        voiceTarget = new VoiceTarget(record, NormalizeAssetPath(record.Asset.Target)[..^extension.Length], extension);
        return true;
    }

    private static string BuildVoiceTargetStem(string plugin, string voiceType, string fileStem)
    {
        return NormalizeAssetPath($"sound/voice/{plugin}/{voiceType}/{fileStem}");
    }

    private static string FormatAllowedExtensions(IReadOnlyList<string> extensions)
    {
        return extensions.Count switch
        {
            0 => "a valid target extension",
            1 => $"the {extensions[0]} extension",
            2 => $"the {extensions[0]} or {extensions[1]} extension",
            _ => string.Join(", ", extensions.Take(extensions.Count - 1)) + $", or {extensions[^1]} extensions"
        };
    }

    private static string FormatAllowedPrefixes(IReadOnlyList<string> prefixes)
    {
        return prefixes.Count switch
        {
            0 => "a valid target root",
            1 => $"'{prefixes[0]}'",
            2 => $"'{prefixes[0]}' or '{prefixes[1]}'",
            _ => string.Join(", ", prefixes.Take(prefixes.Count - 1).Select(prefix => $"'{prefix}'")) + $", or '{prefixes[^1]}'"
        };
    }

    private static bool TryGetAssetSignature(
        string target,
        out string expectedSignature,
        out Func<byte[], bool> isMatch)
    {
        switch (Path.GetExtension(target).ToLowerInvariant())
        {
            case ".dds":
                expectedSignature = "a DDS file with a DDS magic header";
                isMatch = bytes => StartsWithAscii(bytes, "DDS ");
                return true;
            case ".wav":
                expectedSignature = "a WAV file with RIFF/WAVE headers";
                isMatch = bytes => StartsWithAscii(bytes, "RIFF") && HasAsciiAt(bytes, "WAVE", 8);
                return true;
            case ".ogg":
                expectedSignature = "an OGG file with an OggS capture pattern";
                isMatch = bytes => StartsWithAscii(bytes, "OggS");
                return true;
            case ".nif":
            case ".kf":
                expectedSignature = "a Gamebryo file header";
                isMatch = bytes => StartsWithAscii(bytes, "Gamebryo File Format");
                return true;
            default:
                expectedSignature = string.Empty;
                isMatch = _ => true;
                return false;
        }
    }

    private static bool StartsWithAscii(byte[] bytes, string value)
    {
        return HasAsciiAt(bytes, value, 0);
    }

    private static bool HasAsciiAt(byte[] bytes, string value, int offset)
    {
        if (bytes.Length < offset + value.Length)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            if (bytes[offset + index] != value[index])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsYamlNonSpecificTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag) ||
            StringComparer.Ordinal.Equals(tag, "!") ||
            StringComparer.Ordinal.Equals(tag, "?");
    }

    private static bool TryConvertJsonCompatibleScalar(string? value, out JsonNode? node)
    {
        if (StringComparer.Ordinal.Equals(value, "null"))
        {
            node = null;
            return true;
        }

        if (StringComparer.Ordinal.Equals(value, "true"))
        {
            node = JsonValue.Create(true);
            return true;
        }

        if (StringComparer.Ordinal.Equals(value, "false"))
        {
            node = JsonValue.Create(false);
            return true;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
        {
            node = JsonValue.Create(integer);
            return true;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
            double.IsFinite(number))
        {
            node = JsonValue.Create(number);
            return true;
        }

        node = null;
        return false;
    }

    private static string ToDisplayPath(string projectRoot, string path)
    {
        return Path.GetRelativePath(projectRoot, path).Replace('\\', '/');
    }

    private static bool IsInside(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedCandidate = Path.GetFullPath(candidate);
        return normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record LoadedSourceDocument(
        string Path,
        string DisplayPath,
        JsonObject Root,
        IReadOnlyDictionary<string, SourcePosition> SourceLocations);

    private sealed record LoadedManifest(
        string ProjectRoot,
        string ManifestPath,
        string DisplayPath,
        JsonObject Manifest,
        IReadOnlyDictionary<string, SourcePosition> SourceLocations);

    private sealed record RegistryDocument(
        string Path,
        string DisplayPath,
        JsonObject Root,
        IReadOnlyDictionary<string, SourcePosition> SourceLocations);

    private sealed record RequiredCapability(int Index, string Id);

    private sealed record AssetRecord(RegistryDocument Document, AssetEntry Asset);

    private sealed record VoiceTarget(AssetRecord Record, string Stem, string Extension);

    private sealed record DialogueQuestReference(int Index, string LineId, string QuestId);

    private sealed record QuestObjectiveStageReference(
        int QuestIndex,
        int ObjectiveIndex,
        string QuestId,
        string ObjectiveId,
        string PropertyName,
        string StageId,
        IReadOnlySet<string> StageIds);

    private sealed record QuestTransitionStageReference(
        int QuestIndex,
        int TransitionIndex,
        string QuestId,
        string TransitionId,
        string PropertyName,
        string StageId,
        IReadOnlySet<string> StageIds);

    private sealed record QuestConditionStageReference(
        int QuestIndex,
        int ConditionIndex,
        string QuestId,
        string ConditionId,
        string StageId,
        IReadOnlySet<string> StageIds);

    private sealed record DialogueVoiceWorkItem(
        int Index,
        string LineId,
        string Plugin,
        string VoiceType,
        string FileStem);

    private sealed record AssetEntry(
        int Index,
        string Id,
        string AssetType,
        string Source,
        string Target,
        bool Required);

    private readonly record struct SourcePosition(int Line, int Column);
}
