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
    IReadOnlyList<ReleasePublishArchiveEvidenceExpectedPath> ExpectedPaths,
    IReadOnlyList<ReleasePublishArchiveEvidencePath> Paths);

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
    private static readonly string[] BoundaryLines =
    [
        "Gate 278 revalidates release-archive-evidence archive digest metadata for release publish preflight.",
        "JSON evidence is parsed for well-formed shape only; checksum sidecar entries are parsed for expected-path coverage only.",
        "Build-manifest outputs and release-archive-evidence metadata are cross-referenced to local evidence paths, checksum sidecar paths, and existing build-manifest output paths only.",
        "Checksum sidecar digests are recomputed for expected local evidence files only.",
        "Build-manifest output digests are recomputed for expected local evidence files only.",
        "Release-archive-evidence archive sha256 and length metadata are recomputed for the expected local archive file only.",
        "Release archives are not opened or revalidated.",
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
        var presentArtifacts = localEvidenceArtifacts.Count(artifact => artifact.Exists);
        var missingArtifacts = localEvidenceArtifacts.Count - presentArtifacts;
        var wellFormedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "well-formed-not-validated"));
        var malformedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "malformed"));
        var unclassifiedArtifacts = localEvidenceArtifacts.Count(artifact => StringComparer.Ordinal.Equals(artifact.Status, "present-not-classified"));
        var releasePrepareEvidenceStatus = ReleasePrepareEvidenceStatus(presentArtifacts, missingArtifacts, malformedArtifacts, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference);
        var status = options.DryRun ? "planned" : "refused";
        var refusalReason = options.DryRun
            ? null
            : "Release publish requires validated local governance preflight evidence and explicit human approval; Gate 278 does not publish releases.";

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
            refusalReason,
            CreateRequiredEvidence(localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference, archiveEvidenceCrossReference),
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
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference) =>
    [
        Evidence("schema-validation", "Schema validation passed", "ADR-011 layered validation"),
        Evidence("semantic-validation", "Semantic validation passed", "ADR-011 layered validation"),
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
                DigestRevalidationInCurrentGate: false,
                ArchiveRevalidationInCurrentGate: false,
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

            var archive = root?["archive"] as JsonObject;
            var archivePathMatchesOutput = TryGetStringProperty(archive, "path", out var archivePath) &&
                TryGetStringProperty(output, "releaseArchive", out var releaseArchivePath) &&
                StringComparer.Ordinal.Equals(archivePath, releaseArchivePath);
            var archiveSha256MetadataPresent = TryGetStringProperty(archive, "sha256", out _);
            var archiveLengthMetadataPresent = TryGetInt64Property(archive, "length", out _);
            var expectedPathCoverage = CreateArchiveEvidenceExpectedPathCoverage(coveredExpectedRoles, localEvidenceArtifacts, checksumSidecar, buildManifestCrossReference);
            var missingExpectedPaths = ExpectedArchiveEvidencePaths.Length - coveredExpectedRoles.Count;
            var missingLocalPaths = expectedPathCoverage.Count(path => !path.LocalArtifactPresent);
            var missingChecksumPaths = expectedPathCoverage.Count(path => !path.ChecksumEntryPresent);
            var missingBuildManifestOutputPaths = expectedPathCoverage.Count(path => !path.BuildManifestOutputPresent);
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
                archiveLengthMetadataPresent);

            return new ReleasePublishArchiveEvidenceCrossReference(
                displayPath,
                Exists: true,
                Status: status,
                MetadataCrossReferenceInCurrentGate: true,
                ContentReadInCurrentGate: true,
                DigestRevalidationInCurrentGate: false,
                ArchiveRevalidationInCurrentGate: false,
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
            DigestRevalidationInCurrentGate: false,
            ArchiveRevalidationInCurrentGate: false,
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

    private static bool TryGetInt64Property(JsonObject? source, string propertyName, out long value)
    {
        value = 0;
        return source?[propertyName] is JsonValue jsonValue &&
            jsonValue.TryGetValue<long>(out value);
    }

    private static string ReleasePrepareEvidenceStatus(
        int presentArtifacts,
        int missingArtifacts,
        int malformedArtifacts,
        ReleasePublishChecksumSidecar checksumSidecar,
        ReleasePublishBuildManifestCrossReference buildManifestCrossReference,
        ReleasePublishArchiveEvidenceCrossReference archiveEvidenceCrossReference)
    {
        if (presentArtifacts == 0)
        {
            return "missing";
        }

        if (malformedArtifacts > 0 ||
            StringComparer.Ordinal.Equals(checksumSidecar.Status, "malformed-not-validated") ||
            StringComparer.Ordinal.Equals(buildManifestCrossReference.Status, "malformed-not-validated") ||
            StringComparer.Ordinal.Equals(archiveEvidenceCrossReference.Status, "malformed-not-validated"))
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

        return missingArtifacts == 0
            ? "complete-build-manifest-digest-revalidated"
            : "partial-build-manifest-digest-revalidated";
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
        bool archiveLengthMetadataPresent)
    {
        if (malformedPaths > 0 ||
            parsedPaths == 0 ||
            !archiveSha256MetadataPresent ||
            !archiveLengthMetadataPresent)
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
            return "complete-cross-referenced-not-revalidated";
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
                ["artifactPathChecksInCurrentGate"] = true,
                ["artifactReadsInCurrentGate"] = result.LocalEvidenceArtifacts.Any(artifact => artifact.ContentReadInCurrentGate),
            ["contentShapeClassificationInCurrentGate"] = true,
            ["checksumEntryClassificationInCurrentGate"] = result.ChecksumSidecar.EntryClassificationInCurrentGate,
            ["checksumDigestRevalidationInCurrentGate"] = result.ChecksumSidecar.DigestRevalidationInCurrentGate,
            ["buildManifestOutputCrossReferenceInCurrentGate"] = result.BuildManifestCrossReference.OutputCrossReferenceInCurrentGate,
            ["buildManifestDigestRevalidationInCurrentGate"] = result.BuildManifestCrossReference.DigestRevalidationInCurrentGate,
            ["archiveEvidenceMetadataCrossReferenceInCurrentGate"] = result.ArchiveEvidenceCrossReference.MetadataCrossReferenceInCurrentGate
            },
            ["checksumSidecar"] = ToChecksumSidecar(result.ChecksumSidecar),
            ["buildManifestCrossReference"] = ToBuildManifestCrossReference(result.BuildManifestCrossReference),
            ["archiveEvidenceCrossReference"] = ToArchiveEvidenceCrossReference(result.ArchiveEvidenceCrossReference),
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
                ["summary"] = "Release publish reports governance preflight requirements and refuses publish until local evidence checks and explicit human approval are implemented."
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
            ["expectedPaths"] = ToArchiveEvidenceExpectedPathArray(crossReference.ExpectedPaths),
            ["paths"] = ToArchiveEvidencePathArray(crossReference.Paths)
        };

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
            ["archiveEvidenceDigestRevalidation"] = false,
            ["semanticEvidenceValidation"] = false,
            ["archiveRevalidation"] = false,
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
        builder.AppendLine(" malformed)");
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
        builder.AppendLine("  release archive evidence digest revalidation: false");
        builder.AppendLine("  semantic evidence validation: false");
        builder.AppendLine("  archive revalidation: false");
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
