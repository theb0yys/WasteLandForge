using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Schema;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record GeckHostProbeIssue(string RuleId, string Message, string Path);
public sealed record GeckHostProbeFileIdentity(string Path, long Length, string Sha256);

public sealed record GeckHostProbeBuildPreview(
    string Status,
    GeckHostProbeFileIdentity? BuildManifest,
    GeckHostProbeFileIdentity? Dll,
    string? ProbeVersion,
    string? XnvseCommit,
    string? XnvseTree,
    bool LaunchAuthorized,
    string? ApprovedProbeRunId,
    IReadOnlyList<GeckHostProbeIssue> Issues,
    bool ExternalToolExecuted = false,
    bool FilesWritten = false);

public sealed class GeckHostProbeAuthorizedBuildPreviewer
{
    public const string ExpectedCommit = "694cdde6cbfa5e75afa661df587c73e8f0f6f441";
    public const string ExpectedTree = "e80453c217027f47979d2dfca03665de0a93fa6f";

    public GeckHostProbeBuildPreview Preview(string buildManifestPath, string dllPath, string runId)
    {
        var issues = new List<GeckHostProbeIssue>();
        GeckHostProbeFileIdentity? manifestIdentity = null;
        GeckHostProbeFileIdentity? dllIdentity = null;
        string? probeVersion = null;
        string? commit = null;
        string? tree = null;
        string? approvedRunId = null;
        var launchAuthorized = false;

        try
        {
            if (!HostProbeEvidence.IsRunId(runId))
                throw new InvalidDataException("The approved probe run ID must be 32 lowercase hexadecimal characters.");

            var manifestPath = HostProbeEvidence.ResolveRegularFile(buildManifestPath, "probe build manifest");
            var outputPath = HostProbeEvidence.ResolveRegularFile(dllPath, "probe DLL");
            manifestIdentity = HostProbeEvidence.Identity(manifestPath);
            dllIdentity = HostProbeEvidence.Identity(outputPath);
            var manifest = GeckHostProbeRunPlanParser.ReadStrictObject(manifestPath, "probe build manifest");

            RequireText(manifest, "formatVersion", "0.1");
            RequireText(manifest, "kind", "wastelandforge.geck-host-probe-build");
            RequireText(manifest, "configuration", "Release GECK");
            RequireText(manifest, "platform", "Win32");
            RequireText(manifest, "platformToolset", "v143");
            probeVersion = RequiredText(manifest, "probeVersion");
            var external = RequiredObject(manifest, "externalSource");
            RequireText(external, "name", "xNVSE");
            RequireText(external, "version", "6.4.4");
            commit = RequiredText(external, "commit");
            tree = RequiredText(external, "tree");
            if (!StringComparer.Ordinal.Equals(commit, ExpectedCommit) || !StringComparer.Ordinal.Equals(tree, ExpectedTree))
                throw new InvalidDataException("The probe build does not use the pinned xNVSE commit and tree.");
            if (external["clean"]?.GetValue<bool>() != true)
                throw new InvalidDataException("The probe build source was not recorded as clean.");

            var output = RequiredObject(manifest, "output");
            var declaredOutputPath = HostProbeEvidence.ResolveContainedPath(Path.GetDirectoryName(manifestPath)!, RequiredText(output, "path"));
            if (!StringComparer.OrdinalIgnoreCase.Equals(declaredOutputPath, outputPath))
                throw new InvalidDataException("The supplied probe DLL path does not match the build manifest output path.");
            if (output["length"]?.GetValue<long>() != dllIdentity.Length ||
                !StringComparer.Ordinal.Equals(output["sha256"]?.GetValue<string>(), dllIdentity.Sha256))
                throw new InvalidDataException("The probe DLL bytes do not match the build manifest.");

            launchAuthorized = manifest["launchAuthorized"]?.GetValue<bool>() == true;
            approvedRunId = RequiredText(manifest, "approvedProbeRunId");
            if (!launchAuthorized)
                issues.Add(HostProbeEvidence.Security("The probe build has launchAuthorized=false and is build-only evidence.", manifestPath));
            if (!StringComparer.Ordinal.Equals(approvedRunId, runId))
                issues.Add(HostProbeEvidence.Security("The probe build approvedProbeRunId does not match the requested run ID.", manifestPath));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or ArgumentException or CryptographicException)
        {
            issues.Add(HostProbeEvidence.Security("Authorized-build preview refused: " + exception.Message, buildManifestPath));
        }

        return new(
            issues.Count == 0 ? "planned" : "refused",
            manifestIdentity,
            dllIdentity,
            probeVersion,
            commit,
            tree,
            launchAuthorized,
            approvedRunId,
            issues);
    }

