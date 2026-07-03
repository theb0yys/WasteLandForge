# Gate 204 - JIP LN Text-Script Source Semantic Validation

Status: Complete

## Purpose

Add semantic validation for schema-valid JIP LN text-script source contracts.
This gate checks lifecycle/output prefix consistency and required JIP Script
Runner capability declaration only. It stops before script body/source-line
contract design, text emission, package staging, CLI target wiring, runtime
probes, GECK automation, MO2 VFS inspection, live Data mutation, or external
tool execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN Script Runner scripts are selected by
  lifecycle filename prefixes, must be explicit about event prefixes, and
  should stay bounded before generation.
- Documented: R006/ADR-009 says JIP text scripts depend on explicit provider
  requirements and FormID-resolution strategy.
- Documented: R005/ADR-008 says generated JIP tooling should depend on
  composable capabilities such as `runtime.scripting.jip_script_runner`.
- Documented: ADR-007 says source contracts are validated by JSON Schema plus
  deterministic semantic validators.
- Documented: ADR-011 says public fixtures must be deterministic,
  redistributable, synthetic where needed, and must not redistribute Bethesda
  assets or third-party mod files without explicit permission.
- Inferred: The first semantic checks should validate relationships that JSON
  Schema cannot express locally: lifecycle prefix must match output filename
  prefix, and every JIP script source contract must explicitly require the
  base JIP Script Runner capability.
- Open: Script body syntax, deterministic formatting, generated output paths,
  script size accounting over emitted bytes, unresolved reference diagnostics,
  capability readiness diagnostics, and JohnnyGuitar extension fields remain
  future gates.

## Implemented

- Added `WF-SEM-040` for JIP source lifecycle prefix and output filename
  prefix mismatch.
- Added `WF-SEM-041` for JIP source contracts that do not declare
  `runtime.scripting.jip_script_runner`.
- Threaded manifest-declared JIP script registry documents into the semantic
  validation pass.
- Added synthetic fixtures for both semantic failure cases.
- Added semantic fixture tests for both diagnostics.
- Updated planning, generation notes, ADRs, governance docs, README files,
  and project-local prompt routing.

## Not implemented

- No script body/source-line contract.
- No generated JIP script files.
- No generator implementation.
- No build graph target.
- No `forge generate` or `forge build` target wiring.
- No package staging.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No generated output schema.
- No capability readiness/runtime diagnostic.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Schema-valid lifecycle/output prefix mismatch emits `WF-SEM-040` | Complete |
| Schema-valid missing JIP Script Runner requirement emits `WF-SEM-041` | Complete |
| Existing valid JIP source fixture remains valid | Complete |
| Broken fixtures are synthetic and redistributable JSON registry documents | Complete |
| Text emission, package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SemanticTests\WastelandForge.SemanticTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScript"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 205 should add the first JIP LN text-script body/source-line contract
skeleton. It should stop before text emission, package staging, CLI target
wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data
mutation, or external tool execution.
