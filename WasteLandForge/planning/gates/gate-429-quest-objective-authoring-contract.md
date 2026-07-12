# Gate 429 - Quest Objective Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gate 20, Gates 399-428

## Goal

Define preview-gated authoring of one objective on an existing source-backed
quest, with independently optional start and completion references to existing
quest-owned stages and no invented objective display or runtime semantics.

## Research classification

- **Documented:** New Vegas quests are editor containers whose principal parts
  include objectives and dialogue.
- **Documented:** Quest state owns canonical progression and objectives in the
  hybrid state architecture.
- **Documented:** Quest schema `0.6.0` requires objective `id` and non-empty
  `text`; `startStageId` and `completionStageId` are independently optional.
- **Documented:** `WF-SEM-017` requires authored objective stage references to
  resolve inside the owning quest.
- **Documented:** The GECK handoff already emits objective declarations in
  `worklists/quest-objectives.tsv`.
- **Documented:** Canonical registry source remains authoritative and GECK
  remains the plugin-record authority.
- **Inferred:** Independent start/completion toggles faithfully expose all four
  existing schema shapes without assigning lifecycle behavior.
- **Open:** Exact GECK objective display, activation, completion, condition,
  ordering, visibility, and runtime mapping semantics remain unresolved.

## Supported source shape

The workflow supports canonical JSON projects using quest schema `0.6.0`, the
exact manifest quest-registry path `src/registries/quests/`, and exactly one
`src/registries/quests/main.json`. The project must pass canonical validation
before choices or preview. Unsupported paths, YAML, multiple quest documents,
and schema migration are refused rather than rewritten.

## Desktop workflow

Narrative Author gains an `Add Quest Objective` workflow with:

- a source-backed quest selector;
- required lowercase ASCII alphanumeric objective slug;
- required non-empty objective text;
- independent `Use start stage` and `Use completion stage` toggles;
- source-backed start and completion stage selectors owned by the quest.

Changing the quest refreshes both stage selectors. Disabled references are
omitted, not serialized as empty values. The user previews the exact objective
declaration and complete proposed quest JSON, then appends, validates, and
rebuilds the GECK handoff with an unchanged token.

## Derived declaration

For selected quest `<questId>` and authored `<slug>`:

```text
objective ID: <questId>.objective.<slug>
```

The maximal declaration is:

```json
{
  "id": "<objectiveId>",
  "text": "<trimmedObjectiveText>",
  "startStageId": "<selectedStartStageId>",
  "completionStageId": "<selectedCompletionStageId>"
}
```

Each stage reference is included only when its toggle is enabled. The
declaration records author intent only; Forge does not infer when the objective
appears, becomes active, completes, or is shown to the player.

## Preservation and transaction

- Parse and deep-clone quest source through structured JSON APIs.
- Append one object only to the selected quest's `objectives` array, creating
  that optional array only when absent.
- Preserve every existing quest, stage, objective, transition, condition,
  variable, result intent, reference, tag, value, and array order before the
  appended item by deep equality.
- Preview and refusals write zero bytes.
- Bind the token to normalized inputs, toggle states, selected IDs,
  manifest/quest bytes, and exact proposed quest bytes.
- Append re-runs preview, verifies the token, writes the proposal, validates,
  and restores original bytes after write or validation failure.
- Successful validation rebuilds `geck-handoff`; package failure retains valid
  canonical source and reports a retryable output failure.

## Handoff behavior

The existing `worklists/quest-objectives.tsv` row must contain exact quest,
objective, text, optional start-stage, and optional completion-stage values.
Disabled references produce empty TSV fields. The handoff manifest, checksums,
and build manifest must cover the regenerated worklist deterministically. No
new worklist, executable script, or unresolved mapping action is introduced.

## Refusals

- Unsupported canonical shape or failed pre-validation.
- Selected quest no longer exists.
- An enabled selected stage no longer exists or belongs to another quest.
- Invalid lowercase ASCII alphanumeric slug.
- Blank objective text.
- Derived objective ID duplicates an objective in the selected quest.
- Stale toggle, selection, input, source, manifest, or proposal token.
- Path escape, parse failure, write failure, or post-write validation failure.

The schema does not prohibit equal start and completion references. That shape
is therefore previewed for human review without an invented rejection or
runtime interpretation.

## Safety boundaries

- No objective edit/delete/reorder, objective condition, display priority,
  visibility rule, marker, target, script, or runtime lifecycle semantics.
- No stage creation/edit/delete/reorder or stage-number mutation.
- No transition, condition, variable, result intent, dialogue, asset, external
  reference, quest identity, or existing-object mutation.
- No GECK/xEdit launch, plugin mutation, game Data/MO2 write, game launch,
  runtime probe, network, release publication, or AI requirement.

## Gate 430 acceptance criteria

- Quest-owned stage choices load and refresh deterministically.
- Text-only, start-only, completion-only, both-reference, missing-array, and
  schema-permitted same-stage modes append exactly the expected shape.
- Preview writes zero bytes and shows the exact declaration and proposed quest.
- Append preserves existing source, validates, and rebuilds the handoff.
- Wrong ownership, missing enabled stage, blank text, invalid slug, duplicate
  identity, stale token, unsupported shape, write failure, and rollback receive
  focused coverage.
- Published desktop automation appends one both-reference objective, verifies
  its handoff row, and proves duplicate refusal preserves quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 430: implement source-backed quest objective authoring, Narrative Author
controls, all four optional-reference modes, transaction/refusal coverage,
handoff verification, and published-app regression.
