# Gate 109 - MCM Extender Package Verify-Existing Checksum Blank-Line Revalidation

Status: Complete

## Purpose

Gate 109 adds checksum blank-line revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` files contain one checksum entry per line
and a final newline. They do not contain blank or whitespace-only rows. This
gate makes verify-existing mode report those rows directly instead of silently
skipping them during checksum parsing.

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
- Documented: Gate 107 established checksum line-ending and final-newline
  checks for Forge-generated checksum text format.
- Documented: Gate 108 left checksum blank-line revalidation open as the next
  checksum text-format follow-up.

## Scope

Gate 109 implements:

- checksum text-format detection for blank or whitespace-only rows,
- a blocking `WF-BUILD-006` diagnostic for the first blank checksum row found,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 109 does not implement:

- checksum entry spacing canonicalization,
- checksum comment-line support,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains a blank row such as:

```text
<sha256>  MCM/ExampleMod.json

<sha256>  package-verification.md
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum blank line is not canonical
```

The verifier still parses the valid checksum rows, so this gate reports the
text-format fault directly without changing digest, ordering, path separator,
or missing/unexpected-entry behavior.

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
- Targeted verifier tests passed: 38 total.
- Targeted package verify-existing golden tests passed: 22 total.
- Full local suite passed: 352 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum blank-line revalidation | Complete | Blank or whitespace-only checksum rows now report `WF-BUILD-006`. |
| Checksum entry spacing canonicalization | Open | Candidate for Gate 110. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 110 should add checksum entry spacing canonicalization revalidation for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
