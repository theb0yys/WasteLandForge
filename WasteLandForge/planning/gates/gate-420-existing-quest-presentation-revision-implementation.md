# Gate 420 - Existing Quest Presentation Revision Implementation

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 419, ADR-003, ADR-004, ADR-007, ADR-009, ADR-011

## Goal

Implement preview-gated revision of existing quest, stage, and objective
presentation while preserving identity, numbering, references, gameplay intent,
and GECK ownership boundaries.

## Implemented

- Added source-backed quest, quest-owned stage, and quest-owned objective choices.
- Added exact current-value population and ownership-aware refresh behavior.
- Added revision of only quest title/summary, stage title/summary, and objective
  text, including deliberate clearing of optional fields.
- Added required-field, wrong-owner, no-op, and stale-token refusals.
- Added complete quest proposal and before/after field preview.
- Added original-byte restoration after write or canonical validation failure.
- Added Narrative Author navigation, controls, validation, and GECK-handoff rebuild.
- Added focused Windows preservation and refusal tests.

## Published regression

- Published the self-contained `win-x64` desktop app.
- Revised one synthetic quest/stage/objective in writable isolated LocalAppData.
- Verified exact revised values in `quests.tsv`, `quest-stages.tsv`, and
  `quest-objectives.tsv`.
- Repeated preview refused the no-op, disabled apply, and preserved quest SHA-256.
- Verified application responsiveness and removed the isolated test directory.

## Validation

- Windows test build passed.
- Focused quest revision tests passed: 2 tests.
- Full solution passed: 718 tests, zero failures and zero skips.
- Publication and published regression passed.

## Boundaries

- No ID, stage number, variable, reference, transition, condition, result intent,
  external reference, tag, dialogue, asset, GECK mapping, plugin, external tool,
  game Data/MO2, network, release, or AI mutation.

## Next route

Gate 421: define source-backed authoring of one quest-local integer variable
with stable logical ID, initial integer value, optional title/summary, strict
duplicate/refusal behavior, validation, and GECK-handoff integration.
