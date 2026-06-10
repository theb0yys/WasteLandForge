# Gate 29 - Dialogue Result-Script Variable Mutation Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 28, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 29 adds the first dialogue result-script variable mutation skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, and `0.5.0` and adds immutable Draft 2020-12 dialogue registry
schema `0.6.0`. The new schema supports optional `mutations` arrays on
line-local dialogue result scripts. Gate 29 only models explicit
`questVariableIncrement` declarations with integer deltas against the dialogue
line's referenced quest.

This gate adds `WF-SEM-029` for dialogue result-script mutation variable
references that do not resolve inside the dialogue line's referenced quest.

This gate does not model raw script bodies, full result-script mutation
semantics, quest stage mutations, condition evaluation, execution ordering,
generated plugin records, dialogue result-script compilation, or exact GECK
script syntax.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue infos have result scripts, and dialogue is a state query and state transition surface. | FNV narrative systems research |
| Documented | GECK advanced conversation guidance describes variable-driven conversations where a single topic advances by incrementing a quest variable in result fields. | FNV narrative systems research |
| Documented | The Dialogue Registry needs support for variable-driven conversations and result scripts that mutate state. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A minimal `questVariableIncrement` mutation object is the smallest safe source-contract skeleton for the researched result-field variable increment pattern. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | Mutation variable reference checks belong in `WF-SEM-*` because they validate authored cross-object meaning after schema shape succeeds. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Raw script bodies, exact GECK syntax, non-variable result-script effects, quest stage mutations, condition evaluation, execution order, generated plugin records, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.6.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue060`.
- Built-in schema catalog entry for dialogue schema `0.6.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, and `0.6.0`.
- Dialogue semantic validation for result-script mutation variable references.
- Valid `ExampleMod` dialogue registry updated to schema `0.6.0` with a
  synthetic result-script quest-variable increment declaration.
- `InvalidDialogueResultScriptMutationRegistry` schema broken fixture.
- `MissingDialogueResultScriptMutationVariableReference` semantic broken
  fixture.
- Schema, back-compat, and semantic fixture tests for Gate 29.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0, 0.2.0, 0.3.0, 0.4.0, and 0.5.0
  - validate dialogue registry schema 0.6.0 result-script mutation shape

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
  - validate dialogue result-script mutation variable references resolve inside the line quest
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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate29/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate29/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate29/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueResultScriptMutationRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueResultScriptMutationVariableReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 48 passed.
Focused back-compat tests passed: 18 passed.
Focused semantic tests passed: 38 passed.
Full suite passed: 129 passed.
TRX files emitted under TestResults/Gate29/Schema, TestResults/Gate29/BackCompat, and TestResults/Gate29/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.6.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueResultScriptMutationRegistry returned exit 1 with WF-SCHEMA-001.
MissingDialogueResultScriptMutationVariableReference returned exit 1 with WF-SEM-029.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue `Link From` skeletons. | Open | Gate 30 |
| Add raw dialogue result-script payload modeling. | Open | Later narrative/generator gate |
| Add dialogue result-script quest stage mutation modeling. | Open | Later dialogue/generator gate |
| Add full result-script execution and side-effect semantics. | Open | Later generator/validation gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add dialogue graph traversal and cycle checks. | Open | Later dialogue validation gate |
| Validate external dialogue/topic/result-script references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 30 should add the dialogue `Link From` skeleton.
