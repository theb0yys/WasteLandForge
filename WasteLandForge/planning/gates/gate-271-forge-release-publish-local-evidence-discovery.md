# Gate 271 - forge release publish Local Evidence Discovery

Status: Complete
Date: 2026-07-04

## Goal

Extend `forge release publish` preflight so it discovers expected local
release-prepare evidence paths and reports whether each artifact is present,
missing, and not yet validated, while remaining no-publish by default.

## Research grounding

- Documented: ADR-010 keeps `forge release publish` inside the canonical
  release command surface.
- Documented: ADR-011 and the release policy require local build manifests,
  checksums, release verification, governance checks, and explicit human
  approval before publishing.
- Documented: Gate 270 starts release-publish as a no-publish governance
  preflight and defers release artifact reads and evidence verification.
- Inferred: The next safe slice is path discovery for existing
  release-prepare evidence before content parsing, checksum revalidation, or
  remote publishing exists.

## Implemented behavior

Gate 271 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.status`,
- expected, present, and missing artifact counts,
- `artifactPathChecksInCurrentGate: true`,
- `artifactReadsInCurrentGate: false`,
- `localEvidenceArtifacts[]` with `id`, `path`, `kind`, `exists`, `status`,
  and `length` when present,
- required evidence statuses for build manifest, checksum sidecar, and archive
  evidence as `missing` or `present-not-validated`.

## Expected local artifacts

Gate 271 checks path existence for:

- `dist/release-prepare/staging/release-payload.json`,
- `dist/release-prepare/release-archive-plan.json`,
- `dist/release-prepare/archives/release.zip`,
- `dist/release-prepare/release-archive-evidence.json`,
- `dist/release-prepare/release-plan.json`,
- `dist/release-prepare/release-summary.json`,
- `dist/release-prepare/build-manifest.json`,
- `dist/release-prepare/checksums.sha256`.

## Explicit non-goals

Gate 271 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- artifact content parsing,
- checksum revalidation,
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
| Missing artifacts reported | Complete | Empty project roots report eight missing expected release-prepare artifacts. |
| Present artifacts reported | Complete | After `forge release prepare`, `forge release publish --dry-run --format json` reports eight present local artifacts. |
| No content validation | Complete | Present files are reported as `present-not-validated`; artifact reads remain false. |
| No publish side effects | Complete | Publish, remote, upload, signing, filesystem mutation, external tool, runtime probe, and AI flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 271 path-existence-only discovery. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
```

## Next gate

Gate 272 should add `forge release publish` local evidence content-shape
classification. It should parse expected JSON/text evidence enough to report
well-formed, malformed, and not-yet-validated statuses while remaining
no-publish by default. It must still stop before checksum revalidation,
archive revalidation, remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
