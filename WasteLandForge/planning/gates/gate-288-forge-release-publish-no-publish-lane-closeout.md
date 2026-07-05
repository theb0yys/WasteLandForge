# Gate 288 - forge release publish No-Publish Lane Closeout

Status: Complete
Date: 2026-07-05

## Goal

Close the `forge release publish` no-publish preflight lane and route the next
local value slice without enabling release publication.

## Research grounding

- Documented: ADR-011 requires layered validation, deterministic local release
  evidence, offline-first release correctness, and governance that does not
  require AI.
- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  command surface, but R006 says release publishing waits until release
  governance is locked down.
- Documented: R006 says `forge doctor export` can arrive early when it is a
  pure local bundle exporter using shared contracts.
- Inferred: After Gate 287 aggregated local publish readiness while still
  refusing publish, the safe next step is to close the no-publish lane and
  route toward a Doctor export release-readiness handoff rather than adding
  more publish edge cases.

## Implemented behavior

`forge release publish` now includes `laneCloseout` in JSON output:

- `laneCloseout.closedInCurrentGate`,
- `laneCloseout.status`,
- `laneCloseout.lane`,
- `laneCloseout.completedThroughGate`,
- `laneCloseout.nextGate`,
- `laneCloseout.nextValueSlice`,
- `laneCloseout.detail`,
- `laneCloseout.deferredCapabilities`.

Execution metadata now includes:

- `execution.releasePublishLaneCloseout`,
- `execution.nextValueSliceRouted`.

Human/plain output prints a `Lane closeout` section and names Gate 289 as the
next value slice:

```text
forge doctor export release-readiness handoff
```

Normal execution still returns exit code 6. `--dry-run` still reports the same
preflight with exit code 0.

## Explicit non-goals

Gate 288 does not implement:

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
| Lane closeout is visible in machine output | Complete | JSON includes `laneCloseout.status: closed-no-publish-lane`. |
| Next local value slice is routed | Complete | JSON and human output name Gate 289 and `forge doctor export release-readiness handoff`. |
| Publish remains disabled | Complete | Execution flags keep release publishing, repository calls, uploads, and output writes false. |
| Help reflects Gate 288 | Complete | `forge help release publish` documents no-publish lane closeout. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 289 should add a local `forge doctor export` release-readiness handoff
skeleton over existing release/readiness evidence. It must still stop before
remote repository calls, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
