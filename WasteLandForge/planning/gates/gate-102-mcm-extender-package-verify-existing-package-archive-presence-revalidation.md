# Gate 102 - MCM Extender Package Verify-Existing Package Archive Presence Revalidation

Status: Complete

## Purpose

Gate 102 adds package archive presence revalidation for existing MCM Extender
package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate extends the file-based package-verification evidence verifier so it
checks for a physical `package.zip` beside `package-manifest.json` even when
the generated package evidence records archive status `not-created`. This
catches stale or manually edited no-archive evidence without regenerating
package outputs.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 72 writes `package.zip` for build/package output under the
  deterministic `mcm-json` package tree.
- Documented: Gates 83 through 86 add file-based package evidence verification,
  payload digest recomputation, archive digest recomputation, and archive entry
  revalidation.
- Documented: Gates 98 through 101 revalidate archive detail content and
  cross-report consistency for existing package evidence.
- Inferred: Verify-existing mode should flag `package.zip` when all package
  evidence claims no archive was created, because a present generated archive
  is package evidence that must agree with `package-manifest.json`.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 102 implements:

- package archive presence detection beside `package-manifest.json` when
  package-manifest archive status is not `created`,
- blocking `WF-BUILD-006` diagnostics for present `package.zip` files that are
  missing from archive evidence,
- targeted unit coverage with internally consistent no-archive JSON/Markdown
  evidence and a real generated archive left on disk,
- updated golden CLI coverage for
  `forge package --target mcm-json --verify-existing`.

Gate 102 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- new package-manifest, install-preview, or package-verification schema
  versions,
- checksum unexpected-entry rejection,
- build-manifest unexpected-output policy beyond existing checks,
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
2. Derives archive digest expectations from `package-manifest.json` as before.
3. If `package-manifest.json` does not record a created archive, checks whether
   `package.zip` exists beside `package-manifest.json`.
4. Reports a blocking `WF-BUILD-006` diagnostic when that physical archive is
   present but package evidence records no archive.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~McmPackageVerificationEvidence"` passed 30 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~PackageVerifyExistingMcmJson"` passed 14 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed the
  full suite: 336 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Package archive presence revalidation | Complete | Unit file-verifier and golden CLI tests cover no-archive evidence with an existing `package.zip`. |
| Checksum unexpected-entry revalidation | Open | Candidate for Gate 103. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 103 should add checksum unexpected-entry revalidation for
`forge package --target mcm-json --verify-existing`.
