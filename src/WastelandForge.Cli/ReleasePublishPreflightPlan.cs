using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace WastelandForge.Cli;

internal sealed record ReleasePublishPreflightOptions(
    string ProjectRoot,
    bool DryRun,
    bool Yes,
    string? Confirm);

internal sealed record ReleasePublishEvidenceRequirement(
    string Id,
    string Title,
    string Source,
    string Status,
    bool Required,
    bool CheckedInCurrentGate);

internal sealed record ReleasePublishEvidenceArtifact(
    string Id,
    string Path,
    string Kind,
    string ContentKind,
    bool Required,
    bool Exists,
    string Status,
    long? Length,
    bool ShapeCheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    string? ShapeDetail);

internal sealed record ReleasePublishChecksumSidecar(
    string Path,
    bool Exists,
    string Status,
    bool EntryClassificationInCurrentGate,
    bool DigestRevalidationInCurrentGate,
    int ExpectedEntries,
    int ParsedEntries,
    int CoveredExpectedEntries,
    int MissingExpectedEntries,
    int UnexpectedEntries,
    int DuplicateEntries,
    int MalformedEntries,
    int DigestRevalidatedEntries,
    int DigestMatchedEntries,
    int DigestMismatchedEntries,
    int MissingLocalFileEntries,
    IReadOnlyList<ReleasePublishChecksumExpectedPath> ExpectedPaths,
    IReadOnlyList<ReleasePublishChecksumEntry> Entries);

internal sealed record ReleasePublishChecksumExpectedPath(
    string Path,
    string Status,
    bool EntryPresent,
    bool LocalFilePresent,
    bool DigestRevalidatedInCurrentGate,
    string? ExpectedSha256,
    string? ActualSha256);

internal sealed record ReleasePublishChecksumEntry(
    int LineNumber,
    string? Sha256,
    string? ActualSha256,
    string? Path,
    string Status,
    bool ExpectedPath,
    bool DuplicatePath,
    bool LocalFilePresent,
    bool DigestRevalidatedInCurrentGate,
    string ShapeDetail);

internal sealed record ReleasePublishBuildManifestCrossReference(
    string Path,
    bool Exists,
    string Status,
    bool OutputCrossReferenceInCurrentGate,
    bool ContentReadInCurrentGate,
    bool DigestRevalidationInCurrentGate,
    int ExpectedOutputCount,
    int ParsedOutputCount,
    int CoveredExpectedOutputCount,
    int MissingExpectedOutputCount,
    int MissingLocalArtifactOutputCount,
    int MissingChecksumEntryOutputCount,
    int UnexpectedOutputCount,
    int DuplicateOutputCount,
    int MalformedOutputCount,
    int DigestRevalidatedOutputCount,
    int DigestMatchedOutputCount,
    int DigestMismatchedOutputCount,
    IReadOnlyList<ReleasePublishBuildManifestExpectedOutput> ExpectedPaths,
    IReadOnlyList<ReleasePublishBuildManifestOutput> Outputs);

internal sealed record ReleasePublishBuildManifestExpectedOutput(
    string Path,
    string ChecksumPath,
    string Status,
    bool ManifestOutputPresent,
    bool LocalArtifactPresent,
    bool ChecksumEntryPresent,
    bool DigestRevalidatedInCurrentGate,
    string? ExpectedSha256,
    string? ActualSha256);

internal sealed record ReleasePublishBuildManifestOutput(
    int Index,
    string? Path,
    string? ChecksumPath,
    string? Sha256,
    string? ActualSha256,
    string Status,
    bool ExpectedPath,
    bool DuplicatePath,
    bool LocalArtifactPresent,
    bool ChecksumEntryPresent,
    bool DigestRevalidatedInCurrentGate,
    string ShapeDetail);

internal sealed record ReleasePublishArchiveEvidenceCrossReference(
    string Path,
    bool Exists,
    string Status,
    bool MetadataCrossReferenceInCurrentGate,
    bool ContentReadInCurrentGate,
    bool DigestRevalidationInCurrentGate,
    bool ArchiveRevalidationInCurrentGate,
    int ExpectedPathCount,
    int ParsedPathCount,
    int CoveredExpectedPathCount,
    int MissingExpectedPathCount,
    int MissingLocalArtifactPathCount,
    int MissingChecksumEntryPathCount,
    int MissingBuildManifestOutputPathCount,
    int UnexpectedPathCount,
    int MalformedPathCount,
    bool ArchivePathMatchesOutput,
    bool ArchiveSha256MetadataPresent,
    bool ArchiveLengthMetadataPresent,
    bool ArchiveSha256MatchesLocal,
    bool ArchiveLengthMatchesLocal,
    string? ExpectedArchiveSha256,
    string? ActualArchiveSha256,
    long? ExpectedArchiveLength,
    long? ActualArchiveLength,
    bool ArchiveOpenedInCurrentGate,
    int ExpectedArchiveEntryCount,
    int EvidenceActualArchiveEntryCount,
    int ActualArchiveEntryCount,
    bool ArchiveEntryCountMatchesMetadata,
    bool ArchiveEntryNamesMatchLocal,
    bool ArchiveEvidenceActualEntryNamesMatchLocal,
    bool ArchiveEntryOrderingMatchesLocal,
    bool ArchiveDeterministicTimestampsMatchLocal,
    bool ArchiveStoredCompressionMatchesLocal,
    string ArchiveRevalidationDetail,
    IReadOnlyList<ReleasePublishArchiveEntryRevalidation> ArchiveEntries,
    IReadOnlyList<ReleasePublishArchiveEvidenceExpectedPath> ExpectedPaths,
    IReadOnlyList<ReleasePublishArchiveEvidencePath> Paths);

internal sealed record ReleasePublishArchiveEntryRevalidation(
    int Index,
    string Path,
    long Length,
    long CompressedLength,
    string LastWriteTimeUtc,
    string Status,
    bool ExpectedPath,
    bool ExpectedAtIndex,
    bool EvidenceActualAtIndex,
    bool TimestampMatches,
    bool Stored);

internal sealed record ReleasePublishArchiveEvidenceExpectedPath(
    string Role,
    string Path,
    string ChecksumPath,
    string Status,
    bool EvidenceMetadataPresent,
    bool LocalArtifactPresent,
    bool ChecksumEntryPresent,
    bool BuildManifestOutputPresent);

internal sealed record ReleasePublishArchiveEvidencePath(
    string Role,
    string? Path,
    string? ChecksumPath,
    string Status,
    bool ExpectedPath,
    bool LocalArtifactPresent,
    bool ChecksumEntryPresent,
    bool BuildManifestOutputPresent,
    bool DigestRevalidatedInCurrentGate,
    bool ArchiveRevalidatedInCurrentGate,
    string ShapeDetail);

internal sealed record ReleasePublishSemanticEvidenceValidation(
    string Status,
    bool ValidationInCurrentGate,
    bool ContentReadInCurrentGate,
    int ExpectedChecks,
    int PassedChecks,
    int FailedChecks,
    int SkippedChecks,
    string Detail,
    IReadOnlyList<ReleasePublishSemanticEvidenceCheck> Checks);

internal sealed record ReleasePublishSemanticEvidenceCheck(
    string Id,
    string Title,
    string Path,
    string Status,
    string Detail);

internal sealed record ReleasePublishSchemaValidationEvidence(
    string Path,
    bool Exists,
    string Status,
    bool CheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    int Errors,
    int Warnings,
    int Notes,
    int IssueCount,
    int SchemaIssueCount,
    string Detail);

internal sealed record ReleasePublishCapabilityEnvironmentEvidence(
    string Path,
    bool Exists,
    string Status,
    bool CheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    bool ProjectScoped,
    bool RuntimeProbesEnabled,
    bool Mo2VfsEnabled,
    int Providers,
    int Capabilities,
    int MissingProviders,
    int UnknownProviders,
    int WrongScopeProviders,
    int MissingCapabilities,
    int UnknownCapabilities,
    int WrongScopeCapabilities,
    int Requirements,
    int RequiredUnavailable,
    int OptionalUnavailable,
    int Diagnostics,
    int CapabilityDiagnostics,
    int DoctorAreas,
    int DoctorActionNeededAreas,
    int DoctorUnknownAreas,
    string Detail);

internal sealed record ReleasePublishPackageValidationEvidence(
    string Path,
    bool Exists,
    string Status,
    bool CheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    string? Target,
    string? Mode,
    string? OutputRoot,
    bool DistScoped,
    bool PackageArchivePresent,
    int Errors,
    int Warnings,
    int Notes,
    int IssueCount,
    int PackageIssueCount,
    string Detail);

internal sealed record ReleasePublishReleaseVerificationEvidence(
    string Path,
    bool Exists,
    string Status,
    bool CheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    bool DryRun,
    string? OutputRoot,
    bool DistScoped,
    int Errors,
    int Warnings,
    int Notes,
    int IssueCount,
    int ReleaseIssueCount,
    string Detail);

internal sealed record ReleasePublishCollectionPlanEvidence(
    string Path,
    bool Exists,
    string Status,
    bool CheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    bool DryRun,
    string? OutputRoot,
    bool DistScoped,
    bool LinksEvidenceIndex,
    bool LinksEvidenceStatus,
    bool LinksActionChecklist,
    bool LinksHandoffSummary,
    int Steps,
    int ManualSteps,
    int AvailableSteps,
    int MalformedSteps,
    bool NoExecutionBoundary,
    string Detail);

internal sealed record ReleasePublishDryRunCrossLinkEvidence(
    string Status,
    bool CheckedInCurrentGate,
    bool ContentReadInCurrentGate,
    int ExpectedFiles,
    int PresentFiles,
    int MissingFiles,
    int MalformedFiles,
    int ExpectedLinks,
    int ValidLinks,
    int MismatchedLinks,
    int RequiredEvidenceEntries,
    int StatusEntries,
    int ActionItems,
    int CollectionSteps,
    bool IdentityConsistent,
    bool SummaryCountersConsistent,
    bool RequiredEvidenceConsistent,
    bool ActionReferencesConsistent,
    bool CollectionStepsConsistent,
    bool ExecutionBoundariesConsistent,
    bool HandoffReferencesConsistent,
    string Detail);

internal sealed record ReleasePublishDryRunEvidenceRemediation(
    string Status,
    bool CheckedInCurrentGate,
    bool RequiresOperatorAction,
    string SourceStatus,
    int ActionItems,
    int CommandHints,
    int AffectedPaths,
    int BlockingIssues,
    string RecommendedCommand,
    string Detail,
    IReadOnlyList<string> TargetPaths,
    IReadOnlyList<ReleasePublishDryRunEvidenceRemediationItem> Items);

internal sealed record ReleasePublishDryRunEvidenceRemediationItem(
    string Id,
    string Priority,
    string Category,
    string Status,
    string Reason,
    string CommandHint,
    string Execution,
    bool BlocksPublishReadiness,
    IReadOnlyList<string> TargetPaths);

internal sealed record ReleasePublishGovernanceCheck(
    string Id,
    string Title,
    string Status,
    bool Required,
    bool CheckedInCurrentGate,
    string EvidencePath,
    string Detail);

internal sealed record ReleasePublishApprovalRequirement(
    bool Required,
    bool Provided,
    bool YesProvided,
    string? ConfirmationValue,
    bool ConfirmationValidated,
    bool? ConfirmationMatches,
    bool ProjectManifestRead,
    string? ProjectManifestPath,
    string? ProjectId,
    string Status,
    string Description,
    string Detail);

internal sealed record ReleasePublishApprovalProjectIdReadResult(
    string? ProjectId,
    string? Status,
    string? Error);

internal sealed record ReleasePublishReadinessCheck(
    string Id,
    string Title,
    string Source,
    string Status,
    bool Required,
    bool Satisfied,
    string Detail);

internal sealed record ReleasePublishReadiness(
    bool EvaluatedInCurrentGate,
    string Status,
    bool LocalPreconditionsSatisfied,
    bool EvidenceSatisfied,
    bool GovernanceSatisfied,
    bool ApprovalSatisfied,
    bool PublishExecutionEnabled,
    bool ReadyForRealPublish,
    int RequiredChecks,
    int SatisfiedChecks,
    int BlockingChecks,
    string Detail,
    IReadOnlyList<string> BlockingCheckIds,
    IReadOnlyList<ReleasePublishReadinessCheck> Checks);

internal sealed record ReleasePublishLaneCloseout(
    bool ClosedInCurrentGate,
    string Status,
    string Lane,
    string CompletedThroughGate,
    string NextGate,
    string NextValueSlice,
    string Detail,
    IReadOnlyList<string> DeferredCapabilities);

internal sealed record ReleasePublishPreflightResult(
    string Status,
    string ProjectRoot,
    bool DryRun,
    bool PlanningOnly,
    bool PublishReady,
    string ReleasePrepareEvidenceRoot,
    string ReleasePrepareEvidenceStatus,
    int ExpectedEvidenceArtifacts,
    int PresentEvidenceArtifacts,
    int MissingEvidenceArtifacts,
    int WellFormedEvidenceArtifacts,
    int MalformedEvidenceArtifacts,
    int UnclassifiedEvidenceArtifacts,
    ReleasePublishChecksumSidecar ChecksumSidecar,
    ReleasePublishBuildManifestCrossReference BuildManifestCrossReference,
    ReleasePublishArchiveEvidenceCrossReference ArchiveEvidenceCrossReference,
    ReleasePublishSemanticEvidenceValidation SemanticEvidenceValidation,
    ReleasePublishSchemaValidationEvidence SchemaValidationEvidence,
    ReleasePublishCapabilityEnvironmentEvidence CapabilityEnvironmentEvidence,
    ReleasePublishPackageValidationEvidence PackageValidationEvidence,
    ReleasePublishReleaseVerificationEvidence ReleaseVerificationEvidence,
    ReleasePublishCollectionPlanEvidence CollectionPlanEvidence,
    ReleasePublishDryRunCrossLinkEvidence DryRunCrossLinkEvidence,
    ReleasePublishDryRunEvidenceRemediation DryRunEvidenceRemediation,
    ReleasePublishReadiness PublishReadiness,
    ReleasePublishLaneCloseout LaneCloseout,
    string? RefusalReason,
    IReadOnlyList<ReleasePublishEvidenceRequirement> RequiredEvidence,
    IReadOnlyList<ReleasePublishEvidenceArtifact> LocalEvidenceArtifacts,
    IReadOnlyList<ReleasePublishGovernanceCheck> GovernanceChecks,
    ReleasePublishApprovalRequirement Approval,
    IReadOnlyList<string> Boundaries)
{
    public bool IsRefused => StringComparer.Ordinal.Equals(Status, "refused");
}

internal static class ReleasePublishPreflightPlanner
{
    private sealed record ArchiveRevalidationResult(
        bool Opened,
        int ExpectedEntryCount,
        int EvidenceActualEntryCount,
        int ActualEntryCount,
        bool EntryCountMatchesMetadata,
        bool EntryNamesMatchLocal,
        bool EvidenceActualEntryNamesMatchLocal,
        bool EntryOrderingMatchesLocal,
        bool DeterministicTimestampsMatchLocal,
        bool StoredCompressionMatchesLocal,
        string Detail,
        IReadOnlyList<ReleasePublishArchiveEntryRevalidation> Entries);

    private sealed record ReleaseDryRunEvidenceJsonRead(
        string Path,
        bool Exists,
        bool ContentRead,
        bool Parsed,
        bool IdentityValid,
        JsonObject? Root,
        string Detail);

    private sealed record ReleaseDryRunEvidenceEntryShape(
        string Id,
        string Path,
        bool Exists,
        string Status);

    private sealed record ReleaseDryRunEvidenceActionShape(
        string Id,
        string EvidenceId,
        string TargetPath);

    private sealed record ReleaseDryRunEvidenceCollectionStepShape(
        int Order,
        string Id,
        string EvidenceId,
        string TargetPath,
        string Status,
        string CollectionMode,
        string Execution,
        string? ActionId);

    private sealed record ReleaseDryRunEvidenceSummaryShape(
        int Total,
        int Present,
        int Missing,
        int Actions,
        int? Steps,
        int? ManualSteps,
        int? AvailableSteps);

    private static readonly string[] BoundaryLines =
    [
        "Gate 288 closes the release publish no-publish lane and routes the next value slice.",
        "JSON evidence is parsed for well-formed shape only; checksum sidecar entries are parsed for expected-path coverage only.",
        "Build-manifest outputs and release-archive-evidence metadata are cross-referenced to local evidence paths, checksum sidecar paths, and existing build-manifest output paths only.",
        "Checksum sidecar digests are recomputed for expected local evidence files only.",
        "Build-manifest output digests are recomputed for expected local evidence files only.",
        "Release-archive-evidence archive sha256 and length metadata are recomputed for the expected local archive file only.",
        "The expected local release archive is reopened only to compare entry names, entry order, deterministic timestamps, and stored compression metadata.",
        "Release archive entries are inspected as metadata only; payload contents are not semantically validated.",
        "Semantic release-evidence validation checks local evidence kind/status contracts, output path maps, release-summary counters, archive-plan inputs, archive-evidence checks, build-manifest output sets, and no-publish execution boundaries.",
        "Governance checks read local policy, workflow, CODEOWNERS, fixture, AI-optional, and tool-version evidence only.",
        "Schema-validation evidence reads local dist/release-dry-run/validation.json only and checks the diagnostic report shape plus WF-SCHEMA issue count.",
        "Capability/environment evidence reads local dist/release-dry-run/capabilities-scan.json only and checks project-scoped capabilities scan shape plus WF-CAP issue count.",
        "Package-validation evidence reads local dist/release-dry-run/package-verify.json only and checks package verify-existing report shape plus WF-BUILD issue count.",
        "Release-verification evidence reads local dist/release-dry-run/release-verify.json only and checks release verify report shape plus WF-REL-* issue count.",
        "Collection-plan evidence reads local dist/release-dry-run/release-evidence-collection-plan.json only and checks ordered release-publish preflight evidence steps plus no-execution flags.",
        "Release dry-run cross-link evidence reads local release-evidence-index.json, release-evidence-status.json, release-evidence-actions.json, release-evidence-collection-plan.json, and release-evidence-handoff.md only.",
        "Explicit human approval reads only the root project manifest ID and requires --yes plus --confirm <project-id>.",
        "Publish readiness is an aggregate of local evidence, governance, and approval states only.",
        "Lane closeout is reported as local routing metadata only.",
        "No release is published.",
        "Remote repositories are not called.",
        "Release assets are not uploaded.",
        "Attestations and signing are not performed.",
        "External tools, plugin mutation, MO2 automation, GECK automation, runtime probes, and AI calls are not used."
    ];

    private static readonly string[] DeferredPublishCapabilities =
    [
        "remote-repository-calls",
        "release-asset-uploads",
        "attestation-signing",
        "external-tool-execution",
        "plugin-mutation",
        "mo2-automation",
        "geck-automation",
        "runtime-probes",
        "ai-behavior"
    ];

    private static readonly string[] ExpectedChecksumEntryPaths =
    [
        "archives/release.zip",
        "build-manifest.json",
        "release-archive-evidence.json",
        "release-archive-plan.json",
        "release-plan.json",
        "release-summary.json",
        "staging/release-payload.json"
    ];

    private static readonly string[] ExpectedBuildManifestOutputPaths =
    [
        "dist/release-prepare/archives/release.zip",
        "dist/release-prepare/release-archive-evidence.json",
        "dist/release-prepare/release-archive-plan.json",
        "dist/release-prepare/release-plan.json",
        "dist/release-prepare/release-summary.json",
        "dist/release-prepare/staging/release-payload.json"
    ];

    private static readonly (string Role, string Path)[] ExpectedArchiveEvidencePaths =
    [
        ("releaseArchive", "dist/release-prepare/archives/release.zip"),
        ("releaseArchiveEvidence", "dist/release-prepare/release-archive-evidence.json"),
        ("releaseArchivePlan", "dist/release-prepare/release-archive-plan.json"),
        ("releasePlan", "dist/release-prepare/release-plan.json"),
        ("releaseSummary", "dist/release-prepare/release-summary.json"),
        ("stagingPayload", "dist/release-prepare/staging/release-payload.json")
    ];

    private static readonly (string Key, string Path)[] ExpectedSemanticOutputPaths =
    [
        ("root", "dist/release-prepare"),
        ("stagingRoot", "dist/release-prepare/staging"),
        ("stagingPayload", "dist/release-prepare/staging/release-payload.json"),
        ("releaseArchivePlan", "dist/release-prepare/release-archive-plan.json"),
        ("releaseArchive", "dist/release-prepare/archives/release.zip"),
        ("plannedArchive", "dist/release-prepare/archives/release.zip"),
        ("releaseArchiveEvidence", "dist/release-prepare/release-archive-evidence.json"),
        ("releasePlan", "dist/release-prepare/release-plan.json"),
        ("releaseSummary", "dist/release-prepare/release-summary.json"),
        ("buildManifest", "dist/release-prepare/build-manifest.json"),
        ("checksums", "dist/release-prepare/checksums.sha256")
    ];

    private static readonly (string Kind, string Path)[] ExpectedReleasePlanOutputs =
    [
        ("staging-root", "dist/release-prepare/staging/"),
        ("staging-payload", "dist/release-prepare/staging/release-payload.json"),
        ("release-archive-plan", "dist/release-prepare/release-archive-plan.json"),
        ("release-archive", "dist/release-prepare/archives/release.zip"),
        ("release-archive-evidence", "dist/release-prepare/release-archive-evidence.json"),
        ("release-plan", "dist/release-prepare/release-plan.json"),
        ("release-summary", "dist/release-prepare/release-summary.json"),
        ("build-manifest", "dist/release-prepare/build-manifest.json"),
        ("checksums", "dist/release-prepare/checksums.sha256")
    ];

    private static readonly (string Kind, string Path)[] ExpectedArchivePlanInputs =
    [
        ("release-archive-plan", "dist/release-prepare/release-archive-plan.json"),
        ("release-plan", "dist/release-prepare/release-plan.json"),
        ("release-summary", "dist/release-prepare/release-summary.json"),
        ("staging-payload", "dist/release-prepare/staging/release-payload.json")
    ];

    private static readonly string[] ExpectedArchiveEntryPaths =
    [
        "release-archive-plan.json",
        "release-plan.json",
        "release-summary.json",
        "staging/release-payload.json"
    ];

    private static readonly (string Id, string Title, string Path)[] SemanticCheckTemplates =
    [
        ("release-plan-contract", "release-plan contract fields are consistent", "dist/release-prepare/release-plan.json"),
        ("release-summary-contract", "release-summary contract fields are consistent", "dist/release-prepare/release-summary.json"),
        ("staging-payload-contract", "staging payload contract fields are consistent", "dist/release-prepare/staging/release-payload.json"),
        ("release-archive-plan-contract", "release-archive-plan contract fields are consistent", "dist/release-prepare/release-archive-plan.json"),
        ("release-archive-evidence-contract", "release-archive-evidence contract fields are consistent", "dist/release-prepare/release-archive-evidence.json"),
        ("build-manifest-contract", "build-manifest contract fields are consistent", "dist/release-prepare/build-manifest.json"),
        ("project-output-map-consistency", "release evidence project roots and output maps are consistent", "dist/release-prepare"),
        ("release-plan-planned-output-consistency", "release-plan planned outputs match release-prepare evidence outputs", "dist/release-prepare/release-plan.json"),
        ("release-summary-counts", "release-summary counters match prepared release evidence", "dist/release-prepare/release-summary.json"),
        ("staging-payload-boundary", "staging payload remains skeleton-only and non-mutating", "dist/release-prepare/staging/release-payload.json"),
        ("release-archive-plan-metadata", "release-archive-plan metadata matches deterministic local archive policy", "dist/release-prepare/release-archive-plan.json"),
        ("release-archive-plan-inputs", "release-archive-plan inputs match deterministic archive entries", "dist/release-prepare/release-archive-plan.json"),
        ("release-archive-evidence-checks", "release-archive-evidence checks report deterministic archive metadata", "dist/release-prepare/release-archive-evidence.json"),
        ("build-manifest-output-set", "build-manifest output set matches release-prepare evidence outputs", "dist/release-prepare/build-manifest.json"),
        ("non-publish-execution-boundary", "release evidence execution flags remain no-publish and local-only", "dist/release-prepare")
    ];

    private const string SchemaValidationEvidenceRelativePath = "dist/release-dry-run/validation.json";
    private const string CapabilityEnvironmentEvidenceRelativePath = "dist/release-dry-run/capabilities-scan.json";
    private const string PackageValidationEvidenceRelativePath = "dist/release-dry-run/package-verify.json";
    private const string ReleaseVerificationEvidenceRelativePath = "dist/release-dry-run/release-verify.json";
    private const string ReleaseEvidenceIndexRelativePath = "dist/release-dry-run/release-evidence-index.json";
    private const string ReleaseEvidenceStatusRelativePath = "dist/release-dry-run/release-evidence-status.json";
    private const string ReleaseEvidenceActionsRelativePath = "dist/release-dry-run/release-evidence-actions.json";
    private const string CollectionPlanEvidenceRelativePath = "dist/release-dry-run/release-evidence-collection-plan.json";
    private const string ReleaseEvidenceHandoffRelativePath = "dist/release-dry-run/release-evidence-handoff.md";
    private const string ReleaseVerifyEvidenceCommandHint = "forge release verify <project-root> --format json --no-input";

    private static readonly string[] ReleaseDryRunEvidenceRelativePaths =
    [
        ReleaseEvidenceIndexRelativePath,
        ReleaseEvidenceStatusRelativePath,
        ReleaseEvidenceActionsRelativePath,
        CollectionPlanEvidenceRelativePath,
        ReleaseEvidenceHandoffRelativePath
    ];

    private static readonly (int Order, string Id, string EvidenceId, string TargetPath)[] CollectionPlanExpectedSteps =
    [
        (1, "collect-schema-validation", "schema-validation", SchemaValidationEvidenceRelativePath),
        (2, "collect-capability-environment-validation", "capability-environment-validation", CapabilityEnvironmentEvidenceRelativePath),
        (3, "collect-package-validation", "package-validation", PackageValidationEvidenceRelativePath),
        (4, "collect-release-verification", "release-verification", ReleaseVerificationEvidenceRelativePath)
    ];

