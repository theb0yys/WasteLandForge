# Gate 446 - Narrative Author Change Journal And Undo Contract

Status: Defined
Phase: v0.1 desktop authoring safety
Decision base: ADR-007, ADR-009, ADR-010, ADR-011, Gates 415-445

## Goal

Define a bounded local journal for the most recent successful Narrative Author
source transaction and a preview-gated one-step undo operation. Undo restores
exact canonical source bytes only when the project still matches the recorded
post-change state.

Gate 446 defines behavior only. Gate 447 owns implementation.

## Research Classification

- **Documented:** ADR-007 makes version-controlled registry documents canonical
  truth; local UI state, generated output, and reports are not canonical truth.
- **Documented:** current authoring transactions capture original bytes, write
  proposed source, validate, and restore originals on failure.
- **Documented:** several transactions affect one file, while narrative source,
  extension, and voice work-item transactions may affect multiple files.
- **Documented:** ADR-009 requires deterministic provenance and constrains clean
  operations to owned non-canonical output.
- **Inferred:** retaining one validated pre/post byte set locally extends the
  existing rollback pattern into a recoverable user action without creating a
  new canonical format.
- **Open:** repository history, arbitrary multi-level undo, merge conflict
  resolution, and undo of external GECK/xEdit/plugin edits remain excluded.

## Ownership And Storage

Journal data is private app state under:

`%LOCALAPPDATA%/WastelandForge/authoring-journal/<project-key>/`

`project-key` is a deterministic SHA-256 digest of the normalized absolute
project path. The directory contains one metadata document and exact before/after
byte snapshots for only the files changed by the latest successful transaction.

Journal files are not project source, generated project output, build evidence,
release provenance, or files intended for version control. They may contain
authored content and must not be logged, uploaded, included in packages, or
exposed to other projects. Normal app uninstall does not promise removal of
user-local journal data unless the installer explicitly owns that cleanup.

## Journal Entry

The single current entry records:

- journal format version;
- normalized project root and project-key;
- transaction ID, workflow name, operation summary, and UTC completion time;
- each project-relative canonical path in ordinal order;
- SHA-256 and byte length for exact before and after content; and
- the corresponding before and after snapshot filenames.

Paths must resolve beneath the recorded project root and must be files explicitly
owned by the supported Narrative Author transaction. Absolute paths, traversal,
symlinks escaping the project, generated/dist paths, plugin binaries, game Data,
and arbitrary user-selected files are refused.

## Transaction Lifecycle

1. Before source writes, stage original bytes and proposed after bytes in a
   temporary journal directory.
2. Verify the existing preview token and source preconditions.
3. Apply all source changes and run the existing shared project validation.
4. On failure, restore original bytes and delete the staged journal.
5. On success, atomically replace the prior one-step entry with the staged entry.
6. Continue the existing optional backend validation/handoff presentation; a
   later handoff failure does not invalidate the successful source journal.

A new successful source transaction replaces the previous undo entry. Preview,
load, explorer routing, inventory refresh, backend-only commands, generation,
packaging, and failed/refused transactions do not create or replace an entry.

Startup removes incomplete temporary journal directories. It must never infer
that a prepared but uncommitted entry represents a successful source change.

## Undo Availability

Undo is available only when:

- the selected project root matches the journal project root exactly after path
  normalization;
- every recorded file exists or absence is explicitly represented by the entry;
- every current file hash and length equals its recorded after state;
- no recorded path escapes the selected project; and
- shared project validation currently passes.

Any manual edit, repository operation, external tool change, missing file,
unexpected file, hash mismatch, corrupt metadata, or unsupported journal version
blocks undo. The refusal names affected project-relative paths without exposing
snapshot contents.

## Preview-Gated Undo

`Review undo` is read-only and displays the workflow, summary, completion time,
affected paths, and before/after hashes. It creates a short-lived token over the
journal metadata, snapshot bytes, selected project path, and current source
bytes.

`Undo last change` is enabled only after a successful review. Apply recomputes
the token and all preconditions. Changed journal/source state requires another
review. No generic confirmation dialog substitutes for this preview.

## Undo Apply

Undo stages current after-state bytes, restores all recorded before states, and
runs shared project validation. Files recorded as absent before the transaction
are removed only when their current bytes match the recorded created-file after
state. Parent directories created solely for those files may be removed only if
empty and beneath the project root.

If write or validation fails, the operation restores the complete recorded
after state. Successful undo:

- consumes the journal entry; there is no redo in this slice;
- marks inventory and explorer state `Stale`;
- leaves generated handoff/package output untouched and visibly reports that it
  may need rebuilding; and
- writes no replacement journal entry for the undo itself.

## Desktop Interaction

- Show a compact `Last change` summary near the fixed Narrative Author inventory
  header, with `Review undo` and `Undo last change` commands.
- Commands are disabled for no entry, wrong project, corrupt/unsupported entry,
  stale source, or busy authoring state.
- Render review/refusal/result details in the existing persistent output pane.
- Project/workflow switching must not consume or silently replace a journal.
- The UI must remain usable at 1366x768 and 1920x1080 without hiding inventory,
  explorer, workflow status, or undo state.

## Safety Boundaries

- One entry and one undo only; no history browser, redo, branching, or merges.
- No schema or canonical source contract changes.
- No undo for generated files, handoffs, packages, archives, plugin records,
  GECK/xEdit operations, game files, MO2 state, or external tools.
- No network, cloud sync, telemetry, AI, signing, release publication, or remote
  backup behavior.
- Journal cleanup may target only the verified project-key directory under the
  app-owned LocalAppData journal root.

## Gate 447 Acceptance

Gate 447 must prove:

1. one-file, multi-file, and created-file transactions journal exact bytes;
2. failed/refused transactions create no entry and preserve any prior entry;
3. a new successful transaction replaces the previous entry atomically;
4. review writes zero project bytes and token changes on source/journal changes;
5. intervening edits and path escapes block undo without modifying source;
6. successful undo restores exact bytes, validates, consumes the entry, and
   marks inventory/explorer stale;
7. failed undo restores the complete after state and retains actionable evidence;
8. generated and distribution trees remain untouched;
9. published UI regression completes authoring, review, undo, and exact-byte
   comparison at both target viewport sizes; and
10. restart recovery preserves a committed entry and removes incomplete staged
    journal state without touching project source.

## Stop Point

Gate 446 stops at this contract. No journal directory, product code, source
data, schema, validator, generator, CLI/backend, package, installer, GECK, or
game-facing behavior changes.

Next route: Gate 447 - implement the local one-entry Narrative Author journal,
preview-gated undo, transactional restoration, focused tests, and published-app
regression.
