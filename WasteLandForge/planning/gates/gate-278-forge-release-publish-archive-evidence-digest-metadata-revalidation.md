# Gate 278 - forge release publish Archive Evidence Digest Metadata Revalidation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it revalidates
`release-archive-evidence.json` archive SHA-256 and length metadata against
the expected local release archive while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires checksums, local build manifests, release
  evidence, governance checks, and explicit human approval before release
  publication.
- Documented: Gate 275 cross-references release-archive-evidence output
  metadata without archive digest revalidation.
- Documented: Gate 276 revalidates checksum sidecar digests.
- Documented: Gate 277 revalidates build-manifest output digests while
  stopping before release-archive-evidence archive digest metadata
  revalidation, archive reopening, semantic evidence acceptance, or real
  publish behavior.
- Inferred: The next safe release-publish slice is release-archive-evidence
  archive digest metadata revalidation for the expected local archive file,
  before any archive reopening/revalidation, semantic evidence acceptance, or
  real publish behavior.

## Implemented behavior

Gate 278 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.archiveEvidenceArchiveSha256MatchesLocal`,
- `releasePrepareEvidence.archiveEvidenceArchiveLengthMatchesLocal`,
- `releasePrepareEvidence.archiveEvidenceDigestRevalidationInCurrentGate`,
- `archiveEvidenceCrossReference.digestRevalidationInCurrentGate`,
- `archiveEvidenceCrossReference.archiveSha256MatchesLocal`,
- `archiveEvidenceCrossReference.archiveLengthMatchesLocal`,
- `archiveEvidenceCrossReference.expectedArchiveSha256`,
- `archiveEvidenceCrossReference.actualArchiveSha256`,
- `archiveEvidenceCrossReference.expectedArchiveLength`,
- `archiveEvidenceCrossReference.actualArchiveLength`,
- `execution.archiveEvidenceDigestRevalidation: true`.

## Classification rules

The only archive file revalidated by this gate is:

```text
dist/release-prepare/archives/release.zip
```

The archive evidence metadata must provide:

```text
archive.path
archive.sha256
archive.length
```

Archive evidence cross-reference statuses are:

- `complete-digest-revalidated`,
- `mismatch-digest-revalidated`,
- `mismatch-cross-referenced-not-revalidated`,
- `malformed-not-validated`,
- `missing`.

`mismatch-cross-referenced-not-revalidated` remains separate from digest
metadata mismatch so unexpected release-archive-evidence output paths do not
look like archive digest failures.

## Explicit non-goals

Gate 278 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- semantic evidence validation,
- archive revalidation,
- archive reopening,
- archive entry inspection,
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
| Expected local archive SHA-256 metadata is revalidated | Complete | Prepared release evidence reports matching expected and actual archive SHA-256 values. |
| Expected local archive length metadata is revalidated | Complete | Prepared release evidence reports matching expected and actual archive length values. |
| Edited archive SHA-256 metadata is reported | Complete | Rewritten `archive.sha256` reports `mismatch-digest-revalidated`. |
| Archive-evidence path mismatches remain separate | Complete | Rewritten output metadata still reports `mismatch-cross-referenced-not-revalidated`. |
| Non-goals remain disabled | Complete | Semantic evidence, archive reopening/revalidation, publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 278 archive digest metadata revalidation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 279 should move to release archive reopening/revalidation in
`forge release publish`. It must still stop before semantic evidence
validation, remote repository calls, release uploads, attestation/signing,
external tool execution, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, or AI behavior.
