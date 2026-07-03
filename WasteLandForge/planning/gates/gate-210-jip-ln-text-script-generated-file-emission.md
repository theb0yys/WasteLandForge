# Gate 210 - JIP LN Text-Script Generated-File Emission

Status: Complete

## Purpose

Add generated-file emission for rendered JIP LN text scripts under the
project-local `generated/jip-scripts` tree only. This gate writes the Gate 209
in-memory rendered documents to their planned generated paths and continues to
treat game-relative `Data\nvse\plugins\scripts` paths as install metadata
only. It stops before package staging, CLI target wiring, runtime probes,
GECK automation, MO2 VFS inspection, live Data mutation, or external tool
execution.

## Research grounding

- Documented: R006/ADR-009 says generated outputs should live under
  Forge-owned generated or distribution roots, with live game/Data mutation
  outside the safe generation boundary.
- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes and live under `Data\nvse\plugins\scripts`.
- Documented: R006/ADR-009 says JIP text-script generation remains carefully
  bounded and capability-aware.
- Documented: R005/ADR-008 says generated JIP tooling is capability-gated and
  depends on JIP LN plus optional JohnnyGuitar-backed behavior depending on
  FormID-resolution strategy.
- Documented: ADR-010 says command-surface changes belong under canonical
  `forge generate` and `forge build` targets, not aliases.
- Documented: ADR-011 says public fixtures must remain deterministic,
  redistributable, and synthetic where needed.
- Inferred: The first file-emission slice should write only to
  `generated/jip-scripts`, preserving install paths as metadata and leaving
  generated manifest/provenance evidence to the next gate.
- Open: Final emitted-file header, trailing newline, generated output schema,
  generation manifest, CLI target wiring, package staging, runtime capability
  readiness, and install verification remain future gates.

## Implemented

- Added `JipScriptFileEmitter` in `WastelandForge.Generation`.
- Added `JipScriptFileEmissionResult` and `JipScriptGeneratedFile` records.
- Reused `JipScriptTextRenderer` so file emission only happens after existing
  source, schema, semantic, planning, and render checks pass.
- Wrote rendered content to `generated/jip-scripts/nvse/plugins/scripts/...`
  using UTF-8 without a byte-order mark.
- Added containment checks so planned generated files must resolve under the
  project-local `generated/jip-scripts` output root.
- Preserved Data/install path metadata without writing to `Data`.
- Added unit coverage for generated-root-only file writes and
  validation-error no-write behavior.
- Updated planning, generation notes, ADRs, governance docs, README files, and
  project-local prompt routing.

## Not implemented

- No generation manifest.
- No output digest/checksum report.
- No generated output schema.
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
| Valid rendered JIP documents are written under `generated/jip-scripts` | Complete |
| File emission uses UTF-8 without a byte-order mark | Complete |
| Generated output path containment is checked against `generated/jip-scripts` | Complete |
| Game-relative `Data/...` path remains metadata and is not written | Complete |
| Validation errors prevent generated-file writes | Complete |
| Package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 211 should add a generated JIP emission manifest and digest skeleton under
`generated/jip-scripts`. It should stop before package staging, CLI target
wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data
mutation, or external tool execution.
