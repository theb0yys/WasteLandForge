# Gate 96 - MCM Extender Package Verify-Existing Package-Verification JSON Check Content Revalidation

Status: Complete

## Purpose

Gate 96 adds deeper package-verification JSON check content revalidation for
existing MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the reusable package-verification evidence validator so
generated `package-verification.json` check objects are checked against the
package evidence they summarize.

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
- Inferred: Verify-existing mode can revalidate check object content because
  the JSON report is generated evidence that already participates in local
  manifests and checksums.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 96 implements:

- package-verification check `status` validation for schema checks, the
  install-preview summary check, and payload digest evidence,
- package-verification check `evidence` validation for package manifest,
  install-preview, and install-preview summary evidence paths,
- continued payload digest count validation against package manifest and
  recomputed payload digests,
- continued package archive check status/validation revalidation,
- targeted tests for edited package-verification JSON check content,
- updated golden CLI coverage for package-verification JSON check diagnostics.

Gate 96 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- package-verification metadata field revalidation beyond existing checks,
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
2. Checks `package-verification.json` check objects for expected schema check
   statuses and evidence paths.
3. Checks the install-preview summary check status and evidence path.
4. Checks payload digest status and count against package manifest and
   recomputed payload digest evidence.
5. Checks package archive status and validation against computed archive
   evidence.
6. Reports blocking `WF-BUILD-006` diagnostics when package-verification JSON
   check content is inconsistent or stale.
7. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 20 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 8 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 320 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Edited package-verification JSON check evidence detection | Complete | Unit, file-verifier, and golden CLI tests cover stale `package-manifest-schema` evidence. |
| Edited package-verification JSON check status detection | Complete | Unit tests cover stale `package-payload-digests` status. |
| Package-verification metadata content revalidation | Open | Candidate for Gate 97. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 97 should add package-verification JSON metadata content revalidation for
`forge package --target mcm-json --verify-existing`.