    private static JsonObject RequiredObject(JsonObject value, string name) =>
        value[name] as JsonObject ?? throw new InvalidDataException($"Probe build manifest property '{name}' is missing or invalid.");

    private static string RequiredText(JsonObject value, string name) =>
        value[name]?.GetValue<string>() is { Length: > 0 } text
            ? text
            : throw new InvalidDataException($"Probe build manifest property '{name}' is missing or invalid.");

    private static void RequireText(JsonObject value, string name, string expected)
    {
        if (!StringComparer.Ordinal.Equals(RequiredText(value, name), expected))
            throw new InvalidDataException($"Probe build manifest property '{name}' is not '{expected}'.");
    }
}

public sealed record GeckHostProbeStagingPreview(
    string Status,
    GeckHostProbeFileIdentity? Source,
    string? OperatorApprovedModRoot,
    string? DestinationPath,
    IReadOnlyList<GeckHostProbeIssue> Issues,
    bool CreateOnly = true,
    bool StagingPerformed = false,
    bool FilesWritten = false);

public sealed class GeckHostProbeStagingPreviewer
{
    public const string RelativePath = "NVSE/Plugins/WastelandForge.GeckProbe.dll";
    public const string EffectivePath = "Data/NVSE/Plugins/WastelandForge.GeckProbe.dll";

    public GeckHostProbeStagingPreview Preview(
        GeckHostProbeBuildPreview build,
        string modsRoot,
        string operatorApprovedModRoot,
        string gameDataRoot,
        string? mo2OverwriteRoot = null)
    {
        var issues = new List<GeckHostProbeIssue>();
        string? modRoot = null;
        string? destination = null;

        try
        {
            if (build.Status != "planned" || build.Dll is null)
                throw new InvalidDataException("An exact launch-authorized build preview is required before staging can be previewed.");
            if (!HostProbeEvidence.MatchesCurrent(build.Dll))
                throw new InvalidDataException("The launch-authorized probe DLL changed after build preview.");

            var root = HostProbeEvidence.ResolveRegularDirectory(modsRoot, "MO2 mods root");
            modRoot = HostProbeEvidence.ResolveRegularDirectory(operatorApprovedModRoot, "operator-approved probe mod root");
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetDirectoryName(modRoot), root))
                throw new InvalidDataException("The operator-approved probe mod must be an existing direct child of the MO2 mods root.");

            var dataRoot = Path.GetFullPath(gameDataRoot);
            if (HostProbeEvidence.IsInsideOrEqual(dataRoot, modRoot))
                throw new InvalidDataException("The staging destination must not be inside physical game Data.");
            if (!string.IsNullOrWhiteSpace(mo2OverwriteRoot) && HostProbeEvidence.IsInsideOrEqual(Path.GetFullPath(mo2OverwriteRoot), modRoot))
                throw new InvalidDataException("The staging destination must not be inside MO2 Overwrite.");

            var allowedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "NVSE", "NVSE/Plugins" };
            foreach (var entry in HostProbeEvidence.EnumerateEntriesNoReparse(modRoot))
            {
                var relative = HostProbeEvidence.Normalize(Path.GetRelativePath(modRoot, entry));
                if (File.Exists(entry) || !allowedDirectories.Contains(relative))
                    throw new InvalidDataException($"The operator-approved probe mod contains unexpected content: {relative}");
            }

            destination = Path.GetFullPath(Path.Combine(modRoot, RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!HostProbeEvidence.IsInsideOrEqual(modRoot, destination))
                throw new InvalidDataException("The staging target escaped the operator-approved mod root.");
            if (File.Exists(destination) || Directory.Exists(destination))
                throw new InvalidDataException("The create-only staging target already exists.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
        {
            issues.Add(HostProbeEvidence.Security("Create-only staging preview refused: " + exception.Message, operatorApprovedModRoot));
        }

        return new(
            issues.Count == 0 ? "planned" : "refused",
            build.Dll,
            modRoot,
            destination,
            issues);
    }
}

