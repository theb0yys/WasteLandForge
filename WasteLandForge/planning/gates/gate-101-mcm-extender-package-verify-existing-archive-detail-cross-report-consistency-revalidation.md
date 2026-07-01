# Gate 101 - MCM Extender Package Verify-Existing Archive Detail Cross-Report Consistency Revalidation

Status: Complete

## Purpose

Gate 101 adds archive detail cross-report consistency revalidation for existing
MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the reusable package-verification evidence validator so
archive SHA-256 and length fields are compared across `package-manifest.json`,
`install-preview.json`, and `package-verification.json` when the package
manifest archive digest is already stale against the actual archive. This
keeps normal clean-path diagnostics focused on computed archive evidence while
still detecting disagreement among generated reports during stale-evidence
investigation.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 85 recomputes package archive SHA-256 and length values
  from generated `package.zip` files recorded by `package-manifest.json`.
- Documented: Gates 98 through 100 revalidate archive detail content in
  `package-verification.json`, `install-preview.json`, and
  `package-manifest.json`.
- Inferred: Verify-existing mode should also compare report-to-report archive
  digest fields when computed manifest archive evidence is already stale,
  because those reports are generated evidence that should agree.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 101 implements:

- install-preview archive SHA-256 and length comparison against
  package-manifest evidence when package-manifest archive digest evidence is
  stale,
- package-verification archive SHA-256 and length comparison against
  package-manifest evidence when package-manifest archive digest evidence is
  stale,
- package-verification archive SHA-256 and length comparison against
  install-preview evidence when package-manifest archive digest evidence is
  stale,
- targeted tests for stale package-manifest archive digest evidence with
  cross-report disagreement,
- updated golden CLI coverage for archive detail cross-report diagnostics.

Gate 101 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
- duplicate clean-path SHA-256 or length diagnostics when package-manifest
  archive digest evidence already matches the actual archive,
- package archive presence revalidation when all reports claim no archive,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Reads package manifest, install preview, install-preview summary, package
   verification, package-verification summary, checksum, and build-manifest
   evidence without regenerating package outputs.
2. Recomputes package archive digest evidence from `package.zip` as before.
3. Uses the existing computed-evidence checks when `package-manifest.json`
   archive digest evidence matches the actual archive.
4. When `package-manifest.json` archive digest evidence is stale, compares
   archive SHA-256 and length fields across package-manifest, install-preview,
   and package-verification evidence.
5. Reports blocking `WF-BUILD-006` diagnostics when archive detail evidence
   disagrees across reports.
6. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 29 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 13 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 334 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Archive detail cross-report consistency detection | Complete | Unit file-verifier and golden CLI tests cover stale package-manifest archive digest evidence with cross-report disagreement. |
| Package archive presence revalidation | Open | Candidate for Gate 102. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 102 should add package archive presence revalidation for
`forge package --target mcm-json --verify-existing`.
