# Gate 106 - MCM Extender Package Verify-Existing Checksum Digest Canonical-Casing Revalidation

Status: Complete

## Purpose

Gate 106 adds checksum digest canonical-casing revalidation for existing MCM
Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the file-based package-verification evidence verifier so
expected package evidence entries in `checksums.sha256` must use lowercase
SHA-256 hex, matching the checksum evidence Forge writes. Uppercase digest text
is accepted for recomputation so the verifier can report the casing issue
without producing a false digest mismatch.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 91 adds checksum-file revalidation for
  `forge package --target mcm-json --verify-existing`.
- Documented: Gate 105 adds checksum canonical-order revalidation for existing
  package evidence.
- Inferred: Verify-existing mode should reject uppercase checksum digests
  because Forge writes checksum evidence using lowercase SHA-256 hex and
  deterministic text evidence should preserve that canonical form.
- Open: Checksum line-ending and trailing-newline revalidation remains a
  separate follow-up gate.

## Scope

Gate 106 implements:

- canonical lowercase SHA-256 digest checks for expected package evidence
  entries in `checksums.sha256`,
- blocking `WF-BUILD-006` diagnostics when expected checksum digests use
  uppercase hex,
- digest recomputation that still normalizes accepted hex text so uppercase
  but otherwise correct digests do not also produce digest-mismatch failures,
- targeted unit coverage with an expected checksum digest rewritten to
  uppercase,
- updated golden CLI coverage for
  `forge package --target mcm-json --verify-existing`.

Gate 106 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
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
4. Checks expected package evidence entries for lowercase SHA-256 digest text.
5. Checks expected package evidence entries for canonical path ordering.
6. Keeps existing digest, missing-entry, and unexpected-entry revalidation.
7. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 34 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 18 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed the
  full suite: 344 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Checksum digest canonical-casing revalidation | Complete | Unit file-verifier and golden CLI tests cover an uppercase expected checksum digest. |
| Checksum line-ending and trailing-newline revalidation | Open | Candidate for Gate 107. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 107 should add checksum line-ending and trailing-newline revalidation for
`forge package --target mcm-json --verify-existing`.
