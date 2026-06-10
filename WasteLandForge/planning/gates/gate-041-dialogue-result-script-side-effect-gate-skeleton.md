# Gate 41 - Dialogue Result-Script Side-Effect Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 40, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 41 adds the first line-local dialogue result-script side-effect gate
source-contract skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, and `0.16.0` and adds immutable
Draft 2020-12 dialogue registry schema `0.17.0`. The new schema adds optional
line-local `resultScriptSideEffectGates` declarations with explicit `id`,
`effect`, and `state` fields.

Gate 41 adds no new `WF-SEM-*` rule. Invalid result-script side-effect gate
source shape is reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model an effect registry, exact side-effect taxonomy, raw
script semantics, execution ordering, GECK condition function mapping,
evaluation semantics, generated plugin records, or exact GECK info selection
behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue is a general-purpose state query and state transition surface with conditions and result scripts. | FNV narrative systems research |
| Documented | The Dialogue Registry should compile to a condition language and needs result scripts that mutate state. | FNV narrative systems research |
| Documented | Dialogue authoring needs condition support over result-script side effects. | WastelandForge narrative reactivity skill |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `resultScriptSideEffectGates` array with explicit `id`, `effect`, and `state` fields is the smallest safe source-contract skeleton for researched result-script side-effect checks. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference an effect registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact side-effect registry shape, effect taxonomy, raw script semantics, execution ordering, GECK condition mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.17.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0170`.
- Built-in schema catalog entry for dialogue schema `0.17.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`, and `0.17.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.17.0` with a
  synthetic result-script side-effect gate declaration.
- `InvalidDialogueResultScriptSideEffectGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 41.
- Documentation and planning updates recording Gate 41 and Gate 42.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.16.0
  - validate dialogue registry schema 0.17.0 result-script side-effect gate shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - add no new semantic rule until a side-effect registry or authored target
    state exists
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate41/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate41/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate41/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueResultScriptSideEffectGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 81 passed.
Focused back-compat tests passed: 29 passed.
Focused semantic tests passed: 54 passed.
Full suite passed: 189 passed.
TRX files emitted under TestResults/Gate41/Schema, TestResults/Gate41/BackCompat, and TestResults/Gate41/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.17.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueResultScriptSideEffectGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/resultScriptSideEffectGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue condition boolean composition skeleton. | Open | Gate 42 |
| Decide exact side-effect taxonomy and key format. | Open | Later dialogue validation gate |
| Add side-effect registry resolution. | Open | Later registry validation gate |
| Decide raw result-script semantics and execution ordering. | Open | Later narrative validation gate |
| Decide GECK condition mapping for result-script side-effect checks. | Open | Later dialogue validation gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 42 should add the dialogue condition boolean composition skeleton.
