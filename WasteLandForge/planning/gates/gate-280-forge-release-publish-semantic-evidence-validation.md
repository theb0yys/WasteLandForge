# Gate 280 - forge release publish Semantic Evidence Validation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it validates local
release-prepare evidence semantics after lower-layer evidence checks pass
while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires layered validation, release evidence, local
  build manifests, checksums, governance checks, and explicit human approval
  before release publication.
- Documented: Gate 268 defines local release archive evidence and deterministic
  archive metadata.
- Documented: Gate 279 reopens the expected local archive and revalidates entry
  metadata while stopping before semantic release evidence validation, archive
  payload validation, or real publish behavior.
- Inferred: The next safe release-publish slice is semantic validation of the
  existing local release-prepare evidence files, before governance-check
  evaluation, archive payload validation, or real publish behavior.

## Implemented behavior

Gate 280 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now reports:

- `releasePrepareEvidence.semanticEvidenceStatus`,
- `releasePrepareEvidence.semanticEvidenceExpectedChecks`,
- `releasePrepareEvidence.semanticEvidencePassedChecks`,
- `releasePrepareEvidence.semanticEvidenceFailedChecks`,
- `releasePrepareEvidence.semanticEvidenceSkippedChecks`,
- `releasePrepareEvidence.semanticEvidenceValidationInCurrentGate`,
- top-level `semanticEvidenceValidation`,
- `semanticEvidenceValidation.checks[]`,
- `requiredEvidence[semantic-validation]` status from Gate 280,
- `execution.semanticEvidenceValidation: true`.

## Semantic check set

Gate 280 validates local evidence status only. It checks:

- release-plan, release-summary, staging-payload, release-archive-plan,
  release-archive-evidence, and build-manifest contract fields,
- project root and output path-map consistency,
- release-plan planned output consistency,
- release-summary counters,
- staging-payload skeleton/non-mutation boundary,
- release-archive-plan metadata and inputs,
- release-archive-evidence deterministic archive checks,
- build-manifest output set,
- no-publish execution boundaries.

Semantic validation is blocked until lower-layer artifact presence,
shape/classification, checksum digest, build-manifest digest, archive digest,
and archive entry metadata checks are clean.

## Classification rules

Semantic release-evidence statuses are:

- `complete-semantic-validated`,
- `mismatch-semantic-validated`,
- `blocked-not-validated`,
- `malformed-not-validated`.

Release-prepare evidence status now reports:

- `complete-semantic-validated` for clean local release evidence,
- `semantic-evidence-mismatch-validated` for semantic mismatch after lower
  layers pass.

Lower-layer mismatch statuses keep precedence so checksum, build-manifest, and
archive evidence failures do not look like semantic release-evidence failures.

## Explicit non-goals

Gate 280 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- governance-check execution,
- human approval acceptance,
- archive payload content validation,
- FOMOD installer assembly,
- evidence acceptance for publishing,
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
| Clean local release evidence is semantically validated | Complete | Prepared release evidence reports `complete-semantic-validated` with 15 passed semantic checks. |
| Semantic mismatch is reported after lower layers pass | Complete | Tampered release-summary counters report `semantic-evidence-mismatch-validated` while checksum, build-manifest, and archive evidence checks stay complete. |
| Lower-layer mismatch remains separate | Complete | Existing checksum, build-manifest, and archive mismatch tests keep their lower-layer statuses. |
| Non-goals remain disabled | Complete | Archive payload validation, publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 280 semantic release-evidence validation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 281 should move to release-publish governance-check evaluation. It must
still stop before remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
