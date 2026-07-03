# Gate 229 - forge docs Reference Index Skeleton

Status: Complete

## Purpose

Start the `forge docs` lane with deterministic local documentation index
evidence. The command should make Forge useful for inspecting source truth and
reference data without introducing a static site generator, watch mode,
network publishing, or higher-risk game/tool automation.

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

## Implemented

Gate 229 implements:

- canonical top-level `forge docs`,
- `--project <path>` and positional project-root parsing,
- `--output generated/<name>` as a generated output directory,
- `--format human|plain|json`,
- `--dry-run`,
- validation-first no-write behavior on blocking diagnostics,
- generated output containment under `generated/`,
- `generated/docs/reference-index.json`,
- `generated/docs/reference-index.md`,
- `generated/docs/docs-manifest.json`,
- `generated/docs/checksums.sha256`,
- index sections for embedded schemas, project registry files, reserved rule
  families, built-in FNV capabilities, built-in FNV providers, and canonical
  commands,
- deterministic source and output digest reporting,
- fixture-backed CLI tests for write mode, dry-run mode, and output
  containment diagnostics.

## Not implemented

Gate 229 does not implement:

- static site generation,
- docs watch mode,
- network publishing,
- full schema reference page rendering,
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
| `forge docs` command wired | Complete | `docs` is no longer a reserved top-level command. |
| Reference index generated | Complete | JSON and Markdown index files are written under `generated/docs`. |
| Manifest/checksum evidence generated | Complete | `docs-manifest.json` and `checksums.sha256` are written beside the index. |
| Output containment enforced | Complete | `WF-GEN-001` blocks docs output outside `generated/`. |
| Dry-run supported | Complete | Dry-run reports planned outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 230 should expand `forge docs` from an aggregate reference index to a
schema reference page skeleton. It should generate per-schema Markdown and JSON
reference pages under `generated/docs/schemas/` from the embedded schema
catalog while still stopping before a static site generator, watch mode,
network publishing, full prose documentation rendering, graph/explain/clean
behavior, package/release behavior, xEdit process execution, plugin patch
generation, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
