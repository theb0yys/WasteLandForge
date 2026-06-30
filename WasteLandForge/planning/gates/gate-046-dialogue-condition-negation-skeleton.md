# Gate 46 - Dialogue Condition Negation Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 44, Gate 45, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 46 adds the first explicit dialogue condition negation source-contract
skeleton.

This gate preserves dialogue registry schemas `0.1.0` through `0.19.0` and
adds immutable Draft 2020-12 dialogue registry schema `0.20.0`. The new schema
adds optional `negatedConditionIds` under line-local `conditionLogic`
declarations and nested condition groups. `negatedConditionIds` entries point
to conditions authored on the same dialogue line.

Gate 46 adds no new `WF-SEM-*` rule. Invalid negation source shape is reported
by runtime schema validation as `WF-SCHEMA-001`. Existing `WF-SEM-034`
condition ID reference validation now also traverses `negatedConditionIds`.

This gate does not decide negation execution semantics, precedence,
short-circuit behavior, GECK condition-list mapping, generated plugin records,
or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue is gameplay state exposure and should support a condition language over quest stages, quest variables, faction reputation, skills, perks, identity, local world flags, event history, companion state, and result-script side effects. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Dialogue conditions are condition lists made of script functions, and the same general system governs quests, stages, objectives, packages, and dialogue. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | `negatedConditionIds` is the smallest safe additive skeleton for authored line-local condition negation because it records negation intent without deciding GECK condition-list mapping or runtime evaluation semantics. | ADR-003 dialogue condition-language requirement plus Gate 42 through Gate 44 |
| Inferred | Reusing `WF-SEM-034` for negated condition IDs is safe because it preserves the same same-line authored-condition invariant introduced by Gate 43. | R004 / ADR-007 plus Gate 43 |
| Open | Negation execution semantics, precedence, short-circuit behavior, GECK condition-list mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.20.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0200`.
- Built-in schema catalog entry for dialogue schema `0.20.0`.
- Runtime dialogue schema dispatch for dialogue schema `0.20.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.20.0` with a
  synthetic negated condition reference.
- Recursive `WF-SEM-034` traversal for root and nested
  `conditionLogic.negatedConditionIds[]` entries.
- `InvalidDialogueConditionNegationRegistry` schema broken fixture.
- `MissingNegatedDialogueConditionLogicReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 46.
- Documentation and planning updates recording Gate 46 and Gate 47.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.19.0
  - validate dialogue registry schema 0.20.0 negated condition reference shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - preserve WF-SEM-034 for positive condition logic references
  - extend WF-SEM-034 traversal to negated condition references
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate46/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate46/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate46/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueConditionNegationRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingNegatedDialogueConditionLogicReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 90 passed.
Focused back-compat tests passed: 32 passed.
Focused semantic tests passed: 60 passed.
Full suite passed: 210 passed.
TRX files emitted under TestResults/Gate46/Schema, TestResults/Gate46/BackCompat, and TestResults/Gate46/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.20.0 enabled.
InvalidDialogueConditionNegationRegistry returned exit 1 with WF-SCHEMA-001 including /lines/0/conditionLogic/negatedConditionIds.
MissingNegatedDialogueConditionLogicReference returned exit 1 with WF-SEM-034 at /lines/0/conditionLogic/negatedConditionIds/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue condition precedence skeleton. | Open | Gate 47 |
| Decide short-circuit semantics. | Open | Later narrative validation gate |
| Decide GECK condition-list mapping for boolean composition and negation. | Open | Later dialogue/tooling gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 47 should add a dialogue condition precedence skeleton.
