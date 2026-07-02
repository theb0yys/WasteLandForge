# Gate 117 - MCM Extender Install-Plan Verify-Existing Content Revalidation

Status: Complete

## Purpose

Gate 117 strengthens existing package evidence verification for the MCM
Extender JSON slice by revalidating install-plan JSON and Markdown content in
`forge package --target mcm-json --verify-existing`.

The gate keeps install-plan evidence as local export planning only. It does
not turn Forge into an installer and does not mutate a game `Data` folder,
MO2 profile, or runtime state.

## Research grounding

- Documented: ADR-009 requires generated outputs to remain disposable,
  deterministic, and traceable through local manifests.
- Documented: ADR-010 keeps the command surface stable and routes existing
  package evidence checks through `forge package --target mcm-json
  --verify-existing`.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed tests.
- Inferred: Because Gate 116 records install-plan files in manifests and
  checksums, `--verify-existing` should also detect stale install-plan content
  instead of only detecting missing or digest-mismatched files.

## Implemented

Gate 117 implements:

- optional install-plan paths on
  `McmPackageVerificationEvidenceFileVerificationRequest`,
- CLI verify-existing wiring for `install-plan.json` and `install-plan.md`,
- install-plan metadata revalidation against package manifest evidence,
- install-plan package root, type, layout, install root, export-plan mode,
  manual-approval, and non-mutation flag checks,
- install-plan archive status/detail revalidation against package manifest
  archive evidence,
- install-plan entry revalidation against package-manifest entries,
- install-plan required copy action and required-entry checks,
- install-plan Markdown summary revalidation against `install-plan.json`,
- focused unit tests for edited install-plan metadata, entry content, and
  summary content,
- golden CLI coverage for stale install-plan evidence through
  `forge package --target mcm-json --verify-existing`.

## Not implemented

Gate 117 does not implement:

- schema validation of existing `install-plan.json` in verify-existing mode,
- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- inspecting MO2 VFS conflicts,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Verifier content checks | Complete | Install-plan JSON and Markdown content are checked in verify-existing mode. |
| CLI coverage | Complete | Golden tests exercise edited install-plan evidence through the real CLI. |
| Unit coverage | Complete | Focused verifier tests cover metadata, entry, and summary drift. |
| Command surface | Complete | No new command or alias was added. |
| Install behavior | Not implemented | Forge still produces reports only and does not install files. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmPackageVerificationEvidence
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmJson
```

## Next gate

Gate 118 should add verify-existing schema revalidation for
`install-plan.json`.
