using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal interface IGeckIntentBuilderCommandRunner
{
    string ApprovalIdentity { get; }
    Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments);
}

internal sealed class ForgeGeckIntentBuilderCommandRunner(ForgeCommandRunner runner) : IGeckIntentBuilderCommandRunner
{
    public string ApprovalIdentity => runner.ApprovalIdentity;

    public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) =>
        runner.RunInWorkingDirectoryAsync(workingDirectory, cancellationToken, arguments);
}

internal enum GeckIntentBuilderState
{
    NotLoaded,
    Blocked,
    Editing,
    PreviewReady,
    Applying,
    SavedProvisional,
    SavedBlocked,
    ReadyForReview,
    RefreshRequired
}

internal sealed record GeckIntentProviderInput(string Role, string EvidencePath, bool Attested);

internal sealed record GeckIntentResolutionInput(
    string Id,
    string Kind,
    string EditorId,
    string FormId,
    string Signature,
    string Status,
    string EvidencePath,
    int Quantity);

internal sealed record GeckIntentBuilderInput(
    string ProjectRoot,
    string PluginFileName,
    string Author,
    string Summary,
    string EnvironmentMode,
    string OutputRoot,
    IReadOnlyList<GeckIntentProviderInput> Providers,
    IReadOnlyList<GeckIntentResolutionInput> Resolutions,
    string ContainerEditorId,
    string ContainerStrategy,
    string ReferenceEditorId,
    string PositionX,
    string PositionY,
    string PositionZ,
    string RotationX,
    string RotationY,
    string RotationZ,
    bool Persistent,
    string EncounterZonePolicy);

internal sealed record GeckIntentEvidenceAttachment(
    string SourcePath,
    string ProjectPath,
    string DestinationPath,
    long Length,
    string Sha256,
    bool CopyRequired,
    byte[] Bytes);

internal sealed record GeckIntentBuilderPreview(
    bool Success,
    string Message,
    GeckIntentBuilderState State,
    string? Token,
    string Operation,
    string ProjectRoot,
    string ManifestPath,
    string ManifestVersionBefore,
    string IntentPath,
    string? ManifestJson,
    string? IntentJson,
    string? IntentSha256,
    long IntentLength,
    IReadOnlyList<GeckIntentEvidenceAttachment> Evidence,
    IReadOnlyList<GeckIntentCanonicalWrite> Writes,
    bool HasProvisional,
    IReadOnlyList<GeckAuthoringReviewDiagnostic> Diagnostics);

internal sealed record GeckIntentBuilderResult(
    bool Success,
    bool Cancelled,
    GeckIntentBuilderState State,
    string Message,
    IReadOnlyList<GeckAuthoringReviewDiagnostic> Diagnostics,
    GeckIntentBuilderPreview? Preview = null);

internal sealed record GeckIntentBuilderLoadResult(
    bool Success,
    string Message,
    string Operation,
    GeckIntentBuilderState State,
    GeckIntentBuilderInput? Input,
    string? IntentPath);

