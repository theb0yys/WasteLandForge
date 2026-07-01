# Gate 113 - MCM Extender Package Verify-Existing Checksum Malformed-Entry Format Revalidation

Status: Complete

## Purpose

Gate 113 adds checksum malformed-entry format revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` entries must contain a SHA-256 hex digest,
the canonical separator, and a package-root-relative path. A malformed checksum
row that still names a recognizable expected package path is treated as one
text-format problem, not as a separate missing checksum entry.

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
- Documented: Gate 110 established checksum entry spacing canonicalization as a
  text-format diagnostic separate from malformed-entry parsing.
- Documented: Gate 112 left checksum malformed-entry format revalidation open
  as the next checksum text-format follow-up.

## Scope

Gate 113 implements:

- missing-entry cascade suppression for malformed checksum rows with
  recognizable package-root-relative paths,
- clearer malformed checksum entry message text for SHA-256 hex digest format,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 113 does not implement:

- checksum path containment revalidation coverage,
- checksum comment-line support,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains a malformed expected entry such as:

```text
zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz  MCM/ExampleMod.json
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum entry is malformed
```

The verifier reports a single malformed-entry diagnostic and does not also
report the same recognizable expected path as a missing checksum entry.

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
- Targeted verifier tests passed: 42 total.
- Targeted package verify-existing golden tests passed: 26 total.
- Full local suite passed: 360 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum malformed-entry format revalidation | Complete | Malformed expected checksum rows now report one `WF-BUILD-006` malformed-entry diagnostic without missing-entry cascade. |
| Checksum path containment revalidation | Open | Candidate for Gate 114. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 114 should add checksum path containment revalidation coverage for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
