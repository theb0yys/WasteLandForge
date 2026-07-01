# Gate 91 - MCM Extender Package Verify-Existing Checksum-File Revalidation

Status: Complete

## Purpose

Gate 91 adds checksum-file revalidation for existing MCM Extender package
evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate reuses the existing file-based package verifier and extends it to
read `checksums.sha256` from the selected package output root.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, and local build manifests/checksums.
- Inferred: `checksums.sha256` can be revalidated in verify-existing mode
  because Gate 90 already established that package verification diagnostics are
  projections from the same canonical `DiagnosticReport`.
- Open: Build-manifest content revalidation remains a separate package
  verifier gate.

## Scope

Gate 91 implements:

- `checksums.sha256` loading for package verify-existing mode,
- checksum entry format validation,
- package-root containment validation for checksum entry paths,
- SHA-256 recomputation for checksum file entries,
- required checksum entry checks for package evidence, payload files, created
  package archives, and `build-manifest.json`,
- JSON and human/plain verify-existing output that lists the checksum file as
  package evidence,
- targeted tests for edited checksum digests and missing checksum entries,
- updated golden CLI coverage for checksum-file diagnostics.

Gate 91 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- build-manifest content revalidation,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Resolves `dist/mcm-json/checksums.sha256` by default, or the corresponding
   checksum file under a custom `--output <path>` inside project `dist/`.
2. Reads package manifest, install preview, package verification, Markdown
   summary, and checksum evidence without regenerating package outputs.
3. Recomputes SHA-256 values for files listed in `checksums.sha256`.
4. Reports blocking `WF-BUILD-006` diagnostics when checksum entries are
   malformed, point outside the package root, reference unreadable files,
   mismatch file contents, or omit required package evidence.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 41 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 307 tests.
- `git diff --check` passed.
- Targeted trailing-whitespace scan over changed files returned no matches.
- Stale current-Gate-90 wording scan returned no matches.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms found only self-referential validation-record lines in planning gate
  files; no protected project files were touched.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Checksum file path surfaced in verify-existing JSON | Complete | `outputs.checksums` now reports `dist/mcm-json/checksums.sha256`. |
| Edited checksum digest detection | Complete | Unit and golden tests cover a changed checksum entry for `MCM/ExampleMod.json`. |
| Missing checksum entry detection | Complete | Unit tests cover a missing `package-verification.md` checksum entry. |
| Build-manifest content revalidation | Open | Candidate for Gate 92. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 92 should add build-manifest content revalidation for
`forge package --target mcm-json --verify-existing`.
