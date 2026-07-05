# Gate 279 - forge release publish Archive Revalidation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it reopens the expected local
release archive and revalidates archive entry metadata while remaining
no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires checksums, local build manifests, release
  evidence, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 268 defines the local release archive evidence shape,
  including expected entry names, deterministic timestamps, and stored
  compression metadata.
- Documented: Gate 278 revalidates release-archive-evidence archive SHA-256
  and length metadata while stopping before archive reopening/revalidation,
  semantic evidence acceptance, or real publish behavior.
- Inferred: The next safe release-publish slice is archive entry metadata
  revalidation for the expected local archive after lower-layer path,
  checksum, build-manifest, and archive digest metadata checks pass.

## Implemented behavior

Gate 279 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.archiveEvidenceArchiveRevalidationInCurrentGate`,
- `releasePrepareEvidence.archiveEvidenceArchiveOpenedInCurrentGate`,
- `releasePrepareEvidence.archiveEvidenceArchiveEntryCountMatchesMetadata`,
- `releasePrepareEvidence.archiveEvidenceArchiveEntryNamesMatchLocal`,
- `releasePrepareEvidence.archiveEvidenceArchiveEvidenceActualEntryNamesMatchLocal`,
- `releasePrepareEvidence.archiveEvidenceArchiveEntryOrderingMatchesLocal`,
- `releasePrepareEvidence.archiveEvidenceArchiveDeterministicTimestampsMatchLocal`,
- `releasePrepareEvidence.archiveEvidenceArchiveStoredCompressionMatchesLocal`,
- `archiveEvidenceCrossReference.archiveRevalidationInCurrentGate`,
- `archiveEvidenceCrossReference.archiveOpenedInCurrentGate`,
- `archiveEvidenceCrossReference.expectedArchiveEntryCount`,
- `archiveEvidenceCrossReference.evidenceActualArchiveEntryCount`,
- `archiveEvidenceCrossReference.actualArchiveEntryCount`,
- `archiveEvidenceCrossReference.archiveEntries[]`,
- `execution.archiveRevalidation: true`.

## Classification rules

The only archive file reopened by this gate is:

```text
dist/release-prepare/archives/release.zip
```

Archive reopening happens only after the expected release-archive-evidence
path coverage, local artifact coverage, checksum sidecar coverage,
build-manifest output coverage, and archive SHA-256/length metadata checks are
clean.

The archive evidence metadata must provide:

```text
archive.entries
expected.entries
expected.timestampUtc
expected.compression
actual.entries
```

Archive evidence cross-reference statuses are:

- `complete-archive-revalidated`,
- `mismatch-archive-revalidated`,
- `mismatch-digest-revalidated`,
- `mismatch-cross-referenced-not-revalidated`,
- `malformed-not-validated`,
- `missing`.

`mismatch-cross-referenced-not-revalidated` and `mismatch-digest-revalidated`
remain separate from archive entry metadata mismatch so lower-layer evidence
failures do not look like archive content failures.

## Explicit non-goals

Gate 279 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
- archive payload content validation,
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
| Clean expected archive metadata is revalidated | Complete | Prepared release evidence reports `complete-archive-revalidated` with matched entry counts, names, order, timestamps, and stored compression. |
| Unexpected archive entry is reported | Complete | A temp-only added ZIP entry reports `mismatch-archive-revalidated` and an `unexpected-not-revalidated` archive entry. |
| Lower-layer mismatch remains separate | Complete | Cross-reference and digest mismatch statuses retain precedence before archive-entry acceptance. |
| Non-goals remain disabled | Complete | Semantic evidence, archive payload validation, publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 279 archive entry metadata revalidation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 280 should move to semantic release evidence validation in
`forge release publish`. It must still stop before remote repository calls,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
