# Gate 214 - JIP LN Text-Script Generate Command

Status: Complete

## Purpose

Add canonical `forge generate --target jip-scripts` CLI target wiring for the
existing generated-root JIP LN text-script emitter. This gate makes the JIP
text-script slice accessible through the real `forge generate` command while
keeping `forge build` target wiring, package staging, runtime probes, GECK
automation, MO2 VFS inspection, live Data mutation, external tool execution,
and command aliases out of scope.

## Research grounding

- Documented: ADR-010 reserves the canonical command surface and includes
  `forge generate` and `forge build` without aliases.
- Documented: ADR-009 says `forge generate` produces generated artifacts and
  provenance, while `forge build` runs the fuller build pipeline.
- Documented: ADR-009 keeps generated outputs disposable and rebuildable under
  `generated/` or `dist/`.
- Documented: ADR-011 requires deterministic fixture-backed testing and local
  generated evidence.
- Inferred: JIP script generate target wiring should reuse the existing
  generated-root emitter and should not silently accept unsupported custom
  output roots or dry-run behavior.
- Inferred: `forge build --target jip-scripts` should remain unsupported until
  a separate dist/build-manifest slice exists.

## Implemented

- Added `forge generate --target jip-scripts` target parsing.
- Reused `JipScriptFileEmitter` from the CLI target path.
- Added JIP generate JSON output with project, status, generated script
  metadata, output paths, diagnostics, and output digests.
- Added JIP generate text output for human/plain modes.
- Rejected `--output` and `--dry-run` for `jip-scripts` in this gate instead
  of ignoring unsupported options.
- Kept `forge build --target jip-scripts` unsupported.
- Updated `forge generate --help` to list the new target and output evidence.
- Updated generated JIP emission manifest limitations from "No CLI target
  wiring" to "No build target wiring."
- Added CLI golden coverage for successful JIP generate output and for the
  unsupported build target boundary.
- Updated planning, generation notes, ADRs, README files, governance docs, and
  project-local prompt routing.

## Not implemented

- No `forge build --target jip-scripts` target wiring.
- No package staging.
- No install plan for JIP script payloads.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No command alias.
- No public fixture using Bethesda assets or third-party mod files.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| `forge generate --target jip-scripts` invokes the existing JIP file emitter | Complete |
| CLI JSON output reports generated script, manifest, checksum, diagnostics, and output digests | Complete |
| Generated file remains under `generated/jip-scripts/nvse/plugins/scripts/...` | Complete |
| No live `Data/nvse/plugins/scripts/...` file is written | Complete |
| `forge build --target jip-scripts` remains unsupported | Complete |
| Package staging, runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CliGoldenTests"` passed with 174 tests.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed with 10 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed with
  554 tests.
- `git diff --check` passed. Git reported line-ending warnings for working
  tree text files, but returned success.
- `rg -n "[ \t]$" . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!generated/**' --glob '!dist/**'` found no trailing whitespace outside build output trees.
- Protected path scan found no matching protected filenames. Protected content
  scan found only older gate audit lines that mention the protected-scan
  phrase, not protected project content.

## Next gate

Gate 215 should add canonical `forge build --target jip-scripts` dist/build
target wiring with local build-manifest and checksum evidence. It should stop
before package staging, runtime probes, GECK automation, MO2 VFS inspection,
live Data mutation, external tool execution, or installer/archive generation.
