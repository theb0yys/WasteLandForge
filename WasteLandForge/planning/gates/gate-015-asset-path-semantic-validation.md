# Gate 15 - Asset Path Semantic Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 14, ADR-004, ADR-007, ADR-011

## Gate Definition

Gate 15 adds the first semantic asset path validation layer.

This gate validates schema-valid asset registry entries after load/source and
schema validation succeed. It emits deterministic `WF-ASSET-*` diagnostics for
source paths that escape the project root, required source files that are
missing, target paths that are not game-relative, and target extensions that do
not match the declared asset type.

This gate does not inspect NIF internals, validate DDS texture content, inspect
WAV/OGG encoding, validate LIP generation prerequisites, enforce BSA packaging
rules, materialize MO2 output mods, or run external tools.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Forge owns validation and production-layer automation, but not creative tools, raw plugin editing, or replacement of established FNV tools. | ADR-004 |
| Documented | FNV asset production is path-sensitive and record-driven. | ADR-004 |
| Documented | WastelandForge source truth is YAML/JSON contracts normalized to canonical JSON, validated by JSON Schema plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Validation runs in layered stages from load/source validation through schema and semantic validation. | R008 / ADR-011 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Asset source paths must stay inside the project root because canonical source belongs in the repository and generated/distribution outputs are separate trees. | ADR-007 and ADR-009 boundaries |
| Inferred | Asset target paths are game-data-relative paths; traversal outside that logical root is invalid before packaging is considered. | ADR-004 asset workflow and Gate 14 schema model |
| Inferred | Basic target extension checks are safe in Gate 15 because they use contract data only and do not inspect proprietary or game-derived files. | ADR-004 asset path rules and R008 fixture policy |
| Open | Type-specific content validation for NIF, DDS, WAV, OGG, LIP, KF, and BSA rules remains later work. | Later asset/package gates |

## Deliverables

- `WF-ASSET-001` for asset source paths that escape the project root.
- `WF-ASSET-002` for required asset source files that are missing.
- `WF-ASSET-003` for target paths that are not game-relative.
- `WF-ASSET-004` for target extensions that do not match asset type.
- Tiny synthetic asset files for valid JSON and YAML fixtures.
- `InvalidAssetPaths` broken fixture with schema-valid asset path failures.
- Semantic fixture tests for all Gate 15 asset rules.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, and declared asset registries

semantic validation
  - validate asset source paths stay inside the project root
  - validate required asset source files exist
  - validate target paths are game-data-relative and traversal-free
  - validate target extensions match declared asset types
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate15
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidAssetPaths --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused semantic tests passed.
Focused schema tests passed.
Focused back-compat tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate15.
ExampleMod returned exit 0 with required synthetic asset source present.
YamlExample returned exit 0 with required synthetic asset source present.
InvalidAssetPaths returned exit 1 with WF-ASSET-001 through WF-ASSET-004.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add deeper type-specific validation for NIF, DDS, WAV, OGG, LIP, KF, and RDT assets. | Open | Gate 16 |
| Add voice/dialogue asset pairing and audio encoding validation. | Open | Later asset/voice gate |
| Add package-specific BSA and loose-file validation. | Open | Package/release gate |
| Add provider catalogue schema validation. | Open | Later capability/provider gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |

## Next Gate

Gate 16 should add asset type-specific validation.
