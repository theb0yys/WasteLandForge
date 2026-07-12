# Gate 423 - Quest-Local Condition Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gate 24, Gates 408-422

## Goal

Define a source-backed Narrative Author edit that appends one typed quest-local
`stageDone` or `variableEquals` condition using quest-owned operands, stable
identity, canonical validation, and GECK-handoff review output.

## Research classification

- **Documented:** Quest state is a distinct reactivity domain and should not be
  collapsed into dialogue, faction, reputation, world, or event-history state.
- **Documented:** Quest schema `0.6.0` models only `stageDone` and
  `variableEquals` quest condition shapes.
- **Documented:** `stageDone` requires `stageId` and forbids variable operands.
- **Documented:** `variableEquals` requires `variableId` and integer
  `equalsInteger`, and forbids `stageId`.
- **Documented:** Semantic validation resolves referenced stages and variables
  inside the condition's owning quest.
- **Documented:** GECK handoff emits quest conditions as `manual-map` and adds an
  unresolved condition-mapping action because no complete proven mapping exists.
- **Inferred:** Explicit source-backed mode and operand choices are the smallest
  safe authoring surface over the existing typed condition contract.
- **Open:** GECK function names, comparison/run-on settings, evaluation order,
  boolean composition, runtime behavior, and plugin output remain unresolved.

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

Narrative Author gains an `Add Quest Condition` workflow with source-backed
selectors for quest, quest-owned stage, and quest-owned integer variable.
Condition mode is an explicit choice:

- `Stage Done`;
- `Variable Equals`.

Authored fields are:

- lowercase ASCII alphanumeric condition slug;
- required integer comparison value in Variable Equals mode;
- optional non-empty title;
- optional non-empty summary.

Selecting a quest refreshes its stage and variable choices. The user previews
complete proposed quest JSON and the exact declaration, then appends, validates,
and rebuilds the GECK handoff with an unchanged preview token.

## Derived declarations

For selected quest `<questId>` and authored `<slug>`:

```text
condition ID: <questId>.condition.<slug>
```

Stage Done emits exactly:

```json
{
  "id": "<conditionId>",
  "conditionType": "stageDone",
  "stageId": "<selectedStageId>"
}
```

Variable Equals emits exactly:

```json
{
  "id": "<conditionId>",
  "conditionType": "variableEquals",
  "variableId": "<selectedVariableId>",
  "equalsInteger": 0
}
```

The comparison value is illustrative; emitted `equalsInteger` is the exact
authored JSON integer. Optional title/summary are included only when non-empty.

## Preservation and transaction

- Parse and deep-clone the quest root with structured JSON APIs.
- Read dialogue source for bounded-shape and validation authority only.
- Append one object to only the selected quest's `conditions` array, creating
  the optional array only when absent.
- Preserve all existing quest values, objects, and array order by deep equality
  before the appended item; preserve every non-selected quest by deep equality.
- Preview and every refusal write zero bytes.
- Bind normalized inputs, selected quest/mode/operand IDs, manifest/quest/dialogue
  bytes, and exact proposed quest bytes into the preview token.
- Append re-runs preview, compares the token, retains original quest bytes,
  writes the proposal, and runs canonical validation.
- Write or validation failure restores original bytes.
- Successful validation rebuilds `forge package . --target geck-handoff`.
  Packaging failure retains valid source and reports retryable output failure.

## Refusals

- Unsupported path, filename, schema, document count, parse state, or failed
  canonical pre-validation.
- Selected quest, stage, or variable no longer exists or is owned by another
  quest.
- Selected quest has no stage for Stage Done or no integer variable for Variable
  Equals.
- Invalid mode or lowercase ASCII alphanumeric slug.
- Missing, malformed, or out-of-range 32-bit comparison integer in Variable
  Equals mode.
- Derived condition ID duplicates an existing condition on the selected quest.
- Stale selection/input/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No boolean composition, condition edit/delete/reorder, implicit operands,
  evaluation semantics, GECK function mapping, run-on target, or script text.
- No quest identity, variable, stage, objective, transition, result intent,
  external reference, tag, dialogue, or asset mutation.
- No schema migration, raw JSON editor, EditorID/FormID invention, plugin
  mutation, GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 424 acceptance criteria

- Quest-owned stage/variable choices load and refresh deterministically.
- Preview writes zero bytes and displays exact proposed source/declaration.
- Both modes preserve existing source, append exactly one condition, validate,
  and rebuild handoff.
- `quest-conditions.tsv` contains exact type/operands and `manual-map`; unresolved
  actions contain the exact condition-mapping action.
- Existing-array and missing-array cases pass.
- Wrong ownership, missing operand, invalid value/slug/mode, duplicate ID, stale
  token, unsupported shape, write failure, and validation rollback have focused
  Windows coverage.
- Published desktop automation appends one condition, verifies handoff/action
  output, and proves repeated duplicate refusal preserves quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 424: implement quest-condition choices, both typed append modes, preview
token transaction, Narrative Author controls, canonical validation and handoff
rebuild, focused tests, and published-app regression.
