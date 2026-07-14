using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal interface IGeckAuthoringReviewCommandRunner
{
    Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments);
}

internal sealed class ForgeGeckAuthoringReviewCommandRunner(ForgeCommandRunner runner) : IGeckAuthoringReviewCommandRunner
{
    public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) =>
        runner.RunInWorkingDirectoryAsync(workingDirectory, cancellationToken, arguments);
}

internal enum GeckAuthoringReviewTarget
{
    Plan,
    SubjectHandoff,
    Observer,
    Verification
}

internal enum GeckAuthoringReviewState
{
    NotLoaded,
    Blocked,
    ReadyToGenerate,
    Current,
    Locked,
    Required,
    Selected,
    AcceptedForPreview,
    ReadyToSeal,
    Verified,
    Failed,
    RefreshRequired
}

internal sealed record GeckAuthoringReviewDiagnostic(
    string RuleId,
    string Severity,
    string Stage,
    string Title,
    string Message,
    string File);

internal sealed record GeckAuthoringReviewOutput(string Path, string FullPath, string Sha256, long Length, bool Current);

internal sealed record GeckAuthoringReviewOperationResult(
    bool Success,
    bool Cancelled,
    string Message,
    string? PreviewToken,
    string? PlanSha256,
    IReadOnlyList<GeckAuthoringReviewOutput> Outputs,
    IReadOnlyList<GeckAuthoringReviewDiagnostic> Diagnostics);

internal sealed record GeckAuthoringReviewSnapshot(
    string ProjectRoot,
    string? ObservationsPath,
    GeckAuthoringReviewState PlanState,
    GeckAuthoringReviewState ObserverState,
    GeckAuthoringReviewState ObservationsState,
    GeckAuthoringReviewState VerificationState,
    string Message,
    string? PlanSha256,
    IReadOnlyList<GeckAuthoringReviewOutput> Outputs,
    IReadOnlyList<GeckAuthoringReviewDiagnostic> Diagnostics)
{
    public GeckAuthoringReviewState SubjectHandoffState { get; init; } = GeckAuthoringReviewState.Locked;
    public bool CanPreviewPlan => PlanState != GeckAuthoringReviewState.NotLoaded;
    public bool CanPreviewSubjectHandoff => PlanState == GeckAuthoringReviewState.Current;
    public bool CanPreviewObserver => PlanState == GeckAuthoringReviewState.Current;
    public bool CanPreviewVerification => ObserverState == GeckAuthoringReviewState.Current &&
        ObservationsState is GeckAuthoringReviewState.Selected or GeckAuthoringReviewState.AcceptedForPreview;

    public static GeckAuthoringReviewSnapshot NotLoaded() => new(
        string.Empty,
        null,
        GeckAuthoringReviewState.NotLoaded,
        GeckAuthoringReviewState.Locked,
        GeckAuthoringReviewState.Required,
        GeckAuthoringReviewState.Locked,
        "Select a Forge project and refresh authoring evidence.",
        null,
        [],
        []);
}

internal sealed class GeckAuthoringReviewWorkspace(IGeckAuthoringReviewCommandRunner runner)
{
    private const string PlanTarget = "geck-authoring-plan";
    private const string SubjectHandoffTarget = "geck-authoring-subject-handoff";
    private const string ObserverTarget = "geck-authoring-verifier";
    private const string VerificationTarget = "geck-authoring-verification";
    private const string PlanPath = "generated/geck-authoring-plan/plan.json";
    private const int MaxObservationBytes = 4 * 1024 * 1024;

