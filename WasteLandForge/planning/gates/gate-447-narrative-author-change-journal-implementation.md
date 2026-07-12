# Gate 447 - Narrative Author Change Journal Implementation

Status: Complete
Phase: v0.1 desktop authoring safety
Decision base: Gate 446, ADR-007, ADR-009, ADR-010, ADR-011

## Implemented

- Added a private one-entry journal under app-owned LocalAppData, keyed by the
  normalized project path digest.
- Added exact before/after byte snapshots, SHA-256/length metadata, fixed
  canonical path allowlisting, project-root containment, and format/version
  checks.
- Added staged journal commits that preserve the previous entry until the new
  directory move succeeds and remove incomplete staged directories at startup.
- Added current-after hash verification, validation-gated review tokens,
  transactional undo, post-state rollback on undo failure, created-file
  removal, entry consumption, and no redo.
- Added fixed Last Change, Review Undo, and Undo Last Change controls using the
  existing output pane and inventory/explorer stale state.
- Integrated all 15 Narrative Author source transaction workflows through the
  shared journal boundary. Dialogue revision retains its preview-specific
  preparation so review details include the exact field changes.

## Validation

- Focused journal/inventory/workspace tests passed: 9 tests.
- Tests cover exact one-file and two-file restoration, real `forge init`
  created quest/dialogue file removal, entry consumption, and intervening-edit
  refusal without source mutation.
- Full solution passed: 757 tests, zero failures and zero skips.
- Release app-shell publication and bundled-backend verification passed.
- Published regression completed dialogue revision, committed the entry,
  reopened the app, reviewed the persisted entry, restored the exact fixture
  SHA-256, and consumed the journal.
- Final published build kept journal, undo, inventory, and explorer controls
  visible at 1366x768 and 1920x1080.
- Isolated project and journal entries were removed after regression.
- NuGet vulnerability metadata lookup emitted NU1900 warnings because
  `api.nuget.org` was unavailable; builds and tests completed.

## Boundaries

- No canonical schema, validator, generator, CLI/backend, package, handoff,
  GECK automation, external tool, plugin, game Data/MO2, network, release, or AI
  contract changed.
- Journal data remains private local app state and is never package input.

## Next Route

Gate 448: rebuild the unsigned installer and verify installed authoring journal
commit, restart review, exact-byte undo, refusal after intervening edits,
uninstall, and isolated cleanup.
