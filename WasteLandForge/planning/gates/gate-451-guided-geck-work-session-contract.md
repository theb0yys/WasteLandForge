# Gate 451 - Guided GECK Work-Session Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, Gates 399-400, 449-450

## Goal

Define one implementation-ready desktop slice that turns the read-only GECK
handoff viewer into a useful guided human work session while preserving the
boundary that GECK owns plugin records and Forge source remains canonical.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| GECK remains the authority for plugin records and Forge must not replace raw record editing. | Documented | FNV Asset Pipeline report / ADR-004 |
| Generated outputs are disposable, non-canonical, and provenance-bearing. | Documented | Generator and Build Pipeline report / ADR-009 |
| Build freshness should be based on cryptographic input digests rather than timestamps alone. | Documented | Generator and Build Pipeline report |
| Environment and application state must remain separate from canonical source and generated output. | Documented | Developer Experience report / ADR-010 |
| A private completion ledger can improve operator continuity if it never changes validation or generated evidence. | Inferred | ADR-004, ADR-009, Gate 449 boundary |
| Automatic GECK action execution and authoritative proof of in-plugin completion remain unsupported. | Open | Current research and Gate 399 unresolved-action model |

## Workspace contract

The existing `GECK Handoff` tab becomes a guided session workspace with four
coherent areas:

1. Summary: fresh/stale/invalid handoff state and pending/completed counts.
2. Filters: text search, category selector, and status selector.
3. Task list and selected-task detail: action, owner, reason, source reference,
   and explicit local completion controls.
4. Provenance: source digests, safety evidence, output folder, and worklist.

No additional top-level CLI command or slash alias is introduced.

## Freshness contract

Before enabling session controls, the desktop must:

- require `handoff-manifest.json`, `build-manifest.json`, `checksums.sha256`, and
  `worklists/unresolved-actions.tsv` under the exact contained handoff root;
- require every safety flag to be `false`;
- verify every manifest source path resolves inside the selected project and
  matches its recorded SHA-256 and byte length;
- verify the unresolved-action worklist digest against generated manifest or
  checksum evidence;
- classify the handoff as `fresh`, `stale`, or `invalid`;
- allow review of stale evidence but disable completion mutation until rebuilt;
- refuse invalid, escaped, malformed, or unsafe evidence entirely.

Timestamp comparison is informational only and cannot establish freshness.

## Local completion ledger

Completion state is private desktop state stored under:

```text
%LOCALAPPDATA%/WastelandForge/GeckSessions/
  <project-fingerprint>/<handoff-manifest-sha256>.json
```

The project fingerprint is SHA-256 over the normalized absolute project path.
The handoff key is SHA-256 over the exact manifest bytes. The versioned ledger
contains only:

```text
version
projectFingerprint
handoffSha256
tasks[]:
  actionId
  state: pending | completed
  changedUtc
```

Rules:

- new tasks default to `pending` without requiring a ledger write;
- `Mark complete` and `Reopen` are explicit selected-task actions;
- writes use a same-directory temporary file and atomic replacement;
- unknown, duplicate, or missing action IDs make the ledger invalid;
- a rebuilt handoff gets a new ledger key and never inherits completion;
- old ledgers may be listed for cleanup but are never merged automatically;
- deleting the ledger loses only local progress, never project or plugin data;
- ledger state never changes validation, generation, package manifests,
  checksums, release evidence, or canonical registries.

No free-form notes, plugin FormIDs, script text, credentials, or game paths are
stored in v0.1.

## Filtering and detail

- Search matches action ID, owner ID, category, required action, reason, and
  source file using ordinal case-insensitive comparison.
- Category options derive from the loaded worklist plus `All categories`.
- Status options are `All`, `Pending`, and `Completed`.
- Filters compose with logical AND and never mutate task order.
- Clearing filters restores canonical worklist order.
- Selecting a task exposes full untruncated details and its source reference.
- The UI shows visible/total and completed/total counters without cards nested
  inside cards or layout-shifting controls.

## Safety and refusal behavior

- No GECK/xEdit launch, plugin mutation, record creation, script compilation,
  game Data write, MO2 write, external-tool execution, runtime probe, network,
  release publication, or AI behavior.
- Folder/file opening remains constrained to existing paths under the exact
  generated handoff root.
- A ledger path is derived internally; users cannot redirect it.
- Ledger write failure leaves the previous valid ledger intact and reports the
  failure without changing the visible task state.
- Source drift immediately disables completion controls after refresh.

## Acceptance criteria for Gate 452

- Fresh ExampleMod handoff loads 23 tasks and three source records.
- Source byte drift produces `stale`, names the mismatched source, and disables
  completion mutation while retaining review access.
- Worklist or safety tampering produces `invalid` and disables all handoff
  actions.
- Search/category/status filters compose and preserve canonical ordering.
- Complete/reopen survives application restart for the exact handoff.
- Rebuilding to a different manifest digest starts a separate pending session.
- Interrupted or failed ledger replacement preserves the prior valid ledger.
- Tests cover containment, hashes, malformed ledgers, task identity, filters,
  atomic writes, stale-session isolation, and no source/generated mutations.
- Focused Windows tests, full suite, and published-app desktop regression pass.

## Next route

Gate 452: implement the complete guided GECK work-session slice in the desktop,
including digest-backed freshness, composable filters, selected-task detail,
atomic LocalAppData completion state, tests, and published-app regression.
