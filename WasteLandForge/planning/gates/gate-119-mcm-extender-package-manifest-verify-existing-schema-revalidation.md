# Gate 119 - MCM Extender Package-Manifest Verify-Existing Schema Revalidation

Status: Complete

## Purpose

Gate 119 strengthens existing package evidence verification for the MCM
Extender JSON slice by revalidating existing `package-manifest.json` against
the embedded `package-manifest/0.1.0` schema in
`forge package --target mcm-json --verify-existing`.

The package manifest is foundational package evidence. If it no longer
matches the published generated-evidence schema, dependent package evidence
checks are skipped to avoid cascading diagnostics from an untrusted manifest
shape.

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
- Inferred: Because newly generated package manifests are already validated
  against `package-manifest/0.1.0`, verify-existing mode should reject
  existing package manifests that no longer match that embedded schema before
  running checks derived from package-manifest content.

## Implemented

Gate 119 implements:

- embedded `package-manifest/0.1.0` schema loading in the file-based package
  evidence verifier,
- local schema-registry isolation shared with install-plan schema loading,
- schema-first validation of existing `package-manifest.json` in
  `forge package --target mcm-json --verify-existing`,
- one coalesced `WF-BUILD-006` package-manifest schema diagnostic per invalid
  package-manifest document,
- cascade control so dependent package evidence checks run only after the
  existing package manifest passes schema validation,
- focused unit coverage for edited package-manifest schema shape,
- golden CLI coverage for stale package-manifest schema evidence through
  `forge package --target mcm-json --verify-existing`.

## Not implemented

Gate 119 does not implement:

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
| Verifier schema checks | Complete | Existing package-manifest JSON is checked against embedded `package-manifest/0.1.0`. |
| CLI coverage | Complete | Golden tests exercise edited package-manifest schema evidence through the real CLI. |
| Unit coverage | Complete | Focused verifier tests cover root and nested package-manifest schema drift. |
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

Gate 120 should add verify-existing schema revalidation for
`install-preview.json`.