    public async Task<GeckAuthoringReviewSnapshot> InspectAsync(string projectRoot, string? observationsPath, CancellationToken cancellationToken)
    {
        var root = NormalizeProjectRoot(projectRoot);
        var diagnostics = new List<GeckAuthoringReviewDiagnostic>();
        var outputs = new List<GeckAuthoringReviewOutput>();

        var plan = await RunAsync(root, GeckAuthoringReviewTarget.Plan, dryRun: true, null, cancellationToken).ConfigureAwait(false);
        diagnostics.AddRange(plan.Diagnostics);
        outputs.AddRange(plan.Outputs);
        if (!plan.Success)
            return Snapshot(root, observationsPath, GeckAuthoringReviewState.Blocked, GeckAuthoringReviewState.Locked,
                string.IsNullOrWhiteSpace(observationsPath) ? GeckAuthoringReviewState.Required : GeckAuthoringReviewState.Selected,
                GeckAuthoringReviewState.Locked, plan.Message, plan.PlanSha256, outputs, diagnostics);

        GeckAuthoringReviewOutput? planOutput = null;
        var planCurrent = plan.PlanSha256 is not null && FileMatches(root, PlanPath, plan.PlanSha256, out planOutput);
        if (planOutput is not null) outputs.Add(planOutput);
        if (!planCurrent)
            return Snapshot(root, observationsPath, GeckAuthoringReviewState.ReadyToGenerate, GeckAuthoringReviewState.Locked,
                string.IsNullOrWhiteSpace(observationsPath) ? GeckAuthoringReviewState.Required : GeckAuthoringReviewState.Selected,
                GeckAuthoringReviewState.Locked, "Authoring plan preview is valid; generate the current plan before continuing.", plan.PlanSha256, outputs, diagnostics);

        var subjectHandoff = await RunAsync(root, GeckAuthoringReviewTarget.SubjectHandoff, dryRun: true, null, cancellationToken).ConfigureAwait(false);
        diagnostics.AddRange(subjectHandoff.Diagnostics);
        outputs.AddRange(subjectHandoff.Outputs);
        var subjectHandoffState = !subjectHandoff.Success
            ? GeckAuthoringReviewState.Blocked
            : subjectHandoff.Outputs.Count > 0 && subjectHandoff.Outputs.All(output => output.Current)
                ? GeckAuthoringReviewState.Current
                : GeckAuthoringReviewState.ReadyToGenerate;

        var observer = await RunAsync(root, GeckAuthoringReviewTarget.Observer, dryRun: true, null, cancellationToken).ConfigureAwait(false);
        diagnostics.AddRange(observer.Diagnostics);
        outputs.AddRange(observer.Outputs);
        if (!observer.Success)
            return Snapshot(root, observationsPath, GeckAuthoringReviewState.Current, GeckAuthoringReviewState.Blocked,
                string.IsNullOrWhiteSpace(observationsPath) ? GeckAuthoringReviewState.Required : GeckAuthoringReviewState.Selected,
                GeckAuthoringReviewState.Locked, observer.Message, plan.PlanSha256, outputs, diagnostics, subjectHandoffState);

        var observerCurrent = observer.Outputs.Count > 0 && observer.Outputs.All(output => output.Current);
        if (!TryResolveObservation(root, observationsPath, out var observation, out var observationFailure))
        {
            if (observationFailure is not null) diagnostics.Add(observationFailure);
            var observationState = string.IsNullOrWhiteSpace(observationsPath) ? GeckAuthoringReviewState.Required : GeckAuthoringReviewState.Blocked;
            return Snapshot(root, observationsPath, GeckAuthoringReviewState.Current,
                observerCurrent ? GeckAuthoringReviewState.Current : GeckAuthoringReviewState.ReadyToGenerate,
                observationState, GeckAuthoringReviewState.Locked,
                observationFailure?.Message ?? (observerCurrent
                    ? "Observer bundle is current. Select project-contained raw observations to continue."
                    : "Generate the current observer bundle, then select raw observations."),
                plan.PlanSha256, outputs, diagnostics, subjectHandoffState);
        }

        var selectedObservation = observation!;
        if (!observerCurrent)
            return Snapshot(root, selectedObservation.RelativePath, GeckAuthoringReviewState.Current, GeckAuthoringReviewState.ReadyToGenerate,
                GeckAuthoringReviewState.Selected, GeckAuthoringReviewState.Locked,
                "Observer preview is valid; generate the current bundle before sealing observations.", plan.PlanSha256, outputs, diagnostics, subjectHandoffState);

        var verification = await RunAsync(root, GeckAuthoringReviewTarget.Verification, dryRun: true, selectedObservation, cancellationToken).ConfigureAwait(false);
        diagnostics.AddRange(verification.Diagnostics);
        outputs.AddRange(verification.Outputs);
        if (!verification.Success)
        {
            var semanticFailure = verification.Diagnostics.Any(issue => issue.RuleId == "WF-SEM-046");
            return Snapshot(root, selectedObservation.RelativePath, GeckAuthoringReviewState.Current, GeckAuthoringReviewState.Current,
                semanticFailure ? GeckAuthoringReviewState.AcceptedForPreview : GeckAuthoringReviewState.Blocked,
                semanticFailure ? GeckAuthoringReviewState.Failed : GeckAuthoringReviewState.Locked,
                verification.Message, plan.PlanSha256, outputs, diagnostics, subjectHandoffState);
        }

        var reportCurrent = verification.Outputs.Count == 1 && verification.Outputs.All(output => output.Current);
        return Snapshot(root, selectedObservation.RelativePath, GeckAuthoringReviewState.Current, GeckAuthoringReviewState.Current,
            GeckAuthoringReviewState.AcceptedForPreview,
            reportCurrent ? GeckAuthoringReviewState.Verified : GeckAuthoringReviewState.ReadyToSeal,
            reportCurrent ? "Semantic verification report is current." : "Observations passed preview; seal the semantic verification report.",
            plan.PlanSha256, outputs, diagnostics, subjectHandoffState);
    }

