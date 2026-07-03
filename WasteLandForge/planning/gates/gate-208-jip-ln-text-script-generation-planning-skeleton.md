# Gate 208 - JIP LN Text-Script Generation Planning Skeleton

Status: Complete

## Purpose

Add a non-emitting generation planning model for validated JIP LN text-script
source contracts. This gate records deterministic future output metadata for
each manifest-declared JIP script, including generated path intent,
game-relative Data path intent, install path intent, source byte budget,
required capabilities, and source location. It stops before text emission,
package staging, CLI target wiring, runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, or external tool execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes and live under `Data\nvse\plugins\scripts`.
- Documented: R006/ADR-009 says JIP script generation must be explicit about
  event prefixes, size budgeting, provider requirements, and
  FormID-resolution strategy.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  under Forge-owned output roots, with provenance and build-manifest evidence.
- Documented: ADR-010 says user-facing generation belongs under canonical
  `forge generate` and `forge build` targets, not ad hoc aliases.
- Documented: ADR-011 says validation remains layered and fixtures remain
  deterministic, redistributable, and synthetic where public.
- Inferred: A planner should consume the existing validated JIP source read
  model before text rendering so future emission can share one deterministic
  path and byte-budget projection.
- Open: Final emitted text formatting, line endings, encoding/header policy,
  generated output schema, CLI target wiring, package staging, runtime
  capability readiness, and install verification remain future gates.

## Implemented

- Added `JipScriptGenerationPlanner` in `WastelandForge.Generation`.
- Added `JipScriptGenerationPlanResult` and `JipScriptGenerationPlanEntry`
  records.
- Reused `ProjectValidationPipeline.Validate` and `ReadJipScripts` so planning
  only proceeds after existing source, schema, and semantic validation.
- Recorded deterministic `generated/jip-scripts/nvse/plugins/scripts/...`,
  `nvse/plugins/scripts/...`, and `Data/nvse/plugins/scripts/...` path
  metadata without creating directories or files.
- Recorded source-body byte counts using the same UTF-8 plus LF separator
  calculation used by source-line budget validation.
- Added unit coverage for successful non-emitting planning and validation-error
  short-circuit behavior.
- Updated planning, generation notes, ADRs, governance docs, README files, and
  project-local prompt routing.

## Not implemented

- No emitted JIP script text.
- No in-memory renderer.
- No generated JIP script files.
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
| Valid JIP source contracts produce deterministic plan entries | Complete |
| Plan entries record generated path, Data path, install path, capability, size, and source metadata | Complete |
| Source byte count matches Gate 206 source-level UTF-8 plus LF calculation | Complete |
| Planner returns no entries when existing validation reports errors | Complete |
| Planner does not create generated files or live Data files | Complete |
| Text emission, package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 209 should add an in-memory JIP LN text-script renderer skeleton for
validated opaque source lines. It should stop before file emission, package
staging, CLI target wiring, runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, or external tool execution.
