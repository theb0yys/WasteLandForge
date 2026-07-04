# Gate 248 - forge explain Provenance Subject Planning Skeleton

Status: Complete

## Purpose

Implement the final executable top-level `forge explain` subject:

```text
forge explain provenance <manifest-or-output-path>
```

This gate explains deterministic provenance boundaries for documented
generated and distribution paths. It does not read manifests, checksums,
provenance sidecars, generated artifacts, project files, or local provider
evidence.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as the place where
  provenance and reasoned explanations are surfaced instead of adding a
  separate top-level provenance command.
- Documented: ADR-009 requires generated artifacts to be disposable,
  rebuildable, and traceable through local manifests.
- Documented: ADR-011 requires deterministic fixture-backed tests,
  offline-first behavior, and no required AI.
- Documented: Gate 247 completed deterministic capability catalogue metadata
  and selected `forge explain provenance <manifest-or-output-path>` as the
  final explain subject skeleton.
- Inferred: A no-read provenance planning skeleton is the safe next step
  because real build-manifest and provenance sidecar readers should be added
  only after the command boundary is stable.

## Implemented

Gate 248 implements:

- `forge explain provenance <manifest-or-output-path>` parsing,
- deterministic path classification by reusing documented generated/dist
  output metadata,
- plain/human output with classification status, normalized path, evidence
  role, output pattern, output kind, boundary, target, rebuild command,
  provenance expectation, trace plan, related rules, and boundaries,
- JSON output with `classification`, `target`, `tracePlan`, and explicit false
  execution flags,
- usage JSON for unknown provenance paths,
- help text that marks all planned `forge explain` subjects implemented.

## Not implemented

Gate 248 does not implement:

- build manifest reads,
- generated manifest reads,
- provenance sidecar reads,
- checksum reads,
- generated artifact existence checks,
- project file reads,
- local provider evidence reads,
- build planning changes,
- generator execution,
- package execution,
- release execution,
- provider resolution,
- capability scan behavior changes,
- graph visualization formats,
- xEdit process execution,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Provenance subject executes | Complete | `forge explain provenance dist/build/build-manifest.json` returns exit code 0 and deterministic provenance planning metadata. |
| JSON provenance planning emitted | Complete | `forge explain provenance generated/docs/reference-index.json --format json` returns classification, target, trace plan, and false execution flags. |
| Unknown paths rejected | Complete | Unknown provenance paths return usage exit code 2. |
| Explain subject lane complete | Complete | Diagnostic, target, output, capability, and provenance subjects now execute. |
| Runtime mutation avoided | Complete | No project files, generated manifests, build manifests, sidecars, checksums, artifacts, provider evidence, external tools, runtime probes, or AI calls are used. |

## Validation

Gate 248 requires build, targeted explain golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 249 should close out the `forge explain` command slice and route the next
implementation lane to `forge clean` planning. It should record the completed
explain subject surface, keep unknown explain subjects reserved/usage-safe,
and stop before delete behavior, filesystem mutation, generated manifest
reads, artifact existence checks, build planning changes, generator execution,
package execution, release execution, provider resolution, capability scan
behavior changes, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
