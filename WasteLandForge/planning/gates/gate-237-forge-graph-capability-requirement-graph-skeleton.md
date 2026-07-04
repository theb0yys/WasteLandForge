# Gate 237 - forge graph Capability Requirement Graph Skeleton

Status: Complete

## Purpose

Extend the first `forge graph` output with declaration-only capability
requirement graph evidence. The command should link dependency registry
capability requirements to the built-in capability/provider catalogue without
running capability scans, resolving provider status, or changing build
planning.

## Research grounding

- Documented: ADR-008 says projects depend on capabilities, not provider
  names, and providers satisfy capabilities.
- Documented: R005 says projects declare required and optional capabilities
  and Forge resolves them against providers, while runtime probes enrich rather
  than define correctness.
- Documented: R006/ADR-010 include `forge graph` in the canonical
  offline-first CLI command surface.
- Documented: ADR-009 says generated artifacts are disposable and must carry
  provenance.
- Documented: ADR-011 requires deterministic fixture-backed tests and
  offline-first governance.

## Implemented

Gate 237 extends Gate 236 with:

- capability requirement nodes from validated dependency registry documents,
- built-in capability catalogue node,
- referenced built-in capability nodes,
- referenced built-in provider nodes,
- edges from dependency source documents to requirement nodes,
- edges from requirements to required capabilities,
- edges from catalogue capabilities to satisfying catalogue providers,
- graph summary counts for requirements, referenced capabilities, referenced
  providers, and catalogue size,
- graph manifest summary and catalogue metadata,
- explicit execution flags showing capability scan and provider resolution are
  not run,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 237 does not implement:

- graph visualization formats,
- `--subject` selection,
- capability scan behavior changes,
- runtime provider resolution,
- provider status claims,
- provider version parsing or comparison,
- build planning or execution changes,
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
| Requirement graph nodes | Complete | Dependency registry capability requirements are emitted as graph nodes. |
| Catalogue links | Complete | Referenced built-in capabilities and satisfying catalogue providers are linked. |
| No provider status claims | Complete | The graph does not run scans or assert provider readiness. |
| Manifest/checksum evidence updated | Complete | Graph manifest and checksums cover the expanded output. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 238 expanded `forge graph` with a generator target graph skeleton, Gate
239 expanded it with a generated artifact expectation graph skeleton, Gate 240
expanded it with a manifest provenance reference graph skeleton, and Gate 241
closed the graph command lane. The next implementation lane is Gate 242:
top-level `forge explain` subject contract and planning skeleton, still
stopping before generated manifest reads, build planning changes, generator
execution changes, generated artifact existence checks, runtime provider
resolution, capability scan behavior changes, graph visualization formats,
package/release behavior, xEdit process execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
or AI behavior.
