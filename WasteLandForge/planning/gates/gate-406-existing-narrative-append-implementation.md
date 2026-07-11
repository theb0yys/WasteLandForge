# Gate 406 - Existing Narrative Append Implementation

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 405, ADR-007, ADR-009

## Goal

Implement preview-gated extension of an existing validated quest/dialogue
registry and rebuild the GECK authoring handoff without replacing unrelated
canonical content.

## Implemented

- Added validated existing-source loading with source-backed quest, stage, and
  topic choices and exact JSON/schema/path shape refusals.
- Added full proposed quest/dialogue preview and a token binding manifest,
  source bytes, selections, inputs, and proposed bytes.
- Added append-only stage, objective, transition, optional new topic, and
  dialogue-line generation with duplicate identity/number and prompt refusals.
- Added guarded two-file writes with original-byte restoration on write or
  canonical post-write validation failure.
- Added Narrative Author controls for existing-topic and new-topic workflows,
  source-backed selectors, preview, append, CLI validation presentation, and
  GECK-handoff rebuild.
- Added deterministic Windows tests for load, zero-write preview, preservation,
  new/existing topic modes, stale token, duplicate number/identity, prompt
  refusal, canonical validation, and package generation.

## Validation

- All six test suites passed on the clean rerun: 706 tests, zero failures and
  zero skips.
- Release app publication passed.
- Published-app automation loaded synthetic ExampleMod, appended one stage,
  objective, transition, and line using an existing topic, and rebuilt the
  handoff.
- Result contained three stages, three dialogue lines, and a handoff reporting
  three lines and 23 unresolved actions.
- Repeated duplicate preview kept append disabled and preserved both source
  SHA-256 values.
- App remained responsive; isolated LocalAppData project and process were removed.

## Test-run note

The first broad run reported one unit failure without retaining failure detail;
an immediate isolated unit rerun passed all 117 tests, and the subsequent clean
six-project no-build run passed all 706 tests. No source or fixture cleanup was
needed between those two runs.

## Boundaries

- No broad JSON editor, existing-node replacement, conditions, branches,
  variables, scripts, voice, GECK/xEdit execution, plugin mutation, game Data
  or MO2 write, network, release publication, or AI.

## Next route

Gate 407: refresh the unsigned installer and run an isolated installed-app
existing-narrative append regression covering selectors, preview, append,
handoff, duplicate refusal, uninstall, and cleanup.
