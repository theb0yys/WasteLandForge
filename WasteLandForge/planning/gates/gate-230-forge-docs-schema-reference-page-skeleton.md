# Gate 230 - forge docs Schema Reference Page Skeleton

Status: Complete

## Purpose

Expand `forge docs` from a single aggregate reference index into deterministic
per-schema reference page evidence. The command should make the embedded schema
catalog inspectable as local generated JSON and Markdown without introducing a
static site generator, watch mode, network publishing, or higher-risk game/tool
automation.

## Research grounding

- Documented: ADR-009 says generated artifacts are disposable and rebuildable,
  and outputs must carry provenance.
- Documented: ADR-010 defines `forge docs` as part of the stable offline-first
  command surface.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed tests, local build manifests, and offline-first governance.
- Documented: R006 defines `forge docs` as an offline command that generates
  documentation from project source, schemas, registries, and reference data.
- Documented: the generator/build research recommends deterministic text and
  metadata artifacts such as schema references, registry references,
  capability reports, validation rule pages, build manifests, JSON, and
  Markdown reports before higher-risk generated game artifacts.
- Documented: the platform architecture research says JSON Schema annotations
  support generated documentation and editor-facing reference material.

## Implemented

Gate 230 implements:

- per-schema JSON reference skeletons under
  `generated/docs/schemas/<kind>/<version>/schema-reference.json`,
- per-schema Markdown reference skeletons under
  `generated/docs/schemas/<kind>/<version>/schema-reference.md`,
- schema reference metadata in `reference-index.json`,
- schema reference metadata in `docs-manifest.json`,
- schema reference output path lists in `forge docs --format json`,
- dry-run planning for schema reference pages without writes,
- checksum and output digest coverage for schema reference pages,
- schema reference summaries for embedded schema identity, source path,
  embedded schema digest, schema type, required properties, and top-level
  properties,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 230 does not implement:

- static site generation,
- docs watch mode,
- network publishing,
- full prose schema documentation rendering,
- project registry reference pages,
- graph command behavior,
- explain command behavior,
- clean command behavior,
- package/release behavior,
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
| Schema reference pages generated | Complete | One JSON and one Markdown page are written for each embedded schema resource. |
| Reference index linked | Complete | `reference-index.json` and `reference-index.md` list schema reference pages. |
| Manifest/checksum evidence generated | Complete | `docs-manifest.json` and `checksums.sha256` include schema reference page outputs. |
| Dry-run supported | Complete | Dry-run reports planned schema reference outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 231 should expand `forge docs` from schema reference pages to project
registry reference page skeletons. It should generate per-registry Markdown and
JSON reference pages under `generated/docs/registries/` from local source
registry documents while still stopping before a static site generator, watch
mode, network publishing, full prose documentation rendering,
graph/explain/clean behavior, package/release behavior, xEdit process
execution, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
