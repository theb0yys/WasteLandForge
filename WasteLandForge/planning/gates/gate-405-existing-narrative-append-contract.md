# Gate 405 - Existing Narrative Append Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-007, ADR-009, Gates 402-404

## Goal

Define a preview-token-gated desktop edit that extends one existing quest and
adds one dialogue line without replacing unrelated quest/dialogue content.
Gate 405 defines behavior only; Gate 406 implements the complete vertical slice.

## Research classification

- **Documented:** Quest schema `0.6.0` supports stages, objectives, and
  transitions with semantic validation of stage references.
- **Documented:** Dialogue schema `0.23.0` requires each line to reference a
  declared quest and topic and supports optional speaker and prompt/priority.
- **Documented:** Canonical source remains JSON/YAML registry data; generated
  handoff output is disposable and GECK remains the plugin-record authority.
- **Documented:** Gates 403-404 prove preview-gated first-source creation and
  canonical GECK-handoff regeneration in published and installed apps.
- **Inferred:** A single stage/objective/transition plus a single dialogue line
  is the smallest coherent existing-project extension that adds practical
  quest progression and dialogue value in one reviewable transaction.
- **Open:** Branch semantics, conditions, result scripts, voice, record/Form
  IDs, and GECK function mapping remain later authoring contracts.

## Supported source shape

This first append workflow supports JSON projects where the manifest declares
the exact canonical paths:

```text
registries.quests   = src/registries/quests/
registries.dialogue = src/registries/dialogue/
```

The edit targets exactly:

```text
src/registries/quests/main.json
src/registries/dialogue/main.json
```

Both files must exist, parse as objects, use quest schema `0.6.0` and dialogue
schema `0.23.0`, and pass canonical validation before preview. YAML, multiple
documents in either registry directory, alternate filenames, or unsupported
schema versions are refused rather than rewritten or migrated implicitly.

## Desktop workflow

Narrative Author gains an `Extend Existing Narrative` mode. It loads validated
source and presents selectors populated from source, not free-text references:

- existing quest;
- existing source stage;
- topic mode: `Existing topic` or `New topic`;
- existing topic selector, or new topic slug/title fields.

Authored extension fields are:

- new stage slug, number, title, and optional summary;
- objective slug and text;
- transition slug and optional title/summary;
- dialogue-line slug and response text;
- optional speaker;
- optional prompt plus required integer priority.

The user performs:

1. `Load Existing Narrative`;
2. selects source-backed quest/stage/topic identities;
3. `Preview Narrative Extension`;
4. reviews exact full proposed quest/dialogue JSON and append summary;
5. `Append, Validate and Rebuild Handoff` with an unchanged token;
6. receives structured validation and GECK-handoff results.

## Derived identities and references

For selected quest `<questId>` and authored slugs:

```text
new stage:   <questId>.stage.<stageSlug>
objective:   <questId>.objective.<objectiveSlug>
transition:  <questId>.transition.<transitionSlug>
line:        <dialogueRegistryId>.<questLeaf>.<lineSlug>
new topic:   <projectId>.topic.<topicSlug>
```

The objective references the selected source stage as `startStageId` and the
new stage as `completionStageId`. The transition references the same pair as
`fromStageId` and `toStageId`. The line references the selected quest and the
selected or newly created topic.

Only Forge logical IDs are derived. No EditorID, FormID, GECK condition,
script, voice path, or plugin record is invented.

## Preservation rules

- Parse through structured JSON APIs and deep-clone both roots.
- Append only to the selected quest's `stages`, `objectives`, and `transitions`
  arrays, creating a missing optional array only on that quest.
- Append only to dialogue `topics` when new-topic mode is selected and always
  append one item to `lines`.
- Existing objects, values, array ordering, unknown-but-schema-valid optional
  content, and all non-selected quests/lines/topics remain deep-equal.
- Source formatting may be normalized by the established indented JSON writer;
  semantic preservation, not byte-position preservation, is guaranteed after
  a successful append.
- Preview and every refusal write zero bytes.

## Preview token and transaction

The preview token binds:

- normalized authored inputs and selected logical IDs;
- complete current quest and dialogue source bytes;
- current manifest bytes;
- exact proposed quest/dialogue JSON bytes.

Changing any input, selection, source file, or manifest invalidates append and
requires a new preview.

Append uses one guarded two-file transaction:

1. re-run preview and compare the token;
2. retain both original byte streams;
3. write both proposed files;
4. run canonical `forge validate .`;
5. restore both original byte streams if either write or validation fails;
6. on successful validation, run
   `forge package . --target geck-handoff`;
7. keep valid source if packaging fails, report the package failure, and allow
   an explicit retry because packaging does not define canonical source truth.

## Refusals

- Missing/invalid manifest or unsupported registry path/file/schema shape.
- Project does not pass canonical validation before preview.
- Selected quest, source stage, or existing topic no longer exists.
- Slug is not lowercase ASCII alphanumeric or derives a duplicate logical ID.
- Stage number is negative or duplicates any stage number in the selected quest.
- New topic identity duplicates an existing topic.
- Dialogue line identity duplicates an existing line.
- Prompt without integer priority.
- Stale token, path escape, source change, manifest change, parse failure,
  write failure, or post-write validation failure.

Every refusal reports a concrete reason, writes no partial source state, and
does not run GECK or any external game tool.

## Safety boundaries

- No existing-node delete, replace, reorder, migration, or broad editor.
- No conditions, branches, variables, result scripts, links, response routes,
  voice declarations, raw JSON editing, plugin mutation, or binary output.
- No GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 406 acceptance criteria

- Source-backed selectors load deterministic quest/stage/topic choices.
- Preview writes zero bytes and displays full proposed documents.
- Successful append preserves all pre-existing nodes by deep equality, adds
  exactly one stage/objective/transition/line and at most one topic, validates,
  and rebuilds the handoff.
- Handoff counts and worklists reflect the extension.
- Existing-topic and new-topic modes both pass.
- Duplicate identity/number, stale token, invalid prompt, unsupported shape,
  write failure, and validation rollback receive focused Windows coverage.
- Published desktop automation completes one append and proves repeated append
  refusal preserves source hashes.
- Release build and all test suites pass.

## Next route

Gate 406: implement the complete existing narrative append engine, desktop
mode/selectors, two-file rollback, canonical validation/handoff workflow,
focused tests, and published-app regression.
