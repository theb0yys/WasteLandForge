# Gate 416 - Published Dialogue Branch Interaction Regression

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gates 414-415, ADR-004, ADR-007, ADR-009, ADR-011

## Goal

Make dialogue branch authoring reliably reachable in the published desktop app
and close the Gate 415 published interaction regression.

## Implemented

- Added a Narrative Author workflow navigator for source, dialogue behavior,
  dialogue branch, and voice work-item sections.
- Moved the complete dialogue branch editor into the first Narrative Author
  viewport so its preview and append actions do not depend on deep scrolling.
- Kept the existing source-backed branch engine, transaction, validation, and
  GECK-handoff behavior unchanged.

## Published regression

- Published the self-contained `win-x64` desktop app.
- Copied the synthetic ExampleMod fixture to an isolated writable LocalAppData
  test directory.
- Loaded source-backed branch choices, previewed a `Link To`, and invoked append.
- Verified the exact `gate416branch` declaration in canonical dialogue source.
- Verified the exact line/link/type/target row in
  `worklists/dialogue-links.tsv`.
- Repeated preview refused the duplicate, left append disabled, and preserved
  dialogue SHA-256.
- Verified the application remained responsive.
- Closed the application and removed the isolated LocalAppData test directory.

## Validation

- Desktop release build passed with zero warnings and errors.
- Focused dialogue branch tests passed: 4 tests.
- Publication passed.
- The first full run reported the known environment-sensitive
  `SOURCE_DATE_EPOCH` unit failure; its isolated rerun passed.
- The subsequent complete run passed all 714 tests with zero failures/skips.

## Diagnostic note

Early regression attempts correctly refused writes because a repository-tree
copy retained read-only/protected write behavior. Capturing the visible status
proved the append event was dispatching. The final regression used a writable
isolated LocalAppData copy, matching the published-app test boundary.

## Boundaries

- No route taxonomy, runtime selection, graph traversal, GECK mapping, plugin
  creation/mutation, external tool execution, game Data/MO2 write, network, or
  AI behavior.

## Next route

Gate 417: define preview-gated revision of an existing dialogue line's response
text, optional speaker, optional prompt, and priority while preserving identity,
quest/topic references, gameplay declarations, voice, branches, and handoff
safety boundaries.
