# Gate 397 - MO2 Settings and Discovery Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: Gates 395-396, ADR-008, ADR-012

## Goal

Implement separate persisted MO2 mods-root settings and bounded read-only
portable/global discovery with explicit user selection.

## Implemented

- Added backward-compatible `Mo2ModsRoot` to local app settings while keeping
  `Mo2Path` as the separate executable path.
- Added Settings browse, save, load, reset, readiness validation, path-pressure
  reporting, and Project Outputs initialization for the mods root.
- Invalid persisted roots remain visible in Settings but are not applied to
  Project Outputs.
- Added strict UTF-8/ASCII `ModOrganizer.ini` parsing for the Gate 396 subset:
  `[Settings]`, `base_directory`, `mod_directory`, defaults, and literal
  `%BASE_DIR%` expansion.
- Added portable discovery beside the explicitly configured executable and
  global direct-child discovery under `%LOCALAPPDATA%/ModOrganizer`.
- Candidate identity, de-duplication, and ordering use normalized INI paths.
- Added read-only `Discover MO2 Instances`, candidate list, and explicit
  `Use Selected` workflow in Project Outputs.
- Using or browsing a candidate updates the local settings draft but does not
  persist until the existing Save Settings action is invoked.
- Candidate selection invalidates the current export preview through the
  existing root-field change behavior.

## Refusals and boundaries

- Missing mods roots, reparse roots, Overwrite paths, game-Data-contained
  roots, malformed/duplicate supported keys, NUL/invalid UTF-8, and unknown
  percent tokens are invalid or unsupported.
- Discovery never creates directories, launches MO2, reads profiles or VFS,
  selects an instance automatically, enables mods, or changes priority/load
  order/plugins.
- The backend CLI remains explicit and does not read desktop settings.

## Verification

- Release solution build passed.
- Fixture-backed focused Windows tests passed portable/global ordering,
  defaults, `%BASE_DIR%`, unsupported token, missing root, Overwrite/Data,
  duplicate/malformed INI, old-settings compatibility, round-trip, and reset.
- All six test projects passed: 699 tests, 0 failed, 0 skipped.
- Refreshed standalone CLI and desktop app publishing passed.
- Published-app Windows UI Automation confirmed Settings `MO2 mods folder`,
  Save/Reset, Project Outputs discovery/selection, Preview, and Export controls;
  the main window remained responsive.

## Next route

Gate 398: rebuild the unsigned installer and run an isolated installed-app
regression covering settings persistence/reset, synthetic portable/global
discovery, explicit selection, export preview invalidation, and cleanup.
