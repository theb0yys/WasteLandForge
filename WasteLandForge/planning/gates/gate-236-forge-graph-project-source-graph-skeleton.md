# Gate 236 - forge graph Project Source Graph Skeleton

Status: Complete

## Purpose

Introduce the first real `forge graph` command slice as deterministic project
source graph evidence. The command should link the project, known source
contract documents, and generated/dist output boundaries without introducing
graph visualization formats, build planning changes, package/release behavior,
or higher-risk game/tool automation.

## Research grounding

- Documented: ADR-010 includes `forge graph` in the canonical offline-first CLI
  command surface.
- Documented: R006 describes `forge graph` as a project-root graph command
  while keeping canonical command names stable.
- Documented: ADR-007 says canonical truth lives in source YAML/JSON registry
  documents normalized to a canonical graph.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable,
  and outputs must carry provenance.
- Documented: ADR-011 requires deterministic fixture-backed tests, local build
  manifests, and offline-first governance.

## Implemented

Gate 236 implements:

- `forge graph [project-root]`,
- `forge graph --project <path>`,
- `forge graph --output generated/<name>`,
- `forge graph --dry-run`,
- `forge graph --format human|plain|json`,
- generated graph JSON under
  `generated/graph/project-source-graph.json`,
- generated graph Markdown under
  `generated/graph/project-source-graph.md`,
- generated graph manifest under `generated/graph/graph-manifest.json`,
- generated checksum sidecar under `generated/graph/checksums.sha256`,
- graph nodes for project, source root, source manifest/registry documents,
  and generated/dist output boundaries,
- graph edges for source containment and generated-output boundary flow,
- source and output digest reporting,
- fixture-backed CLI tests for write mode, dry-run mode, and output
  containment.

## Not implemented

Gate 236 does not implement:

- graph visualization formats,
- `--subject` selection,
- capability requirement graphing,
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
| `forge graph` command implemented | Complete | The command writes project source graph evidence for valid projects. |
| Generated output contained | Complete | Output is accepted only under project `generated/`; outside paths fail with `WF-GEN-001`. |
| Manifest/checksum evidence generated | Complete | `graph-manifest.json` and `checksums.sha256` cover graph outputs. |
| Dry-run supported | Complete | Dry-run reports planned graph outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 237 should expand `forge graph` with a capability requirement graph
skeleton. It should link manifest-declared capability requirements to the
built-in capability/provider catalogue while still stopping before runtime
provider resolution, capability scan behavior changes, graph visualization
formats, build planning changes, package/release behavior, xEdit process
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
