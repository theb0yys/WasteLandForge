# Gate 104 - MCM Extender Package Verify-Existing Checksum Duplicate-Entry Revalidation

Status: Complete

## Purpose

Gate 104 adds checksum duplicate-entry revalidation for existing MCM Extender
package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the file-based package-verification evidence verifier so
`checksums.sha256` cannot silently record the same normalized
package-root-relative path more than once. Duplicate checksum lines are stale
or ambiguous generated package evidence, even when the duplicated digest still
matches the file.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 91 adds checksum-file revalidation for
  `forge package --target mcm-json --verify-existing`.
- Documented: Gate 103 adds checksum unexpected-entry revalidation for existing
  package evidence.
- Inferred: Verify-existing mode should reject duplicate checksum paths because
  `checksums.sha256` is generated package evidence and each package output path
  should have one unambiguous checksum entry.
- Open: Checksum canonical ordering remains a separate follow-up gate.

## Scope

Gate 104 implements:

- duplicate normalized checksum path detection while reading
  `checksums.sha256`,
- blocking `WF-BUILD-006` diagnostics when `checksums.sha256` records the same
  package-root-relative entry more than once,
- preservation of the first checksum entry for subsequent digest validation,
- targeted unit coverage with a duplicated expected MCM JSON checksum entry,
- updated golden CLI coverage for
  `forge package --target mcm-json --verify-existing`.

Gate 104 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
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
2. Builds checksum entries from `checksums.sha256` using normalized
   package-root-relative paths.
3. Reports duplicate checksum entries when the same normalized path appears
   more than once.
4. Keeps existing digest, missing-entry, and unexpected-entry revalidation.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 32 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 16 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed the
  full suite: 340 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Checksum duplicate-entry revalidation | Complete | Unit file-verifier and golden CLI tests cover a duplicated expected MCM JSON checksum entry. |
| Checksum canonical-order revalidation | Open | Candidate for Gate 105. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 105 should add checksum canonical-order revalidation for
`forge package --target mcm-json --verify-existing`.