public sealed record GeckHostProbeProtectedFileRequest(
    string Set,
    string Root,
    string RelativePath,
    bool Required);

public sealed record GeckHostProbeProtectedEntry(
    string Set,
    string Path,
    string State,
    long? Length,
    string? Sha256);

public sealed record GeckHostProbeProtectedManifestRequest(
    string DataRoot,
    IReadOnlyList<GeckHostProbeProtectedFileRequest> ExactFiles);

public sealed record GeckHostProbeProtectedManifest(
    string Status,
    string? ManifestSha256,
    IReadOnlyList<GeckHostProbeProtectedEntry> Entries,
    IReadOnlyList<GeckHostProbeIssue> Issues,
    bool FilesWritten = false,
    bool ExternalToolExecuted = false);

public sealed class GeckHostProbeProtectedManifestCapture
{
    public GeckHostProbeProtectedManifest Capture(GeckHostProbeProtectedManifestRequest request)
    {
        var issues = new List<GeckHostProbeIssue>();
        var entries = new List<GeckHostProbeProtectedEntry>();
        try
        {
            var dataRoot = HostProbeEvidence.ResolveRegularDirectory(request.DataRoot, "physical game Data root");
            foreach (var path in Directory.EnumerateFiles(dataRoot, "*", SearchOption.TopDirectoryOnly)
                         .Where(path => Path.GetExtension(path).Equals(".esp", StringComparison.OrdinalIgnoreCase) ||
                                        Path.GetExtension(path).Equals(".esm", StringComparison.OrdinalIgnoreCase))
                         .Order(StringComparer.OrdinalIgnoreCase))
                entries.Add(CaptureFile("physical-plugin", dataRoot, path));

            var nvseRoot = Path.Combine(dataRoot, "NVSE", "Plugins");
            if (Directory.Exists(nvseRoot))
            {
                HostProbeEvidence.ResolveRegularDirectory(nvseRoot, "physical NVSE plugin root");
                foreach (var path in HostProbeEvidence.EnumerateFilesNoReparse(nvseRoot).Order(StringComparer.OrdinalIgnoreCase))
                    entries.Add(CaptureFile("physical-nvse-plugin-tree", dataRoot, path));
            }
            else
            {
                entries.Add(new("physical-nvse-plugin-tree", "NVSE/Plugins", "absent", null, null));
            }

            foreach (var exact in request.ExactFiles.OrderBy(item => item.Set, StringComparer.Ordinal).ThenBy(item => item.RelativePath, StringComparer.Ordinal))
            {
                var root = HostProbeEvidence.ResolveRegularDirectory(exact.Root, $"protected {exact.Set} root");
                var fullPath = HostProbeEvidence.ResolveContainedPath(root, exact.RelativePath);
                if (!File.Exists(fullPath))
                {
                    entries.Add(new(exact.Set, HostProbeEvidence.Normalize(exact.RelativePath), "absent", null, null));
                    if (exact.Required)
                        issues.Add(HostProbeEvidence.Security($"Required protected file is absent: {exact.Set}/{HostProbeEvidence.Normalize(exact.RelativePath)}", fullPath));
                    continue;
                }
                entries.Add(CaptureFile(exact.Set, root, HostProbeEvidence.ResolveRegularFile(fullPath, $"protected {exact.Set} file")));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException or CryptographicException)
        {
            issues.Add(HostProbeEvidence.Security("Protected-manifest capture refused: " + exception.Message, request.DataRoot));
        }

        var ordered = entries
            .OrderBy(item => item.Set, StringComparer.Ordinal)
            .ThenBy(item => item.Path, StringComparer.Ordinal)
            .ToArray();
        var digest = issues.Count == 0 ? DigestEntries(ordered) : null;
        return new(issues.Count == 0 ? "passed" : "refused", digest, ordered, issues);
    }

    private static GeckHostProbeProtectedEntry CaptureFile(string set, string root, string path)
    {
        if (HostProbeEvidence.IsReparse(path))
            throw new InvalidDataException($"Protected file is a reparse point: {path}");
        var before = new FileInfo(path);
        var identity = HostProbeEvidence.Identity(path);
        var after = new FileInfo(path);
        if (before.Length != after.Length || before.LastWriteTimeUtc != after.LastWriteTimeUtc)
            throw new InvalidDataException($"Protected file changed while it was being captured: {path}");
        return new(set, HostProbeEvidence.Normalize(Path.GetRelativePath(root, path)), "present", identity.Length, identity.Sha256);
    }

    private static string DigestEntries(IReadOnlyList<GeckHostProbeProtectedEntry> entries)
    {
        var array = new JsonArray(entries.Select(entry => new JsonObject
        {
            ["set"] = entry.Set,
            ["path"] = entry.Path,
            ["state"] = entry.State,
            ["length"] = entry.Length,
            ["sha256"] = entry.Sha256
        }).ToArray());
        return HostProbeEvidence.Sha(GeckHostProbeRunPlanParser.CanonicalBytes(array));
    }
}

public sealed record GeckHostProbeMo2Evidence(
    GeckHostProbeFileIdentity? Executable,
    string? Instance,
    string? Profile,
    GeckHostProbeFileIdentity? PythonPlugin,
    GeckHostProbeFileIdentity? CompanionPackage);

public sealed record GeckHostProbeEnvironmentEvidence(
    string GameRoot,
    string DataRoot,
    GeckHostProbeFileIdentity? Geck,
    GeckHostProbeFileIdentity? FalloutNv,
    GeckHostProbeFileIdentity? NvseLoader,
    GeckHostProbeFileIdentity? NvseRuntime,
    GeckHostProbeFileIdentity? NvseEditor,
    GeckHostProbeMo2Evidence Mo2);

public sealed record GeckHostProbeEffectiveProviderSnapshot(
    string Status,
    string? Instance,
    string? Profile,
    string? VirtualTreeSha256,
    IReadOnlyList<GeckHostProbeFileIdentity> Providers);

public sealed record GeckHostProbePlanRequest(
    string RunId,
    DateTimeOffset CreatedUtc,
    GeckHostProbeBuildPreview Build,
    GeckHostProbeEnvironmentEvidence Environment,
    GeckHostProbeStagingPreview Staging,
    GeckHostProbeEffectiveProviderSnapshot? EffectiveProviders,
    GeckHostProbeProtectedManifest? ProtectedBaseline,
    string ObservationRoot);

public sealed record GeckHostProbePlanPreview(
    string Status,
    JsonObject Plan,
    string PlanSha256,
    IReadOnlyList<GeckHostProbeIssue> Issues,
    bool Executable = false,
    bool ExternalToolExecuted = false,
    bool FilesWritten = false);

public sealed class GeckHostProbeRunPlanner
{
    public GeckHostProbePlanPreview Preview(GeckHostProbePlanRequest request)
    {
        var blockers = new SortedSet<string>(StringComparer.Ordinal);
        if (!HostProbeEvidence.IsRunId(request.RunId)) blockers.Add("run-id-unresolved");
        if (request.CreatedUtc.Offset != TimeSpan.Zero) blockers.Add("created-utc-not-utc");
        if (request.Build.Status != "planned" || !request.Build.LaunchAuthorized || request.Build.Dll is null || request.Build.BuildManifest is null || request.Build.ApprovedProbeRunId != request.RunId)
            blockers.Add("authorized-build-unresolved");
        else if (!HostProbeEvidence.MatchesCurrent(request.Build.Dll) || !HostProbeEvidence.MatchesCurrent(request.Build.BuildManifest))
            blockers.Add("authorized-build-drift");
        if (request.Environment.Geck is null || request.Environment.FalloutNv is null || request.Environment.NvseLoader is null || request.Environment.NvseRuntime is null || request.Environment.NvseEditor is null)
            blockers.Add("game-provider-identities-unresolved");
        if (request.Environment.Mo2.Executable is null || request.Environment.Mo2.PythonPlugin is null || request.Environment.Mo2.CompanionPackage is null)
            blockers.Add("mo2-provider-identities-unresolved");
        var environmentIdentities = new[]
        {
            request.Environment.Geck,
            request.Environment.FalloutNv,
            request.Environment.NvseLoader,
            request.Environment.NvseRuntime,
            request.Environment.NvseEditor,
            request.Environment.Mo2.Executable,
            request.Environment.Mo2.PythonPlugin,
            request.Environment.Mo2.CompanionPackage
        };
        if (environmentIdentities.Where(identity => identity is not null).Cast<GeckHostProbeFileIdentity>().Any(identity => !HostProbeEvidence.MatchesCurrent(identity)))
            blockers.Add("environment-identity-drift");
        if (string.IsNullOrWhiteSpace(request.Environment.Mo2.Instance)) blockers.Add("mo2-instance-unresolved");
        if (string.IsNullOrWhiteSpace(request.Environment.Mo2.Profile)) blockers.Add("mo2-profile-unresolved");
        if (request.Staging.Status != "planned" || request.Staging.DestinationPath is null) blockers.Add("create-only-staging-unresolved");
        ValidateEffectiveProviders(request, blockers);
        if (request.ProtectedBaseline is null || request.ProtectedBaseline.Status != "passed" || request.ProtectedBaseline.ManifestSha256 is null)
            blockers.Add("protected-baseline-unresolved");

        var status = blockers.Count == 0 ? "preview-ready" : "blocked";
        var plan = new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.geck-host-probe-run-plan",
            ["runId"] = request.RunId,
            ["createdUtc"] = request.CreatedUtc.ToUniversalTime().ToString("O"),
            ["status"] = status,
            ["projectRoot"] = null,
            ["probeBuild"] = new JsonObject
            {
                ["status"] = request.Build.Status,
                ["buildManifest"] = FileNode(request.Build.BuildManifest),
                ["dll"] = FileNode(request.Build.Dll),
                ["probeVersion"] = request.Build.ProbeVersion,
                ["xnvseCommit"] = request.Build.XnvseCommit,
                ["xnvseTree"] = request.Build.XnvseTree,
                ["launchAuthorized"] = request.Build.LaunchAuthorized,
                ["approvedProbeRunId"] = request.Build.ApprovedProbeRunId
            },
            ["environment"] = EnvironmentNode(request.Environment),
            ["staging"] = new JsonObject
            {
                ["status"] = request.Staging.Status,
                ["operatorApprovedModRoot"] = request.Staging.OperatorApprovedModRoot,
                ["destinationPath"] = request.Staging.DestinationPath,
                ["relativePath"] = GeckHostProbeStagingPreviewer.RelativePath,
                ["effectivePath"] = GeckHostProbeStagingPreviewer.EffectivePath,
                ["createOnly"] = true
            },
            ["effectiveProviders"] = EffectiveProvidersNode(request.EffectiveProviders),
            ["protectedBaseline"] = new JsonObject
            {
                ["status"] = request.ProtectedBaseline?.Status ?? "unresolved",
                ["manifestSha256"] = request.ProtectedBaseline?.ManifestSha256,
                ["entryCount"] = request.ProtectedBaseline?.Entries.Count ?? 0
            },
            ["observation"] = new JsonObject
            {
                ["root"] = Path.GetFullPath(request.ObservationRoot),
                ["queryFilePattern"] = "<runId>-<pid>-query.json",
                ["loadFilePattern"] = "<runId>-<pid>-load.json"
            },
            ["launch"] = new JsonObject
            {
                ["contextKind"] = "geck-host-probe",
                ["contextId"] = request.RunId,
                ["contextSha256"] = null,
                ["executable"] = request.Environment.Geck?.Path,
                ["arguments"] = new JsonArray()
            },
            ["recovery"] = new JsonObject
            {
                ["operatorClosesGeck"] = true,
                ["automaticRetry"] = false,
                ["preserveEvidence"] = true,
                ["newRunIdRequiredForRetry"] = true
            },
            ["blockers"] = new JsonArray(blockers.Select(blocker => JsonValue.Create(blocker)).ToArray()),
            ["safety"] = new JsonObject
            {
                ["authorizedBuildExecuted"] = false,
                ["stagingPerformed"] = false,
                ["profileMutated"] = false,
                ["requestWritten"] = false,
                ["externalToolExecuted"] = false,
                ["processLaunched"] = false,
                ["pluginMutation"] = false,
                ["physicalDataWritten"] = false
            }
        };
        var digest = GeckHostProbeRunPlanParser.ComputePlanDigest(plan);
        plan["planSha256"] = digest;
        GeckHostProbeRunPlanParser.ValidateSchema(plan);
        var issues = blockers.Select(blocker => HostProbeEvidence.Capability($"Host-probe run plan is blocked: {blocker}.", "geck-host-probe-run-plan")).ToArray();
        return new(status, plan, digest, issues);
    }

