# Gate 393 - Named MO2 Mod Export Vertical Slice

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 392, ADR-004, ADR-009, ADR-010, ADR-011, ADR-012

## Goal

Implement the explicit named-MO2 export backend and Project Outputs workflow
without mutating MO2 profiles, game Data, Overwrite, plugins, or load order.

## Implemented

- Added paired `--mo2-mods-root` and `--mo2-mod-name` options to canonical
  `forge package --target mod-package`.
- Freshly rebuilds the combined package before export and emits an exact
  three-entry dry-run preview for the synthetic combined fixture.
- Refuses missing roots, unsafe Windows names, Overwrite paths, reparse roots,
  existing destinations, and non-direct-child destinations before writing.
- Copies Data contents directly into a unique temporary sibling, verifies each
  SHA-256 digest, then atomically promotes one new named mod directory.
- Rolls back the Forge-owned temporary or promoted destination on failure.
- Added immutable `mo2-export-manifest/0.1.0` evidence with package evidence,
  per-file source/destination digests, counts, and disabled mutation flags.
- Added structured human/JSON export results and CLI help.
- Added a Project Outputs MO2 panel with explicit root browse, editable mod
  name, backend dry-run preview, current-preview gating, export, and exact
  exported-folder handoff.
- The WPF shell invokes `forge.exe` for preview and export and contains no copy
  implementation.

## Diagnostics

- `WF-BUILD-011`: source package or staged evidence invalid.
- `WF-BUILD-012`: unsafe mods root or mod name.
- `WF-BUILD-013`: destination already exists.
- `WF-BUILD-014`: copy, verification, promotion, evidence, or rollback failure.

## Boundaries

- Export means copied loose files only, not installed, enabled, active, or
  verified in game.
- No MO2 discovery, profile activation, priority, load order, plugin mutation,
  `meta.ini`, game launch, external-tool execution, network, or AI.
- Absolute export paths are local evidence and require review before sharing.

## Next route

Gate 394: rebuild the unsigned installer and run an isolated installed-app
regression covering MO2 preview, export, evidence, exact folder handoff,
existing-destination refusal, uninstall, and cleanup.
