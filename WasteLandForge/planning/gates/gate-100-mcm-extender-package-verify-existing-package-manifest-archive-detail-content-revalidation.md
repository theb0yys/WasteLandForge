# Gate 100 - MCM Extender Package Verify-Existing Package-Manifest Archive Detail Content Revalidation

Status: Complete

## Purpose

Gate 100 adds package-manifest archive detail content revalidation for existing
MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the reusable package-verification evidence validator so
generated `package-manifest.json` archive metadata is checked against expected
generated package evidence. Gate 85 already recomputes the package archive
SHA-256 and length recorded by `package-manifest.json`; this gate fills the
remaining manifest-side archive metadata gap.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 71 defines `package-manifest.json` as deterministic
  loose-file package evidence.
- Documented: Gate 74 validates generated `package-manifest.json` against the
  immutable `package-manifest/0.1.0` schema before writing it.
- Documented: Gate 85 already recomputes package archive SHA-256 and length
  values from generated `package.zip` files recorded by `package-manifest.json`.
- Inferred: Verify-existing mode can revalidate package-manifest archive
  reason/media/compression fields because they are generated evidence already
  covered by local package manifests, checksums, and build manifests.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 100 implements:

- package-manifest no-archive reason text revalidation,
- package-manifest created-archive output file revalidation,
- package-manifest created-archive media type revalidation,
- package-manifest created-archive compression revalidation,
- targeted tests for edited package-manifest archive detail content,
- updated golden CLI coverage for package-manifest archive detail diagnostics.

Gate 100 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest schema versions,
- duplicate SHA-256 or length diagnostics beyond the existing Gate 85 archive
  digest recomputation path,
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
2. Checks `package-manifest.json` archive status as before.
3. Checks no-archive reason text for generated package-manifest evidence.
4. Checks created-archive output file, media type, and compression for
   generated package-manifest evidence.
5. Leaves package-manifest archive SHA-256 and length verification on the
   existing file-based archive digest recomputation path.
6. Reports blocking `WF-BUILD-006` diagnostics when package-manifest JSON
   archive detail content is inconsistent or stale.
7. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 28 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 12 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 332 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Edited package-manifest archive detail detection | Complete | Unit, file-verifier, and golden CLI tests cover stale archive metadata. |
| Archive detail cross-report consistency revalidation | Open | Candidate for Gate 101. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 101 should add archive detail cross-report consistency revalidation for
`forge package --target mcm-json --verify-existing`.
