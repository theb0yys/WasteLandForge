# Gate 415 - Dialogue Branching Authoring Implementation

Status: Implementation complete; published interaction regression open
Phase: v0.1 desktop product value
Decision base: Gate 414, ADR-003, ADR-004, ADR-007, ADR-009

## Goal

Implement source-backed dialogue topic-link and response-route authoring through
the Narrative Author while preserving the Gate 414 evidence boundaries.

## Implemented

- Added deterministic source-backed line and endpoint-backed topic choices.
- Added `Link To`, `Link From`, and `Response Route` authoring modes.
- Added exact logical-ID derivation, opaque response-route keys, optional
  summaries, duplicate ID/key refusals, and missing-endpoint refusal.
- Added zero-write preview with manifest, quest, dialogue, selection, input, and
  proposal digest binding.
- Added dialogue-only transactional write with original-byte restoration after
  write or canonical validation failure.
- Added Narrative Author controls for loading, selecting, previewing, appending,
  validation, and GECK-handoff rebuild.
- Added focused Windows tests for all three modes, source preservation, exact
  handoff rows, zero-write preview, endpoint filtering, duplicate-key refusal,
  stale-token refusal, and repeated duplicate refusal.

## Validation

- Release build passed with no compiler errors. NuGet vulnerability-source
  warnings occurred because `api.nuget.org` was unavailable during restore.
- Focused dialogue branch tests passed: 4 tests.
- The first complete run had the known environment-sensitive
  `SOURCE_DATE_EPOCH` unit failure. Its isolated rerun passed.
- The subsequent complete run passed all 714 tests with zero failures/skips.
- Self-contained `win-x64` desktop publication passed.
- Published automation loaded choices, retained isolated project/input values,
  and enabled append after preview.

## Open published interaction check

The published automation provider did not dispatch the deeply scrolled branch
append button through InvokePattern, focused Enter/Space, or an off-viewport
coordinate click. No isolated source was changed by those attempts. One first
attempt used the app's synthetic LocalAppData demo before project focus was
committed; its exact test declaration was removed, the demo validated, and its
GECK handoff was rebuilt. All isolated regression data was removed.

Gate 415 therefore does not claim the published append interaction criterion.
Gate 416 owns that focused interaction/accessibility regression.

## Boundaries

- No route taxonomy, route evaluation, reciprocal-link invention, graph
  traversal, cycle policy, Speech Challenge routing, or GECK field mapping.
- No plugin creation/mutation, GECK/xEdit execution, game Data/MO2 write,
  network-dependent correctness, release publication, or AI requirement.

## Next route

Gate 416: make the dialogue branch controls reliably reachable in the published
desktop viewport, then complete an isolated published-app append, validation,
handoff, repeated-refusal, responsiveness, and cleanup regression.
