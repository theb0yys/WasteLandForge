# Gate 80 - MCM Extender Package Verification Human Summary

Status: Complete

## Purpose

Gate 80 adds a deterministic human-readable summary beside the Gate 79
schema-validated package-verification JSON report for the first MCM Extender
package path.

The summary is generated evidence only. It does not become source truth, does
not install into a game Data folder or MO2 profile, does not inspect MO2 VFS
state, and does not prove in-game MCM Extender runtime visibility.

## Research Grounding

- Documented: ADR-009 requires generated outputs to be deterministic,
  disposable, rebuildable, and traceable through local manifests.
- Documented: ADR-010 requires a small stable CLI with machine-readable output
  and human-readable command output.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Inferred: After Gate 79 validates `package-verification.json`, a Markdown
  summary can safely summarize the same local evidence for human review.
- Open: A standalone package verifier command, MO2 profile/VFS inspection,
  in-game runtime confirmation, and FOMOD install packaging remain later work.

## Scope

Gate 80 implements:

- `generated/mcm-json/package-verification.md` for
  `forge generate --target mcm-json`.
- `dist/mcm-json/package-verification.md` for
  `forge build --target mcm-json`.
- `dist/mcm-json/package-verification.md` for
  `forge package --target mcm-json`.
- Manifest `packageVerification.summary` evidence.
- CLI JSON `outputs.packageVerificationSummary` evidence.
- Human CLI output for the summary file.
- Output digest and build/package checksum coverage for the summary file.
- Unit and golden CLI assertions over generated summary content and evidence
  wiring.

Gate 80 does not implement:

- a standalone verifier command,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The generated Markdown summary includes:

- package root,
- command,
- target,
- package layout,
- menu, translation, asset, and entry counts,
- package archive status,
- package manifest schema check,
- install-preview schema check,
- install-preview summary presence,
- package payload digest count,
- archive validation status,
- explicit local-only verification limitations.

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
| Package-verification human summary | Complete | Implemented and validated in Gate 80. |
| Evidence cross-checks | Open | Candidate for Gate 81. |
| Standalone package verifier | Open | Later command surface decision. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 81 should add deterministic evidence cross-checks that the
package-verification report and summary agree with package manifest,
install-preview, payload digest, and optional archive evidence.
