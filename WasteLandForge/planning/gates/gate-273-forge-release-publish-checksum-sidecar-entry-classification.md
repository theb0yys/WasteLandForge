# Gate 273 - forge release publish Checksum Sidecar Entry Classification

Status: Complete
Date: 2026-07-04

## Goal

Extend `forge release publish` preflight so it classifies
`dist/release-prepare/checksums.sha256` entries and reports expected-path
coverage while remaining no-publish by default.

## Research grounding

- Documented: ADR-011 requires local build manifests, checksums, release
  verification, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 272 classifies local release-prepare evidence content
  shape but stops before checksum digest revalidation.
- Inferred: The next safe release-publish slice is checksum sidecar entry
  classification and expected-path coverage before digest revalidation,
  archive revalidation, or publish behavior exists.

## Implemented behavior

Gate 273 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.checksumExpectedEntries`,
- `releasePrepareEvidence.checksumCoveredExpectedEntries`,
- `releasePrepareEvidence.checksumMissingExpectedEntries`,
- `releasePrepareEvidence.checksumUnexpectedEntries`,
- `releasePrepareEvidence.checksumDuplicateEntries`,
- `releasePrepareEvidence.checksumMalformedEntries`,
- `releasePrepareEvidence.checksumEntryClassificationInCurrentGate`,
- top-level `checksumSidecar`,
- `checksumSidecar.expectedPaths[]`,
- `checksumSidecar.entries[]`,
- `execution.checksumEntryClassification: true`,
- `execution.checksumDigestRevalidation: false`.

## Classification rules

The expected release-prepare checksum paths are:

```text
archives/release.zip
build-manifest.json
release-archive-evidence.json
release-archive-plan.json
release-plan.json
release-summary.json
staging/release-payload.json
```

Checksum entries are parsed only for:

- lowercase SHA-256 text shape,
- two-space separator,
- sidecar-relative path,
- exact expected-path coverage,
- duplicate path entries,
- unexpected path entries.

The command does not open referenced paths or recompute SHA-256 digests.

## Explicit non-goals

Gate 273 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
- checksum digest revalidation,
- archive revalidation,
- build-manifest cross-reference validation,
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
| Checksum entries parsed | Complete | Prepared release sidecar reports seven parsed entries. |
| Expected-path coverage reported | Complete | Prepared release sidecar reports seven covered expected paths. |
| Digest revalidation remains disabled | Complete | Deliberately changed digest text remains `expected-not-revalidated`. |
| Coverage mismatch reported | Complete | Missing expected and unexpected sidecar paths report mismatch status. |
| No publish side effects | Complete | Publish, remote, upload, signing, checksum digest revalidation, archive revalidation, external tool, runtime probe, and AI flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 273 checksum sidecar entry coverage. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
```

## Next gate

Gate 274 should add build-manifest output cross-reference for
`forge release publish`. It should compare build-manifest output paths to
local evidence and checksum sidecar paths without recomputing digests or
accepting evidence. It must still stop before checksum digest revalidation,
archive revalidation, semantic evidence validation, remote repository calls,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
