# Gate 234 - forge docs Built-In Provider Reference Page Skeleton

Status: Complete

## Purpose

Expand `forge docs` from built-in capability reference pages into deterministic
built-in provider reference page evidence. The command should make the built-in
FNV provider catalogue inspectable as generated JSON and Markdown without
introducing provider detection changes, provider-version parsing changes,
static site generation, watch mode, network publishing, runtime probes, or
higher-risk game/tool automation.

## Research grounding

- Documented: ADR-008 says projects depend on capabilities, not provider names,
  and providers are versioned registry data that satisfy capabilities.
- Documented: R005 recommends a built-in FNV capability/provider catalogue
  covering the common New Vegas stack while keeping detection local-first and
  deterministic.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable,
  and outputs must carry provenance.
- Documented: ADR-010 defines `forge docs` as part of the stable offline-first
  command surface.
- Documented: ADR-011 requires deterministic fixture-backed tests, local build
  manifests, and offline-first governance.
- Documented: the generator/build research recommends deterministic text and
  metadata artifacts before higher-risk generated game artifacts.

## Implemented

Gate 234 implements:

- per-provider JSON reference skeletons under
  `generated/docs/providers/<provider-id>/provider-reference.json`,
- per-provider Markdown reference skeletons under
  `generated/docs/providers/<provider-id>/provider-reference.md`,
- provider reference metadata in `reference-index.json`,
- provider reference metadata in `docs-manifest.json`,
- provider reference output path lists in `forge docs --format json`,
- dry-run planning for provider reference pages without writes,
- checksum and output digest coverage for provider reference pages,
- provider reference summaries for catalogue identity, provider ID, title,
  provider type, install scope, capabilities, detector kinds, notes, and
  declaration-only version metadata,
- fixture-backed CLI tests for write mode and dry-run mode.

## Not implemented

Gate 234 does not implement:

- static site generation,
- docs watch mode,
- network publishing,
- full prose provider documentation rendering,
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
| Provider reference pages generated | Complete | One JSON and one Markdown page are written for each built-in FNV provider. |
| Reference index linked | Complete | `reference-index.json` and `reference-index.md` list provider reference pages. |
| Manifest/checksum evidence generated | Complete | `docs-manifest.json` and `checksums.sha256` include provider reference page outputs. |
| Dry-run supported | Complete | Dry-run reports planned provider reference outputs and writes no files. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, xEdit process, plugin file, runtime probe, network, or AI path is used. |

## Validation

Required validation:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- whitespace/diff checks
- protected-file scan

## Next Gate

Gate 235 should expand `forge docs` from built-in provider reference pages to
canonical command reference page skeletons. It should generate Markdown and
JSON reference pages under `generated/docs/commands/` from the ADR-010 command
surface while still stopping before a static site generator, watch mode,
network publishing, full prose documentation rendering, graph/explain/clean
behavior, package/release behavior, xEdit process execution, plugin patch
generation, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
