# Gate 94 - MCM Extender Package Verify-Existing Install-Preview Package-Manifest Entry Cross-Check

Status: Complete

## Purpose

Gate 94 adds entry-level install-preview/package-manifest cross-checking for
existing MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate reuses the existing file-based package verifier and extends it to
compare `package-manifest.json` entries with `install-preview.json` entries.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 71 defines `package-manifest.json` as deterministic
  loose-file package evidence.
- Documented: Gate 75 defines `install-preview.json` as preview-only install
  intent evidence for the same package output tree.
- Inferred: Manifest entries and install-preview entries can be cross-checked
  in verify-existing mode because they describe the same generated package
  payload from different review perspectives.
- Open: Actual Data/MO2 installation and runtime visibility remain separate
  safety/runtime gates.

## Scope

Gate 94 implements:

- package-manifest entry loading inside the file-based verifier,
- install-preview entry loading inside the file-based verifier,
- entry-set comparison by package kind, ID, and package path,
- field comparison for source file, install path, media type, action,
  declared asset source, and asset target file,
- blocking `WF-BUILD-006` diagnostics for missing, undeclared, or stale
  install-preview entry evidence,
- targeted tests for edited install-preview entry content,
- updated golden CLI coverage for install-preview/package-manifest entry
  mismatch diagnostics.

Gate 94 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
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
2. Builds package-manifest entry keys from `kind`, `id`, and `path`.
3. Builds install-preview entry keys from `kind`, `id`, and `dataPath`.
4. Requires every package-manifest entry to have a matching install-preview
   entry.
5. Requires every install-preview entry to be declared by the package
   manifest.
6. Compares install-preview `sourceFile`, `installPath`, `mediaType`,
   `action`, `declaredSourceFile`, and `targetFile` against the package
   manifest entry.
7. Reports blocking `WF-BUILD-006` diagnostics when entry evidence is missing,
   undeclared, inconsistent, or stale.
8. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 35 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 44 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 314 tests.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Install-preview/package-manifest entry source mismatch detection | Complete | Unit and golden tests cover stale `sourceFile` entry content. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| MO2 profile and VFS conflict inspection | Open | Requires later capability/runtime integration gates. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |
| Package-verification Markdown summary full content revalidation | Open | Candidate for Gate 95. |

## Next Gate

Gate 95 should add fuller package-verification Markdown summary content
revalidation for `forge package --target mcm-json --verify-existing`.