    public async Task<GeckAuthoringReviewOperationResult> PreviewAsync(
        GeckAuthoringReviewTarget target,
        string projectRoot,
        string? observationsPath,
        CancellationToken cancellationToken)
    {
        var root = NormalizeProjectRoot(projectRoot);
        ObservationEvidence? observation = null;
        if (target == GeckAuthoringReviewTarget.Verification &&
            !TryResolveObservation(root, observationsPath, out observation, out var failure))
            return Failed(failure?.Message ?? "Select project-contained raw observations before previewing verification.", failure is null ? [] : [failure]);

        var result = await RunAsync(root, target, dryRun: true, observation, cancellationToken).ConfigureAwait(false);
        if (!result.Success || result.Cancelled) return ToOperation(result, null);
        var token = Token(result, observation);
        var label = target switch
        {
            GeckAuthoringReviewTarget.Plan => "Authoring plan preview ready. No files were written.",
            GeckAuthoringReviewTarget.SubjectHandoff => "Verifier subject handoff preview ready. No files were written.",
            GeckAuthoringReviewTarget.Observer => "Observer bundle preview ready. No files were written.",
            _ => "Semantic verification preview passed. No report was written."
        };
        return ToOperation(result, token) with { Message = label };
    }

    public async Task<GeckAuthoringReviewOperationResult> ApplyAsync(
        GeckAuthoringReviewTarget target,
        string projectRoot,
        string? observationsPath,
        string previewToken,
        CancellationToken cancellationToken)
    {
        var preview = await PreviewAsync(target, projectRoot, observationsPath, cancellationToken).ConfigureAwait(false);
        if (!preview.Success || preview.Cancelled) return preview with { PreviewToken = null };
        if (!StringComparer.Ordinal.Equals(preview.PreviewToken, previewToken))
            return Failed("Inputs changed. Preview again.", []);

        var root = NormalizeProjectRoot(projectRoot);
        ObservationEvidence? observation = null;
        if (target == GeckAuthoringReviewTarget.Verification &&
            !TryResolveObservation(root, observationsPath, out observation, out var failure))
            return Failed(failure?.Message ?? "Selected observations are no longer available.", failure is null ? [] : [failure]);

        var result = await RunAsync(root, target, dryRun: false, observation, cancellationToken).ConfigureAwait(false);
        if (!result.Success || result.Cancelled) return ToOperation(result, null);
        if (result.Outputs.Count == 0 || result.Outputs.Any(output => !output.Current))
            return Failed("Backend output evidence was not current after generation.", [BridgeIssue("Generated output digest or containment check failed.")]);

        var message = target switch
        {
            GeckAuthoringReviewTarget.Plan => "Current authoring plan generated.",
            GeckAuthoringReviewTarget.SubjectHandoff => "Current manual verifier subject handoff generated.",
            GeckAuthoringReviewTarget.Observer => "Current read-only observer bundle generated.",
            _ => "Semantic verification report sealed and verified."
        };
        return ToOperation(result, null) with { Message = message };
    }

    private async Task<BackendResult> RunAsync(
        string root,
        GeckAuthoringReviewTarget target,
        bool dryRun,
        ObservationEvidence? observation,
        CancellationToken cancellationToken)
    {
        var targetName = TargetName(target);
        var arguments = new List<string> { "generate", root, "--target", targetName };
        if (target == GeckAuthoringReviewTarget.Verification)
        {
            if (observation is null) return BackendResult.Failed("Raw observations are required.", [BridgeIssue("Raw observations are required.")]);
            arguments.AddRange(["--observations", observation.RelativePath]);
        }
        if (dryRun) arguments.Add("--dry-run");
        arguments.AddRange(["--format", "json", "--no-input"]);

        try
        {
            var command = await runner.RunAsync(root, cancellationToken, arguments.ToArray()).ConfigureAwait(false);
            if (command.ExitCode == 7 || cancellationToken.IsCancellationRequested)
                return BackendResult.CancelledResult();
            return Parse(root, target, dryRun, command);
        }
        catch (OperationCanceledException)
        {
            return BackendResult.CancelledResult();
        }
    }

