# Gate 111 - MCM Extender Package Verify-Existing Checksum Path Casing Canonicalization Revalidation

Status: Complete

## Purpose

Gate 111 adds checksum path casing canonicalization revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` paths must match package evidence casing
exactly. This gate treats case-only path drift as a canonical-format problem,
not as separate missing and unexpected checksum entries.

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
- Documented: Gate 108 established checksum path separator canonicalization.
- Documented: Gate 110 left checksum path casing canonicalization open as the
  next checksum text-format follow-up.

## Scope

Gate 111 implements:

- case-insensitive expected-entry matching for checksum verifier bookkeeping,
- canonical path casing diagnostics for expected checksum entries,
- suppression of missing/unexpected checksum cascades for case-only path drift,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 111 does not implement:

- case-insensitive duplicate checksum entry revalidation,
- checksum comment-line support,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains a case-only path drift such as:

```text
<sha256>  mcm/ExampleMod.json
```

when package evidence declares:

```text
MCM/ExampleMod.json
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum path casing is not canonical
```

The verifier still resolves and hashes the intended package file, so this gate
does not also report missing or unexpected checksum entries for that same path.

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
- Targeted verifier tests passed: 40 total.
- Targeted package verify-existing golden tests passed: 24 total.
- Full local suite passed: 356 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum path casing canonicalization | Complete | Case-only expected checksum path drift now reports `WF-BUILD-006`. |
| Case-insensitive duplicate checksum entry revalidation | Open | Candidate for Gate 112. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 112 should add case-insensitive duplicate checksum entry revalidation for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
