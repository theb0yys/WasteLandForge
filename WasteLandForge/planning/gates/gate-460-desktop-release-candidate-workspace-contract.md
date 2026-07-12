# Gate 460 - Desktop Release Candidate Workspace Contract

Status: Complete
Phase: v0.1 desktop release-candidate workflow definition
Decision base: ADR-004, ADR-005, ADR-009, ADR-010, ADR-011, Gates 301-307,
388, 455, 458, and 459

## Purpose

Define one desktop workflow that answers a concrete operator question: can the
selected Forge project produce a locally verifiable release candidate now?
The workspace composes existing backend behavior; it does not duplicate
validation, package, or release policy in WPF.

## Documented boundaries

- Canonical project truth remains in repository source contracts.
- The desktop invokes canonical `forge validate`, `forge package`, and
  `forge release verify` behavior through the bundled backend.
- Generated package and release evidence remains disposable, digest-backed, and
  contained under project output trees.
- Human-authored ESP/ESM bytes remain opaque. Forge reports their review status
  and provenance but does not parse, mutate, execute, or approve them.
- This workflow is local and offline-first. It does not run `release prepare` or
  `release publish`, contact remote services, install providers, or automate
  GECK, xEdit, MO2, or the game.

## Inferred product contract

The desktop adds a dedicated **Release Candidate** workspace for the selected
project. Its primary action is **Run release candidate check**.

One invocation runs these stages in order:

1. Validate the project.
2. Build the combined `mod-package` target.
3. Run release verification.

A blocking exit from a stage stops the sequence. Later stages are shown as not
run, never as successful or inferred. Non-blocking package warnings remain
visible when release verification proceeds.

The workspace states are `Not run`, `Running`, `Blocked`, `Candidate ready`,
`Cancelled`, and `Stale`. Candidate readiness is a projection of the latest
successful backend run, not canonical project state.

## Presented evidence

The result view must show:

- each canonical command, stage status, exit code, duration, and diagnostic
  counts;
- exact blocking diagnostic identifiers and messages without desktop-side
  reinterpretation;
- the contained package path and release-evidence root reported by the backend;
- packaged plugin data paths, SHA-256 digests, review state, review-evidence
  digest, and report-snapshot digest when present;
- the build/package manifest and release-verification evidence used for the
  decision.

The desktop reads structured backend output and evidence contracts. It must not
scrape human console text or implement an independent release policy.

## Freshness and containment

The result binds to the selected project, backend version, source/evidence
inputs, and generated manifest digests. A project change, plugin review change,
backend change, or selected-project change marks the displayed result `Stale`
and disables the candidate-ready claim until rerun.

Open actions are limited to existing paths resolved inside the selected
project's generated/distribution roots:

- Open package folder.
- Open package archive.
- Open release evidence folder.
- Open release handoff summary.

Missing, stale, malformed, or escaping paths disable the relevant action and
produce a visible diagnostic. No open action creates or repairs evidence.

## Interaction requirements

- Only one release-candidate run may be active.
- The run can be cancelled between backend stages and while a backend process
  is cancellable.
- Project selection and destructive/project-mutating controls are disabled
  while the sequence is active.
- Output is retained after failure so the operator can inspect the reason.
- Rerun replaces the projection atomically after the new run completes; a
  failed partial refresh must not present mixed old/new readiness.

## Gate 461 acceptance criteria

Gate 461 must implement the complete workspace and prove:

- a valid synthetic project with a digest-bound reviewed plugin reaches
  `Candidate ready` through all three stages;
- invalid source blocks at validation and does not run package or release;
- package failure prevents release verification;
- a pending plugin may package with its warning but blocks release verification
  with the exact policy diagnostic;
- tampered review evidence or report snapshots cannot produce readiness;
- plugin provenance shown in the desktop matches package/release evidence;
- changed project/evidence inputs mark prior results stale;
- missing or escaping output paths cannot be opened;
- cancellation and repeated runs do not leave mixed state;
- focused unit/integration tests, the full test suite, published desktop smoke,
  and an installed regression pass.

All fixtures must remain synthetic and redistributable. The installed
regression must clean up its isolated install, project, and test-output roots.

## Open implementation details

- The existing desktop/backend bridge may need a typed orchestration result,
  but the backend command contracts remain authoritative.
- Exact WPF control composition may follow current app-shell patterns provided
  all required states and evidence are accessible without an advanced-log view.

## Next route

Gate 461: implement and verify the end-to-end desktop Release Candidate
workspace defined here, including published and installed regression coverage.