    private static void ValidateEffectiveProviders(GeckHostProbePlanRequest request, ISet<string> blockers)
    {
        var snapshot = request.EffectiveProviders;
        if (snapshot is null || snapshot.Status != "passed" || snapshot.VirtualTreeSha256 is null)
        {
            blockers.Add("effective-provider-view-unresolved");
            return;
        }
        if (!StringComparer.Ordinal.Equals(snapshot.Instance, request.Environment.Mo2.Instance) || !StringComparer.Ordinal.Equals(snapshot.Profile, request.Environment.Mo2.Profile))
            blockers.Add("effective-provider-context-mismatch");
        var expected = request.Build.Dll;
        if (expected is null || snapshot.Providers.Count != 1 ||
            !StringComparer.OrdinalIgnoreCase.Equals(HostProbeEvidence.Normalize(snapshot.Providers[0].Path), GeckHostProbeStagingPreviewer.EffectivePath) ||
            snapshot.Providers[0].Length != expected.Length ||
            !StringComparer.Ordinal.Equals(snapshot.Providers[0].Sha256, expected.Sha256))
            blockers.Add("effective-provider-set-mismatch");
    }

    private static JsonObject EnvironmentNode(GeckHostProbeEnvironmentEvidence environment) => new()
    {
        ["gameRoot"] = Path.GetFullPath(environment.GameRoot),
        ["dataRoot"] = Path.GetFullPath(environment.DataRoot),
        ["geck"] = FileNode(environment.Geck),
        ["falloutnv"] = FileNode(environment.FalloutNv),
        ["nvseLoader"] = FileNode(environment.NvseLoader),
        ["nvseRuntime"] = FileNode(environment.NvseRuntime),
        ["nvseEditor"] = FileNode(environment.NvseEditor),
        ["mo2"] = new JsonObject
        {
            ["executable"] = FileNode(environment.Mo2.Executable),
            ["instance"] = environment.Mo2.Instance,
            ["profile"] = environment.Mo2.Profile,
            ["pythonPlugin"] = FileNode(environment.Mo2.PythonPlugin),
            ["companionPackage"] = FileNode(environment.Mo2.CompanionPackage)
        }
    };