    private static BackendResult Parse(string root, GeckAuthoringReviewTarget target, bool dryRun, ForgeCommandResult command)
    {
        try
        {
            var json = JsonNode.Parse(command.StandardOutput)?.AsObject() ?? throw new InvalidOperationException("Backend JSON is not an object.");
            var expectedTarget = TargetName(target);
            if (Text(json, "formatVersion") != "1.0") throw new InvalidOperationException("Backend JSON format version changed.");
            if (Text(json, "command") != "generate") throw new InvalidOperationException("Backend command identity is not generate.");
            if (Text(json, "target") != expectedTarget) throw new InvalidOperationException("Backend target identity changed.");
            if (json["dryRun"]?.GetValue<bool>() != dryRun) throw new InvalidOperationException("Backend dry-run identity changed.");
            if (!SamePath(root, Text(json, "projectRoot"))) throw new InvalidOperationException("Backend project root changed.");
            var tool = json["tool"]?.AsObject() ?? throw new InvalidOperationException("Backend tool identity is missing.");
            var toolName = Text(tool, "name");
            var toolVersion = Text(tool, "version");
            if (toolName != "WastelandForge" || toolVersion.Length == 0) throw new InvalidOperationException("Backend tool identity changed.");

            var issues = ParseIssues(json["issues"]?.AsArray() ?? throw new InvalidOperationException("Backend issues are missing."));
            var outputs = ParseOutputs(root, json["outputs"]?.AsArray() ?? throw new InvalidOperationException("Backend outputs are missing."));
            var safety = json["safety"]?.AsObject() ?? throw new InvalidOperationException("Backend safety evidence is missing.");
            ValidateSafety(target, dryRun, safety);
            var safetyFingerprint = string.Join('|', safety
                .OrderBy(property => property.Key, StringComparer.Ordinal)
                .Select(property => $"{property.Key}={property.Value?.ToJsonString()}"));
            var status = Text(json, "status");
            var successStatus = dryRun ? "planned" : target == GeckAuthoringReviewTarget.Verification ? "verified" : "passed";
            var success = command.ExitCode == 0 && status == successStatus && !issues.Any(issue => issue.Severity == "error");
            if (command.ExitCode == 0 && !success) throw new InvalidOperationException("Backend success status is inconsistent.");
            if (command.ExitCode != 0 && status != "failed") throw new InvalidOperationException("Backend failure status is inconsistent.");

            var planSha = target is GeckAuthoringReviewTarget.Plan or GeckAuthoringReviewTarget.SubjectHandoff ? TextOrNull(json, "planSha256") : null;
            if (success && target is (GeckAuthoringReviewTarget.Plan or GeckAuthoringReviewTarget.SubjectHandoff) && (planSha is null || planSha.Length != 64))
                throw new InvalidOperationException("Backend plan digest is missing or invalid.");
            var message = success ? status : issues.FirstOrDefault()?.Message ?? ErrorFallback(command);
            return new(success, false, message, toolVersion, planSha, safetyFingerprint, outputs, issues, expectedTarget, root, dryRun);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or ArgumentException or IOException or UnauthorizedAccessException or CryptographicException)
        {
            var issue = BridgeIssue("Backend result could not be trusted: " + exception.Message);
            return BackendResult.Failed(issue.Message, [issue]);
        }
    }

    private static IReadOnlyList<GeckAuthoringReviewDiagnostic> ParseIssues(JsonArray issues) => issues.Select(node =>
    {
        var issue = node?.AsObject() ?? throw new InvalidOperationException("Backend issue is not an object.");
        var location = issue["primaryLocation"]?.AsObject();
        return new GeckAuthoringReviewDiagnostic(
            Text(issue, "ruleId"), Text(issue, "severity"), Text(issue, "category"), Text(issue, "title"), Text(issue, "message"),
            location is null ? string.Empty : Text(location, "file"));
    }).ToArray();

