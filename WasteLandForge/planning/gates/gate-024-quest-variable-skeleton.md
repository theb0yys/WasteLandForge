# Gate 24 - Quest Variable Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 23, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 24 adds the first quest variable skeleton.

This gate preserves quest registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, and `0.5.0` and adds immutable Draft 2020-12 quest registry schema
`0.6.0`. The new schema supports minimal quest-local `variables` arrays and a
`variableEquals` condition skeleton that references those variables.

This gate also adds `WF-SEM-021` for quest conditions that reference
undeclared quest-local variable IDs.

This gate does not model variable mutation, raw result-script payloads,
dialogue condition compilation, non-integer variable typing, condition
evaluation, result-script execution, generator output, lockouts, fallback
routes, soft points of no return, branch semantics, or plugin record
generation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | GECK advanced conversation guidance uses quest variables with `GetQuestVariable ... == 0/1/2` to drive multi-step dialogue state. | FNV narrative systems research |
| Documented | Dialogue registry work needs condition language support over quest variables and variable-driven conversations. | FNV narrative systems research |
| Documented | Dialogue infos have result scripts and result fields can mutate state, but exact mutation modeling is not settled in this gate. | FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Semantic validators handle reference resolution after schema validation. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal integer variable declaration and `variableEquals` condition skeleton is safe before full condition language work because the research documents integer-like `GetQuestVariable` equality examples. | ADR-003 quest state model plus FNV narrative systems research |
| Inferred | Variable reference validation is a deterministic semantic rule and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Exact shipped variable patterns, hidden variables, variable mutation effects, result-script writes, dialogue condition compilation, non-integer variable support, and representative record shapes still require direct `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/quests/0.6.0/schema.json`.
- `WastelandForgeSchemaIds.Quest060`.
- Built-in schema catalog entry for quest schema `0.6.0`.
- Runtime quest schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, and `0.6.0`.
- Quest semantic validation for variable-equals condition references.
- `WF-SEM-021` for unresolved condition variable references.
- Valid `ExampleMod` quest registry updated to schema `0.6.0`.
- `InvalidQuestVariableRegistry` schema broken fixture.
- `MissingQuestConditionVariableReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 24.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve quest registry schemas 0.1.0, 0.2.0, 0.3.0, 0.4.0, and 0.5.0
  - validate quest registry schema 0.6.0 variable and variable-equals condition shape

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
  - validate quest condition variable references resolve inside the same quest
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate24/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate24/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate24/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestVariableRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingQuestConditionVariableReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 33 passed.
Focused back-compat tests passed: 13 passed.
Focused semantic tests passed: 25 passed.
Full suite passed: 96 passed.
TRX files emitted under TestResults/Gate24/Schema, TestResults/Gate24/BackCompat, and TestResults/Gate24/Semantic.
ExampleMod returned exit 0 with quest schema 0.6.0 enabled.
YamlExample returned exit 0 without a quest registry.
InvalidQuestVariableRegistry returned exit 1 with WF-SCHEMA-001.
MissingQuestConditionVariableReference returned exit 1 with WF-SEM-021.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue condition skeletons over quest state. | Open | Gate 25 |
| Add variable mutation from result scripts. | Open | Later narrative/generator gate |
| Add non-integer variable typing if representative records require it. | Open | Later narrative gate |
| Add full GECK condition language and boolean composition. | Open | Later narrative gate |
| Add lockouts, fallback routes, and soft points of no return. | Open | Later narrative gate |
| Validate external quest/stage/condition/variable references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 25 should add the dialogue condition skeleton.
