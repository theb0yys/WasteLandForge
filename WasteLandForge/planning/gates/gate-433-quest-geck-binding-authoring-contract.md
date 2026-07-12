# Gate 433 - Quest GECK Binding Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-004, ADR-007, ADR-009, Gate 19, Gates 399-432

## Goal

Define preview-gated authoring of one GECK external-reference binding on an
existing source-backed quest, using an explicitly supplied plugin filename and
EditorID without looking up, creating, or mutating any plugin record.

## Research classification

- **Documented:** Quest schema `0.6.0` supports `externalRefs` with `geck` or
  `xedit` provider, required plugin, optional EditorID, and optional FormID.
- **Documented:** Plugin names must be path-free `.esm` or `.esp` filenames.
- **Documented:** GECK handoff reads the first GECK reference and exposes plugin
  and EditorID in `worklists/quests.tsv`.
- **Documented:** GECK remains the quest-record authority; Forge owns canonical
  source and deterministic handoff generation, not raw plugin editing.
- **Documented:** Existing handoff quest rows retain a human
  `create-or-verify` action even when a binding is declared.
- **Inferred:** Requiring an EditorID in this desktop workflow provides a useful
  record binding while remaining stricter than the schema's optional field.
- **Inferred:** Refusing a second GECK reference avoids ambiguous selection by
  the current first-match handoff behavior.
- **Open:** Record existence, EditorID uniqueness, plugin load order, masters,
  FormID resolution, record type verification, and live GECK/xEdit lookup remain
  unresolved without external-tool evidence.

## Supported source shape

The workflow supports canonical JSON projects using quest schema `0.6.0`, the
exact manifest quest-registry path `src/registries/quests/`, and exactly one
`src/registries/quests/main.json`. The project must pass canonical validation
before choices or preview. Unsupported paths, YAML, multiple quest documents,
and schema migration are refused rather than rewritten.

## Desktop workflow

Narrative Author gains an `Add GECK Binding` workflow with:

- source-backed quest selector;
- required plugin filename;
- required trimmed EditorID.

The user previews the exact external-reference declaration and complete
proposed quest JSON, then appends, validates, and rebuilds the GECK handoff with
an unchanged token. Inputs are declarations supplied by the author; Forge does
not browse plugins or claim they exist.

## Authored declaration

```json
{
  "provider": "geck",
  "plugin": "ExampleMod.esm",
  "editorId": "WFIntroQuest"
}
```

Provider is fixed to `geck`. FormID is omitted. Plugin and EditorID are not
derived from quest IDs, project IDs, filenames elsewhere in the project, or
external tools.

## Preservation and transaction

- Parse and deep-clone quest source through structured JSON APIs.
- Append one object only to the selected quest's `externalRefs` array, creating
  that optional array only when absent.
- Preserve existing GECK-independent references, including xEdit references,
  in their original order and content.
- Preserve every other existing quest field and every non-selected quest by
  deep equality.
- Preview and refusals write zero bytes.
- Bind the token to normalized inputs, selected quest ID, manifest/quest bytes,
  and exact proposed quest bytes.
- Append re-runs preview, verifies the token, writes the proposal, validates,
  and restores original bytes after write or validation failure.
- Successful validation rebuilds `geck-handoff`; package failure retains valid
  canonical source and reports a retryable output failure.

## Handoff behavior

The selected quest row in `worklists/quests.tsv` must contain the exact plugin
and EditorID plus the existing `create-or-verify` action. Manifest, checksums,
and build manifest must cover the regenerated worklist deterministically. A
binding does not remove human plugin verification or project-level plugin
review actions.

## Refusals

- Unsupported canonical shape or failed pre-validation.
- Selected quest no longer exists.
- Selected quest already contains any `provider: geck` reference.
- Blank plugin or EditorID.
- Plugin contains `/` or `\`, or does not end in `.esm`/`.esp` under the schema
  case rules.
- Stale selection, input, source, manifest, or proposal token.
- Path escape, parse failure, write failure, or post-write validation failure.

Existing xEdit references do not cause refusal and must remain unchanged.

## Safety boundaries

- No binding edit/delete/reorder, xEdit binding authoring, FormID authoring,
  plugin discovery, record lookup, EditorID uniqueness claim, master/load-order
  inference, or plugin existence claim.
- No quest, stage, objective, transition, condition, variable, result intent,
  dialogue, asset, or existing-object mutation outside the one append.
- No GECK/xEdit launch, plugin read/write, game Data/MO2 write, game launch,
  runtime probe, network, release publication, or AI requirement.

## Gate 434 acceptance criteria

- Quest choices load deterministically.
- Missing-array and existing-xEdit-array modes append exactly one GECK binding
  while preserving all prior source.
- Preview writes zero bytes and shows the exact declaration and proposed quest.
- Append validates, rebuilds handoff, and emits exact plugin/EditorID/action.
- Existing GECK binding, invalid plugin, blank EditorID, stale token, missing
  quest, unsupported shape, write failure, and rollback receive focused tests.
- Published desktop automation appends one binding to an isolated source copy,
  verifies the quest handoff row, and proves repeated refusal preserves quest
  SHA-256.
- Release build and all test suites pass.

## Next route

Gate 434: implement source-backed quest GECK binding authoring, Narrative Author
controls, transaction/refusal coverage, exact handoff verification, and
published-app regression.
