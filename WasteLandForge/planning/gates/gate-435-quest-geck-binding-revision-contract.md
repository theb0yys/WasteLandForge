# Gate 435 - Quest GECK Binding Revision Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-004, ADR-007, ADR-009, Gates 419-420, Gates 433-434

## Goal

Define preview-gated revision of the plugin and EditorID on exactly one existing
quest GECK binding while preserving provider, optional FormID, array position,
other external references, and all unrelated quest source.

## Research classification

- **Documented:** Quest schema `0.6.0` external references contain provider,
  required plugin, optional EditorID, and optional FormID.
- **Documented:** Schema-valid plugin names are path-free `.esm`/`.esp` names.
- **Documented:** GECK handoff projects the first GECK binding's plugin and
  EditorID into `worklists/quests.tsv` and retains `create-or-verify`.
- **Documented:** Existing revision workflows use source-backed choices,
  exact-field mutation, no-op refusal, source-bound preview tokens, rollback
  validation, and handoff rebuild.
- **Inferred:** Exactly one GECK reference is required because zero references
  provide nothing to revise and multiple references are ambiguous under current
  first-match handoff behavior.
- **Open:** Plugin/record existence, EditorID uniqueness, FormID correctness,
  load order, masters, and record type remain unverified without external tools.

## Supported source shape

The workflow supports canonical JSON projects using quest schema `0.6.0`, the
exact manifest quest-registry path `src/registries/quests/`, and exactly one
`src/registries/quests/main.json`. Canonical validation must pass before load or
preview. Each selectable quest must contain exactly one `provider: geck`
reference. Unsupported paths, YAML, multiple documents, and migration are
refused.

## Desktop workflow

Narrative Author gains `Revise GECK Binding` with a deterministic source-backed
quest/binding selector. Selection populates exact current values for:

- required plugin filename;
- required EditorID.

The user previews exact before/after values, the complete binding declaration,
and proposed quest JSON, then applies, validates, and rebuilds the handoff with
an unchanged token. FormID is displayed as preserved evidence but is not
editable in this gate.

## Exact mutation

On only the selected GECK reference:

- replace `plugin` with the trimmed schema-valid input;
- replace `editorId` with the trimmed non-empty input.

Preserve `provider: geck`, optional `formId`, object property meaning, external
reference array position, and every other field. Blank EditorID does not remove
the property; it is refused because this workflow's binding contract requires
an EditorID.

## Preservation and transaction

- Parse and deep-clone quest source with structured JSON APIs.
- Preserve all xEdit references and their ordering/content by deep equality.
- Preserve every quest field outside the selected binding's plugin/EditorID and
  every non-selected quest by deep equality.
- Preview and refusals write zero bytes.
- Bind normalized inputs, selected quest/binding identity, manifest/quest bytes,
  preserved FormID, and exact proposed quest bytes into the token.
- Apply re-runs preview, verifies the token, writes the proposal, validates, and
  restores original bytes after write or validation failure.
- Successful validation rebuilds `geck-handoff`; package failure retains valid
  canonical source and reports a retryable output failure.

## Handoff behavior

`worklists/quests.tsv` must contain the revised plugin and EditorID plus the
unchanged `create-or-verify` action. Manifest, checksums, and build manifest must
cover the regenerated output. Revision does not prove record existence and does
not remove human plugin review actions.

## Refusals

- Unsupported canonical shape or failed pre-validation.
- Selected quest no longer exists.
- Selected quest has zero or more than one GECK reference.
- Invalid/path-bearing plugin or blank EditorID.
- No effective plugin or EditorID change.
- Stale selection, input, source, manifest, FormID, or proposal token.
- Path escape, parse failure, write failure, or post-write validation failure.

## Safety boundaries

- No binding add/delete/reorder, provider change, FormID edit/remove, xEdit
  binding mutation, plugin discovery, record lookup, or uniqueness claim.
- No unrelated quest/narrative/asset mutation.
- No GECK/xEdit launch, plugin read/write, game Data/MO2 write, game launch,
  runtime probe, network, release publication, or AI requirement.

## Gate 436 acceptance criteria

- Only quests with exactly one GECK binding are selectable and current values
  populate deterministically.
- Plugin-only, EditorID-only, and both-field revisions preserve FormID and xEdit
  references exactly.
- Preview writes zero bytes; apply validates and rebuilds handoff.
- No-op, missing/multiple binding, invalid plugin, blank EditorID, stale token,
  unsupported shape, write failure, and rollback receive focused coverage.
- Published automation revises both fields, verifies handoff, and proves
  repeated no-op refusal preserves quest SHA-256.
- Release build and all test suites pass.

## Next route

Gate 436: implement existing quest GECK binding revision, exact preservation,
Narrative Author controls, focused tests, handoff verification, and
published-app regression.
