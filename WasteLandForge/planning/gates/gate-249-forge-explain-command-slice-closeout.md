# Gate 249 - forge explain Command Slice Closeout

Status: Complete

## Purpose

Close out the top-level `forge explain` command slice and route the next
implementation lane to `forge clean` planning.

This gate records the completed explain subject surface. It does not add new
runtime command behavior.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as the place for
  diagnostic, target, output path, capability, and provenance explanations.
- Documented: R006 defines `forge clean` as a canonical command whose scope is
  `generated`, `dist`, `cache`, or `all`.
- Documented: ADR-009 requires generated artifacts to be disposable,
  rebuildable, traceable through local manifests, and separated from canonical
  source.
- Documented: ADR-011 requires offline-first, deterministic validation and test
  behavior.
- Documented: Gate 248 completed the final planned executable `forge explain`
  subject skeleton with `forge explain provenance <manifest-or-output-path>`.
- Inferred: A closeout gate is needed before `forge clean` so routing,
  documentation, and slash-command prompts stop requesting more explain subject
  skeletons.

## Implemented

Gate 249 records that the top-level `forge explain` lane now includes:

```text
forge explain diagnostic <rule-id>
forge explain target <target-id>
forge explain output <generated-or-dist-path>
forge explain capability <capability-id>
forge explain provenance <manifest-or-output-path>
```

It also updates planning and routing documents so the next implementation lane
starts with `forge clean` planning.

## Not implemented

Gate 249 does not implement:

- delete behavior,
- filesystem mutation,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- artifact existence checks,
- build planning changes,
- generator execution,
- package execution,
- release execution,
- provider resolution,
- capability scan behavior changes,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Explain subject surface recorded | Complete | Diagnostic, target, output, capability, and provenance subjects are listed as the complete planned top-level explain slice. |
| Unknown explain subjects remain usage-safe | Complete | Gate 248 retained reserved-command JSON for unknown explain subjects; Gate 249 changes no runtime behavior. |
| Clean lane routed | Complete | Planning and prompt-routing docs point the next gate at `forge clean` planning. |
| Runtime mutation avoided | Complete | No command behavior, filesystem deletion, manifest reads, artifact checks, external tools, runtime probes, or AI calls are added. |

## Validation

Gate 249 requires build, targeted explain golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 250 should implement the `forge clean` planning skeleton. It should define
the command contract, documented scopes, safety boundaries, usage-safe unknown
scope behavior, and reporting expectations for `generated`, `dist`, `cache`,
and `all`, stopping before delete behavior, filesystem mutation, generated
manifest reads, build manifest reads, provenance sidecar reads, checksum reads,
artifact existence checks, build planning changes, generator execution, package
execution, release execution, provider resolution, capability scan behavior
changes, external tool execution, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
