# Gate 84 - MCM Extender Package Payload Digest Verification

Status: Complete

## Purpose

Gate 84 adds package payload digest recomputation to the file-based MCM
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
- Documented: ADR-010 requires the stable offline-first CLI command surface;
  do not invent package-verification aliases outside that surface.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Documented: R004 says generated artifacts should prove their source through
  local build manifests and output digests.
- Inferred: Recomputing payload digests from generated files is the smallest
  safe follow-up to the Gate 83 file-based verifier because it verifies the
  package manifest against actual generated payload files without deciding a
  standalone command shape.
- Open: Archive digest recomputation, standalone verifier command shape, MO2
  profile/VFS inspection, in-game runtime confirmation, and FOMOD install
  packaging remain later work.

## Scope

Gate 84 implements:

- SHA-256 and length recomputation for package payload files listed in
  `package-manifest.json`,
- blocking `WF-BUILD-006` diagnostics when payload files cannot be read,
- blocking `WF-BUILD-006` diagnostics when recomputed payload digest evidence
  does not match `package-manifest.json`,
- delegation to the reusable Gate 82 validator using recomputed payload
  digest evidence,
- targeted unit coverage for an edited generated payload file.

Gate 84 does not implement:

- a standalone verifier command,
- a new CLI alias or command outside ADR-010,
- archive digest recomputation,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The file-based verifier now:

1. Loads generated package-verification evidence files.
2. Reads `payloadDigests` from `package-manifest.json`.
3. Recomputes SHA-256 and byte length for each listed payload file.
4. Emits `WF-BUILD-006` if a payload cannot be read or the recomputed digest
   does not match manifest evidence.
5. Delegates to `McmPackageVerificationEvidenceValidator` with recomputed
   payload digest evidence for count cross-checks.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 27 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 293 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Payload digest recomputation | Complete | Implemented in the file-based verifier. |
| Edited payload mismatch coverage | Complete | Unit test edits generated MCM JSON payload and expects `WF-BUILD-006`. |
| Archive digest recomputation | Open | Candidate for Gate 85. |
| Standalone package verifier command | Open | Later command-surface decision; do not invent an alias. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 85 should add archive digest recomputation to the file-based verifier so
`package.zip` can be checked against `package-manifest.json` archive digest
evidence before any standalone command shape is decided.
