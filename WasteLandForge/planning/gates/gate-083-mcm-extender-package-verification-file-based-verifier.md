# Gate 83 - MCM Extender Package Verification File-Based Verifier

Status: Complete

## Purpose

Gate 83 adds a file-based MCM Extender package-verification verifier that
reuses the Gate 82 package-verification evidence validator.

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
- Inferred: A file-based verifier is the smallest safe step after the reusable
  validator because it can verify generated evidence already on disk without
  deciding a standalone command shape.
- Open: Standalone verifier command shape, payload digest recomputation,
  MO2 profile/VFS inspection, in-game runtime confirmation, and FOMOD install
  packaging remain later work.

## Scope

Gate 83 implements:

- `McmPackageVerificationEvidenceFileVerifier`,
- `McmPackageVerificationEvidenceFileVerificationRequest`,
- file loading for generated package manifest, install-preview,
  package-verification JSON, and package-verification Markdown evidence,
- reconstruction of payload and archive digest evidence from
  `package-manifest.json`,
- delegation to the reusable Gate 82 validator,
- targeted unit tests over generated files.

Gate 83 does not implement:

- a standalone verifier command,
- a new CLI alias or command outside ADR-010,
- payload digest recomputation from package files,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The file-based verifier loads:

- `package-manifest.json`,
- `install-preview.json`,
- `package-verification.json`,
- `package-verification.md`.

It reconstructs:

- package payload digest evidence from `payloadDigests`,
- package archive evidence from `archive` when the archive status is
  `created`.

Then it delegates to `McmPackageVerificationEvidenceValidator` for the same
`WF-BUILD-006` cross-check diagnostics used by generated runs.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore` passed 26 tests.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 31 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 292 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| File-based verifier entrypoint | Complete | Implemented in `WastelandForge.Generation`. |
| Generated-file verifier tests | Complete | Covers generate evidence, build evidence with archive, and edited evidence mismatch. |
| Payload digest recomputation | Open | Candidate for Gate 84. |
| Standalone package verifier command | Open | Later command-surface decision; do not invent an alias. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 84 should add package payload digest recomputation to the file-based
verifier so generated package files can be checked against
`package-manifest.json` payload digest evidence before any standalone command
shape is decided.
