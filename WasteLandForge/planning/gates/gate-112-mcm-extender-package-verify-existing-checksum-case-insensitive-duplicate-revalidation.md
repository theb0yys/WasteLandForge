# Gate 112 - MCM Extender Package Verify-Existing Checksum Case-Insensitive Duplicate Revalidation

Status: Complete

## Purpose

Gate 112 adds checksum case-insensitive duplicate revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` paths are package-root-relative identifiers.
Two checksum entries that normalize to the same path ignoring case are treated
as one duplicate checksum entry problem, not as separate path-casing,
missing-entry, or unexpected-entry failures.

## Research grounding

- Documented: ADR-009 requires deterministic, rebuildable generated outputs and
  provenance evidence.
- Documented: ADR-010 keeps package verification on the canonical
  `forge package --target mcm-json --verify-existing` command surface and does
  not add verifier aliases.
- Documented: ADR-011 requires layered validation, deterministic fixture-backed
  tests, and local build manifests/checksums.
- Documented: Gate 91 introduced checksum-file revalidation for existing
  package evidence.
- Documented: Gate 104 introduced checksum duplicate-entry revalidation.
- Documented: Gate 111 established case-insensitive expected-entry matching and
  left case-insensitive duplicate checksum entry revalidation open as the next
  checksum text-format follow-up.

## Scope

Gate 112 implements:

- case-insensitive duplicate detection for normalized checksum entry paths,
- suppression of path-casing/missing/unexpected cascades for the later duplicate
  checksum row,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 112 does not implement:

- checksum malformed-entry format revalidation beyond the existing parser
  diagnostics,
- checksum comment-line support,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains two entries such as:

```text
<sha256>  MCM/ExampleMod.json
<sha256>  mcm/ExampleMod.json
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum entry is duplicated
```

The verifier reports a single duplicate-entry diagnostic and does not also
report checksum path casing, missing-entry, or unexpected-entry diagnostics for
the later duplicate row.

## Validation results

Ran:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmPackageVerificationEvidence
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~PackageVerifyExistingMcmJson
dotnet test WastelandForge.sln --no-build --no-restore -m:1
```

Passed:

- Build succeeded with 0 warnings and 0 errors.
- Targeted verifier tests passed: 41 total.
- Targeted package verify-existing golden tests passed: 25 total.
- Full local suite passed: 358 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Case-insensitive duplicate checksum entry revalidation | Complete | Case-only duplicate checksum rows now report one `WF-BUILD-006` duplicate-entry diagnostic. |
| Checksum malformed-entry format revalidation | Open | Candidate for Gate 113. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 113 should add checksum malformed-entry format revalidation coverage for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
