# Gate 118 - MCM Extender Install-Plan Verify-Existing Schema Revalidation

Status: Complete

## Purpose

Gate 118 strengthens existing package evidence verification for the MCM
Extender JSON slice by revalidating existing `install-plan.json` against the
embedded `install-plan/0.1.0` schema in
`forge package --target mcm-json --verify-existing`.

The gate keeps install-plan evidence as local export planning only. It does
not turn Forge into an installer and does not mutate a game `Data` folder,
MO2 profile, or runtime state.

## Research grounding

- Documented: ADR-007 makes versioned JSON Schema Draft 2020-12 validation
  part of the canonical contract model.
- Documented: ADR-009 requires generated outputs to remain disposable,
  deterministic, and traceable through local manifests.
- Documented: ADR-010 keeps the command surface stable and routes existing
  package evidence checks through `forge package --target mcm-json
  --verify-existing`.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed tests.
- Inferred: Because Gate 116 introduced `install-plan/0.1.0` and Gate 117
  revalidates install-plan content, verify-existing mode should reject
  existing install-plan files that no longer match the published install-plan
  schema before running deeper content checks.

## Implemented

Gate 118 implements:

- embedded `install-plan/0.1.0` schema loading in the file-based package
  evidence verifier,
- local schema-registry isolation so repeated schema loads do not collide with
  other JsonSchema.Net schema instances in the same process,
- schema-first validation of existing `install-plan.json` in
  `forge package --target mcm-json --verify-existing`,
- one coalesced `WF-BUILD-006` install-plan schema diagnostic per invalid
  install-plan document,
- cascade control so deeper install-plan JSON and Markdown content checks run
  only after the existing install-plan document passes schema validation,
- focused unit coverage for edited install-plan schema shape,
- golden CLI coverage for stale install-plan schema evidence through
  `forge package --target mcm-json --verify-existing`.

## Not implemented

Gate 118 does not implement:

- package-manifest schema validation in verify-existing mode,
- install-preview schema validation in verify-existing mode,
- package-verification schema validation in verify-existing mode,
- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- inspecting MO2 VFS conflicts,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Verifier schema checks | Complete | Existing install-plan JSON is checked against embedded `install-plan/0.1.0`. |
| CLI coverage | Complete | Golden tests exercise edited install-plan schema evidence through the real CLI. |
| Unit coverage | Complete | Focused verifier tests cover schema mismatch and preserve schema-valid semantic drift coverage. |
| Command surface | Complete | No new command or alias was added. |
| Install behavior | Not implemented | Forge still produces and verifies reports only and does not install files. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmPackageVerificationEvidence
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmJson
```

## Next gate

Gate 119 should add verify-existing schema revalidation for
`package-manifest.json`.
