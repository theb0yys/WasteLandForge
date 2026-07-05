# Gate 290 - forge doctor export Release Readiness Triage Worklist

Status: Complete
Date: 2026-07-05

## Goal

Integrate `forge doctor export` release-readiness blockers into the existing
Doctor triage and operator worklist without enabling release publication.

## Research grounding

- Documented: ADR-010/R006 keep `forge doctor export` and
  `forge release publish` in the canonical command surface.
- Documented: R006 says `forge doctor export` can arrive early when it is a
  pure local bundle exporter using shared contracts.
- Documented: ADR-011 requires layered validation, deterministic local release
  evidence, offline-first release correctness, mandatory local build
  manifests, and governance that does not require AI.
- Inferred: After Gate 289 exposed local release-readiness status in Doctor
  export artifacts, blocking checks should become first-class Doctor triage
  and worklist items so operators know what must be resolved before release
  readiness can advance.

## Implemented behavior

When `forge doctor export` includes release-readiness data and the local
release-readiness projection has blocking checks, Doctor triage now:

- reports status `blocked`,
- adds a `release-readiness-blocking-checks` blocking item,
- reports `releaseReadinessBlockingChecks` in primary and bundle triage
  summaries,
- adds `review-release-readiness` and, when applicable,
  `confirm-release-approval-dry-run` command hints,
- creates one blocker work item per blocking release-readiness check,
- maps those work items to `releaseReadiness` in primary output and
  `release-readiness/index.md` in bundle output,
- adds `releaseReadiness` / `release-readiness/index.md` as a review target.

The generated command hints remain local and no-publish:

```text
forge release publish <project-root> --dry-run --format json --no-input
forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input
```

When release-readiness is not included, Doctor triage is unchanged.

## Explicit non-goals

Gate 290 does not implement:

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
| Release-readiness blockers affect triage status | Complete | Included blocking release-readiness checks make Doctor triage `blocked`. |
| Blocking checks become worklist items | Complete | Each blocking release-readiness check becomes a `resolve-release-readiness-*` blocker item. |
| Command hints stay no-publish | Complete | Hints use `forge release publish --dry-run` only. |
| Bundle triage points to release-readiness index | Complete | Bundle worklist paths and review paths include `release-readiness/index.md`. |
| No publish behavior is enabled | Complete | No release outputs, remote calls, uploads, signing, attestation, external tools, runtime probes, or AI are added. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore --filter DoctorExport
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 291 should close the local Doctor export release-readiness handoff lane
and route to the next value slice. It must still stop before remote
repository calls, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
