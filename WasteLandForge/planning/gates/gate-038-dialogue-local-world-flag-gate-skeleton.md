# Gate 38 - Dialogue Local World Flag Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 37, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 38 adds the first line-local dialogue local world flag gate
source-contract skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, and `0.13.0` and adds immutable Draft 2020-12 dialogue registry
schema `0.14.0`. The new schema adds optional line-local `worldFlagGates`
declarations with explicit `id`, `flag`, and `state` fields.

Gate 38 adds no new `WF-SEM-*` rule. Invalid local world flag gate source
shape is reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model the world-state registry, exact local world flag
taxonomy, boolean/value modeling, scope/location/region semantics, GECK
condition function mapping, evaluation semantics, generated plugin records, or
exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | New Vegas-style reactivity is modeled as hybrid state that includes quest state, faction state, local world-state flags, and event history. | ADR-003 / FNV narrative systems research |
| Documented | Dialogue authoring needs condition support over quest, faction, world, and event state, including local world flags. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `worldFlagGates` array with explicit `id`, `flag`, and `state` fields is the smallest safe source-contract skeleton for researched local world flag checks. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference a world-state registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact world-state registry shape, local world flag taxonomy, value model, scope semantics, GECK condition mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.14.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0140`.
- Built-in schema catalog entry for dialogue schema `0.14.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, `0.13.0`, and `0.14.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.14.0` with a
  synthetic local world flag gate declaration.
- `InvalidDialogueWorldFlagGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 38.
- Documentation and planning updates recording Gate 38 and Gate 39.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.13.0
  - validate dialogue registry schema 0.14.0 local world flag gate shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - add no new semantic rule until a world-state registry or authored target
    state exists
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate38/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate38/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate38/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueWorldFlagGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 72 passed.
Focused back-compat tests passed: 26 passed.
Focused semantic tests passed: 51 passed.
Full suite passed: 174 passed.
TRX files emitted under TestResults/Gate38/Schema, TestResults/Gate38/BackCompat, and TestResults/Gate38/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.14.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueWorldFlagGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/worldFlagGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue event history gate skeleton. | Open | Gate 39 |
| Decide exact local world flag taxonomy and key format. | Open | Later dialogue validation gate |
| Add world-state registry resolution. | Open | Later registry validation gate |
| Decide local world flag value model and scope semantics. | Open | Later narrative validation gate |
| Decide GECK condition mapping for local world flag checks. | Open | Later dialogue validation gate |
| Add companion state gates. | Open | Later narrative gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 39 should add the dialogue event history gate skeleton.
