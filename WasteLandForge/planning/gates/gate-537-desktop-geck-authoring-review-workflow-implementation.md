# Gate 537 - Desktop GECK Authoring Review Workflow Implementation

Status: Complete - bounded desktop workflow and synthetic verification proven
Phase: post-v0.1 desktop product value
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, ADR-013, R009, and
Gates 511-514, 521-522, 532-536

## Goal

Implement the Gate 536 desktop contract over the canonical Forge backend so a
user can review and generate GECK authoring plans, read-only observer bundles,
and sealed semantic verification reports without launching an external tool or
duplicating backend correctness in WPF.

## Evidence classification

- **Documented:** Gate 536 fixes the nested GECK route, four-phase readiness
  model, six canonical commands, digest-bound preview/apply flow, stale-state
  rules, automation identities, and external-execution boundary.
- **Documented:** Gates 521, 522, and 533 own plan generation, observer bundle
  production, raw-observation sealing, and semantic verification.
- **Observed:** the desktop service completes the checked-in synthetic plan,
  observer, and report workflow through the canonical CLI and leaves opaque
  synthetic plugin bytes unchanged.
- **Observed:** focused Windows tests, the release solution build, and all 912
  serial .NET tests pass after implementation.
- **Open:** real FNVEdit compatibility remains deferred by Gate 534. Installed
  application automation and publication proof remain Gate 538 work.

## Implementation

Added `GeckAuthoringReviewWorkspace` as a thin desktop orchestration service.
It constructs only the six Gate 536 commands, strictly validates their JSON
identity and safety evidence, checks project containment and file digests,
projects the four readiness phases, binds previews to current inputs and
safety declarations, re-previews before every write, and maps cancellation to
a non-verification outcome.

Extended the existing `GECK Handoff` route with two nested views:

1. `Authoring Plan & Verification`, selected by default, exposes plan,
   observer, observation-intake, and semantic-verification phases.
2. `Manual Handoff` preserves the existing handoff, task ledger, GECK launch,
   and MO2 request ownership and automation identities.

The authoring view exposes exact diagnostics and evidence paths/digests, marks
project changes, observation changes, focus changes, cancellation, and
untrusted bridge results stale, and provides explicit routes to Manual
Handoff, xEdit Audit, Project Outputs, and Validation. It states that FNVEdit
execution and script installation remain manual and unproven under Gate 534.

Project Outputs now includes a read-only GECK authoring plan and verification
lane rooted at `generated/geck-authoring-plan`. It discovers existing evidence
only and does not generate, verify, launch, or mutate anything.

## Test coverage

Focused synthetic Windows coverage proves:

- the complete plan, observer, and report workflow through the desktop service;
- dry-run no-write behavior and exact preview-before-write command sequencing;
- changed-input refusal before write;
- projection of `WF-GEN-016`, `WF-GEN-017`, and `WF-SEM-046`;
- refusal of malformed, unsafe, wrong-target, wrong-project, outside-project,
  and uncontained backend evidence;
- cancellation propagation;
- unchanged opaque synthetic plugin bytes;
- preserved GECK route identity and all Gate 536 automation IDs;
- the new read-only Project Outputs lane and existing GECK handoff coverage.

Console-capturing Windows tests share one non-parallel collection so the
in-process Forge backend remains deterministic.

## Validation

- `dotnet test tests/WastelandForge.WindowsTests/WastelandForge.WindowsTests.csproj -c Release --no-restore -m:1 --filter "FullyQualifiedName~GeckAuthoringReviewWorkspaceTests|FullyQualifiedName~ProjectOutputWorkspaceTests|FullyQualifiedName~GeckHandoff"` - 19 passed.
- `dotnet test tests/WastelandForge.WindowsTests/WastelandForge.WindowsTests.csproj -c Release --no-build --no-restore -m:1` - 163 passed.
- `dotnet build WastelandForge.sln -c Release --no-restore -m:1` - passed with no errors; only offline NuGet vulnerability-advisory warnings were emitted.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1` - 912 passed, 0 failed, 0 skipped.
- `git diff --check` - passed; line-ending conversion notices only.
- Protected-path audit - no Tales from the Age of Men, Age of Men, or overhaul path changed.

## Actions withheld

- No FNVEdit/xEdit, GECK, MO2, game, provider, or observer-script execution.
- No plugin mutation, game Data write, load-order change, profile mutation, or
  external-state operation.
- No publication, installer rebuild, installed UI Automation, signing,
  timestamping, release publication, remote, network, or AI action.
- No real compatibility claim and no change to the Gate 514 v0.1 candidate.

## Next route

Gate 538: publish and install the application, extend the existing installed UI
Automation regression with redistributable synthetic authoring evidence, and
prove the Gate 536 installed contract without launching GECK, FNVEdit/xEdit, or
MO2 and without changing opaque plugin bytes.