    private static JsonObject EffectiveProvidersNode(GeckHostProbeEffectiveProviderSnapshot? snapshot) => new()
    {
        ["status"] = snapshot?.Status ?? "unresolved",
        ["instance"] = snapshot?.Instance,
        ["profile"] = snapshot?.Profile,
        ["virtualTreeSha256"] = snapshot?.VirtualTreeSha256,
        ["providers"] = new JsonArray((snapshot?.Providers ?? []).OrderBy(item => item.Path, StringComparer.Ordinal).Select(item => new JsonObject
        {
            ["path"] = HostProbeEvidence.Normalize(item.Path),
            ["length"] = item.Length,
            ["sha256"] = item.Sha256
        }).ToArray())
    };

    private static JsonObject? FileNode(GeckHostProbeFileIdentity? file) => file is null ? null : new JsonObject
    {
        ["path"] = Path.GetFullPath(file.Path),
        ["length"] = file.Length,
        ["sha256"] = file.Sha256
    };
}

public sealed record GeckHostProbeRunPlanParseResult(
    string Status,
    string Path,
    JsonObject? Plan,
    string? PlanSha256,
    IReadOnlyList<GeckHostProbeIssue> Issues,
    bool Executable = false,
    bool ExternalToolExecuted = false,
    bool FilesWritten = false);

