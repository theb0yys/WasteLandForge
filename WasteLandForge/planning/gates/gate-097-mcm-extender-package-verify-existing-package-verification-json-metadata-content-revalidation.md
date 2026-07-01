# Gate 97 - MCM Extender Package Verify-Existing Package-Verification JSON Metadata Content Revalidation

Status: Complete

## Purpose

Gate 97 adds package-verification JSON metadata content revalidation for
existing MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the reusable package-verification evidence validator so
generated `package-verification.json` metadata fields are checked against
expected generated evidence, `package-manifest.json`, and
`install-preview.json`.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 78 defines `package-verification.json` as local package
  evidence beside package manifest and install-preview evidence.
- Documented: Gate 79 validates generated `package-verification.json` against
  the immutable `package-verification/0.1.0` schema before writing it.
- Inferred: Verify-existing mode can revalidate package-verification metadata
  because it is generated evidence that already participates in local
  manifests and checksums.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 97 implements:

- package-verification format, kind, verification type, target, and result
  metadata checks,
- command, dry-run, and project ID checks against package manifest and
  install-preview evidence,
- package type, layout, root, entry count, menu count, translation count, and
  asset count checks against package manifest and install-preview evidence,
- local-only limitation content checks,
- targeted tests for edited package-verification JSON metadata content,
- updated golden CLI coverage for package-verification metadata diagnostics.

Gate 97 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- package-verification archive digest/detail field revalidation beyond
  existing status, validation, and output-file checks,
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
2. Checks `package-verification.json` top-level metadata against expected
   generated evidence and the package manifest/install-preview reports.
3. Checks package metadata and package counts against package manifest and
   install-preview evidence.
4. Checks local-only limitations against expected generated package
   verification limitations.
5. Reports blocking `WF-BUILD-006` diagnostics when package-verification JSON
   metadata content is inconsistent or stale.
6. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 22 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 9 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 323 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Edited package-verification JSON metadata detection | Complete | Unit, file-verifier, and golden CLI tests cover stale `verificationType` metadata. |
| Package-verification archive detail content revalidation | Open | Candidate for Gate 98. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 98 should add package-verification archive detail content revalidation for
`forge package --target mcm-json --verify-existing`.
