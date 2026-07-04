using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record CleanPlanOptions(
    string? ProjectPath,
    string? Scope,
    bool ScopeWasExplicit,
    bool Yes,
    string? Confirm,
    bool RequestedDryRun);

internal sealed record CleanPlanRoot(
    string Kind,
    string RelativePath,
    string FullPath,
    string Boundary,
    bool Contained,
    string PlannedAction,
    bool? ExistsBefore,
    bool Removed,
    bool Missing);

internal sealed record CleanPlanResult(
    string ProjectInput,
    string ProjectRoot,
    string Scope,
    string ScopeSource,
    CleanScopeContract ScopeContract,
    IReadOnlyList<CleanPlanRoot> PlannedRoots,
    bool RequestedDryRun,
    bool EffectiveDryRun,
    bool ConfirmationRequired,
    bool ConfirmationProvided,
    string? ConfirmationValue,
    string SafetyStatus,
    string? RefusalReason,
    string OperationStatus,
    bool DeleteBehavior,
    bool FilesystemMutation,
    IReadOnlyList<string> RemovedPaths,
    IReadOnlyList<string> MissingPaths,
    IReadOnlyList<string> Boundaries);

internal static class CleanPlanPlanner
{
    public static CleanPlanResult Plan(CleanPlanOptions options)
    {
        var projectInput = string.IsNullOrWhiteSpace(options.ProjectPath) ? "." : options.ProjectPath;
        var projectRoot = Path.GetFullPath(projectInput);
        var scope = string.IsNullOrWhiteSpace(options.Scope) ? "generated" : options.Scope;
        var scopeSource = options.ScopeWasExplicit ? "explicit" : "default-generated";
        if (!CleanScopeContracts.TryGetByScope(scope, out var contract))
        {
            throw new InvalidOperationException($"Unknown clean scope '{scope}'.");
        }

        var confirmationRequired = StringComparer.Ordinal.Equals(scope, "all");
        var confirmationProvided = !confirmationRequired ||
            (options.Yes && !string.IsNullOrWhiteSpace(options.Confirm));

        var confirmationRefused = confirmationRequired && !confirmationProvided;
        var plannedRoots = CreateRoots(projectRoot, scope);
        var result = new CleanPlanResult(
            projectInput,
            projectRoot,
            scope,
            scopeSource,
            contract,
            plannedRoots,
            options.RequestedDryRun,
            EffectiveDryRun: true,
            confirmationRequired,
            confirmationProvided,
            options.Confirm,
            confirmationRefused ? "refused" : "planned",
            confirmationRefused
                ? "Scope 'all' requires --yes and --confirm <project-id> before clean execution can proceed."
                : null,
            confirmationRefused ? "refused" : "path-plan-only",
            DeleteBehavior: false,
            FilesystemMutation: false,
            RemovedPaths: [],
            MissingPaths: [],
            CreateBoundaries(scope));

        return ShouldExecuteGeneratedClean(options, scope)
            ? ExecuteGeneratedClean(result)
            : result;
    }

    private static bool ShouldExecuteGeneratedClean(CleanPlanOptions options, string scope) =>
        options.ScopeWasExplicit &&
        StringComparer.Ordinal.Equals(scope, "generated") &&
        !options.RequestedDryRun;

    private static CleanPlanResult ExecuteGeneratedClean(CleanPlanResult result)
    {
        var root = result.PlannedRoots.Single();
        if (!root.Contained)
        {
            return result with
            {
                SafetyStatus = "refused",
                OperationStatus = "refused-path-containment",
                RefusalReason = "Generated clean target is outside the project root and was refused.",
                EffectiveDryRun = false
            };
        }

        var existsBefore = Directory.Exists(root.FullPath);
        if (existsBefore)
        {
            Directory.Delete(root.FullPath, recursive: true);
        }

        var executedRoot = root with
        {
            PlannedAction = existsBefore ? "delete-root" : "delete-root-missing",
            ExistsBefore = existsBefore,
            Removed = existsBefore,
            Missing = !existsBefore
        };

        return result with
        {
            SafetyStatus = existsBefore ? "cleaned" : "missing",
            PlannedRoots = [executedRoot],
            EffectiveDryRun = false,
            OperationStatus = existsBefore ? "deleted-generated-root" : "generated-root-missing",
            DeleteBehavior = true,
            FilesystemMutation = existsBefore,
            RemovedPaths = existsBefore ? [root.FullPath] : [],
            MissingPaths = existsBefore ? [] : [root.FullPath]
        };
    }

