# Gate 287 - forge release publish Readiness Aggregation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it aggregates local publish
readiness without enabling release publication.

## Research grounding

- Documented: ADR-011 requires layered validation, release governance,
  mandatory local build manifests, SemVer-governed version streams, and
  offline-first release correctness.
- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  command surface, but release publishing waits until release governance is
  locked down.
- Documented: R008 recommends human approval and release validation before
  publication.
- Documented: Gate 286 records explicit human approval but leaves readiness
  aggregation for a later gate.
- Inferred: The next safe release-publish slice is an aggregate readiness
  report over already-evaluated local evidence, governance, and approval
  states, while still refusing real publish.

## Implemented behavior

Gate 287 keeps the command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run]
  [--yes] [--confirm <project-id>] [--no-input]
```

The JSON report now includes:

- `publishReadiness.evaluatedInCurrentGate`,
- `publishReadiness.status`,
- `publishReadiness.localPreconditionsSatisfied`,
- `publishReadiness.evidenceSatisfied`,
- `publishReadiness.governanceSatisfied`,
- `publishReadiness.approvalSatisfied`,
- `publishReadiness.publishExecutionEnabled`,
- `publishReadiness.readyForRealPublish`,
- `publishReadiness.requiredChecks`,
- `publishReadiness.satisfiedChecks`,
- `publishReadiness.blockingChecks`,
- `publishReadiness.blockingCheckIds`,
- `publishReadiness.checks`,
- `execution.publishReadinessEvaluation`,
- `execution.localPublishPreconditionsSatisfied`,
- `execution.publishReadyForRealPublish`.

## Evaluation rules

Gate 287 aggregates these accepted states:

- schema validation: `complete-schema-validated`,
- semantic validation: `complete-semantic-validated`,
- capability/environment validation:
  `complete-capability-environment-validated`,
- package validation: `complete-package-validated`,
- release verification: `complete-release-verified`,
- release-prepare build manifest: `complete-digest-revalidated`,
- release-prepare checksums: `complete-digest-revalidated`,
- release archive evidence: `complete-archive-revalidated`,
- governance checks: `complete-governance-evaluated`,
- human approval: `provided`.

If any required state is not satisfied, `publishReadiness.status` is
`blocked-by-preconditions`.

If all local preconditions are satisfied, `publishReadiness.status` is
`locally-ready-no-publish-gate`, but `publishReadiness.readyForRealPublish`
remains false because publish execution is still disabled.

Normal execution still returns exit code 6. If local preconditions are
satisfied, the refusal reason is:

```text
Local publish readiness is satisfied, but Gate 287 still does not publish releases.
```

`--dry-run` reports the same aggregate with exit code 0.

## Explicit non-goals

Gate 287 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
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
| Missing local evidence blocks readiness | Complete | Bare publish preflight reports `publishReadiness.status: blocked-by-preconditions`. |
| Matching approval participates in readiness | Complete | Confirmed approval satisfies only the human approval readiness check when other evidence is missing. |
| Complete synthetic local evidence is aggregated | Complete | Temp-only evidence reports `localPreconditionsSatisfied: true` and `readyForRealPublish: false`. |
| Publish remains disabled | Complete | Complete readiness still reports `releasePublishing: false`, no remote calls, and exits 6. |
| CLI help updated | Complete | `forge help release publish` documents publish-readiness aggregation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 288 should close the `forge release publish` no-publish lane and route
the next value slice. It must still stop before remote repository calls,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
