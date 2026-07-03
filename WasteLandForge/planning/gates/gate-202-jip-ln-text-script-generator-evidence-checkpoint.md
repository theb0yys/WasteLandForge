# Gate 202 - JIP LN Text-Script Generator Evidence Checkpoint

Status: Complete

## Purpose

Start the next real mod-building function slice by recording the JIP LN
text-script generator evidence, output boundaries, and safety limits before
adding source contracts, schemas, generator code, CLI wiring, fixtures,
runtime probes, GECK automation, MO2 VFS inspection, or external tool
execution.

## Research grounding

- Documented: R006/ADR-009 says deterministic text artefacts and MCM Extender
  JSON are the first generator targets, and carefully bounded JIP LN text
  scripts are a later game-support generator.
- Documented: R006/ADR-009 says JIP LN Script Runner scripts live in
  `Data\nvse\plugins\scripts`, use lifecycle filename prefixes, run in the
  console environment, cannot exceed 16,384 bytes, and do not resolve Editor
  IDs by default.
- Documented: R006/ADR-009 says JIP script generation must be explicit about
  event prefixes, size budgeting, provider requirements, and FormID-resolution
  strategy.
- Documented: R005/ADR-008 says generated JIP tooling should depend on
  composable capabilities such as `runtime.scripting.jip_script_runner` and
  optional JohnnyGuitar-backed Editor ID support.
- Documented: ADR-009 says generated outputs are disposable, rebuildable, and
  must carry provenance under generated or distribution output trees.
- Documented: ADR-010 says generation/build behavior stays under canonical
  `forge generate` and `forge build` command targets.
- Documented: ADR-011 says public fixtures must be deterministic,
  redistributable, synthetic where needed, and must not redistribute Bethesda
  assets or third-party mod files without explicit permission.
- Inferred: The safest next implementation slice is a source contract
  skeleton with synthetic validation coverage before emitting any script text.
- Open: Exact schema name/version, lifecycle prefix enumeration, syntax
  subset, deterministic formatting, FormID-resolution fields, optional
  JohnnyGuitar extension point, and diagnostic IDs remain unresolved.

## Implemented

- Added durable generation notes under `docs/generation/`.
- Added the JIP LN text-script generator evidence checkpoint.
- Recorded the next implementation slice as Gate 203.
- Updated planning, ADR, governance, CLI, source/test README, and
  project-local prompt routing notes.

## Not implemented

- No JIP LN source contract.
- No schema or schema catalog update.
- No generator code.
- No generated script files.
- No build graph target.
- No CLI behavior or output contract change.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No public fixture using Bethesda assets or third-party mod files.
- No additional Doctor/provider-version expansion.
- No command alias.
- No new output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| JIP LN Script Runner constraints are recorded with research classification | Complete |
| Output boundary keeps generated/dist roots separate from live Data trees | Complete |
| Capability boundary separates JIP LN Script Runner from optional JohnnyGuitar support | Complete |
| Next slice is limited to source contract skeleton and synthetic validation coverage | Complete |
| Generator code, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 203 should add the JIP LN text-script source contract skeleton with
synthetic validation coverage only. It should stop before text emission,
package staging, CLI target wiring, runtime probes, GECK automation, MO2 VFS
inspection, or external tool execution.
