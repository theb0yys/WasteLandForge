# Gate 231 - forge docs Registry Reference Page Skeleton

Status: Complete

## Purpose

Expand `forge docs` from embedded schema reference pages into deterministic
project registry reference page evidence. The command should make local source
registry documents inspectable as generated JSON and Markdown without
introducing a static site generator, watch mode, network publishing, or
higher-risk game/tool automation.

## Research grounding

- Documented: ADR-007 says canonical source truth is versioned YAML/JSON
  registry documents normalized to canonical JSON and validated by deterministic
  validators.
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

## Implemented

Gate 231 implements:

- per-registry JSON reference skeletons under
  `generated/docs/registries/<registry-path>/registry-reference.json`,
- per-registry Markdown reference skeletons under
  `generated/docs/registries/<registry-path>/registry-reference.md`,
- registry reference metadata in `reference-index.json`,
- registry reference metadata in `docs-manifest.json`,
- registry reference output path lists in `forge docs --format json`,
- dry-run planning for registry reference pages without writes,
- checksum and output digest coverage for registry reference pages,
- registry reference summaries for registry identity, source path, group,
  source format, source SHA-256, source length, JSON parse status, top-level
  JSON properties, and array item count when applicable,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 231 does not implement:

- static site generation,
- docs watch mode,
- network publishing,
- full prose registry documentation rendering,
- YAML content rendering beyond metadata-only source evidence,
- validation rule reference pages,
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
| Registry reference pages generated | Complete | One JSON and one Markdown page are written for each local registry source document. |
| Reference index linked | Complete | `reference-index.json` and `reference-index.md` list registry reference pages. |
| Manifest/checksum evidence generated | Complete | `docs-manifest.json` and `checksums.sha256` include registry reference page outputs. |
| Dry-run supported | Complete | Dry-run reports planned registry reference outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 232 should expand `forge docs` from schema and registry reference pages to
validation rule reference page skeletons. It should generate Markdown and JSON
reference pages under `generated/docs/rules/` for reserved rule families and
known local diagnostics while still stopping before a static site generator,
watch mode, network publishing, full prose documentation rendering,
graph/explain/clean behavior, package/release behavior, xEdit process
execution, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