    private static IReadOnlyList<GeckAuthoringReviewOutput> ParseOutputs(string root, JsonArray outputs) => outputs.Select(node =>
    {
        var output = node?.AsObject() ?? throw new InvalidOperationException("Backend output is not an object.");
        var relative = NormalizeRelative(Text(output, "path"));
        var sha = Text(output, "sha256");
        var length = output["length"]?.GetValue<long>() ?? throw new InvalidOperationException("Backend output length is missing.");
        if (Path.IsPathRooted(relative) || relative.Length == 0 || sha.Length != 64 || length < 0)
            throw new InvalidOperationException("Backend output identity is invalid.");
        var full = ResolveContained(root, relative);
        return new GeckAuthoringReviewOutput(relative, full, sha, length, FileMatches(full, sha, length));
    }).OrderBy(output => output.Path, StringComparer.Ordinal).ToArray();

    private static void ValidateSafety(GeckAuthoringReviewTarget target, bool dryRun, JsonObject safety)
    {
        if (target == GeckAuthoringReviewTarget.Plan)
        {
            if (Boolean(safety, "executesExternalTools") || Boolean(safety, "writesPluginBytes") || Boolean(safety, "writesGameData"))
                throw new InvalidOperationException("Backend plan safety flags permit an unsafe effect.");
            return;
        }

        if (target == GeckAuthoringReviewTarget.SubjectHandoff)
        {
            if (Boolean(safety, "executesExternalTools") || Boolean(safety, "writesPluginBytes") || Boolean(safety, "writesGameData") ||
                Boolean(safety, "verificationPerformed") || Boolean(safety, "approvalGranted") || Boolean(safety, "promotionPerformed"))
                throw new InvalidOperationException("Backend subject-handoff safety flags permit an unsafe effect or claim.");
            if (dryRun && Boolean(safety, "filesWritten"))
                throw new InvalidOperationException("Backend subject-handoff dry-run reported file writes.");
            return;
        }

        if (Boolean(safety, "externalToolExecuted") || Boolean(safety, "pluginMutation") || Boolean(safety, "writesGameData"))
            throw new InvalidOperationException("Backend verifier safety flags permit an unsafe effect.");
        if (dryRun && Boolean(safety, "filesWritten"))
            throw new InvalidOperationException("Backend dry-run reported file writes.");
    }

    private static bool TryResolveObservation(
        string root,
        string? value,
        out ObservationEvidence? observation,
        out GeckAuthoringReviewDiagnostic? failure)
    {
        observation = null;
        failure = null;
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            var candidate = Path.IsPathRooted(value.Trim())
                ? Path.GetFullPath(value.Trim())
                : Path.GetFullPath(Path.Combine(root, value.Trim().Replace('/', Path.DirectorySeparatorChar)));
            if (!ContainedBy(root, candidate)) throw new InvalidOperationException("Raw observations must stay inside the selected project.");
            if (!File.Exists(candidate)) throw new InvalidOperationException("Raw observations file does not exist.");
            var info = new FileInfo(candidate);
            if (info.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new InvalidOperationException("Raw observations cannot be a reparse point.");
            if (info.Length > MaxObservationBytes) throw new InvalidOperationException("Raw observations exceed the 4 MiB evidence limit.");
            var relative = NormalizeRelative(Path.GetRelativePath(root, candidate));
            observation = new(relative, info.Length, Sha(File.ReadAllBytes(candidate)));
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException or PathTooLongException)
        {
            failure = BridgeIssue(exception.Message);
            return false;
        }
    }

    private static string Token(BackendResult result, ObservationEvidence? observation)
    {
        var payload = new StringBuilder()
            .AppendLine(result.Target)
            .AppendLine(result.ProjectRoot)
            .AppendLine(result.ToolVersion)
            .AppendLine(result.PlanSha256 ?? string.Empty)
            .AppendLine(result.SafetyFingerprint)
            .AppendLine(observation?.RelativePath ?? string.Empty)
            .AppendLine(observation?.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty)
            .AppendLine(observation?.Sha256 ?? string.Empty);
        foreach (var output in result.Outputs.OrderBy(output => output.Path, StringComparer.Ordinal))
            payload.Append(output.Path).Append('|').Append(output.Length).Append('|').AppendLine(output.Sha256);
        return Sha(Encoding.UTF8.GetBytes(payload.ToString()));
    }

    private static GeckAuthoringReviewSnapshot Snapshot(
        string root,
        string? observations,
        GeckAuthoringReviewState plan,
        GeckAuthoringReviewState observer,
        GeckAuthoringReviewState observation,
        GeckAuthoringReviewState verification,
        string message,
        string? planSha,
        IEnumerable<GeckAuthoringReviewOutput> outputs,
        IEnumerable<GeckAuthoringReviewDiagnostic> diagnostics,
        GeckAuthoringReviewState subjectHandoff = GeckAuthoringReviewState.Locked) =>
        new(root, observations, plan, observer, observation, verification, message, planSha,
            outputs.GroupBy(output => output.Path, StringComparer.OrdinalIgnoreCase).Select(group => group.Last()).OrderBy(output => output.Path, StringComparer.Ordinal).ToArray(),
            diagnostics.GroupBy(issue => string.Join('|', issue.RuleId, issue.Message, issue.File), StringComparer.Ordinal).Select(group => group.First()).ToArray())
        { SubjectHandoffState = subjectHandoff };

