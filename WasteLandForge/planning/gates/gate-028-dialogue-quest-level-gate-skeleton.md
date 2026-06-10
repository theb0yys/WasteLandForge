# Gate 28 - Dialogue Quest-Level Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 27, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 28 adds the first dialogue quest-level gate skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`, and
`0.4.0` and adds immutable Draft 2020-12 dialogue registry schema `0.5.0`.
The new schema supports optional top-level `questGates` declarations keyed by
`questId`. Gate 28 only models quest-stage and quest-variable gate conditions
against the gate quest.

This gate adds `WF-SEM-026` for dialogue quest gates whose `questId` does not
resolve to a declared quest, `WF-SEM-027` for quest-level dialogue gate
condition stage references that do not resolve inside the gate quest, and
`WF-SEM-028` for quest-level dialogue gate condition variable references that
do not resolve inside the gate quest.

This gate does not model full GECK condition language, boolean composition,
gate execution ordering, condition evaluation, generated plugin records,
dialogue condition compilation, or result-script variable mutation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Quest-level conditions are evaluated before info-level dialogue conditions. | FNV narrative systems research |
| Documented | Quest-level conditions act as a gate on all dialogue in that quest before info-level conditions are evaluated. | FNV narrative systems research |
| Documented | Dialogue is a state query and state transition surface, not only text storage. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A minimal `questGates` array keyed by `questId` is the smallest safe source-contract skeleton for the researched quest-level dialogue gate pattern. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | Gate quest, stage, and variable reference checks belong in `WF-SEM-*` because they validate authored cross-object meaning after schema shape succeeds. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Full GECK condition functions, boolean composition, evaluation order details, plugin record output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.5.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue050`.
- Built-in schema catalog entry for dialogue schema `0.5.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  and `0.5.0`.
- Dialogue semantic validation for quest-level gate quest references.
- Dialogue semantic validation for quest-level gate stage references.
- Dialogue semantic validation for quest-level gate variable references.
- Valid `ExampleMod` dialogue registry updated to schema `0.5.0` with a
  synthetic quest-level gate declaration.
- `InvalidDialogueQuestGateRegistry` schema broken fixture.
- `MissingDialogueQuestGateReference` semantic broken fixture.
- `MissingDialogueQuestGateStageReference` semantic broken fixture.
- `MissingDialogueQuestGateVariableReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 28.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0, 0.2.0, 0.3.0, and 0.4.0
  - validate dialogue registry schema 0.5.0 quest-level gate shape

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
  - validate dialogue topic references resolve to declared dialogue topics
  - validate dialogue topic link targets resolve to declared dialogue topics
  - validate dialogue quest gate quest references resolve to declared quest IDs
  - validate dialogue quest gate stage references resolve inside the gate quest
  - validate dialogue quest gate variable references resolve inside the gate quest
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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate28/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate28/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate28/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueQuestGateRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestGateReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestGateStageReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestGateVariableReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 45 passed.
Focused back-compat tests passed: 17 passed.
Focused semantic tests passed: 36 passed.
Full suite passed: 123 passed.
TRX files emitted under TestResults/Gate28/Schema, TestResults/Gate28/BackCompat, and TestResults/Gate28/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.5.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueQuestGateRegistry returned exit 1 with WF-SCHEMA-001.
MissingDialogueQuestGateReference returned exit 1 with WF-SEM-026.
MissingDialogueQuestGateStageReference returned exit 1 with WF-SEM-027.
MissingDialogueQuestGateVariableReference returned exit 1 with WF-SEM-028.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue result-script variable mutation skeletons. | Open | Gate 29 |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add dialogue gate evaluation and compilation semantics. | Open | Later generator/tooling gate |
| Add `Link From` modeling. | Open | Later dialogue gate |
| Add dialogue graph traversal and cycle checks. | Open | Later dialogue validation gate |
| Add topic ordering, priority, prompt routing, and response routing. | Open | Later narrative/tooling gate |
| Validate external dialogue/topic/result-script references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 29 should add the dialogue result-script variable mutation skeleton.
