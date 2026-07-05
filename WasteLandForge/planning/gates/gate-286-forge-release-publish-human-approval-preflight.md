# Gate 286 - forge release publish Human-Approval Preflight

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it records explicit human approval
without enabling release publication.

## Research grounding

- Documented: ADR-011 requires release governance and contribution rules that
  do not require AI.
- Documented: R008 recommends at least one human approval, with stricter
  approval for release and schema changes.
- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  command surface, but release publishing waits until release governance is
  locked down.
- Documented: Gate 285 evaluates release-verification evidence and leaves
  explicit human approval for a later gate.
- Inferred: The safest approval slice is a local manifest-ID confirmation
  preflight using the existing severe-operation pattern of `--yes` plus
  `--confirm <project-id>`, while still refusing real publish.

## Implemented behavior

Gate 286 extends the command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run]
  [--yes] [--confirm <project-id>] [--no-input]
```

The command now reports `approval` fields:

- `approval.required`,
- `approval.provided`,
- `approval.yesProvided`,
- `approval.confirmationValue`,
- `approval.confirmationValidated`,
- `approval.confirmationMatches`,
- `approval.projectManifestRead`,
- `approval.projectManifestPath`,
- `approval.projectId`,
- `approval.status`,
- `approval.detail`,
- `execution.humanApprovalEvaluation`,
- `execution.humanApprovalConfirmationValidated`,
- `execution.humanApprovalProvided`.

## Evaluation rules

Gate 286 reads only the root project manifest ID when approval is attempted:

- no `--yes` and no `--confirm` reports `missing`,
- only one of `--yes` or `--confirm` reports `incomplete`,
- missing project manifest reports `project-manifest-missing`,
- multiple project manifests report `multiple-project-manifests`,
- unreadable manifest reports `project-manifest-unreadable`,
- missing manifest ID reports `project-id-missing`,
- invalid manifest ID reports `project-id-invalid`,
- confirmation mismatch reports `project-id-mismatch`,
- `--yes --confirm <project-id>` matching the root manifest ID reports
  `provided`.

Even when approval is `provided`, normal `forge release publish` execution
still returns exit code 6 and reports:

```text
Release publish approval was recorded, but Gate 286 still does not publish releases.
```

`--dry-run` reports the same preflight with exit code 0.

## Explicit non-goals

Gate 286 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- publish-ready aggregation,
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
| Missing approval is explicit | Complete | Bare publish preflight reports `approval.status: missing`. |
| Matching approval is recorded | Complete | `--yes --confirm io.github.theboyyss.examplemod` reports `approval.status: provided`. |
| Mismatched approval is rejected | Complete | Wrong project ID reports `approval.status: project-id-mismatch`. |
| Publish remains disabled | Complete | Confirmed publish still reports `releasePublishing: false` and exits 6. |
| CLI help updated | Complete | `forge help release publish` documents `--yes --confirm <project-id>`. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 287 should move to publish-readiness aggregation for the no-publish
preflight. It must still stop before remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
