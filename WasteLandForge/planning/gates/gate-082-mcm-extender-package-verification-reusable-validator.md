# Gate 82 - MCM Extender Package Verification Reusable Validator

Status: Complete

## Purpose

Gate 82 extracts MCM Extender package-verification evidence checks into a
reusable validator component.

This keeps the existing `forge generate`, `forge build`, and `forge package`
command surface unchanged. It remains local generated evidence only: Forge
does not install files into a game Data folder or MO2 profile, inspect MO2 VFS
state, launch the game, or prove in-game MCM Extender runtime visibility.

## Research Grounding

- Documented: ADR-009 requires generated outputs to be deterministic,
  disposable, rebuildable, and traceable through local manifests.
- Documented: ADR-010 requires the stable offline-first CLI command surface;
  do not invent package-verification aliases outside that surface.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Documented: R004 says generated artifacts should prove their source through
  local build manifests and output digests.
- Inferred: Package-verification evidence checks should be reusable before a
  later file-based verifier or command-surface decision is made.
- Open: Standalone verifier command shape, MO2 profile/VFS inspection,
  in-game runtime confirmation, and FOMOD install packaging remain later work.

## Scope

Gate 82 implements:

- reusable `McmPackageVerificationEvidenceValidator`,
- reusable `McmPackageVerificationEvidenceValidationRequest`,
- the same blocking `WF-BUILD-006` diagnostics for evidence mismatches,
- the same manifest `packageVerification.crossChecks` evidence shape,
- targeted unit tests for consistent evidence and mismatch diagnostics.

Gate 82 does not implement:

- a standalone verifier command,
- a new CLI alias or command outside ADR-010,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The reusable validator compares:

- package root across package manifest, install-preview, and
  package-verification report,
- package entry counts across package manifest, install-preview, and
  package-verification report,
- payload digest counts across package manifest, computed payload digests, and
  package-verification report,
- archive status and validation across package manifest, install-preview, and
  package-verification report,
- package-verification Markdown summary lines against the validated JSON
  report.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 23 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 289 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Reusable validator component | Complete | Implemented in `WastelandForge.Generation`. |
| Targeted mismatch tests | Complete | Covers package root, payload digest count, and summary mismatches. |
| Standalone package verifier command | Open | Later command-surface decision; do not invent an alias. |
| File-based verifier entrypoint | Open | Candidate for Gate 83 using the reusable validator. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 83 should add a file-based package verification entrypoint that reuses
`McmPackageVerificationEvidenceValidator` while preserving the ADR-010 command
surface until the command shape is explicitly decided.
