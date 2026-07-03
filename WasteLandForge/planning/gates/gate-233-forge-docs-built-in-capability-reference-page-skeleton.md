# Gate 233 - forge docs Built-In Capability Reference Page Skeleton

Status: Complete

## Purpose

Expand `forge docs` from validation rule reference pages into deterministic
built-in capability reference page evidence. The command should make the
built-in FNV capability catalogue inspectable as generated JSON and Markdown
without introducing provider reference pages, static site generation, watch
mode, network publishing, runtime probes, or higher-risk game/tool automation.

## Research grounding

- Documented: ADR-008 says projects depend on capabilities, not directly on
  provider names, and providers are versioned registry data that satisfy
  capabilities.
- Documented: R005 recommends a built-in FNV provider/capability catalogue
  covering the common New Vegas stack and keeping detection local-first and
  deterministic.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable,
  and outputs must carry provenance.
- Documented: ADR-010 defines `forge docs` as part of the stable offline-first
  command surface.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed tests, local build manifests, and offline-first governance.
- Documented: the generator/build research recommends deterministic text and
  metadata artifacts such as schema references, registry references,
  capability reports, validation rule pages, build manifests, JSON, and
  Markdown reports before higher-risk generated game artifacts.

## Implemented

Gate 233 implements:

- per-capability JSON reference skeletons under
  `generated/docs/capabilities/<capability-id>/capability-reference.json`,
- per-capability Markdown reference skeletons under
  `generated/docs/capabilities/<capability-id>/capability-reference.md`,
- capability reference metadata in `reference-index.json`,
- capability reference metadata in `docs-manifest.json`,
- capability reference output path lists in `forge docs --format json`,
- dry-run planning for capability reference pages without writes,
- checksum and output digest coverage for capability reference pages,
- capability reference summaries for catalogue identity, capability title,
  description, catalogue version, and provider IDs that satisfy the capability,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 233 does not implement:

- provider reference pages,
- static site generation,
- docs watch mode,
- network publishing,
- full prose capability documentation rendering,
- capability scan behavior,
- capability explain behavior,
- provider detection changes,
- provider-version parsing changes,
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
| Capability reference pages generated | Complete | One JSON and one Markdown page are written for each built-in FNV capability. |
| Reference index linked | Complete | `reference-index.json` and `reference-index.md` list capability reference pages. |
| Manifest/checksum evidence generated | Complete | `docs-manifest.json` and `checksums.sha256` include capability reference page outputs. |
| Dry-run supported | Complete | Dry-run reports planned capability reference outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 234 should expand `forge docs` from built-in capability reference pages
to built-in provider reference page skeletons. It should generate Markdown and
JSON reference pages under `generated/docs/providers/` from the built-in FNV
provider catalogue while still stopping before a static site generator, watch
mode, network publishing, full prose documentation rendering, capability scan
behavior, provider detection changes, provider-version parsing changes,
graph/explain/clean behavior, package/release behavior, xEdit process
execution, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
