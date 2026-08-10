# Gate 245 - forge explain Target Subject Skeleton

Status: Complete

## Purpose

Implement the second executable top-level `forge explain` subject:

```text
forge explain target <target-id>
```

This gate explains documented command targets from deterministic local
metadata. It does not inspect a project, plan a build, execute a generator,
read generated manifests, or check artifact existence.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as a canonical
  offline-first CLI command for diagnostic IDs, targets, output paths,
  capability IDs, provenance, and reasoned explanations.
- Documented: ADR-009 requires generated artifacts to be disposable and
  traceable through local manifests, with deterministic build graph behavior.
- Documented: Gate 244 completed diagnostic rule explanation metadata and
  selected `forge explain target <target-id>` as the next subject skeleton.
- Inferred: Target explanation is the safe next value step because it helps
  users understand implemented Forge targets without executing those targets.

## Implemented

Gate 245 implements:

- `forge explain target <target-id>` parsing,
- deterministic target metadata for `reports`, `mcm-json`, `jip-scripts`,
  `xedit-audit`, `xedit-audit-report-handoff`, `docs`, `graph`, and
  `release-verify`,
- plain/human output with title, category, status, command surface, output
  roots, primary outputs, required capabilities, related rules, and
  boundaries,
- JSON output with a `target` object and explicit false execution flags,
- usage JSON for unknown target IDs,
- help text that marks `diagnostic` and `target` implemented while leaving
  `output`, `capability`, and `provenance` reserved.

## Not implemented

Gate 245 does not implement:

- build planning changes,
- generator execution,
- package execution,
- release execution,
- project file reads,
- generated manifest reads,
- generated artifact existence checks,
- provenance sidecar reads,
- output explanation execution,
- capability subject execution,
- provenance subject execution,
- runtime provider resolution,
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
| Target subject executes | Complete | `forge explain target mcm-json` returns exit code 0 and deterministic target metadata. |
| JSON target metadata emitted | Complete | `forge explain target graph --format json` returns target metadata and false execution flags. |
| Unknown targets rejected | Complete | Unknown target IDs return usage exit code 2. |
| Other explain subjects stay reserved | Complete | `output`, `capability`, and `provenance` still use reserved status metadata. |
| Runtime mutation avoided | Complete | No project files, generated manifests, provenance sidecars, artifacts, provider evidence, external tools, or AI calls are used. |

## Validation

Gate 245 requires build, targeted explain golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 246 should implement the next `forge explain` subject skeleton:
`forge explain output <generated-or-dist-path>`. It should classify expected
generated or dist output paths from deterministic target metadata only, and
stop before generated manifest reads, generated artifact existence checks,
provenance sidecar reads, build planning changes, generator execution,
package execution, release execution, runtime provider resolution, capability
scan behavior changes, graph visualization formats, external tool execution,
plugin mutation, MO2 automation, GECK automation, runtime probes, real
third-party plugin fixtures, or AI behavior.
