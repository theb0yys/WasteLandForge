# Gate 209 - JIP LN Text-Script In-Memory Renderer Skeleton

Status: Complete

## Purpose

Add an in-memory renderer skeleton for validated JIP LN text-script source
contracts. This gate converts validated opaque source lines into deterministic
in-memory text documents while preserving the Gate 208 plan metadata. It stops
before file emission, package staging, CLI target wiring, runtime probes, GECK
automation, MO2 VFS inspection, live Data mutation, or external tool
execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN text scripts are carefully bounded
  second-wave game-support outputs.
- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes and live under `Data\nvse\plugins\scripts`.
- Documented: R006/ADR-009 says generated artifacts are disposable and
  rebuildable, and generated files should live under Forge-owned output roots.
- Documented: R005/ADR-008 says generated JIP tooling is capability-gated and
  depends on JIP LN plus optional JohnnyGuitar-backed behavior depending on
  FormID-resolution strategy.
- Documented: ADR-010 says command-surface changes belong under canonical
  `forge generate` and `forge build` targets, not aliases.
- Documented: ADR-011 says public fixtures must remain deterministic,
  redistributable, and synthetic where needed.
- Inferred: Rendering opaque source lines in memory before file emission gives
  the future generator a testable formatting boundary without mutating
  generated outputs or live game/MO2 Data trees.
- Open: Final emitted-file header, trailing newline, generated output schema,
  CLI target wiring, package staging, runtime capability readiness, and install
  verification remain future gates.

## Implemented

- Added `JipScriptTextRenderer` in `WastelandForge.Generation`.
- Added `JipScriptTextRenderResult` and `JipScriptRenderedDocument` records.
- Reused the existing validation/read pipeline and Gate 208 plan-entry
  projection before rendering.
- Rendered opaque source lines in memory with LF separators.
- Recorded in-memory content bytes using UTF-8 byte counting.
- Preserved generated path, Data path, install path, source location, and
  diagnostics in the render result.
- Added unit coverage for successful in-memory rendering and validation-error
  short-circuit behavior.
- Updated planning, generation notes, ADRs, governance docs, README files, and
  project-local prompt routing.

## Not implemented

- No generated JIP script files.
- No file emission.
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
| Valid JIP source contracts render to in-memory text documents | Complete |
| Rendered documents preserve Gate 208 generated/Data/install path metadata | Complete |
| Rendered content uses LF separators for opaque source lines | Complete |
| Rendered content byte count uses UTF-8 | Complete |
| Renderer returns no documents when existing validation reports errors | Complete |
| Renderer does not create generated files or live Data files | Complete |
| File emission, package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 210 should add generated-file emission for rendered JIP LN text scripts
under `generated/jip-scripts` only. It should stop before package staging, CLI
target wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data
mutation, or external tool execution.
