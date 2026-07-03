# Gate 220 - xEdit Audit Scaffold Manifest and Checksums

Status: Complete

## Purpose

Write local manifest and checksum evidence for generated xEdit audit script
scaffolds under `generated/xedit-audit`.

This gate makes the Gate 219 scaffold output traceable without adding a CLI
target, xEdit process execution, xEdit report parsing, plugin patch
generation, plugin mutation, MO2 automation, GECK automation, runtime probes,
or real third-party plugin fixtures.

## Research grounding

- Documented: ADR-009 requires generated artifacts to be disposable,
  rebuildable, and traceable.
- Documented: ADR-011 requires local build manifests and deterministic
  fixture-backed testing.
- Documented: R006 says xEdit support should stay narrow: audit scripts,
  inspection scripts, scaffolds, and report parsers, not silent high-risk
  patch authoring.
- Inferred: the first xEdit scaffold sidecar should follow the existing local
  generated-evidence pattern from JIP script emission: payload files,
  deterministic manifest, and checksums over payload plus manifest.

## Implemented

Gate 220 implements:

- `xedit-audit-script-manifest.json` under `generated/xedit-audit`,
- `checksums.sha256` under `generated/xedit-audit`,
- result paths for manifest and checksum sidecars,
- manifest evidence for scaffold IDs, expected reports, safety flags, source
  pointers, target plugins, record types, required capabilities, and output
  digests,
- checksum evidence for generated scaffold files and the scaffold manifest,
- tests proving sidecar creation and continued no-report/no-Data behavior.

## Not implemented

Gate 220 does not implement:

- `forge generate --target xedit-audit`,
- xEdit process execution,
- xEdit report parsing,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Scaffold manifest is written under `generated/xedit-audit` | Complete | Uses `xedit-audit-script-manifest.json`. |
| Checksums are written under `generated/xedit-audit` | Complete | Uses paths relative to the xEdit audit generated root. |
| Manifest records safety limits | Complete | Records no xEdit execution, report parsing, patch generation, plugin mutation, Data writes, MO2/GECK automation, or runtime probes. |
| Report paths remain metadata | Complete | No report file is created. |
| Runtime/plugin mutation avoided | Complete | No `Data`, plugin, MO2, GECK, or runtime path is touched. |
| Validation errors stop output | Complete | Broken fixture still produces no generated directory. |

## Validation

Required validation:

- unit tests for manifest and checksum sidecars,
- .NET build/test smoke,
- protected-file scan.

## Next Gate

Gate 221 should wire canonical `forge generate --target xedit-audit` to the
local scaffold emitter. It should stop before xEdit process execution, xEdit
report parsing, plugin patch generation, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, or package/release behavior.
