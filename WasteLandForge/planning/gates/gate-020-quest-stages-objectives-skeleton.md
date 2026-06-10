# Gate 20 - Quest Stages and Objectives Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 19, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 20 adds the first quest stage and objective skeleton.

This gate preserves quest registry schema `0.1.0` and adds immutable Draft
2020-12 quest registry schema `0.2.0`. The new schema supports minimal
`stages` and `objectives` arrays on quest entries, plus `startStageId` and
`completionStageId` references from objectives to stages declared in the same
quest. Runtime validation selects the quest schema by the registry document's
`schemaVersion`.

This gate also adds `WF-SEM-017` for quest objectives that reference undeclared
stage IDs.

This gate does not model stage transitions, quest conditions, result scripts,
quest variables, lockouts, fallback routes, soft points of no return, branch
semantics, dialogue condition compilation, or plugin record generation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | New Vegas quests are editor containers whose principal parts are objectives and dialogue. | FNV narrative systems research |
| Documented | `SetStage`, `GetStage`, and `GetStageDone` make quest stages a persistent authored state surface. | FNV narrative systems research |
| Documented | Quest State should own canonical progression and objectives in the hybrid state architecture. | ADR-003 / FNV narrative systems research |
| Documented | The Quest Registry should be stage-aware and should support stage transitions, lockouts, fallback paths, and downstream claims over time. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON registry documents normalized to canonical JSON and validated with JSON Schema plus semantic validators. | R004 / ADR-007 |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Semantic validators handle reference resolution after schema shape validation. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Stage and objective identity plus objective-to-stage references are safe to model before exact GECK result-script and condition shapes are verified. | ADR-003 quest state model plus R004 staged schema rollout |
| Inferred | Objective stage reference validation is a deterministic semantic rule and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Exact stage transition, condition, result-script, variable, lockout, fallback-route, and branch schema shape still requires representative `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/quests/0.2.0/schema.json`.
- `WastelandForgeSchemaIds.Quest020`.
- Built-in schema catalog entry for quest schema `0.2.0`.
- Runtime quest schema dispatch for `0.1.0` and `0.2.0`.
- Quest semantic validation for objective stage references.
- `WF-SEM-017` for unresolved objective stage references.
- Valid `ExampleMod` quest registry updated to schema `0.2.0`.
- `InvalidQuestStageObjectiveRegistry` schema broken fixture.
- `MissingQuestObjectiveStageReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 20.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve quest registry schema 0.1.0
  - validate quest registry schema 0.2.0 stage and objective shape

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
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=gate20-schema.trx" --results-directory TestResults/Gate20
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=gate20-backcompat.trx" --results-directory TestResults/Gate20
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=gate20-semantic.trx" --results-directory TestResults/Gate20
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal" --logger "trx;LogFileName=gate20-full.trx" --results-directory TestResults/Gate20
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestStageObjectiveRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestObjectiveStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 21 passed.
Focused back-compat tests passed: 9 passed.
Focused semantic tests passed: 17 passed.
Full suite passed: 72 passed.
TRX files emitted under TestResults/Gate20.
ExampleMod returned exit 0 with quest schema 0.2.0 enabled.
YamlExample returned exit 0 without a quest registry.
InvalidQuestRegistry returned exit 1 with WF-SCHEMA-001 against quest schema 0.1.0.
InvalidQuestStageObjectiveRegistry returned exit 1 with WF-SCHEMA-001 against quest schema 0.2.0.
MissingQuestObjectiveStageReference returned exit 1 with WF-SEM-017.
MissingDialogueQuestReference still returned exit 1 with WF-SEM-016.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add quest stage transition skeleton. | Open | Gate 21 |
| Add quest condition and result-script modeling. | Open | Later narrative gate |
| Add quest variables, lockouts, fallback routes, and soft points of no return. | Open | Later narrative gate |
| Add dialogue conditions over quest stages. | Open | Later dialogue gate |
| Validate external quest/stage references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 21 should add the quest stage transition skeleton.
