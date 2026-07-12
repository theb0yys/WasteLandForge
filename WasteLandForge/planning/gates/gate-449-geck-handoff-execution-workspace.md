# Gate 449 - GECK Handoff Execution Workspace

Status: Complete
Phase: v0.1 desktop implementation
Decision base: ADR-004, ADR-009, Gates 399-401, Gate 448

## Goal

Expose a read-only desktop workspace for the human side of a generated GECK
handoff: inspect manual tasks, task status, source provenance, safety evidence,
and exact output paths without launching GECK or changing canonical source or
plugin records.

## Delivered

- Dedicated `GECK Handoff` desktop tab.
- Strict reader for generated `handoff-manifest.json` and
  `worklists/unresolved-actions.tsv` under `dist/geck-handoff`.
- Read-only manual-task table with status, category, owner, action, reason, and
  source file.
- Source provenance table with path, SHA-256, and byte length.
- Safety evidence display that refuses any enabled execution or mutation flag.
- Exact contained output-folder and task-worklist opening actions.
- Focused valid, missing, incomplete, and unsafe evidence tests.

## Boundaries

- Completion state is projected from generated blocking metadata and is not
  persisted by the desktop.
- No GECK/xEdit launch, source/plugin mutation, script compilation, game Data
  or MO2 write, external-tool execution, or AI behavior was added.
- The desktop consumes backend evidence and does not parse canonical registries.

## Validation

- Focused Windows tests: 2 passed.
- Full release suite: 759 passed, zero failures, zero skips.
- Release WPF build and XAML event wiring passed.
- NuGet emitted `NU1900` because api.nuget.org vulnerability metadata was
  unavailable; cached dependencies compiled successfully.

## Next route

Gate 450: rebuild the unsigned installer and run an isolated installed-app GECK
handoff workspace regression covering task/provenance visibility, safety
refusal, exact output actions, responsiveness, uninstall, and cleanup.