public sealed class GeckHostProbeRunPlanParser
{
    private static readonly Lazy<JsonSchema> Schema = new(LoadSchema);

    public GeckHostProbeRunPlanParseResult Parse(string path)
    {
        var issues = new List<GeckHostProbeIssue>();
        JsonObject? plan = null;
        string? digest = null;
        var fullPath = path;
        try
        {
            fullPath = HostProbeEvidence.ResolveRegularFile(path, "GECK host-probe run plan");
            plan = ReadStrictObject(fullPath, "GECK host-probe run plan");
            ValidateSchema(plan);
            digest = ComputePlanDigest(plan);
            if (!StringComparer.Ordinal.Equals(plan["planSha256"]?.GetValue<string>(), digest))
                throw new InvalidDataException("Run-plan digest does not match its canonical content.");
            VerifyCurrentFileIdentities(plan);
            var stagingTarget = plan["staging"]?["destinationPath"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(stagingTarget) && (File.Exists(stagingTarget) || Directory.Exists(stagingTarget)))
                throw new InvalidDataException("Create-only staging target now exists; the preview is stale.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException or ArgumentException or CryptographicException)
        {
            issues.Add(HostProbeEvidence.Security("GECK host-probe run plan refused: " + exception.Message, fullPath));
        }
        return new(issues.Count == 0 ? "parsed" : "refused", fullPath, plan, digest, issues);
    }

    internal static JsonObject ReadStrictObject(string path, string label)
    {
        var info = new FileInfo(path);
        if (info.Length > 4 * 1024 * 1024) throw new InvalidDataException($"{label} exceeds the 4 MiB limit.");
        var bytes = File.ReadAllBytes(path);
        if (bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble)) throw new InvalidDataException($"{label} must be UTF-8 without BOM.");
        var text = new UTF8Encoding(false, true).GetString(bytes);
        using var document = JsonDocument.Parse(text);
        RejectDuplicateKeys(document.RootElement, label);
        return JsonNode.Parse(text) as JsonObject ?? throw new JsonException($"{label} root is not an object.");
    }

    internal static void ValidateSchema(JsonObject plan)
    {
        using var document = JsonDocument.Parse(plan.ToJsonString());
        if (!Schema.Value.Evaluate(document.RootElement).IsValid)
            throw new InvalidDataException("Run plan does not satisfy geck-host-probe-run-plan/0.1.0.");
    }

    public static string ComputePlanDigest(JsonObject plan)
    {
        var clone = plan.DeepClone().AsObject();
        clone.Remove("planSha256");
        return HostProbeEvidence.Sha(CanonicalBytes(clone));
    }

    internal static byte[] CanonicalBytes(JsonNode node)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            WriteCanonical(writer, node);
        return stream.ToArray();
    }

