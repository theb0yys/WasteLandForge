# Gate 212 - JIP LN Text-Script Emission Manifest Schema Validation

Status: Complete

## Purpose

Add an embedded generated-evidence schema for JIP LN text-script emission
manifests and validate `jip-script-emission-manifest.json` before the checksum
sidecar is written. This gate keeps the output internal to
`generated/jip-scripts` and stops before package staging, CLI target wiring,
runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, or
external tool execution.

## Research grounding

- Documented: ADR-007 says canonical contracts are versioned JSON/YAML
  documents normalized to canonical JSON and validated with JSON Schema Draft
  2020-12 plus deterministic semantic validators.
- Documented: ADR-009 says generated outputs are disposable, rebuildable, and
  must carry provenance.
- Documented: ADR-011 reserves `WF-GEN-*` diagnostics for generator
  input/output/provenance gaps and requires deterministic fixture-backed
  tests.
- Inferred: The generated JIP emission manifest is generated evidence, not
  canonical source truth, so it belongs in the schema catalog as a generated
  evidence contract rather than a source registry contract.
- Inferred: Manifest schema validation should happen before checksum sidecar
  emission so Forge does not finalize checksum evidence around a malformed
  manifest.
- Open: Checksum sidecar parsing and recomputation against emitted files
  remains future work.

## Implemented

- Added `schemas/jip-script-emission-manifest/0.1.0/schema.json`.
- Added the JIP emission manifest schema to `WastelandForgeSchemaIds` and
  `WastelandForgeSchemaCatalog`.
- Added `JipScriptFileEmitter.ValidateManifestJson` using the embedded schema.
- Added `WF-GEN-007` for generated JIP emission manifest schema failures.
- Updated `JipScriptFileEmitter.Emit` to validate the manifest JSON before
  writing the manifest and checksum sidecar.
- Added schema catalog tests for the new built-in schema.
- Added unit coverage for valid emitted manifests and malformed manifest
  diagnostics.
- Updated planning, schema, generation, governance, ADR, fixture, test, and
  project-local routing documentation.

## Not implemented

- No checksum sidecar parser or recomputation verifier.
- No build graph target.
- No `forge generate` or `forge build` target wiring.
- No package staging.
- No runtime probe.
- No GECK automation.
- No MO2 VFS inspection.
- No live game Data mutation.
- No binary plugin generation.
- No external tool execution.
- No public fixture using Bethesda assets or third-party mod files.
- No command alias.
- No new CLI output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| JIP emission manifest schema exists with immutable `0.1.0` schema ID | Complete |
| Built-in schema catalog resolves and reads the manifest schema | Complete |
| JIP file emission validates generated manifest JSON before writing checksum evidence | Complete |
| Malformed manifest JSON produces `WF-GEN-007` | Complete |
| Valid generated manifest JSON produces no `WF-GEN-007` diagnostics | Complete |
| Package staging, CLI wiring, runtime probes, GECK automation, MO2 VFS inspection, live Data mutation, and external tools remain out of scope | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SchemaTests\WastelandForge.SchemaTests.csproj --no-build --no-restore --filter "FullyQualifiedName~ManifestSchemaTests"` passed with 129 tests.
- `dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~JipScriptGenerationPlanner"` passed with 7 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed with
  549 tests.
- `git diff --check` passed. Git reported line-ending warnings for working
  tree text files, but returned success.
- `rg -n "[ \t]$" . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!generated/**' --glob '!dist/**'` found no trailing whitespace outside build output trees.
- Protected path scan found no matching protected filenames. Protected content
  scan found only older gate audit lines that mention the protected-scan
  phrase, not protected project content.

## Next gate

Gate 213 should add a JIP emission checksum sidecar revalidation skeleton. It
should parse `generated/jip-scripts/checksums.sha256`, verify the manifest and
generated script entries against local files, and stop before package staging,
CLI target wiring, runtime probes, GECK automation, MO2 VFS inspection, live
Data mutation, or external tool execution.
