# Gate 283 - forge release publish Capability/Environment Evidence

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it evaluates local
capability/environment evidence while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires layered validation before release publication,
  including capability/environment validation after schema and semantic
  validation.
- Documented: ADR-008 requires local-first deterministic capability/provider
  detection, with runtime probes only as secondary enrichment.
- Documented: Gate 282 evaluates local schema-validation evidence and
  explicitly leaves capability/environment evidence acceptance for a later
  gate.
- Inferred: The next safe release-publish slice is local
  capability/environment evidence evaluation from an existing
  `forge capabilities scan --project --format json` report, before
  package-validation evidence, release-verification evidence, human approval,
  or real publish behavior.

## Implemented behavior

Gate 283 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now evaluates:

```text
dist/release-dry-run/capabilities-scan.json
```

as prior `forge capabilities scan --project --format json`
capability/environment evidence.

The JSON report now includes:

- `capabilityEnvironmentEvidence.path`,
- `capabilityEnvironmentEvidence.exists`,
- `capabilityEnvironmentEvidence.status`,
- `capabilityEnvironmentEvidence.checkedInCurrentGate`,
- `capabilityEnvironmentEvidence.contentReadInCurrentGate`,
- `capabilityEnvironmentEvidence.projectScoped`,
- `capabilityEnvironmentEvidence.runtimeProbesEnabled`,
- `capabilityEnvironmentEvidence.mo2VfsEnabled`,
- provider, capability, requirement, diagnostic, and Doctor summary counts,
- `requiredEvidence[capability-environment-validation]` status from Gate 283,
- `execution.capabilityEnvironmentEvidenceEvaluation: true`.

## Evaluation rules

Gate 283 reads local evidence only:

- missing `dist/release-dry-run/capabilities-scan.json` reports `missing`,
- malformed JSON or malformed report shape reports `malformed`,
- wrong report identity reports `unexpected-report`,
- reports with runtime probes or MO2 VFS evidence enabled report
  `unsupported-evidence`,
- reports without project-scoped requirement summary report
  `not-project-scoped`,
- project-scoped reports with zero required-unavailable requirements and zero
  `WF-CAP-*` diagnostics report
  `complete-capability-environment-validated`,
- project-scoped reports with one or more required-unavailable requirements or
  `WF-CAP-*` diagnostics report `capability-diagnostics-present`.

Unknown optional providers are counted but do not make release-publish
capability/environment evidence fail in this gate unless they are projected as
required-unavailable requirements or `WF-CAP-*` diagnostics.

## Explicit non-goals

Gate 283 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- human approval acceptance,
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
| Missing capability/environment evidence is evaluated | Complete | Bare temp projects report `capabilityEnvironmentEvidence.status: missing`. |
| Clean capability/environment evidence is evaluated | Complete | Temp-only project-scoped `capabilities-scan.json` with no required-unavailable requirements and no `WF-CAP-*` issues reports `complete-capability-environment-validated`. |
| Capability diagnostics are reported | Complete | Temp-only project-scoped scan report with `WF-CAP-*` issues reports `capability-diagnostics-present`. |
| Capability/environment evidence execution is explicit | Complete | JSON reports `execution.capabilityEnvironmentEvidenceEvaluation: true`. |
| Non-goals remain disabled | Complete | Publish, remote, upload, signing, tool, runtime, MO2/GECK, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 283 capability/environment evidence evaluation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 284 should move to release-publish package-validation evidence
evaluation. It must still stop before remote repository calls, release
uploads, attestation/signing, external tool execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
or AI behavior.