    private static void WriteCanonical(Utf8JsonWriter writer, JsonNode? node)
    {
        switch (node)
        {
            case null:
                writer.WriteNullValue();
                break;
            case JsonObject value:
                writer.WriteStartObject();
                foreach (var property in value.OrderBy(item => item.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Key);
                    WriteCanonical(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonArray value:
                writer.WriteStartArray();
                foreach (var item in value) WriteCanonical(writer, item);
                writer.WriteEndArray();
                break;
            default:
                node.WriteTo(writer);
                break;
        }
    }

    private static void RejectDuplicateKeys(JsonElement element, string label)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!keys.Add(property.Name)) throw new InvalidDataException($"{label} contains duplicate JSON key '{property.Name}'.");
                RejectDuplicateKeys(property.Value, label);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray()) RejectDuplicateKeys(item, label);
        }
    }

    private static void VerifyCurrentFileIdentities(JsonObject plan)
    {
        var identities = new List<JsonObject?>
        {
            plan["probeBuild"]?["buildManifest"] as JsonObject,
            plan["probeBuild"]?["dll"] as JsonObject,
            plan["environment"]?["geck"] as JsonObject,
            plan["environment"]?["falloutnv"] as JsonObject,
            plan["environment"]?["nvseLoader"] as JsonObject,
            plan["environment"]?["nvseRuntime"] as JsonObject,
            plan["environment"]?["nvseEditor"] as JsonObject,
            plan["environment"]?["mo2"]?["executable"] as JsonObject,
            plan["environment"]?["mo2"]?["pythonPlugin"] as JsonObject,
            plan["environment"]?["mo2"]?["companionPackage"] as JsonObject
        };
        foreach (var identity in identities.Where(value => value is not null).Cast<JsonObject>())
        {
            var path = identity["path"]!.GetValue<string>();
            var current = HostProbeEvidence.Identity(HostProbeEvidence.ResolveRegularFile(path, "run-plan identity file"));
            if (current.Length != identity["length"]!.GetValue<long>() || !StringComparer.Ordinal.Equals(current.Sha256, identity["sha256"]!.GetValue<string>()))
                throw new InvalidDataException($"Run-plan identity changed: {path}");
        }
    }

    private static JsonSchema LoadSchema()
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.GeckHostProbeRunPlan010, out var resource) || resource is null)
            throw new InvalidOperationException("GECK host-probe run-plan schema is not registered.");
        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource));
    }
}

