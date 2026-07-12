# Gate 418 - Existing Dialogue Line Revision Implementation

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 417, ADR-003, ADR-004, ADR-007, ADR-009, ADR-011

## Goal

Implement preview-gated revision of an existing dialogue line's presentation
fields while preserving all identity, references, gameplay declarations, voice,
branches, and GECK ownership boundaries.

## Implemented

- Added deterministic source-backed dialogue-line loading with exact current
  response, speaker, prompt, and priority values.
- Added replacement and deliberate clearing for only the four contracted fields.
- Added blank response, prompt-without-priority, invalid priority, no-op, and
  stale-token refusals.
- Added complete proposed-source and before/after field preview.
- Added original-byte restoration after write or canonical validation failure.
- Added Narrative Author workflow navigation, field population, preview, apply,
  validation, and GECK-handoff rebuild.
- Added focused Windows tests for field isolation, exact handoff output,
  optional-field clearing, invalid inputs, no-op, and stale source.
- Disabled xUnit parallelization for UnitTests because several classes mutate
  process-wide `SOURCE_DATE_EPOCH`; this follows the existing GoldenTests
  assembly pattern and removes the observed environment race.

## Published regression

- Published the self-contained `win-x64` desktop app.
- Revised one synthetic line in an isolated writable LocalAppData project.
- Verified exact response, speaker, prompt, and priority values in canonical
  source and `worklists/dialogue-lines.tsv`.
- Repeated preview refused the no-op, disabled apply, and preserved dialogue
  SHA-256.
- Verified application responsiveness and removed the isolated test directory.

## Validation

- Desktop and Windows-test builds passed.
- Focused revision tests passed: 2 tests.
- Publication passed.
- Initial full runs exposed the existing parallel environment-variable race.
- After applying deterministic UnitTests isolation, all 716 tests passed with
  zero failures and zero skips.

## Boundaries

- No identity, quest/topic reference, condition, result intent, branch, voice,
  tag, topic, schema, GECK mapping, plugin, external tool, game Data/MO2,
  network, release, or AI mutation.

## Next route

Gate 419: define preview-gated revision of existing quest title/summary, stage
title/summary, and objective text while preserving quest identity, stage
numbers, references, transitions, conditions, result intent, and GECK handoff
safety boundaries.
