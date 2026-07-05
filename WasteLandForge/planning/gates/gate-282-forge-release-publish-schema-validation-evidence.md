# Gate 282 - forge release publish Schema-Validation Evidence

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it evaluates local
schema-validation evidence while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires layered validation before release publication,
  with schema validation preceding semantic, capability/environment, package,
  release, governance, and approval stages.
- Documented: Gate 281 evaluates local governance checks and explicitly leaves
  schema-validation evidence acceptance for a later gate.
- Inferred: The next safe release-publish slice is local schema-validation
  evidence evaluation from an existing Forge diagnostic report, before
  capability/environment evidence, package evidence, release-verification
  evidence, human approval, or real publish behavior.

## Implemented behavior

Gate 282 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now evaluates:

```text
dist/release-dry-run/validation.json
```

as prior `forge validate --format json` schema-validation evidence.

The JSON report now includes:

- `schemaValidationEvidence.path`,
- `schemaValidationEvidence.exists`,
- `schemaValidationEvidence.status`,
- `schemaValidationEvidence.checkedInCurrentGate`,
- `schemaValidationEvidence.contentReadInCurrentGate`,
- `schemaValidationEvidence.errors`,
- `schemaValidationEvidence.warnings`,
- `schemaValidationEvidence.notes`,
- `schemaValidationEvidence.issues`,
- `schemaValidationEvidence.schemaIssues`,
- `schemaValidationEvidence.detail`,
- `requiredEvidence[schema-validation]` status from Gate 282,
- `execution.schemaValidationEvidenceEvaluation: true`.

## Evaluation rules

Gate 282 reads local evidence only:

- missing `dist/release-dry-run/validation.json` reports `missing`,
- malformed JSON or malformed report shape reports `malformed`,
- wrong report identity reports `unexpected-report`,
- a Forge `validate` report with zero `WF-SCHEMA-*` issues reports
  `complete-schema-validated`,
- a Forge `validate` report with one or more `WF-SCHEMA-*` issues reports
  `schema-diagnostics-present`.

Non-schema diagnostics are counted but do not make schema-validation evidence
fail in this gate.

## Explicit non-goals

Gate 282 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- human approval acceptance,
- capability/environment evidence acceptance,
- package-validation evidence acceptance,
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
| Missing schema-validation evidence is evaluated | Complete | Bare temp projects report `schemaValidationEvidence.status: missing`. |
| Clean schema-validation evidence is evaluated | Complete | Temp-only `dist/release-dry-run/validation.json` with no `WF-SCHEMA-*` issues reports `complete-schema-validated`. |
| Schema diagnostics are reported | Complete | Temp-only validation report with `WF-SCHEMA-001` reports `schema-diagnostics-present`. |
| Schema evidence execution is explicit | Complete | JSON reports `execution.schemaValidationEvidenceEvaluation: true`. |
| Non-goals remain disabled | Complete | Publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 282 schema-validation evidence evaluation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 283 should move to release-publish capability/environment evidence
evaluation. It must still stop before remote repository calls, release
uploads, attestation/signing, external tool execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
or AI behavior.
