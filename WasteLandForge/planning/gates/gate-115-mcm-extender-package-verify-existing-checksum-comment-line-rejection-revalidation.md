# Gate 115 - MCM Extender Package Verify-Existing Checksum Comment-Line Rejection Revalidation

Status: Complete

## Purpose

Gate 115 adds checksum comment-line rejection revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` evidence is a strict checksum entry list. It
does not include comments. A `#` comment row is treated as one canonical text
format problem, not as a generic malformed checksum entry.

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
- Documented: Gate 107 established canonical checksum text formatting for line
  endings and final newlines.
- Documented: Gate 114 left checksum comment-line rejection revalidation open
  as the next checksum text-format follow-up.

## Scope

Gate 115 implements:

- dedicated checksum comment-line diagnostics,
- parser suppression so comment rows do not also report generic malformed-entry
  diagnostics,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 115 does not implement:

- actual Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains a comment row such as:

```text
# Forge checksum comments are not canonical
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum comment line is not canonical
```

The verifier reports a single comment-line diagnostic and does not also report
that same row as a generic malformed checksum entry.

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
- Targeted verifier tests passed: 44 total.
- Targeted package verify-existing golden tests passed: 28 total.
- Full local suite passed: 364 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum comment-line rejection revalidation | Complete | Comment rows now report one `WF-BUILD-006` comment-line diagnostic without malformed-entry cascade. |
| Install-ready layout and export planning | Open | Candidate for Gate 116. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 116 should move the MCM slice toward install-ready layout and export
planning while preserving the offline-first, no-mutation default.
