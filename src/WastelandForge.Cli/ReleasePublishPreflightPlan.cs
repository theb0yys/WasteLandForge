using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record ReleasePublishPreflightOptions(
    string ProjectRoot,
    bool DryRun);

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

internal sealed record ReleasePublishGovernanceCheck(
    string Id,
    string Title,
    string Status,
    bool Required,
    bool CheckedInCurrentGate);

internal sealed record ReleasePublishApprovalRequirement(
    bool Required,
    bool Provided,
    string Status,
    string Description);

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

    private static readonly string[] BoundaryLines =
    [
        "Gate 280 validates local release-prepare evidence semantics for release publish preflight.",
        "JSON evidence is parsed for well-formed shape only; checksum sidecar entries are parsed for expected-path coverage only.",
        "Build-manifest outputs and release-archive-evidence metadata are cross-referenced to local evidence paths, checksum sidecar paths, and existing build-manifest output paths only.",
        "Checksum sidecar digests are recomputed for expected local evidence files only.",
        "Build-manifest output digests are recomputed for expected local evidence files only.",
        "Release-archive-evidence archive sha256 and length metadata are recomputed for the expected local archive file only.",
        "The expected local release archive is reopened only to compare entry names, entry order, deterministic timestamps, and stored compression metadata.",
        "Release archive entries are inspected as metadata only; payload contents are not semantically validated.",
        "Semantic release-evidence validation checks local evidence kind/status contracts, output path maps, release-summary counters, archive-plan inputs, archive-evidence checks, build-manifest output sets, and no-publish execution boundaries.",
        "No release is published.",
        "Remote repositories are not called.",
        "Release assets are not uploaded.",
        "Attestations and signing are not performed.",
        "External tools, plugin mutation, MO2 automation, GECK automation, runtime probes, and AI calls are not used."
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
        var presentArtifacts = localEvidenceArtifacts.Count(artifact => artifact.Exists);
        var missingArtifacts = localEvidenceArtifacts.Count - presentArtifacts;
        var wellFormedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "well-formed-not-validated"));
        var malformedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "malformed"));
        var unclassifiedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "present-not-classified"));
        var releasePrepareEvidenceStatus = ReleasePrepareEvidenceStatus(presentArtifacts, missingArtifacts, malformedArtifacts, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference, semanticEvidenceValidation);
        var status = options.DryRun ? "planned" : "refused";
        var refusalReason = options.DryRun
            ? null
            : "Release publish requires validated local governance preflight evidence and explicit human approval; Gate 280 does not publish releases.";

        return new ReleasePublishPreflightResult(
            status,
            projectRoot,
            options.DryRun,
            PlanningOnly: true,
            PublishReady: false,
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
            refusalReason,
            CreateRequiredEvidence(localEvidenceArtifacts, semanticEvidenceValidation, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference),
            localEvidenceArtifacts,
            CreateGovernanceChecks(),
            new ReleasePublishApprovalRequirement(
                Required: true,
                Provided: false,
                Status: "missing",
                Description: "A human operator must explicitly approve publish after local evidence and governance checks pass."),
            BoundaryLines);
    }

    private static IReadOnlyList<ReleasePublishEvidenceRequirement> CreateRequiredEvidence(
        IReadOnlyList<ReleasePublishEvidenceArtifact> localEvidenceArtifacts,
        ReleasePublishSemanticEvidenceValidation semanticEvidenceValidation,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference) =>
    [
        Evidence("schema-validation", "Schema validation passed", "ADR-011 layered validation"),
        Evidence("semantic-validation", "Semantic validation passed", "ADR-011 layered validation", semanticEvidenceValidation.Status, checkedInCurrentGate: true),
        Evidence("capability-environment-validation", "Capability and environment validation passed", "ADR-008 and ADR-011"),
        Evidence("package-validation", "Package validation passed", "ADR-011 release validation"),
        Evidence("release-verification", "Release verification passed", "forge release verify"),
        Evidence("release-prepare-build-manifest", "Local release prepare build-manifest.json exists and is accepted", "ADR-011 mandatory local build manifest", BuildManifestRequirementStatus(localEvidenceArtifacts, buildManifestCrossReference), checkedInCurrentGate: true),
        Evidence("release-prepare-checksums", "Local release prepare checksums.sha256 exists and is accepted", "ADR-011 checksum evidence", ChecksumRequirementStatus(localEvidenceArtifacts, checksumSidecar), checkedInCurrentGate: true),
        Evidence("release-prepare-archive-evidence", "Local release archive evidence exists and is accepted", "Gate 268 release archive evidence", ArchiveEvidenceRequirementStatus(localEvidenceArtifacts, archiveEvidenceCrossReference), checkedInCurrentGate: true),
        Evidence("governance-checks", "Governance checks passed", "ADR-011 release governance")
    ];

    private static IReadOnlyList<ReleasePublishGovernanceCheck> CreateGovernanceChecks() =>
    [
        Check("immutable-schema-policy", "Published schemas remain immutable"),
        Check("semver-version-stream", "Release version follows SemVer-governed streams"),
        Check("least-privilege-release-permissions", "Release workflow uses least-privilege permissions"),
        Check("codeowner-sensitive-path-review", "Sensitive release paths have required review"),
        Check("redistributable-fixture-policy", "Public fixtures remain synthetic and redistributable"),
        Check("ai-optional-release-path", "Release correctness path does not require AI")
    ];

    private static ReleasePublishEvidenceRequirement Evidence(string id, string title, string source, string status = "required-not-evaluated", bool checkedInCurrentGate = false) =>
        new(
            id,
            title,
            source,
            Status: status,
            Required: true,
            CheckedInCurrentGate: checkedInCurrentGate);

    private static ReleasePublishGovernanceCheck Check(string id, string title) =>
        new(
            id,
            title,
            Status: "required-not-evaluated",
            Required: true,
            CheckedInCurrentGate: false);

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
            ["requiredEvidence"] = ToEvidenceArray(result.RequiredEvidence),
            ["localEvidenceArtifacts"] = ToArtifactArray(result.LocalEvidenceArtifacts),
            ["governanceChecks"] = ToGovernanceCheckArray(result.GovernanceChecks),
            ["approval"] = new JsonObject
            {
                ["required"] = result.Approval.Required,
                ["provided"] = result.Approval.Provided,
                ["status"] = result.Approval.Status,
                ["description"] = result.Approval.Description
            },
            ["reportContract"] = new JsonObject
            {
                ["status"] = result.Status,
                ["canonicalFormat"] = "json",
                ["mutatesFilesystemInCurrentGate"] = false,
                ["summary"] = "Release publish reports governance preflight requirements and refuses publish until remaining governance checks and explicit human approval are implemented."
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
                ["checkedInCurrentGate"] = check.CheckedInCurrentGate
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
            ["governanceCheckExecution"] = false,
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
        builder.Append("Mode: ");
        builder.AppendLine(result.DryRun ? "dry-run-preflight" : "no-publish-refusal");
        builder.Append("Publish ready: ");
        builder.AppendLine(result.PublishReady.ToString().ToLowerInvariant());
        if (result.RefusalReason is not null)
        {
            builder.Append("Refusal: ");
            builder.AppendLine(result.RefusalReason);
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
        builder.AppendLine("Governance checks");
        foreach (var check in result.GovernanceChecks)
        {
            builder.Append("  REQUIRED ");
            builder.Append(check.Id);
            builder.Append(": ");
            builder.Append(check.Title);
            builder.Append(" (");
            builder.Append(check.Status);
            builder.AppendLine(")");
        }

        builder.AppendLine();
        builder.AppendLine("Approval");
        builder.Append("  required: ");
        builder.AppendLine(result.Approval.Required.ToString().ToLowerInvariant());
        builder.Append("  provided: ");
        builder.AppendLine(result.Approval.Provided.ToString().ToLowerInvariant());
        builder.Append("  status: ");
        builder.AppendLine(result.Approval.Status);

        builder.AppendLine();
        builder.AppendLine("Execution");
        builder.AppendLine("  release publish preflight planning: true");
        builder.AppendLine("  release publish execution: false");
        builder.AppendLine("  release publishing: false");
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
        builder.AppendLine("  governance check execution: false");
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
