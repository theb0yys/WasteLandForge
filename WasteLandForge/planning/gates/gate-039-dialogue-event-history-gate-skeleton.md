# Gate 39 - Dialogue Event History Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 38, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 39 adds the first line-local dialogue event history gate source-contract
skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
`0.12.0`, `0.13.0`, and `0.14.0` and adds immutable Draft 2020-12 dialogue
registry schema `0.15.0`. The new schema adds optional line-local
`eventHistoryGates` declarations with explicit `id`, `event`, and `state`
fields.

Gate 39 adds no new `WF-SEM-*` rule. Invalid event history gate source shape
is reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model the event-history registry, exact event signal
taxonomy, lifecycle model, consumer routing, time horizon semantics, GECK
condition function mapping, evaluation semantics, generated plugin records, or
exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | New Vegas-style reactivity is modeled as hybrid state that includes quest state, faction state, local world-state flags, and event history. | ADR-003 / FNV narrative systems research |
| Documented | Dialogue authoring needs condition support over quest, faction, world, and event state, including event history. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Event history should be authored consequence signals, not simulated gossip by default. Consumers include dialogue, companion observers, radio/news, codex, achievements, and endings. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `eventHistoryGates` array with explicit `id`, `event`, and `state` fields is the smallest safe source-contract skeleton for researched event history checks. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference an event-history registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact event-history registry shape, event signal taxonomy, lifecycle model, consumer routing, time horizon semantics, GECK condition mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.15.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0150`.
- Built-in schema catalog entry for dialogue schema `0.15.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, `0.13.0`, `0.14.0`, and `0.15.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.15.0` with a
  synthetic event history gate declaration.
- `InvalidDialogueEventHistoryGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 39.
- Documentation and planning updates recording Gate 39 and Gate 40.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.14.0
  - validate dialogue registry schema 0.15.0 event history gate shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, and
    cross-registry semantic validators
  - add no new semantic rule until an event-history registry or authored
    target state exists
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate39/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate39/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate39/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueEventHistoryGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 75 passed.
Focused back-compat tests passed: 27 passed.
Focused semantic tests passed: 52 passed.
Full suite passed: 179 passed.
TRX files emitted under TestResults/Gate39/Schema, TestResults/Gate39/BackCompat, and TestResults/Gate39/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.15.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueEventHistoryGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/eventHistoryGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue companion state gate skeleton. | Open | Gate 40 |
| Decide exact event signal taxonomy and key format. | Open | Later dialogue validation gate |
| Add event-history registry resolution. | Open | Later registry validation gate |
| Decide event lifecycle and time horizon semantics. | Open | Later narrative validation gate |
| Decide event-history consumer routing for dialogue, companions, radio/news, codex, achievements, and endings. | Open | Later narrative/tooling gate |
| Decide GECK condition mapping for event history checks. | Open | Later dialogue validation gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 40 should add the dialogue companion state gate skeleton.
