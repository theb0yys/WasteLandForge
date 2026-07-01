# Gate 73 - MCM Extender Package Command Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 62, Gate 63, Gate 64, Gate 70, Gate 71, Gate 72, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Definition

Gate 73 exposes the canonical `forge package` command for the existing MCM
Extender package path:

- `forge package` is implemented as a top-level ADR-010 command,
- the command defaults to `--target mcm-json`,
- `forge package --target mcm-json` writes the deterministic package tree under
  project `dist/mcm-json`,
- package output includes `package-manifest.json`, `package.zip`,
  `build-manifest.json`, and `checksums.sha256`,
- package manifests and build manifests record `command: package`,
- the local build manifest records build type
  `wastelandforge/package-mcm-json/v1`,
- human/plain/json output is supported,
- `--dry-run`, `--project`, `--output`, `--target`, and `--no-input` are
  accepted for the package command.

Gate 73 does not create FOMOD installers, inspect MO2 profile/VFS visibility,
launch the game, verify the menu in-game, generate plugin records, or publish
release assets.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| `forge package` is part of the canonical ADR-010 command surface. | Documented | R006 CLI workflow report / ADR-010 |
| `forge package` takes build outputs and package config and produces a staged package or archive. | Documented | R006 CLI workflow report |
| Generated package outputs belong under `dist/`. | Documented | R006 CLI workflow report / ADR-009 |
| The v0.1 package target should be deterministic ZIP staging plus metadata; FOMOD is later. | Documented | Generator/build pipeline report |
| Package digests and provenance must be recorded locally. | Documented | Generator/build pipeline report / ADR-011 |
| Reusing the Gate 72 MCM package tree for the first `forge package` command is the smallest safe command-surface step. | Inferred | Gate 72 archive evidence plus ADR-010 command boundary |
| Formal package/archive schema, MO2 profile visibility, FOMOD shape, and in-game verification remain unresolved. | Open | Package validation/runtime verification gates not yet implemented |

## Deliverables

- Unreserve `forge package` and route it to a real command handler.
- Parse package flags with the same offline-first CLI conventions as build and
  generate.
- Default the package target to `mcm-json`; reject unsupported package targets.
- Treat MCM package mode as a distribution-writing command under `dist/`.
- Write package-specific provenance in the MCM build manifest.
- Update unit and golden CLI tests.
- Update current-gate documentation, ADR/governance notes, and `/forge`
  routing prompts.

## Validation mapping

Gate 73 uses this path:

```text
forge package --target mcm-json
  -> manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename validation
  -> generated MCM JSON output schema validation
  -> referenced texture asset loose-file staging
  -> package-manifest metadata emission
  -> deterministic ZIP archive creation
  -> package-specific build manifest and checksum evidence
```

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- package <temp-copy-of-ExampleMod> --format json --no-input
git diff --check
```

Results:

- `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore` passed with 0 warnings and 0 errors.
- Focused project tests passed:
  - `WastelandForge.UnitTests`: 19 passed.
  - `WastelandForge.GoldenTests`: 31 passed.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1` passed with 273 total tests, 0 failed, and 0 skipped.
- CLI smoke validation on a temporary copy of the synthetic `ExampleMod` fixture passed:
  - `package --format json --no-input` exited 0.
  - JSON output reported `command: package`, `status: passed`, `target: mcm-json`, and `outputs.packageArchive: dist/mcm-json/package.zip`.
  - `package-manifest.json` recorded `command: package` and archive status `created`.
  - `build-manifest.json` recorded `command: package` and build type `wastelandforge/package-mcm-json/v1`.
  - `checksums.sha256` included `package.zip`.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Formal package/archive schema | Open | Candidate for Gate 74. |
| Package manifest validation | Open | Gate 73 emits metadata but does not validate it against a published schema. |
| Archive entry validation command | Open | Gate 73 relies on generator tests and checksums, not a standalone package validator. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| Full DDS metadata validation | Open | Gate 73 still only uses existing DDS magic-header validation. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 74 should add a formal MCM package manifest/archive validation skeleton,
starting with schema-backed validation of `package-manifest.json` and archive
entry evidence before FOMOD, MO2, or in-game verification work.
