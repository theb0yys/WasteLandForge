# Gate 120 - MCM Extender Install-Preview Verify-Existing Schema Revalidation

Status: Complete

## Purpose

Gate 120 strengthens existing package evidence verification for the MCM
Extender JSON slice by revalidating existing `install-preview.json` against
the embedded `install-preview/0.1.0` schema in
`forge package --target mcm-json --verify-existing`.

The install preview feeds summary validation, install-plan comparisons,
package-verification cross-checks, and archive consistency checks. If it no
longer matches the published generated-evidence schema, dependent package
evidence checks are skipped to avoid cascading diagnostics from an untrusted
install-preview shape.

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
- Inferred: Because newly generated install previews are already validated
  against `install-preview/0.1.0`, verify-existing mode should reject
  existing install previews that no longer match that embedded schema before
  running checks derived from install-preview content.

## Implemented

Gate 120 implements:

- embedded `install-preview/0.1.0` schema loading in the file-based package
  evidence verifier,
- local schema-registry isolation shared with package-manifest and install-plan
  schema loading,
- schema-first validation of existing `install-preview.json` in
  `forge package --target mcm-json --verify-existing`,
- one coalesced `WF-BUILD-006` install-preview schema diagnostic per invalid
  install-preview document,
- cascade control so dependent package evidence checks run only after the
  existing install preview passes schema validation,
- focused unit coverage for edited install-preview schema shape,
- golden CLI coverage for stale install-preview schema evidence through
  `forge package --target mcm-json --verify-existing`.

## Not implemented

Gate 120 does not implement:

- package-verification schema validation in verify-existing mode,
- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- inspecting MO2 VFS conflicts,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Verifier schema checks | Complete | Existing install-preview JSON is checked against embedded `install-preview/0.1.0`. |
| CLI coverage | Complete | Golden tests exercise edited install-preview schema evidence through the real CLI. |
| Unit coverage | Complete | Focused verifier tests cover install-preview schema drift. |
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

Gate 121 should add verify-existing schema revalidation for
`package-verification.json`.
