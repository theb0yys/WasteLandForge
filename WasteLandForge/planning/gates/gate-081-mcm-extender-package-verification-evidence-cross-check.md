# Gate 81 - MCM Extender Package Verification Evidence Cross-Check

Status: Complete

## Purpose

Gate 81 adds deterministic consistency checks between generated
package-verification evidence and the package evidence it summarizes.

This is still local generated evidence only. It does not install files into a
game Data folder or MO2 profile, does not inspect MO2 VFS state, and does not
prove in-game MCM Extender runtime visibility.

## Research Grounding

- Documented: ADR-009 requires generated outputs to be deterministic,
  disposable, rebuildable, and traceable through local manifests.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Documented: R004 says generated artifacts should prove their source through
  local build manifests and output digests.
- Inferred: Package-verification evidence should be checked against package
  manifest, install-preview, payload digest, archive, and summary evidence
  before manifest/checksum finalization.
- Open: Reusable package verifier components, standalone verifier command
  shape, MO2 profile/VFS inspection, in-game runtime confirmation, and FOMOD
  install packaging remain later work.

## Scope

Gate 81 implements:

- deterministic package-verification evidence cross-checks before final
  manifests and checksums are written,
- blocking `WF-BUILD-006` diagnostics for cross-check mismatches,
- local manifest `packageVerification.crossChecks` evidence,
- package-verification Markdown cross-check summary lines,
- unit and golden CLI assertions for successful cross-check evidence.

Gate 81 does not implement:

- a standalone verifier command,
- a reusable public package-verification API,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The cross-check stage compares:

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

## Planned Checks

Run:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore
dotnet test WastelandForge.sln --no-build --no-restore -m:1
git diff --check
```

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 19 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 285 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Evidence cross-checks | Complete | Implemented and validated in Gate 81. |
| Reusable verifier component | Open | Candidate for Gate 82. |
| Standalone package verifier command | Open | Later command-surface decision; do not invent an alias. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 82 should extract package-verification checking into a reusable verifier
component with targeted mismatch tests while keeping the CLI command surface
unchanged.