    private static GeckAuthoringReviewOperationResult ToOperation(BackendResult result, string? token) =>
        new(result.Success, result.Cancelled, result.Message, token, result.PlanSha256, result.Outputs, result.Diagnostics);

    private static GeckAuthoringReviewOperationResult Failed(string message, IReadOnlyList<GeckAuthoringReviewDiagnostic> diagnostics) =>
        new(false, false, message, null, null, [], diagnostics);

    private static string NormalizeProjectRoot(string projectRoot)
    {
        if (string.IsNullOrWhiteSpace(projectRoot)) throw new ArgumentException("Project root is required.", nameof(projectRoot));
        return Path.GetFullPath(projectRoot.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool FileMatches(string root, string relative, string sha, out GeckAuthoringReviewOutput? output)
    {
        var full = ResolveContained(root, relative);
        if (!File.Exists(full)) { output = null; return false; }
        var info = new FileInfo(full);
        output = new(relative, full, sha, info.Length, FileMatches(full, sha, info.Length));
        return output.Current;
    }

    private static bool FileMatches(string path, string sha, long length)
    {
        try
        {
            var info = new FileInfo(path);
            return info.Exists && !info.Attributes.HasFlag(FileAttributes.ReparsePoint) && info.Length == length && StringComparer.Ordinal.Equals(Sha(File.ReadAllBytes(path)), sha);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or CryptographicException)
        {
            return false;
        }
    }

    private static string ResolveContained(string root, string relative)
    {
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!ContainedBy(root, path)) throw new InvalidOperationException("Backend output escaped the selected project.");
        return path;
    }

    private static bool ContainedBy(string root, string path) =>
        path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static bool SamePath(string left, string right) => StringComparer.OrdinalIgnoreCase.Equals(
        Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    private static string TargetName(GeckAuthoringReviewTarget target) => target switch
    {
        GeckAuthoringReviewTarget.Plan => PlanTarget,
        GeckAuthoringReviewTarget.SubjectHandoff => SubjectHandoffTarget,
        GeckAuthoringReviewTarget.Observer => ObserverTarget,
        GeckAuthoringReviewTarget.Verification => VerificationTarget,
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
    };

    private static string Text(JsonObject value, string property) =>
        value[property]?.GetValue<string>() ?? throw new InvalidOperationException($"Backend field '{property}' is missing.");

    private static string? TextOrNull(JsonObject value, string property) => value[property]?.GetValue<string>();
    private static bool Boolean(JsonObject value, string property) => value[property]?.GetValue<bool>() ?? throw new InvalidOperationException($"Backend safety field '{property}' is missing.");
    private static string NormalizeRelative(string value) => value.Replace('\\', '/');
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string ErrorFallback(ForgeCommandResult result) => string.IsNullOrWhiteSpace(result.StandardError) ? $"Backend exited with code {result.ExitCode}." : result.StandardError.Trim();
    private static GeckAuthoringReviewDiagnostic BridgeIssue(string message) => new("WF-LOAD-DESKTOP", "error", "desktop bridge", "GECK authoring evidence unavailable", message, string.Empty);

    private sealed record ObservationEvidence(string RelativePath, long Length, string Sha256);

    private sealed record BackendResult(
        bool Success,
        bool Cancelled,
        string Message,
        string ToolVersion,
        string? PlanSha256,
        string SafetyFingerprint,
        IReadOnlyList<GeckAuthoringReviewOutput> Outputs,
        IReadOnlyList<GeckAuthoringReviewDiagnostic> Diagnostics,
        string Target,
        string ProjectRoot,
        bool DryRun)
    {
        public static BackendResult Failed(string message, IReadOnlyList<GeckAuthoringReviewDiagnostic> diagnostics) =>
            new(false, false, message, string.Empty, null, string.Empty, [], diagnostics, string.Empty, string.Empty, false);

        public static BackendResult CancelledResult() =>
            new(false, true, "Operation cancelled.", string.Empty, null, string.Empty, [], [], string.Empty, string.Empty, false);
    }
}