    private static IReadOnlyList<CleanPlanRoot> CreateRoots(string projectRoot, string scope)
    {
        var roots = StringComparer.Ordinal.Equals(scope, "all")
            ? [Root("generated", "generated/", "Generated docs, scripts, reports, metadata, manifests, and checksums."), Root("dist", "dist/", "Build, package, staging, release dry-run, manifest, and checksum outputs."), Root("cache", ".wastelandforge/cache/", "Incremental planner/cache state.")]
            : scope switch
            {
                "generated" => [Root("generated", "generated/", "Generated docs, scripts, reports, metadata, manifests, and checksums.")],
                "dist" => [Root("dist", "dist/", "Build, package, staging, release dry-run, manifest, and checksum outputs.")],
                "cache" => [Root("cache", ".wastelandforge/cache/", "Incremental planner/cache state.")],
                _ => Array.Empty<CleanRootSpec>()
            };

        return roots
            .Select(root => CreateRoot(projectRoot, root))
            .ToArray();
    }

    private static CleanPlanRoot CreateRoot(string projectRoot, CleanRootSpec root)
    {
        var fullPath = TrimTrailingSeparators(Path.GetFullPath(Path.Combine(projectRoot, root.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
        return new CleanPlanRoot(
            root.Kind,
            root.RelativePath,
            fullPath,
            root.Boundary,
            IsInsideOrEqual(projectRoot, fullPath),
            "path-plan-only",
            ExistsBefore: null,
            Removed: false,
            Missing: false);
    }

    private static IReadOnlyList<string> CreateBoundaries(string scope) =>
        [
            "Gate 253 executes only explicit --generated cleans; omitted scope, --dist, --cache, and --all remain path plans or refusals.",
            "Generated clean execution deletes only the contained project generated/ root after target-root containment validation.",
            "Generated manifests, build manifests, provenance sidecars, checksums, artifacts, and local provider evidence are not read.",
            "Artifact existence checks beyond the selected target root, build planning, generator execution, package execution, release execution, provider resolution, capability scans, external tools, runtime probes, and AI calls are not performed.",
            StringComparer.Ordinal.Equals(scope, "all")
                ? "The all scope is severe; --yes and --confirm <project-id> are required before even a future mutation gate may proceed."
                : "The selected scope is planned under the project root only."
        ];

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string TrimTrailingSeparators(string path) =>
        path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static CleanRootSpec Root(string kind, string relativePath, string boundary) =>
        new(kind, relativePath, boundary);

    private sealed record CleanRootSpec(string Kind, string RelativePath, string Boundary);
}

internal static class CleanPlanTextRenderer
{
    public static string Render(CleanPlanResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("forge clean");
        builder.AppendLine($"Status: {result.SafetyStatus}");
        builder.AppendLine($"Mode: {(result.DeleteBehavior ? "generated clean execution" : "dry-run path plan")}");
        builder.AppendLine($"Project root: {result.ProjectRoot}");
        builder.AppendLine($"Scope: {result.Scope} ({result.ScopeContract.Risk}; {result.ScopeSource})");
        builder.AppendLine($"Effective dry-run: {result.EffectiveDryRun.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Delete behavior: {result.DeleteBehavior.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Filesystem mutation: {result.FilesystemMutation.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Operation: {result.OperationStatus}");
        builder.AppendLine($"Confirmation required: {result.ConfirmationRequired.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Confirmation provided: {result.ConfirmationProvided.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(result.RefusalReason))
        {
            builder.AppendLine($"Refusal: {result.RefusalReason}");
        }

        builder.AppendLine();
        builder.AppendLine("Planned roots:");
        foreach (var root in result.PlannedRoots)
        {
            var existsBefore = root.ExistsBefore is null
                ? "unknown"
                : root.ExistsBefore.Value.ToString().ToLowerInvariant();
            builder.AppendLine($"  - {root.Kind}: {root.RelativePath} -> {root.FullPath} ({root.PlannedAction}; contained={root.Contained.ToString().ToLowerInvariant()}; existsBefore={existsBefore}; removed={root.Removed.ToString().ToLowerInvariant()}; missing={root.Missing.ToString().ToLowerInvariant()})");
        }

        if (result.RemovedPaths.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Removed paths:");
            foreach (var path in result.RemovedPaths)
            {
                builder.AppendLine($"  - {path}");
            }
        }

        if (result.MissingPaths.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Missing paths:");
            foreach (var path in result.MissingPaths)
            {
                builder.AppendLine($"  - {path}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Boundary:");
        foreach (var boundary in result.Boundaries)
        {
            builder.AppendLine($"  - {boundary}");
        }

        return builder.ToString();
    }
}

internal static class CleanPlanJsonSerializer
{
    public static string Serialize(CleanPlanResult result)
    {
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "clean",
            ["status"] = result.SafetyStatus,
            ["mode"] = result.DeleteBehavior ? "generated-clean-execution" : "dry-run-path-plan",
            ["project"] = new JsonObject
            {
                ["input"] = result.ProjectInput,
                ["root"] = result.ProjectRoot
            },
            ["scope"] = new JsonObject
            {
                ["id"] = result.Scope,
                ["source"] = result.ScopeSource,
                ["flag"] = result.ScopeContract.Flag,
                ["root"] = result.ScopeContract.Root,
                ["risk"] = result.ScopeContract.Risk,
                ["confirmation"] = result.ScopeContract.Confirmation,
                ["reportExpectation"] = result.ScopeContract.ReportExpectation,
                ["boundary"] = result.ScopeContract.Boundary
            },
            ["plannedRoots"] = ToRootArray(result.PlannedRoots),
            ["safety"] = new JsonObject
            {
                ["requestedDryRun"] = result.RequestedDryRun,
                ["effectiveDryRun"] = result.EffectiveDryRun,
                ["deleteBehavior"] = result.DeleteBehavior,
                ["filesystemMutation"] = result.FilesystemMutation,
                ["confirmationRequired"] = result.ConfirmationRequired,
                ["confirmationProvided"] = result.ConfirmationProvided,
                ["confirmationValue"] = result.ConfirmationValue,
                ["refusalReason"] = result.RefusalReason
            },
            ["reportContract"] = new JsonObject
            {
                ["status"] = result.SafetyStatus,
                ["canonicalFormat"] = "json",
                ["summary"] = "Clean reports removed output roots, refused unsafe operations, and missing target roots.",
                ["mutatesFilesystemInCurrentGate"] = result.FilesystemMutation
            },
            ["boundaries"] = ToJsonArray(result.Boundaries),
            ["operation"] = new JsonObject
            {
                ["status"] = result.OperationStatus,
                ["removedPaths"] = ToJsonArray(result.RemovedPaths),
                ["missingPaths"] = ToJsonArray(result.MissingPaths)
            },
            ["execution"] = new JsonObject
            {
                ["deleteBehavior"] = result.DeleteBehavior,
                ["filesystemMutation"] = result.FilesystemMutation,
                ["targetRootExistenceCheck"] = result.DeleteBehavior,
                ["generatedManifestRead"] = false,
                ["buildManifestRead"] = false,
                ["provenanceSidecarRead"] = false,
                ["checksumRead"] = false,
                ["artifactExistenceCheck"] = false,
                ["buildPlanning"] = false,
                ["generatorExecution"] = false,
                ["packageExecution"] = false,
                ["releaseExecution"] = false,
                ["providerResolution"] = false,
                ["capabilityScanBehaviorChange"] = false,
                ["externalToolExecution"] = false,
                ["aiRequired"] = false
            }
        };

        return root.ToJsonString(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    private static JsonArray ToRootArray(IEnumerable<CleanPlanRoot> roots)
    {
        var array = new JsonArray();
        foreach (var root in roots)
        {
            var node = new JsonObject
            {
                ["kind"] = root.Kind,
                ["relativePath"] = root.RelativePath,
                ["fullPath"] = root.FullPath,
                ["boundary"] = root.Boundary,
                ["contained"] = root.Contained,
                ["plannedAction"] = root.PlannedAction,
                ["removed"] = root.Removed,
                ["missing"] = root.Missing
            };
            node["existsBefore"] = root.ExistsBefore;
            array.Add(node);
        }

        return array;
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());
}
