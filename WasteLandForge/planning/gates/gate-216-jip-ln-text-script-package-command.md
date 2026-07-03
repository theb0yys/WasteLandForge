# Gate 216 - JIP LN Text-Script Package Command

Status: Complete

## Purpose

Add canonical `forge package --target jip-scripts` package staging and
install-plan skeleton evidence for the existing JIP LN text-script renderer.
This gate writes a loose-file package tree under
`dist/jip-scripts/package/Data/nvse/plugins/scripts`, records
`package-manifest.json`, `install-plan.json`, `build-manifest.json`, and
`checksums.sha256`, and keeps runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, external tool execution, FOMOD generation,
archive generation, and verify-existing expansion out of scope.

## Research grounding

- Documented: ADR-009 says build and package outputs stay under Forge-owned
  `dist/` trees and carry provenance.
- Documented: ADR-010 reserves canonical `forge package` usage without
  aliases.
- Documented: ADR-011 requires deterministic fixture-backed tests, local
  build manifests, checksums, and offline-first release evidence.
- Inferred: JIP script package staging should copy rendered script payloads
  into a package-local `Data/nvse/plugins/scripts` tree and record install
  intent as local evidence only.
- Inferred: The first JIP package gate should write an install-plan skeleton
  but not add a verifier micro-slice, installer, archive, or runtime proof.

## Implemented

- Added `JipScriptPackageEmitter`.
- Added `JipScriptPackageOptions`, `JipScriptPackageResult`,
  `JipScriptPackageOutputs`, and `JipScriptPackageFile`.
- Added `JipScriptPackageJsonSerializer` and `JipScriptPackageTextRenderer`.
- Wired `forge package --target jip-scripts` through the new package emitter.
- Staged rendered JIP text scripts under
  `dist/jip-scripts/package/Data/nvse/plugins/scripts/...`.
- Wrote `dist/jip-scripts/package-manifest.json` with package entries,
  payload digests, non-mutation flags, and archive-not-created evidence.
- Wrote `dist/jip-scripts/install-plan.json` with manual-copy install intent,
  approval-required status, and non-mutation flags.
- Wrote `dist/jip-scripts/build-manifest.json` with package provenance,
  declared capability requirements, source digests, output digests, generator
  metadata, and limitations.
- Wrote `dist/jip-scripts/checksums.sha256` with output-root-relative entries
  for staged scripts and evidence files.
- Supported `--dry-run` planning without writing `dist/`.
- Supported `--output dist/<name>` with `WF-BUILD-001` containment validation.
- Kept `--verify-existing` scoped to `mcm-json`.
- Updated CLI help and CLI JSON/text output.
- Added focused unit coverage and CLI golden coverage.
- Updated planning, generation notes, ADRs, README files, governance docs, and
  project-local prompt routing.

## Not implemented

- No JIP package verify-existing mode.
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
| `forge package --target jip-scripts` invokes a JIP package emitter | Complete |
| Package script files stay under `dist/jip-scripts/package/Data/nvse/plugins/scripts/...` | Complete |
| `package-manifest.json` records staged script payloads and non-mutation limits | Complete |
| `install-plan.json` records manual-copy install intent without installing files | Complete |
| `build-manifest.json` records local package provenance | Complete |
| `checksums.sha256` records staged script and evidence checksums | Complete |
| No live `Data/nvse/plugins/scripts/...` file is written | Complete |
| `--dry-run` plans outputs without writing `dist/` | Complete |
| Output containment outside `dist/` emits `WF-BUILD-001` | Complete |
| Runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, external tools, FOMOD generation, archive generation, and JIP verify-existing remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed with 18 tests.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CliGoldenTests"` passed with 175 tests.

## Next gate

Gate 217 should close the JIP LN text-script command slice and record the next
real Forge value transition. It should avoid a long JIP package verifier
micro-gate chain unless the user explicitly asks for JIP package
verify-existing work.
