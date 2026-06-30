# Gate 44 - Dialogue Nested Condition Group Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 43, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 44 adds the first explicit nested dialogue condition group source-contract
skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`, `0.17.0`, and `0.18.0`
and adds immutable Draft 2020-12 dialogue registry schema `0.19.0`. The new
schema adds optional nested `groups` under line-local `conditionLogic`
declarations. Each group has an explicit `id`, `operator`, and direct
`conditionIds` and/or child groups.

Gate 44 adds no new `WF-SEM-*` rule. Invalid nested condition group source
shape is reported by runtime schema validation as `WF-SCHEMA-001`. Existing
`WF-SEM-034` condition ID reference validation now also traverses nested
groups.

This gate does not model negation, precedence, short-circuit behavior, GECK
condition-list mapping, evaluation semantics, generated plugin records, or
exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue conditions are condition lists made of script functions, and the same general system governs quests, stages, objectives, packages, and dialogue. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Dialogue should compile to a condition language rather than just a text database. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Quest-level conditions gate dialogue before info-level conditions. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A nested `groups` array under `conditionLogic` is the smallest safe additive skeleton for authored nested dialogue condition composition because it records grouping intent without deciding evaluation semantics. | ADR-003 dialogue condition-language requirement plus Gate 42 and Gate 43 |
| Inferred | Reusing `WF-SEM-034` for nested group condition IDs is safe because it preserves the same same-line authored-condition invariant introduced by Gate 43. | R004 / ADR-007 plus Gate 43 |
| Open | Negation, precedence, short-circuit behavior, GECK condition-list mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.19.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0190`.
- Built-in schema catalog entry for dialogue schema `0.19.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`, `0.17.0`, `0.18.0`,
  and `0.19.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.19.0` with a
  synthetic nested condition group declaration.
- Recursive `WF-SEM-034` traversal for nested `conditionLogic.groups[]`
  condition IDs.
- `InvalidDialogueNestedConditionGroupRegistry` schema broken fixture for an
  unsupported nested group operator.
- `MissingNestedDialogueConditionLogicReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 44.
- Documentation and planning updates recording Gate 44 and Gate 45.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.18.0
  - validate dialogue registry schema 0.19.0 nested condition group shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - preserve WF-SEM-034 for root condition logic references
  - extend WF-SEM-034 traversal to nested condition group references
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate44/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate44/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate44/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueNestedConditionGroupRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingNestedDialogueConditionLogicReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 87 passed.
Focused back-compat tests passed: 31 passed.
Focused semantic tests passed: 58 passed.
Full suite passed: 201 passed.
TRX files emitted under TestResults/Gate44/Schema, TestResults/Gate44/BackCompat, and TestResults/Gate44/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.19.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueNestedConditionGroupRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/conditionLogic/groups/0/operator.
MissingNestedDialogueConditionLogicReference returned exit 1 with WF-SEM-034 at /lines/0/conditionLogic/groups/0/conditionIds/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add a file-based GECK dialogue export validation bridge. | Open | Gate 45 |
| Add dialogue condition negation skeleton. | Open | Gate 46 |
| Decide precedence and short-circuit semantics. | Open | Later narrative validation gate |
| Decide GECK condition-list mapping for boolean composition. | Open | Later dialogue/tooling gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 45 should add a file-based GECK dialogue export validation bridge.
