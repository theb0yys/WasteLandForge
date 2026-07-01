# Gate 85 - MCM Extender Package Archive Digest Verification

Status: Complete

## Purpose

Gate 85 adds package archive digest recomputation to the file-based MCM
Extender package-verification verifier.

This keeps the existing `forge generate`, `forge build`, and `forge package`
command surface unchanged. It remains local generated evidence only: Forge
does not install files into a game Data folder or MO2 profile, inspect MO2 VFS
state, launch the game, or prove in-game MCM Extender runtime visibility.

## Research Grounding

- Documented: ADR-009 requires generated outputs to be deterministic,
  disposable, rebuildable, and traceable through local manifests.
- Documented: ADR-009 and the generator/build report recommend cryptographic
  checksums and hash-based build state for comparing generated artifacts.
- Documented: ADR-009 says packaged archives require deterministic assembly
  and final package digest recording.
- Documented: ADR-010 requires the stable offline-first CLI command surface;
  do not invent package-verification aliases outside that surface.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Inferred: Recomputing archive digests from generated `package.zip` files is
  the smallest safe follow-up to Gate 84 because it verifies archive evidence
  already recorded in `package-manifest.json` without deciding a standalone
  command shape.
- Open: Archive entry revalidation, standalone verifier command shape, MO2
  profile/VFS inspection, in-game runtime confirmation, and FOMOD install
  packaging remain later work.

## Scope

Gate 85 implements:

- SHA-256 and length recomputation for generated `package.zip` when
  `package-manifest.json` records a created archive,
- blocking `WF-BUILD-006` diagnostics when the archive file cannot be read,
- blocking `WF-BUILD-006` diagnostics when recomputed archive digest evidence
  does not match `package-manifest.json`,
- delegation to the reusable Gate 82 validator using recomputed archive
  digest evidence,
- targeted unit coverage for an edited generated archive file.

Gate 85 does not implement:

- a standalone verifier command,
- a new CLI alias or command outside ADR-010,
- archive entry revalidation,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The file-based verifier now:

1. Loads generated package-verification evidence files.
2. Reads archive digest evidence from `package-manifest.json`.
3. Recomputes SHA-256 and byte length for generated `package.zip` when the
   archive status is `created`.
4. Emits `WF-BUILD-006` if the archive cannot be read or the recomputed digest
   does not match manifest evidence.
5. Delegates to `McmPackageVerificationEvidenceValidator` with recomputed
   archive digest evidence for archive cross-checks.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 28 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 294 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Archive digest recomputation | Complete | Implemented in the file-based verifier. |
| Edited archive mismatch coverage | Complete | Unit test edits generated `package.zip` and expects `WF-BUILD-006`. |
| Archive entry revalidation | Open | Candidate for Gate 86. |
| Standalone package verifier command | Open | Later command-surface decision; do not invent an alias. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 86 should add archive entry revalidation to the file-based verifier so
`package.zip` entry names can be checked against `package-manifest.json`
payload evidence before any standalone command shape is decided.
