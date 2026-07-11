# Gate 408 - Dialogue Gate and Result Intent Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-005, ADR-007, ADR-009, Gates 405-407

## Goal

Define a source-backed Narrative Author edit that adds one quest-stage dialogue
condition and one quest-variable increment result intent to an existing line.
The edit preserves typed gameplay intent for validation and GECK handoff review;
it does not generate executable script text or GECK condition mappings.

## Research classification

- **Documented:** New Vegas reactivity keeps quest state distinct from faction,
  world, reputation, companion, and event-history state.
- **Documented:** Dialogue schema `0.23.0` models line-local
  `questStageDone` conditions with a stage reference.
- **Documented:** The same schema models `dialogueResult` declarations with
  `questVariableIncrement` mutations containing variable ID and integer delta.
- **Documented:** Semantic validators require the selected stage and variable
  to belong to the quest referenced by the dialogue line.
- **Documented:** Gate 400 emits condition rows as `manual-map` and result intent
  as `manual-script-authoring-required`; Forge does not invent executable text.
- **Inferred:** Pairing line availability with one explicit quest-state mutation
  is the smallest useful behavior-authoring slice after structural narrative
  creation and append.
- **Open:** Exact GECK condition functions, comparison/run-on settings, script
  syntax, execution ordering, compilation, and runtime behavior remain human
  authoring/review concerns.

## Supported source shape

The workflow uses the same bounded JSON shape as Gate 405:

```text
src/registries/quests/main.json    schema 0.6.0
src/registries/dialogue/main.json  schema 0.23.0
```

The manifest must declare exact canonical quest/dialogue registry directories,
each directory must contain only `main.json`, and canonical project validation
must pass before source choices are loaded or previewed.

## Desktop workflow

Narrative Author gains an `Add Dialogue Behavior` section with source-backed
selectors:

- dialogue line;
- quest stage from the selected line's `questId` only;
- integer quest variable from the selected line's `questId` only.

Authored fields are:

- condition slug and optional summary;
- result slug and optional summary;
- mutation slug and optional summary;
- integer increment delta, explicitly entered and allowed to be negative,
  zero, or positive because the schema defines an integer without a range.

The user performs:

1. `Load Dialogue Behavior Choices`;
2. selects a line, stage, and variable;
3. `Preview Dialogue Behavior`;
4. reviews the complete proposed dialogue JSON and append summary;
5. `Append, Validate and Rebuild Handoff` with an unchanged preview token;
6. reviews validation and GECK-handoff results.

Projects or selected quests with no integer variable receive a clear refusal;
this gate does not create quest variables implicitly.

## Derived declarations

For selected line `<lineId>`:

```text
condition ID: <lineId>.condition.<conditionSlug>
result ID:    <lineId>.result.<resultSlug>
mutation ID:  <resultId>.mutation.<mutationSlug>
```

The appended condition is exactly:

```json
{
  "id": "<conditionId>",
  "conditionType": "questStageDone",
  "stageId": "<selectedStageId>"
}
```

The appended result intent is exactly:

```json
{
  "id": "<resultId>",
  "scriptType": "dialogueResult",
  "mutations": [
    {
      "id": "<mutationId>",
      "mutationType": "questVariableIncrement",
      "variableId": "<selectedVariableId>",
      "deltaInteger": "<authored integer>"
    }
  ]
}
```

Optional summaries are included only when non-empty. The JSON example denotes
the placeholder textually; emitted `deltaInteger` is a JSON integer.

## Preservation and transaction

- Parse and deep-clone quest/dialogue roots using structured JSON APIs.
- Read quest source for selector/reference authority only; do not modify it.
- Append one object to the selected line's `conditions` array and one object to
  its `resultScripts` array, creating either missing optional array only on that
  selected line.
- Preserve every existing object, value, and array item by deep equality and
  retain existing order before appended items.
- Preview and refusals write zero bytes.
- Token binds normalized input, selected IDs, manifest bytes, quest bytes,
  dialogue bytes, and exact proposed dialogue bytes.
- Append re-runs preview, checks the token, retains original dialogue bytes,
  writes the proposal, and runs canonical validation.
- Write or validation failure restores the original dialogue bytes.
- Successful validation runs `forge package . --target geck-handoff`.
- Package failure keeps valid canonical source and reports a retryable output
  failure rather than rolling back source truth.

## Refusals

- Unsupported path, filename, schema, document count, parse state, or failed
  canonical pre-validation.
- Selected line no longer exists or references an unknown quest.
- Selected stage/variable does not belong to the line quest.
- Selected quest has no integer variable.
- Invalid lowercase ASCII alphanumeric slug.
- Duplicate condition, result, or mutation logical ID.
- Delta is not an integer.
- Stale input/selection/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No condition boolean composition, variable creation, variable assignment,
  stage mutation, raw script, raw JSON editor, GECK function mapping, run-on
  target, execution ordering, compilation, or plugin mutation.
- No GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 409 acceptance criteria

- Source-backed line/stage/variable choices are deterministic and constrained
  to the line quest.
- Preview writes zero bytes and displays exact proposed dialogue JSON.
- Successful append preserves all existing nodes, adds exactly one condition,
  one result, and one mutation, validates, and rebuilds the handoff.
- Handoff contains the new `manual-map` condition row and
  `manual-script-authoring-required` result row with no executable script.
- Missing-variable, wrong-quest reference, duplicate IDs, invalid delta, stale
  token, unsupported shape, write failure, and validation rollback have focused
  Windows coverage.
- Published desktop automation completes append and proves repeated duplicate
  refusal preserves dialogue source SHA-256.
- Release build and all test suites pass.

## Next route

Gate 409: implement the complete dialogue behavior choice loader, preview-token
append engine, validation rollback, Narrative Author controls, GECK-handoff
workflow, focused tests, and published-app regression.
