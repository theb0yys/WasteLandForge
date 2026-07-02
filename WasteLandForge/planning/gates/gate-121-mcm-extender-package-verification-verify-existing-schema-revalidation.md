# Gate 121 - MCM Extender Package-Verification Verify-Existing Schema Revalidation

Status: Complete

## Purpose

Gate 121 strengthens existing package evidence verification for the MCM
Extender JSON slice by revalidating existing `package-verification.json`
against the embedded `package-verification/0.1.0` schema in
`forge package --target mcm-json --verify-existing`.

The package-verification report feeds summary validation, checksum/build
manifest checks, metadata checks, archive checks, and cross-report consistency
checks. If it no longer matches the published generated-evidence schema,
dependent package-verification checks are skipped to avoid cascading
diagnostics from an untrusted package-verification shape.

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
- Inferred: Because newly generated package-verification reports are already
  validated against `package-verification/0.1.0`, verify-existing mode should
  reject existing package-verification reports that no longer match that
  embedded schema before running checks derived from package-verification
  content.

## Implemented

Gate 121 implements:

- embedded `package-verification/0.1.0` schema loading in the file-based
  package evidence verifier,
- local schema-registry isolation shared with package-manifest, install-preview,
  and install-plan schema loading,
- schema-first validation of existing `package-verification.json` in
  `forge package --target mcm-json --verify-existing`,
- one coalesced `WF-BUILD-006` package-verification schema diagnostic per
  invalid package-verification document,
- cascade control so dependent package evidence checks run only after the
  existing package-verification report passes schema validation,
- focused unit coverage for edited package-verification schema shape,
- golden CLI coverage for stale package-verification schema evidence through
  `forge package --target mcm-json --verify-existing`.

## Not implemented

Gate 121 does not implement:

- new package-verification schema IDs,
- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- inspecting MO2 VFS conflicts,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Verifier schema checks | Complete | Existing package-verification JSON is checked against embedded `package-verification/0.1.0`. |
| CLI coverage | Complete | Golden tests exercise edited package-verification schema evidence through the real CLI. |
| Unit coverage | Complete | Focused verifier tests cover package-verification schema drift. |
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

Gate 122 should add focused diagnostic projection coverage for schema-gated
verify-existing failures.
