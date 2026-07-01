# Gate 93 - MCM Extender Package Verify-Existing Install-Preview Summary Content Revalidation

Status: Complete

## Purpose

Gate 93 adds install-preview Markdown summary content revalidation for existing
MCM Extender package evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate reuses the existing file-based package verifier and extends it to
read `install-preview.md` from the selected package output root.

## Research Grounding

- Documented: ADR-009 requires generated artifacts to be deterministic,
  disposable, rebuildable, and traceable through local build evidence.
- Documented: ADR-010 defines the canonical command surface and rejects
  undocumented verifier aliases.
- Documented: ADR-011 requires layered validation and deterministic
  fixture-backed testing.
- Documented: Gate 77 defines `install-preview.md` as a deterministic human
  summary beside the schema-validated install-preview JSON report.
- Inferred: `install-preview.md` can be revalidated in verify-existing mode
  because Gates 88-92 already established package verification as an
  existing-evidence diagnostic mode.
- Open: Entry-level install-preview/package-manifest cross-checking remains a
  separate package-verifier gate.

## Scope

Gate 93 implements:

- `install-preview.md` loading for package verify-existing mode,
- install-preview summary header and provenance line checks,
- project, command, target, package root, install root, mode, and preview-only
  flag checks,
- entry count, archive status, and archive validation checks,
- would-copy entry line checks, including declared source and target file
  details,
- limitation line checks,
- targeted tests for edited install-preview summary content,
- updated golden CLI coverage for install-preview summary diagnostics.

Gate 93 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- entry-level install-preview/package-manifest cross-checking,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Resolves `dist/mcm-json/install-preview.md` by default, or the
   corresponding install-preview summary under a custom `--output <path>`
   inside project `dist/`.
2. Reads package manifest, install preview, install-preview summary, package
   verification, package-verification summary, checksum, and build-manifest
   evidence without regenerating package outputs.
3. Compares required install-preview Markdown lines against
   `install-preview.json`.
4. Reports blocking `WF-BUILD-006` diagnostics when install-preview summary
   content is missing, inconsistent, or stale.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 34 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 43 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed
  the full suite: 312 tests.
- `git diff --check` passed with line-ending warnings only.
- Trailing-whitespace scan returned no matches.
- Stale current-Gate-92 wording scan returned no matches.
- Protected-file scan returned no matches in the scanned project paths; no
  protected project files were touched.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Edited install-preview summary count detection | Complete | Unit and golden tests cover stale `Entries` summary content. |
| Install-preview would-copy line revalidation | Complete | The verifier checks every install-preview entry line against JSON evidence. |
| Install-preview/package-manifest entry cross-checking | Open | Candidate for Gate 94. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 94 should add install-preview/package-manifest entry content cross-check
revalidation for `forge package --target mcm-json --verify-existing`.
