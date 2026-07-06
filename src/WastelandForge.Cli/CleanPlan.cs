using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

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

internal sealed record CleanProjectIdentity(
    bool ManifestRead,
    string? ManifestPath,
    string? ProjectId,
    bool ConfirmationValidated,
    bool? ConfirmationMatches,
    string? RefusalStatus,
    string? RefusalReason)
{
    public static CleanProjectIdentity NotRead { get; } = new(
        ManifestRead: false,
        ManifestPath: null,
        ProjectId: null,
        ConfirmationValidated: false,
        ConfirmationMatches: null,
        RefusalStatus: null,
        RefusalReason: null);
}

internal sealed record CleanCacheLock(
    bool Checked,
    string MarkerPath,
    bool Present,
    string? RefusalStatus,
    string? RefusalReason)
{
    public static CleanCacheLock NotChecked(string projectRoot) => new(
        Checked: false,
        MarkerPath: CreateMarkerPath(projectRoot),
        Present: false,
        RefusalStatus: null,
        RefusalReason: null);

    public static string CreateMarkerPath(string projectRoot) =>
        Path.GetFullPath(Path.Combine(projectRoot, ".wastelandforge", "cache", "build.lock"));
}

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
    CleanProjectIdentity ProjectIdentity,
    CleanCacheLock CacheLock,
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
            CleanProjectIdentity.NotRead,
            CleanCacheLock.NotChecked(projectRoot),
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

        if (ShouldExecuteOutputRootClean(options, scope))
        {
            if (StringComparer.Ordinal.Equals(scope, "cache"))
            {
                var cacheLock = CheckCacheLock(projectRoot);
                result = result with { CacheLock = cacheLock };
                if (cacheLock.RefusalStatus is not null)
                {
                    return RefuseForCacheLock(result, cacheLock);
                }
            }

            return ExecuteCleanRoots(result);
        }

        if (!ShouldExecuteAllClean(options, scope))
        {
            return result;
        }

        var projectIdentity = ValidateAllCleanProjectIdentity(projectRoot, options.Confirm);
        result = result with { ProjectIdentity = projectIdentity };
        if (projectIdentity.RefusalStatus is not null)
        {
            return result with
            {
                SafetyStatus = "refused",
                OperationStatus = projectIdentity.RefusalStatus,
                RefusalReason = projectIdentity.RefusalReason,
                EffectiveDryRun = false
            };
        }

        var allCacheLock = CheckCacheLock(projectRoot);
        result = result with { CacheLock = allCacheLock };
        if (allCacheLock.RefusalStatus is not null)
        {
            return RefuseForCacheLock(result, allCacheLock);
        }

        return ExecuteCleanRoots(result);
    }

    private static bool ShouldExecuteOutputRootClean(CleanPlanOptions options, string scope) =>
        options.ScopeWasExplicit &&
        (StringComparer.Ordinal.Equals(scope, "generated") ||
            StringComparer.Ordinal.Equals(scope, "dist") ||
            StringComparer.Ordinal.Equals(scope, "cache")) &&
        !options.RequestedDryRun;

    private static bool ShouldExecuteAllClean(CleanPlanOptions options, string scope) =>
        options.ScopeWasExplicit &&
        StringComparer.Ordinal.Equals(scope, "all") &&
        options.Yes &&
        !string.IsNullOrWhiteSpace(options.Confirm) &&
        !options.RequestedDryRun;

    private static CleanCacheLock CheckCacheLock(string projectRoot)
    {
        var markerPath = CleanCacheLock.CreateMarkerPath(projectRoot);
        var present = File.Exists(markerPath);
        return new CleanCacheLock(
            Checked: true,
            MarkerPath: markerPath,
            Present: present,
            RefusalStatus: present ? "refused-active-build-cache-lock" : null,
            RefusalReason: present
                ? $"Active build/cache lock marker '{markerPath}' is present; cache-affecting clean execution was refused."
                : null);
    }

    private static CleanPlanResult RefuseForCacheLock(CleanPlanResult result, CleanCacheLock cacheLock) =>
        result with
        {
            SafetyStatus = "refused",
            OperationStatus = cacheLock.RefusalStatus ?? "refused-active-build-cache-lock",
            RefusalReason = cacheLock.RefusalReason,
            EffectiveDryRun = false,
            DeleteBehavior = false,
            FilesystemMutation = false
        };

    private static CleanProjectIdentity ValidateAllCleanProjectIdentity(string projectRoot, string? confirmation)
    {
        var manifestCandidates = FindProjectManifestCandidates(projectRoot);
        if (manifestCandidates.Count == 0)
        {
            return RefusedProjectIdentity(
                manifestRead: false,
                manifestPath: null,
                projectId: null,
                "refused-project-manifest-missing",
                "Project manifest is required to validate --confirm <project-id> before clean --all execution. Expected wastelandforge.json, wastelandforge.yaml, or wastelandforge.yml.");
        }

        if (manifestCandidates.Count > 1)
        {
            return RefusedProjectIdentity(
                manifestRead: false,
                manifestPath: manifestCandidates[0],
                projectId: null,
                "refused-multiple-project-manifests",
                "Multiple project manifests were found; clean --all requires exactly one manifest before project ID confirmation can be validated.");
        }

        var manifestPath = manifestCandidates[0];
        var readResult = ReadProjectId(manifestPath);
        if (readResult.Error is not null)
        {
            return RefusedProjectIdentity(
                manifestRead: true,
                manifestPath,
                projectId: null,
                readResult.RefusalStatus ?? "refused-project-manifest-unreadable",
                readResult.Error);
        }

        var projectId = readResult.ProjectId;
        if (!LogicalId.TryParse(projectId, out _))
        {
            return RefusedProjectIdentity(
                manifestRead: true,
                manifestPath,
                projectId,
                "refused-project-id-invalid",
                $"Project manifest '{manifestPath}' does not contain a valid top-level id for --confirm validation.");
        }

        var matches = StringComparer.Ordinal.Equals(projectId, confirmation);
        if (!matches)
        {
            return RefusedProjectIdentity(
                manifestRead: true,
                manifestPath,
                projectId,
                "refused-project-id-mismatch",
                $"Confirmation project ID '{confirmation}' does not match manifest project ID '{projectId}'.");
        }

        return new CleanProjectIdentity(
            ManifestRead: true,
            ManifestPath: manifestPath,
            ProjectId: projectId,
            ConfirmationValidated: true,
            ConfirmationMatches: true,
            RefusalStatus: null,
            RefusalReason: null);
    }

    private static CleanProjectIdentity RefusedProjectIdentity(
        bool manifestRead,
        string? manifestPath,
        string? projectId,
        string refusalStatus,
        string refusalReason) =>
        new(
            manifestRead,
            manifestPath,
            projectId,
            ConfirmationValidated: true,
            ConfirmationMatches: false,
            refusalStatus,
            refusalReason);

    private static IReadOnlyList<string> FindProjectManifestCandidates(string projectRoot)
    {
        if (!Directory.Exists(projectRoot))
        {
            return [];
        }

        var candidates = new[]
        {
            Path.Combine(projectRoot, "wastelandforge.json"),
            Path.Combine(projectRoot, "wastelandforge.yaml"),
            Path.Combine(projectRoot, "wastelandforge.yml")
        };

        return candidates
            .Where(File.Exists)
            .Select(Path.GetFullPath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static ProjectIdReadResult ReadProjectId(string manifestPath)
    {
        try
        {
            var extension = Path.GetExtension(manifestPath);
            var projectId = extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
                ? ReadJsonProjectId(manifestPath)
                : ReadYamlProjectId(manifestPath);

            return string.IsNullOrWhiteSpace(projectId)
                ? new ProjectIdReadResult(null, "refused-project-id-missing", $"Project manifest '{manifestPath}' does not contain a top-level id for --confirm validation.")
                : new ProjectIdReadResult(projectId, null, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or YamlException or InvalidOperationException)
        {
            return new ProjectIdReadResult(null, "refused-project-manifest-unreadable", $"Project manifest '{manifestPath}' could not be read for project ID confirmation: {ex.Message}");
        }
    }

    private static string? ReadJsonProjectId(string manifestPath)
    {
        var root = JsonNode.Parse(File.ReadAllText(manifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Manifest root must be a JSON object.");
        return root["id"]?.GetValue<string>();
    }

    private static string? ReadYamlProjectId(string manifestPath)
    {
        var stream = new YamlStream();
        using var reader = new StringReader(File.ReadAllText(manifestPath));
        stream.Load(reader);
        if (stream.Documents.Count != 1)
        {
            throw new InvalidOperationException("Manifest YAML must contain exactly one document.");
        }

        if (stream.Documents[0].RootNode is not YamlMappingNode mapping)
        {
            throw new InvalidOperationException("Manifest root must be a YAML mapping.");
        }

        foreach (var pair in mapping.Children)
        {
            if (pair.Key is YamlScalarNode { Value: "id" } &&
                pair.Value is YamlScalarNode value)
            {
                return value.Value;
            }
        }

        return null;
    }

    private static CleanPlanResult ExecuteCleanRoots(CleanPlanResult result)
    {
        var outsideRoots = result.PlannedRoots
            .Where(root => !root.Contained)
            .ToArray();
        if (outsideRoots.Length > 0)
        {
            return result with
            {
                SafetyStatus = "refused",
                OperationStatus = "refused-path-containment",
                RefusalReason = $"Clean target '{outsideRoots[0].Kind}' is outside the project root and was refused.",
                EffectiveDryRun = false
            };
        }

        var executedRoots = new List<CleanPlanRoot>(result.PlannedRoots.Count);
        var removedPaths = new List<string>();
        var missingPaths = new List<string>();

        foreach (var root in result.PlannedRoots)
        {
            var existsBefore = Directory.Exists(root.FullPath);
            if (existsBefore)
            {
                DeleteDirectoryTree(root.FullPath);
                removedPaths.Add(root.FullPath);
            }
            else
            {
                missingPaths.Add(root.FullPath);
            }

            executedRoots.Add(root with
            {
                PlannedAction = existsBefore ? "delete-root" : "delete-root-missing",
                ExistsBefore = existsBefore,
                Removed = existsBefore,
                Missing = !existsBefore
            });
        }

        var removedAny = removedPaths.Count > 0;
        var allScope = StringComparer.Ordinal.Equals(result.Scope, "all");

        return result with
        {
            SafetyStatus = removedAny ? "cleaned" : "missing",
            PlannedRoots = executedRoots,
            EffectiveDryRun = false,
            OperationStatus = CreateOperationStatus(executedRoots, allScope, removedAny),
            DeleteBehavior = true,
            FilesystemMutation = removedAny,
            RemovedPaths = removedPaths,
            MissingPaths = missingPaths
        };
    }

    private static string CreateOperationStatus(
        IReadOnlyList<CleanPlanRoot> roots,
        bool allScope,
        bool removedAny)
    {
        if (allScope)
        {
            return removedAny ? "deleted-all-roots" : "all-roots-missing";
        }

        var root = roots.Single();
        return removedAny ? $"deleted-{root.Kind}-root" : $"{root.Kind}-root-missing";
    }

    private static void DeleteDirectoryTree(string path)
    {
        try
        {
            Directory.Delete(path, recursive: true);
            return;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try
            {
                DeleteDirectoryTreeBottomUp(path);
            }
            catch (Exception retryEx) when (retryEx is IOException or UnauthorizedAccessException)
            {
                DeleteDirectoryTreeWithWindowsShell(path, retryEx);
            }
        }
    }

    private static void DeleteDirectoryTreeBottomUp(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            ClearReadOnlyAttribute(file);
            File.Delete(file);
        }

        var directories = Directory
            .EnumerateDirectories(path, "*", SearchOption.AllDirectories)
            .OrderByDescending(directory => directory.Length)
            .ToArray();

        foreach (var directory in directories)
        {
            ClearReadOnlyAttribute(directory);
            Directory.Delete(directory, recursive: false);
        }

        ClearReadOnlyAttribute(path);
        Directory.Delete(path, recursive: false);
    }

    private static void DeleteDirectoryTreeWithWindowsShell(string path, Exception previousException)
    {
        if (!OperatingSystem.IsWindows() || !Directory.Exists(path))
        {
            throw new IOException($"Could not delete clean target root '{path}'.", previousException);
        }

        var fullPath = Path.GetFullPath(path);
        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("rmdir");
        startInfo.ArgumentList.Add("/S");
        startInfo.ArgumentList.Add("/Q");
        startInfo.ArgumentList.Add(fullPath);

        using var process = Process.Start(startInfo)
            ?? throw new IOException($"Could not start Windows directory removal for clean target root '{fullPath}'.", previousException);
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0 && !Directory.Exists(fullPath))
        {
            return;
        }

        throw new IOException(
            $"Could not delete clean target root '{fullPath}' with Windows rmdir. Exit code: {process.ExitCode}. Stdout: {stdout.Trim()} Stderr: {stderr.Trim()}",
            previousException);
    }

    private static void ClearReadOnlyAttribute(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReadOnly) != 0)
        {
            File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
        }
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
            "Gate 258 executes explicit --generated, --dist, --cache, and manifest-confirmed --all cleans; omitted scope and unconfirmed --all remain path plans or refusals.",
            "Clean execution deletes only contained planned output roots after target-root containment validation.",
            "Cache-affecting clean execution is refused when .wastelandforge/cache/build.lock is present.",
            "Only root project manifest identity and the cache lock marker are read before relevant clean mutation; generated manifests, build manifests, provenance sidecars, checksums, artifacts, and local provider evidence are not read.",
            "Artifact existence checks beyond the selected target root, build planning, generator execution, package execution, release execution, provider resolution, capability scans, external tools, runtime probes, and AI calls are not performed.",
            StringComparer.Ordinal.Equals(scope, "all")
                ? "The all scope is severe; --yes and --confirm <project-id> must match the root project manifest id before mutation."
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

    private sealed record ProjectIdReadResult(string? ProjectId, string? RefusalStatus, string? Error);
}

internal static class CleanPlanTextRenderer
{
    public static string Render(CleanPlanResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("forge clean");
        builder.AppendLine($"Status: {result.SafetyStatus}");
        builder.AppendLine($"Mode: {(result.DeleteBehavior ? $"{result.Scope} clean execution" : "dry-run path plan")}");
        builder.AppendLine($"Project root: {result.ProjectRoot}");
        builder.AppendLine($"Scope: {result.Scope} ({result.ScopeContract.Risk}; {result.ScopeSource})");
        builder.AppendLine($"Effective dry-run: {result.EffectiveDryRun.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Delete behavior: {result.DeleteBehavior.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Filesystem mutation: {result.FilesystemMutation.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Operation: {result.OperationStatus}");
        builder.AppendLine($"Confirmation required: {result.ConfirmationRequired.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Confirmation provided: {result.ConfirmationProvided.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Cache lock checked: {result.CacheLock.Checked.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Cache lock marker: {result.CacheLock.MarkerPath}");
        builder.AppendLine($"Cache lock present: {result.CacheLock.Present.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Project manifest read: {result.ProjectIdentity.ManifestRead.ToString().ToLowerInvariant()}");
        if (result.ProjectIdentity.ManifestPath is not null)
        {
            builder.AppendLine($"Project manifest: {result.ProjectIdentity.ManifestPath}");
        }

        if (result.ProjectIdentity.ProjectId is not null)
        {
            builder.AppendLine($"Project id: {result.ProjectIdentity.ProjectId}");
        }

        if (result.ProjectIdentity.ConfirmationMatches is not null)
        {
            builder.AppendLine($"Confirmation matches project: {result.ProjectIdentity.ConfirmationMatches.Value.ToString().ToLowerInvariant()}");
        }

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
            ["mode"] = result.DeleteBehavior ? $"{result.Scope}-clean-execution" : "dry-run-path-plan",
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
            ["projectIdentity"] = ToProjectIdentity(result.ProjectIdentity),
            ["cacheLock"] = ToCacheLock(result.CacheLock),
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
                ["projectManifestRead"] = result.ProjectIdentity.ManifestRead,
                ["cacheLockCheck"] = result.CacheLock.Checked,
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

    private static JsonObject ToProjectIdentity(CleanProjectIdentity identity)
    {
        var json = new JsonObject
        {
            ["manifestRead"] = identity.ManifestRead,
            ["manifestPath"] = identity.ManifestPath,
            ["projectId"] = identity.ProjectId,
            ["confirmationValidated"] = identity.ConfirmationValidated,
            ["confirmationMatches"] = identity.ConfirmationMatches,
            ["refusalStatus"] = identity.RefusalStatus,
            ["refusalReason"] = identity.RefusalReason
        };

        return json;
    }

    private static JsonObject ToCacheLock(CleanCacheLock cacheLock)
    {
        var json = new JsonObject
        {
            ["checked"] = cacheLock.Checked,
            ["markerPath"] = cacheLock.MarkerPath,
            ["present"] = cacheLock.Present,
            ["refusalStatus"] = cacheLock.RefusalStatus,
            ["refusalReason"] = cacheLock.RefusalReason
        };

        return json;
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());
}
