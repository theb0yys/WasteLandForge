# Gate 403 - Narrative Authoring Vertical Slice

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 402, ADR-007, ADR-009, ADR-010

## Goal

Implement the complete preview-gated desktop workflow that creates a minimal
canonical quest/dialogue narrative and builds the existing GECK authoring
handoff without plugin mutation or external-tool execution.

## Implemented

- Added `NarrativeSourceAuthoring` with structured quest `0.6.0`, dialogue
  `0.23.0`, and manifest JSON generation.
- Added a preview token binding normalized proposal plus current manifest and
  target-source existence.
- Added stale-preview, existing-source, conflicting-path, invalid-stage,
  incomplete GECK reference, slug, required-text, prompt/priority, and plugin
  filename refusals.
- Added guarded source creation with manifest restoration and new-source
  deletion on write failure.
- Added the Narrative Author desktop workspace with quest, stages, objective,
  topic, line, optional speaker/prompt/priority, and optional explicit GECK
  reference fields.
- Successful creation runs canonical `forge validate .` and
  `forge package . --target geck-handoff`, presenting both structured results.
- Added deterministic Windows tests for zero-write preview, successful
  validation/package, stale token, invalid input, conflicting manifest path,
  and no-overwrite preservation.

## Validation

- All six test suites passed: 704 tests, zero failures, zero skips.
- Release desktop publication passed.
- Published-app automation previewed and created a synthetic narrative under
  isolated LocalAppData, then completed a handoff with one quest, one dialogue
  line, and four unresolved GECK actions.
- Confirmed quest source, dialogue source, and handoff manifest existed and the
  app remained responsive.
- Removed repository and LocalAppData synthetic test roots and confirmed no
  WastelandForge process remained.

## Environment correction

The first published-app test used a repository path under Documents. Windows
Controlled Folder Access refused the unsigned GUI/CLI child process source
writes, consistent with Gate 394 evidence. The completed regression moved only
the synthetic project to LocalAppData; no security policy was changed.

## Boundaries

- No existing quest/dialogue append or editing.
- No GECK/xEdit launch, ESP/ESM mutation, FormID invention, condition mapping,
  script compilation, voice export, game Data/MO2 write, network, release, or AI.

## Next route

Gate 404: refresh the unsigned installer and run an isolated installed-app
Narrative Author regression covering preview, create, validation, GECK handoff,
no-overwrite refusal, uninstall, and cleanup.
