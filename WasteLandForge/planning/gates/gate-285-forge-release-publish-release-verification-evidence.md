# Gate 285 - forge release publish Release-Verification Evidence

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it evaluates local
release-verification evidence while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` and
  `forge release verify` in the canonical release command surface.
- Documented: ADR-011 requires layered validation before release publication,
  including release validation after package validation.
- Documented: The validation and release research requires release dry-runs,
  mandatory local build manifests, checksums, and governance before publish.
- Documented: Gate 284 evaluates local package-validation evidence and leaves
  release-verification evidence acceptance for a later gate.
- Inferred: The next safe release-publish slice is local release-verification
  evidence evaluation from an existing `forge release verify --format json`
  report, before explicit human approval or real publish behavior.

## Implemented behavior

Gate 285 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now evaluates:

```text
dist/release-dry-run/release-verify.json
```

as prior `forge release verify --format json` release-verification evidence.

The JSON report now includes:

- `releaseVerificationEvidence.path`,
- `releaseVerificationEvidence.exists`,
- `releaseVerificationEvidence.status`,
- `releaseVerificationEvidence.checkedInCurrentGate`,
- `releaseVerificationEvidence.contentReadInCurrentGate`,
- `releaseVerificationEvidence.dryRun`,
- `releaseVerificationEvidence.outputRoot`,
- `releaseVerificationEvidence.distScoped`,
- release verifier summary and issue counts,
- `requiredEvidence[release-verification]` status from Gate 285,
- `execution.releaseVerificationEvidenceEvaluation: true`.

## Evaluation rules

Gate 285 reads local evidence only:

- missing `dist/release-dry-run/release-verify.json` reports `missing`,
- malformed JSON or malformed report shape reports `malformed`,
- wrong report identity reports `unexpected-report`,
- non-dry-run report evidence reports `unsupported-evidence`,
- reports whose output root is present but not under `dist/` report
  `not-dist-scoped`,
- `release verify` reports with `status: passed`, dry-run evidence,
  `dist/` outputs, zero errors, and zero `WF-REL-*` diagnostics report
  `complete-release-verified`,
- reports with errors or one or more `WF-REL-*` diagnostics report
  `release-diagnostics-present`.

Gate 285 does not rerun release verification. It consumes a saved local report
from the existing verifier and exposes its state as release-publish preflight
evidence.

## Explicit non-goals

Gate 285 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- human approval acceptance,
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
| Missing release-verification evidence is evaluated | Complete | Bare temp projects report `releaseVerificationEvidence.status: missing`. |
| Clean release-verification evidence is evaluated | Complete | Temp-only `release-verify.json` from a clean release verifier report returns `complete-release-verified`. |
| Release diagnostics are reported | Complete | Temp-only `release-verify.json` with `WF-REL-*` verifier issues returns `release-diagnostics-present`. |
| Release-verification evidence execution is explicit | Complete | JSON reports `execution.releaseVerificationEvidenceEvaluation: true`. |
| Non-goals remain disabled | Complete | Publish, remote, upload, signing, tool, runtime, MO2/GECK, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 285 release-verification evidence evaluation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 286 should move to the explicit human-approval preflight skeleton. It
must still stop before remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
