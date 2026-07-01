# Gate 86 - MCM Extender Package Archive Entry Revalidation

Status: Complete

## Purpose

Gate 86 adds generated ZIP archive entry-name revalidation to the file-based
MCM Extender package-verification verifier.

This keeps the existing `forge generate`, `forge build`, and `forge package`
command surface unchanged. It remains local generated evidence only: Forge
does not install files into a game Data folder or MO2 profile, inspect MO2 VFS
state, launch the game, or prove in-game MCM Extender runtime visibility.

## Research Grounding

- Documented: ADR-009 requires generated outputs to be deterministic,
  disposable, rebuildable, and traceable through local manifests.
- Documented: ADR-009 says packaged archives require deterministic assembly
  and final package digest recording.
- Documented: ADR-010 requires the stable offline-first CLI command surface;
  do not invent package-verification aliases outside that surface.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Inferred: Revalidating ZIP entry names against generated
  `package-manifest.json` entries is the smallest safe follow-up to Gate 85
  because it verifies archive evidence already produced by the deterministic
  package path without deciding a standalone command shape.
- Open: Standalone verifier command shape, archive entry byte-for-byte payload
  comparison, MO2 profile/VFS inspection, in-game runtime confirmation, and
  FOMOD install packaging remain later work.

## Scope

Gate 86 implements:

- reading expected package entry names from generated `package-manifest.json`,
- opening generated `package.zip` when package-manifest archive status is
  `created`,
- comparing normalized ZIP file entry names against the package manifest
  entries,
- blocking `WF-BUILD-006` diagnostics for unreadable ZIP entries,
- blocking `WF-BUILD-006` diagnostics for missing expected archive entries,
- blocking `WF-BUILD-006` diagnostics for undeclared archive entries,
- targeted unit coverage for an edited generated archive entry.

Gate 86 does not implement:

- a standalone verifier command,
- a new CLI alias or command outside ADR-010,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The file-based verifier now:

1. Loads generated package-verification evidence files.
2. Recomputes generated payload digests.
3. Recomputes generated package archive SHA-256 and byte length when a created
   archive is recorded.
4. Re-opens generated `package.zip` and compares file entry names against
   `package-manifest.json` entries.
5. Emits `WF-BUILD-006` if ZIP entries cannot be read, are missing from the
   archive, or are not declared in the package manifest.
6. Delegates to `McmPackageVerificationEvidenceValidator` with recomputed
   payload and archive digest evidence for cross-checks.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors after an
  initial parallel build/test file-lock warning was rerun cleanly.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 29 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 295 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Targeted trailing-whitespace scan over changed files returned no matches.
- Stale current-Gate-85 wording scan returned no matches.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Archive entry revalidation | Complete | Implemented in the file-based verifier. |
| Edited archive entry mismatch coverage | Complete | Unit test adds an undeclared ZIP entry and expects `WF-BUILD-006`. |
| Standalone package verifier command | Open | Later command-surface decision; do not invent an alias. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 87 should decide how package-verification evidence should surface through
the canonical ADR-010 command model before any public verifier command is
implemented.
