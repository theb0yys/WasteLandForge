# Gate 277 - forge release publish Build Manifest Digest Revalidation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it revalidates
`dist/release-prepare/build-manifest.json` output digests for expected local
release-prepare evidence files while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires checksums, local build manifests, release
  evidence, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 274 cross-references build-manifest output paths without
  digest revalidation.
- Documented: Gate 276 revalidates checksum sidecar digests while stopping
  before build-manifest digest validation, archive reopening, semantic
  evidence acceptance, or real publish behavior.
- Inferred: The next safe release-publish slice is build-manifest output
  digest revalidation for expected local evidence files before any
  release-archive-evidence digest validation, archive reopening, semantic
  evidence acceptance, or real publish behavior.

## Implemented behavior

Gate 277 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.buildManifestDigestRevalidatedOutputs`,
- `releasePrepareEvidence.buildManifestDigestMatchedOutputs`,
- `releasePrepareEvidence.buildManifestDigestMismatchedOutputs`,
- `releasePrepareEvidence.buildManifestDigestRevalidationInCurrentGate`,
- `buildManifestCrossReference.digestRevalidationInCurrentGate`,
- `buildManifestCrossReference.digestRevalidatedOutputs`,
- `buildManifestCrossReference.digestMatchedOutputs`,
- `buildManifestCrossReference.digestMismatchedOutputs`,
- `buildManifestCrossReference.expectedPaths[].digestRevalidatedInCurrentGate`,
- `buildManifestCrossReference.expectedPaths[].expectedSha256`,
- `buildManifestCrossReference.expectedPaths[].actualSha256`,
- `buildManifestCrossReference.outputs[].sha256`,
- `buildManifestCrossReference.outputs[].actualSha256`,
- `buildManifestCrossReference.outputs[].digestRevalidatedInCurrentGate`,
- `execution.buildManifestDigestRevalidation: true`.

## Classification rules

Expected build-manifest output paths remain:

```text
dist/release-prepare/archives/release.zip
dist/release-prepare/release-archive-evidence.json
dist/release-prepare/release-archive-plan.json
dist/release-prepare/release-plan.json
dist/release-prepare/release-summary.json
dist/release-prepare/staging/release-payload.json
```

Expected, non-duplicate build-manifest outputs are revalidated only when the
local evidence file exists under the project root.

Output statuses are:

- `matched-revalidated`,
- `mismatched-revalidated`,
- `missing-manifest-output-not-revalidated`,
- `missing-local-and-checksum-not-revalidated`,
- `missing-local-artifact-not-revalidated`,
- `missing-checksum-entry-not-revalidated`,
- `unexpected-not-revalidated`,
- `duplicate-not-revalidated`,
- `malformed`.

Build-manifest cross-reference statuses are:

- `complete-digest-revalidated`,
- `mismatch-digest-revalidated`,
- `mismatch-cross-referenced-not-revalidated`,
- `malformed-not-validated`,
- `missing`.

## Explicit non-goals

Gate 277 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
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
| Expected build-manifest outputs are digest-revalidated | Complete | Prepared release evidence reports six digest-revalidated and matched outputs. |
| Edited build-manifest output digest values are reported | Complete | Rewritten `release-summary.json` manifest digest reports `mismatched-revalidated`. |
| Build-manifest cross-reference mismatches remain separate | Complete | Rewritten output path still reports `mismatch-cross-referenced-not-revalidated`. |
| Archive-evidence mismatch remains isolated | Complete | Archive-evidence mismatch tests refresh lower-layer checksum and build-manifest evidence. |
| Non-goals remain disabled | Complete | Release-archive-evidence digest, semantic evidence, archive, publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 277 build-manifest digest revalidation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 278 should move to release-archive-evidence digest metadata
revalidation in `forge release publish`. It must still stop before archive
reopening/revalidation, semantic evidence validation, remote repository calls,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
