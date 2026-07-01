# Gate 103 - MCM Extender Package Verify-Existing Checksum Unexpected-Entry Revalidation

Status: Complete

## Purpose

Gate 103 adds checksum unexpected-entry revalidation for existing MCM Extender
package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the file-based package-verification evidence verifier so
`checksums.sha256` is checked in both directions: expected package evidence
entries must be present, and present checksum entries must be expected by
package evidence. This catches stale or manually added checksum lines for
files that are not part of the generated package evidence.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 91 adds checksum-file revalidation for
  `forge package --target mcm-json --verify-existing`.
- Documented: Gate 102 adds package archive presence revalidation for existing
  package evidence.
- Inferred: Verify-existing mode should reject checksum entries that are not
  expected from package evidence, because `checksums.sha256` is generated
  package evidence and should not describe stray files as valid package output.
- Open: Checksum duplicate-entry policy remains a separate follow-up gate.

## Scope

Gate 103 implements:

- expected checksum set reuse inside checksum-file revalidation,
- blocking `WF-BUILD-006` diagnostics when `checksums.sha256` records a valid
  package-root-relative file entry that package evidence does not expect,
- targeted unit coverage with an extra temp-only file and a matching checksum
  line,
- updated golden CLI coverage for
  `forge package --target mcm-json --verify-existing`.

Gate 103 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
- checksum duplicate-entry revalidation,
- checksum canonical ordering revalidation,
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
2. Builds the expected checksum entry set from package evidence.
3. Reports missing expected checksum entries as before.
4. Reports unexpected checksum entries when `checksums.sha256` contains valid
   package-root-relative entries that are absent from the expected set.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 31 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 15 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed the
  full suite: 338 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Checksum unexpected-entry revalidation | Complete | Unit file-verifier and golden CLI tests cover an extra checksum entry with a matching stray file. |
| Checksum duplicate-entry revalidation | Open | Candidate for Gate 104. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 104 should add checksum duplicate-entry revalidation for
`forge package --target mcm-json --verify-existing`.
