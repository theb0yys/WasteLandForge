# Gate 215 - JIP LN Text-Script Build Command

Status: Complete

## Purpose

Add canonical `forge build --target jip-scripts` dist output wiring for the
existing JIP LN text-script renderer. This gate writes build-owned JIP script
payloads under `dist/jip-scripts`, records local `build-manifest.json`
provenance, writes `checksums.sha256`, and keeps package staging, runtime
probes, GECK automation, MO2 VFS inspection, live Data mutation, external tool
execution, and installer/archive generation out of scope.

## Research grounding

- Documented: ADR-009 says `forge build` runs the fuller build pipeline after
  generation and writes local provenance evidence.
- Documented: ADR-009 keeps build outputs under `dist/` and generated outputs
  under `generated/`.
- Documented: ADR-010 reserves canonical `forge build` target usage without
  aliases.
- Documented: ADR-011 requires deterministic fixture-backed tests, mandatory
  local build manifests, and checksums.
- Inferred: The first JIP build target should reuse validated rendered JIP
  source lines and write build evidence under `dist/jip-scripts` only.
- Inferred: `Data/nvse/plugins/scripts/...` remains install metadata until a
  later package/install-plan gate.

## Implemented

- Added `JipScriptBuildEmitter`.
- Added `JipScriptBuildOptions`, `JipScriptBuildResult`,
  `JipScriptBuildOutputs`, and `JipScriptBuildFile`.
- Added `JipScriptBuildJsonSerializer` and `JipScriptBuildTextRenderer`.
- Wired `forge build --target jip-scripts` through the new build emitter.
- Wrote rendered JIP text scripts under
  `dist/jip-scripts/nvse/plugins/scripts/...`.
- Wrote `dist/jip-scripts/build-manifest.json` with project, validation,
  declared capability, package non-mutation, generator, source digest, script,
  output digest, and limitation evidence.
- Wrote `dist/jip-scripts/checksums.sha256` with output-root-relative entries
  for built scripts and `build-manifest.json`.
- Supported `--dry-run` planning without writing `dist/`.
- Supported `--output dist/<name>` with `WF-BUILD-001` containment validation.
- Kept `forge generate --target jip-scripts` guards for unsupported `--output`
  and `--dry-run`.
- Updated CLI help and CLI JSON/text output.
- Added focused unit coverage and CLI golden coverage.
- Updated planning, generation notes, ADRs, README files, governance docs, and
  project-local prompt routing.

## Not implemented

- No package staging.
- No install preview or install plan for JIP script payloads.
- No package archive.
- No FOMOD installer.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No public fixture using Bethesda assets or third-party mod files.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| `forge build --target jip-scripts` invokes a JIP build emitter | Complete |
| Built script files stay under `dist/jip-scripts/nvse/plugins/scripts/...` | Complete |
| `build-manifest.json` records local build provenance and non-mutation limits | Complete |
| `checksums.sha256` records built script and manifest checksums | Complete |
| No live `Data/nvse/plugins/scripts/...` file is written | Complete |
| `--dry-run` plans outputs without writing `dist/` | Complete |
| Output containment outside `dist/` emits `WF-BUILD-001` | Complete |
| Package staging, runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, external tools, and installer/archive generation remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed with 14 tests.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CliGoldenTests"` passed with 174 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed with
  558 tests.
- `git diff --check` passed. Git reported line-ending warnings for working
  tree text files, but returned success.
- `rg -n "[ \t]$" . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!generated/**' --glob '!dist/**'` found no trailing whitespace outside build output trees.
- Protected path scan found no matching protected filenames. Protected content
  scan found only older gate audit lines that mention the protected-scan
  phrase, not protected project content.

## Next gate

Gate 216 should add canonical `forge package --target jip-scripts` package
staging and install-plan skeleton evidence for built JIP scripts. It should
stop before runtime probes, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, FOMOD generation, or archive generation.
