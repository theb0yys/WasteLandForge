# Gate 110 - MCM Extender Package Verify-Existing Checksum Entry Spacing Canonicalization Revalidation

Status: Complete

## Purpose

Gate 110 adds checksum entry spacing canonicalization revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` entries use exactly two spaces between the
SHA-256 digest and the package-root-relative path, and they do not include
leading or trailing path whitespace. This gate reports spacing drift directly
instead of letting it become a malformed-entry, missing-entry, or file-read
cascade.

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
- Documented: Gate 107, Gate 108, and Gate 109 established checksum
  text-format checks for line endings, final newlines, path separators, and
  blank rows.
- Documented: Gate 109 left checksum entry spacing canonicalization open as the
  next checksum text-format follow-up.

## Scope

Gate 110 implements:

- lenient checksum row parsing that preserves raw separator and raw path text,
  while still resolving the intended package file,
- a blocking `WF-BUILD-006` diagnostic when an expected checksum entry does not
  use exactly two spaces between digest and path,
- a blocking `WF-BUILD-006` diagnostic when an expected checksum entry contains
  leading or trailing path whitespace,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 110 does not implement:

- checksum path casing canonicalization,
- checksum comment-line support,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains a single-space separator such as:

```text
<sha256> MCM/ExampleMod.json
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum entry spacing is not canonical
```

The verifier still resolves the intended package path for other checks, so this
gate reports the spacing fault directly without changing digest, ordering, path
separator, blank-line, or missing/unexpected-entry behavior.

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
- Targeted verifier tests passed: 39 total.
- Targeted package verify-existing golden tests passed: 23 total.
- Full local suite passed: 354 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum entry spacing canonicalization | Complete | Expected checksum entries with non-canonical spacing now report `WF-BUILD-006`. |
| Checksum path casing canonicalization | Open | Candidate for Gate 111. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 111 should add checksum path casing canonicalization revalidation for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
