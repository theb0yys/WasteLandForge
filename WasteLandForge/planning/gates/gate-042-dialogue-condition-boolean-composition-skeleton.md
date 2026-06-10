# Gate 42 - Dialogue Condition Boolean Composition Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 41, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 42 adds the first line-local dialogue condition boolean composition
source-contract skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`, and `0.17.0` and adds
immutable Draft 2020-12 dialogue registry schema `0.18.0`. The new schema adds
optional line-local `conditionLogic` declarations with explicit `id`,
`operator`, and `conditionIds` fields.

Gate 42 adds no new `WF-SEM-*` rule. Invalid condition boolean composition
source shape is reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model condition ID reference resolution, nested groups,
negation, precedence, short-circuit behavior, GECK condition-list mapping,
evaluation semantics, generated plugin records, or exact GECK info selection
behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue conditions are condition lists made of script functions, and the same general system governs quests, stages, objectives, packages, and dialogue. | FNV narrative systems research |
| Documented | Dialogue should compile to a condition language rather than just a text database. | FNV narrative systems research |
| Documented | Quest-level conditions gate dialogue before info-level conditions. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `conditionLogic` object with explicit `id`, `operator`, and `conditionIds` fields is the smallest safe source-contract skeleton for researched dialogue condition boolean composition. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because Gate 42 validates composition shape only; condition ID reference resolution remains a later semantic rule. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact condition ID resolution, nested expression shape, negation, precedence, short-circuit behavior, GECK condition-list mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.18.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0180`.
- Built-in schema catalog entry for dialogue schema `0.18.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`, `0.17.0`, and
  `0.18.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.18.0` with a
  synthetic condition boolean composition declaration.
- `InvalidDialogueConditionLogicRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 42.
- Documentation and planning updates recording Gate 42 and Gate 43.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.17.0
  - validate dialogue registry schema 0.18.0 condition boolean composition shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - add no new semantic rule until condition logic reference resolution is
    introduced
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate42/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate42/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate42/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueConditionLogicRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 84 passed.
Focused back-compat tests passed: 30 passed.
Focused semantic tests passed: 55 passed.
Full suite passed: 194 passed.
TRX files emitted under TestResults/Gate42/Schema, TestResults/Gate42/BackCompat, and TestResults/Gate42/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.18.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueConditionLogicRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/conditionLogic.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue condition logic reference validation. | Open | Gate 43 |
| Decide nested condition expression shape. | Open | Later dialogue validation gate |
| Decide negation, precedence, and short-circuit semantics. | Open | Later narrative validation gate |
| Decide GECK condition-list mapping for boolean composition. | Open | Later dialogue/tooling gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 43 should add dialogue condition logic reference validation.
