# Gate 14 - Asset Registry Schema Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 3, Gate 12, Gate 13, ADR-004, ADR-007, ADR-011

## Gate Definition

Gate 14 adds runtime schema validation for optional asset registry documents.

This gate creates the immutable Draft 2020-12 public schema for
`assets/0.1.0`, registers it in the built-in schema catalog, validates asset
registry documents when the manifest declares `registries.assets`, and adds
synthetic JSON and YAML fixture coverage.

This gate does not validate physical asset existence, texture suffixes, audio
encoding, NIF material references, BSA packaging rules, MO2 materialization, or
release packaging. Those checks are semantic, asset, packaging, or release
validation work after the source contract shape is stable.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Forge owns production-layer generation, validation, packaging, release automation, and workflow integration, but not Blender, NifSkope, DAWs, GECK record editing, xEdit conflict resolution, or raw plugin editing. | ADR-004 |
| Documented | Source truth is versioned YAML/JSON registry documents normalized to canonical JSON and validated by JSON Schema Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | The asset registry is strongly recommended in v0.1 after manifest, dependency, and capability registries. | R004 / ADR-007 |
| Documented | Validation runs in layered stages from load/source validation through schema and semantic validation. | R008 / ADR-011 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Gate 14 validates asset registry contract shape only; file existence, suffix, and packaging checks are later `WF-ASSET-*` diagnostics. | ADR-004 and ADR-011 validation layering |
| Inferred | Asset entries use separate `source` and `target` paths so later generation and packaging can reason about authored project paths separately from game-relative output paths. | ADR-004 asset workflow and ADR-009 provenance boundaries |
| Open | Exact asset semantic rules for mesh, texture, audio, voice, lip, animation, interface, and config assets remain later validator work. | Later asset validation gate |
| Open | Packaging-specific BSA and loose-file rules remain later package/release validation work. | Later package/release gate |

## Deliverables

- `schemas/assets/0.1.0/schema.json`
- `WastelandForgeSchemaIds.Asset010`
- built-in schema catalog entry for asset schema
- optional runtime asset registry schema validation
- synthetic asset registry documents in `ExampleMod` and `YamlExample`
- invalid asset registry broken-case fixture
- schema, semantic, and back-compat tests for asset schema validation

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest against manifest/0.1.0
  - validate dependency registries against dependencies/0.1.0
  - validate capability registries against capabilities/0.1.0
  - validate asset registries against assets/0.1.0 when declared

semantic validation
  - run only after blocking schema diagnostics are absent
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate14
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidAssetRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed.
Focused semantic tests passed.
Focused back-compat tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate14.
ExampleMod returned exit 0 with asset registry validation enabled.
YamlExample returned exit 0 with YAML asset registry validation enabled.
InvalidAssetRegistry returned exit 1 with WF-SCHEMA-001.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add asset path existence and path convention validation. | Open | Gate 15 |
| Add asset type semantic rules for mesh, texture, audio, voice, lip, animation, interface, and config assets. | Open | Gate 15 or later |
| Add package-specific BSA and loose-file validation. | Open | Package/release gate |
| Add provider catalogue schema validation. | Open | Later capability/provider gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |

## Next Gate

Gate 15 should add asset path semantic validation.
