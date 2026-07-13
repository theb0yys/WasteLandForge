# Gate 509 - Plugin Mod Workbench and Revised Plugin Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 508, ADR-003, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Goal

Implement an evidence-derived seven-phase desktop workbench and the missing
safe iteration path for replacing a project-owned opaque plugin revision while
resetting digest-bound review and downstream readiness.

## Implemented

- Added a first-class **Plugin Mod Workbench** desktop tab with seven ordered
  phases: narrative, GECK handoff, human GECK session, plugin artifact, xEdit
  review, FOMOD, and Candidate readiness.
- Derives phase state from existing typed Narrative inventory, GECK handoff,
  local task ledger, opaque plugin registry, Project Outputs, and Candidate
  evidence rather than introducing another canonical workflow file.
- Supports explicit primary-plugin selection and exposes exact phase state,
  action, evidence, plugin digest, review limitation, and advisory GECK progress.
- Routes selected phases into the existing Narrative Author, GECK Handoff,
  Plugin Intake/xEdit review, Project Outputs, and Candidate workspaces.
- Added preview-gated revised-plugin intake with exact artifact identity,
  basename/type checks, old/new length and SHA-256, same-digest refusal, source
  drift refusal, reparse refusal, and review-reset disclosure.
- Revision apply copies to a same-directory temporary file, verifies copied
  bytes, atomically replaces only the project-owned opaque artifact, updates
  registry digest/length, sets review to pending, detaches review evidence, and
  restores exact plugin/registry bytes on failure.
- Previous review report/evidence files remain untouched and unreferenced.
- Review promotion now chooses deterministic plugin-digest-suffixed report and
  evidence paths when prior revision evidence exists, allowing a revised plugin
  to be reviewed again without overwriting audit history.
- Successful revision clears cached Candidate state and refreshes all workbench
  phases.

## Evidence classification

- **Documented:** Forge may validate, package, and govern exact opaque plugin
  bytes while GECK/xEdit retain ownership of plugin record editing and review.
- **Documented:** local GECK task completion is advisory state and cannot prove
  plugin contents.
- **Inferred:** exact-byte replacement plus mandatory review reset is the
  minimum safe iteration model for a human-authored plugin.
- **Open:** Forge still cannot prove narrative source and plugin records are
  semantically equivalent.

## Validation

- Revised-plugin and workbench focused tests: 3 passed.
- Combined plugin review/revision focused tests: 5 passed.
- Full solution suite: 872 passed, zero failed, zero skipped.
- Coverage proves reviewed -> revised/pending -> reviewed-again, distinct
  digest-specific evidence, unchanged prior evidence, same/renamed/stale source
  refusal, exact revised bytes, phase derivation, and advisory GECK state.
- Release backend/app publication passed; only offline NuGet vulnerability-feed
  warnings (`NU1900`) were reported.
- Unsigned Inno Setup installer rebuilt successfully with Inno Setup 6.7.3.
- Installed UI Automation began with an existing reviewed Candidate-ready
  synthetic plugin, revised it through the Plugin Mod Workbench, verified exact
  selected bytes, confirmed review reset to pending, confirmed prior evidence
  remained byte-identical, and confirmed cached Candidate readiness was cleared.
- The complete installed regression, uninstall, and isolated cleanup passed.

## Boundaries preserved

- Forge never parses or modifies plugin records; revision replaces the entire
  project-owned opaque file only after explicit digest-bound preview.
- External GECK output remains unchanged.
- No game Data, MO2 profile/load order, GECK/xEdit automation, game launch,
  network operation, remote publication, signing, or AI behavior was added.
- Workbench phase state is derived UI evidence, not canonical truth.

## Remaining workbench limits

- The phase action currently routes to the authoritative specialist workspace;
  it does not execute package or Candidate commands inline.
- FOMOD phase projection uses existing output presence plus current in-memory
  Candidate staleness; independent per-refresh package-manifest/source digest
  verification should be tightened before lane closeout.
- Primary selection is session-local and is not yet stored as a LocalAppData UI
  preference.

## Next route

Gate 510: harden Plugin Mod Workbench freshness with independent package/plugin
evidence verification, add cancellable direct build/check actions where they
reuse existing services safely, persist only primary-plugin UI preference, and
close the workbench lane with installed refresh/restart regression.
