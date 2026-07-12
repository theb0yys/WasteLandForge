# Gate 478 - Local Release Handoff Verification Closeout

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gate 477

## Goal

Close the local release handoff lane with a read-only audit of an existing
handed-off archive against its checksum and structured evidence.

## Delivered

- Added verification that operates from an existing handoff folder without a
  project, Candidate run, regeneration, or network access.
- Requires exactly one top-level handoff evidence file and validates its format
  version, kind, exported status, archive metadata, and false safety flags.
- Resolves only a safe sibling ZIP and checksum filename, refusing path-bearing
  archive names and reparse-point folders or files.
- Recomputes archive length and SHA-256 and requires the checksum sidecar to
  exactly match the evidence and deterministic checksum line.
- Added a desktop `Verify Existing` action and verified archive evidence view.
- Preserved read-only behavior: verification creates, modifies, and deletes no
  handoff files.
- Added success/no-write and archive/checksum/evidence tamper coverage.

## Verification

- Focused local handoff suite: 6 tests passed.
- Full solution: 804 tests passed serially with MSBuild node reuse disabled.
- App shell and backend republished; unsigned Inno Setup installer rebuilt.
- Installed regression created a synthetic handoff and verified it through the
  installed desktop control before uninstall and isolated cleanup.

## Boundaries

Verification establishes only internal consistency between the local archive,
checksum, and Forge handoff evidence. It does not establish authorship, signing,
timestamping, malware safety, third-party manager compatibility, game runtime
correctness, or remote publication. It does not mutate source, packages,
plugins, game Data, manager profiles, or load order.

## Lane closeout

The v0.1 local release handoff lane is complete: Forge can prepare a FOMOD-backed
Candidate, preview and create a versioned local handoff, and later verify its
local integrity. Further release edge cases are deferred until real use exposes
a concrete need.

## Next route

Gate 479: research and define a safe explicit external-tool launch contract for
existing GECK handoff work, beginning with GECK executable selection, preview,
and user-authorized launch while stopping before record editing automation,
plugin mutation, MO2 profile changes, or game launch.
