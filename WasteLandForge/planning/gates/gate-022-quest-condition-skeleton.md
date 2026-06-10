# Gate 22 - Quest Condition Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 21, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 22 adds the first quest condition skeleton.

This gate preserves quest registry schemas `0.1.0`, `0.2.0`, and `0.3.0` and
adds immutable Draft 2020-12 quest registry schema `0.4.0`. The new schema
supports minimal quest-local `conditions` arrays on quest entries. Gate 22 only
models `stageDone` conditions with `stageId` references to stages declared in
the same quest.

This gate also adds `WF-SEM-019` for quest conditions that reference undeclared
stage IDs.

This gate does not model the full GECK condition language, condition
evaluation, result scripts, quest variables, lockouts, fallback routes, soft
points of no return, branch semantics, dialogue condition compilation, or
plugin record generation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Conditions can govern quests, stages, objectives, and dialogue in New Vegas. | FNV narrative systems research |
| Documented | `GetStage` and `GetStageDone` make quest stage state a documented condition surface. | FNV narrative systems research |
| Documented | Quest-level conditions are checked before info-level dialogue conditions. | FNV narrative systems research |
| Documented | The Dialogue Registry eventually needs a condition language over quest stages and variables, among other domains. | FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Semantic validators handle reference resolution after schema validation. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal `stageDone` condition skeleton is safe before the full condition language is verified because `GetStageDone` is directly documented. | ADR-003 quest state model plus FNV narrative systems research |
| Inferred | Condition stage reference validation is a deterministic semantic rule and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Exact condition function coverage, boolean composition, parameter typing, quest variable handling, result-script interaction, and dialogue condition compilation still require representative `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/quests/0.4.0/schema.json`.
- `WastelandForgeSchemaIds.Quest040`.
- Built-in schema catalog entry for quest schema `0.4.0`.
- Runtime quest schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, and `0.4.0`.
- Quest semantic validation for condition stage references.
- `WF-SEM-019` for unresolved condition stage references.
- Valid `ExampleMod` quest registry updated to schema `0.4.0`.
- `InvalidQuestConditionRegistry` schema broken fixture.
- `MissingQuestConditionStageReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 22.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve quest registry schemas 0.1.0, 0.2.0, and 0.3.0
  - validate quest registry schema 0.4.0 condition shape

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
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=gate22-schema.trx" --results-directory TestResults/Gate22
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=gate22-backcompat.trx" --results-directory TestResults/Gate22
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=gate22-semantic.trx" --results-directory TestResults/Gate22
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal" --logger "trx;LogFileName=gate22-full.trx" --results-directory TestResults/Gate22
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestConditionRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestConditionStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestTransitionStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 27 passed.
Focused back-compat tests passed: 11 passed.
Focused semantic tests passed: 21 passed.
Full suite passed: 84 passed.
TRX files emitted under TestResults/Gate22.
ExampleMod returned exit 0 with quest schema 0.4.0 enabled.
YamlExample returned exit 0 without a quest registry.
InvalidQuestConditionRegistry returned exit 1 with WF-SCHEMA-001.
MissingQuestConditionStageReference returned exit 1 with WF-SEM-019.
MissingQuestTransitionStageReference still returned exit 1 with WF-SEM-018.
MissingDialogueQuestReference still returned exit 1 with WF-SEM-016.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add quest result-script skeleton. | Open | Gate 23 |
| Add boolean condition composition and richer GECK condition functions. | Open | Later narrative gate |
| Add quest variables, lockouts, fallback routes, and soft points of no return. | Open | Later narrative gate |
| Add dialogue conditions over quest stages and variables. | Open | Later dialogue gate |
| Validate external quest/stage/condition references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 23 should add the quest result-script skeleton.
