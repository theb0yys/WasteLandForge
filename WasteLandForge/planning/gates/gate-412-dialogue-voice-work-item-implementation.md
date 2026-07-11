# Gate 412 - Dialogue Voice Work-Item Implementation

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 411, ADR-004, ADR-007, ADR-009

## Goal

Implement source-backed voice work-item authoring with a complete validated
WAV/OGG/LIP mapping and rebuild the GECK handoff without recording, converting,
exporting, calculating lip data, or launching GECK.

## Implemented

- Added unvoiced-line and complete declared-trio discovery.
- Added bind-existing-trio and declare-project-files modes.
- Added project-contained source path, extension, existence, distinctness,
  plugin/type/stem, duplicate ID/target/source, and canonical-shape refusals.
- Added complete dialogue/manifest/asset preview with source-file digest binding.
- Added atomic dialogue/manifest/asset writes and original-byte restoration on
  write or canonical validation failure, including safe removal of a newly
  created asset registry file.
- Added derived `sound/voice/<Plugin>/<VoiceType>/<Stem>` targets and required
  voice/voice/lip asset declarations without touching source media bytes.
- Added Narrative Author voice controls, preview, append, validation, and
  GECK-handoff rebuild.
- Added focused Windows tests for both modes, zero-write preview, missing source,
  stale source digest, duplicate refusal, validation, and voice worklist output.

## Validation

- All six test suites passed on the clean run: 710 tests, zero failures and zero skips.
- Release publication passed.
- Published-app automation bound the existing synthetic complete trio to the
  unvoiced ExampleMod line.
- Result preserved exact `ExampleMod.esm`, `ExampleVoice`, and `intro_hello`
  metadata; handoff contained WAV/OGG/LIP source mappings,
  `validated-declaration`, and unresolved `voice-export` action.
- Repeated preview disabled append and preserved dialogue SHA-256.
- App remained responsive; isolated LocalAppData project and process were removed.

## Test-run note

The first broad no-build run again reported one transient unit failure without
retaining failure detail. The isolated unit rerun passed all 117 tests and the
subsequent complete six-project run passed all 710 tests without source changes.

## Boundaries

- No source-media copy or mutation, recording, conversion, LIP generation,
  audio metadata analysis, GECK export, plugin mutation, GECK/xEdit execution,
  game Data/MO2 write, network, release publication, or AI.

## Next route

Gate 413: refresh the unsigned installer and run an isolated installed-app
voice work-item regression covering choice discovery, bind, validated handoff
mapping, repeated refusal, uninstall, and cleanup.
