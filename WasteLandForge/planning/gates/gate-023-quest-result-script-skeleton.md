# Gate 23 - Quest Result-Script Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 22, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 23 adds the first quest result-script skeleton.

This gate preserves quest registry schemas `0.1.0`, `0.2.0`, `0.3.0`, and
`0.4.0` and adds immutable Draft 2020-12 quest registry schema `0.5.0`. The
new schema supports minimal stage-local `resultScripts` arrays. Gate 23 only
models `stageResult` declarations with stable IDs and optional `conditionId`
references to conditions declared in the same quest.

This gate also adds `WF-SEM-020` for quest result scripts that reference
undeclared quest-local condition IDs.

This gate does not model raw script bodies, script commands, side-effect
language, result-script execution, generator output, quest variables, lockouts,
fallback routes, soft points of no return, branch semantics, dialogue result
scripts, or plugin record generation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | `SetStage` marks a stage as completed and immediately runs any attached stage results that pass their conditions. | FNV narrative systems research |
| Documented | Quest data exposes "View All Results Scripts", which confirms result scripts are a first-class quest authoring surface. | FNV narrative systems research |
| Documented | Dialogue infos also have result scripts, but Gate 23 is scoped to quest stage result scripts only. | FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Semantic validators handle reference resolution after schema validation. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal stage result-script declaration with optional `conditionId` is safe before raw script payload modeling because the research documents attached stage results and condition-gated execution. | ADR-003 quest state model plus FNV narrative systems research |
| Inferred | Result-script condition reference validation is a deterministic semantic rule and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Exact raw script body shape, command set, side-effect model, generator output, execution semantics, dialogue result-script modeling, and representative shipped-record patterns still require direct `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/quests/0.5.0/schema.json`.
- `WastelandForgeSchemaIds.Quest050`.
- Built-in schema catalog entry for quest schema `0.5.0`.
- Runtime quest schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`, and
  `0.5.0`.
- Quest semantic validation for result-script condition references.
- `WF-SEM-020` for unresolved result-script condition references.
- Valid `ExampleMod` quest registry updated to schema `0.5.0`.
- `InvalidQuestResultScriptRegistry` schema broken fixture.
- `MissingQuestResultScriptConditionReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 23.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve quest registry schemas 0.1.0, 0.2.0, 0.3.0, and 0.4.0
  - validate quest registry schema 0.5.0 result-script shape

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
  - validate quest objective stage references resolve inside the same quest
  - validate quest transition stage references resolve inside the same quest
  - validate quest condition stage references resolve inside the same quest
  - validate quest result-script condition references resolve inside the same quest
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate23/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate23/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate23/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestResultScriptRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestResultScriptConditionReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 30 passed.
Focused back-compat tests passed: 12 passed.
Focused semantic tests passed: 23 passed.
Full suite passed: 90 passed.
TRX files emitted under TestResults/Gate23/Schema, TestResults/Gate23/BackCompat, and TestResults/Gate23/Semantic.
ExampleMod returned exit 0 with quest schema 0.5.0 enabled.
YamlExample returned exit 0 without a quest registry.
InvalidQuestResultScriptRegistry returned exit 1 with WF-SCHEMA-001.
MissingQuestResultScriptConditionReference returned exit 1 with WF-SEM-020.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add quest variable skeleton. | Open | Gate 24 |
| Add raw result-script payload modeling. | Open | Later narrative gate |
| Add result-script execution and side-effect semantics. | Open | Later generator/validation gate |
| Add dialogue result-script skeletons. | Open | Later dialogue gate |
| Add lockouts, fallback routes, and soft points of no return. | Open | Later narrative gate |
| Validate external quest/stage/condition/script references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 24 should add the quest variable skeleton.
