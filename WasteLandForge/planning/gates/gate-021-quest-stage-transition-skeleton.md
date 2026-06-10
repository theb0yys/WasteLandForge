# Gate 21 - Quest Stage Transition Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 20, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 21 adds the first quest stage transition skeleton.

This gate preserves quest registry schemas `0.1.0` and `0.2.0` and adds
immutable Draft 2020-12 quest registry schema `0.3.0`. The new schema supports
minimal `transitions` arrays on quest entries, with optional `fromStageId` and
required `toStageId` references to stages declared in the same quest.

This gate also adds `WF-SEM-018` for quest transitions that reference
undeclared stage IDs.

This gate does not model transition conditions, result scripts, quest
variables, lockouts, fallback routes, soft points of no return, branch
semantics, dialogue condition compilation, or plugin record generation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | `SetStage` marks a quest stage as completed and immediately runs attached stage results that pass their conditions. | FNV narrative systems research |
| Documented | `GetStage` and `GetStageDone` make quest stages a persistent state surface used by gameplay and dialogue. | FNV narrative systems research |
| Documented | The Quest Registry should model quests as stateful hubs with stage transitions, not linear checklists. | FNV narrative systems research |
| Documented | Quest State should own canonical progression, objectives, lockouts, and local branching. | ADR-003 / FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Semantic validators handle reference resolution after schema validation. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal transition object with declared stage references is safe before exact transition conditions and result-script payloads are verified. | ADR-003 quest state model plus R004 staged schema rollout |
| Inferred | Transition stage reference validation is a deterministic semantic rule and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Exact transition triggers, condition language, result-script shape, lockout semantics, fallback route modeling, and branch semantics still require representative `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/quests/0.3.0/schema.json`.
- `WastelandForgeSchemaIds.Quest030`.
- Built-in schema catalog entry for quest schema `0.3.0`.
- Runtime quest schema dispatch for `0.1.0`, `0.2.0`, and `0.3.0`.
- Quest semantic validation for transition stage references.
- `WF-SEM-018` for unresolved transition stage references.
- Valid `ExampleMod` quest registry updated to schema `0.3.0`.
- `InvalidQuestTransitionRegistry` schema broken fixture.
- `MissingQuestTransitionStageReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 21.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve quest registry schemas 0.1.0 and 0.2.0
  - validate quest registry schema 0.3.0 transition shape

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
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=gate21-schema.trx" --results-directory TestResults/Gate21
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=gate21-backcompat.trx" --results-directory TestResults/Gate21
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=gate21-semantic.trx" --results-directory TestResults/Gate21
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal" --logger "trx;LogFileName=gate21-full.trx" --results-directory TestResults/Gate21
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestTransitionRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestTransitionStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestObjectiveStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 24 passed.
Focused back-compat tests passed: 10 passed.
Focused semantic tests passed: 19 passed.
Full suite passed: 78 passed.
TRX files emitted under TestResults/Gate21.
ExampleMod returned exit 0 with quest schema 0.3.0 enabled.
YamlExample returned exit 0 without a quest registry.
InvalidQuestTransitionRegistry returned exit 1 with WF-SCHEMA-001.
MissingQuestTransitionStageReference returned exit 1 with WF-SEM-018.
MissingQuestObjectiveStageReference still returned exit 1 with WF-SEM-017.
MissingDialogueQuestReference still returned exit 1 with WF-SEM-016.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add quest condition skeleton. | Open | Gate 22 |
| Add result-script modeling. | Open | Later narrative gate |
| Add quest variables, lockouts, fallback routes, and soft points of no return. | Open | Later narrative gate |
| Add dialogue conditions over quest stages and variables. | Open | Later dialogue gate |
| Validate external quest/stage references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 22 should add the quest condition skeleton.
