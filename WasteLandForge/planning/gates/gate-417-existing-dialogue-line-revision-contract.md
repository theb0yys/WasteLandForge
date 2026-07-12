# Gate 417 - Existing Dialogue Line Revision Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, Gates 405-416

## Goal

Define a preview-gated Narrative Author edit that revises one existing dialogue
line's response text, optional speaker, optional prompt text, and optional
priority without changing the line's identity, references, gameplay intent,
voice declaration, or branching structure.

## Research classification

- **Documented:** Dialogue is a gameplay state query and transition surface;
  presentation fields must not be treated as authority for gameplay state.
- **Documented:** Dialogue schema `0.23.0` requires non-empty `responseText`,
  allows optional non-empty `speaker` and `promptText`, and allows integer
  `priority`.
- **Documented:** A line with `promptText` must also declare `priority`.
- **Documented:** Semantic validation rejects duplicate prompt routes sharing
  topic, prompt text, and priority.
- **Documented:** Gate 400 emits these fields to
  `worklists/dialogue-lines.tsv`; GECK remains the dialogue-record authority.
- **Inferred:** Revising presentation fields in one guarded transaction is the
  smallest useful correction workflow after source creation and append.
- **Open:** Exact GECK priority ranges, speaker form resolution, alternate sound
  handling, record mapping, and runtime dialogue selection remain unresolved.

## Supported source shape

The workflow supports the bounded canonical JSON shape used by existing
Narrative Author edits:

```text
src/registries/quests/main.json    schema 0.6.0
src/registries/dialogue/main.json  schema 0.23.0
```

The manifest must declare exact canonical quest/dialogue registry directories,
each directory must contain only `main.json`, and canonical project validation
must pass before choices load or preview begins.

## Desktop workflow

Narrative Author gains a `Revise Dialogue Line` workflow. `Load Dialogue Lines`
provides deterministic source-backed line choices and populates editable fields
from the selected line:

- required response text;
- optional speaker;
- optional prompt text;
- optional integer priority.

Blank speaker, prompt, or priority means remove that optional property. A
non-empty prompt requires a priority. Priority may remain present without a
prompt because schema `0.23.0` permits that shape and its exact GECK semantics
remain open.

The user selects a line, edits values, previews the complete proposed dialogue
JSON and exact before/after field summary, then applies, validates, and rebuilds
the GECK handoff with an unchanged preview token.

## Exact mutation

On only the selected line:

- replace `responseText` with trimmed non-empty authored text;
- set `speaker` to trimmed authored text, or remove it when blank;
- set `promptText` to trimmed authored text, or remove it when blank;
- set `priority` to the authored JSON integer, or remove it when blank.

No-op revisions are refused. Forge does not alter or derive any other property.

## Preservation and transaction

- Parse and deep-clone the dialogue root with structured JSON APIs.
- Read quest source for bounded-shape and validation authority only.
- Preserve the selected line's `id`, `questId`, `topicId`, speech challenge,
  gates, conditions, condition logic, result scripts, links, response routes,
  voice, tags, and any other schema-valid property by deep equality.
- Preserve every non-selected topic and line by deep equality and retain array
  ordering.
- Preview and every refusal write zero bytes.
- Bind normalized inputs, selected line ID, manifest/quest/dialogue bytes, and
  exact proposed dialogue bytes into the preview token.
- Apply re-runs preview, compares the token, retains original dialogue bytes,
  writes the proposal, and runs canonical validation.
- Write or validation failure restores the original bytes.
- Successful validation rebuilds `forge package . --target geck-handoff`.
  Packaging failure retains valid canonical source and reports retryable output
  failure.

## Refusals

- Unsupported path, filename, schema, document count, parse state, or failed
  canonical pre-validation.
- Selected line no longer exists.
- Blank response text, non-integer priority, or prompt without priority.
- Proposed topic/prompt/priority combination conflicts with another line under
  existing semantic validation.
- No effective field change.
- Stale selection/input/source/manifest token, path escape, write failure, or
  post-write validation failure.

## Safety boundaries

- No line identity, quest/topic reference, gameplay condition, result intent,
  branch, voice, tag, or topic mutation.
- No bulk replacement, raw JSON editing, schema migration, EditorID/FormID
  invention, GECK mapping, executable script generation, or plugin mutation.
- No GECK/xEdit launch, game Data/MO2 write, game launch, runtime probe,
  network, release publication, or AI requirement.

## Gate 418 acceptance criteria

- Source-backed choices populate exact current values and reload on selection.
- Preview writes zero bytes and shows exact proposed source plus field changes.
- Apply changes only the four supported fields, validates, and rebuilds handoff.
- Handoff `dialogue-lines.tsv` contains exact revised presentation values.
- Set, replace, and clear optional-field cases pass.
- Blank response, prompt without priority, invalid priority, semantic duplicate,
  no-op, stale token, unsupported shape, write failure, and validation rollback
  receive focused Windows coverage.
- Published desktop automation revises one line, verifies handoff output, and
  proves repeated no-op refusal preserves dialogue SHA-256.
- Release build and all test suites pass.

## Next route

Gate 418: implement the dialogue-line revision loader, exact-field preview and
transaction engine, Narrative Author controls, canonical validation and handoff
rebuild, focused tests, and published-app regression.
