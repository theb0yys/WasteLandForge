# Gate 419 - Existing Quest Presentation Revision Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gates 405-418

## Goal

Define a preview-gated Narrative Author edit that revises one existing quest's
title/summary, one selected stage's title/summary, and one selected objective's
text without changing identity, stage numbering, references, gameplay intent,
or GECK ownership boundaries.

## Research classification

- **Documented:** Quest state remains distinct from faction, world, event,
  reputation, companion, and dialogue state.
- **Documented:** Quest schema `0.6.0` requires non-empty quest `title`, allows
  optional quest `summary`, requires each stage's `id` and integer `stage`, and
  allows optional stage `title`/`summary`.
- **Documented:** Quest objectives require non-empty `text`; optional start and
  completion stage references are semantic links to stages in the same quest.
- **Documented:** GECK handoff emits quest, stage, and objective presentation
  values while leaving record creation and verification to GECK authors.
- **Inferred:** Revising one quest, stage, and objective together is the smallest
  coherent correction workflow for authored quest presentation.
- **Open:** Exact GECK record-field mapping, objective display lifecycle, stage
  result execution, and runtime quest behavior remain human/tooling concerns.

## Supported source shape

The workflow supports canonical JSON projects using:

```text
src/registries/quests/main.json    schema 0.6.0
src/registries/dialogue/main.json  schema 0.23.0
```

The manifest must declare exact canonical quest/dialogue registry directories,
each directory must contain only `main.json`, and canonical validation must pass
before choices load or preview begins.

## Desktop workflow

Narrative Author gains a `Revise Quest Presentation` workflow with deterministic
source-backed selectors:

- quest;
- stage belonging to the selected quest;
- objective belonging to the selected quest.

Selecting a quest refreshes the stage/objective choices. Selecting each object
populates exact current values. Editable fields are:

- required quest title;
- optional quest summary;
- optional selected-stage title and summary;
- required selected-objective text.

Blank optional fields remove their properties. The user previews complete
proposed quest JSON plus exact before/after field changes, then applies,
validates, and rebuilds the GECK handoff with an unchanged preview token.

Projects or selected quests without both a stage and objective receive a clear
refusal; this workflow does not create either object implicitly.

## Exact mutation

On only the selected quest, stage, and objective:

- replace quest `title` with trimmed non-empty text;
- set or remove quest `summary`;
- set or remove stage `title` and `summary`;
- replace objective `text` with trimmed non-empty text.

No-op revisions are refused. Forge does not alter or derive any other property.

## Preservation and transaction

- Parse and deep-clone the quest root with structured JSON APIs.
- Read dialogue source for bounded-shape and validation authority only.
- Preserve quest ID, variables, all stage IDs/numbers/result scripts, objective
  IDs/stage references, transitions, conditions, external references, tags,
  and any other schema-valid property by deep equality outside the five fields.
- Preserve every non-selected quest, stage, and objective by deep equality and
  retain all array ordering.
- Preview and every refusal write zero bytes.
- Bind normalized inputs, selected IDs, manifest/quest/dialogue bytes, and exact
  proposed quest bytes into the preview token.
- Apply re-runs preview, compares the token, retains original quest bytes,
  writes the proposal, and runs canonical validation.
- Write or validation failure restores the original bytes.
- Successful validation rebuilds `forge package . --target geck-handoff`.
  Packaging failure retains valid source and reports a retryable output failure.

## Refusals

- Unsupported path, filename, schema, document count, parse state, or failed
  canonical pre-validation.
- Selected quest, stage, or objective no longer exists or does not belong to the
  selected quest.
- Selected quest has no stage or no objective.
- Blank quest title or objective text.
- No effective field change.
- Stale selection/input/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No identity, stage number, variable, reference, transition, condition, result
  intent, external reference, tag, dialogue, or asset mutation.
- No stage/objective creation or deletion, broad editor, raw JSON, schema
  migration, EditorID/FormID invention, GECK mapping, script generation, or
  plugin mutation.
- No GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 420 acceptance criteria

- Source-backed quest/stage/objective choices populate and refresh correctly.
- Preview writes zero bytes and shows exact proposed quest source and changes.
- Apply changes only the five supported fields, validates, and rebuilds handoff.
- `quests.tsv`, `quest-stages.tsv`, and `quest-objectives.tsv` contain exact
  revised values.
- Set, replace, and clear optional fields pass.
- Missing stage/objective, wrong ownership, blank required fields, no-op, stale
  token, unsupported shape, write failure, and validation rollback have focused
  Windows coverage.
- Published desktop automation revises one quest/stage/objective, verifies all
  three handoff worklists, and proves repeated no-op refusal preserves quest
  SHA-256.
- Release build and all test suites pass.

## Next route

Gate 420: implement the quest presentation loader, exact-field preview and
transaction engine, Narrative Author controls, canonical validation and handoff
rebuild, focused tests, and published-app regression.
