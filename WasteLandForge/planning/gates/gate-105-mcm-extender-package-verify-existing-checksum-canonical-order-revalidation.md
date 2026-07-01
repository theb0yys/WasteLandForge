# Gate 105 - MCM Extender Package Verify-Existing Checksum Canonical-Order Revalidation

Status: Complete

## Purpose

Gate 105 adds checksum canonical-order revalidation for existing MCM Extender
package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the file-based package-verification evidence verifier so
expected package evidence entries in `checksums.sha256` must appear in the same
canonical path order used by Forge when it writes checksum evidence. This keeps
checksum evidence deterministic and makes hand-edited or stale ordering visible
without regenerating package outputs.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 91 adds checksum-file revalidation for
  `forge package --target mcm-json --verify-existing`.
- Documented: Gate 104 adds checksum duplicate-entry revalidation for existing
  package evidence.
- Inferred: Verify-existing mode should reject non-canonical checksum ordering
  because Forge writes checksum evidence sorted by normalized
  package-root-relative path.
- Open: Checksum digest canonical casing remains a separate follow-up gate.

## Scope

Gate 105 implements:

- canonical ordering checks for expected package evidence entries in
  `checksums.sha256`,
- blocking `WF-BUILD-006` diagnostics when expected checksum entries are not
  sorted by normalized package-root-relative path,
- focused ownership boundaries so unexpected checksum rows keep their Gate 103
  diagnostic instead of also producing an order diagnostic,
- targeted unit coverage with an expected checksum entry moved before an
  earlier-sorting package file,
- updated golden CLI coverage for
  `forge package --target mcm-json --verify-existing`.

Gate 105 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
- checksum digest canonical-casing revalidation,
- checksum line-ending or trailing-newline revalidation,
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
4. Checks the remaining expected package evidence entries for canonical path
   ordering.
5. Keeps existing digest, missing-entry, and unexpected-entry revalidation.
6. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 33 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 17 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed the
  full suite: 342 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Checksum canonical-order revalidation | Complete | Unit file-verifier and golden CLI tests cover a moved expected checksum entry. |
| Checksum digest canonical-casing revalidation | Open | Candidate for Gate 106. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 106 should add checksum digest canonical-casing revalidation for
`forge package --target mcm-json --verify-existing`.
