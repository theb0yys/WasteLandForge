# Gate 26 - Dialogue Result-Script Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 25, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 26 adds the first dialogue result-script skeleton.

This gate preserves dialogue registry schemas `0.1.0` and `0.2.0` and adds
immutable Draft 2020-12 dialogue registry schema `0.3.0`. The new schema
supports minimal line-local `resultScripts` arrays on dialogue lines. Gate 26
only models `dialogueResult` declarations with stable IDs and optional
summaries.

This gate does not add a new semantic rule because Gate 26 result-script fields
do not reference other authored state. Invalid result-script shape is reported
through runtime schema validation as `WF-SCHEMA-001`.

This gate does not model raw script bodies, variable mutation, result-script
execution, result-script condition gates, side-effect language, topic linking,
dialogue graph traversal, plugin record generation, or generated dialogue
output.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue infos have result scripts, and dialogue is a gameplay state transition surface rather than only text storage. | FNV narrative systems research |
| Documented | Variable-driven conversations can increment quest variables in result fields. | FNV narrative systems research |
| Documented | The Dialogue Registry should support result scripts that mutate state. | FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal dialogue result-script declaration is safe before raw script payload modeling because the research documents the surface but leaves exact shipped patterns unresolved. | ADR-003 dialogue state model plus FNV narrative systems research |
| Open | Raw script body shape, mutation semantics, result-script condition gates, variable write modeling, generated plugin records, and representative shipped-record patterns still require direct `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.3.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue030`.
- Built-in schema catalog entry for dialogue schema `0.3.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, and `0.3.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.3.0`.
- `InvalidDialogueResultScriptRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 26.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 and 0.2.0
  - validate dialogue registry schema 0.3.0 line-local result-script shape

semantic validation
  - validate asset source paths stay inside the project root
  - validate required asset source files exist
  - validate target paths are game-data-relative and traversal-free
  - validate target extensions match declared asset types
  - validate source file signatures for known target file types
  - validate target roots match declared asset types
  - validate voice/lip target shape under sound/voice
  - validate required voice WAV/OGG pairs by target stem
  - validate required voice LIP pairs by target stem
  - validate dialogue voice worklist entries have declared WAV/OGG/LIP assets
  - validate dialogue quest references resolve to declared quest IDs
  - validate dialogue condition stage references resolve inside the line quest
  - validate dialogue condition variable references resolve inside the line quest
  - validate quest objective stage references resolve inside the same quest
  - validate quest transition stage references resolve inside the same quest
  - validate quest condition stage references resolve inside the same quest
  - validate quest result-script condition references resolve inside the same quest
  - validate quest condition variable references resolve inside the same quest
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate26/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate26/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate26/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueResultScriptRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueConditionRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 39 passed.
Focused back-compat tests passed: 15 passed.
Focused semantic tests passed: 29 passed.
Full suite passed: 108 passed.
TRX files emitted under TestResults/Gate26/Schema, TestResults/Gate26/BackCompat, and TestResults/Gate26/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.3.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueResultScriptRegistry returned exit 1 with WF-SCHEMA-001.
InvalidDialogueConditionRegistry still returned exit 1 with WF-SCHEMA-001.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue topic/link skeletons. | Open | Gate 27 |
| Add dialogue result-script variable mutation modeling. | Open | Later dialogue/generator gate |
| Add raw result-script payload modeling. | Open | Later narrative/generator gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add result-script compilation into plugin records. | Open | Later generator/tooling gate |
| Validate external dialogue/topic/result-script references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 27 should add the dialogue topic link skeleton.
