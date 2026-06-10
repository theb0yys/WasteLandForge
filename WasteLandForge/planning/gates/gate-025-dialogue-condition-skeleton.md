# Gate 25 - Dialogue Condition Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 24, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 25 adds the first dialogue condition skeleton.

This gate preserves dialogue registry schema `0.1.0` and adds immutable Draft
2020-12 dialogue registry schema `0.2.0`. The new schema supports minimal
line-local `conditions` arrays on dialogue lines. Gate 25 only models
`questStageDone` and `questVariableEquals` conditions that reference quest
state through the dialogue line's existing `questId`.

This gate also adds `WF-SEM-022` for dialogue conditions that reference
undeclared stages inside the line's referenced quest, and `WF-SEM-023` for
dialogue conditions that reference undeclared variables inside the line's
referenced quest.

This gate does not model full GECK dialogue condition language, boolean
composition, quest-level dialogue gates, cross-quest dialogue conditions,
condition evaluation, result scripts, topic linking, dialogue graph traversal,
plugin record generation, or generated dialogue output.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue conditions are condition lists made of script functions and use the same general condition system as quests, stages, objectives, and packages. | FNV narrative systems research |
| Documented | Quest-level conditions are evaluated before info-level dialogue conditions. | FNV narrative systems research |
| Documented | Dialogue registry work needs condition language support over quest stages and quest variables. | FNV narrative systems research |
| Documented | GECK advanced conversation guidance uses quest variables with `GetQuestVariable ... == 0/1/2` to drive variable-driven conversations. | FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Semantic validators handle reference resolution after schema validation. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Line-local quest-stage and quest-variable condition skeletons are a safe first dialogue slice because existing dialogue lines already declare `questId` and the research documents quest-stage and quest-variable condition surfaces. | ADR-003 quest state model plus FNV narrative systems research |
| Inferred | Dialogue condition quest-state reference validation is deterministic semantic validation and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Full condition function coverage, boolean composition, quest-level dialogue gates, cross-quest condition targeting, topic linking, result-script interaction, generated plugin records, and representative shipped-record patterns still require direct `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.2.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue020`.
- Built-in schema catalog entry for dialogue schema `0.2.0`.
- Runtime dialogue schema dispatch for `0.1.0` and `0.2.0`.
- Dialogue semantic validation for line-local quest-stage condition references.
- Dialogue semantic validation for line-local quest-variable condition references.
- `WF-SEM-022` for unresolved dialogue condition stage references.
- `WF-SEM-023` for unresolved dialogue condition variable references.
- Valid `ExampleMod` dialogue registry updated to schema `0.2.0`.
- `InvalidDialogueConditionRegistry` schema broken fixture.
- `MissingDialogueConditionStageReference` semantic broken fixture.
- `MissingDialogueConditionVariableReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 25.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schema 0.1.0
  - validate dialogue registry schema 0.2.0 line-local condition shape

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate25/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate25/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate25/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueConditionRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueConditionStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueConditionVariableReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 36 passed.
Focused back-compat tests passed: 14 passed.
Focused semantic tests passed: 28 passed.
Full suite passed: 103 passed.
TRX files emitted under TestResults/Gate25/Schema, TestResults/Gate25/BackCompat, and TestResults/Gate25/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.2.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueConditionRegistry returned exit 1 with WF-SCHEMA-001.
MissingDialogueConditionStageReference returned exit 1 with WF-SEM-022.
MissingDialogueConditionVariableReference returned exit 1 with WF-SEM-023.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue result-script skeletons. | Open | Gate 26 |
| Add quest-level dialogue condition gates. | Open | Later dialogue gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add topic linking and dialogue graph validation. | Open | Later dialogue gate |
| Add condition compilation into plugin records. | Open | Later generator/tooling gate |
| Validate external dialogue/topic/condition references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 26 should add the dialogue result-script skeleton.
