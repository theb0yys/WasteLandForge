# Gate 203 - JIP LN Text-Script Source Contract Skeleton

Status: Complete

## Purpose

Add the first JIP LN text-script source contract skeleton with synthetic
validation coverage only. This gate stops before text emission, package
staging, CLI target wiring, runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, or external tool execution.

## Research grounding

- Documented: R006/ADR-009 says JIP LN Script Runner scripts live in
  `Data\nvse\plugins\scripts`, use lifecycle filename prefixes, run in the
  console environment, cannot exceed 16,384 bytes, and do not resolve Editor
  IDs by default.
- Documented: R006/ADR-009 says JIP script generation must be explicit about
  event prefixes, size budgeting, provider requirements, and
  FormID-resolution strategy.
- Documented: R005/ADR-008 says generated JIP tooling should depend on
  composable capabilities such as `runtime.scripting.jip_script_runner`.
- Documented: R005/ADR-008 says JohnnyGuitar-backed Editor ID call-through is
  a separate optional capability from the base JIP Script Runner path.
- Documented: ADR-007 says canonical source truth is versioned YAML/JSON
  contracts validated by JSON Schema plus deterministic semantic validators.
- Documented: ADR-011 says public fixtures must be deterministic,
  redistributable, synthetic where needed, and must not redistribute Bethesda
  assets or third-party mod files without explicit permission.
- Inferred: The first safe implementation step is a source registry schema
  that records script identity, lifecycle prefix, output filename intent,
  required capabilities, size policy, and explicit-reference FormID strategy
  without defining script body syntax or emitting text files.
- Open: Safe emitted script syntax, deterministic formatting, JohnnyGuitar
  extension fields, semantic rule IDs, and generator output behavior remain
  future gates.

## Implemented

- Added `schemas/jip-scripts/0.1.0/schema.json`.
- Added optional manifest `registries.jipScripts` support.
- Added schema catalog ID/resource wiring for the `jip-script` registry kind.
- Added validation pipeline loading for manifest-declared JIP script
  registries.
- Added `ProjectJipScriptReadResult` and `JipScriptDefinition` read models
  for schema-valid source contracts.
- Added synthetic valid and broken JIP script fixture projects.
- Added schema, back-compat, and semantic fixture tests.
- Updated planning, generation notes, ADRs, governance docs, README files,
  and project-local prompt routing.

## Not implemented

- No script body syntax contract.
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
- No new semantic or generator diagnostic rule ID.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| JIP script source schema is discoverable through the built-in schema catalog | Complete |
| Manifest schema can declare optional `registries.jipScripts` root | Complete |
| Validation pipeline schema-validates declared JIP script registries | Complete |
| Typed read model extracts schema-valid JIP source intent | Complete |
| Valid and broken fixtures are synthetic and redistributable | Complete |
| Text emission, package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SchemaTests\WastelandForge.SchemaTests.csproj --no-build --no-restore` passed.
- `dotnet test tests\WastelandForge.BackCompatTests\WastelandForge.BackCompatTests.csproj --no-build --no-restore` passed.
- `dotnet test tests\WastelandForge.SemanticTests\WastelandForge.SemanticTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScript"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.
- `git diff --check` passed. Git reported line-ending warnings for existing
  text files, but returned success.

## Next gate

Gate 204 should add JIP LN text-script source semantic validation for
schema-valid contracts. Start with lifecycle/output prefix consistency and
required capability declaration. Stop before text emission, package staging,
CLI target wiring, runtime probes, GECK automation, MO2 VFS inspection, live
Data mutation, or external tool execution.
