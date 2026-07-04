# Gate 240 - forge graph Manifest Provenance Reference Graph Skeleton

Status: Complete

## Purpose

Extend `forge graph` with declaration-only manifest provenance reference graph
evidence. The command should show which manifest sidecar families are expected
to record provenance for known generated artifact families, without reading
generated manifests, checking file existence, executing targets, or changing
build planning.

## Research grounding

- Documented: ADR-009 says generated artifacts are disposable, rebuildable,
  and must carry provenance.
- Documented: R004 says generated artifacts should prove their source through
  local build manifests, output digests, and source metadata.
- Documented: The build/CLI/release skill identifies build manifests, package
  manifests, reports, and deterministic metadata artifacts as v0.1 generator
  scope.
- Documented: ADR-011 requires deterministic fixture-backed tests and
  offline-first governance.

## Implemented

Gate 240 extends Gate 239 with:

- manifest provenance reference nodes for known manifest sidecar families,
- generator target to manifest reference edges,
- manifest reference to generated/dist boundary edges,
- manifest reference to covered artifact expectation edges,
- graph summary counts for manifest references and reference edges,
- graph JSON, Markdown, CLI JSON/text, and manifest evidence for the new graph
  layer,
- explicit execution flag showing generated manifests are not read,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 240 does not implement:

- generated manifest reads,
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
| Manifest reference nodes | Complete | Known manifest sidecar families are emitted as `manifest-provenance-reference` graph nodes. |
| Target-to-manifest links | Complete | Generator targets declare known manifest provenance sidecars. |
| Manifest-to-artifact links | Complete | Manifest references link to covered artifact expectation families. |
| No generated manifest read | Complete | Graph output records `manifestRead: false`. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 241 closes the current `forge graph` command metadata lane and
transitions to `forge explain` planning. Gate 242 starts top-level
`forge explain` subject contract planning as help and reserved JSON metadata,
and Gate 243 implements the diagnostic subject skeleton. Gate 244 should move
to rule-specific diagnostic explanation metadata while keeping the existing
declaration-only graph boundaries and stopping before generated manifest
reads, generated artifact existence checks, build planning changes, generator
execution changes, runtime provider resolution, capability scan behavior
changes, graph visualization formats, package/release behavior, xEdit process
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
