# Gate 40 - Dialogue Companion State Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 39, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 40 adds the first line-local dialogue companion state gate source-contract
skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, `0.13.0`, `0.14.0`, and `0.15.0` and adds immutable Draft 2020-12
dialogue registry schema `0.16.0`. The new schema adds optional line-local
`companionStateGates` declarations with explicit `id`, `companion`, and
`state` fields.

Gate 40 adds no new `WF-SEM-*` rule. Invalid companion state gate source shape
is reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model the companion registry, exact companion identity
taxonomy, companion-specific observer model, trust/history/trigger value
semantics, GECK condition function mapping, evaluation semantics, generated
plugin records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | New Vegas-style reactivity includes companion-specific memory triggers and companion reactions as separate consequence surfaces. | FNV narrative systems research |
| Documented | Companion reactivity is not one universal approval meter; companions use bespoke observer models and local state machines. | FNV narrative systems research |
| Documented | Dialogue authoring needs condition support over quest, faction, world, event, and companion state. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `companionStateGates` array with explicit `id`, `companion`, and `state` fields is the smallest safe source-contract skeleton for researched companion state checks. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference a companion registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact companion registry shape, companion identity taxonomy, bespoke observer models, value semantics, GECK condition mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.16.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0160`.
- Built-in schema catalog entry for dialogue schema `0.16.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, and `0.16.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.16.0` with a
  synthetic companion state gate declaration.
- `InvalidDialogueCompanionStateGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 40.
- Documentation and planning updates recording Gate 40 and Gate 41.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.15.0
  - validate dialogue registry schema 0.16.0 companion state gate shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - add no new semantic rule until a companion registry or authored target
    state exists
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate40/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate40/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate40/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueCompanionStateGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 78 passed.
Focused back-compat tests passed: 28 passed.
Focused semantic tests passed: 53 passed.
Full suite passed: 184 passed.
TRX files emitted under TestResults/Gate40/Schema, TestResults/Gate40/BackCompat, and TestResults/Gate40/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.16.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueCompanionStateGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/companionStateGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue result-script side-effect gate skeleton. | Open | Gate 41 |
| Decide exact companion identity taxonomy and key format. | Open | Later dialogue validation gate |
| Add companion registry resolution. | Open | Later registry validation gate |
| Decide companion-specific observer models and value semantics. | Open | Later narrative validation gate |
| Decide GECK condition mapping for companion state checks. | Open | Later dialogue validation gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 41 should add the dialogue result-script side-effect gate skeleton.
