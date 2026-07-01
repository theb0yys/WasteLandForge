# Gate 107 - MCM Extender Package Verify-Existing Checksum Line-Ending And Trailing-Newline Revalidation

Status: Complete

## Purpose

Gate 107 adds checksum line-ending and trailing-newline revalidation for
existing MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the file-based package-verification evidence verifier so
`checksums.sha256` must keep the final newline and line-ending style written by
Forge on the current platform. This preserves the generated checksum evidence
format without regenerating package outputs in verify-existing mode.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 91 adds checksum-file revalidation for
  `forge package --target mcm-json --verify-existing`.
- Documented: Gate 106 adds checksum digest canonical-casing revalidation for
  existing package evidence.
- Inferred: Verify-existing mode should reject missing final newlines and
  non-canonical checksum line endings because Forge writes checksum evidence
  with a final newline and the platform line separator.
- Open: Checksum path separator canonicalization remains a separate follow-up
  gate.

## Scope

Gate 107 implements:

- final-newline checks for `checksums.sha256`,
- line-ending checks against Forge's current checksum writer line separator,
- blocking `WF-BUILD-006` diagnostics for missing final newlines,
- blocking `WF-BUILD-006` diagnostics for non-canonical checksum line endings,
- targeted unit coverage for missing final newline and non-canonical line
  endings,
- updated golden CLI coverage for
  `forge package --target mcm-json --verify-existing`.

Gate 107 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
- checksum path separator canonicalization,
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
2. Reports missing final newline in `checksums.sha256`.
3. Reports non-canonical checksum line endings in `checksums.sha256`.
4. Builds checksum entries from `checksums.sha256` using normalized
   package-root-relative paths.
5. Reports duplicate checksum entries when the same normalized path appears
   more than once.
6. Checks expected package evidence entries for lowercase SHA-256 digest text.
7. Checks expected package evidence entries for canonical path ordering.
8. Keeps existing digest, missing-entry, and unexpected-entry revalidation.
9. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 36 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 20 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed the
  full suite: 348 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Checksum line-ending revalidation | Complete | Unit file-verifier and golden CLI tests cover non-canonical line endings. |
| Checksum trailing-newline revalidation | Complete | Unit file-verifier and golden CLI tests cover a missing final newline. |
| Checksum path separator canonicalization | Open | Candidate for Gate 108. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 108 should add checksum path separator canonicalization revalidation for
`forge package --target mcm-json --verify-existing`.
