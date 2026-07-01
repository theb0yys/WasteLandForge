# Gate 114 - MCM Extender Package Verify-Existing Checksum Path Containment Revalidation

Status: Complete

## Purpose

Gate 114 adds checksum path containment revalidation for
`forge package --target mcm-json --verify-existing`.

Forge-generated `checksums.sha256` paths must stay package-root-relative and
must resolve inside the package output root. A checksum row that escapes the
package root is treated as one path-containment problem, not as a separate
missing checksum entry when the row still clearly refers to an expected package
path.

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
- Documented: Gate 108 established package-root-relative path separator
  canonicalization.
- Documented: Gate 113 left checksum path containment revalidation open as the
  next checksum text-format follow-up.

## Scope

Gate 114 implements:

- missing-entry cascade suppression for containment-rejected checksum rows with
  recognizable expected package paths,
- unit coverage for the file-based verifier,
- golden CLI coverage for
  `forge package --target mcm-json --verify-existing --format json`,
- documentation, ADR, governance, prompt-library, and planning updates.

Gate 114 does not implement:

- checksum comment-line rejection revalidation,
- checksum manifest signing,
- Data or MO2 installation,
- GECK automation,
- in-game MCM Extender runtime verification,
- FOMOD packaging,
- plugin record generation.

## User-visible behavior

If `checksums.sha256` contains an escaping expected entry such as:

```text
<sha256>  ../MCM/ExampleMod.json
```

then `forge package --target mcm-json --verify-existing` reports:

```text
WF-BUILD-006 MCM package checksum path must stay under package root
```

The verifier reports a single path-containment diagnostic and does not also
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
- Targeted verifier tests passed: 43 total.
- Targeted package verify-existing golden tests passed: 27 total.
- Full local suite passed: 362 total.

Failed:

- None.

Not run:

- GECK, MO2, Data-directory installation, runtime MCM Extender, and in-game
  checks.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Checksum path containment revalidation | Complete | Escaping expected checksum rows now report one `WF-BUILD-006` path-containment diagnostic without missing-entry cascade. |
| Checksum comment-line rejection revalidation | Open | Candidate for Gate 115. |
| Runtime MCM Extender verification | Open | Requires later game-facing validation design. |
| Data/MO2 installation | Open | Outside current package evidence verifier scope. |

## Next gate

Gate 115 should add checksum comment-line rejection revalidation for
`forge package --target mcm-json --verify-existing`, keeping the same
file-based verifier and canonical command surface.
