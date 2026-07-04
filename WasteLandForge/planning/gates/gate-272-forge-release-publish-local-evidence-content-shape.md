# Gate 272 - forge release publish Local Evidence Content Shape

Status: Complete
Date: 2026-07-04

## Goal

Extend `forge release publish` preflight so it classifies local
release-prepare evidence content shape while remaining no-publish by default.

## Research grounding

- Documented: ADR-011 requires local build manifests, checksums, release
  verification, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 271 discovers expected local release-prepare evidence paths
  and reports missing or present-not-yet-validated artifacts.
- Inferred: The next safe release-publish slice is content-shape
  classification for JSON and checksum text evidence before checksum digest
  revalidation, archive revalidation, or remote publishing exists.

## Implemented behavior

Gate 272 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.wellFormedArtifacts`,
- `releasePrepareEvidence.malformedArtifacts`,
- `releasePrepareEvidence.unclassifiedArtifacts`,
- `contentShapeClassificationInCurrentGate: true`,
- `localEvidenceArtifacts[].contentKind`,
- `localEvidenceArtifacts[].shapeCheckedInCurrentGate`,
- `localEvidenceArtifacts[].contentReadInCurrentGate`,
- `localEvidenceArtifacts[].shapeDetail`,
- `execution.contentShapeClassification: true`,
- `execution.checksumRevalidation: false`,
- `execution.archiveRevalidation: false`.

## Classification rules

- JSON evidence files must parse as JSON objects to report
  `well-formed-not-validated`.
- Malformed JSON or JSON with a non-object root reports `malformed`.
- `checksums.sha256` must contain one or more non-empty lines shaped as
  lowercase SHA-256, two spaces, then a non-empty path to report
  `well-formed-not-validated`.
- The release archive ZIP remains `present-not-classified` and is not opened.
- Missing files remain `missing`.

## Explicit non-goals

Gate 272 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
- checksum digest revalidation,
- archive revalidation,
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
| JSON shape classified | Complete | Prepared release JSON evidence reports `well-formed-not-validated`. |
| Checksum text shape classified | Complete | Prepared `checksums.sha256` reports `well-formed-not-validated`. |
| Archive deferred | Complete | Prepared `archives/release.zip` reports `present-not-classified`. |
| Malformed JSON reported | Complete | Corrupted `release-plan.json` reports `malformed`. |
| No publish side effects | Complete | Publish, remote, upload, signing, checksum revalidation, archive revalidation, external tool, runtime probe, and AI flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 272 shape-only classification. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
```

## Next gate

Gate 273 should add checksum sidecar entry classification for
`forge release publish`. It should parse checksum entries into command output
and report expected-path coverage without recomputing digests. It must still
stop before checksum digest revalidation, archive revalidation, remote
repository calls, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
