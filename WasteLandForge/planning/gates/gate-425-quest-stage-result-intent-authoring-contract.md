# Gate 425 - Quest Stage Result-Intent Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gate 23, Gates 399-424

## Goal

Define source-backed authoring of one typed `stageResult` declaration on an
existing quest stage, optionally gated by an existing quest-local condition,
and close the GECK-handoff visibility gap without generating executable script.

## Research classification

- **Documented:** Setting a quest stage runs attached stage results whose
  conditions pass.
- **Documented:** Quest schema `0.6.0` models stage-local `stageResult`
  declarations with stable ID, optional quest-local `conditionId`, and optional
  summary.
- **Documented:** Semantic validation rejects result-intent condition references
  that do not resolve inside the owning quest.
- **Documented:** Forge owns deterministic authoring handoffs but GECK remains
  the plugin record and script authority.
- **Documented:** Current GECK handoff exposes only comma-separated result IDs in
  `quest-stages.tsv`; it does not expose condition references or manual script
  actions.
- **Inferred:** A dedicated quest result-intent worklist and unresolved action
  are required for a reviewable handoff equivalent to dialogue result intent.
- **Open:** Raw script syntax, commands, side effects, execution ordering, GECK
  result-field mapping, compilation, and runtime behavior remain unresolved.

## Supported source shape

The workflow supports canonical JSON projects using quest schema `0.6.0` and
dialogue schema `0.23.0`, with exact canonical registry paths, one `main.json`
per registry, and passing canonical validation before choices or preview.

## Desktop workflow

Narrative Author gains an `Add Stage Result Intent` workflow with source-backed
selectors for quest, quest-owned stage, and optional quest-owned condition.
Authored fields are:

- lowercase ASCII alphanumeric result slug;
- `Use condition` toggle;
- optional non-empty summary.

Selecting a quest refreshes stage and condition choices. The user previews the
complete proposed quest JSON and exact declaration, then appends, validates, and
rebuilds the GECK handoff with an unchanged preview token.

## Derived declaration

For selected stage `<stageId>` and authored `<slug>`:

```text
result ID: <stageId>.result.<slug>
```

The declaration is:

```json
{
  "id": "<resultId>",
  "scriptType": "stageResult",
  "conditionId": "<selectedConditionId>"
}
```

`conditionId` is omitted when the toggle is off. Summary is included only when
non-empty. No script body or command is synthesized.

## Handoff extension

Gate 426 adds deterministic `worklists/quest-result-intent.tsv` with columns:

```text
questId stageId resultId scriptType conditionId summary status
```

Each row uses status `manual-script-authoring-required`. Existing
`quest-stages.tsv.resultIntentIds` remains for stage-level overview.

Each result also adds one unresolved action:

```text
actionId: <resultId>.script
ownerId: <resultId>
category: result-script
requiredAction: Author and review the GECK stage result script.
reason: Registry intent is not executable script text.
blockingForPluginCompletion: true
```

No published handoff schema change is required because worklist files are
already manifest-described outputs; any manifest counts/digests/checksums must
include the new file deterministically.

## Preservation and transaction

- Parse and deep-clone quest source with structured JSON APIs.
- Read dialogue source for bounded-shape and validation authority only.
- Append one object only to the selected stage's `resultScripts` array, creating
  that optional array only when absent.
- Preserve all existing source and ordering before the appended item by deep
  equality; preserve all non-selected quests/stages unchanged.
- Preview/refusals write zero bytes.
- Token binds normalized inputs, selected quest/stage/condition IDs,
  manifest/quest/dialogue bytes, and exact proposed quest bytes.
- Append re-runs preview, verifies token, writes proposal, validates, and
  restores original bytes after write or validation failure.
- Successful validation rebuilds GECK handoff; package failure retains valid
  source and reports retryable output failure.

## Refusals

- Unsupported canonical shape or failed pre-validation.
- Selected quest, stage, or enabled condition no longer exists or has different
  quest ownership.
- Selected quest has no stage; enabled condition mode has no condition choice.
- Invalid lowercase ASCII alphanumeric slug.
- Derived result ID duplicates any result on the selected stage.
- Stale selection/input/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No raw script, command, side-effect language, evaluation/execution semantics,
  result edit/delete/reorder, GECK mapping, compilation, or plugin mutation.
- No quest identity, variable, stage number, objective, transition, condition,
  external reference, tag, dialogue, or asset mutation.
- No GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 426 acceptance criteria

- Quest-owned stage/condition choices load and refresh deterministically.
- Conditional and unconditional modes both preserve source and append exactly
  one typed result intent.
- Preview writes zero bytes; append validates and rebuilds handoff.
- Stage overview, dedicated result worklist, unresolved action, manifest,
  digests, and checksums contain exact deterministic evidence.
- Missing-array and existing-array cases pass.
- Wrong ownership, missing enabled condition, invalid slug, duplicate ID, stale
  token, unsupported shape, write failure, and validation rollback have focused
  coverage.
- Published desktop automation appends one result, verifies worklist/action,
  and proves repeated duplicate refusal preserves quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 426: implement stage result-intent authoring, dedicated handoff worklist and
unresolved action, Narrative Author controls, focused tests, and published-app
regression.
