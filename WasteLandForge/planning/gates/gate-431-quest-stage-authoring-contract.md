# Gate 431 - Quest Stage Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gate 20, Gates 402-430

## Goal

Define preview-gated authoring of one minimal stage on an existing source-backed
quest, with explicit unique non-negative stage number and optional presentation,
without assigning execution, completion, transition, or result-script behavior.

## Research classification

- **Documented:** `SetStage`, `GetStage`, and `GetStageDone` make quest stages a
  persistent authored state surface.
- **Documented:** Quest state owns canonical progression in the hybrid state
  architecture.
- **Documented:** Quest schema `0.6.0` requires stage `id` and an integer `stage`
  value greater than or equal to zero; title, summary, and result scripts are
  optional.
- **Documented:** Existing Narrative Author contracts require distinct stage
  numbers and existing authoring code refuses duplicates inside a quest.
- **Documented:** The GECK handoff emits stage declarations in
  `worklists/quest-stages.tsv`, including result-intent IDs when present.
- **Documented:** Canonical registry source remains authoritative and GECK
  remains the plugin-record authority.
- **Inferred:** Per-quest stage-number uniqueness is a necessary authoring rule
  for deterministic stage selection even though JSON Schema cannot express it
  through `uniqueItems` alone.
- **Open:** Exact stage firing, completion, ordering beyond numeric identity,
  conditions, result execution, flags, and GECK record mapping remain unresolved.

## Supported source shape

The workflow supports canonical JSON projects using quest schema `0.6.0`, the
exact manifest quest-registry path `src/registries/quests/`, and exactly one
`src/registries/quests/main.json`. The project must pass canonical validation
before choices or preview. Unsupported paths, YAML, multiple quest documents,
and schema migration are refused rather than rewritten.

## Desktop workflow

Narrative Author gains an `Add Quest Stage` workflow with:

- a source-backed quest selector;
- required lowercase ASCII alphanumeric stage slug;
- required non-negative integer stage number;
- optional title;
- optional summary.

The user previews the exact stage declaration and complete proposed quest JSON,
then appends, validates, and rebuilds the GECK handoff with an unchanged token.
No result intent is created as part of this workflow.

## Derived declaration

For selected quest `<questId>` and authored `<slug>`:

```text
stage ID: <questId>.stage.<slug>
```

The minimal declaration is:

```json
{
  "id": "<stageId>",
  "stage": 30
}
```

Title and summary are included only when their trimmed values are non-empty.
`resultScripts` is omitted. The declaration records stage identity only; Forge
does not execute `SetStage`, mark the stage complete, or connect it to another
quest node.

## Preservation and transaction

- Parse and deep-clone quest source through structured JSON APIs.
- Append one object only to the selected quest's `stages` array, creating that
  optional array only when absent.
- Preserve every existing quest, stage, objective, transition, condition,
  variable, result intent, reference, tag, value, and array order before the
  appended item by deep equality.
- Preview and refusals write zero bytes.
- Bind the token to normalized inputs, selected quest ID, manifest/quest bytes,
  and exact proposed quest bytes.
- Append re-runs preview, verifies the token, writes the proposal, validates,
  and restores original bytes after write or validation failure.
- Successful validation rebuilds `geck-handoff`; package failure retains valid
  canonical source and reports a retryable output failure.

## Handoff behavior

The existing `worklists/quest-stages.tsv` row must contain the exact quest ID,
stage ID, number, title, and summary. Its `resultIntentIds` field must be empty.
The handoff manifest, checksums, and build manifest must cover the regenerated
worklist deterministically. No additional worklist or unresolved action is
introduced.

## Refusals

- Unsupported canonical shape or failed pre-validation.
- Selected quest no longer exists.
- Invalid lowercase ASCII alphanumeric slug.
- Stage number is blank, non-integer, or negative.
- Stage number duplicates any existing stage number in the selected quest.
- Derived stage ID duplicates any existing stage ID in the selected quest.
- Stale selection, input, source, manifest, or proposal token.
- Path escape, parse failure, write failure, or post-write validation failure.

Stage numbers need not be greater than all existing numbers and Forge does not
sort or reorder canonical source. A valid unused number is appended exactly as
authored; generated handoff ordering remains the emitter's existing behavior.

## Safety boundaries

- No stage edit/delete/reorder, automatic numbering, condition, flag,
  transition, result intent, script, side effect, or execution semantics.
- No objective, variable, dialogue, asset, external reference, quest identity,
  or existing-object mutation.
- No GECK/xEdit launch, plugin mutation, game Data/MO2 write, game launch,
  runtime probe, network, release publication, or AI requirement.

## Gate 432 acceptance criteria

- Quest choices load deterministically.
- Minimal, presentation-populated, and missing-stages-array modes append exactly
  the expected schema shape.
- Preview writes zero bytes and shows the exact declaration and proposed quest.
- Append preserves existing source, validates, and rebuilds the handoff.
- Invalid/negative/duplicate number, invalid/duplicate slug, stale token,
  missing quest, unsupported shape, write failure, and validation rollback have
  focused coverage.
- Published desktop automation appends one titled stage, verifies its handoff
  row and empty result-intent field, and proves duplicate refusal preserves
  quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 432: implement standalone source-backed quest-stage authoring, Narrative
Author controls, transaction/refusal coverage, handoff verification, and
published-app regression.
