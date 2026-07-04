# Gate 274 - forge release publish Build Manifest Cross-Reference

Status: Complete
Date: 2026-07-04

## Goal

Extend `forge release publish` preflight so it cross-references
`dist/release-prepare/build-manifest.json` outputs against local
release-prepare evidence paths and `checksums.sha256` entries while remaining
no-publish by default.

## Research grounding

- Documented: ADR-011 requires local build manifests, checksums, release
  verification, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 273 classifies checksum sidecar entries and expected-path
  coverage but stops before digest revalidation.
- Inferred: The next safe release-publish slice is build-manifest output
  cross-reference before checksum digest revalidation, archive revalidation,
  semantic evidence acceptance, or publish behavior exists.

## Implemented behavior

Gate 274 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.buildManifestExpectedOutputs`,
- `releasePrepareEvidence.buildManifestParsedOutputs`,
- `releasePrepareEvidence.buildManifestCoveredExpectedOutputs`,
- `releasePrepareEvidence.buildManifestMissingExpectedOutputs`,
- `releasePrepareEvidence.buildManifestMissingLocalArtifactOutputs`,
- `releasePrepareEvidence.buildManifestMissingChecksumEntryOutputs`,
- `releasePrepareEvidence.buildManifestUnexpectedOutputs`,
- `releasePrepareEvidence.buildManifestDuplicateOutputs`,
- `releasePrepareEvidence.buildManifestMalformedOutputs`,
- `releasePrepareEvidence.buildManifestOutputCrossReferenceInCurrentGate`,
- top-level `buildManifestCrossReference`,
- `buildManifestCrossReference.expectedPaths[]`,
- `buildManifestCrossReference.outputs[]`,
- `execution.buildManifestOutputCrossReference: true`,
- `execution.buildManifestDigestRevalidation: false`,
- `execution.semanticEvidenceValidation: false`.

## Classification rules

The expected release-prepare build-manifest output paths are:

```text
dist/release-prepare/archives/release.zip
dist/release-prepare/release-archive-evidence.json
dist/release-prepare/release-archive-plan.json
dist/release-prepare/release-plan.json
dist/release-prepare/release-summary.json
dist/release-prepare/staging/release-payload.json
```

Build-manifest outputs are parsed only for:

- output object shape,
- `outputs[].path`,
- exact expected-path coverage,
- duplicate output paths,
- unexpected output paths,
- local evidence path presence,
- matching checksum sidecar path presence after stripping
  `dist/release-prepare/`.

The command does not open referenced outputs, recompute SHA-256 digests, or
accept release evidence.

## Explicit non-goals

Gate 274 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
- checksum digest revalidation,
- build-manifest digest revalidation,
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
| Build-manifest outputs parsed | Complete | Prepared release manifest reports six parsed outputs. |
| Expected output coverage reported | Complete | Prepared release manifest reports six covered expected outputs. |
| Local evidence and checksum sidecar paths cross-referenced | Complete | Prepared release manifest outputs report local artifact and checksum entry presence. |
| Digest revalidation remains disabled | Complete | Execution reports checksum and build-manifest digest revalidation as false. |
| Build-manifest mismatch reported | Complete | Rewritten build-manifest output path reports mismatch status without publishing. |
| No publish side effects | Complete | Publish, remote, upload, signing, checksum digest revalidation, build-manifest digest revalidation, archive revalidation, semantic evidence validation, external tool, runtime probe, and AI flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 274 build-manifest output cross-reference. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
```

## Next gate

Gate 275 should add release-archive-evidence cross-reference for
`forge release publish`. It should compare `release-archive-evidence.json`
metadata to local evidence, checksum sidecar paths, and build-manifest outputs
without reopening the archive or recomputing digests. It must still stop
before archive reopening/revalidation, checksum digest revalidation, semantic
evidence validation, remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
