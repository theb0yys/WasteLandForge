# Gate 238 - forge graph Generator Target Graph Skeleton

Status: Complete

## Purpose

Extend `forge graph` with declaration-only generator target graph evidence.
The command should show which known generator targets are represented in the
current source tree and which generated/dist output boundaries they write to,
without executing targets or changing build planning.

## Research grounding

- Documented: ADR-009 says outputs are generated through a deterministic,
  capability-aware build graph and generated artifacts are disposable with
  provenance.
- Documented: R006/ADR-010 include `forge graph`, `forge generate`, and
  `forge build` in the canonical offline-first CLI command surface.
- Documented: R006 identifies graph inspection as a developer-facing command
  for understanding project structure.
- Documented: ADR-011 requires deterministic fixture-backed tests and
  offline-first governance.

## Implemented

Gate 238 extends Gate 237 with:

- generator target nodes for currently implemented targets,
- project-to-target declaration edges,
- source registry document to generator target edges,
- generated/dist output boundary edges from generator targets,
- generated evidence handoff edge for the xEdit audit report handoff target,
- graph summary counts for generator targets and target input/output edges,
- graph JSON, Markdown, and manifest evidence for the new graph layer,
- explicit execution flag showing generator targets are not executed,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 238 does not implement:

- generator execution from `forge graph`,
- build planning or execution changes,
- graph visualization formats,
- `--subject` selection,
- capability scan behavior changes,
- runtime provider resolution,
- provider status claims,
- provider version parsing or comparison,
- command aliases,
- static site generation,
- watch mode,
- network publishing,
- package/release behavior changes,
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
| Generator target nodes | Complete | Implemented targets are emitted as `generator-target` graph nodes. |
| Source-to-target links | Complete | Relevant source registry documents feed target nodes through declaration-only edges. |
| Output boundary links | Complete | Generator targets link to generated/dist boundaries without writing target outputs. |
| No target execution | Complete | Graph output records `generatorExecution: false`. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 239 expanded `forge graph` with a generated artifact expectation graph
skeleton, Gate 240 expanded it with a manifest provenance reference graph
skeleton, Gate 241 closed the graph command lane, and Gate 242 started the
top-level `forge explain` subject contract as help and reserved JSON metadata.
Gate 243 implements `forge explain diagnostic <rule-id>` as a family-level
subject skeleton. The next implementation lane is Gate 244: rule-specific
diagnostic explanation metadata, still stopping before generated manifest
reads, build planning changes, generator execution changes, generated artifact
existence checks, runtime provider resolution, capability scan behavior
changes, graph visualization formats, package/release behavior, xEdit process
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
