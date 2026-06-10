# Gate 36 - Dialogue Faction And Reputation Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 35, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 36 adds the first line-local dialogue faction relation and reputation
standing gate source-contract skeletons.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, and
`0.11.0` and adds immutable Draft 2020-12 dialogue registry schema `0.12.0`.
The new schema adds optional line-local `factionGates` declarations with
explicit `id`, `faction`, and `relation` fields, plus optional line-local
`reputationGates` declarations with explicit `id`, `faction`, and `standing`
fields.

Gate 36 adds no new `WF-SEM-*` rule. Invalid faction or reputation gate source
shape is reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model faction registry resolution, exact faction relation
taxonomy, exact reputation standing taxonomy, fame/infamy math, GECK condition
function mapping, evaluation semantics, generated plugin records, or exact GECK
info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Factions determine actor reactions, including inter-faction relations, group combat reaction, and crime tracking. | FNV narrative systems research |
| Documented | Reputation is separate from faction relations and tracks fame, infamy, and social standing. | FNV narrative systems research |
| Documented | Reputation thresholds can unlock services, quest access, companion reactions, and dialogue-facing consequences. | FNV narrative systems research |
| Documented | Skill checks, faction checks, reputation checks, quest checks, perk checks, and identity checks are merged into dialogue authoring. | FNV narrative systems research |
| Documented | The Dialogue Registry needs first-class support for skill gates, perk gates, faction/reputation gates, quest-stage gates, variable-driven conversations, and result scripts. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | Separate `factionGates` and `reputationGates` arrays are the smallest safe source-contract skeleton that preserves the researched distinction between operational faction relation and social reputation standing. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference faction or reputation registries. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact faction registry shape, relation taxonomy, reputation standing taxonomy, threshold mapping, GECK condition mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.12.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0120`.
- Built-in schema catalog entry for dialogue schema `0.12.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`, and
  `0.12.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.12.0` with
  synthetic faction relation and reputation standing gate declarations.
- `InvalidDialogueFactionGateRegistry` schema broken fixture.
- `InvalidDialogueReputationGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 36.
- Documentation and planning updates recording Gate 36 and Gate 37.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0, 0.2.0, 0.3.0, 0.4.0, 0.5.0,
    0.6.0, 0.7.0, 0.8.0, 0.9.0, 0.10.0, and 0.11.0
  - validate dialogue registry schema 0.12.0 faction and reputation gate shape

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
  - validate dialogue Link From sources resolve to declared dialogue topics
  - validate dialogue link target topics have authored line endpoints
  - validate dialogue Link From source topics have authored line endpoints
  - validate duplicate dialogue prompt routes
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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate36/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate36/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate36/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueFactionGateRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueReputationGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 66 passed.
Focused back-compat tests passed: 24 passed.
Focused semantic tests passed: 49 passed.
Full suite passed: 164 passed.
TRX files emitted under TestResults/Gate36/Schema, TestResults/Gate36/BackCompat, and TestResults/Gate36/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.12.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueFactionGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/factionGates/0.
InvalidDialogueReputationGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/reputationGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue identity gate skeleton. | Open | Gate 37 |
| Add faction registry skeleton and reference validation. | Open | Later faction registry gate |
| Add reputation event/standing registry skeleton and reference validation. | Open | Later reputation registry gate |
| Decide exact faction relation taxonomy. | Open | Later dialogue validation gate |
| Decide exact reputation standing and threshold taxonomy. | Open | Later dialogue validation gate |
| Decide GECK condition mapping for faction and reputation checks. | Open | Later dialogue validation gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 37 should add the dialogue identity gate skeleton.
