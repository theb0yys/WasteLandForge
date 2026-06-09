using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed class ProjectValidationPipeline
{
    public DiagnosticReport Validate(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var projectRoot = Path.GetFullPath(projectPath);
        var issues = new List<DiagnosticIssue>();
        var manifestLoad = LoadManifest(projectRoot, issues);
        LogicalId? projectId = null;

        if (manifestLoad is not null)
        {
            var issueCountBeforeSchemaValidation = issues.Count;
            projectId = ValidateManifestShape(manifestLoad, issues);
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

        var manifestPath = candidates[0];
        if (!Path.GetExtension(manifestPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(CreateIssue(
                "WF-LOAD-003",
                DiagnosticSeverity.Error,
                "load",
                "YAML loading is not implemented in Gate 5",
                "YAML source contracts are documented, but YamlDotNet integration is deferred. Use wastelandforge.json for the Gate 5 validation slice.",
                new SourceLocation(ToDisplayPath(projectRoot, manifestPath)),
                docsRule: "WF-LOAD-003"));
            return null;
        }

        try
        {
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath)) as JsonObject;
            if (manifest is null)
            {
                issues.Add(CreateIssue(
                    "WF-LOAD-004",
                    DiagnosticSeverity.Error,
                    "load",
                    "Manifest root is not an object",
                    "The WastelandForge manifest must be a JSON object.",
                    new SourceLocation(ToDisplayPath(projectRoot, manifestPath)),
                    docsRule: "WF-LOAD-004"));
                return null;
            }

            return new LoadedManifest(projectRoot, manifestPath, manifest);
        }
        catch (JsonException ex)
        {
            issues.Add(CreateIssue(
                "WF-LOAD-004",
                DiagnosticSeverity.Error,
                "load",
                "Manifest JSON could not be parsed",
                ex.Message,
                new SourceLocation(ToDisplayPath(projectRoot, manifestPath)),
                docsRule: "WF-LOAD-004"));
            return null;
        }
    }

    private static LogicalId? ValidateManifestShape(LoadedManifest manifestLoad, List<DiagnosticIssue> issues)
    {
        var manifest = manifestLoad.Manifest;
        var manifestFile = ToDisplayPath(manifestLoad.ProjectRoot, manifestLoad.ManifestPath);
        LogicalId? projectId = null;

        RequireConstant(manifest, "schemaVersion", "0.1.0", manifestFile, "/schemaVersion", issues, "Unsupported manifest schema version");
        RequireConstant(manifest, "kind", "manifest", manifestFile, "/kind", issues, "Invalid manifest kind");
        RequireConstant(manifest, "game", "falloutnv", manifestFile, "/game", issues, "Unsupported target game");
        RequireString(manifest, "name", manifestFile, "/name", issues, "Missing project name");
        RequireString(manifest, "version", manifestFile, "/version", issues, "Missing project version");

        var rawId = RequireString(manifest, "id", manifestFile, "/id", issues, "Missing project id");
        if (rawId is not null)
        {
            if (LogicalId.TryParse(rawId, out var parsedProjectId))
            {
                projectId = parsedProjectId;
            }
            else
            {
                issues.Add(CreateIssue(
                    "WF-SCHEMA-003",
                    DiagnosticSeverity.Error,
                    "schema",
                    "Invalid project id",
                    "Project id must be a stable dotted lowercase logical identifier.",
                    new SourceLocation(manifestFile, JsonPointer.Parse("/id")),
                    projectId: null,
                    docsRule: "WF-SCHEMA-003"));
            }
        }

        var registries = manifest["registries"] as JsonObject;
        if (registries is null)
        {
            issues.Add(CreateIssue(
                "WF-SCHEMA-001",
                DiagnosticSeverity.Error,
                "schema",
                "Missing registries",
                "Manifest must declare registry roots.",
                new SourceLocation(manifestFile, JsonPointer.Parse("/registries")),
                projectId,
                docsRule: "WF-SCHEMA-001"));
        }
        else
        {
            RequireString(registries, "dependencies", manifestFile, "/registries/dependencies", issues, "Missing dependency registry", projectId);
            RequireString(registries, "capabilities", manifestFile, "/registries/capabilities", issues, "Missing capability registry", projectId);
        }

        return projectId;
    }

    private static void RunSemanticPlaceholder(LogicalId? projectId, LoadedManifest manifestLoad, List<DiagnosticIssue> issues)
    {
        var registries = manifestLoad.Manifest["registries"] as JsonObject;
        var dependencyPath = GetString(registries, "dependencies");
        var capabilityPath = GetString(registries, "capabilities");
        if (dependencyPath is null || capabilityPath is null)
        {
            return;
        }

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

        if (dependencyDocuments.Count == 0 || capabilityDocuments.Count == 0)
        {
            return;
        }

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

                issues.Add(CreateIssue(
                    "WF-SEM-014",
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Unknown capability reference",
                    $"Dependency registry references capability '{requiredCapability.Id}' which is not defined.",
                    new SourceLocation(
                        ToDisplayPath(manifestLoad.ProjectRoot, document.Path),
                        JsonPointer.Parse($"/requires/capabilities/{requiredCapability.Index}/id")),
                    projectId,
                    capabilityDocuments.Select(capabilityDocument => ToCapabilityRelatedLocation(manifestLoad.ProjectRoot, capabilityDocument)).ToArray(),
                    "Declare the capability in the capability registry or remove the dependency.",
                    "WF-SEM-014",
                    $"wf:sem:014:{requiredCapability.Id}"));
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
                $"The {label} registry root '{declaredPath}' did not resolve to any JSON files.",
                new SourceLocation(declaredPath),
                projectId,
                docsRule: "WF-LOAD-006"));
            return [];
        }

        var documents = new List<RegistryDocument>();
        foreach (var file in files)
        {
            try
            {
                var root = JsonNode.Parse(File.ReadAllText(file)) as JsonObject;
                if (root is null)
                {
                    issues.Add(CreateIssue(
                        "WF-LOAD-004",
                        DiagnosticSeverity.Error,
                        "load",
                        "Registry document root is not an object",
                        "Registry documents must be JSON objects.",
                        new SourceLocation(ToDisplayPath(projectRoot, file)),
                        projectId,
                        docsRule: "WF-LOAD-004"));
                    continue;
                }

                var kind = GetString(root, "kind");
                if (!StringComparer.Ordinal.Equals(kind, expectedKind))
                {
                    issues.Add(CreateIssue(
                        "WF-SCHEMA-001",
                        DiagnosticSeverity.Error,
                        "schema",
                        "Registry kind mismatch",
                        $"Expected registry kind '{expectedKind}'.",
                        new SourceLocation(ToDisplayPath(projectRoot, file), JsonPointer.Parse("/kind")),
                        projectId,
                        docsRule: "WF-SCHEMA-001"));
                    continue;
                }

                documents.Add(new RegistryDocument(file, root));
            }
            catch (JsonException ex)
            {
                issues.Add(CreateIssue(
                    "WF-LOAD-004",
                    DiagnosticSeverity.Error,
                    "load",
                    "Registry JSON could not be parsed",
                    ex.Message,
                    new SourceLocation(ToDisplayPath(projectRoot, file)),
                    projectId,
                    docsRule: "WF-LOAD-004"));
            }
        }

        return documents;
    }

    private static IReadOnlyList<string> ResolveRegistryFiles(string absolutePath)
    {
        if (File.Exists(absolutePath))
        {
            return Path.GetExtension(absolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase)
                ? [absolutePath]
                : [];
        }

        if (!Directory.Exists(absolutePath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(absolutePath, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
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

    private static SourceLocation ToCapabilityRelatedLocation(string projectRoot, RegistryDocument document)
    {
        var pointer = GetString(document.Root, "id") is not null
            ? "/id"
            : "/capabilities";

        return new SourceLocation(
            ToDisplayPath(projectRoot, document.Path),
            JsonPointer.Parse(pointer));
    }

    private static string? RequireString(
        JsonObject root,
        string propertyName,
        string file,
        string pointer,
        List<DiagnosticIssue> issues,
        string title,
        LogicalId? projectId = null)
    {
        var value = GetString(root, propertyName);
        if (value is not null)
        {
            return value;
        }

        issues.Add(CreateIssue(
            "WF-SCHEMA-001",
            DiagnosticSeverity.Error,
            "schema",
            title,
            $"Expected non-empty string property '{propertyName}'.",
            new SourceLocation(file, JsonPointer.Parse(pointer)),
            projectId,
            docsRule: "WF-SCHEMA-001"));
        return null;
    }

    private static void RequireConstant(
        JsonObject root,
        string propertyName,
        string expectedValue,
        string file,
        string pointer,
        List<DiagnosticIssue> issues,
        string title)
    {
        var value = GetString(root, propertyName);
        if (StringComparer.Ordinal.Equals(value, expectedValue))
        {
            return;
        }

        issues.Add(CreateIssue(
            propertyName == "schemaVersion" ? "WF-SCHEMA-002" : "WF-SCHEMA-001",
            DiagnosticSeverity.Error,
            "schema",
            title,
            $"Expected '{propertyName}' to be '{expectedValue}'.",
            new SourceLocation(file, JsonPointer.Parse(pointer)),
            docsRule: propertyName == "schemaVersion" ? "WF-SCHEMA-002" : "WF-SCHEMA-001"));
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

    private sealed record LoadedManifest(string ProjectRoot, string ManifestPath, JsonObject Manifest);

    private sealed record RegistryDocument(string Path, JsonObject Root);

    private sealed record RequiredCapability(int Index, string Id);
}