internal sealed class GeckIntentBuilderWorkspace
{
    private const int MaxEvidenceBytes = 4 * 1024 * 1024;
    private const string DefaultIntentPath = "src/registries/geck-authoring/main.json";
    private static readonly string[] ManifestVersions = ["0.1.0", "0.2.0", "0.3.0", "0.4.0", "0.5.0"];
    private static readonly string[] RequiredProviderRoles = ["geck", "authoring-provider", "xedit-verifier"];
    private static readonly string[] OptionalProviderRoles = ["xnvse", "geck-extender"];
    private static readonly HashSet<string> EvidenceExtensions = new(StringComparer.OrdinalIgnoreCase) { ".json", ".txt", ".log", ".csv", ".tsv" };
    private static readonly Regex LogicalId = new("^[a-z][a-z0-9]*(?:[._-][a-z0-9]+)+$", RegexOptions.CultureInvariant);
    private static readonly Regex Signature = new("^[A-Z0-9_]{4}$", RegexOptions.CultureInvariant);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly Lazy<JsonSchema> ManifestSchema = new(() => LoadSchema(WastelandForgeSchemaIds.Manifest050));
    private static readonly Lazy<JsonSchema> IntentSchema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringIntent010));

    private readonly IGeckIntentBuilderCommandRunner runner;
    private readonly GeckIntentBuilderJournal journal;
    private readonly GeckAuthoringReviewWorkspace reviewWorkspace;

    public GeckIntentBuilderWorkspace(IGeckIntentBuilderCommandRunner runner, GeckIntentBuilderJournal? journal = null)
    {
        this.runner = runner;
        this.journal = journal ?? new GeckIntentBuilderJournal();
        reviewWorkspace = new GeckAuthoringReviewWorkspace(new ReviewRunnerAdapter(runner));
    }

    public GeckIntentBuilderLoadResult Load(string projectRoot)
    {
        try
        {
            var root = NormalizeProjectRoot(projectRoot);
            EnsureSafeProjectRoot(root);
            var recovery = journal.ReviewRecovery(root);
            if (recovery.Success)
                return new(false, recovery.Message, "recovery", GeckIntentBuilderState.Blocked, null, null);

            var validation = new ProjectValidationPipeline().Validate(root);
            if (validation.HasErrors)
                return new(false, FirstIssue(validation, "Project validation blocks GECK intent authoring."), "blocked", GeckIntentBuilderState.Blocked, null, null);

            var manifestPath = Path.Combine(root, "wastelandforge.json");
            var manifest = ParseObject(manifestPath, "Project manifest");
            var declared = manifest["registries"]?["geckAuthoringIntent"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(declared))
                return new(true, "Project ready for a new GECK authoring intent.", "create", GeckIntentBuilderState.Editing, EmptyInput(root), DefaultIntentPath);

            var intentPath = ResolveRegisteredIntent(root, declared);
            var intent = ParseObject(intentPath, "GECK authoring intent");
            return new(true, "Existing GECK authoring intent loaded for revision.", "revise", GeckIntentBuilderState.Editing, FromIntent(root, intent), Relative(root, intentPath));
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(false, exception.Message, "blocked", GeckIntentBuilderState.Blocked, null, null);
        }
    }

    public GeckIntentBuilderPreview Preview(GeckIntentBuilderInput input)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(input);
            var root = NormalizeProjectRoot(input.ProjectRoot);
            EnsureSafeProjectRoot(root);
            var recovery = journal.ReviewRecovery(root);
            if (recovery.Success) return FailedPreview(recovery.Message, root);

            var validation = new ProjectValidationPipeline().Validate(root);
            if (validation.HasErrors) return FailedPreview(FirstIssue(validation, "Project validation blocks GECK intent authoring."), root, Diagnostics(validation));

            var manifestPath = Path.Combine(root, "wastelandforge.json");
            EnsureRegularFile(manifestPath, "Project manifest");
            var manifestBytes = File.ReadAllBytes(manifestPath);
            var manifest = ParseObject(manifestBytes, "Project manifest");
            var version = Text(manifest, "schemaVersion");
            if (!ManifestVersions.Contains(version, StringComparer.Ordinal)) throw new InvalidOperationException("Only registered JSON manifest versions 0.1.0 through 0.5.0 are supported.");
            var registries = manifest["registries"]?.AsObject() ?? throw new InvalidOperationException("Project manifest registries are missing.");
            var declared = registries["geckAuthoringIntent"]?.GetValue<string>();
            var operation = string.IsNullOrWhiteSpace(declared) ? "create" : "revise";
            var intentRelative = operation == "create" ? DefaultIntentPath : Relative(root, ResolveRegisteredIntent(root, declared!));
            var intentPath = ResolveContained(root, intentRelative);
            if (operation == "create" && File.Exists(intentPath))
                throw new InvalidOperationException("The default GECK authoring intent path is already occupied but is not registered in the manifest.");
            var beforeIntent = operation == "revise" ? File.ReadAllBytes(intentPath) : null;

            ValidateEnvironment(input);
            var evidence = new List<GeckIntentEvidenceAttachment>();
            var providers = CreateProviders(root, input.Providers, evidence);
            var resolutions = CreateResolutions(root, input.Resolutions, evidence, out var itemRows, out var placement, out var containerBase);
            var projectId = Text(manifest, "id");
            var intentId = projectId + ".authoring";
            var containerId = projectId + ".container";
            var referenceId = projectId + ".reference";
            ValidateLogicalId(intentId, "Derived intent ID");
            ValidateLogicalId(containerId, "Derived container ID");
            ValidateLogicalId(referenceId, "Derived reference ID");

            var pluginFileName = input.PluginFileName.Trim();
            if (pluginFileName.Length == 0 || Path.GetFileName(pluginFileName) != pluginFileName || !pluginFileName.EndsWith(".esp", StringComparison.Ordinal))
                throw new InvalidOperationException("Plugin filename must be one filename ending in .esp.");
            var author = Required(input.Author, "Plugin author");
            var summary = Required(input.Summary, "Plugin summary");
            var containerEditorId = Required(input.ContainerEditorId, "Container EditorID");
            var referenceEditorId = Required(input.ReferenceEditorId, "Reference EditorID");
            if (input.ContainerStrategy is not ("new" or "clone-approved")) throw new InvalidOperationException("Container strategy must be new or clone-approved.");
            if (input.EncounterZonePolicy is not ("inherit-cell" or "none")) throw new InvalidOperationException("Encounter policy must be inherit-cell or none.");

            var intent = new JsonObject
            {
                ["schemaVersion"] = "0.1.0",
                ["kind"] = "geck-authoring-intent",
                ["id"] = intentId,
                ["plugin"] = new JsonObject
                {
                    ["fileName"] = pluginFileName,
                    ["author"] = author,
                    ["summary"] = summary,
                    ["masters"] = new JsonArray("FalloutNV.esm")
                },
                ["environment"] = new JsonObject
                {
                    ["mode"] = "physical-data",
                    ["outputRoot"] = Path.GetFullPath(input.OutputRoot.Trim()),
                    ["providers"] = providers
                },
                ["resolutions"] = resolutions,
                ["container"] = new JsonObject
                {
                    ["id"] = containerId,
                    ["editorId"] = containerEditorId,
                    ["strategy"] = input.ContainerStrategy,
                    ["respawns"] = false,
                    ["items"] = new JsonArray(itemRows.Select(row => new JsonObject
                    {
                        ["resolutionId"] = row.Id,
                        ["quantity"] = row.Quantity
                    }).ToArray())
                },
                ["reference"] = new JsonObject
                {
                    ["id"] = referenceId,
                    ["editorId"] = referenceEditorId,
                    ["cellResolutionId"] = placement.Id,
                    ["baseContainerId"] = containerId,
                    ["position"] = Vector(input.PositionX, input.PositionY, input.PositionZ, "Position"),
                    ["rotation"] = Vector(input.RotationX, input.RotationY, input.RotationZ, "Rotation"),
                    ["ownership"] = "unowned",
                    ["persistent"] = input.Persistent,
                    ["encounterZonePolicy"] = input.EncounterZonePolicy
                }
            };
            _ = containerBase;

            var candidateManifest = manifest.DeepClone().AsObject();
            candidateManifest["schemaVersion"] = "0.5.0";
            candidateManifest["registries"]!.AsObject()["geckAuthoringIntent"] = intentRelative;
            var manifestJson = candidateManifest.ToJsonString(JsonOptions) + Environment.NewLine;
            var intentJson = intent.ToJsonString(JsonOptions) + Environment.NewLine;
            ValidateSchema(ManifestSchema.Value, manifestJson, "Candidate manifest does not satisfy manifest/0.5.0.");
            ValidateSchema(IntentSchema.Value, intentJson, "Candidate intent does not satisfy geck-authoring-intent/0.1.0.");

            var writes = evidence.Where(item => item.CopyRequired)
                .GroupBy(item => item.ProjectPath, StringComparer.Ordinal)
                .Select(group => new GeckIntentCanonicalWrite(group.Key, group.First().Bytes))
                .ToList();
            AddChangedWrite(writes, root, intentRelative, Encoding.UTF8.GetBytes(intentJson));
            AddChangedWrite(writes, root, "wastelandforge.json", Encoding.UTF8.GetBytes(manifestJson));
            if (writes.Count == 0) throw new InvalidOperationException("The current GECK authoring intent already matches these inputs.");

            var hasProvisional = input.Resolutions.Any(row => row.Status == "provisional");
            var token = CreateToken(root, operation, manifestBytes, beforeIntent, runner.ApprovalIdentity, input, evidence, writes);
            var intentBytes = Encoding.UTF8.GetBytes(intentJson);
            return new(
                true,
                "GECK intent preview ready. No files were written.",
                GeckIntentBuilderState.PreviewReady,
                token,
                operation,
                root,
                "wastelandforge.json",
                version,
                intentRelative,
                manifestJson,
                intentJson,
                Hash(intentBytes),
                intentBytes.LongLength,
                evidence.OrderBy(item => item.ProjectPath, StringComparer.Ordinal).ToArray(),
                writes,
                hasProvisional,
                []);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return FailedPreview(exception.Message, SafeRoot(input?.ProjectRoot));
        }
    }

    public async Task<GeckIntentBuilderResult> ApplyAsync(GeckIntentBuilderInput input, string previewToken, CancellationToken cancellationToken)
    {
        var preview = Preview(input);
        if (!preview.Success || preview.Token is null)
            return FailedResult(string.IsNullOrWhiteSpace(previewToken) ? preview.Message : "Inputs changed. Preview again.", preview.Diagnostics, preview);
        if (!StringComparer.Ordinal.Equals(preview.Token, previewToken)) return FailedResult("Inputs changed. Preview again.", [], preview);
        if (cancellationToken.IsCancellationRequested) return CancelledResult();

        string? pending = null;
        var promoted = false;
        try
        {
            pending = journal.Begin(preview.ProjectRoot, preview.Writes);
            cancellationToken.ThrowIfCancellationRequested();
            journal.PromoteCandidate(pending);
            promoted = true;

            var backendValidation = await runner.RunAsync(
                preview.ProjectRoot,
                CancellationToken.None,
                "validate", preview.ProjectRoot, "--format", "json", "--no-input").ConfigureAwait(false);
            if (backendValidation.ExitCode != 0 || new ProjectValidationPipeline().Validate(preview.ProjectRoot).HasErrors)
                throw new InvalidOperationException("Canonical validation failed after the source transaction. " + CommandFailure(backendValidation));

            var plan = await reviewWorkspace.PreviewAsync(
                GeckAuthoringReviewTarget.Plan,
                preview.ProjectRoot,
                null,
                CancellationToken.None).ConfigureAwait(false);
            journal.Commit(pending);
            pending = null;

            if (plan.Success)
                return new(true, false, GeckIntentBuilderState.ReadyForReview, "Canonical GECK intent is resolved and ready for Authoring Review.", plan.Diagnostics, preview);
            if (preview.HasProvisional)
                return new(true, false, GeckIntentBuilderState.SavedProvisional, "Canonical GECK intent saved with provisional resolutions; local evidence is still required.", plan.Diagnostics, preview);
            return new(true, false, GeckIntentBuilderState.SavedBlocked, "Canonical GECK intent saved, but plan dry-run remains blocked.", plan.Diagnostics, preview);
        }
        catch (OperationCanceledException)
        {
            if (pending is not null) journal.DeletePending(pending);
            return CancelledResult();
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            var rollback = string.Empty;
            if (pending is not null)
            {
                try
                {
                    if (promoted) journal.RestoreOriginal(pending);
                    journal.DeletePending(pending);
                }
                catch (Exception rollbackException) when (IsExpected(rollbackException))
                {
                    rollback = " Rollback requires recovery: " + rollbackException.Message;
                }
            }
            return FailedResult(exception.Message + rollback);
        }
    }

    public GeckIntentRecoveryReview ReviewRecovery(string projectRoot) => journal.ReviewRecovery(projectRoot);

    public async Task<GeckIntentBuilderResult> ResumeValidationAsync(string projectRoot, string token, CancellationToken cancellationToken)
    {
        var review = journal.ReviewRecovery(projectRoot);
        if (!journal.RecoveryTokenMatches(review, token) || review.PendingPath is null || review.State != GeckIntentRecoveryFileState.Candidate)
            return FailedResult(review.Success ? "Recovery state changed; review recovery again." : review.Message);
        try
        {
            var root = NormalizeProjectRoot(projectRoot);
            var validation = await runner.RunAsync(root, cancellationToken, "validate", root, "--format", "json", "--no-input").ConfigureAwait(false);
            if (validation.ExitCode != 0) throw new InvalidOperationException("Canonical validation failed during recovery. " + CommandFailure(validation));
            var plan = await reviewWorkspace.PreviewAsync(GeckAuthoringReviewTarget.Plan, root, null, cancellationToken).ConfigureAwait(false);
            journal.Commit(review.PendingPath);
            var provisional = CurrentIntentHasProvisional(root);
            if (plan.Success) return new(true, false, GeckIntentBuilderState.ReadyForReview, "Recovered GECK intent is ready for Authoring Review.", plan.Diagnostics);
            return new(true, false, provisional ? GeckIntentBuilderState.SavedProvisional : GeckIntentBuilderState.SavedBlocked,
                provisional ? "Recovered GECK intent remains provisional." : "Recovered GECK intent plan remains blocked.", plan.Diagnostics);
        }
        catch (OperationCanceledException) { return CancelledResult(); }
        catch (Exception exception) when (IsExpected(exception)) { return FailedResult(exception.Message); }
    }

    public GeckIntentBuilderResult RestoreOriginal(string projectRoot, string token)
    {
        var review = journal.ReviewRecovery(projectRoot);
        if (!journal.RecoveryTokenMatches(review, token) || review.PendingPath is null || review.State == GeckIntentRecoveryFileState.Unknown)
            return FailedResult(review.Success ? "Recovery state changed; review recovery again." : review.Message);
        try
        {
            journal.RestoreOriginal(review.PendingPath);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors) throw new InvalidOperationException("Restored original project did not validate.");
            journal.DeletePending(review.PendingPath);
            return new(true, false, GeckIntentBuilderState.Editing, "Original canonical source restored.", []);
        }
        catch (Exception exception) when (IsExpected(exception)) { return FailedResult(exception.Message); }
    }

    public GeckIntentUndoReview ReviewUndo(string projectRoot) => journal.ReviewUndo(projectRoot);

    public async Task<GeckIntentBuilderResult> UndoAsync(string projectRoot, string token, CancellationToken cancellationToken)
    {
        try
        {
            var root = NormalizeProjectRoot(projectRoot);
            journal.RestoreUndo(root, token);
            var validation = await runner.RunAsync(root, cancellationToken, "validate", root, "--format", "json", "--no-input").ConfigureAwait(false);
            if (validation.ExitCode != 0 || new ProjectValidationPipeline().Validate(root).HasErrors)
            {
                journal.RestoreCommittedCandidate(root);
                return FailedResult("Undo validation failed; post-change source was restored. " + CommandFailure(validation));
            }
            journal.CompleteUndo(root);
            return new(true, false, GeckIntentBuilderState.Editing, "Last GECK intent source change was undone and validated.", []);
        }
        catch (OperationCanceledException)
        {
            try { journal.RestoreCommittedCandidate(projectRoot); } catch { }
            return CancelledResult();
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            try { journal.RestoreCommittedCandidate(projectRoot); } catch { }
            return FailedResult(exception.Message);
        }
    }

    private static JsonArray CreateProviders(string root, IReadOnlyList<GeckIntentProviderInput> rows, List<GeckIntentEvidenceAttachment> evidence)
    {
        var allowed = RequiredProviderRoles.Concat(OptionalProviderRoles).ToHashSet(StringComparer.Ordinal);
        if (rows.Any(row => !allowed.Contains(row.Role))) throw new InvalidOperationException("Provider evidence contains an unsupported role.");
        if (rows.GroupBy(row => row.Role, StringComparer.Ordinal).Any(group => group.Count() != 1)) throw new InvalidOperationException("Provider evidence roles must be unique.");
        foreach (var role in RequiredProviderRoles)
            if (!rows.Any(row => row.Role == role)) throw new InvalidOperationException("Provider evidence is required for role: " + role);
        var array = new JsonArray();
        foreach (var row in rows.OrderBy(row => Array.IndexOf(RequiredProviderRoles.Concat(OptionalProviderRoles).ToArray(), row.Role)))
        {
            if (!row.Attested) throw new InvalidOperationException("Local evidence attestation is required for provider role: " + row.Role);
            var attachment = InspectEvidence(root, row.EvidencePath);
            evidence.Add(attachment);
            array.Add(new JsonObject
            {
                ["role"] = row.Role,
                ["path"] = attachment.ProjectPath,
                ["length"] = attachment.Length,
                ["sha256"] = attachment.Sha256,
                ["status"] = "local-verified"
            });
        }
        return array;
    }

    private static JsonArray CreateResolutions(
        string root,
        IReadOnlyList<GeckIntentResolutionInput> rows,
        List<GeckIntentEvidenceAttachment> evidence,
        out GeckIntentResolutionInput[] items,
        out GeckIntentResolutionInput placement,
        out GeckIntentResolutionInput containerBase)
    {
        items = rows.Where(row => row.Kind == "item").ToArray();
        var placements = rows.Where(row => row.Kind is "cell" or "worldspace").ToArray();
        var bases = rows.Where(row => row.Kind == "container-base").ToArray();
        if (items.Length < 2) throw new InvalidOperationException("The first slice requires at least two item resolutions.");
        if (placements.Length != 1) throw new InvalidOperationException("The first slice requires exactly one cell or worldspace resolution.");
        if (bases.Length != 1) throw new InvalidOperationException("The first slice requires exactly one container-base resolution.");
        if (rows.Count != items.Length + placements.Length + bases.Length) throw new InvalidOperationException("The first slice contains an unsupported resolution kind.");
        if (rows.GroupBy(row => row.Id.Trim(), StringComparer.Ordinal).Any(group => group.Count() != 1)) throw new InvalidOperationException("Resolution IDs must be unique.");
        placement = placements[0];
        containerBase = bases[0];
        var array = new JsonArray();
        foreach (var row in rows)
        {
            var id = row.Id.Trim();
            ValidateLogicalId(id, "Resolution ID");
            if (row.Kind == "item" && row.Quantity < 1) throw new InvalidOperationException("Every item quantity must be at least one.");
            if (row.Status is not ("local-verified" or "provisional")) throw new InvalidOperationException("Resolution status must be local-verified or provisional.");
            var attachment = InspectEvidence(root, row.EvidencePath);
            evidence.Add(attachment);
            array.Add(new JsonObject
            {
                ["id"] = id,
                ["kind"] = row.Kind,
                ["editorId"] = Required(row.EditorId, "Resolution EditorID"),
                ["formId"] = NormalizeFormId(row.FormId),
                ["signature"] = NormalizeSignature(row.Signature),
                ["status"] = row.Status,
                ["evidence"] = new JsonObject
                {
                    ["path"] = attachment.ProjectPath,
                    ["length"] = attachment.Length,
                    ["sha256"] = attachment.Sha256
                }
            });
        }
        return array;
    }

    private static GeckIntentEvidenceAttachment InspectEvidence(string root, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Every provider and resolution requires an evidence document.");
        var input = value.Trim();
        var source = Path.IsPathFullyQualified(input)
            ? Path.GetFullPath(input)
            : Path.GetFullPath(Path.Combine(root, input.Replace('/', Path.DirectorySeparatorChar)));
        EnsureRegularFile(source, "Evidence document");
        var extension = Path.GetExtension(source).ToLowerInvariant();
        if (!EvidenceExtensions.Contains(extension)) throw new InvalidOperationException("Evidence documents must be JSON, TXT, LOG, CSV, or TSV text.");
        var bytes = File.ReadAllBytes(source);
        if (bytes.Length is < 1 or > MaxEvidenceBytes) throw new InvalidOperationException("Evidence documents must contain 1 byte through 4 MiB.");
        string text;
        try { text = StrictUtf8.GetString(bytes); }
        catch (DecoderFallbackException exception) { throw new InvalidOperationException("Evidence documents must be valid UTF-8 text.", exception); }
        if (text.Contains('\0', StringComparison.Ordinal)) throw new InvalidOperationException("Evidence documents must not contain NUL characters.");
        if (extension == ".json") using (JsonDocument.Parse(text)) { }
        var sha = Hash(bytes);
        if (ContainedBy(root, source))
            return new(source, Relative(root, source), source, bytes.LongLength, sha, false, bytes);

        var relative = $"evidence/geck-authoring/{sha}{extension}";
        var destination = ResolveContained(root, relative);
        RefuseExistingReparseComponents(Path.GetDirectoryName(destination)!);
        var copy = true;
        if (File.Exists(destination))
        {
            EnsureRegularFile(destination, "Evidence destination");
            var current = File.ReadAllBytes(destination);
            if (current.LongLength != bytes.LongLength || Hash(current) != sha) throw new InvalidOperationException("The content-addressed evidence destination is occupied by different bytes.");
            copy = false;
        }
        return new(source, relative, destination, bytes.LongLength, sha, copy, bytes);
    }

    private static void ValidateEnvironment(GeckIntentBuilderInput input)
    {
        if (input.EnvironmentMode != "physical-data") throw new InvalidOperationException("Gate 541 supports physical-data only; MO2 routing remains deferred.");
        if (string.IsNullOrWhiteSpace(input.OutputRoot) || !Path.IsPathFullyQualified(input.OutputRoot.Trim())) throw new InvalidOperationException("Choose an existing absolute Fallout New Vegas Data directory.");
        var output = Path.GetFullPath(input.OutputRoot.Trim());
        if (!Directory.Exists(output) || !StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(output.TrimEnd(Path.DirectorySeparatorChar)), "Data"))
            throw new InvalidOperationException("Output root must be an existing directory named Data.");
        RefuseExistingReparseComponents(output);
    }

    private static GeckIntentBuilderInput EmptyInput(string root) => new(
        root, string.Empty, string.Empty, string.Empty, "physical-data", string.Empty,
        RequiredProviderRoles.Select(role => new GeckIntentProviderInput(role, string.Empty, false)).ToArray(),
        [
            new("", "item", "", "", "", "provisional", "", 1),
            new("", "item", "", "", "", "provisional", "", 1),
            new("", "cell", "", "", "", "provisional", "", 0),
            new("", "container-base", "", "", "", "provisional", "", 0)
        ],
        string.Empty, "new", string.Empty, "0", "0", "0", "0", "0", "0", false, "inherit-cell");

    private static GeckIntentBuilderInput FromIntent(string root, JsonObject intent)
    {
        var quantities = intent["container"]?["items"]?.AsArray().OfType<JsonObject>()
            .ToDictionary(item => Text(item, "resolutionId"), item => item["quantity"]?.GetValue<int>() ?? 0, StringComparer.Ordinal)
            ?? new Dictionary<string, int>(StringComparer.Ordinal);
        var providers = intent["environment"]?["providers"]?.AsArray().OfType<JsonObject>()
            .Select(item => new GeckIntentProviderInput(Text(item, "role"), Text(item, "path"), true)).ToArray() ?? [];
        var resolutions = intent["resolutions"]?.AsArray().OfType<JsonObject>().Select(item =>
        {
            var id = Text(item, "id");
            return new GeckIntentResolutionInput(id, Text(item, "kind"), Text(item, "editorId"), Text(item, "formId"), Text(item, "signature"), Text(item, "status"), Text(item["evidence"]!.AsObject(), "path"), quantities.GetValueOrDefault(id));
        }).ToArray() ?? [];
        var reference = intent["reference"]?.AsObject() ?? throw new InvalidOperationException("Existing intent reference is missing.");
        var position = reference["position"]?.AsObject() ?? throw new InvalidOperationException("Existing position is missing.");
        var rotation = reference["rotation"]?.AsObject() ?? throw new InvalidOperationException("Existing rotation is missing.");
        return new(
            root,
            Text(intent["plugin"]!.AsObject(), "fileName"),
            Text(intent["plugin"]!.AsObject(), "author"),
            Text(intent["plugin"]!.AsObject(), "summary"),
            Text(intent["environment"]!.AsObject(), "mode"),
            Text(intent["environment"]!.AsObject(), "outputRoot"),
            providers,
            resolutions,
            Text(intent["container"]!.AsObject(), "editorId"),
            Text(intent["container"]!.AsObject(), "strategy"),
            Text(reference, "editorId"),
            Number(position, "x"), Number(position, "y"), Number(position, "z"),
            Number(rotation, "x"), Number(rotation, "y"), Number(rotation, "z"),
            reference["persistent"]?.GetValue<bool>() ?? false,
            Text(reference, "encounterZonePolicy"));
    }

    private static string CreateToken(
        string root,
        string operation,
        byte[] manifest,
        byte[]? intent,
        string backendIdentity,
        GeckIntentBuilderInput input,
        IReadOnlyList<GeckIntentEvidenceAttachment> evidence,
        IReadOnlyList<GeckIntentCanonicalWrite> writes)
    {
        var payload = new StringBuilder()
            .AppendLine(root)
            .AppendLine(operation)
            .AppendLine(Hash(manifest))
            .AppendLine(Hash(intent))
            .AppendLine(backendIdentity)
            .AppendLine(JsonSerializer.Serialize(input));
        foreach (var item in evidence.OrderBy(item => item.SourcePath, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.ProjectPath, StringComparer.Ordinal))
            payload.Append(item.SourcePath).Append('|').Append(item.ProjectPath).Append('|').Append(item.Length).Append('|').AppendLine(item.Sha256);
        foreach (var write in writes)
            payload.Append(write.RelativePath).Append('|').Append(write.AfterBytes.LongLength).Append('|').AppendLine(Hash(write.AfterBytes));
        payload.AppendLine("executesExternalTools=false").AppendLine("writesPluginBytes=false").AppendLine("writesGameData=false");
        return Hash(Encoding.UTF8.GetBytes(payload.ToString()));
    }

    private static bool CurrentIntentHasProvisional(string root)
    {
        var manifest = ParseObject(Path.Combine(root, "wastelandforge.json"), "Project manifest");
        var declared = manifest["registries"]?["geckAuthoringIntent"]?.GetValue<string>() ?? throw new InvalidOperationException("GECK intent registration is missing.");
        var intent = ParseObject(ResolveRegisteredIntent(root, declared), "GECK authoring intent");
        return intent["resolutions"]!.AsArray().OfType<JsonObject>().Any(item => Text(item, "status") == "provisional");
    }

    private static void AddChangedWrite(List<GeckIntentCanonicalWrite> writes, string root, string relative, byte[] after)
    {
        var path = ResolveContained(root, relative);
        if (File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(after)) return;
        writes.Add(new(relative, after));
    }

    private static JsonObject Vector(string x, string y, string z, string label) => new()
    {
        ["x"] = ParseFinite(x, label + " X"),
        ["y"] = ParseFinite(y, label + " Y"),
        ["z"] = ParseFinite(z, label + " Z")
    };

    private static double ParseFinite(string value, string label) =>
        double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && double.IsFinite(parsed)
            ? parsed
            : throw new InvalidOperationException(label + " must be a finite number using '.' as the decimal separator.");

    private static string NormalizeFormId(string value)
    {
        var text = value.Trim();
        if (text.Length is < 1 or > 8 || !uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
            throw new InvalidOperationException("FormID must contain one through eight hexadecimal characters.");
        return parsed.ToString("X8", CultureInfo.InvariantCulture);
    }

    private static string NormalizeSignature(string value)
    {
        var signature = value.Trim().ToUpperInvariant();
        if (!Signature.IsMatch(signature)) throw new InvalidOperationException("Record signature must contain exactly four uppercase letters, numbers, or underscores.");
        return signature;
    }

    private static void ValidateLogicalId(string value, string label)
    {
        if (!LogicalId.IsMatch(value)) throw new InvalidOperationException(label + " is not a valid dotted lowercase logical ID.");
    }

    private static string ResolveRegisteredIntent(string root, string declared)
    {
        if (Path.IsPathRooted(declared)) throw new InvalidOperationException("GECK intent registration must be project-relative.");
        var path = ResolveContained(root, declared);
        if (Directory.Exists(path)) throw new InvalidOperationException("Directory-valued GECK intent registrations are not supported by the first builder slice.");
        EnsureRegularFile(path, "Registered GECK authoring intent");
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(path), ".json")) throw new InvalidOperationException("Registered GECK authoring intent must be one JSON file.");
        return path;
    }

    private static void EnsureSafeProjectRoot(string root)
    {
        if (!Directory.Exists(root)) throw new InvalidOperationException("Select an existing Forge project directory.");
        RefuseExistingReparseComponents(root);
        var manifest = Path.Combine(root, "wastelandforge.json");
        if (!File.Exists(manifest)) throw new InvalidOperationException("Selected project does not contain wastelandforge.json.");
    }

    private static void EnsureRegularFile(string path, string label)
    {
        if (!File.Exists(path)) throw new InvalidOperationException(label + " does not exist.");
        RefuseExistingReparseComponents(path);
        var info = new FileInfo(path);
        if (info.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new InvalidOperationException(label + " must not be a reparse point.");
    }

    private static void RefuseExistingReparseComponents(string path)
    {
        FileSystemInfo? current = File.Exists(path) ? new FileInfo(Path.GetFullPath(path)) : new DirectoryInfo(Path.GetFullPath(path));
        while (current is not null)
        {
            if (current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new InvalidOperationException("Selected path contains a reparse point: " + current.FullName);
            current = current switch { FileInfo file => file.Directory, DirectoryInfo directory => directory.Parent, _ => null };
        }
    }

    private static JsonObject ParseObject(string path, string label) => ParseObject(File.ReadAllBytes(path), label);
    private static JsonObject ParseObject(byte[] bytes, string label) => JsonNode.Parse(bytes)?.AsObject() ?? throw new InvalidOperationException(label + " is not a JSON object.");
    private static void ValidateSchema(JsonSchema schema, string json, string message) { using var document = JsonDocument.Parse(json); if (!schema.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException(message); }
    private static JsonSchema LoadSchema(string id) { if (!WastelandForgeSchemaCatalog.TryGetById(id, out var resource) || resource is null) throw new InvalidOperationException("Required schema is unavailable: " + id); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource), new BuildOptions { SchemaRegistry = new SchemaRegistry() }); }
    private static string NormalizeProjectRoot(string value) { if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Project root is required."); return Path.GetFullPath(value.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); }
    private static string SafeRoot(string? value) { try { return string.IsNullOrWhiteSpace(value) ? string.Empty : NormalizeProjectRoot(value); } catch { return string.Empty; } }
    private static string ResolveContained(string root, string relative) { if (Path.IsPathRooted(relative)) throw new InvalidOperationException("Canonical path must be project-relative."); var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar))); if (!ContainedBy(root, path)) throw new InvalidOperationException("Canonical path escaped the project root."); return path; }
    private static bool ContainedBy(string root, string path) => path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Required(string value, string label) => string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException(label + " is required.") : value.Trim();
    private static string Text(JsonObject value, string name) => value[name]?.GetValue<string>() ?? throw new InvalidOperationException("Missing text value: " + name);
    private static string Number(JsonObject value, string name) => (value[name]?.GetValue<double>() ?? throw new InvalidOperationException("Missing numeric value: " + name)).ToString("R", CultureInfo.InvariantCulture);
    private static string Hash(byte[]? bytes) => bytes is null ? "absent" : Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string CommandFailure(ForgeCommandResult result) => string.IsNullOrWhiteSpace(result.StandardOutput) ? result.StandardError.Trim() : result.StandardOutput.Trim();
    private static string FirstIssue(DiagnosticReport report, string fallback) => report.Issues.FirstOrDefault(issue => issue.Severity == DiagnosticSeverity.Error)?.Message ?? fallback;
    private static IReadOnlyList<GeckAuthoringReviewDiagnostic> Diagnostics(DiagnosticReport report) => report.Issues.Select(issue => new GeckAuthoringReviewDiagnostic(issue.RuleId.Value, issue.Severity.ToString().ToLowerInvariant(), issue.Category, issue.Title, issue.Message, issue.PrimaryLocation.File)).ToArray();
    private static bool IsExpected(Exception exception) => exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or NotSupportedException or PathTooLongException or CryptographicException or DecoderFallbackException;
    private static GeckIntentBuilderPreview FailedPreview(string message, string root, IReadOnlyList<GeckAuthoringReviewDiagnostic>? diagnostics = null) => new(false, message, GeckIntentBuilderState.Blocked, null, "blocked", root, "wastelandforge.json", string.Empty, string.Empty, null, null, null, 0, [], [], false, diagnostics ?? []);
    private static GeckIntentBuilderResult FailedResult(string message, IReadOnlyList<GeckAuthoringReviewDiagnostic>? diagnostics = null, GeckIntentBuilderPreview? preview = null) => new(false, false, GeckIntentBuilderState.Blocked, message, diagnostics ?? [], preview);
    private static GeckIntentBuilderResult CancelledResult() => new(false, true, GeckIntentBuilderState.Editing, "GECK intent operation cancelled before canonical promotion.", []);

    private sealed class ReviewRunnerAdapter(IGeckIntentBuilderCommandRunner inner) : IGeckAuthoringReviewCommandRunner
    {
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) => inner.RunAsync(workingDirectory, cancellationToken, arguments);
    }
}

internal sealed class GeckIntentProviderRow
{
    public string Role { get; set; } = string.Empty;
    public string EvidencePath { get; set; } = string.Empty;
    public bool Attested { get; set; }
}

internal sealed class GeckIntentResolutionRow
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = "item";
    public string EditorId { get; set; } = string.Empty;
    public string FormId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Status { get; set; } = "provisional";
    public string EvidencePath { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
