# Gate 108 - MCM Extender Package Verify-Existing Checksum Path Separator Canonicalization Revalidation

Status: Complete

## Purpose

Gate 108 adds checksum path separator canonicalization revalidation for
`forge package --target mcm-json --verify-existing`.

The goal is to keep `checksums.sha256` aligned with Forge-generated package
evidence: package-root-relative paths use `/` separators, not platform
backslashes, while verification remains file-system-aware enough to resolve and
hash the referenced file.

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
- Documented: Gate 105 and Gate 106 established canonical checksum ordering and
  digest formatting checks.
- Documented: Gate 107 left checksum path separator canonicalization open as
  the next checksum text-format follow-up.

## Scope

Gate 108 implements:

- retention of the raw checksum entry path while normalizing paths for local
  file resolution,
- a blocking `WF-BUILD-006` diagnostic when an expected package evidence entry
  in `checksums.sha256` uses backslash separators,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 108 does not implement:

- checksum blank-line revalidation,
- checksum comment-line support,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If an expected checksum entry uses a path such as:

```text
<sha256>  MCM\ExampleMod.json
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum path separator is not canonical
```

The verifier still normalizes the path internally for file resolution, so this
gate reports the formatting fault directly rather than producing misleading
missing-entry or digest diagnostics.

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
- Targeted verifier tests passed: 37 total.
- Targeted package verify-existing golden tests passed: 21 total.
- Full local suite passed: 350 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum path separator canonicalization | Complete | Expected checksum entries using backslashes now report `WF-BUILD-006`. |
| Checksum blank-line revalidation | Open | Candidate for Gate 109. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 109 should add checksum blank-line revalidation for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
