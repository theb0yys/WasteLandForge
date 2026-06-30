# Gate 48 - Dialogue Condition Short-Circuit Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 42, Gate 44, Gate 46, Gate 47, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 48 adds the first explicit dialogue condition short-circuit source-contract
skeleton.

This gate preserves dialogue registry schemas `0.1.0` through `0.21.0` and
adds immutable Draft 2020-12 dialogue registry schema `0.22.0`. The new schema
adds optional boolean `shortCircuit` values under line-local `conditionLogic`
declarations and nested condition groups. The value records authored
short-circuit intent only.

Gate 48 adds no new `WF-SEM-*` rule. Invalid short-circuit source shape is
reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not decide short-circuit execution behavior, evaluation
semantics, precedence execution ordering, GECK condition-list mapping,
generated plugin records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue is gameplay state exposure and should support a condition language over quest stages, quest variables, faction reputation, skills, perks, identity, local world flags, event history, companion state, and result-script side effects. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Dialogue conditions are condition lists made of script functions, and the same general system governs quests, stages, objectives, packages, and dialogue. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | An optional boolean `shortCircuit` field is the smallest safe additive skeleton for authored condition short-circuit intent because it records author intent without deciding evaluation behavior, GECK mapping, or runtime behavior. | ADR-003 dialogue condition-language requirement plus Gates 42, 44, 46, and 47 |
| Open | Short-circuit execution behavior, precedence execution ordering, GECK condition-list mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.22.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0220`.
- Built-in schema catalog entry for dialogue schema `0.22.0`.
- Runtime dialogue schema dispatch for dialogue schema `0.22.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.22.0` with root
  and nested condition short-circuit intent.
- `InvalidDialogueConditionShortCircuitRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 48.
- Documentation and planning updates recording Gate 48 and Gate 49.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.21.0
  - validate dialogue registry schema 0.22.0 short-circuit shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, condition
    logic reference, and cross-registry semantic validators
  - no new short-circuit semantic rule in this gate
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate48/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate48/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate48/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueConditionShortCircuitRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 96 passed.
Focused back-compat tests passed: 34 passed.
Focused semantic tests passed: 62 passed.
Full suite passed: 220 passed.
TRX files emitted under TestResults/Gate48/Schema, TestResults/Gate48/BackCompat, and TestResults/Gate48/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.22.0 enabled.
InvalidDialogueConditionShortCircuitRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/conditionLogic/shortCircuit.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue condition logic identity validation. | Open | Gate 49 |
| Decide short-circuit execution behavior. | Open | Later narrative validation gate |
| Decide precedence execution ordering. | Open | Later narrative validation gate |
| Decide GECK condition-list mapping for boolean composition, negation, precedence, and short-circuit intent. | Open | Later dialogue/tooling gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 49 should add dialogue condition logic identity validation.
