# Gate 235 - forge docs Canonical Command Reference Page Skeleton

Status: Complete

## Purpose

Expand `forge docs` from built-in provider reference pages into deterministic
canonical command reference page evidence. The command should make the ADR-010
command surface inspectable as generated JSON and Markdown without changing
CLI behavior, adding aliases, introducing a static site generator, watch mode,
network publishing, or higher-risk game/tool automation.

## Research grounding

- Documented: ADR-010 defines a small, stable, offline-first, AI-optional
  hybrid verb-and-namespace CLI.
- Documented: R006 defines the canonical command surface including `forge
  init`, `forge validate`, `forge capabilities list|scan|explain`, `forge
  generate`, `forge build`, `forge package`, `forge release
  verify|prepare|publish`, `forge docs`, `forge graph`, `forge explain`,
  `forge clean`, `forge doctor export`, `forge help`, and `forge --version`.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable,
  and outputs must carry provenance.
- Documented: ADR-011 requires deterministic fixture-backed tests, local build
  manifests, and offline-first governance.
- Documented: the generator/build research recommends deterministic text and
  metadata artifacts before higher-risk generated game artifacts.

## Implemented

Gate 235 implements:

- per-command JSON reference skeletons under
  `generated/docs/commands/<command-path>/command-reference.json`,
- per-command Markdown reference skeletons under
  `generated/docs/commands/<command-path>/command-reference.md`,
- command reference metadata in `reference-index.json`,
- command reference metadata in `docs-manifest.json`,
- command reference output path lists in `forge docs --format json`,
- dry-run planning for command reference pages without writes,
- checksum and output digest coverage for command reference pages,
- command reference summaries for command ID, command text, command group,
  surface status, source document, and research source,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 235 does not implement:

- command behavior changes,
- command aliases,
- new command names,
- static site generation,
- docs watch mode,
- network publishing,
- full prose command documentation rendering,
- graph command behavior,
- explain command behavior,
- clean command behavior,
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
| Command reference pages generated | Complete | One JSON and one Markdown page are written for each ADR-010 canonical command entry. |
| Reference index linked | Complete | `reference-index.json` and `reference-index.md` list command reference pages. |
| Manifest/checksum evidence generated | Complete | `docs-manifest.json` and `checksums.sha256` include command reference page outputs. |
| Dry-run supported | Complete | Dry-run reports planned command reference outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 236 should introduce a minimal `forge graph` project source graph
skeleton. It should emit deterministic JSON/Markdown graph evidence for known
source documents and generated-output boundaries while still stopping before
graph visualization, static site generation, watch mode, network publishing,
package/release behavior, xEdit process execution, plugin patch generation,
plugin mutation, MO2 automation, GECK automation, runtime probes, real
third-party plugin fixtures, or AI behavior.
