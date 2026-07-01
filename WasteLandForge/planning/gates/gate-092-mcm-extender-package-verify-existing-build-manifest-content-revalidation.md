# Gate 92 - MCM Extender Package Verify-Existing Build-Manifest Content Revalidation

Status: Complete

## Purpose

Gate 92 adds build-manifest content revalidation for existing MCM Extender
package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate reuses the existing file-based package verifier and extends it to
read `build-manifest.json` from the selected package output root.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, mandatory local build manifests, and local
  checksums.
- Inferred: `build-manifest.json` can be revalidated in verify-existing mode
  because Gates 88-91 already established package verification as an
  existing-evidence diagnostic mode.
- Open: Human install-preview summary content revalidation remains a separate
  package-verifier gate.

## Scope

Gate 92 implements:

- `build-manifest.json` loading for package verify-existing mode,
- build-manifest command, target, kind, dry-run, and build-type checks,
- package/install-preview/package-verification evidence path checks,
- package-verification cross-check status checks,
- build-manifest output digest recomputation for package payload and evidence
  files,
- JSON and human/plain verify-existing output that lists the build manifest as
  package evidence,
- targeted tests for edited build-manifest cross-checks and output digests,
- updated golden CLI coverage for build-manifest diagnostics.

Gate 92 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- install-preview Markdown summary content revalidation,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Resolves `dist/mcm-json/build-manifest.json` by default, or the
   corresponding build manifest under a custom `--output <path>` inside
   project `dist/`.
2. Reads package manifest, install preview, package verification, Markdown
   summary, checksum, and build-manifest evidence without regenerating package
   outputs.
3. Compares build-manifest package evidence fields against already loaded
   package evidence files.
4. Recomputes output SHA-256 and length values for files that should appear in
   build-manifest `outputs`.
5. Reports blocking `WF-BUILD-006` diagnostics when build-manifest evidence is
   missing, inconsistent, stale, or records unexpected outputs.
6. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 33 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 42 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 310 tests.
- `git diff --check` passed.
- Targeted trailing-whitespace scan over changed files returned no matches.
- Stale current-Gate-91 wording scan returned no matches.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms found only self-referential validation-record lines in planning gate
  files; no protected project files were touched.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Build manifest path surfaced in verify-existing JSON | Complete | `outputs.buildManifest` now reports `dist/mcm-json/build-manifest.json`. |
| Edited build-manifest cross-check detection | Complete | Unit test covers stale `packageVerification.crossChecks.status`. |
| Edited build-manifest output digest detection | Complete | Unit and golden tests cover a changed output digest for `MCM/ExampleMod.json`. |
| Install-preview Markdown summary content revalidation | Open | Candidate for Gate 93. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 93 should add install-preview Markdown summary content revalidation for
`forge package --target mcm-json --verify-existing`.
