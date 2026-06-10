# Gate 43 - Dialogue Condition Logic Reference Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 42, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 43 adds semantic validation for line-local dialogue condition boolean
composition references.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`, `0.17.0`, and `0.18.0`.
It adds no new schema version.

Gate 43 adds `WF-SEM-034`: every `conditionLogic.conditionIds[]` value must
resolve to a condition `id` authored in the same dialogue line's `conditions`
array.

This gate does not model nested groups, negation, precedence, short-circuit
behavior, GECK condition-list mapping, evaluation semantics, generated plugin
records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue conditions are condition lists made of script functions, and the same general system governs quests, stages, objectives, packages, and dialogue. | FNV narrative systems research |
| Documented | Dialogue should compile to a condition language rather than just a text database. | FNV narrative systems research |
| Documented | Quest-level conditions gate dialogue before info-level conditions. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | Same-line condition ID resolution is the smallest safe semantic check for Gate 42's line-local boolean composition skeleton. | ADR-003 dialogue state model plus R004 / ADR-007 |
| Inferred | This gate should add no schema version because reference resolution is semantic validation over schema-valid `0.18.0` dialogue documents. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Nested expression shape, negation, precedence, short-circuit behavior, GECK condition-list mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `WF-SEM-034` semantic diagnostic for unresolved dialogue condition logic
  references.
- Dialogue semantic validation pass for `conditionLogic.conditionIds[]` values
  against same-line authored condition IDs.
- `MissingDialogueConditionLogicReference` semantic broken fixture.
- Semantic fixture test proving `WF-SEM-034` location, rule ID, severity,
  category, fingerprint, and message.
- Documentation and planning updates recording Gate 43 and Gate 44.

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
  - add no new schema version

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - validate dialogue condition logic references against same-line authored
    conditions
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate43/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate43/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate43/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueConditionLogicReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 84 passed.
Focused back-compat tests passed: 30 passed.
Focused semantic tests passed: 56 passed.
Full suite passed: 195 passed.
TRX files emitted under TestResults/Gate43/Schema, TestResults/Gate43/BackCompat, and TestResults/Gate43/Semantic.
ExampleMod returned exit 0 with no issues.
YamlExample returned exit 0 with no issues.
MissingDialogueConditionLogicReference returned exit 1 with WF-SEM-034 at /lines/0/conditionLogic/conditionIds/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue nested condition group skeleton. | Open | Gate 44 |
| Decide negation, precedence, and short-circuit semantics. | Open | Later narrative validation gate |
| Decide GECK condition-list mapping for boolean composition. | Open | Later dialogue/tooling gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 44 should add a dialogue nested condition group skeleton.
