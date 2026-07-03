# Gate 211 - JIP LN Text-Script Emission Manifest And Digests

Status: Complete

## Purpose

Add local manifest, checksum, and digest evidence for generated JIP LN
text-script emission under the project-local `generated/jip-scripts` tree.
This gate makes Gate 210 file emission traceable without turning it into a CLI
target, build graph target, package staging step, runtime probe, GECK
automation, MO2 VFS inspection, live Data mutation, or external tool execution.

## Research grounding

- Documented: ADR-009 says generated outputs are disposable, rebuildable, and
  must carry provenance.
- Documented: R006/ADR-009 says generated outputs should live under
  Forge-owned generated or distribution roots.
- Documented: R006/ADR-009 says JIP LN Script Runner scripts live in
  `Data\nvse\plugins\scripts`, selected by lifecycle filename prefixes.
- Documented: ADR-010 says command-surface changes belong under canonical
  `forge generate` and `forge build` targets, not aliases.
- Documented: ADR-011 requires deterministic fixture-backed testing and local
  build/report evidence.
- Inferred: The first JIP emission evidence slice should record the generated
  script payloads and their hashes under `generated/jip-scripts` without
  making checksum files self-referential.
- Open: The generated JIP emission manifest schema, manifest revalidation,
  full build-manifest integration, CLI target wiring, package staging, runtime
  readiness checks, and install verification remain future gates.

## Implemented

- Extended `JipScriptFileEmitter` to write
  `generated/jip-scripts/jip-script-emission-manifest.json`.
- Extended `JipScriptFileEmitter` to write
  `generated/jip-scripts/checksums.sha256`.
- Added generated script payload digests to the manifest `outputs` array.
- Added package non-mutation flags to the manifest:
  `writesToGameData`, `writesToMo2Profile`, and `launchesGame` are all false.
- Returned manifest path, checksum path, and output digest metadata from
  `JipScriptFileEmissionResult`.
- Kept output digest metadata non-self-referential: generated script files and
  the manifest are digested, while `checksums.sha256` is written but not
  included in the digest list.
- Added focused unit assertions for manifest content, checksum content,
  output digest content, checksum exclusion, and validation-error no-manifest
  behavior.
- Updated planning, generation notes, ADRs, governance docs, README files, and
  project-local prompt routing.

## Not implemented

- No generated JIP emission manifest schema.
- No generated JIP emission manifest validation.
- No build graph target.
- No `forge generate` or `forge build` target wiring.
- No package staging.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No new diagnostic rule ID.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| JIP file emission writes a manifest under `generated/jip-scripts` | Complete |
| JIP file emission writes a checksum sidecar under `generated/jip-scripts` | Complete |
| Manifest records script metadata, generated payload digests, package non-mutation flags, and limitations | Complete |
| Output digest records cover generated scripts plus the manifest, excluding the checksum file | Complete |
| Validation errors prevent generated-file, manifest, checksum, and digest output | Complete |
| Package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors after fixing
  manifest source-pointer serialization.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed with 6 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed with
  545 tests.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.
- `rg -n "[ \t]$" . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!generated/**' --glob '!dist/**'` found no trailing whitespace in tracked or untracked text files outside build output trees.
- Protected path scan found no matching protected filenames. Protected content
  scan found only older gate audit lines that mention the protected-scan
  phrase, not protected project content.

## Next gate

Gate 212 should add a generated JIP emission manifest schema and validation
skeleton. It should stop before package staging, CLI target wiring, runtime
probes, GECK automation, MO2 VFS inspection, live Data mutation, or external
tool execution.
