# Gate 427 - Quest Transition Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gate 21, Gates 399-426

## Goal

Define preview-gated authoring of one minimal transition between two existing
stages owned by the same source-backed quest, without assigning execution,
branch, lockout, fallback, or plugin semantics to that declaration.

## Research classification

- **Documented:** The Quest Registry models quests as stateful hubs with stage
  transitions rather than only linear checklists.
- **Documented:** Quest schema `0.6.0` defines a transition with stable `id`,
  optional `fromStageId`, required `toStageId`, and optional title/summary.
- **Documented:** `WF-SEM-018` requires transition stage references to resolve
  inside the owning quest.
- **Documented:** The GECK handoff already emits transition declarations in
  `worklists/quest-transitions.tsv`.
- **Documented:** Canonical source remains versioned registry data; GECK remains
  the plugin-record and runtime implementation authority.
- **Inferred:** Requiring both source and destination selectors is the smallest
  useful desktop workflow for connecting existing authored stages. It is a
  stricter authoring workflow over the schema's optional `fromStageId`, not a
  schema change.
- **Open:** Transition triggers, automatic stage advancement, conditions,
  result scripts, lockouts, fallback paths, soft points of no return, branch
  semantics, and GECK implementation mapping remain unresolved.

## Supported source shape

The workflow supports canonical JSON projects using quest schema `0.6.0`, the
exact manifest quest-registry path `src/registries/quests/`, and exactly one
`src/registries/quests/main.json`. The project must pass canonical validation
before choices or preview. Unsupported paths, YAML, multiple quest documents,
and schema migration are refused rather than rewritten.

## Desktop workflow

Narrative Author gains an `Add Quest Transition` workflow with source-backed
selectors for:

- quest;
- source stage owned by that quest;
- destination stage owned by that quest.

Authored fields are a lowercase ASCII alphanumeric transition slug and optional
title/summary. Changing the quest refreshes both stage selectors. The user
previews the complete proposed quest JSON and exact transition declaration,
then appends, validates, and rebuilds the GECK handoff with an unchanged token.

## Derived declaration

For selected quest `<questId>` and authored `<slug>`:

```text
transition ID: <questId>.transition.<slug>
```

The declaration is:

```json
{
  "id": "<transitionId>",
  "fromStageId": "<selectedSourceStageId>",
  "toStageId": "<selectedDestinationStageId>"
}
```

Title and summary are included only when non-empty. The declaration records a
reviewable relationship only. It does not mean that Forge will call `SetStage`
or otherwise execute progression.

## Preservation and transaction

- Parse and deep-clone quest source through structured JSON APIs.
- Append one object only to the selected quest's `transitions` array, creating
  that optional array only when absent.
- Preserve every existing quest, stage, objective, transition, condition,
  variable, result intent, reference, tag, value, and array order before the
  appended item by deep equality.
- Preview and refusals write zero bytes.
- Bind the token to normalized inputs, selected IDs, manifest/quest bytes, and
  exact proposed quest bytes.
- Append re-runs preview, verifies the token, writes the proposal, validates,
  and restores original bytes after write or validation failure.
- Successful validation rebuilds `geck-handoff`; package failure retains valid
  canonical source and reports a retryable output failure.

## Handoff behavior

The existing `worklists/quest-transitions.tsv` row must contain exact quest,
transition, source-stage, destination-stage, title, and summary values. The
handoff manifest, checksums, and build manifest must cover the regenerated
worklist deterministically. Gate 427 adds no new worklist or unresolved action:
the transition has no proven executable or GECK mapping behavior to request.

## Refusals

- Unsupported canonical shape or failed pre-validation.
- Selected quest no longer exists.
- Either selected stage no longer exists or belongs to another quest.
- Invalid lowercase ASCII alphanumeric slug.
- Derived transition ID duplicates a transition in the selected quest.
- Stale selection, input, source, manifest, or proposal token.
- Path escape, parse failure, write failure, or post-write validation failure.

The schema does not establish that a self-reference is invalid. Selecting the
same stage for both endpoints is therefore not rejected by an invented rule;
the preview exposes the declaration for human review without assigning runtime
meaning.

## Safety boundaries

- No stage creation/edit/delete/reorder or stage-number mutation.
- No objective, condition, variable, result intent, dialogue, asset, external
  reference, quest identity, or existing transition mutation.
- No trigger, condition, script, side effect, lockout, fallback, branch,
  traversal, reachability, or runtime execution semantics.
- No GECK/xEdit launch, plugin mutation, game Data/MO2 write, game launch,
  runtime probe, network, release publication, or AI requirement.

## Gate 428 acceptance criteria

- Quest-owned source/destination stage choices load and refresh
  deterministically.
- Preview writes zero bytes and shows the exact declaration and complete
  proposed quest document.
- Append preserves existing source, adds exactly one transition, validates, and
  rebuilds the handoff.
- Existing-array, missing-array, distinct-stage, and schema-permitted same-stage
  cases pass.
- Wrong ownership, missing endpoint, invalid slug, duplicate identity, stale
  token, unsupported shape, write failure, and validation rollback have focused
  coverage.
- Published desktop automation appends one transition, verifies its handoff row,
  and proves repeated duplicate refusal preserves quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 428: implement source-backed quest transition authoring, Narrative Author
controls, transaction/refusal coverage, handoff verification, and published-app
regression.
