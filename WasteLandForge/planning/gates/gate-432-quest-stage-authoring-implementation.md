# Gate 432 - Quest Stage Authoring Implementation

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 431, ADR-003, ADR-004, ADR-007, ADR-009, ADR-011

## Goal

Implement preview-gated standalone stage authoring with explicit unique
non-negative numbering and optional presentation.

## Implemented

- Added source-backed quest selection and `<questId>.stage.<slug>` identity.
- Added explicit integer parsing, non-negative and per-quest uniqueness checks,
  optional title/summary, duplicate refusal, source-bound preview, rollback
  validation, handoff rebuild, and Narrative Author controls.
- Added minimal, presentation-populated, and valid missing-stage-array coverage.
- The missing-stage fixture removes dependent quest/dialogue references rather
  than bypassing semantic validation.

## Published regression

- Rebuilt the standalone backend distribution and published the WPF app.
- Appended a titled stage through the published executable in isolated writable
  LocalAppData.
- Verified exact `quest-stages.tsv` output and no authored result intent.
- Repeated preview refused the duplicate, preserved quest SHA-256, and the app
  remained responsive.
- Removed the isolated test directory.

## Validation

- Windows test build passed.
- Focused stage tests passed: 4 tests.
- Full solution passed: 741 tests, zero failures and zero skips.
- Desktop publication and published GUI regression passed.
- The standalone helper produced and verified `dist/local/forge` but left a
  nonzero PowerShell exit code; desktop publication was therefore run
  separately and passed.
- NuGet audit lookup produced NU1900 warnings because `api.nuget.org` was
  unavailable; compilation and tests completed.

## Boundaries

- No stage execution, completion, condition, flag, transition, result intent,
  script, GECK mapping, plugin mutation, external tool launch, game Data/MO2
  write, network correctness dependency, release publication, or AI requirement.

## Next route

Gate 433: define source-backed quest GECK external-reference binding authoring
for existing plugin/editor identities, with strict preservation and no plugin
record creation, lookup, or mutation.
