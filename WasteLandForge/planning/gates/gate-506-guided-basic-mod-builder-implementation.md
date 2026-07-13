# Gate 506 - Guided Basic Mod Builder Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 505, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Goal

Implement one installed desktop workflow that creates a real validated MCM-only
or MCM+JIP Forge project and leaves a verified FOMOD distributable ready for
inspection and testing.

## Implemented

- Added a first-class **Basic Mod Builder** desktop workspace with project,
  MCM toggle, INI, and optional JIP startup inputs.
- Added mandatory write-free preview with deterministic IDs, paths, source
  inventory, expected payload, and a digest token binding all normalized input
  and destination state.
- Added an injectable orchestration service using the existing `forge init`,
  `forge validate`, `forge package --target mod-package`, and
  `forge package --target fomod` implementations.
- Creates source and outputs in a unique sibling transaction directory and
  promotes the complete project with one directory rename only after every
  stage succeeds.
- Added typed transformations for scaffold-owned MCM fields, optional removal
  of JIP source/capability requirements, optional inert JIP source replacement,
  and minimal immutable FOMOD metadata.
- Added stale-preview, existing-destination, validation/package failure,
  cancellation, cleanup, and post-build evidence handling.
- Final UI exposes the project path, FOMOD path, length, SHA-256, entry count,
  ordered stage states, and explicit open actions.

## Evidence classification

- **Documented:** Forge owns source scaffolding, validation, deterministic
  generation, packaging, FOMOD production, and workflow integration.
- **Documented:** existing init, MCM, JIP, combined-package, and FOMOD services
  were previously validated and are composed rather than reimplemented.
- **Inferred:** isolated sibling construction plus atomic directory promotion
  is the appropriate transaction boundary for a brand-new user project.
- **Open:** opaque user-entered JIP lines are structurally bounded but are not
  compiled or certified as valid game runtime behavior.

## Validation

- Focused Basic Mod Builder tests: 4 passed, covering both starter modes,
  stale preview, injected pipeline failure, cancellation, cleanup, validation,
  packaging, and FOMOD evidence.
- Full solution suite before the final cancellation-only test: 867 passed,
  zero failed, zero skipped. With that additional passing focused test the
  repository contains 868 passing tests.
- Release app/backend publication passed; only offline NuGet vulnerability-feed
  warnings (`NU1900`) were reported.
- Unsigned Inno Setup installer rebuilt successfully with Inno Setup 6.7.3.
- Installer size: 3,820,254 bytes.
- Installer SHA-256:
  `D54B5883F59698AF54FBB966E872AC7680D10FED18AB1D7D2C643360A3511D0E`.
- Installed UI Automation filled the Basic Mod Builder, previewed and created
  an MCM+JIP project, observed the ready state, checked all expected source and
  package files, and independently validated the promoted project with the
  installed backend.
- The complete installed regression, uninstall, and isolated cleanup passed.

## Boundaries preserved

- No ESP/ESM was created, parsed, or mutated.
- No game Data, MO2 profile, load order, tool installation, or existing project
  was changed.
- No GECK, xEdit, game, network, publication, signing, AI, or runtime probe was
  invoked by the builder.
- JIP text remains explicit opaque user source; Forge validates its contract,
  byte budget, package placement, and provenance only.

## Next route

Gate 507: close the Basic Mod Builder lane with project reopen/edit/rebuild
handoff verification and select the next major mod-authoring value slice,
without adding more starter fields or duplicating specialist authoring tools.