    public static ReleasePublishPreflightResult Plan(ReleasePublishPreflightOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var releasePrepareEvidenceRootPath = Path.Combine(projectRoot, "dist", "release-prepare");
        var releasePrepareEvidenceRoot = ToDisplayPath(projectRoot, releasePrepareEvidenceRootPath);
        var localEvidenceArtifacts = CreateLocalEvidenceArtifacts(projectRoot, releasePrepareEvidenceRootPath);
        var checksumSidecar = CreateChecksumSidecar(projectRoot, releasePrepareEvidenceRootPath);
        var buildManifestCrossReference = CreateBuildManifestCrossReference(projectRoot, releasePrepareEvidenceRootPath, localEvidenceArtifacts, checksumSidecar);
        var archiveEvidenceCrossReference = CreateArchiveEvidenceCrossReference(projectRoot, releasePrepareEvidenceRootPath, localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference);
        var semanticEvidenceValidation = CreateSemanticEvidenceValidation(projectRoot, releasePrepareEvidenceRootPath, localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference);
        var schemaValidationEvidence = CreateSchemaValidationEvidence(projectRoot);
        var capabilityEnvironmentEvidence = CreateCapabilityEnvironmentEvidence(projectRoot);
        var packageValidationEvidence = CreatePackageValidationEvidence(projectRoot);
        var releaseVerificationEvidence = CreateReleaseVerificationEvidence(projectRoot);
        var collectionPlanEvidence = CreateCollectionPlanEvidence(projectRoot);
        var dryRunCrossLinkEvidence = CreateDryRunCrossLinkEvidence(projectRoot);
        var dryRunEvidenceRemediation = CreateDryRunEvidenceRemediation(dryRunCrossLinkEvidence);
        var governanceChecks = CreateGovernanceChecks(projectRoot);
        var approval = CreateApprovalRequirement(projectRoot, options.Yes, options.Confirm);
        var presentArtifacts = localEvidenceArtifacts.Count(artifact => artifact.Exists);
        var missingArtifacts = localEvidenceArtifacts.Count - presentArtifacts;
        var wellFormedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "well-formed-not-validated"));
        var malformedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "malformed"));
        var unclassifiedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "present-not-classified"));
        var releasePrepareEvidenceStatus = ReleasePrepareEvidenceStatus(presentArtifacts, missingArtifacts, malformedArtifacts, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference, semanticEvidenceValidation);
        var status = options.DryRun ? "planned" : "refused";
        var requiredEvidence = CreateRequiredEvidence(localEvidenceArtifacts, schemaValidationEvidence, capabilityEnvironmentEvidence, packageValidationEvidence, releaseVerificationEvidence, collectionPlanEvidence, dryRunCrossLinkEvidence, semanticEvidenceValidation, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference, governanceChecks);
        var publishReadiness = CreatePublishReadiness(requiredEvidence, approval);
        var laneCloseout = CreateLaneCloseout();
        var refusalReason = CreateRefusalReason(options.DryRun, approval, publishReadiness);

        return new ReleasePublishPreflightResult(
            status,
            projectRoot,
            options.DryRun,
            PlanningOnly: true,
            PublishReady: publishReadiness.ReadyForRealPublish,
            releasePrepareEvidenceRoot,
            releasePrepareEvidenceStatus,
            localEvidenceArtifacts.Count,
            presentArtifacts,
            missingArtifacts,
            wellFormedArtifacts,
            malformedArtifacts,
            unclassifiedArtifacts,
            checksumSidecar,
            buildManifestCrossReference,
            archiveEvidenceCrossReference,
            semanticEvidenceValidation,
            schemaValidationEvidence,
            capabilityEnvironmentEvidence,
            packageValidationEvidence,
            releaseVerificationEvidence,
            collectionPlanEvidence,
            dryRunCrossLinkEvidence,
            dryRunEvidenceRemediation,
            publishReadiness,
            laneCloseout,
            refusalReason,
            requiredEvidence,
            localEvidenceArtifacts,
            governanceChecks,
            approval,
            BoundaryLines);
    }

    private static IReadOnlyList<ReleasePublishEvidenceRequirement> CreateRequiredEvidence(
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishSchemaValidationEvidence schemaValidationEvidence,
        ReleasePublishCapabilityEnvironmentEvidence capabilityEnvironmentEvidence,
        ReleasePublishPackageValidationEvidence packageValidationEvidence,
        ReleasePublishReleaseVerificationEvidence releaseVerificationEvidence,
        ReleasePublishCollectionPlanEvidence collectionPlanEvidence,
        ReleasePublishDryRunCrossLinkEvidence dryRunCrossLinkEvidence,
        ReleasePublishSemanticEvidenceValidation semanticEvidenceValidation,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference,
        IReadOnlyList<ReleasePublishGovernanceCheck> governanceChecks) =>
    [
        Evidence("schema-validation", "Schema validation passed", "ADR-011 layered validation", schemaValidationEvidence.Status, checkedInCurrentGate: true),
        Evidence("semantic-validation", "Semantic validation passed", "ADR-011 layered validation", semanticEvidenceValidation.Status, checkedInCurrentGate: true),
        Evidence("capability-environment-validation", "Capability and environment validation passed", "ADR-008 and ADR-011", capabilityEnvironmentEvidence.Status, checkedInCurrentGate: true),
        Evidence("package-validation", "Package validation passed", "ADR-011 release validation", packageValidationEvidence.Status, checkedInCurrentGate: true),
        Evidence("release-verification", "Release verification passed", "forge release verify", releaseVerificationEvidence.Status, checkedInCurrentGate: true),
        Evidence("release-prepare-build-manifest", "Local release prepare build-manifest.json exists and is accepted", "ADR-011 mandatory local build manifest", BuildManifestRequirementStatus(localEvidenceArtifacts, buildManifestCrossReference), checkedInCurrentGate: true),
        Evidence("release-prepare-checksums", "Local release prepare checksums.sha256 exists and is accepted", "ADR-011 checksum evidence", ChecksumRequirementStatus(localEvidenceArtifacts, checksumSidecar), checkedInCurrentGate: true),
        Evidence("release-prepare-archive-evidence", "Local release archive evidence exists and is accepted", "Gate 268 release archive evidence", ArchiveEvidenceRequirementStatus(localEvidenceArtifacts, archiveEvidenceCrossReference), checkedInCurrentGate: true),
        Evidence("governance-checks", "Governance checks passed", "ADR-011 release governance", GovernanceRequirementStatus(governanceChecks), checkedInCurrentGate: true),
        Evidence("release-dry-run-collection-plan", "Release dry-run evidence collection plan accepted", "Gate 307 release publish collection-plan evidence", collectionPlanEvidence.Status, checkedInCurrentGate: true),
        Evidence("release-dry-run-cross-links", "Release dry-run evidence cross-links are consistent", "Gate 308 release publish dry-run evidence cross-links", dryRunCrossLinkEvidence.Status, checkedInCurrentGate: true)
    ];

    private static ReleasePublishReadiness CreatePublishReadiness(
        IReadOnlyList<ReleasePublishEvidenceRequirement> requiredEvidence,
        ReleasePublishApprovalRequirement approval)
    {
        var checks = requiredEvidence
            .Select(ToReadinessCheck)
            .Append(new ReleasePublishReadinessCheck(
                "human-approval",
                "Explicit human approval recorded",
                "Gate 286 approval preflight",
                approval.Status,
                Required: true,
                Satisfied: approval.Provided,
                approval.Provided
                    ? "readiness-requirement-satisfied"
                    : approval.Detail))
            .ToArray();
        var evidenceSatisfied = checks
            .Where(check => !StringComparer.Ordinal.Equals(check.Id, "governance-checks") && !StringComparer.Ordinal.Equals(check.Id, "human-approval"))
            .All(check => check.Satisfied);
        var governanceSatisfied = checks.Any(check => StringComparer.Ordinal.Equals(check.Id, "governance-checks") && check.Satisfied);
        var approvalSatisfied = approval.Provided;
        var localPreconditionsSatisfied = evidenceSatisfied && governanceSatisfied && approvalSatisfied;
        var blockingCheckIds = checks
            .Where(check => check.Required && !check.Satisfied)
            .Select(check => check.Id)
            .ToArray();
        var status = localPreconditionsSatisfied
            ? "locally-ready-no-publish-gate"
            : "blocked-by-preconditions";

        return new ReleasePublishReadiness(
            EvaluatedInCurrentGate: true,
            status,
            localPreconditionsSatisfied,
            evidenceSatisfied,
            governanceSatisfied,
            approvalSatisfied,
            PublishExecutionEnabled: false,
            ReadyForRealPublish: false,
            checks.Count(check => check.Required),
            checks.Count(check => check.Required && check.Satisfied),
            blockingCheckIds.Length,
            localPreconditionsSatisfied
                ? "publish-readiness-local-preconditions-satisfied-but-publish-execution-disabled"
                : "publish-readiness-local-preconditions-incomplete",
            blockingCheckIds,
            checks);
    }

    private static ReleasePublishReadinessCheck ToReadinessCheck(ReleasePublishEvidenceRequirement requirement)
    {
        var expectedStatus = ExpectedReadinessStatus(requirement.Id);
        var satisfied = expectedStatus is not null &&
            StringComparer.Ordinal.Equals(requirement.Status, expectedStatus);
        var detail = satisfied
            ? "readiness-requirement-satisfied"
            : expectedStatus is null
                ? "readiness-requirement-has-no-accepted-status"
                : $"expected-{expectedStatus}-but-was-{requirement.Status}";
        return new ReleasePublishReadinessCheck(
            requirement.Id,
            requirement.Title,
            requirement.Source,
            requirement.Status,
            requirement.Required,
            satisfied,
            detail);
    }

    private static string? ExpectedReadinessStatus(string id) => id switch
    {
        "schema-validation" => "complete-schema-validated",
        "semantic-validation" => "complete-semantic-validated",
        "capability-environment-validation" => "complete-capability-environment-validated",
        "package-validation" => "complete-package-validated",
        "release-verification" => "complete-release-verified",
        "release-prepare-build-manifest" => "complete-digest-revalidated",
        "release-prepare-checksums" => "complete-digest-revalidated",
        "release-prepare-archive-evidence" => "complete-archive-revalidated",
        "governance-checks" => "complete-governance-evaluated",
        "release-dry-run-collection-plan" => "complete-collection-plan-validated",
        "release-dry-run-cross-links" => "complete-cross-links-validated",
        _ => null
    };

    private static ReleasePublishLaneCloseout CreateLaneCloseout() =>
        new(
            ClosedInCurrentGate: true,
            Status: "closed-no-publish-lane",
            Lane: "forge release publish no-publish preflight",
            CompletedThroughGate: "Gate 287 publish-readiness aggregation",
            NextGate: "Gate 289",
            NextValueSlice: "forge doctor export release-readiness handoff",
            Detail: "release-publish-no-publish-lane-closed-real-publishing-deferred",
            DeferredCapabilities: DeferredPublishCapabilities);

    private static string? CreateRefusalReason(bool dryRun, ReleasePublishApprovalRequirement approval, ReleasePublishReadiness publishReadiness)
    {
        if (dryRun)
        {
            return null;
        }

        if (publishReadiness.LocalPreconditionsSatisfied)
        {
            return "Local publish readiness is satisfied, but Gate 288 closes the no-publish lane without publishing releases.";
        }

        return approval.Provided
            ? "Release publish approval was recorded, but Gate 288 still requires complete local publish-readiness evidence and closes the no-publish lane without publishing releases."
            : "Release publish requires complete local publish-readiness evidence plus explicit human approval; Gate 288 closes the no-publish lane without publishing releases.";
    }

    private static IReadOnlyList<ReleasePublishGovernanceCheck> CreateGovernanceChecks(string projectRoot) =>
    [
        PolicyFileCheck(
            "immutable-schema-policy",
            "Published schemas remain immutable",
            projectRoot,
            Path.Combine("docs", "governance", "schema-version-policy.md"),
            ["schema", "immutable", "$id"]),
        ToolVersionSemVerCheck(),
        ReleaseWorkflowPermissionsCheck(projectRoot),
        CodeownersCheck(projectRoot),
        PolicyFileCheck(
            "redistributable-fixture-policy",
            "Public fixtures remain synthetic and redistributable",
            projectRoot,
            Path.Combine("docs", "governance", "fixture-policy.md"),
            ["synthetic", "redistributable"]),
        AiOptionalReleasePathCheck(projectRoot)
    ];

    private static ReleasePublishEvidenceRequirement Evidence(string id, string title, string source, string status = "required-not-evaluated", bool checkedInCurrentGate = false) =>
        new(
            id,
            title,
            source,
            Status: status,
            Required: true,
            CheckedInCurrentGate: checkedInCurrentGate);

    private static ReleasePublishSchemaValidationEvidence CreateSchemaValidationEvidence(string projectRoot)
    {
        var fullPath = Path.Combine(projectRoot, SchemaValidationEvidenceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return SchemaValidationEvidence(displayPath, exists: false, "missing", contentRead: false, "validation-report-missing");
        }

        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException ex)
        {
            return SchemaValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, $"json-parse-error:{ex.Message}");
        }
        catch (IOException)
        {
            return SchemaValidationEvidence(displayPath, exists: true, "malformed", contentRead: false, "validation-report-unreadable");
        }
        catch (UnauthorizedAccessException)
        {
            return SchemaValidationEvidence(displayPath, exists: true, "malformed", contentRead: false, "validation-report-unreadable");
        }

        if (root is null)
        {
            return SchemaValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "validation-report-not-json-object");
        }

        if (!TryGetStringProperty(root, "formatVersion", out var formatVersion) ||
            !StringComparer.Ordinal.Equals(formatVersion, "1.0") ||
            root["tool"] is not JsonObject tool ||
            !TryGetStringProperty(tool, "name", out var toolName) ||
            !StringComparer.Ordinal.Equals(toolName, CliConstants.ToolName) ||
            !TryGetStringProperty(tool, "version", out var toolVersion) ||
            !StringComparer.Ordinal.Equals(toolVersion, CliConstants.Version) ||
            !TryGetStringProperty(root, "command", out var command) ||
            !StringComparer.Ordinal.Equals(command, "validate"))
        {
            return SchemaValidationEvidence(displayPath, exists: true, "unexpected-report", contentRead: true, "validation-report-identity-mismatch");
        }

        if (root["summary"] is not JsonObject summary ||
            !TryGetInt32Property(summary, "errors", out var errors) ||
            !TryGetInt32Property(summary, "warnings", out var warnings) ||
            !TryGetInt32Property(summary, "notes", out var notes) ||
            root["issues"] is not JsonArray issues)
        {
            return SchemaValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "validation-report-summary-or-issues-malformed");
        }

        var issueCount = 0;
        var schemaIssueCount = 0;
        foreach (var issueNode in issues)
        {
            if (issueNode is not JsonObject issue ||
                !TryGetStringProperty(issue, "ruleId", out var ruleId))
            {
                return SchemaValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "validation-report-issue-malformed");
            }

            issueCount++;
            if (ruleId.StartsWith("WF-SCHEMA-", StringComparison.Ordinal))
            {
                schemaIssueCount++;
            }
        }

        return new ReleasePublishSchemaValidationEvidence(
            displayPath,
            Exists: true,
            Status: schemaIssueCount == 0 ? "complete-schema-validated" : "schema-diagnostics-present",
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: true,
            errors,
            warnings,
            notes,
            issueCount,
            schemaIssueCount,
            schemaIssueCount == 0 ? "validation-report-has-no-schema-diagnostics" : "validation-report-has-schema-diagnostics");
    }

    private static ReleasePublishSchemaValidationEvidence SchemaValidationEvidence(
        string path,
        bool exists,
        string status,
        bool contentRead,
        string detail) =>
        new(
            path,
            exists,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            Errors: 0,
            Warnings: 0,
            Notes: 0,
            IssueCount: 0,
            SchemaIssueCount: 0,
            detail);

    private static ReleasePublishCapabilityEnvironmentEvidence CreateCapabilityEnvironmentEvidence(string projectRoot)
    {
        var fullPath = Path.Combine(projectRoot, CapabilityEnvironmentEvidenceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: false, "missing", contentRead: false, projectScoped: false, "capability-scan-report-missing");
        }

        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException ex)
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: true, "malformed", contentRead: true, projectScoped: false, $"json-parse-error:{ex.Message}");
        }
        catch (IOException)
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: true, "malformed", contentRead: false, projectScoped: false, "capability-scan-report-unreadable");
        }
        catch (UnauthorizedAccessException)
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: true, "malformed", contentRead: false, projectScoped: false, "capability-scan-report-unreadable");
        }

        if (root is null)
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: true, "malformed", contentRead: true, projectScoped: false, "capability-scan-report-not-json-object");
        }

        if (!TryGetStringProperty(root, "formatVersion", out var formatVersion) ||
            !StringComparer.Ordinal.Equals(formatVersion, "1.0") ||
            root["tool"] is not JsonObject tool ||
            !TryGetStringProperty(tool, "name", out var toolName) ||
            !StringComparer.Ordinal.Equals(toolName, CliConstants.ToolName) ||
            !TryGetStringProperty(tool, "version", out var toolVersion) ||
            !StringComparer.Ordinal.Equals(toolVersion, CliConstants.Version) ||
            !TryGetStringProperty(root, "command", out var command) ||
            !StringComparer.Ordinal.Equals(command, "capabilities scan"))
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: true, "unexpected-report", contentRead: true, projectScoped: false, "capability-scan-report-identity-mismatch");
        }

        if (root["inputs"] is not JsonObject inputs ||
            !TryGetBooleanProperty(inputs, "runtimeProbesEnabled", out var runtimeProbesEnabled) ||
            !TryGetBooleanProperty(inputs, "mo2VfsEnabled", out var mo2VfsEnabled) ||
            root["summary"] is not JsonObject summary ||
            !TryGetInt32Property(summary, "providers", out var providers) ||
            !TryGetInt32Property(summary, "capabilities", out var capabilities) ||
            !TryGetInt32Property(summary, "missingProviders", out var missingProviders) ||
            !TryGetInt32Property(summary, "unknownProviders", out var unknownProviders) ||
            !TryGetInt32Property(summary, "wrongScopeProviders", out var wrongScopeProviders) ||
            !TryGetInt32Property(summary, "missingCapabilities", out var missingCapabilities) ||
            !TryGetInt32Property(summary, "unknownCapabilities", out var unknownCapabilities) ||
            !TryGetInt32Property(summary, "wrongScopeCapabilities", out var wrongScopeCapabilities) ||
            root["doctor"] is not JsonObject doctor ||
            doctor["summary"] is not JsonObject doctorSummary ||
            !TryGetInt32Property(doctorSummary, "areas", out var doctorAreas) ||
            !TryGetInt32Property(doctorSummary, "actionNeededAreas", out var doctorActionNeededAreas) ||
            !TryGetInt32Property(doctorSummary, "unknownAreas", out var doctorUnknownAreas) ||
            root["diagnostics"] is not JsonObject diagnostics ||
            diagnostics["issues"] is not JsonArray diagnosticIssues)
        {
            return CapabilityEnvironmentEvidence(displayPath, exists: true, "malformed", contentRead: true, projectScoped: false, "capability-scan-report-summary-or-diagnostics-malformed");
        }

        var diagnosticCount = 0;
        var capabilityDiagnosticCount = 0;
        foreach (var issueNode in diagnosticIssues)
        {
            if (issueNode is not JsonObject issue ||
                !TryGetStringProperty(issue, "ruleId", out var ruleId))
            {
                return CapabilityEnvironmentEvidence(displayPath, exists: true, "malformed", contentRead: true, projectScoped: false, "capability-scan-report-diagnostic-malformed");
            }

            diagnosticCount++;
            if (ruleId.StartsWith("WF-CAP-", StringComparison.Ordinal))
            {
                capabilityDiagnosticCount++;
            }
        }

        var requirementCount = 0;
        var requiredUnavailable = 0;
        var optionalUnavailable = 0;
        var projectScoped = root["requirements"] is JsonObject requirements &&
            requirements["summary"] is JsonObject requirementSummary &&
            TryGetInt32Property(requirementSummary, "requirements", out requirementCount) &&
            TryGetInt32Property(requirementSummary, "requiredUnavailable", out requiredUnavailable) &&
            TryGetInt32Property(requirementSummary, "optionalUnavailable", out optionalUnavailable);

        var status = ResolveCapabilityEnvironmentEvidenceStatus(
            projectScoped,
            runtimeProbesEnabled,
            mo2VfsEnabled,
            requiredUnavailable,
            capabilityDiagnosticCount);
        var detail = ResolveCapabilityEnvironmentEvidenceDetail(status);

        return new ReleasePublishCapabilityEnvironmentEvidence(
            displayPath,
            Exists: true,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: true,
            projectScoped,
            runtimeProbesEnabled,
            mo2VfsEnabled,
            providers,
            capabilities,
            missingProviders,
            unknownProviders,
            wrongScopeProviders,
            missingCapabilities,
            unknownCapabilities,
            wrongScopeCapabilities,
            requirementCount,
            requiredUnavailable,
            optionalUnavailable,
            diagnosticCount,
            capabilityDiagnosticCount,
            doctorAreas,
            doctorActionNeededAreas,
            doctorUnknownAreas,
            detail);
    }

    private static ReleasePublishCapabilityEnvironmentEvidence CapabilityEnvironmentEvidence(
        string path,
        bool exists,
        string status,
        bool contentRead,
        bool projectScoped,
        string detail) =>
        new(
            path,
            exists,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            projectScoped,
            RuntimeProbesEnabled: false,
            Mo2VfsEnabled: false,
            Providers: 0,
            Capabilities: 0,
            MissingProviders: 0,
            UnknownProviders: 0,
            WrongScopeProviders: 0,
            MissingCapabilities: 0,
            UnknownCapabilities: 0,
            WrongScopeCapabilities: 0,
            Requirements: 0,
            RequiredUnavailable: 0,
            OptionalUnavailable: 0,
            Diagnostics: 0,
            CapabilityDiagnostics: 0,
            DoctorAreas: 0,
            DoctorActionNeededAreas: 0,
            DoctorUnknownAreas: 0,
            detail);

    private static string ResolveCapabilityEnvironmentEvidenceStatus(
        bool projectScoped,
        bool runtimeProbesEnabled,
        bool mo2VfsEnabled,
        int requiredUnavailable,
        int capabilityDiagnosticCount)
    {
        if (runtimeProbesEnabled || mo2VfsEnabled)
        {
            return "unsupported-evidence";
        }

        if (!projectScoped)
        {
            return "not-project-scoped";
        }

        return requiredUnavailable == 0 && capabilityDiagnosticCount == 0
            ? "complete-capability-environment-validated"
            : "capability-diagnostics-present";
    }

    private static string ResolveCapabilityEnvironmentEvidenceDetail(string status) => status switch
    {
        "complete-capability-environment-validated" => "capability-scan-report-has-no-required-capability-diagnostics",
        "capability-diagnostics-present" => "capability-scan-report-has-capability-diagnostics",
        "not-project-scoped" => "capability-scan-report-not-project-scoped",
        "unsupported-evidence" => "capability-scan-report-used-runtime-or-mo2-evidence",
        _ => status
    };

    private static ReleasePublishPackageValidationEvidence CreatePackageValidationEvidence(string projectRoot)
    {
        var fullPath = Path.Combine(projectRoot, PackageValidationEvidenceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return PackageValidationEvidence(displayPath, exists: false, "missing", contentRead: false, "package-verify-report-missing");
        }

        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException ex)
        {
            return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, $"json-parse-error:{ex.Message}");
        }
        catch (IOException)
        {
            return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: false, "package-verify-report-unreadable");
        }
        catch (UnauthorizedAccessException)
        {
            return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: false, "package-verify-report-unreadable");
        }

        if (root is null)
        {
            return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "package-verify-report-not-json-object");
        }

        if (!TryGetStringProperty(root, "formatVersion", out var formatVersion) ||
            !StringComparer.Ordinal.Equals(formatVersion, "1.0") ||
            root["tool"] is not JsonObject tool ||
            !TryGetStringProperty(tool, "name", out var toolName) ||
            !StringComparer.Ordinal.Equals(toolName, CliConstants.ToolName) ||
            !TryGetStringProperty(tool, "version", out var toolVersion) ||
            !StringComparer.Ordinal.Equals(toolVersion, CliConstants.Version) ||
            !TryGetStringProperty(root, "command", out var command) ||
            !StringComparer.Ordinal.Equals(command, "package") ||
            !TryGetStringProperty(root, "target", out var target) ||
            !StringComparer.Ordinal.Equals(target, "mcm-json") ||
            !TryGetStringProperty(root, "mode", out var mode) ||
            !StringComparer.Ordinal.Equals(mode, "verify-existing"))
        {
            return PackageValidationEvidence(displayPath, exists: true, "unexpected-report", contentRead: true, "package-verify-report-identity-mismatch");
        }

        if (!TryGetStringProperty(root, "status", out var reportStatus) ||
            root["summary"] is not JsonObject summary ||
            !TryGetInt32Property(summary, "errors", out var errors) ||
            !TryGetInt32Property(summary, "warnings", out var warnings) ||
            !TryGetInt32Property(summary, "notes", out var notes) ||
            root["outputs"] is not JsonObject outputs ||
            !TryGetStringProperty(outputs, "root", out var outputRoot) ||
            !TryGetStringProperty(outputs, "packageManifest", out _) ||
            !TryGetStringProperty(outputs, "installPreview", out _) ||
            !TryGetStringProperty(outputs, "installPlan", out _) ||
            !TryGetStringProperty(outputs, "packageVerification", out _) ||
            !TryGetStringProperty(outputs, "checksums", out _) ||
            !TryGetStringProperty(outputs, "buildManifest", out _) ||
            root["issues"] is not JsonArray issues)
        {
            return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "package-verify-report-summary-outputs-or-issues-malformed");
        }

        if (!StringComparer.Ordinal.Equals(reportStatus, "passed") &&
            !StringComparer.Ordinal.Equals(reportStatus, "failed"))
        {
            return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "package-verify-report-status-malformed");
        }

        var issueCount = 0;
        var packageIssueCount = 0;
        foreach (var issueNode in issues)
        {
            if (issueNode is not JsonObject issue ||
                !TryGetStringProperty(issue, "ruleId", out var ruleId))
            {
                return PackageValidationEvidence(displayPath, exists: true, "malformed", contentRead: true, "package-verify-report-issue-malformed");
            }

            issueCount++;
            if (ruleId.StartsWith("WF-BUILD-", StringComparison.Ordinal))
            {
                packageIssueCount++;
            }
        }

        var distScoped = IsDistScopedPackageOutput(outputRoot);
        var status = ResolvePackageValidationEvidenceStatus(
            reportStatus,
            distScoped,
            errors,
            packageIssueCount);
        var detail = ResolvePackageValidationEvidenceDetail(status);

        return new ReleasePublishPackageValidationEvidence(
            displayPath,
            Exists: true,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: true,
            target,
            mode,
            outputRoot,
            distScoped,
            PackageArchivePresent: TryGetStringProperty(outputs, "packageArchive", out _),
            errors,
            warnings,
            notes,
            issueCount,
            packageIssueCount,
            detail);
    }

    private static ReleasePublishPackageValidationEvidence PackageValidationEvidence(
        string path,
        bool exists,
        string status,
        bool contentRead,
        string detail) =>
        new(
            path,
            exists,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            Target: null,
            Mode: null,
            OutputRoot: null,
            DistScoped: false,
            PackageArchivePresent: false,
            Errors: 0,
            Warnings: 0,
            Notes: 0,
            IssueCount: 0,
            PackageIssueCount: 0,
            detail);

    private static string ResolvePackageValidationEvidenceStatus(
        string reportStatus,
        bool distScoped,
        int errors,
        int packageIssueCount)
    {
        if (!distScoped)
        {
            return "not-dist-scoped";
        }

        return StringComparer.Ordinal.Equals(reportStatus, "passed") &&
            errors == 0 &&
            packageIssueCount == 0
            ? "complete-package-validated"
            : "package-diagnostics-present";
    }

    private static string ResolvePackageValidationEvidenceDetail(string status) => status switch
    {
        "complete-package-validated" => "package-verify-report-has-no-package-diagnostics",
        "package-diagnostics-present" => "package-verify-report-has-package-diagnostics",
        "not-dist-scoped" => "package-verify-report-not-dist-scoped",
        _ => status
    };

    private static bool IsDistScopedPackageOutput(string outputRoot)
    {
        var normalized = outputRoot.Replace('\\', '/');
        return StringComparer.Ordinal.Equals(normalized, "dist") ||
            normalized.StartsWith("dist/", StringComparison.Ordinal);
    }

    private static ReleasePublishReleaseVerificationEvidence CreateReleaseVerificationEvidence(string projectRoot)
    {
        var fullPath = Path.Combine(projectRoot, ReleaseVerificationEvidenceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return ReleaseVerificationEvidence(displayPath, exists: false, "missing", contentRead: false, "release-verify-report-missing");
        }

        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException ex)
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, $"json-parse-error:{ex.Message}");
        }
        catch (IOException)
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: false, "release-verify-report-unreadable");
        }
        catch (UnauthorizedAccessException)
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: false, "release-verify-report-unreadable");
        }

        if (root is null)
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, "release-verify-report-not-json-object");
        }

        if (!TryGetStringProperty(root, "formatVersion", out var formatVersion) ||
            !StringComparer.Ordinal.Equals(formatVersion, "1.0") ||
            root["tool"] is not JsonObject tool ||
            !TryGetStringProperty(tool, "name", out var toolName) ||
            !StringComparer.Ordinal.Equals(toolName, CliConstants.ToolName) ||
            !TryGetStringProperty(tool, "version", out var toolVersion) ||
            !StringComparer.Ordinal.Equals(toolVersion, CliConstants.Version) ||
            !TryGetStringProperty(root, "command", out var command) ||
            !StringComparer.Ordinal.Equals(command, "release verify"))
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "unexpected-report", contentRead: true, "release-verify-report-identity-mismatch");
        }

        if (!TryGetStringProperty(root, "status", out var reportStatus) ||
            !TryGetBooleanProperty(root, "dryRun", out var dryRun) ||
            root["summary"] is not JsonObject summary ||
            !TryGetInt32Property(summary, "errors", out var errors) ||
            !TryGetInt32Property(summary, "warnings", out var warnings) ||
            !TryGetInt32Property(summary, "notes", out var notes) ||
            root["issues"] is not JsonArray issues)
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, "release-verify-report-summary-or-issues-malformed");
        }

        if (!StringComparer.Ordinal.Equals(reportStatus, "passed") &&
            !StringComparer.Ordinal.Equals(reportStatus, "failed"))
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, "release-verify-report-status-malformed");
        }

        string? outputRoot = null;
        var distScoped = false;
        if (root["outputs"] is JsonObject outputs)
        {
            if (!TryGetStringProperty(outputs, "root", out outputRoot) ||
                !TryGetStringProperty(outputs, "stagingRoot", out _) ||
                !TryGetStringProperty(outputs, "validationReport", out _) ||
                !TryGetStringProperty(outputs, "releaseSummary", out _) ||
                !TryGetStringProperty(outputs, "buildManifest", out _) ||
                !TryGetStringProperty(outputs, "checksums", out _))
            {
                return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, "release-verify-report-outputs-malformed");
            }

            distScoped = IsDistScopedReleaseVerifyOutput(outputRoot);
        }
        else if (StringComparer.Ordinal.Equals(reportStatus, "passed"))
        {
            return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, "release-verify-report-passed-without-outputs");
        }

        var issueCount = 0;
        var releaseIssueCount = 0;
        foreach (var issueNode in issues)
        {
            if (issueNode is not JsonObject issue ||
                !TryGetStringProperty(issue, "ruleId", out var ruleId))
            {
                return ReleaseVerificationEvidence(displayPath, exists: true, "malformed", contentRead: true, "release-verify-report-issue-malformed");
            }

            issueCount++;
            if (ruleId.StartsWith("WF-REL-", StringComparison.Ordinal))
            {
                releaseIssueCount++;
            }
        }

        var status = ResolveReleaseVerificationEvidenceStatus(
            reportStatus,
            dryRun,
            outputRoot,
            distScoped,
            errors,
            releaseIssueCount);
        var detail = ResolveReleaseVerificationEvidenceDetail(status);

        return new ReleasePublishReleaseVerificationEvidence(
            displayPath,
            Exists: true,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: true,
            dryRun,
            outputRoot,
            distScoped,
            errors,
            warnings,
            notes,
            issueCount,
            releaseIssueCount,
            detail);
    }

    private static ReleasePublishReleaseVerificationEvidence ReleaseVerificationEvidence(
        string path,
        bool exists,
        string status,
        bool contentRead,
        string detail) =>
        new(
            path,
            exists,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            DryRun: false,
            OutputRoot: null,
            DistScoped: false,
            Errors: 0,
            Warnings: 0,
            Notes: 0,
            IssueCount: 0,
            ReleaseIssueCount: 0,
            detail);

    private static string ResolveReleaseVerificationEvidenceStatus(
        string reportStatus,
        bool dryRun,
        string? outputRoot,
        bool distScoped,
        int errors,
        int releaseIssueCount)
    {
        if (!dryRun)
        {
            return "unsupported-evidence";
        }

        if (outputRoot is not null && !distScoped)
        {
            return "not-dist-scoped";
        }

        return StringComparer.Ordinal.Equals(reportStatus, "passed") &&
            outputRoot is not null &&
            errors == 0 &&
            releaseIssueCount == 0
            ? "complete-release-verified"
            : "release-diagnostics-present";
    }

    private static string ResolveReleaseVerificationEvidenceDetail(string status) => status switch
    {
        "complete-release-verified" => "release-verify-report-has-no-release-diagnostics",
        "release-diagnostics-present" => "release-verify-report-has-release-diagnostics",
        "not-dist-scoped" => "release-verify-report-not-dist-scoped",
        "unsupported-evidence" => "release-verify-report-not-dry-run",
        _ => status
    };

    private static bool IsDistScopedReleaseVerifyOutput(string outputRoot)
    {
        var normalized = outputRoot.Replace('\\', '/');
        return StringComparer.Ordinal.Equals(normalized, "dist") ||
            normalized.StartsWith("dist/", StringComparison.Ordinal);
    }

    private static ReleasePublishCollectionPlanEvidence CreateCollectionPlanEvidence(string projectRoot)
    {
        var fullPath = Path.Combine(projectRoot, CollectionPlanEvidenceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return CollectionPlanEvidence(displayPath, exists: false, "missing", contentRead: false, "collection-plan-missing");
        }

        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException ex)
        {
            return CollectionPlanEvidence(displayPath, exists: true, "malformed", contentRead: true, $"json-parse-error:{ex.Message}");
        }
        catch (IOException)
        {
            return CollectionPlanEvidence(displayPath, exists: true, "malformed", contentRead: false, "collection-plan-unreadable");
        }
        catch (UnauthorizedAccessException)
        {
            return CollectionPlanEvidence(displayPath, exists: true, "malformed", contentRead: false, "collection-plan-unreadable");
        }

        if (root is null)
        {
            return CollectionPlanEvidence(displayPath, exists: true, "malformed", contentRead: true, "collection-plan-not-json-object");
        }

        if (!TryGetStringProperty(root, "formatVersion", out var formatVersion) ||
            !StringComparer.Ordinal.Equals(formatVersion, "0.1") ||
            !TryGetStringProperty(root, "kind", out var kind) ||
            !StringComparer.Ordinal.Equals(kind, "wastelandforge.release-dry-run-evidence-collection-plan") ||
            !TryGetStringProperty(root, "command", out var command) ||
            !StringComparer.Ordinal.Equals(command, "release verify") ||
            !TryGetBooleanProperty(root, "dryRun", out var dryRun) ||
            !TryGetStringProperty(root, "status", out var reportStatus) ||
            !StringComparer.Ordinal.Equals(reportStatus, "planned"))
        {
            return CollectionPlanEvidence(displayPath, exists: true, "unexpected-report", contentRead: true, "collection-plan-identity-mismatch");
        }

        if (!dryRun)
        {
            return CollectionPlanEvidence(displayPath, exists: true, "unsupported-evidence", contentRead: true, "collection-plan-not-dry-run");
        }

        if (!TryGetStringProperty(root, "outputRoot", out var outputRoot) ||
            root["summary"] is not JsonObject summary ||
            !TryGetInt32Property(summary, "total", out var total) ||
            !TryGetInt32Property(summary, "present", out var present) ||
            !TryGetInt32Property(summary, "missing", out var missing) ||
            !TryGetInt32Property(summary, "actions", out var actions) ||
            !TryGetInt32Property(summary, "steps", out var summarySteps) ||
            !TryGetInt32Property(summary, "manualSteps", out var manualSteps) ||
            !TryGetInt32Property(summary, "availableSteps", out var availableSteps) ||
            root["steps"] is not JsonArray steps ||
            root["execution"] is not JsonObject execution)
        {
            return CollectionPlanEvidence(displayPath, exists: true, "malformed", contentRead: true, "collection-plan-summary-steps-or-execution-malformed");
        }

        var linksEvidenceIndex = StringPropertyEquals(root, "evidenceIndex", "dist/release-dry-run/release-evidence-index.json");
        var linksEvidenceStatus = StringPropertyEquals(root, "evidenceStatus", "dist/release-dry-run/release-evidence-status.json");
        var linksActionChecklist = StringPropertyEquals(root, "actionChecklist", "dist/release-dry-run/release-evidence-actions.json");
        var linksHandoffSummary = StringPropertyEquals(root, "handoffSummary", "dist/release-dry-run/release-evidence-handoff.md");
        var distScoped = StringComparer.Ordinal.Equals(outputRoot.Replace('\\', '/'), "dist/release-dry-run");
        var stepContractValid = TryValidateCollectionPlanSteps(steps, out var malformedSteps);
        var summaryCountersValid =
            total == CollectionPlanExpectedSteps.Length &&
            present + missing == total &&
            actions <= total &&
            summarySteps == steps.Count &&
            summarySteps == CollectionPlanExpectedSteps.Length &&
            manualSteps + availableSteps == summarySteps &&
            manualSteps >= 0 &&
            availableSteps >= 0;
        var noExecutionBoundary = HasCollectionPlanNoExecutionBoundary(execution);
        var status = ResolveCollectionPlanEvidenceStatus(
            distScoped,
            linksEvidenceIndex,
            linksEvidenceStatus,
            linksActionChecklist,
            linksHandoffSummary,
            summaryCountersValid,
            stepContractValid,
            noExecutionBoundary);
        var detail = ResolveCollectionPlanEvidenceDetail(status);

        return new ReleasePublishCollectionPlanEvidence(
            displayPath,
            Exists: true,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: true,
            dryRun,
            outputRoot,
            distScoped,
            linksEvidenceIndex,
            linksEvidenceStatus,
            linksActionChecklist,
            linksHandoffSummary,
            summarySteps,
            manualSteps,
            availableSteps,
            malformedSteps,
            noExecutionBoundary,
            detail);
    }

    private static ReleasePublishCollectionPlanEvidence CollectionPlanEvidence(
        string path,
        bool exists,
        string status,
        bool contentRead,
        string detail) =>
        new(
            path,
            exists,
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            DryRun: false,
            OutputRoot: null,
            DistScoped: false,
            LinksEvidenceIndex: false,
            LinksEvidenceStatus: false,
            LinksActionChecklist: false,
            LinksHandoffSummary: false,
            Steps: 0,
            ManualSteps: 0,
            AvailableSteps: 0,
            MalformedSteps: 0,
            NoExecutionBoundary: false,
            detail);

    private static string ResolveCollectionPlanEvidenceStatus(
        bool distScoped,
        bool linksEvidenceIndex,
        bool linksEvidenceStatus,
        bool linksActionChecklist,
        bool linksHandoffSummary,
        bool summaryCountersValid,
        bool stepContractValid,
        bool noExecutionBoundary)
    {
        if (!noExecutionBoundary)
        {
            return "unsupported-evidence";
        }

        if (!distScoped)
        {
            return "not-dist-scoped";
        }

        if (!linksEvidenceIndex || !linksEvidenceStatus || !linksActionChecklist || !linksHandoffSummary)
        {
            return "collection-plan-links-mismatch";
        }

        return summaryCountersValid && stepContractValid
            ? "complete-collection-plan-validated"
            : "collection-plan-contract-mismatch";
    }

    private static string ResolveCollectionPlanEvidenceDetail(string status) => status switch
    {
        "complete-collection-plan-validated" => "collection-plan-contract-and-no-execution-boundary-validated",
        "collection-plan-contract-mismatch" => "collection-plan-summary-or-step-contract-mismatch",
        "collection-plan-links-mismatch" => "collection-plan-required-links-mismatch",
        "not-dist-scoped" => "collection-plan-output-root-not-dist-release-dry-run",
        "unsupported-evidence" => "collection-plan-execution-boundary-mismatch",
        _ => status
    };

    private static bool TryValidateCollectionPlanSteps(JsonArray steps, out int malformedSteps)
    {
        malformedSteps = 0;
        if (steps.Count != CollectionPlanExpectedSteps.Length)
        {
            malformedSteps = Math.Abs(steps.Count - CollectionPlanExpectedSteps.Length);
            return false;
        }

        foreach (var expectedStep in CollectionPlanExpectedSteps)
        {
            var step = steps[expectedStep.Order - 1] as JsonObject;
            if (step is null ||
                !TryGetInt32Property(step, "order", out var order) ||
                order != expectedStep.Order ||
                !StringPropertyEquals(step, "id", expectedStep.Id) ||
                !StringPropertyEquals(step, "evidenceId", expectedStep.EvidenceId) ||
                !StringPropertyEquals(step, "targetPath", expectedStep.TargetPath) ||
                !StringPropertyEquals(step, "requiredBy", "forge release publish") ||
                !TryGetStringProperty(step, "producerCommand", out _) ||
                !TryGetStringProperty(step, "commandHint", out _) ||
                !TryGetStringProperty(step, "sourceGate", out _) ||
                !TryGetStringProperty(step, "status", out var status) ||
                !IsCollectionStepStatus(status) ||
                !TryGetStringProperty(step, "collectionMode", out var collectionMode) ||
                !IsCollectionStepMode(collectionMode) ||
                !TryGetStringProperty(step, "execution", out var execution) ||
                !IsCollectionStepExecution(execution) ||
                !TryGetBooleanProperty(step, "producedByCurrentCommand", out _) ||
                !IsCollectionStepExecutionShapeValid(step, expectedStep.EvidenceId, status, collectionMode, execution))
            {
                malformedSteps++;
            }
        }

        return malformedSteps == 0;
    }

    private static bool IsCollectionStepStatus(string status) =>
        StringComparer.Ordinal.Equals(status, "available") ||
        StringComparer.Ordinal.Equals(status, "manual-required");

    private static bool IsCollectionStepMode(string mode) =>
        StringComparer.Ordinal.Equals(mode, "current-command-output") ||
        StringComparer.Ordinal.Equals(mode, "local-file") ||
        StringComparer.Ordinal.Equals(mode, "manual-command-hint");

    private static bool IsCollectionStepExecution(string execution) =>
        StringComparer.Ordinal.Equals(execution, "none") ||
        StringComparer.Ordinal.Equals(execution, "manual");

    private static bool IsCollectionStepExecutionShapeValid(JsonObject step, string evidenceId, string status, string collectionMode, string execution)
    {
        if (StringComparer.Ordinal.Equals(execution, "manual"))
        {
            return StringComparer.Ordinal.Equals(status, "manual-required") &&
                StringComparer.Ordinal.Equals(collectionMode, "manual-command-hint") &&
                StringPropertyEquals(step, "actionId", $"produce-{evidenceId}");
        }

        return !StringComparer.Ordinal.Equals(status, "manual-required") &&
            !StringComparer.Ordinal.Equals(collectionMode, "manual-command-hint") &&
            step["actionId"] is null;
    }

    private static bool HasCollectionPlanNoExecutionBoundary(JsonObject execution) =>
        BooleanPropertyEquals(execution, "commandFanOut", expected: false) &&
        BooleanPropertyEquals(execution, "capabilityScanExecution", expected: false) &&
        BooleanPropertyEquals(execution, "packageVerifyExecution", expected: false) &&
        BooleanPropertyEquals(execution, "releasePublishExecution", expected: false) &&
        BooleanPropertyEquals(execution, "releaseUploads", expected: false) &&
        BooleanPropertyEquals(execution, "attestationSigning", expected: false) &&
        BooleanPropertyEquals(execution, "externalToolExecution", expected: false) &&
        BooleanPropertyEquals(execution, "pluginMutation", expected: false) &&
        BooleanPropertyEquals(execution, "mo2Automation", expected: false) &&
        BooleanPropertyEquals(execution, "geckAutomation", expected: false) &&
        BooleanPropertyEquals(execution, "runtimeProbes", expected: false) &&
        BooleanPropertyEquals(execution, "realThirdPartyPluginFixtures", expected: false) &&
        BooleanPropertyEquals(execution, "ai", expected: false);

    private static ReleasePublishDryRunCrossLinkEvidence CreateDryRunCrossLinkEvidence(string projectRoot)
    {
        var index = ReadReleaseDryRunEvidenceJson(
            projectRoot,
            ReleaseEvidenceIndexRelativePath,
            "wastelandforge.release-dry-run-evidence-index",
            "planned");
        var status = ReadReleaseDryRunEvidenceJson(
            projectRoot,
            ReleaseEvidenceStatusRelativePath,
            "wastelandforge.release-dry-run-evidence-status",
            "projected");
        var actions = ReadReleaseDryRunEvidenceJson(
            projectRoot,
            ReleaseEvidenceActionsRelativePath,
            "wastelandforge.release-dry-run-missing-evidence-actions",
            "planned");
        var collectionPlan = ReadReleaseDryRunEvidenceJson(
            projectRoot,
            CollectionPlanEvidenceRelativePath,
            "wastelandforge.release-dry-run-evidence-collection-plan",
            "planned");
        var reads = new[] { index, status, actions, collectionPlan };

        var handoffPath = Path.Combine(projectRoot, ReleaseEvidenceHandoffRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var handoffExists = File.Exists(handoffPath);
        var handoffContentRead = false;
        var handoffContent = string.Empty;
        if (handoffExists)
        {
            try
            {
                handoffContent = File.ReadAllText(handoffPath);
                handoffContentRead = true;
            }
            catch (IOException)
            {
                handoffContentRead = false;
            }
            catch (UnauthorizedAccessException)
            {
                handoffContentRead = false;
            }
        }

        var presentFiles = reads.Count(read => read.Exists) + (handoffExists ? 1 : 0);
        var missingFiles = reads.Count(read => !read.Exists) + (handoffExists ? 0 : 1);
        var malformedFiles = reads.Count(read => read.Exists && (!read.Parsed || !read.IdentityValid)) +
            (handoffExists && !handoffContentRead ? 1 : 0);
        var contentRead = reads.Any(read => read.ContentRead) || handoffContentRead;

        if (missingFiles > 0)
        {
            return DryRunCrossLinkEvidence(
                "missing",
                contentRead,
                presentFiles,
                missingFiles,
                malformedFiles,
                "release-dry-run-cross-link-files-missing");
        }

        if (malformedFiles > 0 ||
            index.Root is null ||
            status.Root is null ||
            actions.Root is null ||
            collectionPlan.Root is null)
        {
            return DryRunCrossLinkEvidence(
                "malformed",
                contentRead,
                presentFiles,
                missingFiles,
                malformedFiles,
                "release-dry-run-cross-link-files-malformed");
        }

        var expectedLinks = 20;
        var validLinks = CountValidDryRunEvidenceLinks(index.Root, status.Root, actions.Root, collectionPlan.Root, handoffContent);
        var mismatchedLinks = expectedLinks - validLinks;
        var identityConsistent = reads.All(read => read.IdentityValid);
        var indexEntriesRead = TryReadRequiredEvidence(index.Root, out var indexEntries);
        var statusEntriesRead = TryReadRequiredEvidence(status.Root, out var statusEntries);
        var requiredEvidenceConsistent = indexEntriesRead &&
            statusEntriesRead &&
            RequiredEvidenceEntriesConsistent(indexEntries, statusEntries) &&
            ReleasePublishConsumesConsistent(index.Root, indexEntries) &&
            ReleasePublishConsumesConsistent(collectionPlan.Root, indexEntries);
        var statusEntryCount = statusEntries?.Count ?? 0;
        var requiredEvidenceEntryCount = indexEntries?.Count ?? 0;
        var actionsRead = TryReadActions(actions.Root, out var actionShapes);
        var actionReferencesConsistent = actionsRead &&
            indexEntries is not null &&
            ActionsConsistentWithEvidence(actionShapes, indexEntries);
        var actionCount = actionShapes?.Count ?? 0;
        var collectionStepsRead = TryReadCollectionSteps(collectionPlan.Root, out var stepShapes);
        var collectionStepsConsistent = collectionStepsRead &&
            indexEntries is not null &&
            actionShapes is not null &&
            CollectionStepsConsistentWithEvidence(stepShapes, indexEntries, actionShapes);
        var collectionStepCount = stepShapes?.Count ?? 0;
        var indexSummaryRead = TryReadEvidenceSummary(index.Root, stepsRequired: true, out var indexSummary);
        var statusSummaryRead = TryReadEvidenceSummary(status.Root, stepsRequired: false, out var statusSummary);
        var actionsSummaryRead = TryReadEvidenceSummary(actions.Root, stepsRequired: false, out var actionsSummary);
        var planSummaryRead = TryReadEvidenceSummary(collectionPlan.Root, stepsRequired: true, out var planSummary);
        var summaryCountersConsistent = indexSummaryRead &&
            statusSummaryRead &&
            actionsSummaryRead &&
            planSummaryRead &&
            indexEntries is not null &&
            actionShapes is not null &&
            stepShapes is not null &&
            EvidenceSummariesConsistent(indexSummary, statusSummary, actionsSummary, planSummary, indexEntries, actionShapes, stepShapes);
        var executionBoundariesConsistent =
            index.Root["execution"] is JsonObject indexExecution &&
            status.Root["execution"] is JsonObject statusExecution &&
            actions.Root["execution"] is JsonObject actionsExecution &&
            collectionPlan.Root["execution"] is JsonObject planExecution &&
            HasCollectionPlanNoExecutionBoundary(indexExecution) &&
            HasCollectionPlanNoExecutionBoundary(statusExecution) &&
            HasCollectionPlanNoExecutionBoundary(actionsExecution) &&
            HasCollectionPlanNoExecutionBoundary(planExecution);
        var handoffReferencesConsistent = HandoffReferencesConsistent(handoffContent, indexEntries, actionShapes, stepShapes);
        var statusText = ResolveDryRunCrossLinkEvidenceStatus(
            mismatchedLinks,
            identityConsistent,
            summaryCountersConsistent,
            requiredEvidenceConsistent,
            actionReferencesConsistent,
            collectionStepsConsistent,
            executionBoundariesConsistent,
            handoffReferencesConsistent);
        var detail = ResolveDryRunCrossLinkEvidenceDetail(statusText);

        return new ReleasePublishDryRunCrossLinkEvidence(
            statusText,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            ExpectedFiles: 5,
            presentFiles,
            missingFiles,
            malformedFiles,
            expectedLinks,
            validLinks,
            mismatchedLinks,
            requiredEvidenceEntryCount,
            statusEntryCount,
            actionCount,
            collectionStepCount,
            identityConsistent,
            summaryCountersConsistent,
            requiredEvidenceConsistent,
            actionReferencesConsistent,
            collectionStepsConsistent,
            executionBoundariesConsistent,
            handoffReferencesConsistent,
            detail);
    }

    private static ReleasePublishDryRunCrossLinkEvidence DryRunCrossLinkEvidence(
        string status,
        bool contentRead,
        int presentFiles,
        int missingFiles,
        int malformedFiles,
        string detail) =>
        new(
            status,
            CheckedInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            ExpectedFiles: 5,
            presentFiles,
            missingFiles,
            malformedFiles,
            ExpectedLinks: 20,
            ValidLinks: 0,
            MismatchedLinks: 20,
            RequiredEvidenceEntries: 0,
            StatusEntries: 0,
            ActionItems: 0,
            CollectionSteps: 0,
            IdentityConsistent: false,
            SummaryCountersConsistent: false,
            RequiredEvidenceConsistent: false,
            ActionReferencesConsistent: false,
            CollectionStepsConsistent: false,
            ExecutionBoundariesConsistent: false,
            HandoffReferencesConsistent: false,
            detail);

    private static ReleasePublishDryRunEvidenceRemediation CreateDryRunEvidenceRemediation(
        ReleasePublishDryRunCrossLinkEvidence evidence)
    {
        var items = new List<ReleasePublishDryRunEvidenceRemediationItem>();
        if (StringComparer.Ordinal.Equals(evidence.Status, "missing") || evidence.MissingFiles > 0)
        {
            items.Add(DryRunEvidenceRemediationItem(
                "restore-release-dry-run-evidence-files",
                "missing-files",
                "Generate the missing release dry-run evidence files before reviewing publish readiness."));
        }

        if (StringComparer.Ordinal.Equals(evidence.Status, "malformed") || evidence.MalformedFiles > 0)
        {
            items.Add(DryRunEvidenceRemediationItem(
                "regenerate-malformed-release-dry-run-evidence",
                "malformed-files",
                "Regenerate malformed release dry-run evidence files before reviewing publish readiness."));
        }

        var filesReadable = evidence.MissingFiles == 0 && evidence.MalformedFiles == 0;
        if (StringComparer.Ordinal.Equals(evidence.Status, "cross-link-mismatch") ||
            (filesReadable && evidence.MismatchedLinks > 0))
        {
            items.Add(DryRunEvidenceRemediationItem(
                "regenerate-release-dry-run-evidence-cross-links",
                "cross-link-mismatch",
                "Regenerate release dry-run evidence so index, status, actions, collection plan, and handoff agree."));
        }

        if (!StringComparer.Ordinal.Equals(evidence.Status, "complete-cross-links-validated") && items.Count == 0)
        {
            items.Add(DryRunEvidenceRemediationItem(
                "review-release-dry-run-evidence",
                "unknown",
                "Review release dry-run evidence status before reviewing publish readiness."));
        }

        var requiresOperatorAction = items.Count > 0;
        var targetPaths = requiresOperatorAction ? ReleaseDryRunEvidenceRelativePaths : Array.Empty<string>();
        return new ReleasePublishDryRunEvidenceRemediation(
            Status: requiresOperatorAction ? "action-required" : "no-action-required",
            CheckedInCurrentGate: true,
            RequiresOperatorAction: requiresOperatorAction,
            SourceStatus: evidence.Status,
            ActionItems: items.Count,
            CommandHints: items.Select(item => item.CommandHint).Distinct(StringComparer.Ordinal).Count(),
            AffectedPaths: targetPaths.Length,
            BlockingIssues: items.Count(item => item.BlocksPublishReadiness),
            RecommendedCommand: ReleaseVerifyEvidenceCommandHint,
            Detail: requiresOperatorAction
                ? "release-dry-run-evidence-remediation-required"
                : "release-dry-run-evidence-remediation-not-required",
            TargetPaths: targetPaths,
            Items: items);
    }

    private static ReleasePublishDryRunEvidenceRemediationItem DryRunEvidenceRemediationItem(
        string id,
        string category,
        string reason) =>
        new(
            Id: id,
            Priority: "blocker",
            Category: category,
            Status: "open",
            Reason: reason,
            CommandHint: ReleaseVerifyEvidenceCommandHint,
            Execution: "manual",
            BlocksPublishReadiness: true,
            TargetPaths: ReleaseDryRunEvidenceRelativePaths);

    private static ReleaseDryRunEvidenceJsonRead ReadReleaseDryRunEvidenceJson(
        string projectRoot,
        string relativePath,
        string expectedKind,
        string expectedStatus)
    {
        var fullPath = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return new ReleaseDryRunEvidenceJsonRead(displayPath, Exists: false, ContentRead: false, Parsed: false, IdentityValid: false, Root: null, "missing");
        }

        JsonObject? root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
        }
        catch (JsonException ex)
        {
            return new ReleaseDryRunEvidenceJsonRead(displayPath, Exists: true, ContentRead: true, Parsed: false, IdentityValid: false, Root: null, $"json-parse-error:{ex.Message}");
        }
        catch (IOException)
        {
            return new ReleaseDryRunEvidenceJsonRead(displayPath, Exists: true, ContentRead: false, Parsed: false, IdentityValid: false, Root: null, "unreadable");
        }
        catch (UnauthorizedAccessException)
        {
            return new ReleaseDryRunEvidenceJsonRead(displayPath, Exists: true, ContentRead: false, Parsed: false, IdentityValid: false, Root: null, "unreadable");
        }

        if (root is null)
        {
            return new ReleaseDryRunEvidenceJsonRead(displayPath, Exists: true, ContentRead: true, Parsed: false, IdentityValid: false, Root: null, "not-json-object");
        }

        var identityValid =
            StringPropertyEquals(root, "formatVersion", "0.1") &&
            StringPropertyEquals(root, "kind", expectedKind) &&
            StringPropertyEquals(root, "command", "release verify") &&
            BooleanPropertyEquals(root, "dryRun", expected: true) &&
            StringPropertyEquals(root, "status", expectedStatus) &&
            StringPropertyEquals(root, "outputRoot", "dist/release-dry-run");

        return new ReleaseDryRunEvidenceJsonRead(
            displayPath,
            Exists: true,
            ContentRead: true,
            Parsed: true,
            identityValid,
            root,
            identityValid ? "identity-valid" : "identity-mismatch");
    }

    private static int CountValidDryRunEvidenceLinks(
        JsonObject index,
        JsonObject status,
        JsonObject actions,
        JsonObject collectionPlan,
        string handoffContent)
    {
        var valid = 0;
        valid += CountLink(index, "statusProjection", ReleaseEvidenceStatusRelativePath);
        valid += CountLink(index, "actionChecklist", ReleaseEvidenceActionsRelativePath);
        valid += CountLink(index, "collectionPlan", CollectionPlanEvidenceRelativePath);
        valid += CountLink(index, "handoffSummary", ReleaseEvidenceHandoffRelativePath);
        valid += CountLink(status, "evidenceIndex", ReleaseEvidenceIndexRelativePath);
        valid += CountLink(status, "actionChecklist", ReleaseEvidenceActionsRelativePath);
        valid += CountLink(status, "collectionPlan", CollectionPlanEvidenceRelativePath);
        valid += CountLink(status, "handoffSummary", ReleaseEvidenceHandoffRelativePath);
        valid += CountLink(actions, "evidenceIndex", ReleaseEvidenceIndexRelativePath);
        valid += CountLink(actions, "evidenceStatus", ReleaseEvidenceStatusRelativePath);
        valid += CountLink(actions, "collectionPlan", CollectionPlanEvidenceRelativePath);
        valid += CountLink(actions, "handoffSummary", ReleaseEvidenceHandoffRelativePath);
        valid += CountLink(collectionPlan, "evidenceIndex", ReleaseEvidenceIndexRelativePath);
        valid += CountLink(collectionPlan, "evidenceStatus", ReleaseEvidenceStatusRelativePath);
        valid += CountLink(collectionPlan, "actionChecklist", ReleaseEvidenceActionsRelativePath);
        valid += CountLink(collectionPlan, "handoffSummary", ReleaseEvidenceHandoffRelativePath);
        valid += handoffContent.Contains($"Evidence index: `{ReleaseEvidenceIndexRelativePath}`", StringComparison.Ordinal) ? 1 : 0;
        valid += handoffContent.Contains($"Evidence status: `{ReleaseEvidenceStatusRelativePath}`", StringComparison.Ordinal) ? 1 : 0;
        valid += handoffContent.Contains($"Action checklist: `{ReleaseEvidenceActionsRelativePath}`", StringComparison.Ordinal) ? 1 : 0;
        valid += handoffContent.Contains($"Collection plan: `{CollectionPlanEvidenceRelativePath}`", StringComparison.Ordinal) ? 1 : 0;
        return valid;
    }

    private static int CountLink(JsonObject source, string propertyName, string expectedPath) =>
        StringPropertyEquals(source, propertyName, expectedPath) ? 1 : 0;

    private static bool TryReadRequiredEvidence(JsonObject root, out IReadOnlyList<ReleaseDryRunEvidenceEntryShape> entries)
    {
        entries = [];
        if (root["requiredEvidence"] is not JsonArray array)
        {
            return false;
        }

        var parsedEntries = new List<ReleaseDryRunEvidenceEntryShape>();
        foreach (var node in array)
        {
            if (node is not JsonObject entry ||
                !TryGetStringProperty(entry, "id", out var id) ||
                !TryGetStringProperty(entry, "path", out var path) ||
                !TryGetBooleanProperty(entry, "exists", out var exists) ||
                !TryGetStringProperty(entry, "status", out var status))
            {
                entries = [];
                return false;
            }

            parsedEntries.Add(new ReleaseDryRunEvidenceEntryShape(id, path, exists, status));
        }

        entries = parsedEntries;
        return true;
    }

    private static bool RequiredEvidenceEntriesConsistent(
        IReadOnlyList<ReleaseDryRunEvidenceEntryShape> indexEntries,
        IReadOnlyList<ReleaseDryRunEvidenceEntryShape> statusEntries)
    {
        if (HasDuplicateStrings(indexEntries.Select(entry => entry.Id)) ||
            HasDuplicateStrings(statusEntries.Select(entry => entry.Id)))
        {
            return false;
        }

        var expectedPathsByEvidenceId = CollectionPlanExpectedSteps.ToDictionary(step => step.EvidenceId, step => step.TargetPath, StringComparer.Ordinal);
        var indexMap = indexEntries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        var statusMap = statusEntries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        if (indexMap.Count != expectedPathsByEvidenceId.Count || statusMap.Count != expectedPathsByEvidenceId.Count)
        {
            return false;
        }

        foreach (var (evidenceId, expectedPath) in expectedPathsByEvidenceId)
        {
            if (!indexMap.TryGetValue(evidenceId, out var indexEntry) ||
                !statusMap.TryGetValue(evidenceId, out var statusEntry) ||
                !StringComparer.Ordinal.Equals(indexEntry.Path, expectedPath) ||
                !StringComparer.Ordinal.Equals(statusEntry.Path, expectedPath) ||
                indexEntry.Exists != statusEntry.Exists ||
                !StringComparer.Ordinal.Equals(indexEntry.Status, statusEntry.Status))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ReleasePublishConsumesConsistent(JsonObject root, IReadOnlyList<ReleaseDryRunEvidenceEntryShape> entries)
    {
        if (root["releasePublishPreflight"] is not JsonObject preflight ||
            !StringPropertyEquals(preflight, "commandHint", "forge release publish <project-root> --dry-run --format json --no-input") ||
            !TryGetStringArrayProperty(preflight, "consumes", out var consumes))
        {
            return false;
        }

        return SameStringSet(consumes, entries.Select(entry => entry.Path));
    }

    private static bool TryReadActions(JsonObject root, out IReadOnlyList<ReleaseDryRunEvidenceActionShape> actions)
    {
        actions = [];
        if (root["actions"] is not JsonArray array)
        {
            return false;
        }

        var parsedActions = new List<ReleaseDryRunEvidenceActionShape>();
        foreach (var node in array)
        {
            if (node is not JsonObject action ||
                !TryGetStringProperty(action, "id", out var id) ||
                !TryGetStringProperty(action, "evidenceId", out var evidenceId) ||
                !TryGetStringProperty(action, "targetPath", out var targetPath) ||
                !StringPropertyEquals(action, "status", "open") ||
                !StringPropertyEquals(action, "execution", "manual"))
            {
                actions = [];
                return false;
            }

            parsedActions.Add(new ReleaseDryRunEvidenceActionShape(id, evidenceId, targetPath));
        }

        actions = parsedActions;
        return true;
    }

    private static bool ActionsConsistentWithEvidence(
        IReadOnlyList<ReleaseDryRunEvidenceActionShape> actions,
        IReadOnlyList<ReleaseDryRunEvidenceEntryShape> entries)
    {
        if (HasDuplicateStrings(actions.Select(action => action.Id)) ||
            HasDuplicateStrings(entries.Select(entry => entry.Id)))
        {
            return false;
        }

        var entriesById = entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        var missingEntries = entries
            .Where(entry => !entry.Exists && StringComparer.Ordinal.Equals(entry.Status, "missing"))
            .ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        if (actions.Count != missingEntries.Count)
        {
            return false;
        }

        foreach (var action in actions)
        {
            if (!entriesById.TryGetValue(action.EvidenceId, out var entry) ||
                !missingEntries.ContainsKey(action.EvidenceId) ||
                !StringComparer.Ordinal.Equals(action.Id, $"produce-{action.EvidenceId}") ||
                !StringComparer.Ordinal.Equals(action.TargetPath, entry.Path))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryReadCollectionSteps(JsonObject root, out IReadOnlyList<ReleaseDryRunEvidenceCollectionStepShape> steps)
    {
        steps = [];
        if (root["steps"] is not JsonArray array)
        {
            return false;
        }

        var parsedSteps = new List<ReleaseDryRunEvidenceCollectionStepShape>();
        foreach (var node in array)
        {
            if (node is not JsonObject step ||
                !TryGetInt32Property(step, "order", out var order) ||
                !TryGetStringProperty(step, "id", out var id) ||
                !TryGetStringProperty(step, "evidenceId", out var evidenceId) ||
                !TryGetStringProperty(step, "targetPath", out var targetPath) ||
                !TryGetStringProperty(step, "status", out var status) ||
                !TryGetStringProperty(step, "collectionMode", out var collectionMode) ||
                !TryGetStringProperty(step, "execution", out var execution))
            {
                steps = [];
                return false;
            }

            var actionId = TryGetStringProperty(step, "actionId", out var parsedActionId)
                ? parsedActionId
                : null;
            parsedSteps.Add(new ReleaseDryRunEvidenceCollectionStepShape(order, id, evidenceId, targetPath, status, collectionMode, execution, actionId));
        }

        steps = parsedSteps.OrderBy(step => step.Order).ThenBy(step => step.Id, StringComparer.Ordinal).ToArray();
        return true;
    }

    private static bool CollectionStepsConsistentWithEvidence(
        IReadOnlyList<ReleaseDryRunEvidenceCollectionStepShape> steps,
        IReadOnlyList<ReleaseDryRunEvidenceEntryShape> entries,
        IReadOnlyList<ReleaseDryRunEvidenceActionShape> actions)
    {
        if (steps.Count != CollectionPlanExpectedSteps.Length)
        {
            return false;
        }

        if (HasDuplicateStrings(steps.Select(step => step.Id)) ||
            HasDuplicateInts(steps.Select(step => step.Order)) ||
            HasDuplicateStrings(entries.Select(entry => entry.Id)) ||
            HasDuplicateStrings(actions.Select(action => action.Id)))
        {
            return false;
        }

        var entriesById = entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        var actionsById = actions.ToDictionary(action => action.Id, StringComparer.Ordinal);
        foreach (var expectedStep in CollectionPlanExpectedSteps)
        {
            var step = steps.FirstOrDefault(candidate => candidate.Order == expectedStep.Order);
            if (step is null ||
                !StringComparer.Ordinal.Equals(step.Id, expectedStep.Id) ||
                !StringComparer.Ordinal.Equals(step.EvidenceId, expectedStep.EvidenceId) ||
                !StringComparer.Ordinal.Equals(step.TargetPath, expectedStep.TargetPath) ||
                !entriesById.TryGetValue(step.EvidenceId, out var entry) ||
                !StringComparer.Ordinal.Equals(step.TargetPath, entry.Path))
            {
                return false;
            }

            if (StringComparer.Ordinal.Equals(step.Execution, "manual"))
            {
                if (!StringComparer.Ordinal.Equals(step.Status, "manual-required") ||
                    !StringComparer.Ordinal.Equals(step.CollectionMode, "manual-command-hint") ||
                    step.ActionId is null ||
                    !actionsById.TryGetValue(step.ActionId, out var action) ||
                    !StringComparer.Ordinal.Equals(action.EvidenceId, step.EvidenceId))
                {
                    return false;
                }
            }
            else if (!StringComparer.Ordinal.Equals(step.Execution, "none") ||
                step.ActionId is not null ||
                StringComparer.Ordinal.Equals(step.Status, "manual-required"))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryReadEvidenceSummary(JsonObject root, bool stepsRequired, out ReleaseDryRunEvidenceSummaryShape summary)
    {
        summary = new ReleaseDryRunEvidenceSummaryShape(0, 0, 0, 0, null, null, null);
        if (root["summary"] is not JsonObject summaryRoot ||
            !TryGetInt32Property(summaryRoot, "total", out var total) ||
            !TryGetInt32Property(summaryRoot, "present", out var present) ||
            !TryGetInt32Property(summaryRoot, "missing", out var missing) ||
            !TryGetInt32Property(summaryRoot, "actions", out var actions))
        {
            return false;
        }

        int? steps = null;
        int? manualSteps = null;
        int? availableSteps = null;
        if (stepsRequired)
        {
            if (!TryGetInt32Property(summaryRoot, "steps", out var parsedSteps) ||
                !TryGetInt32Property(summaryRoot, "manualSteps", out var parsedManualSteps) ||
                !TryGetInt32Property(summaryRoot, "availableSteps", out var parsedAvailableSteps))
            {
                return false;
            }

            steps = parsedSteps;
            manualSteps = parsedManualSteps;
            availableSteps = parsedAvailableSteps;
        }

        summary = new ReleaseDryRunEvidenceSummaryShape(total, present, missing, actions, steps, manualSteps, availableSteps);
        return true;
    }

    private static bool EvidenceSummariesConsistent(
        ReleaseDryRunEvidenceSummaryShape indexSummary,
        ReleaseDryRunEvidenceSummaryShape statusSummary,
        ReleaseDryRunEvidenceSummaryShape actionsSummary,
        ReleaseDryRunEvidenceSummaryShape planSummary,
        IReadOnlyList<ReleaseDryRunEvidenceEntryShape> entries,
        IReadOnlyList<ReleaseDryRunEvidenceActionShape> actions,
        IReadOnlyList<ReleaseDryRunEvidenceCollectionStepShape> steps)
    {
        var expectedTotal = entries.Count;
        var expectedPresent = entries.Count(entry => entry.Exists);
        var expectedMissing = entries.Count(entry => !entry.Exists);
        var expectedActions = actions.Count;
        var commonSummaries = new[] { indexSummary, statusSummary, actionsSummary, planSummary };
        if (commonSummaries.Any(summary =>
                summary.Total != expectedTotal ||
                summary.Present != expectedPresent ||
                summary.Missing != expectedMissing ||
                summary.Actions != expectedActions))
        {
            return false;
        }

        return indexSummary.Steps == steps.Count &&
            planSummary.Steps == steps.Count &&
            indexSummary.ManualSteps == steps.Count(step => StringComparer.Ordinal.Equals(step.Execution, "manual")) &&
            planSummary.ManualSteps == steps.Count(step => StringComparer.Ordinal.Equals(step.Execution, "manual")) &&
            indexSummary.AvailableSteps == steps.Count(step => StringComparer.Ordinal.Equals(step.Status, "available")) &&
            planSummary.AvailableSteps == steps.Count(step => StringComparer.Ordinal.Equals(step.Status, "available"));
    }

    private static bool HandoffReferencesConsistent(
        string handoffContent,
        IReadOnlyList<ReleaseDryRunEvidenceEntryShape>? entries,
        IReadOnlyList<ReleaseDryRunEvidenceActionShape>? actions,
        IReadOnlyList<ReleaseDryRunEvidenceCollectionStepShape>? steps)
    {
        if (entries is null || actions is null || steps is null)
        {
            return false;
        }

        if (!handoffContent.Contains($"Evidence index: `{ReleaseEvidenceIndexRelativePath}`", StringComparison.Ordinal) ||
            !handoffContent.Contains($"Evidence status: `{ReleaseEvidenceStatusRelativePath}`", StringComparison.Ordinal) ||
            !handoffContent.Contains($"Action checklist: `{ReleaseEvidenceActionsRelativePath}`", StringComparison.Ordinal) ||
            !handoffContent.Contains($"Collection plan: `{CollectionPlanEvidenceRelativePath}`", StringComparison.Ordinal))
        {
            return false;
        }

        return entries.All(entry => handoffContent.Contains($"| `{entry.Id}` | `{entry.Status}` | `{entry.Path}` |", StringComparison.Ordinal)) &&
            actions.All(action => handoffContent.Contains($"| `{action.Id}` | `{action.TargetPath}` |", StringComparison.Ordinal)) &&
            steps.All(step => handoffContent.Contains($"| {step.Order} | `{step.Id}` | `{step.Status}` | `{step.CollectionMode}` | `{step.Execution}` | `{step.TargetPath}` |", StringComparison.Ordinal));
    }

    private static string ResolveDryRunCrossLinkEvidenceStatus(
        int mismatchedLinks,
        bool identityConsistent,
        bool summaryCountersConsistent,
        bool requiredEvidenceConsistent,
        bool actionReferencesConsistent,
        bool collectionStepsConsistent,
        bool executionBoundariesConsistent,
        bool handoffReferencesConsistent) =>
        mismatchedLinks == 0 &&
        identityConsistent &&
        summaryCountersConsistent &&
        requiredEvidenceConsistent &&
        actionReferencesConsistent &&
        collectionStepsConsistent &&
        executionBoundariesConsistent &&
        handoffReferencesConsistent
            ? "complete-cross-links-validated"
            : "cross-link-mismatch";

    private static string ResolveDryRunCrossLinkEvidenceDetail(string status) => status switch
    {
        "complete-cross-links-validated" => "release-dry-run-evidence-cross-links-consistent",
        "cross-link-mismatch" => "release-dry-run-evidence-cross-links-mismatch",
        "missing" => "release-dry-run-cross-link-files-missing",
        "malformed" => "release-dry-run-cross-link-files-malformed",
        _ => status
    };

    private static bool SameStringSet(IEnumerable<string> left, IEnumerable<string> right)
    {
        var leftSet = new HashSet<string>(left, StringComparer.Ordinal);
        var rightSet = new HashSet<string>(right, StringComparer.Ordinal);
        return leftSet.SetEquals(rightSet);
    }

    private static bool HasDuplicateStrings(IEnumerable<string> values)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            if (!seen.Add(value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDuplicateInts(IEnumerable<int> values)
    {
        var seen = new HashSet<int>();
        foreach (var value in values)
        {
            if (!seen.Add(value))
            {
                return true;
            }
        }

        return false;
    }

    private static ReleasePublishApprovalRequirement CreateApprovalRequirement(
        string projectRoot,
        bool yesProvided,
        string? confirmationValue)
    {
        if (!yesProvided && string.IsNullOrWhiteSpace(confirmationValue))
        {
            return ApprovalRequirement(
                provided: false,
                yesProvided,
                confirmationValue,
                confirmationValidated: false,
                confirmationMatches: null,
                projectManifestRead: false,
                projectManifestPath: null,
                projectId: null,
                "missing",
                "approval-not-provided");
        }

        if (!yesProvided || string.IsNullOrWhiteSpace(confirmationValue))
        {
            return ApprovalRequirement(
                provided: false,
                yesProvided,
                confirmationValue,
                confirmationValidated: false,
                confirmationMatches: false,
                projectManifestRead: false,
                projectManifestPath: null,
                projectId: null,
                "incomplete",
                "approval-requires-yes-and-confirm");
        }

        var manifestCandidates = FindProjectManifestCandidates(projectRoot);
        if (manifestCandidates.Count == 0)
        {
            return ApprovalRequirement(
                provided: false,
                yesProvided,
                confirmationValue,
                confirmationValidated: true,
                confirmationMatches: false,
                projectManifestRead: false,
                projectManifestPath: null,
                projectId: null,
                "project-manifest-missing",
                "project-manifest-required-for-approval");
        }

        if (manifestCandidates.Count > 1)
        {
            return ApprovalRequirement(
                provided: false,
                yesProvided,
                confirmationValue,
                confirmationValidated: true,
                confirmationMatches: false,
                projectManifestRead: false,
                ToDisplayPath(projectRoot, manifestCandidates[0]),
                projectId: null,
                "multiple-project-manifests",
                "multiple-project-manifests-found");
        }

        var manifestPath = manifestCandidates[0];
        var readResult = ReadApprovalProjectId(manifestPath);
        if (readResult.Error is not null)
        {
            return ApprovalRequirement(
                provided: false,
                yesProvided,
                confirmationValue,
                confirmationValidated: true,
                confirmationMatches: false,
                projectManifestRead: true,
                ToDisplayPath(projectRoot, manifestPath),
                projectId: null,
                readResult.Status ?? "project-manifest-unreadable",
                readResult.Error);
        }

        var projectId = readResult.ProjectId;
        if (!LogicalId.TryParse(projectId, out _))
        {
            return ApprovalRequirement(
                provided: false,
                yesProvided,
                confirmationValue,
                confirmationValidated: true,
                confirmationMatches: false,
                projectManifestRead: true,
                ToDisplayPath(projectRoot, manifestPath),
                projectId,
                "project-id-invalid",
                "project-manifest-id-invalid");
        }

        var matches = StringComparer.Ordinal.Equals(projectId, confirmationValue);
        return ApprovalRequirement(
            provided: matches,
            yesProvided,
            confirmationValue,
            confirmationValidated: true,
            confirmationMatches: matches,
            projectManifestRead: true,
            ToDisplayPath(projectRoot, manifestPath),
            projectId,
            matches ? "provided" : "project-id-mismatch",
            matches ? "approval-confirmation-matches-project-id" : "approval-confirmation-does-not-match-project-id");
    }

    private static ReleasePublishApprovalRequirement ApprovalRequirement(
        bool provided,
        bool yesProvided,
        string? confirmationValue,
        bool confirmationValidated,
        bool? confirmationMatches,
        bool projectManifestRead,
        string? projectManifestPath,
        string? projectId,
        string status,
        string detail) =>
        new(
            true,
            provided,
            yesProvided,
            string.IsNullOrWhiteSpace(confirmationValue) ? null : confirmationValue,
            confirmationValidated,
            confirmationMatches,
            projectManifestRead,
            projectManifestPath,
            projectId,
            status,
            "A human operator must explicitly approve publish with --yes and --confirm <project-id> after local evidence and governance checks pass.",
            detail);

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

    private static ReleasePublishApprovalProjectIdReadResult ReadApprovalProjectId(string manifestPath)
    {
        try
        {
            var extension = Path.GetExtension(manifestPath);
            var projectId = extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
                ? ReadJsonApprovalProjectId(manifestPath)
                : ReadYamlApprovalProjectId(manifestPath);

            return string.IsNullOrWhiteSpace(projectId)
                ? new ReleasePublishApprovalProjectIdReadResult(null, "project-id-missing", "project-manifest-id-missing")
                : new ReleasePublishApprovalProjectIdReadResult(projectId, null, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or YamlException or InvalidOperationException)
        {
            return new ReleasePublishApprovalProjectIdReadResult(null, "project-manifest-unreadable", $"project-manifest-unreadable:{ex.Message}");
        }
    }

    private static string? ReadJsonApprovalProjectId(string manifestPath)
    {
        var root = JsonNode.Parse(File.ReadAllText(manifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Manifest root must be a JSON object.");
        return root["id"]?.GetValue<string>();
    }

    private static string? ReadYamlApprovalProjectId(string manifestPath)
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

    private static ReleasePublishGovernanceCheck GovernanceCheck(
        string id,
        string title,
        string status,
        string evidencePath,
        string detail) =>
        new(
            id,
            title,
            status,
            Required: true,
            CheckedInCurrentGate: true,
            evidencePath,
            detail);

    private static ReleasePublishGovernanceCheck PolicyFileCheck(
        string id,
        string title,
        string projectRoot,
        string relativePath,
        IReadOnlyList<string> requiredTerms)
    {
        var fullPath = Path.Combine(projectRoot, relativePath);
        var evidencePath = relativePath.Replace('\\', '/');
        if (!File.Exists(fullPath))
        {
            return GovernanceCheck(id, title, "missing-local-evidence", evidencePath, "local-policy-file-missing");
        }

        if (!TryReadText(fullPath, out var content))
        {
            return GovernanceCheck(id, title, "failed", evidencePath, "local-policy-file-unreadable");
        }

        var passed = requiredTerms.All(term => ContainsOrdinalIgnoreCase(content, term));
        return GovernanceCheck(
            id,
            title,
            passed ? "passed" : "failed",
            ToDisplayPath(projectRoot, fullPath),
            passed ? "local-policy-evidence-matched" : "local-policy-required-terms-missing");
    }

    private static ReleasePublishGovernanceCheck ToolVersionSemVerCheck()
    {
        var passed = IsSemVer(CliConstants.Version);
        return GovernanceCheck(
            "semver-version-stream",
            "Release version follows SemVer-governed streams",
            passed ? "passed" : "failed",
            "tool-version",
            passed ? $"tool-version-semver:{CliConstants.Version}" : $"tool-version-not-semver:{CliConstants.Version}");
    }

    private static ReleasePublishGovernanceCheck ReleaseWorkflowPermissionsCheck(string projectRoot)
    {
        var workflowRoot = Path.Combine(projectRoot, ".github", "workflows");
        if (!Directory.Exists(workflowRoot))
        {
            return GovernanceCheck(
                "least-privilege-release-permissions",
                "Release workflow uses least-privilege permissions",
                "missing-local-evidence",
                ".github/workflows",
                "workflow-directory-missing");
        }

        var workflowFiles = Directory.EnumerateFiles(workflowRoot, "*.yml")
            .Concat(Directory.EnumerateFiles(workflowRoot, "*.yaml"))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (workflowFiles.Length == 0)
        {
            return GovernanceCheck(
                "least-privilege-release-permissions",
                "Release workflow uses least-privilege permissions",
                "missing-local-evidence",
                ".github/workflows",
                "workflow-file-missing");
        }

        var allReadable = true;
        var allDeclarePermissions = true;
        var broadWritePermission = false;
        foreach (var workflowFile in workflowFiles)
        {
            if (!TryReadText(workflowFile, out var content))
            {
                allReadable = false;
                continue;
            }

            allDeclarePermissions &= ContainsOrdinalIgnoreCase(content, "permissions:");
            broadWritePermission |=
                ContainsOrdinalIgnoreCase(content, "write-all") ||
                ContainsOrdinalIgnoreCase(content, "contents: write") ||
                ContainsOrdinalIgnoreCase(content, "contents:write");
        }

        var passed = allReadable && allDeclarePermissions && !broadWritePermission;
        return GovernanceCheck(
            "least-privilege-release-permissions",
            "Release workflow uses least-privilege permissions",
            passed ? "passed" : "failed",
            ToDisplayPath(projectRoot, workflowFiles[0]),
            passed ? "workflow-permissions-are-scoped" : "workflow-permissions-not-least-privilege");
    }

    private static ReleasePublishGovernanceCheck CodeownersCheck(string projectRoot)
    {
        var candidatePaths = new[]
        {
            Path.Combine(projectRoot, ".github", "CODEOWNERS"),
            Path.Combine(projectRoot, "CODEOWNERS")
        };
        var codeownersPath = candidatePaths.FirstOrDefault(File.Exists);
        if (codeownersPath is null)
        {
            return GovernanceCheck(
                "codeowner-sensitive-path-review",
                "Sensitive release paths have required review",
                "missing-local-evidence",
                ".github/CODEOWNERS",
                "codeowners-file-missing");
        }

        if (!TryReadText(codeownersPath, out var content))
        {
            return GovernanceCheck(
                "codeowner-sensitive-path-review",
                "Sensitive release paths have required review",
                "failed",
                ToDisplayPath(projectRoot, codeownersPath),
                "codeowners-file-unreadable");
        }

        var meaningfulLines = content
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .ToArray();
        var hasGovernanceOwner = meaningfulLines.Any(line => ContainsOrdinalIgnoreCase(line, "docs/governance"));
        var hasWorkflowOwner = meaningfulLines.Any(line => ContainsOrdinalIgnoreCase(line, ".github"));
        var hasSchemaOwner = meaningfulLines.Any(line => ContainsOrdinalIgnoreCase(line, "schemas"));
        var passed = hasGovernanceOwner && hasWorkflowOwner && hasSchemaOwner;
        return GovernanceCheck(
            "codeowner-sensitive-path-review",
            "Sensitive release paths have required review",
            passed ? "passed" : "failed",
            ToDisplayPath(projectRoot, codeownersPath),
            passed ? "sensitive-path-codeowners-present" : "sensitive-path-codeowners-incomplete");
    }

    private static ReleasePublishGovernanceCheck AiOptionalReleasePathCheck(string projectRoot)
    {
        var candidatePaths = new[]
        {
            Path.Combine(projectRoot, "AGENTS.md"),
            Path.Combine(projectRoot, "docs", "governance", "release-policy.md"),
            Path.Combine(projectRoot, "docs", "governance", "dependency-policy.md")
        };

        var existingPaths = candidatePaths.Where(File.Exists).ToArray();
        if (existingPaths.Length == 0)
        {
            return GovernanceCheck(
                "ai-optional-release-path",
                "Release correctness path does not require AI",
                "missing-local-evidence",
                "AGENTS.md",
                "ai-optional-policy-evidence-missing");
        }

        foreach (var path in existingPaths)
        {
            if (!TryReadText(path, out var content))
            {
                continue;
            }

            var containsAi = ContainsOrdinalIgnoreCase(content, "AI");
            var containsOptionalBoundary =
                ContainsOrdinalIgnoreCase(content, "optional") ||
                ContainsOrdinalIgnoreCase(content, "does not require AI") ||
                ContainsOrdinalIgnoreCase(content, "do not require AI") ||
                ContainsOrdinalIgnoreCase(content, "without API keys");
            if (containsAi && containsOptionalBoundary)
            {
                return GovernanceCheck(
                    "ai-optional-release-path",
                    "Release correctness path does not require AI",
                    "passed",
                    ToDisplayPath(projectRoot, path),
                    "ai-optional-policy-evidence-matched");
            }
        }

        return GovernanceCheck(
            "ai-optional-release-path",
            "Release correctness path does not require AI",
            "failed",
            ToDisplayPath(projectRoot, existingPaths[0]),
            "ai-optional-policy-evidence-incomplete");
    }

    private static IReadOnlyList<ReleasePublishEvidenceArtifact> CreateLocalEvidenceArtifacts(string projectRoot, string releasePrepareEvidenceRoot) =>
    [
        JsonArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-staging-payload", "staging-payload", "staging/release-payload.json"),
        JsonArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-archive-plan", "release-archive-plan", "release-archive-plan.json"),
        BinaryArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-archive", "release-archive", "archives/release.zip"),
        JsonArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-archive-evidence", "release-archive-evidence", "release-archive-evidence.json"),
        JsonArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-release-plan", "release-plan", "release-plan.json"),
        JsonArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-release-summary", "release-summary", "release-summary.json"),
        JsonArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-build-manifest", "build-manifest", "build-manifest.json"),
        ChecksumsArtifact(projectRoot, releasePrepareEvidenceRoot, "release-prepare-checksums", "checksums", "checksums.sha256")
    ];

    private static ReleasePublishEvidenceArtifact JsonArtifact(string projectRoot, string releasePrepareEvidenceRoot, string id, string kind, string relativePath)
    {
        var fullPath = Path.Combine(releasePrepareEvidenceRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return MissingArtifact(projectRoot, fullPath, id, kind, "json");
        }

        var length = new FileInfo(fullPath).Length;
        try
        {
            var node = JsonNode.Parse(File.ReadAllText(fullPath));
            var isObject = node is JsonObject;
            return new ReleasePublishEvidenceArtifact(
                id,
                ToDisplayPath(projectRoot, fullPath),
                kind,
                ContentKind: "json",
                Required: true,
                Exists: true,
                Status: isObject ? "well-formed-not-validated" : "malformed",
                Length: length,
                ShapeCheckedInCurrentGate: true,
                ContentReadInCurrentGate: true,
                ShapeDetail: isObject ? "json-object" : "json-root-not-object");
        }
        catch (JsonException ex)
        {
            return new ReleasePublishEvidenceArtifact(
                id,
                ToDisplayPath(projectRoot, fullPath),
                kind,
                ContentKind: "json",
                Required: true,
                Exists: true,
                Status: "malformed",
                length,
                ShapeCheckedInCurrentGate: true,
                ContentReadInCurrentGate: true,
                ShapeDetail: $"json-parse-error:{ex.LineNumber}:{ex.BytePositionInLine}");
        }
    }

    private static ReleasePublishEvidenceArtifact ChecksumsArtifact(string projectRoot, string releasePrepareEvidenceRoot, string id, string kind, string relativePath)
    {
        var fullPath = Path.Combine(releasePrepareEvidenceRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return MissingArtifact(projectRoot, fullPath, id, kind, "sha256");
        }

        var length = new FileInfo(fullPath).Length;
        var lines = File.ReadAllLines(fullPath);
        var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        var wellFormed = nonEmptyLines.Length > 0 && nonEmptyLines.All(IsChecksumLineShape);
        return new ReleasePublishEvidenceArtifact(
            id,
            ToDisplayPath(projectRoot, fullPath),
            kind,
                ContentKind: "sha256",
                Required: true,
                Exists: true,
                Status: wellFormed ? "well-formed-not-validated" : "malformed",
                Length: length,
                ShapeCheckedInCurrentGate: true,
                ContentReadInCurrentGate: true,
                ShapeDetail: wellFormed ? "sha256-lines" : "sha256-line-shape-invalid");
    }

    private static ReleasePublishChecksumSidecar CreateChecksumSidecar(string projectRoot, string releasePrepareEvidenceRoot)
    {
        var fullPath = Path.Combine(releasePrepareEvidenceRoot, "checksums.sha256");
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return new ReleasePublishChecksumSidecar(
                displayPath,
                Exists: false,
                Status: "missing",
                EntryClassificationInCurrentGate: true,
                DigestRevalidationInCurrentGate: true,
                ExpectedEntries: ExpectedChecksumEntryPaths.Length,
                ParsedEntries: 0,
                CoveredExpectedEntries: 0,
                MissingExpectedEntries: ExpectedChecksumEntryPaths.Length,
                UnexpectedEntries: 0,
                DuplicateEntries: 0,
                MalformedEntries: 0,
                DigestRevalidatedEntries: 0,
                DigestMatchedEntries: 0,
                DigestMismatchedEntries: 0,
                MissingLocalFileEntries: 0,
                ExpectedPaths: CreateExpectedChecksumPathCoverage(releasePrepareEvidenceRoot, []),
                Entries: []);
        }

        var entries = new List<ReleasePublishChecksumEntry>();
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        var coveredExpectedPaths = new HashSet<string>(StringComparer.Ordinal);
        var unexpectedEntries = 0;
        var duplicateEntries = 0;
        var malformedEntries = 0;
        var lines = File.ReadAllLines(fullPath);

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!TryParseChecksumLineShape(line, out var sha256, out var path))
            {
                malformedEntries++;
                entries.Add(new ReleasePublishChecksumEntry(
                    LineNumber: index + 1,
                    Sha256: null,
                    ActualSha256: null,
                    Path: null,
                    Status: "malformed",
                    ExpectedPath: false,
                    DuplicatePath: false,
                    LocalFilePresent: false,
                    DigestRevalidatedInCurrentGate: false,
                    ShapeDetail: "sha256-line-shape-invalid"));
                continue;
            }

            var duplicate = !seenPaths.Add(path);
            var expected = ExpectedChecksumEntryPaths.Contains(path, StringComparer.Ordinal);
            var localFilePresent = false;
            string? actualSha256 = null;
            var digestRevalidated = false;
            var status = "matched-revalidated";
            if (duplicate)
            {
                duplicateEntries++;
                status = "duplicate-not-revalidated";
            }
            else if (!expected)
            {
                unexpectedEntries++;
                status = "unexpected-not-revalidated";
            }
            else
            {
                coveredExpectedPaths.Add(path);
                var entryFullPath = Path.Combine(releasePrepareEvidenceRoot, path);
                localFilePresent = File.Exists(entryFullPath);
                if (localFilePresent)
                {
                    actualSha256 = ComputeSha256(entryFullPath);
                    digestRevalidated = true;
                    status = StringComparer.Ordinal.Equals(sha256, actualSha256)
                        ? "matched-revalidated"
                        : "mismatched-revalidated";
                }
                else
                {
                    status = "missing-local-file-not-revalidated";
                }
            }

            entries.Add(new ReleasePublishChecksumEntry(
                LineNumber: index + 1,
                Sha256: sha256,
                ActualSha256: actualSha256,
                Path: path,
                Status: status,
                ExpectedPath: expected,
                DuplicatePath: duplicate,
                LocalFilePresent: localFilePresent,
                DigestRevalidatedInCurrentGate: digestRevalidated,
                ShapeDetail: "sha256-entry"));
        }

        var missingExpectedEntries = ExpectedChecksumEntryPaths.Length - coveredExpectedPaths.Count;
        var digestRevalidatedEntries = entries.Count(entry => entry.DigestRevalidatedInCurrentGate);
        var digestMatchedEntries = entries.Count(entry => StringComparer.Ordinal.Equals(entry.Status, "matched-revalidated"));
        var digestMismatchedEntries = entries.Count(entry => StringComparer.Ordinal.Equals(entry.Status, "mismatched-revalidated"));
        var missingLocalFileEntries = entries.Count(entry => StringComparer.Ordinal.Equals(entry.Status, "missing-local-file-not-revalidated"));
        var sidecarStatus = ChecksumSidecarStatus(
            entries.Count,
            missingExpectedEntries,
            unexpectedEntries,
            duplicateEntries,
            malformedEntries,
            digestMismatchedEntries,
            missingLocalFileEntries);

        return new ReleasePublishChecksumSidecar(
            displayPath,
            Exists: true,
            Status: sidecarStatus,
            EntryClassificationInCurrentGate: true,
            DigestRevalidationInCurrentGate: true,
            ExpectedEntries: ExpectedChecksumEntryPaths.Length,
            ParsedEntries: entries.Count,
            CoveredExpectedEntries: coveredExpectedPaths.Count,
            MissingExpectedEntries: missingExpectedEntries,
            UnexpectedEntries: unexpectedEntries,
            DuplicateEntries: duplicateEntries,
            MalformedEntries: malformedEntries,
            DigestRevalidatedEntries: digestRevalidatedEntries,
            DigestMatchedEntries: digestMatchedEntries,
            DigestMismatchedEntries: digestMismatchedEntries,
            MissingLocalFileEntries: missingLocalFileEntries,
            ExpectedPaths: CreateExpectedChecksumPathCoverage(releasePrepareEvidenceRoot, entries),
            Entries: entries);
    }

    private static IReadOnlyList<ReleasePublishChecksumExpectedPath> CreateExpectedChecksumPathCoverage(
        string releasePrepareEvidenceRoot,
        IReadOnlyList<ReleasePublishChecksumEntry> entries) =>
        ExpectedChecksumEntryPaths
            .Select(path =>
            {
                var entry = entries.FirstOrDefault(item =>
                    !item.DuplicatePath &&
                    StringComparer.Ordinal.Equals(item.Path, path));
                return new ReleasePublishChecksumExpectedPath(
                    path,
                    entry?.Status ?? "missing-entry-not-revalidated",
                    EntryPresent: entry is not null,
                    LocalFilePresent: File.Exists(Path.Combine(releasePrepareEvidenceRoot, path)),
                    DigestRevalidatedInCurrentGate: entry?.DigestRevalidatedInCurrentGate ?? false,
                    ExpectedSha256: entry?.Sha256,
                    ActualSha256: entry?.ActualSha256);
            })
            .ToArray();

    private static ReleasePublishBuildManifestCrossReference CreateBuildManifestCrossReference(
        string projectRoot,
        string releasePrepareEvidenceRoot,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar)
    {
        var fullPath = Path.Combine(releasePrepareEvidenceRoot, "build-manifest.json");
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return new ReleasePublishBuildManifestCrossReference(
                displayPath,
                Exists: false,
                Status: "missing",
                OutputCrossReferenceInCurrentGate: true,
                ContentReadInCurrentGate: false,
                DigestRevalidationInCurrentGate: true,
                ExpectedOutputCount: ExpectedBuildManifestOutputPaths.Length,
                ParsedOutputCount: 0,
                CoveredExpectedOutputCount: 0,
                MissingExpectedOutputCount: ExpectedBuildManifestOutputPaths.Length,
                MissingLocalArtifactOutputCount: ExpectedBuildManifestOutputPaths.Count(path => !LocalArtifactExists(localEvidenceArtifacts, path)),
                MissingChecksumEntryOutputCount: ExpectedBuildManifestOutputPaths.Count(path => !ChecksumEntryExists(checksumSidecar, ToChecksumSidecarPath(path))),
                UnexpectedOutputCount: 0,
                DuplicateOutputCount: 0,
                MalformedOutputCount: 0,
                DigestRevalidatedOutputCount: 0,
                DigestMatchedOutputCount: 0,
                DigestMismatchedOutputCount: 0,
                ExpectedPaths: CreateBuildManifestExpectedPathCoverage([], localEvidenceArtifacts, checksumSidecar),
                Outputs: []);
        }

        try
        {
            var root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
            var outputs = root?["outputs"] as JsonArray;
            if (outputs is null)
            {
                return MalformedBuildManifestCrossReference(projectRoot, fullPath, contentRead: true, malformedOutputCount: 1, localEvidenceArtifacts, checksumSidecar);
            }

            var entries = new List<ReleasePublishBuildManifestOutput>();
            var seenPaths = new HashSet<string>(StringComparer.Ordinal);
            var coveredExpectedPaths = new HashSet<string>(StringComparer.Ordinal);
            var unexpectedOutputs = 0;
            var duplicateOutputs = 0;
            var malformedOutputs = 0;

            for (var index = 0; index < outputs.Count; index++)
            {
                var output = outputs[index] as JsonObject;
                var path = output?["path"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(path))
                {
                    malformedOutputs++;
                    entries.Add(new ReleasePublishBuildManifestOutput(
                        Index: index,
                        Path: null,
                        ChecksumPath: null,
                        Sha256: null,
                        ActualSha256: null,
                        Status: "malformed",
                        ExpectedPath: false,
                        DuplicatePath: false,
                        LocalArtifactPresent: false,
                        ChecksumEntryPresent: false,
                        DigestRevalidatedInCurrentGate: false,
                        ShapeDetail: "build-manifest-output-path-missing"));
                    continue;
                }

                var checksumPath = ToChecksumSidecarPath(path);
                var duplicate = !seenPaths.Add(path);
                var expected = ExpectedBuildManifestOutputPaths.Contains(path, StringComparer.Ordinal);
                var localArtifactPresent = LocalArtifactExists(localEvidenceArtifacts, path);
                var checksumEntryPresent = ChecksumEntryExists(checksumSidecar, checksumPath);
                string? sha256 = null;
                string? actualSha256 = null;
                var digestRevalidated = false;
                if (expected && !duplicate && !TryGetSha256Property(output, out sha256))
                {
                    malformedOutputs++;
                    entries.Add(new ReleasePublishBuildManifestOutput(
                        Index: index,
                        Path: path,
                        ChecksumPath: checksumPath,
                        Sha256: null,
                        ActualSha256: null,
                        Status: "malformed",
                        ExpectedPath: expected,
                        DuplicatePath: duplicate,
                        LocalArtifactPresent: localArtifactPresent,
                        ChecksumEntryPresent: checksumEntryPresent,
                        DigestRevalidatedInCurrentGate: false,
                        ShapeDetail: "build-manifest-output-sha256-invalid"));
                    continue;
                }

                if (expected && !duplicate && localArtifactPresent)
                {
                    actualSha256 = ComputeSha256(Path.Combine(projectRoot, path));
                    digestRevalidated = true;
                }

                var outputStatus = BuildManifestOutputStatus(
                    duplicate,
                    expected,
                    localArtifactPresent,
                    checksumEntryPresent,
                    digestRevalidated,
                    sha256 is not null && StringComparer.Ordinal.Equals(sha256, actualSha256));

                if (duplicate)
                {
                    duplicateOutputs++;
                }
                else if (!expected)
                {
                    unexpectedOutputs++;
                }
                else
                {
                    coveredExpectedPaths.Add(path);
                }

                entries.Add(new ReleasePublishBuildManifestOutput(
                    Index: index,
                    Path: path,
                    ChecksumPath: checksumPath,
                    Sha256: sha256,
                    ActualSha256: actualSha256,
                    Status: outputStatus,
                    ExpectedPath: expected,
                    DuplicatePath: duplicate,
                    LocalArtifactPresent: localArtifactPresent,
                    ChecksumEntryPresent: checksumEntryPresent,
                    DigestRevalidatedInCurrentGate: digestRevalidated,
                    ShapeDetail: "build-manifest-output-path"));
            }

            var expectedPathCoverage = CreateBuildManifestExpectedPathCoverage(entries, localEvidenceArtifacts, checksumSidecar);
            var missingExpectedOutputs = ExpectedBuildManifestOutputPaths.Length - coveredExpectedPaths.Count;
            var missingLocalOutputs = expectedPathCoverage.Count(path => !path.LocalArtifactPresent);
            var missingChecksumOutputs = expectedPathCoverage.Count(path => !path.ChecksumEntryPresent);
            var digestRevalidatedOutputs = entries.Count(entry => entry.DigestRevalidatedInCurrentGate);
            var digestMatchedOutputs = entries.Count(entry => StringComparer.Ordinal.Equals(entry.Status, "matched-revalidated"));
            var digestMismatchedOutputs = entries.Count(entry => StringComparer.Ordinal.Equals(entry.Status, "mismatched-revalidated"));
            var status = BuildManifestCrossReferenceStatus(
                entries.Count,
                missingExpectedOutputs,
                missingLocalOutputs,
                missingChecksumOutputs,
                unexpectedOutputs,
                duplicateOutputs,
                malformedOutputs,
                digestMismatchedOutputs);

            return new ReleasePublishBuildManifestCrossReference(
                displayPath,
                Exists: true,
                Status: status,
                OutputCrossReferenceInCurrentGate: true,
                ContentReadInCurrentGate: true,
                DigestRevalidationInCurrentGate: true,
                ExpectedOutputCount: ExpectedBuildManifestOutputPaths.Length,
                ParsedOutputCount: entries.Count,
                CoveredExpectedOutputCount: coveredExpectedPaths.Count,
                MissingExpectedOutputCount: missingExpectedOutputs,
                MissingLocalArtifactOutputCount: missingLocalOutputs,
                MissingChecksumEntryOutputCount: missingChecksumOutputs,
                UnexpectedOutputCount: unexpectedOutputs,
                DuplicateOutputCount: duplicateOutputs,
                MalformedOutputCount: malformedOutputs,
                DigestRevalidatedOutputCount: digestRevalidatedOutputs,
                DigestMatchedOutputCount: digestMatchedOutputs,
                DigestMismatchedOutputCount: digestMismatchedOutputs,
                ExpectedPaths: expectedPathCoverage,
                Outputs: entries);
        }
        catch (JsonException)
        {
            return MalformedBuildManifestCrossReference(projectRoot, fullPath, contentRead: true, malformedOutputCount: 1, localEvidenceArtifacts, checksumSidecar);
        }
    }

    private static ReleasePublishBuildManifestCrossReference MalformedBuildManifestCrossReference(
        string projectRoot,
        string fullPath,
        bool contentRead,
        int malformedOutputCount,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar)
    {
        var expectedPathCoverage = CreateBuildManifestExpectedPathCoverage([], localEvidenceArtifacts, checksumSidecar);
        return new ReleasePublishBuildManifestCrossReference(
            ToDisplayPath(projectRoot, fullPath),
            Exists: true,
            Status: "malformed-not-validated",
            OutputCrossReferenceInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            DigestRevalidationInCurrentGate: true,
            ExpectedOutputCount: ExpectedBuildManifestOutputPaths.Length,
            ParsedOutputCount: 0,
            CoveredExpectedOutputCount: 0,
            MissingExpectedOutputCount: ExpectedBuildManifestOutputPaths.Length,
            MissingLocalArtifactOutputCount: expectedPathCoverage.Count(path => !path.LocalArtifactPresent),
            MissingChecksumEntryOutputCount: expectedPathCoverage.Count(path => !path.ChecksumEntryPresent),
            UnexpectedOutputCount: 0,
            DuplicateOutputCount: 0,
            MalformedOutputCount: malformedOutputCount,
            DigestRevalidatedOutputCount: 0,
            DigestMatchedOutputCount: 0,
            DigestMismatchedOutputCount: 0,
            ExpectedPaths: expectedPathCoverage,
            Outputs: []);
    }

    private static IReadOnlyList<ReleasePublishBuildManifestExpectedOutput> CreateBuildManifestExpectedPathCoverage(
        IReadOnlyList<ReleasePublishBuildManifestOutput> outputs,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar) =>
        ExpectedBuildManifestOutputPaths
            .Select(path =>
            {
                var checksumPath = ToChecksumSidecarPath(path);
                var output = outputs.FirstOrDefault(item =>
                    !item.DuplicatePath &&
                    StringComparer.Ordinal.Equals(item.Path, path));
                var manifestOutputPresent = output is not null;
                var localArtifactPresent = LocalArtifactExists(localEvidenceArtifacts, path);
                var checksumEntryPresent = ChecksumEntryExists(checksumSidecar, checksumPath);
                return new ReleasePublishBuildManifestExpectedOutput(
                    path,
                    checksumPath,
                    output?.Status ?? BuildManifestExpectedOutputStatus(
                        manifestOutputPresent,
                        localArtifactPresent,
                        checksumEntryPresent,
                        digestRevalidated: false,
                        digestMatched: false),
                    manifestOutputPresent,
                    localArtifactPresent,
                    checksumEntryPresent,
                    output?.DigestRevalidatedInCurrentGate ?? false,
                    output?.Sha256,
                    output?.ActualSha256);
            })
            .ToArray();

    private static ReleasePublishArchiveEvidenceCrossReference CreateArchiveEvidenceCrossReference(
        string projectRoot,
        string releasePrepareEvidenceRoot,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference)
    {
        var fullPath = Path.Combine(releasePrepareEvidenceRoot, "release-archive-evidence.json");
        var displayPath = ToDisplayPath(projectRoot, fullPath);
        if (!File.Exists(fullPath))
        {
            return new ReleasePublishArchiveEvidenceCrossReference(
                displayPath,
                Exists: false,
                Status: "missing",
                MetadataCrossReferenceInCurrentGate: true,
                ContentReadInCurrentGate: false,
                DigestRevalidationInCurrentGate: true,
                ArchiveRevalidationInCurrentGate: true,
                ExpectedPathCount: ExpectedArchiveEvidencePaths.Length,
                ParsedPathCount: 0,
                CoveredExpectedPathCount: 0,
                MissingExpectedPathCount: ExpectedArchiveEvidencePaths.Length,
                MissingLocalArtifactPathCount: ExpectedArchiveEvidencePaths.Count(path => !LocalArtifactExists(localEvidenceArtifacts, path.Path)),
                MissingChecksumEntryPathCount: ExpectedArchiveEvidencePaths.Count(path => !ChecksumEntryExists(checksumSidecar, ToChecksumSidecarPath(path.Path))),
                MissingBuildManifestOutputPathCount: ExpectedArchiveEvidencePaths.Count(path => !BuildManifestOutputExists(buildManifestCrossReference, path.Path)),
                UnexpectedPathCount: 0,
                MalformedPathCount: 0,
                ArchivePathMatchesOutput: false,
                ArchiveSha256MetadataPresent: false,
                ArchiveLengthMetadataPresent: false,
                ArchiveSha256MatchesLocal: false,
                ArchiveLengthMatchesLocal: false,
                ExpectedArchiveSha256: null,
                ActualArchiveSha256: null,
                ExpectedArchiveLength: null,
                ActualArchiveLength: null,
                ArchiveOpenedInCurrentGate: false,
                ExpectedArchiveEntryCount: 0,
                EvidenceActualArchiveEntryCount: 0,
                ActualArchiveEntryCount: 0,
                ArchiveEntryCountMatchesMetadata: false,
                ArchiveEntryNamesMatchLocal: false,
                ArchiveEvidenceActualEntryNamesMatchLocal: false,
                ArchiveEntryOrderingMatchesLocal: false,
                ArchiveDeterministicTimestampsMatchLocal: false,
                ArchiveStoredCompressionMatchesLocal: false,
                ArchiveRevalidationDetail: "release-archive-evidence-missing",
                ArchiveEntries: [],
                ExpectedPaths: CreateArchiveEvidenceExpectedPathCoverage(new HashSet<string>(StringComparer.Ordinal), localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference),
                Paths: []);
        }

        try
        {
            var root = JsonNode.Parse(File.ReadAllText(fullPath)) as JsonObject;
            var output = root?["output"] as JsonObject;
            if (output is null)
            {
                return MalformedArchiveEvidenceCrossReference(projectRoot, fullPath, contentRead: true, malformedPathCount: 1, localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference);
            }

            var paths = new List<ReleasePublishArchiveEvidencePath>();
            var coveredExpectedRoles = new HashSet<string>(StringComparer.Ordinal);
            var unexpectedPaths = 0;
            var malformedPaths = 0;

            foreach (var expectedPath in ExpectedArchiveEvidencePaths)
            {
                if (!TryGetStringProperty(output, expectedPath.Role, out var actualPath))
                {
                    malformedPaths++;
                    paths.Add(new ReleasePublishArchiveEvidencePath(
                        expectedPath.Role,
                        Path: null,
                        ChecksumPath: null,
                        Status: "malformed",
                        ExpectedPath: false,
                        LocalArtifactPresent: false,
                        ChecksumEntryPresent: false,
                        BuildManifestOutputPresent: false,
                        DigestRevalidatedInCurrentGate: false,
                        ArchiveRevalidatedInCurrentGate: false,
                        ShapeDetail: "release-archive-evidence-output-path-missing"));
                    continue;
                }

                var checksumPath = ToChecksumSidecarPath(actualPath);
                var expected = StringComparer.Ordinal.Equals(actualPath, expectedPath.Path);
                var localArtifactPresent = LocalArtifactExists(localEvidenceArtifacts, actualPath);
                var checksumEntryPresent = ChecksumEntryExists(checksumSidecar, checksumPath);
                var buildManifestOutputPresent = BuildManifestOutputExists(buildManifestCrossReference, actualPath);
                if (expected)
                {
                    coveredExpectedRoles.Add(expectedPath.Role);
                }
                else
                {
                    unexpectedPaths++;
                }

                paths.Add(new ReleasePublishArchiveEvidencePath(
                    expectedPath.Role,
                    actualPath,
                    checksumPath,
                    ArchiveEvidencePathStatus(expected, localArtifactPresent, checksumEntryPresent, buildManifestOutputPresent),
                    ExpectedPath: expected,
                    localArtifactPresent,
                    checksumEntryPresent,
                    buildManifestOutputPresent,
                    DigestRevalidatedInCurrentGate: false,
                    ArchiveRevalidatedInCurrentGate: false,
                    ShapeDetail: "release-archive-evidence-output-path"));
            }

            var expectedPathCoverage = CreateArchiveEvidenceExpectedPathCoverage(coveredExpectedRoles, localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference);
            var missingExpectedPaths = ExpectedArchiveEvidencePaths.Length - coveredExpectedRoles.Count;
            var missingLocalPaths = expectedPathCoverage.Count(path => !path.LocalArtifactPresent);
            var missingChecksumPaths = expectedPathCoverage.Count(path => !path.ChecksumEntryPresent);
            var missingBuildManifestOutputPaths = expectedPathCoverage.Count(path => !path.BuildManifestOutputPresent);
            var archive = root?["archive"] as JsonObject;
            var releaseArchivePathPresent = TryGetStringProperty(output, "releaseArchive", out var releaseArchivePath);
            var archivePathMatchesOutput = TryGetStringProperty(archive, "path", out var archivePath) &&
                releaseArchivePathPresent &&
                StringComparer.Ordinal.Equals(archivePath, releaseArchivePath);
            var archiveSha256MetadataPresent = TryGetSha256Property(archive, out var expectedArchiveSha256);
            var archiveLengthMetadataPresent = TryGetInt64Property(archive, "length", out var parsedExpectedArchiveLength) &&
                parsedExpectedArchiveLength >= 0;
            long? expectedArchiveLength = archiveLengthMetadataPresent ? parsedExpectedArchiveLength : null;
            string? actualArchiveSha256 = null;
            long? actualArchiveLength = null;
            var archiveDigestRevalidated = false;
            if (archivePathMatchesOutput &&
                archiveSha256MetadataPresent &&
                archiveLengthMetadataPresent &&
                StringComparer.Ordinal.Equals(releaseArchivePath, ExpectedArchiveEvidencePaths[0].Path) &&
                LocalArtifactExists(localEvidenceArtifacts, releaseArchivePath))
            {
                var localArchivePath = Path.Combine(projectRoot, releaseArchivePath);
                if (File.Exists(localArchivePath))
                {
                    actualArchiveSha256 = ComputeSha256(localArchivePath);
                    actualArchiveLength = new FileInfo(localArchivePath).Length;
                    archiveDigestRevalidated = true;
                }
            }

            var archiveSha256MatchesLocal = archiveDigestRevalidated &&
                StringComparer.Ordinal.Equals(expectedArchiveSha256, actualArchiveSha256);
            var archiveLengthMatchesLocal = archiveDigestRevalidated &&
                expectedArchiveLength == actualArchiveLength;
            var expectedArchiveObject = root?["expected"] as JsonObject;
            var actualArchiveObject = root?["actual"] as JsonObject;
            var archiveEntryMetadataPresent = TryGetInt32Property(archive, "entries", out var archiveEntryMetadataCount) &&
                archiveEntryMetadataCount >= 0;
            var expectedEntriesPresent = TryGetStringArrayProperty(expectedArchiveObject, "entries", out var expectedArchiveEntries);
            var expectedTimestampPresent = TryGetStringProperty(expectedArchiveObject, "timestampUtc", out var expectedTimestampUtc);
            var expectedCompressionPresent = TryGetStringProperty(expectedArchiveObject, "compression", out var expectedCompression);
            var evidenceActualEntriesPresent = TryGetStringArrayProperty(actualArchiveObject, "entries", out var evidenceActualArchiveEntries);
            var archiveRevalidationMetadataPresent = archiveEntryMetadataPresent &&
                expectedEntriesPresent &&
                expectedTimestampPresent &&
                expectedCompressionPresent &&
                evidenceActualEntriesPresent;
            var archiveRevalidation = SkippedArchiveRevalidation(
                expectedEntriesPresent ? expectedArchiveEntries.Length : 0,
                evidenceActualEntriesPresent ? evidenceActualArchiveEntries.Length : 0,
                archiveRevalidationMetadataPresent
                    ? "archive-revalidation-prerequisites-not-met"
                    : "release-archive-evidence-archive-metadata-missing");
            if (archiveDigestRevalidated &&
                archiveSha256MatchesLocal &&
                archiveLengthMatchesLocal &&
                archiveRevalidationMetadataPresent &&
                missingExpectedPaths == 0 &&
                missingLocalPaths == 0 &&
                missingChecksumPaths == 0 &&
                missingBuildManifestOutputPaths == 0 &&
                unexpectedPaths == 0 &&
                malformedPaths == 0 &&
                archivePathMatchesOutput &&
                StringComparer.Ordinal.Equals(releaseArchivePath, ExpectedArchiveEvidencePaths[0].Path) &&
                LocalArtifactExists(localEvidenceArtifacts, releaseArchivePath))
            {
                archiveRevalidation = RevalidateArchive(
                    Path.Combine(projectRoot, releaseArchivePath),
                    archiveEntryMetadataCount,
                    expectedArchiveEntries,
                    evidenceActualArchiveEntries,
                    expectedTimestampUtc,
                    expectedCompression);
            }

            var status = ArchiveEvidenceCrossReferenceStatus(
                paths.Count,
                missingExpectedPaths,
                missingLocalPaths,
                missingChecksumPaths,
                missingBuildManifestOutputPaths,
                unexpectedPaths,
                malformedPaths,
                archivePathMatchesOutput,
                archiveSha256MetadataPresent,
                archiveLengthMetadataPresent,
                archiveDigestRevalidated,
                archiveSha256MatchesLocal,
                archiveLengthMatchesLocal,
                archiveRevalidationMetadataPresent,
                archiveRevalidation.Opened,
                ArchiveRevalidationMatches(archiveRevalidation));

            return new ReleasePublishArchiveEvidenceCrossReference(
                displayPath,
                Exists: true,
                Status: status,
                MetadataCrossReferenceInCurrentGate: true,
                ContentReadInCurrentGate: true,
                DigestRevalidationInCurrentGate: true,
                ArchiveRevalidationInCurrentGate: true,
                ExpectedPathCount: ExpectedArchiveEvidencePaths.Length,
                ParsedPathCount: paths.Count,
                CoveredExpectedPathCount: coveredExpectedRoles.Count,
                MissingExpectedPathCount: missingExpectedPaths,
                MissingLocalArtifactPathCount: missingLocalPaths,
                MissingChecksumEntryPathCount: missingChecksumPaths,
                MissingBuildManifestOutputPathCount: missingBuildManifestOutputPaths,
                UnexpectedPathCount: unexpectedPaths,
                MalformedPathCount: malformedPaths,
                ArchivePathMatchesOutput: archivePathMatchesOutput,
                ArchiveSha256MetadataPresent: archiveSha256MetadataPresent,
                ArchiveLengthMetadataPresent: archiveLengthMetadataPresent,
                ArchiveSha256MatchesLocal: archiveSha256MatchesLocal,
                ArchiveLengthMatchesLocal: archiveLengthMatchesLocal,
                ExpectedArchiveSha256: archiveSha256MetadataPresent ? expectedArchiveSha256 : null,
                ActualArchiveSha256: actualArchiveSha256,
                ExpectedArchiveLength: expectedArchiveLength,
                ActualArchiveLength: actualArchiveLength,
                ArchiveOpenedInCurrentGate: archiveRevalidation.Opened,
                ExpectedArchiveEntryCount: archiveRevalidation.ExpectedEntryCount,
                EvidenceActualArchiveEntryCount: archiveRevalidation.EvidenceActualEntryCount,
                ActualArchiveEntryCount: archiveRevalidation.ActualEntryCount,
                ArchiveEntryCountMatchesMetadata: archiveRevalidation.EntryCountMatchesMetadata,
                ArchiveEntryNamesMatchLocal: archiveRevalidation.EntryNamesMatchLocal,
                ArchiveEvidenceActualEntryNamesMatchLocal: archiveRevalidation.EvidenceActualEntryNamesMatchLocal,
                ArchiveEntryOrderingMatchesLocal: archiveRevalidation.EntryOrderingMatchesLocal,
                ArchiveDeterministicTimestampsMatchLocal: archiveRevalidation.DeterministicTimestampsMatchLocal,
                ArchiveStoredCompressionMatchesLocal: archiveRevalidation.StoredCompressionMatchesLocal,
                ArchiveRevalidationDetail: archiveRevalidation.Detail,
                ArchiveEntries: archiveRevalidation.Entries,
                ExpectedPaths: expectedPathCoverage,
                Paths: paths);
        }
        catch (JsonException)
        {
            return MalformedArchiveEvidenceCrossReference(projectRoot, fullPath, contentRead: true, malformedPathCount: 1, localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference);
        }
    }

    private static ReleasePublishArchiveEvidenceCrossReference MalformedArchiveEvidenceCrossReference(
        string projectRoot,
        string fullPath,
        bool contentRead,
        int malformedPathCount,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference)
    {
        var expectedPathCoverage = CreateArchiveEvidenceExpectedPathCoverage(new HashSet<string>(StringComparer.Ordinal), localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference);
        return new ReleasePublishArchiveEvidenceCrossReference(
            ToDisplayPath(projectRoot, fullPath),
            Exists: true,
            Status: "malformed-not-validated",
            MetadataCrossReferenceInCurrentGate: true,
            ContentReadInCurrentGate: contentRead,
            DigestRevalidationInCurrentGate: true,
            ArchiveRevalidationInCurrentGate: true,
            ExpectedPathCount: ExpectedArchiveEvidencePaths.Length,
            ParsedPathCount: 0,
            CoveredExpectedPathCount: 0,
            MissingExpectedPathCount: ExpectedArchiveEvidencePaths.Length,
            MissingLocalArtifactPathCount: expectedPathCoverage.Count(path => !path.LocalArtifactPresent),
            MissingChecksumEntryPathCount: expectedPathCoverage.Count(path => !path.ChecksumEntryPresent),
            MissingBuildManifestOutputPathCount: expectedPathCoverage.Count(path => !path.BuildManifestOutputPresent),
            UnexpectedPathCount: 0,
            MalformedPathCount: malformedPathCount,
            ArchivePathMatchesOutput: false,
            ArchiveSha256MetadataPresent: false,
            ArchiveLengthMetadataPresent: false,
            ArchiveSha256MatchesLocal: false,
            ArchiveLengthMatchesLocal: false,
            ExpectedArchiveSha256: null,
            ActualArchiveSha256: null,
            ExpectedArchiveLength: null,
            ActualArchiveLength: null,
            ArchiveOpenedInCurrentGate: false,
            ExpectedArchiveEntryCount: 0,
            EvidenceActualArchiveEntryCount: 0,
            ActualArchiveEntryCount: 0,
            ArchiveEntryCountMatchesMetadata: false,
            ArchiveEntryNamesMatchLocal: false,
            ArchiveEvidenceActualEntryNamesMatchLocal: false,
            ArchiveEntryOrderingMatchesLocal: false,
            ArchiveDeterministicTimestampsMatchLocal: false,
            ArchiveStoredCompressionMatchesLocal: false,
            ArchiveRevalidationDetail: "release-archive-evidence-malformed",
            ArchiveEntries: [],
            ExpectedPaths: expectedPathCoverage,
            Paths: []);
    }

    private static IReadOnlyList<ReleasePublishArchiveEvidenceExpectedPath> CreateArchiveEvidenceExpectedPathCoverage(
        IReadOnlySet<string> coveredExpectedRoles,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference) =>
        ExpectedArchiveEvidencePaths
            .Select(expectedPath =>
            {
                var checksumPath = ToChecksumSidecarPath(expectedPath.Path);
                var evidenceMetadataPresent = coveredExpectedRoles.Contains(expectedPath.Role);
                var localArtifactPresent = LocalArtifactExists(localEvidenceArtifacts, expectedPath.Path);
                var checksumEntryPresent = ChecksumEntryExists(checksumSidecar, checksumPath);
                var buildManifestOutputPresent = BuildManifestOutputExists(buildManifestCrossReference, expectedPath.Path);
                return new ReleasePublishArchiveEvidenceExpectedPath(
                    expectedPath.Role,
                    expectedPath.Path,
                    checksumPath,
                    ArchiveEvidenceExpectedPathStatus(evidenceMetadataPresent, localArtifactPresent, checksumEntryPresent, buildManifestOutputPresent),
                    evidenceMetadataPresent,
                    localArtifactPresent,
                    checksumEntryPresent,
                    buildManifestOutputPresent);
            })
            .ToArray();

    private static ArchiveRevalidationResult SkippedArchiveRevalidation(
        int expectedEntryCount,
        int evidenceActualEntryCount,
        string detail) =>
        new(
            Opened: false,
            ExpectedEntryCount: expectedEntryCount,
            EvidenceActualEntryCount: evidenceActualEntryCount,
            ActualEntryCount: 0,
            EntryCountMatchesMetadata: false,
            EntryNamesMatchLocal: false,
            EvidenceActualEntryNamesMatchLocal: false,
            EntryOrderingMatchesLocal: false,
            DeterministicTimestampsMatchLocal: false,
            StoredCompressionMatchesLocal: false,
            Detail: detail,
            Entries: []);

    private static ArchiveRevalidationResult RevalidateArchive(
        string archivePath,
        int archiveEntryMetadataCount,
        IReadOnlyList<string> expectedEntries,
        IReadOnlyList<string> evidenceActualEntries,
        string expectedTimestampUtc,
        string expectedCompression)
    {
        try
        {
            using var archive = ZipFile.OpenRead(archivePath);
            var entries = archive.Entries
                .Select((entry, index) =>
                {
                    var lastWriteTimeUtc = entry.LastWriteTime.UtcDateTime.ToString("O");
                    var expectedAtIndex = index < expectedEntries.Count &&
                        StringComparer.Ordinal.Equals(expectedEntries[index], entry.FullName);
                    var evidenceActualAtIndex = index < evidenceActualEntries.Count &&
                        StringComparer.Ordinal.Equals(evidenceActualEntries[index], entry.FullName);
                    var expectedPath = expectedEntries.Contains(entry.FullName, StringComparer.Ordinal);
                    var timestampMatches = StringComparer.Ordinal.Equals(lastWriteTimeUtc, expectedTimestampUtc);
                    var stored = entry.Length == entry.CompressedLength;
                    return new ReleasePublishArchiveEntryRevalidation(
                        Index: index,
                        Path: entry.FullName,
                        Length: entry.Length,
                        CompressedLength: entry.CompressedLength,
                        LastWriteTimeUtc: lastWriteTimeUtc,
                        Status: ArchiveEntryRevalidationStatus(
                            expectedPath,
                            expectedAtIndex,
                            evidenceActualAtIndex,
                            timestampMatches,
                            stored),
                        ExpectedPath: expectedPath,
                        ExpectedAtIndex: expectedAtIndex,
                        EvidenceActualAtIndex: evidenceActualAtIndex,
                        TimestampMatches: timestampMatches,
                        Stored: stored);
                })
                .ToArray();
            var actualEntryNames = entries.Select(entry => entry.Path).ToArray();
            var entryCountMatchesMetadata = archiveEntryMetadataCount == actualEntryNames.Length &&
                expectedEntries.Count == actualEntryNames.Length &&
                evidenceActualEntries.Count == actualEntryNames.Length;
            var entryNamesMatchLocal = expectedEntries.SequenceEqual(actualEntryNames, StringComparer.Ordinal);
            var evidenceActualEntryNamesMatchLocal = evidenceActualEntries.SequenceEqual(actualEntryNames, StringComparer.Ordinal);
            var entryOrderingMatchesLocal = actualEntryNames.SequenceEqual(actualEntryNames.OrderBy(entry => entry, StringComparer.Ordinal), StringComparer.Ordinal);
            var deterministicTimestampsMatchLocal = entries.All(entry => entry.TimestampMatches);
            var storedCompressionMatchesLocal = StringComparer.Ordinal.Equals(expectedCompression, "stored") &&
                entries.All(entry => entry.Stored);

            return new ArchiveRevalidationResult(
                Opened: true,
                ExpectedEntryCount: expectedEntries.Count,
                EvidenceActualEntryCount: evidenceActualEntries.Count,
                ActualEntryCount: actualEntryNames.Length,
                EntryCountMatchesMetadata: entryCountMatchesMetadata,
                EntryNamesMatchLocal: entryNamesMatchLocal,
                EvidenceActualEntryNamesMatchLocal: evidenceActualEntryNamesMatchLocal,
                EntryOrderingMatchesLocal: entryOrderingMatchesLocal,
                DeterministicTimestampsMatchLocal: deterministicTimestampsMatchLocal,
                StoredCompressionMatchesLocal: storedCompressionMatchesLocal,
                Detail: "zip-archive-opened-and-entry-metadata-revalidated",
                Entries: entries);
        }
        catch (InvalidDataException)
        {
            return SkippedArchiveRevalidation(expectedEntries.Count, evidenceActualEntries.Count, "zip-archive-invalid-data");
        }
        catch (IOException)
        {
            return SkippedArchiveRevalidation(expectedEntries.Count, evidenceActualEntries.Count, "zip-archive-io-error");
        }
        catch (UnauthorizedAccessException)
        {
            return SkippedArchiveRevalidation(expectedEntries.Count, evidenceActualEntries.Count, "zip-archive-access-denied");
        }
    }

    private static bool ArchiveRevalidationMatches(ArchiveRevalidationResult result) =>
        result.EntryCountMatchesMetadata &&
        result.EntryNamesMatchLocal &&
        result.EvidenceActualEntryNamesMatchLocal &&
        result.EntryOrderingMatchesLocal &&
        result.DeterministicTimestampsMatchLocal &&
        result.StoredCompressionMatchesLocal;

    private static string ArchiveEntryRevalidationStatus(
        bool expectedPath,
        bool expectedAtIndex,
        bool evidenceActualAtIndex,
        bool timestampMatches,
        bool stored)
    {
        if (!expectedPath)
        {
            return "unexpected-not-revalidated";
        }

        if (!expectedAtIndex || !evidenceActualAtIndex)
        {
            return "mismatched-entry-revalidated";
        }

        if (!timestampMatches || !stored)
        {
            return "metadata-mismatched-revalidated";
        }

        return "matched-revalidated";
    }

    private static ReleasePublishEvidenceArtifact BinaryArtifact(string projectRoot, string releasePrepareEvidenceRoot, string id, string kind, string relativePath)
    {
        var fullPath = Path.Combine(releasePrepareEvidenceRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return MissingArtifact(projectRoot, fullPath, id, kind, "zip");
        }

        return new ReleasePublishEvidenceArtifact(
            id,
            ToDisplayPath(projectRoot, fullPath),
            kind,
            ContentKind: "zip",
            Required: true,
            Exists: true,
            Status: "present-not-classified",
            Length: new FileInfo(fullPath).Length,
            ShapeCheckedInCurrentGate: false,
            ContentReadInCurrentGate: false,
            ShapeDetail: "binary-archive-deferred");
    }

    private static ReleasePublishEvidenceArtifact MissingArtifact(string projectRoot, string fullPath, string id, string kind, string contentKind) =>
        new(
            id,
            ToDisplayPath(projectRoot, fullPath),
            kind,
            contentKind,
            Required: true,
            Exists: false,
            Status: "missing",
            Length: null,
            ShapeCheckedInCurrentGate: false,
            ContentReadInCurrentGate: false,
            ShapeDetail: null);

    private static ReleasePublishSemanticEvidenceValidation CreateSemanticEvidenceValidation(
        string projectRoot,
        string releasePrepareEvidenceRoot,
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference)
    {
        if (!SemanticValidationPrerequisitesMet(localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference))
        {
            return SkippedSemanticEvidenceValidation("lower-layer-release-evidence-not-complete");
        }

        var documents = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        foreach (var (id, relativePath) in SemanticJsonArtifacts())
        {
            var fullPath = Path.Combine(releasePrepareEvidenceRoot, relativePath);
            try
            {
                if (JsonNode.Parse(File.ReadAllText(fullPath)) is not JsonObject root)
                {
                    return SkippedSemanticEvidenceValidation("semantic-evidence-json-root-not-object", status: "malformed-not-validated");
                }

                documents[id] = root;
            }
            catch (JsonException)
            {
                return SkippedSemanticEvidenceValidation("semantic-evidence-json-malformed", status: "malformed-not-validated");
            }
            catch (IOException)
            {
                return SkippedSemanticEvidenceValidation("semantic-evidence-json-unreadable", status: "blocked-not-validated");
            }
            catch (UnauthorizedAccessException)
            {
                return SkippedSemanticEvidenceValidation("semantic-evidence-json-unreadable", status: "blocked-not-validated");
            }
        }

        var checks = new List<ReleasePublishSemanticEvidenceCheck>
        {
            SemanticCheck("release-plan-contract", ContractMatches(documents["release-plan"], "wastelandforge.release-plan", "planned"), "release-plan-contract-validated"),
            SemanticCheck("release-summary-contract", ContractMatches(documents["release-summary"], "wastelandforge.release-summary", "prepared"), "release-summary-contract-validated"),
            SemanticCheck("staging-payload-contract", ContractMatches(documents["staging-payload"], "wastelandforge.release-staging-payload", "skeleton"), "staging-payload-contract-validated"),
            SemanticCheck("release-archive-plan-contract", ContractMatches(documents["release-archive-plan"], "wastelandforge.release-archive-plan", "created"), "release-archive-plan-contract-validated"),
            SemanticCheck("release-archive-evidence-contract", ContractMatches(documents["release-archive-evidence"], "wastelandforge.release-archive-evidence", "passed"), "release-archive-evidence-contract-validated"),
            SemanticCheck("build-manifest-contract", BuildManifestContractMatches(documents["build-manifest"]), "build-manifest-contract-validated"),
            SemanticCheck("project-output-map-consistency", DocumentsShareExpectedProjectAndOutputs(documents.Values, projectRoot), "project-output-map-consistency-validated"),
            SemanticCheck("release-plan-planned-output-consistency", ReleasePlanOutputsMatch(documents["release-plan"]), "release-plan-planned-output-consistency-validated"),
            SemanticCheck("release-summary-counts", ReleaseSummaryCountsMatch(documents["release-summary"]), "release-summary-counts-validated"),
            SemanticCheck("staging-payload-boundary", StagingPayloadBoundaryMatches(documents["staging-payload"]), "staging-payload-boundary-validated"),
            SemanticCheck("release-archive-plan-metadata", ReleaseArchivePlanMetadataMatches(documents["release-archive-plan"]), "release-archive-plan-metadata-validated"),
            SemanticCheck("release-archive-plan-inputs", ReleaseArchivePlanInputsMatch(documents["release-archive-plan"]), "release-archive-plan-inputs-validated"),
            SemanticCheck("release-archive-evidence-checks", ReleaseArchiveEvidenceChecksMatch(documents["release-archive-evidence"]), "release-archive-evidence-checks-validated"),
            SemanticCheck("build-manifest-output-set", BuildManifestOutputSetMatches(documents["build-manifest"]), "build-manifest-output-set-validated"),
            SemanticCheck("non-publish-execution-boundary", NonPublishExecutionBoundariesMatch(documents.Values), "non-publish-execution-boundary-validated")
        };

        var failedChecks = checks.Count(check => StringComparer.Ordinal.Equals(check.Status, "failed"));
        var passedChecks = checks.Count - failedChecks;
        return new ReleasePublishSemanticEvidenceValidation(
            Status: failedChecks == 0 ? "complete-semantic-validated" : "mismatch-semantic-validated",
            ValidationInCurrentGate: true,
            ContentReadInCurrentGate: true,
            ExpectedChecks: SemanticCheckTemplates.Length,
            PassedChecks: passedChecks,
            FailedChecks: failedChecks,
            SkippedChecks: 0,
            Detail: failedChecks == 0 ? "semantic-release-evidence-validated" : "semantic-release-evidence-mismatch",
            Checks: checks);
    }

    private static bool SemanticValidationPrerequisitesMet(
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference) =>
        localEvidenceArtifacts.All(artifact =>
            artifact.Exists &&
            !StringComparer.Ordinal.Equals(artifact.Status, "missing") &&
            !StringComparer.Ordinal.Equals(artifact.Status, "malformed")) &&
        StringComparer.Ordinal.Equals(checksumSidecar.Status, "complete-digest-revalidated") &&
        StringComparer.Ordinal.Equals(buildManifestCrossReference.Status, "complete-digest-revalidated") &&
        StringComparer.Ordinal.Equals(archiveEvidenceCrossReference.Status, "complete-archive-revalidated");

    private static ReleasePublishSemanticEvidenceValidation SkippedSemanticEvidenceValidation(string detail, string status = "blocked-not-validated") =>
        new(
            Status: status,
            ValidationInCurrentGate: true,
            ContentReadInCurrentGate: false,
            ExpectedChecks: SemanticCheckTemplates.Length,
            PassedChecks: 0,
            FailedChecks: 0,
            SkippedChecks: SemanticCheckTemplates.Length,
            Detail: detail,
            Checks: SemanticCheckTemplates
                .Select(template => new ReleasePublishSemanticEvidenceCheck(
                    template.Id,
                    template.Title,
                    template.Path,
                    Status: "skipped",
                    Detail: detail))
                .ToArray());

    private static IEnumerable<(string Id, string RelativePath)> SemanticJsonArtifacts()
    {
        yield return ("staging-payload", "staging/release-payload.json");
        yield return ("release-archive-plan", "release-archive-plan.json");
        yield return ("release-archive-evidence", "release-archive-evidence.json");
        yield return ("release-plan", "release-plan.json");
        yield return ("release-summary", "release-summary.json");
        yield return ("build-manifest", "build-manifest.json");
    }

    private static ReleasePublishSemanticEvidenceCheck SemanticCheck(string id, bool passed, string passedDetail)
    {
        var template = SemanticCheckTemplates.Single(item => StringComparer.Ordinal.Equals(item.Id, id));
        return new ReleasePublishSemanticEvidenceCheck(
            template.Id,
            template.Title,
            template.Path,
            passed ? "passed" : "failed",
            passed ? passedDetail : $"{id}-mismatch");
    }

    private static bool ContractMatches(JsonObject root, string expectedKind, string expectedStatus) =>
        TryGetStringProperty(root, "formatVersion", out _) &&
        TryGetStringProperty(root, "kind", out var kind) &&
        StringComparer.Ordinal.Equals(kind, expectedKind) &&
        TryGetStringProperty(root, "command", out var command) &&
        StringComparer.Ordinal.Equals(command, "release prepare") &&
        TryGetStringProperty(root, "status", out var status) &&
        StringComparer.Ordinal.Equals(status, expectedStatus) &&
        ToolMetadataMatches(root);

    private static bool BuildManifestContractMatches(JsonObject root) =>
        ContractMatches(root, "wastelandforge.build-manifest", "prepared") &&
        TryGetStringProperty(root, "buildType", out var buildType) &&
        StringComparer.Ordinal.Equals(buildType, "wastelandforge/release-prepare/v1") &&
        TryGetBooleanProperty(root, "dryRun", out var dryRun) &&
        !dryRun &&
        root["validation"] is JsonObject validation &&
        TryGetStringProperty(validation, "status", out var validationStatus) &&
        StringComparer.Ordinal.Equals(validationStatus, "not-run") &&
        TryGetInt32Property(validation, "errors", out var errors) &&
        errors == 0 &&
        TryGetInt32Property(validation, "warnings", out var warnings) &&
        warnings == 0 &&
        TryGetInt32Property(validation, "notes", out var notes) &&
        notes == 0 &&
        root["capabilities"] is JsonObject capabilities &&
        TryGetStringProperty(capabilities, "status", out var capabilitiesStatus) &&
        StringComparer.Ordinal.Equals(capabilitiesStatus, "not-evaluated") &&
        capabilities["resolved"] is JsonArray resolved &&
        resolved.Count == 0 &&
        root["sources"] is JsonArray sources &&
        sources.Count == 0 &&
        root["generators"] is JsonArray generators &&
        generators.Count == 1 &&
        generators[0] is JsonObject generator &&
        TryGetStringProperty(generator, "id", out var generatorId) &&
        StringComparer.Ordinal.Equals(generatorId, "wf.release.prepare") &&
        TryGetStringProperty(generator, "target", out var generatorTarget) &&
        StringComparer.Ordinal.Equals(generatorTarget, "release-prepare");

    private static bool ToolMetadataMatches(JsonObject root) =>
        root["tool"] is JsonObject tool &&
        TryGetStringProperty(tool, "name", out var toolName) &&
        StringComparer.Ordinal.Equals(toolName, CliConstants.ToolName) &&
        TryGetStringProperty(tool, "version", out var toolVersion) &&
        StringComparer.Ordinal.Equals(toolVersion, CliConstants.Version);

    private static bool DocumentsShareExpectedProjectAndOutputs(IEnumerable<JsonObject> documents, string projectRoot) =>
        documents.All(document =>
            document["project"] is JsonObject project &&
            TryGetStringProperty(project, "root", out var root) &&
            StringComparer.Ordinal.Equals(root, projectRoot) &&
            OutputMapMatches(document));

    private static bool OutputMapMatches(JsonObject root)
    {
        if (root["output"] is not JsonObject output)
        {
            return false;
        }

        foreach (var (key, path) in ExpectedSemanticOutputPaths)
        {
            if (!TryGetStringProperty(output, key, out var actual) ||
                !StringComparer.Ordinal.Equals(actual, path))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ReleasePlanOutputsMatch(JsonObject releasePlan)
    {
        if (releasePlan["plannedOutputs"] is not JsonArray outputs ||
            outputs.Count != ExpectedReleasePlanOutputs.Length)
        {
            return false;
        }

        for (var index = 0; index < ExpectedReleasePlanOutputs.Length; index++)
        {
            if (outputs[index] is not JsonObject output ||
                !TryGetStringProperty(output, "kind", out var kind) ||
                !StringComparer.Ordinal.Equals(kind, ExpectedReleasePlanOutputs[index].Kind) ||
                !TryGetStringProperty(output, "path", out var path) ||
                !StringComparer.Ordinal.Equals(path, ExpectedReleasePlanOutputs[index].Path) ||
                !TryGetBooleanProperty(output, "wouldWriteInCurrentGate", out var wouldWrite) ||
                !wouldWrite)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ReleaseSummaryCountsMatch(JsonObject releaseSummary) =>
        releaseSummary["summary"] is JsonObject summary &&
        TryGetInt32Property(summary, "plannedOutputs", out var plannedOutputs) &&
        plannedOutputs == ExpectedReleasePlanOutputs.Length &&
        TryGetInt32Property(summary, "writtenOutputs", out var writtenOutputs) &&
        writtenOutputs == 8 &&
        BooleanPropertyEquals(summary, "buildManifestWritten", expected: true) &&
        BooleanPropertyEquals(summary, "checksumsWritten", expected: true) &&
        BooleanPropertyEquals(summary, "stagingPayloadWritten", expected: true) &&
        BooleanPropertyEquals(summary, "archivePlanWritten", expected: true) &&
        BooleanPropertyEquals(summary, "archiveEvidenceWritten", expected: true) &&
        BooleanPropertyEquals(summary, "archiveCreated", expected: true) &&
        BooleanPropertyEquals(summary, "releasePublished", expected: false);

    private static bool StagingPayloadBoundaryMatches(JsonObject stagingPayload) =>
        stagingPayload["payload"] is JsonObject payload &&
        StringPropertyEquals(payload, "status", "skeleton") &&
        TryGetInt32Property(payload, "modPayloadFiles", out var modPayloadFiles) &&
        modPayloadFiles == 0 &&
        BooleanPropertyEquals(payload, "writesToGameData", expected: false) &&
        BooleanPropertyEquals(payload, "writesToMo2Profile", expected: false) &&
        BooleanPropertyEquals(payload, "pluginMutation", expected: false) &&
        BooleanPropertyEquals(payload, "archiveCreated", expected: false) &&
        BooleanPropertyEquals(payload, "installerCreated", expected: false);

    private static bool ReleaseArchivePlanMetadataMatches(JsonObject archivePlan) =>
        archivePlan["archive"] is JsonObject archive &&
        StringPropertyEquals(archive, "status", "created") &&
        StringPropertyEquals(archive, "path", "dist/release-prepare/archives/release.zip") &&
        StringPropertyEquals(archive, "format", "zip") &&
        StringPropertyEquals(archive, "mediaType", "application/zip") &&
        BooleanPropertyEquals(archive, "created", expected: true) &&
        TryGetInt32Property(archive, "entries", out var entries) &&
        entries == ExpectedArchiveEntryPaths.Length &&
        archivePlan["determinism"] is JsonObject determinism &&
        StringPropertyEquals(determinism, "entryOrdering", "ordinal-path-order") &&
        StringPropertyEquals(determinism, "timestampSource", "SOURCE_DATE_EPOCH-clamped-to-zip-range-or-1980-epoch") &&
        StringPropertyEquals(determinism, "compression", "stored") &&
        StringPropertyEquals(determinism, "fomodAssembly", "not-planned-in-current-gate");

    private static bool ReleaseArchivePlanInputsMatch(JsonObject archivePlan)
    {
        if (archivePlan["inputs"] is not JsonArray inputs ||
            inputs.Count != ExpectedArchivePlanInputs.Length)
        {
            return false;
        }

        for (var index = 0; index < ExpectedArchivePlanInputs.Length; index++)
        {
            if (inputs[index] is not JsonObject input ||
                !StringPropertyEquals(input, "kind", ExpectedArchivePlanInputs[index].Kind) ||
                !StringPropertyEquals(input, "path", ExpectedArchivePlanInputs[index].Path) ||
                !StringPropertyEquals(input, "status", "planned-local-evidence"))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ReleaseArchiveEvidenceChecksMatch(JsonObject archiveEvidence) =>
        archiveEvidence["archive"] is JsonObject archive &&
        StringPropertyEquals(archive, "path", "dist/release-prepare/archives/release.zip") &&
        StringPropertyEquals(archive, "format", "zip") &&
        StringPropertyEquals(archive, "mediaType", "application/zip") &&
        TryGetSha256Property(archive, out _) &&
        TryGetInt64Property(archive, "length", out var length) &&
        length > 0 &&
        TryGetInt32Property(archive, "entries", out var entries) &&
        entries == ExpectedArchiveEntryPaths.Length &&
        archiveEvidence["expected"] is JsonObject expected &&
        SequenceMatchesStringArray(expected["entries"] as JsonArray, ExpectedArchiveEntryPaths) &&
        StringPropertyEquals(expected, "compression", "stored") &&
        archiveEvidence["actual"] is JsonObject actual &&
        SequenceMatchesStringArray(actual["entries"] as JsonArray, ExpectedArchiveEntryPaths) &&
        TryGetInt32Property(actual, "storedEntries", out var storedEntries) &&
        storedEntries == ExpectedArchiveEntryPaths.Length &&
        archiveEvidence["checks"] is JsonObject checks &&
        BooleanPropertyEquals(checks, "archiveDigestRecomputed", expected: true) &&
        BooleanPropertyEquals(checks, "entryNamesMatch", expected: true) &&
        BooleanPropertyEquals(checks, "entryOrderingMatch", expected: true) &&
        BooleanPropertyEquals(checks, "deterministicTimestampsMatch", expected: true) &&
        BooleanPropertyEquals(checks, "storedCompressionMatch", expected: true);

    private static bool BuildManifestOutputSetMatches(JsonObject buildManifest)
    {
        if (buildManifest["outputs"] is not JsonArray outputs ||
            outputs.Count != ExpectedBuildManifestOutputPaths.Length)
        {
            return false;
        }

        for (var index = 0; index < ExpectedBuildManifestOutputPaths.Length; index++)
        {
            if (outputs[index] is not JsonObject output ||
                !StringPropertyEquals(output, "path", ExpectedBuildManifestOutputPaths[index]) ||
                !TryGetSha256Property(output, out _) ||
                !TryGetInt64Property(output, "length", out var length) ||
                length <= 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool NonPublishExecutionBoundariesMatch(IEnumerable<JsonObject> documents) =>
        documents.All(document =>
            document["execution"] is JsonObject execution &&
            BooleanPropertyEquals(execution, "releasePrepareExecution", expected: true) &&
            BooleanPropertyEquals(execution, "releasePublishing", expected: false) &&
            BooleanPropertyEquals(execution, "remoteRepositoryCall", expected: false) &&
            BooleanPropertyEquals(execution, "attestationSigning", expected: false) &&
            BooleanPropertyEquals(execution, "externalToolExecution", expected: false) &&
            BooleanPropertyEquals(execution, "pluginMutation", expected: false) &&
            BooleanPropertyEquals(execution, "mo2Automation", expected: false) &&
            BooleanPropertyEquals(execution, "geckAutomation", expected: false) &&
            BooleanPropertyEquals(execution, "runtimeProbe", expected: false) &&
            BooleanPropertyEquals(execution, "aiRequired", expected: false));

    private static bool SequenceMatchesStringArray(JsonArray? actualValues, IReadOnlyList<string> expectedValues)
    {
        if (actualValues is null || actualValues.Count != expectedValues.Count)
        {
            return false;
        }

        for (var index = 0; index < expectedValues.Count; index++)
        {
            if (actualValues[index] is not JsonValue value ||
                !value.TryGetValue<string>(out var actual) ||
                !StringComparer.Ordinal.Equals(actual, expectedValues[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool StringPropertyEquals(JsonObject source, string propertyName, string expected) =>
        TryGetStringProperty(source, propertyName, out var actual) &&
        StringComparer.Ordinal.Equals(actual, expected);

    private static bool BooleanPropertyEquals(JsonObject source, string propertyName, bool expected) =>
        TryGetBooleanProperty(source, propertyName, out var actual) &&
        actual == expected;

    private static bool IsChecksumLineShape(string line)
    {
        return TryParseChecksumLineShape(line, out _, out _);
    }

    private static bool TryParseChecksumLineShape(string line, out string sha256, out string path)
    {
        sha256 = string.Empty;
        path = string.Empty;

        const int hashLength = 64;
        if (line.Length <= hashLength + 2 ||
            !StringComparer.Ordinal.Equals(line.Substring(hashLength, 2), "  "))
        {
            return false;
        }

        for (var index = 0; index < hashLength; index++)
        {
            var ch = line[index];
            if (!((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f')))
            {
                return false;
            }
        }

        path = line[(hashLength + 2)..];
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        sha256 = line[..hashLength];
        return true;
    }

    private static bool TryGetStringProperty(JsonObject? source, string propertyName, out string value)
    {
        value = string.Empty;
        if (source?[propertyName] is not JsonValue jsonValue ||
            !jsonValue.TryGetValue<string>(out var stringValue) ||
            string.IsNullOrWhiteSpace(stringValue))
        {
            return false;
        }

        value = stringValue;
        return true;
    }

    private static bool TryGetSha256Property(JsonObject? source, out string value)
    {
        if (!TryGetStringProperty(source, "sha256", out value) || value.Length != 64)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var ch = value[index];
            if (!((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f')))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetStringArrayProperty(JsonObject? source, string propertyName, out string[] values)
    {
        values = [];
        if (source?[propertyName] is not JsonArray array)
        {
            return false;
        }

        var parsedValues = new List<string>();
        foreach (var item in array)
        {
            if (item is not JsonValue value ||
                !value.TryGetValue<string>(out var stringValue) ||
                string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            parsedValues.Add(stringValue);
        }

        values = parsedValues.ToArray();
        return true;
    }

    private static bool TryGetInt32Property(JsonObject? source, string propertyName, out int value)
    {
        value = 0;
        return source?[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<int>(out value);
    }

    private static bool TryGetInt64Property(JsonObject? source, string propertyName, out long value)
    {
        value = 0;
        return source?[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<long>(out value);
    }

    private static bool TryGetBooleanProperty(JsonObject? source, string propertyName, out bool value)
    {
        value = false;
        return source?[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<bool>(out value);
    }

    private static string ReleasePrepareEvidenceStatus(
        int presentArtifacts,
        int missingArtifacts,
        int malformedArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference,
        ReleasePublishSemanticEvidenceValidation semanticEvidenceValidation)
    {
        if (presentArtifacts == 0)
        {
            return "missing";
        }

        if (malformedArtifacts > 0 ||
            StringComparer.Ordinal.Equals(checksumSidecar.Status, "malformed-not-validated") ||
            StringComparer.Ordinal.Equals(buildManifestCrossReference.Status, "malformed-not-validated") ||
            StringComparer.Ordinal.Equals(archiveEvidenceCrossReference.Status, "malformed-not-validated") ||
            StringComparer.Ordinal.Equals(semanticEvidenceValidation.Status, "malformed-not-validated"))
        {
            return "malformed-not-validated";
        }

        if (StringComparer.Ordinal.Equals(checksumSidecar.Status, "mismatch-entry-classified-not-revalidated"))
        {
            return "checksum-coverage-mismatch-not-revalidated";
        }

        if (StringComparer.Ordinal.Equals(checksumSidecar.Status, "mismatch-digest-revalidated"))
        {
            return "checksum-digest-mismatch-revalidated";
        }

        if (StringComparer.Ordinal.Equals(buildManifestCrossReference.Status, "mismatch-cross-referenced-not-revalidated"))
        {
            return "build-manifest-cross-reference-mismatch-not-revalidated";
        }

        if (StringComparer.Ordinal.Equals(buildManifestCrossReference.Status, "mismatch-digest-revalidated"))
        {
            return "build-manifest-digest-mismatch-revalidated";
        }

        if (StringComparer.Ordinal.Equals(archiveEvidenceCrossReference.Status, "mismatch-cross-referenced-not-revalidated"))
        {
            return "release-archive-evidence-cross-reference-mismatch-not-revalidated";
        }

        if (StringComparer.Ordinal.Equals(archiveEvidenceCrossReference.Status, "mismatch-digest-revalidated"))
        {
            return "release-archive-evidence-digest-mismatch-revalidated";
        }

        if (StringComparer.Ordinal.Equals(archiveEvidenceCrossReference.Status, "mismatch-archive-revalidated"))
        {
            return "release-archive-evidence-archive-mismatch-revalidated";
        }

        if (StringComparer.Ordinal.Equals(semanticEvidenceValidation.Status, "mismatch-semantic-validated"))
        {
            return "semantic-evidence-mismatch-validated";
        }

        return missingArtifacts == 0
            ? "complete-semantic-validated"
            : "partial-semantic-not-validated";
    }

    private static string ChecksumSidecarStatus(
        int parsedEntries,
        int missingExpectedEntries,
        int unexpectedEntries,
        int duplicateEntries,
        int malformedEntries,
        int digestMismatchedEntries,
        int missingLocalFileEntries)
    {
        if (malformedEntries > 0 || parsedEntries == 0)
        {
            return "malformed-not-validated";
        }

        if (missingExpectedEntries == 0 &&
            unexpectedEntries == 0 &&
            duplicateEntries == 0)
        {
            return digestMismatchedEntries == 0 && missingLocalFileEntries == 0
                ? "complete-digest-revalidated"
                : "mismatch-digest-revalidated";
        }

        return "mismatch-entry-classified-not-revalidated";
    }

    private static string BuildManifestCrossReferenceStatus(
        int parsedOutputs,
        int missingExpectedOutputs,
        int missingLocalOutputs,
        int missingChecksumOutputs,
        int unexpectedOutputs,
        int duplicateOutputs,
        int malformedOutputs,
        int digestMismatchedOutputs)
    {
        if (malformedOutputs > 0 || parsedOutputs == 0)
        {
            return "malformed-not-validated";
        }

        if (missingExpectedOutputs == 0 &&
            missingLocalOutputs == 0 &&
            missingChecksumOutputs == 0 &&
            unexpectedOutputs == 0 &&
            duplicateOutputs == 0)
        {
            return digestMismatchedOutputs == 0
                ? "complete-digest-revalidated"
                : "mismatch-digest-revalidated";
        }

        return "mismatch-cross-referenced-not-revalidated";
    }

    private static string BuildManifestOutputStatus(
        bool duplicate,
        bool expected,
        bool localArtifactPresent,
        bool checksumEntryPresent,
        bool digestRevalidated,
        bool digestMatched)
    {
        if (duplicate)
        {
            return "duplicate-not-revalidated";
        }

        if (!expected)
        {
            return "unexpected-not-revalidated";
        }

        return BuildManifestExpectedOutputStatus(
            manifestOutputPresent: true,
            localArtifactPresent,
            checksumEntryPresent,
            digestRevalidated,
            digestMatched);
    }

    private static string BuildManifestExpectedOutputStatus(
        bool manifestOutputPresent,
        bool localArtifactPresent,
        bool checksumEntryPresent,
        bool digestRevalidated,
        bool digestMatched)
    {
        if (!manifestOutputPresent)
        {
            return "missing-manifest-output-not-revalidated";
        }

        if (!localArtifactPresent && !checksumEntryPresent)
        {
            return "missing-local-and-checksum-not-revalidated";
        }

        if (!localArtifactPresent)
        {
            return "missing-local-artifact-not-revalidated";
        }

        if (!checksumEntryPresent)
        {
            return "missing-checksum-entry-not-revalidated";
        }

        if (!digestRevalidated)
        {
            return "cross-referenced-not-revalidated";
        }

        return digestMatched
            ? "matched-revalidated"
            : "mismatched-revalidated";
    }

    private static string ArchiveEvidenceCrossReferenceStatus(
        int parsedPaths,
        int missingExpectedPaths,
        int missingLocalPaths,
        int missingChecksumPaths,
        int missingBuildManifestOutputPaths,
        int unexpectedPaths,
        int malformedPaths,
        bool archivePathMatchesOutput,
        bool archiveSha256MetadataPresent,
        bool archiveLengthMetadataPresent,
        bool archiveDigestRevalidated,
        bool archiveSha256MatchesLocal,
        bool archiveLengthMatchesLocal,
        bool archiveRevalidationMetadataPresent,
        bool archiveOpened,
        bool archiveRevalidationMatched)
    {
        if (malformedPaths > 0 ||
            parsedPaths == 0 ||
            !archiveSha256MetadataPresent ||
            !archiveLengthMetadataPresent ||
            !archiveRevalidationMetadataPresent)
        {
            return "malformed-not-validated";
        }

        if (missingExpectedPaths == 0 &&
            missingLocalPaths == 0 &&
            missingChecksumPaths == 0 &&
            missingBuildManifestOutputPaths == 0 &&
            unexpectedPaths == 0 &&
            archivePathMatchesOutput)
        {
            if (!archiveDigestRevalidated || !archiveSha256MatchesLocal || !archiveLengthMatchesLocal)
            {
                return "mismatch-digest-revalidated";
            }

            return archiveOpened && archiveRevalidationMatched
                ? "complete-archive-revalidated"
                : "mismatch-archive-revalidated";
        }

        return "mismatch-cross-referenced-not-revalidated";
    }

    private static string ArchiveEvidencePathStatus(bool expectedPath, bool localArtifactPresent, bool checksumEntryPresent, bool buildManifestOutputPresent)
    {
        if (!expectedPath)
        {
            return "unexpected-not-revalidated";
        }

        return ArchiveEvidenceExpectedPathStatus(
            evidenceMetadataPresent: true,
            localArtifactPresent,
            checksumEntryPresent,
            buildManifestOutputPresent);
    }

    private static string ArchiveEvidenceExpectedPathStatus(bool evidenceMetadataPresent, bool localArtifactPresent, bool checksumEntryPresent, bool buildManifestOutputPresent)
    {
        if (!evidenceMetadataPresent)
        {
            return "missing-archive-evidence-path-not-revalidated";
        }

        if (!localArtifactPresent || !checksumEntryPresent || !buildManifestOutputPresent)
        {
            return "missing-cross-reference-not-revalidated";
        }

        return "cross-referenced-not-revalidated";
    }

    private static bool LocalArtifactExists(IReadOnlyList<ReleasePublishEvidenceArtifact> artifacts, string path) =>
        artifacts.Any(artifact => artifact.Exists && StringComparer.Ordinal.Equals(artifact.Path, path));

    private static bool ChecksumEntryExists(ReleasePublishChecksumSidecar checksumSidecar, string checksumPath) =>
        checksumSidecar.Entries.Any(entry =>
            !entry.DuplicatePath &&
            StringComparer.Ordinal.Equals(entry.Path, checksumPath));

    private static bool BuildManifestOutputExists(ReleasePublishBuildManifestCrossReference buildManifestCrossReference, string path) =>
        buildManifestCrossReference.Outputs.Any(output =>
            !output.DuplicatePath &&
            StringComparer.Ordinal.Equals(output.Path, path));

    private static string ToChecksumSidecarPath(string buildManifestPath)
    {
        const string releasePreparePrefix = "dist/release-prepare/";
        return buildManifestPath.StartsWith(releasePreparePrefix, StringComparison.Ordinal)
            ? buildManifestPath[releasePreparePrefix.Length..]
            : buildManifestPath;
    }

    private static string ArtifactStatus(IReadOnlyList<ReleasePublishEvidenceArtifact> artifacts, string id)
    {
        var artifact = artifacts.FirstOrDefault(item => StringComparer.Ordinal.Equals(item.Id, id));
        return artifact?.Status ?? "missing";
    }

    private static string ChecksumRequirementStatus(IReadOnlyList<ReleasePublishEvidenceArtifact> artifacts, ReleasePublishChecksumSidecar checksumSidecar)
    {
        var artifactStatus = ArtifactStatus(artifacts, "release-prepare-checksums");
        return StringComparer.Ordinal.Equals(artifactStatus, "well-formed-not-validated")
            ? checksumSidecar.Status
            : artifactStatus;
    }

    private static string BuildManifestRequirementStatus(IReadOnlyList<ReleasePublishEvidenceArtifact> artifacts, ReleasePublishBuildManifestCrossReference buildManifestCrossReference)
    {
        var artifactStatus = ArtifactStatus(artifacts, "release-prepare-build-manifest");
        return StringComparer.Ordinal.Equals(artifactStatus, "well-formed-not-validated")
            ? buildManifestCrossReference.Status
            : artifactStatus;
    }

    private static string ArchiveEvidenceRequirementStatus(IReadOnlyList<ReleasePublishEvidenceArtifact> artifacts, ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference)
    {
        var artifactStatus = ArtifactStatus(artifacts, "release-prepare-archive-evidence");
        return StringComparer.Ordinal.Equals(artifactStatus, "well-formed-not-validated")
            ? archiveEvidenceCrossReference.Status
            : artifactStatus;
    }

    private static string GovernanceRequirementStatus(IReadOnlyList<ReleasePublishGovernanceCheck> governanceChecks)
    {
        if (governanceChecks.Any(check => StringComparer.Ordinal.Equals(check.Status, "failed")))
        {
            return "failed-governance-evaluated";
        }

        return governanceChecks.Any(check => StringComparer.Ordinal.Equals(check.Status, "missing-local-evidence"))
            ? "incomplete-governance-evaluated"
            : "complete-governance-evaluated";
    }

    private static bool TryReadText(string path, out string content)
    {
        try
        {
            content = File.ReadAllText(path);
            return true;
        }
        catch (IOException)
        {
            content = string.Empty;
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            content = string.Empty;
            return false;
        }
    }

    private static bool ContainsOrdinalIgnoreCase(string value, string expected) =>
        value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool IsSemVer(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        var coreVersion = version;
        var metadataIndex = coreVersion.IndexOf('+');
        if (metadataIndex >= 0)
        {
            coreVersion = coreVersion[..metadataIndex];
        }

        var prereleaseIndex = coreVersion.IndexOf('-');
        if (prereleaseIndex >= 0)
        {
            coreVersion = coreVersion[..prereleaseIndex];
        }

        var parts = coreVersion.Split('.');
        return parts.Length == 3 && parts.All(IsSemVerNumber);
    }

    private static bool IsSemVerNumber(string value) =>
        value.Length > 0 &&
        (value.Length == 1 || value[0] != '0') &&
        value.All(char.IsDigit);

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ToDisplayPath(string root, string path)
    {
        var relativePath = Path.GetRelativePath(root, path).Replace('\\', '/');
        return string.IsNullOrWhiteSpace(relativePath) ? "." : relativePath;
    }
}

internal static class ReleasePublishPreflightJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(ReleasePublishPreflightResult result)
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
            ["command"] = "release publish",
            ["status"] = result.Status,
            ["dryRun"] = result.DryRun,
            ["planningOnly"] = result.PlanningOnly,
            ["publishReady"] = result.PublishReady,
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["releasePrepareEvidence"] = new JsonObject
            {
                ["root"] = result.ReleasePrepareEvidenceRoot,
                ["status"] = result.ReleasePrepareEvidenceStatus,
                ["expectedArtifacts"] = result.ExpectedEvidenceArtifacts,
                ["presentArtifacts"] = result.PresentEvidenceArtifacts,
                ["missingArtifacts"] = result.MissingEvidenceArtifacts,
                ["wellFormedArtifacts"] = result.WellFormedEvidenceArtifacts,
                ["malformedArtifacts"] = result.MalformedEvidenceArtifacts,
                ["unclassifiedArtifacts"] = result.UnclassifiedEvidenceArtifacts,
                ["checksumExpectedEntries"] = result.ChecksumSidecar.ExpectedEntries,
                ["checksumCoveredExpectedEntries"] = result.ChecksumSidecar.CoveredExpectedEntries,
                ["checksumMissingExpectedEntries"] = result.ChecksumSidecar.MissingExpectedEntries,
                ["checksumUnexpectedEntries"] = result.ChecksumSidecar.UnexpectedEntries,
                ["checksumDuplicateEntries"] = result.ChecksumSidecar.DuplicateEntries,
                ["checksumMalformedEntries"] = result.ChecksumSidecar.MalformedEntries,
                ["checksumDigestRevalidatedEntries"] = result.ChecksumSidecar.DigestRevalidatedEntries,
                ["checksumDigestMatchedEntries"] = result.ChecksumSidecar.DigestMatchedEntries,
                ["checksumDigestMismatchedEntries"] = result.ChecksumSidecar.DigestMismatchedEntries,
                ["checksumMissingLocalFileEntries"] = result.ChecksumSidecar.MissingLocalFileEntries,
                ["buildManifestExpectedOutputs"] = result.BuildManifestCrossReference.ExpectedOutputCount,
                ["buildManifestParsedOutputs"] = result.BuildManifestCrossReference.ParsedOutputCount,
                ["buildManifestCoveredExpectedOutputs"] = result.BuildManifestCrossReference.CoveredExpectedOutputCount,
                ["buildManifestMissingExpectedOutputs"] = result.BuildManifestCrossReference.MissingExpectedOutputCount,
                ["buildManifestMissingLocalArtifactOutputs"] = result.BuildManifestCrossReference.MissingLocalArtifactOutputCount,
                ["buildManifestMissingChecksumEntryOutputs"] = result.BuildManifestCrossReference.MissingChecksumEntryOutputCount,
                ["buildManifestUnexpectedOutputs"] = result.BuildManifestCrossReference.UnexpectedOutputCount,
                ["buildManifestDuplicateOutputs"] = result.BuildManifestCrossReference.DuplicateOutputCount,
                ["buildManifestMalformedOutputs"] = result.BuildManifestCrossReference.MalformedOutputCount,
                ["buildManifestDigestRevalidatedOutputs"] = result.BuildManifestCrossReference.DigestRevalidatedOutputCount,
                ["buildManifestDigestMatchedOutputs"] = result.BuildManifestCrossReference.DigestMatchedOutputCount,
                ["buildManifestDigestMismatchedOutputs"] = result.BuildManifestCrossReference.DigestMismatchedOutputCount,
                ["archiveEvidenceExpectedPaths"] = result.ArchiveEvidenceCrossReference.ExpectedPathCount,
                ["archiveEvidenceParsedPaths"] = result.ArchiveEvidenceCrossReference.ParsedPathCount,
                ["archiveEvidenceCoveredExpectedPaths"] = result.ArchiveEvidenceCrossReference.CoveredExpectedPathCount,
                ["archiveEvidenceMissingExpectedPaths"] = result.ArchiveEvidenceCrossReference.MissingExpectedPathCount,
                ["archiveEvidenceMissingLocalArtifactPaths"] = result.ArchiveEvidenceCrossReference.MissingLocalArtifactPathCount,
                ["archiveEvidenceMissingChecksumEntryPaths"] = result.ArchiveEvidenceCrossReference.MissingChecksumEntryPathCount,
                ["archiveEvidenceMissingBuildManifestOutputPaths"] = result.ArchiveEvidenceCrossReference.MissingBuildManifestOutputPathCount,
                ["archiveEvidenceUnexpectedPaths"] = result.ArchiveEvidenceCrossReference.UnexpectedPathCount,
                ["archiveEvidenceMalformedPaths"] = result.ArchiveEvidenceCrossReference.MalformedPathCount,
                ["archiveEvidenceArchivePathMatchesOutput"] = result.ArchiveEvidenceCrossReference.ArchivePathMatchesOutput,
                ["archiveEvidenceArchiveSha256MatchesLocal"] = result.ArchiveEvidenceCrossReference.ArchiveSha256MatchesLocal,
                ["archiveEvidenceArchiveLengthMatchesLocal"] = result.ArchiveEvidenceCrossReference.ArchiveLengthMatchesLocal,
                ["archiveEvidenceArchiveOpenedInCurrentGate"] = result.ArchiveEvidenceCrossReference.ArchiveOpenedInCurrentGate,
                ["archiveEvidenceExpectedArchiveEntries"] = result.ArchiveEvidenceCrossReference.ExpectedArchiveEntryCount,
                ["archiveEvidenceActualArchiveEntries"] = result.ArchiveEvidenceCrossReference.ActualArchiveEntryCount,
                ["archiveEvidenceArchiveEntryCountMatchesMetadata"] = result.ArchiveEvidenceCrossReference.ArchiveEntryCountMatchesMetadata,
                ["archiveEvidenceArchiveEntryNamesMatchLocal"] = result.ArchiveEvidenceCrossReference.ArchiveEntryNamesMatchLocal,
                ["archiveEvidenceArchiveEntryOrderingMatchesLocal"] = result.ArchiveEvidenceCrossReference.ArchiveEntryOrderingMatchesLocal,
                ["archiveEvidenceArchiveDeterministicTimestampsMatchLocal"] = result.ArchiveEvidenceCrossReference.ArchiveDeterministicTimestampsMatchLocal,
                ["archiveEvidenceArchiveStoredCompressionMatchesLocal"] = result.ArchiveEvidenceCrossReference.ArchiveStoredCompressionMatchesLocal,
                ["semanticEvidenceStatus"] = result.SemanticEvidenceValidation.Status,
                ["semanticEvidenceExpectedChecks"] = result.SemanticEvidenceValidation.ExpectedChecks,
                ["semanticEvidencePassedChecks"] = result.SemanticEvidenceValidation.PassedChecks,
                ["semanticEvidenceFailedChecks"] = result.SemanticEvidenceValidation.FailedChecks,
                ["semanticEvidenceSkippedChecks"] = result.SemanticEvidenceValidation.SkippedChecks,
                ["schemaValidationEvidenceStatus"] = result.SchemaValidationEvidence.Status,
                ["capabilityEnvironmentEvidenceStatus"] = result.CapabilityEnvironmentEvidence.Status,
                ["capabilityEnvironmentRequiredUnavailable"] = result.CapabilityEnvironmentEvidence.RequiredUnavailable,
                ["capabilityEnvironmentCapabilityDiagnostics"] = result.CapabilityEnvironmentEvidence.CapabilityDiagnostics,
                ["packageValidationEvidenceStatus"] = result.PackageValidationEvidence.Status,
                ["packageValidationEvidenceErrors"] = result.PackageValidationEvidence.Errors,
                ["packageValidationEvidencePackageDiagnostics"] = result.PackageValidationEvidence.PackageIssueCount,
                ["releaseVerificationEvidenceStatus"] = result.ReleaseVerificationEvidence.Status,
                ["releaseVerificationEvidenceErrors"] = result.ReleaseVerificationEvidence.Errors,
                ["releaseVerificationEvidenceReleaseDiagnostics"] = result.ReleaseVerificationEvidence.ReleaseIssueCount,
                ["collectionPlanEvidenceStatus"] = result.CollectionPlanEvidence.Status,
                ["collectionPlanEvidenceSteps"] = result.CollectionPlanEvidence.Steps,
                ["collectionPlanEvidenceMalformedSteps"] = result.CollectionPlanEvidence.MalformedSteps,
                ["dryRunCrossLinkEvidenceStatus"] = result.DryRunCrossLinkEvidence.Status,
                ["dryRunCrossLinkEvidenceFiles"] = result.DryRunCrossLinkEvidence.PresentFiles,
                ["dryRunCrossLinkEvidenceMismatchedLinks"] = result.DryRunCrossLinkEvidence.MismatchedLinks,
                ["dryRunEvidenceRemediationStatus"] = result.DryRunEvidenceRemediation.Status,
                ["dryRunEvidenceRemediationActions"] = result.DryRunEvidenceRemediation.ActionItems,
                ["publishReadinessStatus"] = result.PublishReadiness.Status,
                ["publishReadinessLocalPreconditionsSatisfied"] = result.PublishReadiness.LocalPreconditionsSatisfied,
                ["publishReadinessBlockingChecks"] = result.PublishReadiness.BlockingChecks,
                ["artifactPathChecksInCurrentGate"] = true,
                ["artifactReadsInCurrentGate"] = result.LocalEvidenceArtifacts.Any(artifact => artifact.ContentReadInCurrentGate),
                ["contentShapeClassificationInCurrentGate"] = true,
                ["checksumEntryClassificationInCurrentGate"] = result.ChecksumSidecar.EntryClassificationInCurrentGate,
                ["checksumDigestRevalidationInCurrentGate"] = result.ChecksumSidecar.DigestRevalidationInCurrentGate,
                ["buildManifestOutputCrossReferenceInCurrentGate"] = result.BuildManifestCrossReference.OutputCrossReferenceInCurrentGate,
                ["buildManifestDigestRevalidationInCurrentGate"] = result.BuildManifestCrossReference.DigestRevalidationInCurrentGate,
                ["archiveEvidenceMetadataCrossReferenceInCurrentGate"] = result.ArchiveEvidenceCrossReference.MetadataCrossReferenceInCurrentGate,
                ["archiveEvidenceDigestRevalidationInCurrentGate"] = result.ArchiveEvidenceCrossReference.DigestRevalidationInCurrentGate,
                ["archiveEvidenceArchiveRevalidationInCurrentGate"] = result.ArchiveEvidenceCrossReference.ArchiveRevalidationInCurrentGate,
                ["semanticEvidenceValidationInCurrentGate"] = result.SemanticEvidenceValidation.ValidationInCurrentGate
            },
            ["checksumSidecar"] = ToChecksumSidecar(result.ChecksumSidecar),
            ["buildManifestCrossReference"] = ToBuildManifestCrossReference(result.BuildManifestCrossReference),
            ["archiveEvidenceCrossReference"] = ToArchiveEvidenceCrossReference(result.ArchiveEvidenceCrossReference),
            ["semanticEvidenceValidation"] = ToSemanticEvidenceValidation(result.SemanticEvidenceValidation),
            ["schemaValidationEvidence"] = ToSchemaValidationEvidence(result.SchemaValidationEvidence),
            ["capabilityEnvironmentEvidence"] = ToCapabilityEnvironmentEvidence(result.CapabilityEnvironmentEvidence),
            ["packageValidationEvidence"] = ToPackageValidationEvidence(result.PackageValidationEvidence),
            ["releaseVerificationEvidence"] = ToReleaseVerificationEvidence(result.ReleaseVerificationEvidence),
            ["collectionPlanEvidence"] = ToCollectionPlanEvidence(result.CollectionPlanEvidence),
            ["dryRunCrossLinkEvidence"] = ToDryRunCrossLinkEvidence(result.DryRunCrossLinkEvidence),
            ["dryRunEvidenceRemediation"] = ToDryRunEvidenceRemediation(result.DryRunEvidenceRemediation),
            ["requiredEvidence"] = ToEvidenceArray(result.RequiredEvidence),
            ["localEvidenceArtifacts"] = ToArtifactArray(result.LocalEvidenceArtifacts),
            ["governanceChecks"] = ToGovernanceCheckArray(result.GovernanceChecks),
            ["approval"] = new JsonObject
            {
                ["required"] = result.Approval.Required,
                ["provided"] = result.Approval.Provided,
                ["yesProvided"] = result.Approval.YesProvided,
                ["confirmationValue"] = result.Approval.ConfirmationValue,
                ["confirmationValidated"] = result.Approval.ConfirmationValidated,
                ["confirmationMatches"] = result.Approval.ConfirmationMatches,
                ["projectManifestRead"] = result.Approval.ProjectManifestRead,
                ["projectManifestPath"] = result.Approval.ProjectManifestPath,
                ["projectId"] = result.Approval.ProjectId,
                ["status"] = result.Approval.Status,
                ["description"] = result.Approval.Description,
                ["detail"] = result.Approval.Detail
            },
            ["publishReadiness"] = ToPublishReadiness(result.PublishReadiness),
            ["laneCloseout"] = ToLaneCloseout(result.LaneCloseout),
            ["reportContract"] = new JsonObject
            {
                ["status"] = result.Status,
                ["canonicalFormat"] = "json",
                ["mutatesFilesystemInCurrentGate"] = false,
                ["summary"] = "Release publish aggregates local publish-readiness, closes the no-publish preflight lane, routes the next local value slice, and still refuses real publishing."
            },
            ["execution"] = ToExecution(result),
            ["boundaries"] = ToStringArray(result.Boundaries)
        };

        if (result.RefusalReason is not null)
        {
            root["refusalReason"] = result.RefusalReason;
        }

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToChecksumSidecar(ReleasePublishChecksumSidecar sidecar) =>
        new()
        {
            ["path"] = sidecar.Path,
            ["exists"] = sidecar.Exists,
            ["status"] = sidecar.Status,
            ["entryClassificationInCurrentGate"] = sidecar.EntryClassificationInCurrentGate,
            ["digestRevalidationInCurrentGate"] = sidecar.DigestRevalidationInCurrentGate,
            ["expectedEntries"] = sidecar.ExpectedEntries,
            ["parsedEntries"] = sidecar.ParsedEntries,
            ["coveredExpectedEntries"] = sidecar.CoveredExpectedEntries,
            ["missingExpectedEntries"] = sidecar.MissingExpectedEntries,
            ["unexpectedEntries"] = sidecar.UnexpectedEntries,
            ["duplicateEntries"] = sidecar.DuplicateEntries,
            ["malformedEntries"] = sidecar.MalformedEntries,
            ["digestRevalidatedEntries"] = sidecar.DigestRevalidatedEntries,
            ["digestMatchedEntries"] = sidecar.DigestMatchedEntries,
            ["digestMismatchedEntries"] = sidecar.DigestMismatchedEntries,
            ["missingLocalFileEntries"] = sidecar.MissingLocalFileEntries,
            ["expectedPaths"] = ToChecksumExpectedPathArray(sidecar.ExpectedPaths),
            ["entries"] = ToChecksumEntryArray(sidecar.Entries)
        };

    private static JsonArray ToChecksumExpectedPathArray(IReadOnlyList<ReleasePublishChecksumExpectedPath> expectedPaths)
    {
        var array = new JsonArray();
        foreach (var expectedPath in expectedPaths)
        {
            array.Add(new JsonObject
            {
                ["path"] = expectedPath.Path,
                ["status"] = expectedPath.Status,
                ["entryPresent"] = expectedPath.EntryPresent,
                ["localFilePresent"] = expectedPath.LocalFilePresent,
                ["digestRevalidatedInCurrentGate"] = expectedPath.DigestRevalidatedInCurrentGate,
                ["expectedSha256"] = expectedPath.ExpectedSha256,
                ["actualSha256"] = expectedPath.ActualSha256
            });
        }

        return array;
    }

    private static JsonArray ToChecksumEntryArray(IReadOnlyList<ReleasePublishChecksumEntry> entries)
    {
        var array = new JsonArray();
        foreach (var entry in entries)
        {
            array.Add(new JsonObject
            {
                ["lineNumber"] = entry.LineNumber,
                ["sha256"] = entry.Sha256,
                ["actualSha256"] = entry.ActualSha256,
                ["path"] = entry.Path,
                ["status"] = entry.Status,
                ["expectedPath"] = entry.ExpectedPath,
                ["duplicatePath"] = entry.DuplicatePath,
                ["localFilePresent"] = entry.LocalFilePresent,
                ["digestRevalidatedInCurrentGate"] = entry.DigestRevalidatedInCurrentGate,
                ["shapeDetail"] = entry.ShapeDetail
            });
        }

        return array;
    }

    private static JsonObject ToBuildManifestCrossReference(ReleasePublishBuildManifestCrossReference crossReference) =>
        new()
        {
            ["path"] = crossReference.Path,
            ["exists"] = crossReference.Exists,
            ["status"] = crossReference.Status,
            ["outputCrossReferenceInCurrentGate"] = crossReference.OutputCrossReferenceInCurrentGate,
            ["contentReadInCurrentGate"] = crossReference.ContentReadInCurrentGate,
            ["digestRevalidationInCurrentGate"] = crossReference.DigestRevalidationInCurrentGate,
            ["expectedOutputs"] = crossReference.ExpectedOutputCount,
            ["parsedOutputs"] = crossReference.ParsedOutputCount,
            ["coveredExpectedOutputs"] = crossReference.CoveredExpectedOutputCount,
            ["missingExpectedOutputs"] = crossReference.MissingExpectedOutputCount,
            ["missingLocalArtifactOutputs"] = crossReference.MissingLocalArtifactOutputCount,
            ["missingChecksumEntryOutputs"] = crossReference.MissingChecksumEntryOutputCount,
            ["unexpectedOutputs"] = crossReference.UnexpectedOutputCount,
            ["duplicateOutputs"] = crossReference.DuplicateOutputCount,
            ["malformedOutputs"] = crossReference.MalformedOutputCount,
            ["digestRevalidatedOutputs"] = crossReference.DigestRevalidatedOutputCount,
            ["digestMatchedOutputs"] = crossReference.DigestMatchedOutputCount,
            ["digestMismatchedOutputs"] = crossReference.DigestMismatchedOutputCount,
            ["expectedPaths"] = ToBuildManifestExpectedPathArray(crossReference.ExpectedPaths),
            ["outputs"] = ToBuildManifestOutputArray(crossReference.Outputs)
        };

    private static JsonArray ToBuildManifestExpectedPathArray(IReadOnlyList<ReleasePublishBuildManifestExpectedOutput> expectedPaths)
    {
        var array = new JsonArray();
        foreach (var expectedPath in expectedPaths)
        {
            array.Add(new JsonObject
            {
                ["path"] = expectedPath.Path,
                ["checksumPath"] = expectedPath.ChecksumPath,
                ["status"] = expectedPath.Status,
                ["manifestOutputPresent"] = expectedPath.ManifestOutputPresent,
                ["localArtifactPresent"] = expectedPath.LocalArtifactPresent,
                ["checksumEntryPresent"] = expectedPath.ChecksumEntryPresent,
                ["digestRevalidatedInCurrentGate"] = expectedPath.DigestRevalidatedInCurrentGate,
                ["expectedSha256"] = expectedPath.ExpectedSha256,
                ["actualSha256"] = expectedPath.ActualSha256
            });
        }

        return array;
    }

    private static JsonArray ToBuildManifestOutputArray(IReadOnlyList<ReleasePublishBuildManifestOutput> outputs)
    {
        var array = new JsonArray();
        foreach (var output in outputs)
        {
            array.Add(new JsonObject
            {
                ["index"] = output.Index,
                ["path"] = output.Path,
                ["checksumPath"] = output.ChecksumPath,
                ["sha256"] = output.Sha256,
                ["actualSha256"] = output.ActualSha256,
                ["status"] = output.Status,
                ["expectedPath"] = output.ExpectedPath,
                ["duplicatePath"] = output.DuplicatePath,
                ["localArtifactPresent"] = output.LocalArtifactPresent,
                ["checksumEntryPresent"] = output.ChecksumEntryPresent,
                ["digestRevalidatedInCurrentGate"] = output.DigestRevalidatedInCurrentGate,
                ["shapeDetail"] = output.ShapeDetail
            });
        }

        return array;
    }

    private static JsonObject ToArchiveEvidenceCrossReference(ReleasePublishArchiveEvidenceCrossReference crossReference) =>
        new()
        {
            ["path"] = crossReference.Path,
            ["exists"] = crossReference.Exists,
            ["status"] = crossReference.Status,
            ["metadataCrossReferenceInCurrentGate"] = crossReference.MetadataCrossReferenceInCurrentGate,
            ["contentReadInCurrentGate"] = crossReference.ContentReadInCurrentGate,
            ["digestRevalidationInCurrentGate"] = crossReference.DigestRevalidationInCurrentGate,
            ["archiveRevalidationInCurrentGate"] = crossReference.ArchiveRevalidationInCurrentGate,
            ["expectedPathCount"] = crossReference.ExpectedPathCount,
            ["parsedPaths"] = crossReference.ParsedPathCount,
            ["coveredExpectedPaths"] = crossReference.CoveredExpectedPathCount,
            ["missingExpectedPaths"] = crossReference.MissingExpectedPathCount,
            ["missingLocalArtifactPaths"] = crossReference.MissingLocalArtifactPathCount,
            ["missingChecksumEntryPaths"] = crossReference.MissingChecksumEntryPathCount,
            ["missingBuildManifestOutputPaths"] = crossReference.MissingBuildManifestOutputPathCount,
            ["unexpectedPaths"] = crossReference.UnexpectedPathCount,
            ["malformedPaths"] = crossReference.MalformedPathCount,
            ["archivePathMatchesOutput"] = crossReference.ArchivePathMatchesOutput,
            ["archiveSha256MetadataPresent"] = crossReference.ArchiveSha256MetadataPresent,
            ["archiveLengthMetadataPresent"] = crossReference.ArchiveLengthMetadataPresent,
            ["archiveSha256MatchesLocal"] = crossReference.ArchiveSha256MatchesLocal,
            ["archiveLengthMatchesLocal"] = crossReference.ArchiveLengthMatchesLocal,
            ["expectedArchiveSha256"] = crossReference.ExpectedArchiveSha256,
            ["actualArchiveSha256"] = crossReference.ActualArchiveSha256,
            ["expectedArchiveLength"] = crossReference.ExpectedArchiveLength,
            ["actualArchiveLength"] = crossReference.ActualArchiveLength,
            ["archiveOpenedInCurrentGate"] = crossReference.ArchiveOpenedInCurrentGate,
            ["expectedArchiveEntries"] = crossReference.ExpectedArchiveEntryCount,
            ["evidenceActualArchiveEntries"] = crossReference.EvidenceActualArchiveEntryCount,
            ["actualArchiveEntries"] = crossReference.ActualArchiveEntryCount,
            ["archiveEntryCountMatchesMetadata"] = crossReference.ArchiveEntryCountMatchesMetadata,
            ["archiveEntryNamesMatchLocal"] = crossReference.ArchiveEntryNamesMatchLocal,
            ["archiveEvidenceActualEntryNamesMatchLocal"] = crossReference.ArchiveEvidenceActualEntryNamesMatchLocal,
            ["archiveEntryOrderingMatchesLocal"] = crossReference.ArchiveEntryOrderingMatchesLocal,
            ["archiveDeterministicTimestampsMatchLocal"] = crossReference.ArchiveDeterministicTimestampsMatchLocal,
            ["archiveStoredCompressionMatchesLocal"] = crossReference.ArchiveStoredCompressionMatchesLocal,
            ["archiveRevalidationDetail"] = crossReference.ArchiveRevalidationDetail,
            ["archiveEntries"] = ToArchiveEntryRevalidationArray(crossReference.ArchiveEntries),
            ["expectedPaths"] = ToArchiveEvidenceExpectedPathArray(crossReference.ExpectedPaths),
            ["paths"] = ToArchiveEvidencePathArray(crossReference.Paths)
        };

    private static JsonArray ToArchiveEntryRevalidationArray(IReadOnlyList<ReleasePublishArchiveEntryRevalidation> entries)
    {
        var array = new JsonArray();
        foreach (var entry in entries)
        {
            array.Add(new JsonObject
            {
                ["index"] = entry.Index,
                ["path"] = entry.Path,
                ["length"] = entry.Length,
                ["compressedLength"] = entry.CompressedLength,
                ["lastWriteTimeUtc"] = entry.LastWriteTimeUtc,
                ["status"] = entry.Status,
                ["expectedPath"] = entry.ExpectedPath,
                ["expectedAtIndex"] = entry.ExpectedAtIndex,
                ["evidenceActualAtIndex"] = entry.EvidenceActualAtIndex,
                ["timestampMatches"] = entry.TimestampMatches,
                ["stored"] = entry.Stored
            });
        }

        return array;
    }

    private static JsonArray ToArchiveEvidenceExpectedPathArray(IReadOnlyList<ReleasePublishArchiveEvidenceExpectedPath> expectedPaths)
    {
        var array = new JsonArray();
        foreach (var expectedPath in expectedPaths)
        {
            array.Add(new JsonObject
            {
                ["role"] = expectedPath.Role,
                ["path"] = expectedPath.Path,
                ["checksumPath"] = expectedPath.ChecksumPath,
                ["status"] = expectedPath.Status,
                ["evidenceMetadataPresent"] = expectedPath.EvidenceMetadataPresent,
                ["localArtifactPresent"] = expectedPath.LocalArtifactPresent,
                ["checksumEntryPresent"] = expectedPath.ChecksumEntryPresent,
                ["buildManifestOutputPresent"] = expectedPath.BuildManifestOutputPresent
            });
        }

        return array;
    }

    private static JsonArray ToArchiveEvidencePathArray(IReadOnlyList<ReleasePublishArchiveEvidencePath> paths)
    {
        var array = new JsonArray();
        foreach (var path in paths)
        {
            array.Add(new JsonObject
            {
                ["role"] = path.Role,
                ["path"] = path.Path,
                ["checksumPath"] = path.ChecksumPath,
                ["status"] = path.Status,
                ["expectedPath"] = path.ExpectedPath,
                ["localArtifactPresent"] = path.LocalArtifactPresent,
                ["checksumEntryPresent"] = path.ChecksumEntryPresent,
                ["buildManifestOutputPresent"] = path.BuildManifestOutputPresent,
                ["digestRevalidatedInCurrentGate"] = path.DigestRevalidatedInCurrentGate,
                ["archiveRevalidatedInCurrentGate"] = path.ArchiveRevalidatedInCurrentGate,
                ["shapeDetail"] = path.ShapeDetail
            });
        }

        return array;
    }

    private static JsonObject ToSemanticEvidenceValidation(ReleasePublishSemanticEvidenceValidation validation) =>
        new()
        {
            ["status"] = validation.Status,
            ["validationInCurrentGate"] = validation.ValidationInCurrentGate,
            ["contentReadInCurrentGate"] = validation.ContentReadInCurrentGate,
            ["expectedChecks"] = validation.ExpectedChecks,
            ["passedChecks"] = validation.PassedChecks,
            ["failedChecks"] = validation.FailedChecks,
            ["skippedChecks"] = validation.SkippedChecks,
            ["detail"] = validation.Detail,
            ["checks"] = ToSemanticEvidenceCheckArray(validation.Checks)
        };

    private static JsonArray ToSemanticEvidenceCheckArray(IReadOnlyList<ReleasePublishSemanticEvidenceCheck> checks)
    {
        var array = new JsonArray();
        foreach (var check in checks)
        {
            array.Add(new JsonObject
            {
                ["id"] = check.Id,
                ["title"] = check.Title,
                ["path"] = check.Path,
                ["status"] = check.Status,
                ["detail"] = check.Detail
            });
        }

        return array;
    }

    private static JsonObject ToSchemaValidationEvidence(ReleasePublishSchemaValidationEvidence evidence) =>
        new()
        {
            ["path"] = evidence.Path,
            ["exists"] = evidence.Exists,
            ["status"] = evidence.Status,
            ["checkedInCurrentGate"] = evidence.CheckedInCurrentGate,
            ["contentReadInCurrentGate"] = evidence.ContentReadInCurrentGate,
            ["errors"] = evidence.Errors,
            ["warnings"] = evidence.Warnings,
            ["notes"] = evidence.Notes,
            ["issues"] = evidence.IssueCount,
            ["schemaIssues"] = evidence.SchemaIssueCount,
            ["detail"] = evidence.Detail
        };

    private static JsonObject ToCapabilityEnvironmentEvidence(ReleasePublishCapabilityEnvironmentEvidence evidence) =>
        new()
        {
            ["path"] = evidence.Path,
            ["exists"] = evidence.Exists,
            ["status"] = evidence.Status,
            ["checkedInCurrentGate"] = evidence.CheckedInCurrentGate,
            ["contentReadInCurrentGate"] = evidence.ContentReadInCurrentGate,
            ["projectScoped"] = evidence.ProjectScoped,
            ["runtimeProbesEnabled"] = evidence.RuntimeProbesEnabled,
            ["mo2VfsEnabled"] = evidence.Mo2VfsEnabled,
            ["providers"] = evidence.Providers,
            ["capabilities"] = evidence.Capabilities,
            ["missingProviders"] = evidence.MissingProviders,
            ["unknownProviders"] = evidence.UnknownProviders,
            ["wrongScopeProviders"] = evidence.WrongScopeProviders,
            ["missingCapabilities"] = evidence.MissingCapabilities,
            ["unknownCapabilities"] = evidence.UnknownCapabilities,
            ["wrongScopeCapabilities"] = evidence.WrongScopeCapabilities,
            ["requirements"] = evidence.Requirements,
            ["requiredUnavailable"] = evidence.RequiredUnavailable,
            ["optionalUnavailable"] = evidence.OptionalUnavailable,
            ["diagnostics"] = evidence.Diagnostics,
            ["capabilityDiagnostics"] = evidence.CapabilityDiagnostics,
            ["doctorAreas"] = evidence.DoctorAreas,
            ["doctorActionNeededAreas"] = evidence.DoctorActionNeededAreas,
            ["doctorUnknownAreas"] = evidence.DoctorUnknownAreas,
            ["detail"] = evidence.Detail
        };

    private static JsonObject ToPackageValidationEvidence(ReleasePublishPackageValidationEvidence evidence) =>
        new()
        {
            ["path"] = evidence.Path,
            ["exists"] = evidence.Exists,
            ["status"] = evidence.Status,
            ["checkedInCurrentGate"] = evidence.CheckedInCurrentGate,
            ["contentReadInCurrentGate"] = evidence.ContentReadInCurrentGate,
            ["target"] = evidence.Target,
            ["mode"] = evidence.Mode,
            ["outputRoot"] = evidence.OutputRoot,
            ["distScoped"] = evidence.DistScoped,
            ["packageArchivePresent"] = evidence.PackageArchivePresent,
            ["errors"] = evidence.Errors,
            ["warnings"] = evidence.Warnings,
            ["notes"] = evidence.Notes,
            ["issues"] = evidence.IssueCount,
            ["packageIssues"] = evidence.PackageIssueCount,
            ["detail"] = evidence.Detail
        };

    private static JsonObject ToReleaseVerificationEvidence(ReleasePublishReleaseVerificationEvidence evidence) =>
        new()
        {
            ["path"] = evidence.Path,
            ["exists"] = evidence.Exists,
            ["status"] = evidence.Status,
            ["checkedInCurrentGate"] = evidence.CheckedInCurrentGate,
            ["contentReadInCurrentGate"] = evidence.ContentReadInCurrentGate,
            ["dryRun"] = evidence.DryRun,
            ["outputRoot"] = evidence.OutputRoot,
            ["distScoped"] = evidence.DistScoped,
            ["errors"] = evidence.Errors,
            ["warnings"] = evidence.Warnings,
            ["notes"] = evidence.Notes,
            ["issues"] = evidence.IssueCount,
            ["releaseIssues"] = evidence.ReleaseIssueCount,
            ["detail"] = evidence.Detail
        };

    private static JsonObject ToCollectionPlanEvidence(ReleasePublishCollectionPlanEvidence evidence) =>
        new()
        {
            ["path"] = evidence.Path,
            ["exists"] = evidence.Exists,
            ["status"] = evidence.Status,
            ["checkedInCurrentGate"] = evidence.CheckedInCurrentGate,
            ["contentReadInCurrentGate"] = evidence.ContentReadInCurrentGate,
            ["dryRun"] = evidence.DryRun,
            ["outputRoot"] = evidence.OutputRoot,
            ["distScoped"] = evidence.DistScoped,
            ["linksEvidenceIndex"] = evidence.LinksEvidenceIndex,
            ["linksEvidenceStatus"] = evidence.LinksEvidenceStatus,
            ["linksActionChecklist"] = evidence.LinksActionChecklist,
            ["linksHandoffSummary"] = evidence.LinksHandoffSummary,
            ["steps"] = evidence.Steps,
            ["manualSteps"] = evidence.ManualSteps,
            ["availableSteps"] = evidence.AvailableSteps,
            ["malformedSteps"] = evidence.MalformedSteps,
            ["noExecutionBoundary"] = evidence.NoExecutionBoundary,
            ["detail"] = evidence.Detail
        };

    private static JsonObject ToDryRunCrossLinkEvidence(ReleasePublishDryRunCrossLinkEvidence evidence) =>
        new()
        {
            ["status"] = evidence.Status,
            ["checkedInCurrentGate"] = evidence.CheckedInCurrentGate,
            ["contentReadInCurrentGate"] = evidence.ContentReadInCurrentGate,
            ["expectedFiles"] = evidence.ExpectedFiles,
            ["presentFiles"] = evidence.PresentFiles,
            ["missingFiles"] = evidence.MissingFiles,
            ["malformedFiles"] = evidence.MalformedFiles,
            ["expectedLinks"] = evidence.ExpectedLinks,
            ["validLinks"] = evidence.ValidLinks,
            ["mismatchedLinks"] = evidence.MismatchedLinks,
            ["requiredEvidenceEntries"] = evidence.RequiredEvidenceEntries,
            ["statusEntries"] = evidence.StatusEntries,
            ["actionItems"] = evidence.ActionItems,
            ["collectionSteps"] = evidence.CollectionSteps,
            ["identityConsistent"] = evidence.IdentityConsistent,
            ["summaryCountersConsistent"] = evidence.SummaryCountersConsistent,
            ["requiredEvidenceConsistent"] = evidence.RequiredEvidenceConsistent,
            ["actionReferencesConsistent"] = evidence.ActionReferencesConsistent,
            ["collectionStepsConsistent"] = evidence.CollectionStepsConsistent,
            ["executionBoundariesConsistent"] = evidence.ExecutionBoundariesConsistent,
            ["handoffReferencesConsistent"] = evidence.HandoffReferencesConsistent,
            ["detail"] = evidence.Detail
        };

    private static JsonObject ToDryRunEvidenceRemediation(ReleasePublishDryRunEvidenceRemediation remediation) =>
        new()
        {
            ["status"] = remediation.Status,
            ["checkedInCurrentGate"] = remediation.CheckedInCurrentGate,
            ["requiresOperatorAction"] = remediation.RequiresOperatorAction,
            ["sourceStatus"] = remediation.SourceStatus,
            ["actionItems"] = remediation.ActionItems,
            ["commandHints"] = remediation.CommandHints,
            ["affectedPaths"] = remediation.AffectedPaths,
            ["blockingIssues"] = remediation.BlockingIssues,
            ["recommendedCommand"] = remediation.RecommendedCommand,
            ["detail"] = remediation.Detail,
            ["targetPaths"] = ToStringArray(remediation.TargetPaths),
            ["items"] = ToDryRunEvidenceRemediationItems(remediation.Items)
        };

    private static JsonArray ToDryRunEvidenceRemediationItems(IReadOnlyList<ReleasePublishDryRunEvidenceRemediationItem> items)
    {
        var array = new JsonArray();
        foreach (var item in items)
        {
            array.Add(new JsonObject
            {
                ["id"] = item.Id,
                ["priority"] = item.Priority,
                ["category"] = item.Category,
                ["status"] = item.Status,
                ["reason"] = item.Reason,
                ["commandHint"] = item.CommandHint,
                ["execution"] = item.Execution,
                ["blocksPublishReadiness"] = item.BlocksPublishReadiness,
                ["targetPaths"] = ToStringArray(item.TargetPaths)
            });
        }

        return array;
    }

    private static JsonArray ToArtifactArray(IReadOnlyList<ReleasePublishEvidenceArtifact> artifacts)
    {
        var array = new JsonArray();
        foreach (var artifact in artifacts)
        {
            array.Add(new JsonObject
            {
                ["id"] = artifact.Id,
                ["path"] = artifact.Path,
                ["kind"] = artifact.Kind,
                ["contentKind"] = artifact.ContentKind,
                ["required"] = artifact.Required,
                ["exists"] = artifact.Exists,
                ["status"] = artifact.Status,
                ["length"] = artifact.Length,
                ["shapeCheckedInCurrentGate"] = artifact.ShapeCheckedInCurrentGate,
                ["contentReadInCurrentGate"] = artifact.ContentReadInCurrentGate,
                ["shapeDetail"] = artifact.ShapeDetail
            });
        }

        return array;
    }

    private static JsonArray ToEvidenceArray(IReadOnlyList<ReleasePublishEvidenceRequirement> requirements)
    {
        var array = new JsonArray();
        foreach (var requirement in requirements)
        {
            array.Add(new JsonObject
            {
                ["id"] = requirement.Id,
                ["title"] = requirement.Title,
                ["source"] = requirement.Source,
                ["status"] = requirement.Status,
                ["required"] = requirement.Required,
                ["checkedInCurrentGate"] = requirement.CheckedInCurrentGate
            });
        }

        return array;
    }

    private static JsonArray ToGovernanceCheckArray(IReadOnlyList<ReleasePublishGovernanceCheck> checks)
    {
        var array = new JsonArray();
        foreach (var check in checks)
        {
            array.Add(new JsonObject
            {
                ["id"] = check.Id,
                ["title"] = check.Title,
                ["status"] = check.Status,
                ["required"] = check.Required,
                ["checkedInCurrentGate"] = check.CheckedInCurrentGate,
                ["evidencePath"] = check.EvidencePath,
                ["detail"] = check.Detail
            });
        }

        return array;
    }

    private static JsonObject ToPublishReadiness(ReleasePublishReadiness readiness) =>
        new()
        {
            ["evaluatedInCurrentGate"] = readiness.EvaluatedInCurrentGate,
            ["status"] = readiness.Status,
            ["localPreconditionsSatisfied"] = readiness.LocalPreconditionsSatisfied,
            ["evidenceSatisfied"] = readiness.EvidenceSatisfied,
            ["governanceSatisfied"] = readiness.GovernanceSatisfied,
            ["approvalSatisfied"] = readiness.ApprovalSatisfied,
            ["publishExecutionEnabled"] = readiness.PublishExecutionEnabled,
            ["readyForRealPublish"] = readiness.ReadyForRealPublish,
            ["requiredChecks"] = readiness.RequiredChecks,
            ["satisfiedChecks"] = readiness.SatisfiedChecks,
            ["blockingChecks"] = readiness.BlockingChecks,
            ["detail"] = readiness.Detail,
            ["blockingCheckIds"] = ToStringArray(readiness.BlockingCheckIds),
            ["checks"] = ToReadinessCheckArray(readiness.Checks)
        };

    private static JsonObject ToLaneCloseout(ReleasePublishLaneCloseout closeout) =>
        new()
        {
            ["closedInCurrentGate"] = closeout.ClosedInCurrentGate,
            ["status"] = closeout.Status,
            ["lane"] = closeout.Lane,
            ["completedThroughGate"] = closeout.CompletedThroughGate,
            ["nextGate"] = closeout.NextGate,
            ["nextValueSlice"] = closeout.NextValueSlice,
            ["detail"] = closeout.Detail,
            ["deferredCapabilities"] = ToStringArray(closeout.DeferredCapabilities)
        };

    private static JsonArray ToReadinessCheckArray(IReadOnlyList<ReleasePublishReadinessCheck> checks)
    {
        var array = new JsonArray();
        foreach (var check in checks)
        {
            array.Add(new JsonObject
            {
                ["id"] = check.Id,
                ["title"] = check.Title,
                ["source"] = check.Source,
                ["status"] = check.Status,
                ["required"] = check.Required,
                ["satisfied"] = check.Satisfied,
                ["detail"] = check.Detail
            });
        }

        return array;
    }

    private static JsonObject ToExecution(ReleasePublishPreflightResult result) =>
        new()
        {
            ["releasePublishPreflightPlanning"] = true,
            ["releasePublishExecution"] = false,
            ["releasePublishing"] = false,
            ["publishReady"] = result.PublishReady,
            ["publishReadinessEvaluation"] = result.PublishReadiness.EvaluatedInCurrentGate,
            ["localPublishPreconditionsSatisfied"] = result.PublishReadiness.LocalPreconditionsSatisfied,
            ["publishReadyForRealPublish"] = result.PublishReadiness.ReadyForRealPublish,
            ["releasePublishLaneCloseout"] = result.LaneCloseout.ClosedInCurrentGate,
            ["nextValueSliceRouted"] = result.LaneCloseout.ClosedInCurrentGate,
            ["evidenceArtifactPathCheck"] = true,
            ["evidenceArtifactRead"] = result.LocalEvidenceArtifacts.Any(artifact => artifact.ContentReadInCurrentGate),
            ["artifactExistenceCheck"] = true,
            ["contentShapeClassification"] = true,
            ["checksumEntryClassification"] = true,
            ["checksumDigestRevalidation"] = result.ChecksumSidecar.DigestRevalidationInCurrentGate,
            ["buildManifestOutputCrossReference"] = true,
            ["archiveEvidenceMetadataCrossReference"] = true,
            ["checksumRevalidation"] = result.ChecksumSidecar.DigestRevalidationInCurrentGate,
            ["buildManifestDigestRevalidation"] = result.BuildManifestCrossReference.DigestRevalidationInCurrentGate,
            ["archiveEvidenceDigestRevalidation"] = result.ArchiveEvidenceCrossReference.DigestRevalidationInCurrentGate,
            ["semanticEvidenceValidation"] = result.SemanticEvidenceValidation.ValidationInCurrentGate,
            ["archiveRevalidation"] = result.ArchiveEvidenceCrossReference.ArchiveRevalidationInCurrentGate,
            ["schemaValidationEvidenceEvaluation"] = result.SchemaValidationEvidence.CheckedInCurrentGate,
            ["capabilityEnvironmentEvidenceEvaluation"] = result.CapabilityEnvironmentEvidence.CheckedInCurrentGate,
            ["packageValidationEvidenceEvaluation"] = result.PackageValidationEvidence.CheckedInCurrentGate,
            ["releaseVerificationEvidenceEvaluation"] = result.ReleaseVerificationEvidence.CheckedInCurrentGate,
            ["collectionPlanEvidenceEvaluation"] = result.CollectionPlanEvidence.CheckedInCurrentGate,
            ["dryRunCrossLinkEvidenceEvaluation"] = result.DryRunCrossLinkEvidence.CheckedInCurrentGate,
            ["dryRunEvidenceRemediationEvaluation"] = result.DryRunEvidenceRemediation.CheckedInCurrentGate,
            ["humanApprovalEvaluation"] = true,
            ["humanApprovalConfirmationValidated"] = result.Approval.ConfirmationValidated,
            ["governanceCheckExecution"] = true,
            ["humanApprovalProvided"] = result.Approval.Provided,
            ["filesystemMutation"] = false,
            ["outputWrites"] = false,
            ["remoteRepositoryCall"] = false,
            ["releaseUpload"] = false,
            ["attestationSigning"] = false,
            ["externalToolExecution"] = false,
            ["pluginMutation"] = false,
            ["mo2Automation"] = false,
            ["geckAutomation"] = false,
            ["runtimeProbe"] = false,
            ["aiRequired"] = false
        };

    private static JsonArray ToStringArray(IReadOnlyList<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }
}

internal static class ReleasePublishPreflightTextRenderer
{
    public static string Render(ReleasePublishPreflightResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Release Publish Preflight");
        builder.Append("Status: ");
        builder.AppendLine(result.Status);
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectRoot);
        builder.Append("Release prepare evidence root: ");
        builder.AppendLine(result.ReleasePrepareEvidenceRoot);
        builder.Append("Release prepare evidence status: ");
        builder.AppendLine(result.ReleasePrepareEvidenceStatus);
        builder.Append("Local evidence artifacts: ");
        builder.Append(result.PresentEvidenceArtifacts);
        builder.Append('/');
        builder.Append(result.ExpectedEvidenceArtifacts);
        builder.AppendLine(" present");
        builder.Append("Content shape: ");
        builder.Append(result.WellFormedEvidenceArtifacts);
        builder.Append(" well-formed, ");
        builder.Append(result.MalformedEvidenceArtifacts);
        builder.Append(" malformed, ");
        builder.Append(result.UnclassifiedEvidenceArtifacts);
        builder.AppendLine(" unclassified");
        builder.Append("Checksum sidecar: ");
        builder.Append(result.ChecksumSidecar.Status);
        builder.Append(" (");
        builder.Append(result.ChecksumSidecar.CoveredExpectedEntries);
        builder.Append('/');
        builder.Append(result.ChecksumSidecar.ExpectedEntries);
        builder.Append(" expected paths covered, ");
        builder.Append(result.ChecksumSidecar.UnexpectedEntries);
        builder.Append(" unexpected, ");
        builder.Append(result.ChecksumSidecar.DuplicateEntries);
        builder.Append(" duplicate, ");
        builder.Append(result.ChecksumSidecar.MalformedEntries);
        builder.Append(" malformed, ");
        builder.Append(result.ChecksumSidecar.DigestMatchedEntries);
        builder.Append(" digest matched, ");
        builder.Append(result.ChecksumSidecar.DigestMismatchedEntries);
        builder.Append(" digest mismatched, ");
        builder.Append(result.ChecksumSidecar.MissingLocalFileEntries);
        builder.AppendLine(" missing local files)");
        builder.Append("Build manifest cross-reference: ");
        builder.Append(result.BuildManifestCrossReference.Status);
        builder.Append(" (");
        builder.Append(result.BuildManifestCrossReference.CoveredExpectedOutputCount);
        builder.Append('/');
        builder.Append(result.BuildManifestCrossReference.ExpectedOutputCount);
        builder.Append(" expected outputs covered, ");
        builder.Append(result.BuildManifestCrossReference.UnexpectedOutputCount);
        builder.Append(" unexpected, ");
        builder.Append(result.BuildManifestCrossReference.DuplicateOutputCount);
        builder.Append(" duplicate, ");
        builder.Append(result.BuildManifestCrossReference.MalformedOutputCount);
        builder.Append(" malformed, ");
        builder.Append(result.BuildManifestCrossReference.DigestMatchedOutputCount);
        builder.Append(" digest matched, ");
        builder.Append(result.BuildManifestCrossReference.DigestMismatchedOutputCount);
        builder.AppendLine(" digest mismatched)");
        builder.Append("Release archive evidence cross-reference: ");
        builder.Append(result.ArchiveEvidenceCrossReference.Status);
        builder.Append(" (");
        builder.Append(result.ArchiveEvidenceCrossReference.CoveredExpectedPathCount);
        builder.Append('/');
        builder.Append(result.ArchiveEvidenceCrossReference.ExpectedPathCount);
        builder.Append(" expected paths covered, ");
        builder.Append(result.ArchiveEvidenceCrossReference.UnexpectedPathCount);
        builder.Append(" unexpected, ");
        builder.Append(result.ArchiveEvidenceCrossReference.MalformedPathCount);
        builder.Append(" malformed, archive sha256 match: ");
        builder.Append(result.ArchiveEvidenceCrossReference.ArchiveSha256MatchesLocal.ToString().ToLowerInvariant());
        builder.Append(", archive length match: ");
        builder.Append(result.ArchiveEvidenceCrossReference.ArchiveLengthMatchesLocal.ToString().ToLowerInvariant());
        builder.Append(", archive entries: ");
        builder.Append(result.ArchiveEvidenceCrossReference.ActualArchiveEntryCount);
        builder.Append('/');
        builder.Append(result.ArchiveEvidenceCrossReference.ExpectedArchiveEntryCount);
        builder.Append(", archive entry names match: ");
        builder.Append(result.ArchiveEvidenceCrossReference.ArchiveEntryNamesMatchLocal.ToString().ToLowerInvariant());
        builder.AppendLine(")");
        builder.Append("Collection plan evidence: ");
        builder.Append(result.CollectionPlanEvidence.Status);
        builder.Append(" (");
        builder.Append(result.CollectionPlanEvidence.Steps);
        builder.Append(" steps, ");
        builder.Append(result.CollectionPlanEvidence.ManualSteps);
        builder.Append(" manual, ");
        builder.Append(result.CollectionPlanEvidence.AvailableSteps);
        builder.Append(" available, ");
        builder.Append(result.CollectionPlanEvidence.MalformedSteps);
        builder.AppendLine(" malformed)");
        builder.Append("Release dry-run cross-links: ");
        builder.Append(result.DryRunCrossLinkEvidence.Status);
        builder.Append(" (");
        builder.Append(result.DryRunCrossLinkEvidence.ValidLinks);
        builder.Append('/');
        builder.Append(result.DryRunCrossLinkEvidence.ExpectedLinks);
        builder.Append(" links valid, ");
        builder.Append(result.DryRunCrossLinkEvidence.MismatchedLinks);
        builder.AppendLine(" mismatched)");
        builder.Append("Release dry-run evidence remediation: ");
        builder.Append(result.DryRunEvidenceRemediation.Status);
        builder.Append(" (");
        builder.Append(result.DryRunEvidenceRemediation.ActionItems);
        builder.Append(" action item(s), ");
        builder.Append(result.DryRunEvidenceRemediation.CommandHints);
        builder.Append(" command hint(s))");
        builder.AppendLine();
        builder.Append("Mode: ");
        builder.AppendLine(result.DryRun ? "dry-run-preflight" : "no-publish-refusal");
        builder.Append("Publish ready: ");
        builder.AppendLine(result.PublishReady.ToString().ToLowerInvariant());
        builder.Append("Publish readiness: ");
        builder.Append(result.PublishReadiness.Status);
        builder.Append(" (");
        builder.Append(result.PublishReadiness.SatisfiedChecks);
        builder.Append('/');
        builder.Append(result.PublishReadiness.RequiredChecks);
        builder.Append(" checks satisfied, ");
        builder.Append(result.PublishReadiness.BlockingChecks);
        builder.AppendLine(" blocking)");
        builder.Append("Local publish preconditions satisfied: ");
        builder.AppendLine(result.PublishReadiness.LocalPreconditionsSatisfied.ToString().ToLowerInvariant());
        builder.Append("Ready for real publish: ");
        builder.AppendLine(result.PublishReadiness.ReadyForRealPublish.ToString().ToLowerInvariant());
        builder.Append("Lane closeout: ");
        builder.AppendLine(result.LaneCloseout.Status);
        builder.Append("Next value slice: ");
        builder.Append(result.LaneCloseout.NextGate);
        builder.Append(" - ");
        builder.AppendLine(result.LaneCloseout.NextValueSlice);
        if (result.RefusalReason is not null)
        {
            builder.Append("Refusal: ");
            builder.AppendLine(result.RefusalReason);
        }

        builder.AppendLine();
        builder.AppendLine("Publish readiness");
        builder.Append("  status: ");
        builder.AppendLine(result.PublishReadiness.Status);
        builder.Append("  local preconditions satisfied: ");
        builder.AppendLine(result.PublishReadiness.LocalPreconditionsSatisfied.ToString().ToLowerInvariant());
        builder.Append("  evidence satisfied: ");
        builder.AppendLine(result.PublishReadiness.EvidenceSatisfied.ToString().ToLowerInvariant());
        builder.Append("  governance satisfied: ");
        builder.AppendLine(result.PublishReadiness.GovernanceSatisfied.ToString().ToLowerInvariant());
        builder.Append("  approval satisfied: ");
        builder.AppendLine(result.PublishReadiness.ApprovalSatisfied.ToString().ToLowerInvariant());
        builder.Append("  publish execution enabled: ");
        builder.AppendLine(result.PublishReadiness.PublishExecutionEnabled.ToString().ToLowerInvariant());
        builder.Append("  ready for real publish: ");
        builder.AppendLine(result.PublishReadiness.ReadyForRealPublish.ToString().ToLowerInvariant());
        builder.Append("  detail: ");
        builder.AppendLine(result.PublishReadiness.Detail);
        foreach (var check in result.PublishReadiness.Checks)
        {
            builder.Append("  ");
            builder.Append(check.Satisfied ? "SATISFIED " : "BLOCKING ");
            builder.Append(check.Id);
            builder.Append(": ");
            builder.Append(check.Status);
            builder.Append(" (");
            builder.Append(check.Detail);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Lane closeout");
        builder.Append("  status: ");
        builder.AppendLine(result.LaneCloseout.Status);
        builder.Append("  lane: ");
        builder.AppendLine(result.LaneCloseout.Lane);
        builder.Append("  completed through: ");
        builder.AppendLine(result.LaneCloseout.CompletedThroughGate);
        builder.Append("  next gate: ");
        builder.AppendLine(result.LaneCloseout.NextGate);
        builder.Append("  next value slice: ");
        builder.AppendLine(result.LaneCloseout.NextValueSlice);
        builder.Append("  detail: ");
        builder.AppendLine(result.LaneCloseout.Detail);
        foreach (var deferredCapability in result.LaneCloseout.DeferredCapabilities)
        {
            builder.Append("  DEFERRED ");
            builder.AppendLine(deferredCapability);
        }

        builder.AppendLine();
        builder.AppendLine("Required evidence");
        foreach (var requirement in result.RequiredEvidence)
        {
            builder.Append("  REQUIRED ");
            builder.Append(requirement.Id);
            builder.Append(": ");
            builder.Append(requirement.Title);
            builder.Append(" (");
            builder.Append(requirement.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Local evidence artifacts");
        foreach (var artifact in result.LocalEvidenceArtifacts)
        {
            builder.Append("  ");
            builder.Append(artifact.Exists ? "PRESENT " : "MISSING ");
            builder.Append(artifact.Id);
            builder.Append(": ");
            builder.Append(artifact.Path);
            builder.Append(" (");
            builder.Append(artifact.Status);
            if (artifact.ShapeDetail is not null)
            {
                builder.Append(", ");
                builder.Append(artifact.ShapeDetail);
            }

            if (artifact.Length is not null)
            {
                builder.Append(", ");
                builder.Append(artifact.Length.Value);
                builder.Append(" bytes");
            }

            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Checksum sidecar");
        builder.Append("  path: ");
        builder.AppendLine(result.ChecksumSidecar.Path);
        builder.Append("  status: ");
        builder.AppendLine(result.ChecksumSidecar.Status);
        builder.Append("  digest revalidated entries: ");
        builder.Append(result.ChecksumSidecar.DigestRevalidatedEntries);
        builder.Append(", matched: ");
        builder.Append(result.ChecksumSidecar.DigestMatchedEntries);
        builder.Append(", mismatched: ");
        builder.Append(result.ChecksumSidecar.DigestMismatchedEntries);
        builder.Append(", missing local files: ");
        builder.AppendLine(result.ChecksumSidecar.MissingLocalFileEntries.ToString());
        builder.AppendLine("  expected path coverage");
        foreach (var expectedPath in result.ChecksumSidecar.ExpectedPaths)
        {
            builder.Append("    ");
            builder.Append(expectedPath.Path);
            builder.Append(" (");
            builder.Append(expectedPath.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine("  entries");
        foreach (var entry in result.ChecksumSidecar.Entries)
        {
            builder.Append("    line ");
            builder.Append(entry.LineNumber);
            builder.Append(": ");
            builder.Append(entry.Path ?? "<unparsed>");
            builder.Append(" (");
            builder.Append(entry.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Build manifest cross-reference");
        builder.Append("  path: ");
        builder.AppendLine(result.BuildManifestCrossReference.Path);
        builder.Append("  status: ");
        builder.AppendLine(result.BuildManifestCrossReference.Status);
        builder.Append("  digest revalidated outputs: ");
        builder.Append(result.BuildManifestCrossReference.DigestRevalidatedOutputCount);
        builder.Append(", matched: ");
        builder.Append(result.BuildManifestCrossReference.DigestMatchedOutputCount);
        builder.Append(", mismatched: ");
        builder.AppendLine(result.BuildManifestCrossReference.DigestMismatchedOutputCount.ToString());
        builder.AppendLine("  expected output coverage");
        foreach (var expectedPath in result.BuildManifestCrossReference.ExpectedPaths)
        {
            builder.Append("    ");
            builder.Append(expectedPath.Path);
            builder.Append(" (");
            builder.Append(expectedPath.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine("  outputs");
        foreach (var output in result.BuildManifestCrossReference.Outputs)
        {
            builder.Append("    output ");
            builder.Append(output.Index);
            builder.Append(": ");
            builder.Append(output.Path ?? "<unparsed>");
            builder.Append(" (");
            builder.Append(output.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Release archive evidence cross-reference");
        builder.Append("  path: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.Path);
        builder.Append("  status: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.Status);
        builder.Append("  archive path matches output: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchivePathMatchesOutput.ToString().ToLowerInvariant());
        builder.Append("  archive digest revalidation: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.DigestRevalidationInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  archive sha256 matches local: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveSha256MatchesLocal.ToString().ToLowerInvariant());
        builder.Append("  archive length matches local: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveLengthMatchesLocal.ToString().ToLowerInvariant());
        builder.Append("  archive revalidation: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveRevalidationInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  archive opened: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveOpenedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  archive entries: ");
        builder.Append(result.ArchiveEvidenceCrossReference.ActualArchiveEntryCount);
        builder.Append('/');
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ExpectedArchiveEntryCount.ToString());
        builder.Append("  archive entry count matches metadata: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveEntryCountMatchesMetadata.ToString().ToLowerInvariant());
        builder.Append("  archive entry names match local: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveEntryNamesMatchLocal.ToString().ToLowerInvariant());
        builder.Append("  archive entry ordering matches local: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveEntryOrderingMatchesLocal.ToString().ToLowerInvariant());
        builder.Append("  archive deterministic timestamps match local: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveDeterministicTimestampsMatchLocal.ToString().ToLowerInvariant());
        builder.Append("  archive stored compression matches local: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveStoredCompressionMatchesLocal.ToString().ToLowerInvariant());
        builder.Append("  archive revalidation detail: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveRevalidationDetail);
        builder.AppendLine("  expected path coverage");
        foreach (var expectedPath in result.ArchiveEvidenceCrossReference.ExpectedPaths)
        {
            builder.Append("    ");
            builder.Append(expectedPath.Role);
            builder.Append(": ");
            builder.Append(expectedPath.Path);
            builder.Append(" (");
            builder.Append(expectedPath.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine("  paths");
        foreach (var path in result.ArchiveEvidenceCrossReference.Paths)
        {
            builder.Append("    ");
            builder.Append(path.Role);
            builder.Append(": ");
            builder.Append(path.Path ?? "<unparsed>");
            builder.Append(" (");
            builder.Append(path.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine("  archive entries");
        foreach (var entry in result.ArchiveEvidenceCrossReference.ArchiveEntries)
        {
            builder.Append("    entry ");
            builder.Append(entry.Index);
            builder.Append(": ");
            builder.Append(entry.Path);
            builder.Append(" (");
            builder.Append(entry.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Semantic release evidence validation");
        builder.Append("  status: ");
        builder.AppendLine(result.SemanticEvidenceValidation.Status);
        builder.Append("  validation in current gate: ");
        builder.AppendLine(result.SemanticEvidenceValidation.ValidationInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.SemanticEvidenceValidation.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  checks: ");
        builder.Append(result.SemanticEvidenceValidation.PassedChecks);
        builder.Append(" passed, ");
        builder.Append(result.SemanticEvidenceValidation.FailedChecks);
        builder.Append(" failed, ");
        builder.Append(result.SemanticEvidenceValidation.SkippedChecks);
        builder.AppendLine(" skipped");
        builder.Append("  detail: ");
        builder.AppendLine(result.SemanticEvidenceValidation.Detail);
        builder.AppendLine("  checks");
        foreach (var check in result.SemanticEvidenceValidation.Checks)
        {
            builder.Append("    ");
            builder.Append(check.Id);
            builder.Append(": ");
            builder.Append(check.Status);
            builder.Append(" (");
            builder.Append(check.Detail);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Schema validation evidence");
        builder.Append("  path: ");
        builder.AppendLine(result.SchemaValidationEvidence.Path);
        builder.Append("  exists: ");
        builder.AppendLine(result.SchemaValidationEvidence.Exists.ToString().ToLowerInvariant());
        builder.Append("  status: ");
        builder.AppendLine(result.SchemaValidationEvidence.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.SchemaValidationEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.SchemaValidationEvidence.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  summary: ");
        builder.Append(result.SchemaValidationEvidence.Errors);
        builder.Append(" errors, ");
        builder.Append(result.SchemaValidationEvidence.Warnings);
        builder.Append(" warnings, ");
        builder.Append(result.SchemaValidationEvidence.Notes);
        builder.Append(" notes, ");
        builder.Append(result.SchemaValidationEvidence.SchemaIssueCount);
        builder.Append(" schema issues");
        builder.AppendLine();
        builder.Append("  detail: ");
        builder.AppendLine(result.SchemaValidationEvidence.Detail);

        builder.AppendLine();
        builder.AppendLine("Capability/environment evidence");
        builder.Append("  path: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.Path);
        builder.Append("  exists: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.Exists.ToString().ToLowerInvariant());
        builder.Append("  status: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  project scoped: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.ProjectScoped.ToString().ToLowerInvariant());
        builder.Append("  runtime probes enabled: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.RuntimeProbesEnabled.ToString().ToLowerInvariant());
        builder.Append("  MO2 VFS enabled: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.Mo2VfsEnabled.ToString().ToLowerInvariant());
        builder.Append("  summary: ");
        builder.Append(result.CapabilityEnvironmentEvidence.Providers);
        builder.Append(" providers, ");
        builder.Append(result.CapabilityEnvironmentEvidence.Capabilities);
        builder.Append(" capabilities, ");
        builder.Append(result.CapabilityEnvironmentEvidence.Requirements);
        builder.Append(" requirements, ");
        builder.Append(result.CapabilityEnvironmentEvidence.RequiredUnavailable);
        builder.Append(" required unavailable, ");
        builder.Append(result.CapabilityEnvironmentEvidence.CapabilityDiagnostics);
        builder.Append(" capability diagnostics");
        builder.AppendLine();
        builder.Append("  doctor: ");
        builder.Append(result.CapabilityEnvironmentEvidence.DoctorAreas);
        builder.Append(" areas, ");
        builder.Append(result.CapabilityEnvironmentEvidence.DoctorActionNeededAreas);
        builder.Append(" action-needed, ");
        builder.Append(result.CapabilityEnvironmentEvidence.DoctorUnknownAreas);
        builder.AppendLine(" unknown");
        builder.Append("  detail: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.Detail);

        builder.AppendLine();
        builder.AppendLine("Package validation evidence");
        builder.Append("  path: ");
        builder.AppendLine(result.PackageValidationEvidence.Path);
        builder.Append("  exists: ");
        builder.AppendLine(result.PackageValidationEvidence.Exists.ToString().ToLowerInvariant());
        builder.Append("  status: ");
        builder.AppendLine(result.PackageValidationEvidence.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.PackageValidationEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.PackageValidationEvidence.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  target: ");
        builder.AppendLine(result.PackageValidationEvidence.Target ?? "missing");
        builder.Append("  mode: ");
        builder.AppendLine(result.PackageValidationEvidence.Mode ?? "missing");
        builder.Append("  output root: ");
        builder.AppendLine(result.PackageValidationEvidence.OutputRoot ?? "missing");
        builder.Append("  dist scoped: ");
        builder.AppendLine(result.PackageValidationEvidence.DistScoped.ToString().ToLowerInvariant());
        builder.Append("  package archive present: ");
        builder.AppendLine(result.PackageValidationEvidence.PackageArchivePresent.ToString().ToLowerInvariant());
        builder.Append("  summary: ");
        builder.Append(result.PackageValidationEvidence.Errors);
        builder.Append(" errors, ");
        builder.Append(result.PackageValidationEvidence.Warnings);
        builder.Append(" warnings, ");
        builder.Append(result.PackageValidationEvidence.Notes);
        builder.Append(" notes, ");
        builder.Append(result.PackageValidationEvidence.PackageIssueCount);
        builder.Append(" package diagnostics");
        builder.AppendLine();
        builder.Append("  detail: ");
        builder.AppendLine(result.PackageValidationEvidence.Detail);

        builder.AppendLine();
        builder.AppendLine("Release verification evidence");
        builder.Append("  path: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.Path);
        builder.Append("  exists: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.Exists.ToString().ToLowerInvariant());
        builder.Append("  status: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  dry run: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.DryRun.ToString().ToLowerInvariant());
        builder.Append("  output root: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.OutputRoot ?? "missing");
        builder.Append("  dist scoped: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.DistScoped.ToString().ToLowerInvariant());
        builder.Append("  summary: ");
        builder.Append(result.ReleaseVerificationEvidence.Errors);
        builder.Append(" errors, ");
        builder.Append(result.ReleaseVerificationEvidence.Warnings);
        builder.Append(" warnings, ");
        builder.Append(result.ReleaseVerificationEvidence.Notes);
        builder.Append(" notes, ");
        builder.Append(result.ReleaseVerificationEvidence.ReleaseIssueCount);
        builder.Append(" release diagnostics");
        builder.AppendLine();
        builder.Append("  detail: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.Detail);

        builder.AppendLine();
        builder.AppendLine("Collection plan evidence");
        builder.Append("  path: ");
        builder.AppendLine(result.CollectionPlanEvidence.Path);
        builder.Append("  exists: ");
        builder.AppendLine(result.CollectionPlanEvidence.Exists.ToString().ToLowerInvariant());
        builder.Append("  status: ");
        builder.AppendLine(result.CollectionPlanEvidence.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.CollectionPlanEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.CollectionPlanEvidence.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  dry run: ");
        builder.AppendLine(result.CollectionPlanEvidence.DryRun.ToString().ToLowerInvariant());
        builder.Append("  output root: ");
        builder.AppendLine(result.CollectionPlanEvidence.OutputRoot ?? "missing");
        builder.Append("  dist scoped: ");
        builder.AppendLine(result.CollectionPlanEvidence.DistScoped.ToString().ToLowerInvariant());
        builder.Append("  links evidence index: ");
        builder.AppendLine(result.CollectionPlanEvidence.LinksEvidenceIndex.ToString().ToLowerInvariant());
        builder.Append("  links evidence status: ");
        builder.AppendLine(result.CollectionPlanEvidence.LinksEvidenceStatus.ToString().ToLowerInvariant());
        builder.Append("  links action checklist: ");
        builder.AppendLine(result.CollectionPlanEvidence.LinksActionChecklist.ToString().ToLowerInvariant());
        builder.Append("  links handoff summary: ");
        builder.AppendLine(result.CollectionPlanEvidence.LinksHandoffSummary.ToString().ToLowerInvariant());
        builder.Append("  summary: ");
        builder.Append(result.CollectionPlanEvidence.Steps);
        builder.Append(" steps, ");
        builder.Append(result.CollectionPlanEvidence.ManualSteps);
        builder.Append(" manual, ");
        builder.Append(result.CollectionPlanEvidence.AvailableSteps);
        builder.Append(" available, ");
        builder.Append(result.CollectionPlanEvidence.MalformedSteps);
        builder.Append(" malformed");
        builder.AppendLine();
        builder.Append("  no execution boundary: ");
        builder.AppendLine(result.CollectionPlanEvidence.NoExecutionBoundary.ToString().ToLowerInvariant());
        builder.Append("  detail: ");
        builder.AppendLine(result.CollectionPlanEvidence.Detail);

        builder.AppendLine();
        builder.AppendLine("Release dry-run cross-link evidence");
        builder.Append("  status: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  content read in current gate: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.ContentReadInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  files: ");
        builder.Append(result.DryRunCrossLinkEvidence.PresentFiles);
        builder.Append('/');
        builder.Append(result.DryRunCrossLinkEvidence.ExpectedFiles);
        builder.Append(" present, ");
        builder.Append(result.DryRunCrossLinkEvidence.MissingFiles);
        builder.Append(" missing, ");
        builder.Append(result.DryRunCrossLinkEvidence.MalformedFiles);
        builder.AppendLine(" malformed");
        builder.Append("  links: ");
        builder.Append(result.DryRunCrossLinkEvidence.ValidLinks);
        builder.Append('/');
        builder.Append(result.DryRunCrossLinkEvidence.ExpectedLinks);
        builder.Append(" valid, ");
        builder.Append(result.DryRunCrossLinkEvidence.MismatchedLinks);
        builder.AppendLine(" mismatched");
        builder.Append("  required evidence entries: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.RequiredEvidenceEntries.ToString());
        builder.Append("  status entries: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.StatusEntries.ToString());
        builder.Append("  action items: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.ActionItems.ToString());
        builder.Append("  collection steps: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.CollectionSteps.ToString());
        builder.Append("  identity consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.IdentityConsistent.ToString().ToLowerInvariant());
        builder.Append("  summary counters consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.SummaryCountersConsistent.ToString().ToLowerInvariant());
        builder.Append("  required evidence consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.RequiredEvidenceConsistent.ToString().ToLowerInvariant());
        builder.Append("  action references consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.ActionReferencesConsistent.ToString().ToLowerInvariant());
        builder.Append("  collection steps consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.CollectionStepsConsistent.ToString().ToLowerInvariant());
        builder.Append("  execution boundaries consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.ExecutionBoundariesConsistent.ToString().ToLowerInvariant());
        builder.Append("  handoff references consistent: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.HandoffReferencesConsistent.ToString().ToLowerInvariant());
        builder.Append("  detail: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.Detail);

        builder.AppendLine();
        builder.AppendLine("Release dry-run evidence remediation");
        builder.Append("  status: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.Status);
        builder.Append("  checked in current gate: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  requires operator action: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.RequiresOperatorAction.ToString().ToLowerInvariant());
        builder.Append("  source status: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.SourceStatus);
        builder.Append("  action items: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.ActionItems.ToString());
        builder.Append("  command hints: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.CommandHints.ToString());
        builder.Append("  affected paths: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.AffectedPaths.ToString());
        builder.Append("  blocking issues: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.BlockingIssues.ToString());
        builder.Append("  recommended command: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.RecommendedCommand);
        builder.Append("  detail: ");
        builder.AppendLine(result.DryRunEvidenceRemediation.Detail);
        foreach (var item in result.DryRunEvidenceRemediation.Items)
        {
            builder.Append("  [");
            builder.Append(item.Priority);
            builder.Append("] ");
            builder.Append(item.Id);
            builder.Append(" (");
            builder.Append(item.Category);
            builder.Append(", ");
            builder.Append(item.Execution);
            builder.Append("): ");
            builder.AppendLine(item.Reason);
            builder.Append("    command: ");
            builder.AppendLine(item.CommandHint);
            builder.Append("    paths: ");
            builder.AppendLine(string.Join(", ", item.TargetPaths));
        }

        builder.AppendLine();
        builder.AppendLine("Governance checks");
        foreach (var check in result.GovernanceChecks)
        {
            builder.Append("  REQUIRED ");
            builder.Append(check.Id);
            builder.Append(": ");
            builder.Append(check.Title);
            builder.Append(" (");
            builder.Append(check.Status);
            builder.Append("; ");
            builder.Append(check.EvidencePath);
            builder.Append("; ");
            builder.Append(check.Detail);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Approval");
        builder.Append("  required: ");
        builder.AppendLine(result.Approval.Required.ToString().ToLowerInvariant());
        builder.Append("  provided: ");
        builder.AppendLine(result.Approval.Provided.ToString().ToLowerInvariant());
        builder.Append("  yes provided: ");
        builder.AppendLine(result.Approval.YesProvided.ToString().ToLowerInvariant());
        builder.Append("  confirmation value: ");
        builder.AppendLine(result.Approval.ConfirmationValue ?? "missing");
        builder.Append("  confirmation validated: ");
        builder.AppendLine(result.Approval.ConfirmationValidated.ToString().ToLowerInvariant());
        builder.Append("  confirmation matches: ");
        builder.AppendLine(result.Approval.ConfirmationMatches?.ToString().ToLowerInvariant() ?? "not-evaluated");
        builder.Append("  project manifest read: ");
        builder.AppendLine(result.Approval.ProjectManifestRead.ToString().ToLowerInvariant());
        builder.Append("  project manifest path: ");
        builder.AppendLine(result.Approval.ProjectManifestPath ?? "missing");
        builder.Append("  project id: ");
        builder.AppendLine(result.Approval.ProjectId ?? "missing");
        builder.Append("  status: ");
        builder.AppendLine(result.Approval.Status);
        builder.Append("  detail: ");
        builder.AppendLine(result.Approval.Detail);

        builder.AppendLine();
        builder.AppendLine("Execution");
        builder.AppendLine("  release publish preflight planning: true");
        builder.AppendLine("  release publish execution: false");
        builder.AppendLine("  release publishing: false");
        builder.AppendLine("  publish readiness evaluation: true");
        builder.Append("  local publish preconditions satisfied: ");
        builder.AppendLine(result.PublishReadiness.LocalPreconditionsSatisfied.ToString().ToLowerInvariant());
        builder.Append("  publish ready for real publish: ");
        builder.AppendLine(result.PublishReadiness.ReadyForRealPublish.ToString().ToLowerInvariant());
        builder.AppendLine("  release publish lane closeout: true");
        builder.AppendLine("  next value slice routed: true");
        builder.AppendLine("  evidence artifact path checks: true");
        builder.Append("  evidence artifact reads: ");
        builder.AppendLine(result.LocalEvidenceArtifacts.Any(artifact => artifact.ContentReadInCurrentGate).ToString().ToLowerInvariant());
        builder.AppendLine("  artifact existence checks: true");
        builder.AppendLine("  content shape classification: true");
        builder.AppendLine("  checksum entry classification: true");
        builder.AppendLine("  checksum digest revalidation: true");
        builder.AppendLine("  build manifest output cross-reference: true");
        builder.AppendLine("  release archive evidence metadata cross-reference: true");
        builder.AppendLine("  checksum revalidation: true");
        builder.AppendLine("  build manifest digest revalidation: true");
        builder.Append("  release archive evidence digest revalidation: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.DigestRevalidationInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  semantic evidence validation: ");
        builder.AppendLine(result.SemanticEvidenceValidation.ValidationInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  archive revalidation: ");
        builder.AppendLine(result.ArchiveEvidenceCrossReference.ArchiveRevalidationInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  schema validation evidence evaluation: ");
        builder.AppendLine(result.SchemaValidationEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  capability/environment evidence evaluation: ");
        builder.AppendLine(result.CapabilityEnvironmentEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  package validation evidence evaluation: ");
        builder.AppendLine(result.PackageValidationEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  release verification evidence evaluation: ");
        builder.AppendLine(result.ReleaseVerificationEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  collection plan evidence evaluation: ");
        builder.AppendLine(result.CollectionPlanEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.Append("  release dry-run cross-link evidence evaluation: ");
        builder.AppendLine(result.DryRunCrossLinkEvidence.CheckedInCurrentGate.ToString().ToLowerInvariant());
        builder.AppendLine("  human approval evaluation: true");
        builder.Append("  human approval confirmation validated: ");
        builder.AppendLine(result.Approval.ConfirmationValidated.ToString().ToLowerInvariant());
        builder.AppendLine("  governance check execution: true");
        builder.AppendLine("  filesystem mutation: false");
        builder.AppendLine("  output writes: false");
        builder.AppendLine("  remote repository calls: false");
        builder.AppendLine("  release uploads: false");
        builder.AppendLine("  attestation/signing: false");
        builder.AppendLine("  external tools: false");
        builder.AppendLine("  runtime probes: false");
        builder.AppendLine("  AI required: false");

        builder.AppendLine();
        builder.AppendLine("Boundaries");
        foreach (var boundary in result.Boundaries)
        {
            builder.Append("  - ");
            builder.AppendLine(boundary);
        }

        return builder.ToString();
    }
}
