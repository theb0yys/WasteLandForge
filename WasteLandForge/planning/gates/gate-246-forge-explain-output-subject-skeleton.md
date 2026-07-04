# Gate 246 - forge explain Output Subject Skeleton

Status: Complete

## Purpose

Implement the third executable top-level `forge explain` subject:

```text
forge explain output <generated-or-dist-path>
```

This gate explains documented generated and distribution output paths from
deterministic local metadata. It does not inspect a project, read generated
manifests, check artifact existence, read provenance sidecars, plan a build,
execute a generator, or resolve providers.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as a canonical
  offline-first CLI command for diagnostic IDs, targets, output paths,
  capability IDs, provenance, and reasoned explanations.
- Documented: ADR-009 requires generated artifacts to be disposable,
  rebuildable, and traceable through local manifests.
- Documented: ADR-011 requires deterministic fixture-backed tests,
  offline-first behavior, and no required AI.
- Documented: Gate 245 completed deterministic target metadata and selected
  `forge explain output <generated-or-dist-path>` as the next subject
  skeleton.
- Inferred: Output explanation is the safe next value step because it connects
  generated/dist paths to targets and rebuild commands without reading local
  artifact payloads or manifests.

## Implemented

Gate 246 implements:

- `forge explain output <generated-or-dist-path>` parsing,
- deterministic output pattern metadata for documented `generated/` and
  `dist/` paths produced by `reports`, `mcm-json`, `jip-scripts`,
  `xedit-audit`, `xedit-audit-report-handoff`, `docs`, `graph`, and
  `release-verify`,
- path normalization for slash and leading `./` variants,
- plain/human output with classification status, normalized path, output
  pattern, output kind, boundary, target, rebuild command, provenance
  expectation, related rules, and boundaries,
- JSON output with a `classification` object, target metadata, and explicit
  false execution flags,
- usage JSON for unknown output paths,
- help text that marks `diagnostic`, `target`, and `output` implemented while
  leaving `capability` and `provenance` reserved.

## Not implemented

Gate 246 does not implement:

- project file reads,
- generated manifest reads,
- generated artifact existence checks,
- provenance sidecar reads,
- build planning changes,
- generator execution,
- package execution,
- release execution,
- runtime provider resolution,
- capability scan behavior changes,
- graph visualization formats,
- output provenance traversal,
- capability subject execution,
- provenance subject execution,
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
| Output subject executes | Complete | `forge explain output generated/mcm-json/MCM/MyMenu.json` returns exit code 0 and deterministic output metadata. |
| JSON output classification emitted | Complete | `forge explain output generated/graph/project-source-graph.json --format json` returns classification metadata and false execution flags. |
| Unknown output paths rejected | Complete | Unknown generated/dist paths return usage exit code 2. |
| Remaining explain subjects stay reserved | Complete | `capability` and `provenance` still use reserved status metadata. |
| Runtime mutation avoided | Complete | No project files, generated manifests, provenance sidecars, artifacts, provider evidence, external tools, or AI calls are used. |

## Validation

Gate 246 requires build, targeted explain golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 247 should implement the next `forge explain` subject skeleton:
`forge explain capability <capability-id>`. It should explain existing
capability catalogue metadata deterministically and stop before provider
resolution, capability scan behavior changes, project file reads, generated
manifest reads, generated artifact existence checks, provenance sidecar reads,
build planning changes, generator execution, package execution, release
execution, graph visualization formats, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
