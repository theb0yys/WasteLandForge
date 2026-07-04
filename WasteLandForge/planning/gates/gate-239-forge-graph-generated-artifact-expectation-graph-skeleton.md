# Gate 239 - forge graph Generated Artifact Expectation Graph Skeleton

Status: Complete

## Purpose

Extend `forge graph` with declaration-only generated artifact expectation
graph evidence. The command should show which output artifact families are
expected from known generator targets and which generated/dist boundary owns
those expectations, without executing targets, checking file existence, or
changing build planning.

## Research grounding

- Documented: ADR-009 says outputs are generated through a deterministic,
  capability-aware build graph and generated artifacts are disposable with
  provenance.
- Documented: R006/ADR-010 include `forge graph`, `forge generate`, and
  `forge build` in the canonical offline-first CLI command surface.
- Documented: The build/CLI/release skill identifies deterministic text and
  metadata artifacts, manifests, package manifests, and reports as v0.1
  generator scope.
- Documented: ADR-011 requires deterministic fixture-backed tests and
  offline-first governance.

## Implemented

Gate 239 extends Gate 238 with:

- generated artifact expectation nodes for known generator target output
  families,
- generator target to artifact expectation edges,
- artifact expectation to generated/dist boundary edges,
- graph summary counts for artifact expectations and expectation edges,
- graph JSON, Markdown, CLI JSON/text, and manifest evidence for the new graph
  layer,
- explicit execution flag showing generated artifact existence is not checked,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 239 does not implement:

- generated artifact existence checks,
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
| Artifact expectation nodes | Complete | Known output families are emitted as `generated-artifact-expectation` graph nodes. |
| Target-to-expectation links | Complete | Generator targets declare expected artifact family nodes. |
| Boundary links | Complete | Artifact expectations link to generated or dist boundaries without checking files. |
| No artifact existence check | Complete | Graph output records `artifactExistenceCheck: false`. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 240 expanded `forge graph` with a manifest provenance reference graph
skeleton, Gate 241 closed the graph command lane, and Gate 242 started the
top-level `forge explain` subject contract as help and reserved JSON metadata.
Gate 243 implements `forge explain diagnostic <rule-id>` as a family-level
subject skeleton. The next implementation lane is Gate 244: rule-specific
diagnostic explanation metadata, still stopping before generated manifest
reads, generated artifact existence checks, build planning changes, generator
execution changes, runtime provider resolution, capability scan behavior
changes, graph visualization formats, package/release behavior, xEdit process
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
