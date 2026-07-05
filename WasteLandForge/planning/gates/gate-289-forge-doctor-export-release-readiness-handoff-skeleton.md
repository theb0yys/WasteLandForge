# Gate 289 - forge doctor export Release Readiness Handoff Skeleton

Status: Complete
Date: 2026-07-05

## Goal

Add a local `forge doctor export` release-readiness handoff projection that
reuses existing release-publish dry-run preflight evidence without enabling
release publication.

## Research grounding

- Documented: ADR-010/R006 keep `forge doctor export` and
  `forge release publish` in the canonical command surface.
- Documented: R006 says `forge doctor export` can arrive early when it is a
  pure local bundle exporter using shared contracts.
- Documented: ADR-011 requires layered validation, deterministic local release
  evidence, offline-first release correctness, mandatory build manifests, and
  governance that does not require AI.
- Inferred: After Gate 288 closed the no-publish release-publish lane and
  routed to a Doctor export handoff, the safe next step is to expose the
  existing publish-readiness aggregate inside Doctor export artifacts.

## Implemented behavior

`forge doctor export` now includes a `releaseReadiness` object in JSON output.
When a project root is supplied, the projection is derived from the same
local preflight planner used by:

```text
forge release publish <project-root> --dry-run --format json --no-input
```

The projection reports:

- whether release-readiness was included,
- whether the current gate evaluated it,
- source command,
- release-readiness status and detail,
- local evidence root,
- release-prepare evidence status,
- publish-readiness status,
- evidence, governance, approval, and local precondition satisfaction,
- `readyForRealPublish`,
- required/satisfied/blocking check counts,
- blocking check IDs,
- required evidence item statuses,
- no-publish boundaries.

Plain and Markdown Doctor export output now include a release-readiness
section. `--bundle` archives include:

```text
release-readiness/index.json
release-readiness/index.md
```

The bundle README, bundle index, handoff summary, summary index, manifest,
and checksums include those release-readiness entries.

When no project root is supplied, release-readiness is still represented as
`not-included` with no evidence entries.

## Explicit non-goals

Gate 289 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- release-readiness triage/worklist integration,
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
| JSON handoff is visible | Complete | `doctor export` JSON includes top-level `releaseReadiness`. |
| No-project exports are explicit | Complete | No project root reports `status: not-included` and `detail: project-root-not-provided`. |
| Project exports reuse local release-publish readiness | Complete | Project-root exports derive status, evidence, checks, and boundaries from the dry-run preflight planner. |
| Bundle contains release-readiness indexes | Complete | ZIP bundles include `release-readiness/index.json` and `release-readiness/index.md`. |
| No publish behavior is enabled | Complete | Output reports no-publish boundaries and no release outputs are written. |
| Help reflects Gate 289 | Complete | `forge help doctor export` documents the release-readiness handoff projection. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore --filter DoctorExport
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 290 should integrate release-readiness blockers into the existing Doctor
export triage/worklist handoff. It must still stop before remote repository
calls, release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
