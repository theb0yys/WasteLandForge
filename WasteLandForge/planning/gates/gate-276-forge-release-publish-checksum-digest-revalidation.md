# Gate 276 - forge release publish Checksum Digest Revalidation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it revalidates
`dist/release-prepare/checksums.sha256` digests for expected local
release-prepare evidence files while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires checksums, local build manifests, release
  evidence, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 273 classifies checksum sidecar entries and expected-path
  coverage without digest revalidation.
- Documented: Gate 274 and Gate 275 cross-reference build-manifest outputs and
  release-archive-evidence metadata while stopping before digest revalidation.
- Inferred: The next safe release-publish slice is checksum sidecar digest
  revalidation for expected local evidence files before any build-manifest
  digest validation, archive reopening, semantic evidence acceptance, or real
  publish behavior.

## Implemented behavior

Gate 276 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.checksumDigestRevalidatedEntries`,
- `releasePrepareEvidence.checksumDigestMatchedEntries`,
- `releasePrepareEvidence.checksumDigestMismatchedEntries`,
- `releasePrepareEvidence.checksumMissingLocalFileEntries`,
- `releasePrepareEvidence.checksumDigestRevalidationInCurrentGate`,
- `checksumSidecar.digestRevalidationInCurrentGate`,
- `checksumSidecar.digestRevalidatedEntries`,
- `checksumSidecar.digestMatchedEntries`,
- `checksumSidecar.digestMismatchedEntries`,
- `checksumSidecar.missingLocalFileEntries`,
- `checksumSidecar.expectedPaths[].entryPresent`,
- `checksumSidecar.expectedPaths[].localFilePresent`,
- `checksumSidecar.expectedPaths[].digestRevalidatedInCurrentGate`,
- `checksumSidecar.expectedPaths[].expectedSha256`,
- `checksumSidecar.expectedPaths[].actualSha256`,
- `checksumSidecar.entries[].actualSha256`,
- `checksumSidecar.entries[].localFilePresent`,
- `checksumSidecar.entries[].digestRevalidatedInCurrentGate`,
- `execution.checksumDigestRevalidation: true`,
- `execution.checksumRevalidation: true`.

## Classification rules

Expected checksum paths remain:

```text
archives/release.zip
build-manifest.json
release-archive-evidence.json
release-archive-plan.json
release-plan.json
release-summary.json
staging/release-payload.json
```

Expected, non-duplicate checksum entries are revalidated only when the local
file exists under `dist/release-prepare/`.

Entry statuses are:

- `matched-revalidated`,
- `mismatched-revalidated`,
- `missing-local-file-not-revalidated`,
- `missing-entry-not-revalidated`,
- `unexpected-not-revalidated`,
- `duplicate-not-revalidated`,
- `malformed`.

Checksum sidecar statuses are:

- `complete-digest-revalidated`,
- `mismatch-digest-revalidated`,
- `mismatch-entry-classified-not-revalidated`,
- `malformed-not-validated`,
- `missing`.

## Explicit non-goals

Gate 276 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
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
| Expected checksum entries are digest-revalidated | Complete | Prepared release evidence reports seven digest-revalidated and matched entries. |
| Edited checksum digest values are reported | Complete | Rewritten `build-manifest.json` checksum digest reports `mismatched-revalidated`. |
| Missing checksum coverage remains separate | Complete | Removed checksum entry reports `missing-entry-not-revalidated` and coverage mismatch. |
| Cross-reference gates remain isolated | Complete | Build-manifest and archive-evidence cross-reference mismatch tests refresh their own checksum sidecar entries. |
| Non-goals remain disabled | Complete | Build-manifest digest, release-archive-evidence digest, semantic evidence, archive, publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 276 checksum sidecar digest revalidation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 277 should move to build-manifest digest revalidation for local
release-prepare evidence in `forge release publish`. It must still stop before
release-archive-evidence digest revalidation, archive reopening/revalidation,
semantic evidence validation, remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
