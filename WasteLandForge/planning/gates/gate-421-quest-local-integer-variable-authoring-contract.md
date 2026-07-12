# Gate 421 - Quest-Local Integer Variable Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gate 24, Gates 408-420

## Goal

Define a source-backed Narrative Author edit that appends one typed integer
variable to an existing quest with stable identity, explicit initial value,
optional presentation, canonical validation, and GECK-handoff output.

## Research classification

- **Documented:** New Vegas-style reactivity keeps quest state distinct from
  faction, reputation, world, event-history, companion, and dialogue state.
- **Documented:** Quest schema `0.6.0` models quest-local variables only as
  `variableType: integer`.
- **Documented:** `initialValue` is explicit authored integer state and is not a
  schema default.
- **Documented:** Semantic validators resolve quest and dialogue variable
  references against variables declared on the owning quest.
- **Documented:** Dialogue behavior authoring can reference a quest-local
  integer variable for typed increment intent without generating script text.
- **Documented:** GECK handoff emits quest ID, variable ID, type, initial value,
  and title to `quest-variables.tsv`.
- **Inferred:** One explicit integer declaration is the smallest safe authoring
  slice that unlocks additional validated dialogue state workflows.
- **Open:** Hidden-variable conventions, non-integer types, exact GECK field
  mapping, initialization timing, persistence, and runtime mutation remain open.

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

Narrative Author gains an `Add Quest Variable` workflow with a deterministic
source-backed quest selector. Authored fields are:

- lowercase ASCII alphanumeric variable slug;
- required integer initial value;
- optional non-empty title;
- optional non-empty summary.

The user loads quests, selects one, previews complete proposed quest JSON and an
append summary, then appends, validates, and rebuilds the GECK handoff with an
unchanged preview token.

## Derived declaration

For selected quest `<questId>` and authored `<slug>`:

```text
variable ID: <questId>.variable.<slug>
```

The appended object is:

```json
{
  "id": "<variableId>",
  "variableType": "integer",
  "initialValue": 0
}
```

The example value is illustrative; emitted `initialValue` is the exact authored
JSON integer. `title` and `summary` are included only when non-empty.

## Preservation and transaction

- Parse and deep-clone the quest root with structured JSON APIs.
- Read dialogue source for bounded-shape and validation authority only.
- Append one object to only the selected quest's `variables` array, creating the
  optional array only when absent.
- Preserve all existing quest properties, objects, values, and array order by
  deep equality before the appended item.
- Preserve every non-selected quest by deep equality.
- Preview and every refusal write zero bytes.
- Bind normalized authored inputs, selected quest ID, manifest/quest/dialogue
  bytes, and exact proposed quest bytes into the preview token.
- Append re-runs preview, compares the token, retains original quest bytes,
  writes the proposal, and runs canonical validation.
- Write or validation failure restores the original bytes.
- Successful validation rebuilds `forge package . --target geck-handoff`.
  Packaging failure retains valid canonical source and reports retryable output
  failure.

## Refusals

- Unsupported path, filename, schema, document count, parse state, or failed
  canonical pre-validation.
- Selected quest no longer exists.
- Invalid lowercase ASCII alphanumeric slug.
- Initial value is blank, malformed, or outside the .NET/JSON 32-bit integer
  range used by the established authoring implementation.
- Derived variable ID duplicates any variable on the selected quest.
- Stale selection/input/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No existing variable edit/delete/reorder, non-integer type, implicit default,
  condition creation, mutation intent, script text, or runtime behavior.
- No quest identity, stage, objective, transition, condition, result intent,
  external reference, tag, dialogue, or asset mutation.
- No schema migration, raw JSON editor, EditorID/FormID invention, GECK mapping,
  plugin mutation, GECK/xEdit launch, game Data/MO2 write, game launch, runtime
  probe, network, release publication, or AI requirement.

## Gate 422 acceptance criteria

- Source-backed quest choices load deterministically.
- Preview writes zero bytes and shows exact proposed quest JSON and declaration.
- Successful append preserves all existing source, adds exactly one variable,
  validates, and rebuilds the handoff.
- `quest-variables.tsv` contains exact quest ID, variable ID, integer type,
  initial value, and title.
- Existing-array and missing-array cases both pass.
- Invalid slug/value, duplicate ID, stale token, unsupported shape, write
  failure, and validation rollback receive focused Windows coverage.
- Published desktop automation appends one variable, verifies handoff output,
  and proves repeated duplicate refusal preserves quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 422: implement the quest-variable choice loader, preview-token append
engine, Narrative Author controls, canonical validation and handoff rebuild,
focused tests, and published-app regression.
