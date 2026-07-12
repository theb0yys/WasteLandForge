# Gate 436 - Quest GECK Binding Revision Implementation

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 435, ADR-004, ADR-007, ADR-009, ADR-011

## Goal

Implement exact-field revision of one existing quest GECK binding while
preserving FormID, provider, external-reference ordering, and unrelated source.

## Implemented

- Added deterministic selection of quests with exactly one GECK binding.
- Added source-populated plugin, EditorID, and read-only preserved FormID.
- Added plugin-only, EditorID-only, and both-field revision with no-op,
  ambiguity, invalid-input, and stale-token refusals.
- Added rollback validation, GECK-handoff rebuild, Narrative Author controls,
  and exact xEdit/FormID preservation tests.

## Published regression

- Published the WPF app against the verified bundled backend.
- Revised plugin and EditorID through the published executable in isolated
  writable LocalAppData.
- Verified exact revised values in `quests.tsv`.
- Repeated preview refused the no-op, preserved quest SHA-256, and the app
  remained responsive.
- Removed the isolated test directory.

## Validation

- Windows build passed.
- Focused revision tests passed: 4 tests.
- Full solution passed: 748 tests, zero failures and zero skips.
- Desktop publication and published GUI regression passed.
- NuGet audit lookup produced NU1900 warnings because `api.nuget.org` was
  unavailable; compilation and tests completed.

## Boundaries

- No provider/FormID change, binding add/delete/reorder, plugin lookup/read/write,
  record verification, GECK/xEdit launch, game Data/MO2 write, network
  correctness dependency, release publication, or AI requirement.

## Next route

Gate 437: define Narrative Author workspace navigation and status consolidation
so the implemented quest, dialogue, voice, and GECK-binding workflows are
discoverable without one long control stack, with no source-contract or backend
behavior changes.
