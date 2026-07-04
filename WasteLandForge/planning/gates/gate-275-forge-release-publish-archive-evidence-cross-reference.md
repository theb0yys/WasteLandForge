# Gate 275 - forge release publish Archive Evidence Cross-Reference

Status: Complete
Date: 2026-07-04

## Goal

Extend `forge release publish` preflight so it cross-references
`dist/release-prepare/release-archive-evidence.json` output metadata against
local release-prepare evidence paths, `checksums.sha256` entries, and
`build-manifest.json` outputs while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires release evidence, checksums, local build
  manifests, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 268 emits `release-archive-evidence.json` from
  `forge release prepare`.
- Documented: Gate 274 cross-references build-manifest outputs but stops
  before digest revalidation, archive revalidation, semantic evidence
  acceptance, or publish behavior.
- Inferred: The next safe release-publish slice is metadata cross-reference
  for release archive evidence before any archive reopening or digest
  recomputation.

## Implemented behavior

Gate 275 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.archiveEvidenceExpectedPaths`,
- `releasePrepareEvidence.archiveEvidenceParsedPaths`,
- `releasePrepareEvidence.archiveEvidenceCoveredExpectedPaths`,
- `releasePrepareEvidence.archiveEvidenceMissingExpectedPaths`,
- `releasePrepareEvidence.archiveEvidenceMissingLocalArtifactPaths`,
- `releasePrepareEvidence.archiveEvidenceMissingChecksumEntryPaths`,
- `releasePrepareEvidence.archiveEvidenceMissingBuildManifestOutputPaths`,
- `releasePrepareEvidence.archiveEvidenceUnexpectedPaths`,
- `releasePrepareEvidence.archiveEvidenceMalformedPaths`,
- `releasePrepareEvidence.archiveEvidenceArchivePathMatchesOutput`,
- `releasePrepareEvidence.archiveEvidenceMetadataCrossReferenceInCurrentGate`,
- top-level `archiveEvidenceCrossReference`,
- `archiveEvidenceCrossReference.expectedPaths[]`,
- `archiveEvidenceCrossReference.paths[]`,
- `execution.archiveEvidenceMetadataCrossReference: true`,
- `execution.archiveEvidenceDigestRevalidation: false`.

## Classification rules

The expected release-archive-evidence output metadata roles are:

```text
releaseArchive          dist/release-prepare/archives/release.zip
releaseArchiveEvidence  dist/release-prepare/release-archive-evidence.json
releaseArchivePlan      dist/release-prepare/release-archive-plan.json
releasePlan             dist/release-prepare/release-plan.json
releaseSummary          dist/release-prepare/release-summary.json
stagingPayload          dist/release-prepare/staging/release-payload.json
```

Archive evidence metadata is parsed only for:

- `output.<role>` path presence,
- exact role/path coverage,
- local evidence path presence,
- matching checksum sidecar path presence after stripping
  `dist/release-prepare/`,
- matching build-manifest output path presence,
- `archive.path` matching `output.releaseArchive`,
- presence of archive SHA-256 and length metadata without digest
  revalidation.

The command does not open the archive, recompute SHA-256 digests, compare
recorded digest values, or accept release evidence.

## Explicit non-goals

Gate 275 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
- checksum digest revalidation,
- build-manifest digest revalidation,
- release-archive-evidence digest revalidation,
- archive revalidation,
- archive reopening,
- evidence acceptance,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Archive evidence metadata parsed | Complete | Prepared release evidence reports six parsed metadata paths. |
| Expected metadata path coverage reported | Complete | Prepared release evidence reports six covered expected metadata paths. |
| Local evidence, checksum, and build-manifest paths cross-referenced | Complete | Prepared release evidence paths report all three cross-reference booleans. |
| Archive/digest revalidation remains disabled | Complete | Execution reports release-archive-evidence digest revalidation and archive revalidation as false. |
| Archive evidence mismatch reported | Complete | Rewritten `output.releaseSummary` reports mismatch status without publishing. |
| No publish side effects | Complete | Publish, remote, upload, signing, digest revalidation, archive revalidation, semantic evidence validation, external tool, runtime probe, and AI flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 275 archive-evidence metadata cross-reference. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 276 should add checksum sidecar digest revalidation for local
release-prepare evidence in `forge release publish`. It must still stop before
build-manifest digest revalidation, archive reopening/revalidation, semantic
evidence validation, remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