internal static partial class HostProbeEvidence
{
    [GeneratedRegex("^[0-9a-f]{32}$", RegexOptions.CultureInvariant)]
    private static partial Regex RunIdPattern();

    public static bool IsRunId(string value) => !string.IsNullOrWhiteSpace(value) && RunIdPattern().IsMatch(value);
    public static string Normalize(string path) => path.Replace('\\', '/');
    public static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static GeckHostProbeFileIdentity Identity(string path)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var digest = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        return new(Path.GetFullPath(path), stream.Length, digest);
    }

    public static bool MatchesCurrent(GeckHostProbeFileIdentity identity)
    {
        try
        {
            var current = Identity(ResolveRegularFile(identity.Path, "evidence identity file"));
            return current.Length == identity.Length && StringComparer.Ordinal.Equals(current.Sha256, identity.Sha256);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException or CryptographicException)
        {
            return false;
        }
    }

    public static string ResolveRegularFile(string path, string label)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException($"The {label} does not exist.", fullPath);
        EnsureNoReparseComponents(fullPath, label);
        return fullPath;
    }

    public static string ResolveRegularDirectory(string path, string label)
    {
        var fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath)) throw new DirectoryNotFoundException($"The {label} does not exist: {fullPath}");
        EnsureNoReparseComponents(fullPath, label);
        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static string ResolveContainedPath(string root, string relativePath)
    {
        if (Path.IsPathRooted(relativePath)) throw new InvalidDataException("Protected manifest paths must be root-relative.");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInsideOrEqual(root, fullPath) || StringComparer.OrdinalIgnoreCase.Equals(root, fullPath))
            throw new InvalidDataException("Protected manifest path escaped its declared root.");
        var current = root;
        foreach (var segment in Path.GetRelativePath(root, fullPath).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            if ((File.Exists(current) || Directory.Exists(current)) && IsReparse(current))
                throw new InvalidDataException("Protected manifest path contains a reparse point.");
        }
        return fullPath;
    }

    public static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return normalizedCandidate.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
               normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsReparse(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    public static IEnumerable<string> EnumerateEntriesNoReparse(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory, "*", SearchOption.TopDirectoryOnly).Order(StringComparer.OrdinalIgnoreCase))
            {
                if (IsReparse(entry)) throw new InvalidDataException($"A protected tree contains a reparse point: {entry}");
                yield return entry;
                if (Directory.Exists(entry)) pending.Push(entry);
            }
        }
    }

    public static IEnumerable<string> EnumerateFilesNoReparse(string root) =>
        EnumerateEntriesNoReparse(root).Where(File.Exists);

    private static void EnsureNoReparseComponents(string path, string label)
    {
        var current = path;
        while (!string.IsNullOrEmpty(current))
        {
            if ((File.Exists(current) || Directory.Exists(current)) && IsReparse(current))
                throw new InvalidDataException($"The {label} path contains a reparse point.");
            current = Path.GetDirectoryName(current);
        }
    }
    public static GeckHostProbeIssue Security(string message, string path) => new("WF-SEC-006", message, Normalize(path));
    public static GeckHostProbeIssue Capability(string message, string path) => new("WF-CAP-012", message, Normalize(path));
}
