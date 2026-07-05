# Gate 284 - forge release publish Package-Validation Evidence

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it evaluates local
package-validation evidence while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires layered validation before release publication,
  including package validation after output and capability/environment
  validation.
- Documented: The build/release research says v0.1 packaging should be a
  deterministic staging tree plus ZIP metadata, with package digests recorded
  before release dry-runs and publishing.
- Documented: Gate 283 evaluates local capability/environment evidence and
  explicitly leaves package-validation evidence acceptance for a later gate.
- Inferred: The next safe release-publish slice is local package-validation
  evidence evaluation from an existing
  `forge package --target mcm-json --verify-existing --format json` report,
  before release-verification evidence, human approval, or real publish
  behavior.

## Implemented behavior

Gate 284 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now evaluates:

```text
dist/release-dry-run/package-verify.json
```

as prior `forge package --target mcm-json --verify-existing --format json`
package-validation evidence.

The JSON report now includes:

- `packageValidationEvidence.path`,
- `packageValidationEvidence.exists`,
- `packageValidationEvidence.status`,
- `packageValidationEvidence.checkedInCurrentGate`,
- `packageValidationEvidence.contentReadInCurrentGate`,
- `packageValidationEvidence.target`,
- `packageValidationEvidence.mode`,
- `packageValidationEvidence.outputRoot`,
- `packageValidationEvidence.distScoped`,
- `packageValidationEvidence.packageArchivePresent`,
- package verifier summary and issue counts,
- `requiredEvidence[package-validation]` status from Gate 284,
- `execution.packageValidationEvidenceEvaluation: true`.

## Evaluation rules

Gate 284 reads local evidence only:

- missing `dist/release-dry-run/package-verify.json` reports `missing`,
- malformed JSON or malformed report shape reports `malformed`,
- wrong report identity reports `unexpected-report`,
- reports whose package output root is not under `dist/` report
  `not-dist-scoped`,
- `package` / `mcm-json` / `verify-existing` reports with zero errors and
  zero `WF-BUILD-*` diagnostics report `complete-package-validated`,
- reports with errors or one or more `WF-BUILD-*` diagnostics report
  `package-diagnostics-present`.

Gate 284 does not rerun package verification. It consumes a saved local report
from the existing verifier and exposes its state as release-publish preflight
evidence.

## Explicit non-goals

Gate 284 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- human approval acceptance,
- release-verification evidence acceptance,
- archive payload content validation,
- FOMOD installer assembly,
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
| Missing package-validation evidence is evaluated | Complete | Bare temp projects report `packageValidationEvidence.status: missing`. |
| Clean package-validation evidence is evaluated | Complete | Temp-only `package-verify.json` from a clean package verifier report returns `complete-package-validated`. |
| Package diagnostics are reported | Complete | Temp-only `package-verify.json` with `WF-BUILD-*` verifier issues returns `package-diagnostics-present`. |
| Package-validation evidence execution is explicit | Complete | JSON reports `execution.packageValidationEvidenceEvaluation: true`. |
| Non-goals remain disabled | Complete | Publish, remote, upload, signing, tool, runtime, MO2/GECK, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 284 package-validation evidence evaluation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 285 should move to release-publish release-verification evidence
evaluation. It must still stop before remote repository calls, release
uploads, attestation/signing, external tool execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
or AI behavior.
