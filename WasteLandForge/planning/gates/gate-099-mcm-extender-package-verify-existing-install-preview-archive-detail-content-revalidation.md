# Gate 99 - MCM Extender Package Verify-Existing Install-Preview Archive Detail Content Revalidation

Status: Complete

## Purpose

Gate 99 adds install-preview archive detail content revalidation for existing
MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the reusable package-verification evidence validator so
generated `install-preview.json` archive metadata is checked against expected
generated evidence and computed archive evidence when the package manifest
archive digest is already consistent with the package archive.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 75 defines `install-preview.json` as local preview-only
  package evidence beside the package manifest.
- Documented: Gate 76 validates generated `install-preview.json` against the
  immutable `install-preview/0.1.0` schema before writing it.
- Inferred: Verify-existing mode can revalidate install-preview archive
  details because they are generated evidence already covered by local package
  manifests, checksums, and build manifests.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 99 implements:

- install-preview no-archive reason text revalidation,
- install-preview created-archive output file revalidation,
- install-preview created-archive media type revalidation,
- install-preview created-archive compression revalidation,
- install-preview created-archive SHA-256 revalidation when package manifest
  archive evidence matches the computed archive digest,
- install-preview created-archive length revalidation when package manifest
  archive evidence matches the computed archive digest,
- targeted tests for edited install-preview archive detail content,
- updated golden CLI coverage for install-preview archive detail diagnostics.

Gate 99 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- package-manifest archive detail content revalidation,
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
2. Checks `install-preview.json` archive status and validation as before.
3. Checks no-archive reason text for generated install-preview evidence.
4. Checks created-archive output file, media type, and compression for
   generated install-preview evidence.
5. Checks created-archive SHA-256 and length against computed archive evidence
   when the package manifest archive digest is already consistent.
6. Reports blocking `WF-BUILD-006` diagnostics when install-preview JSON
   archive detail content is inconsistent or stale.
7. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 26 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 11 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 329 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Edited install-preview archive detail detection | Complete | Unit, file-verifier, and golden CLI tests cover stale archive digest metadata. |
| Package-manifest archive detail content revalidation | Open | Candidate for Gate 100. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 100 should add package-manifest archive detail content revalidation for
`forge package --target mcm-json --verify-existing`.
