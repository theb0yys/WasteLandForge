# Gate 34 - Dialogue Skill Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 33, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 34 adds the first line-local dialogue skill gate source-contract skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, and `0.9.0` and adds immutable
Draft 2020-12 dialogue registry schema `0.10.0`. The new schema adds optional
line-local `skillGates` declarations with explicit `id`, `skill`, and integer
`threshold` fields.

Gate 34 adds no new `WF-SEM-*` rule. Invalid skill gate source shape is
reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model the exact New Vegas skill taxonomy, GECK condition
function mapping, threshold ranges, evaluation semantics, Speech Challenge
integration, non-skill gates, generated plugin records, or exact GECK info
selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Shipped content can open dialogue paths through Speech, Science, Medicine, Barter, reputation, and other checks. | FNV narrative systems research |
| Documented | Skill checks, faction checks, reputation checks, quest checks, perk checks, and identity checks are merged into dialogue authoring. | FNV narrative systems research |
| Documented | The Dialogue Registry needs first-class support for skill gates, perk gates, faction/reputation gates, quest-stage gates, variable-driven conversations, and result scripts. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema defaults are annotations, not implicit authored state. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `skillGates` array with explicit `id`, `skill`, and integer `threshold` is the smallest safe source-contract skeleton for researched dialogue skill gates. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference a skill registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact skill taxonomy, threshold ranges, GECK condition mapping, evaluation behavior, Speech Challenge integration, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.10.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0100`.
- Built-in schema catalog entry for dialogue schema `0.10.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, and `0.10.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.10.0` with a
  synthetic skill gate declaration.
- `InvalidDialogueSkillGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 34.
- Documentation and planning updates recording Gate 34 and Gate 35.

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
    0.6.0, 0.7.0, 0.8.0, and 0.9.0
  - validate dialogue registry schema 0.10.0 skill gate shape

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate34/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate34/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate34/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueSkillGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 60 passed.
Focused back-compat tests passed: 22 passed.
Focused semantic tests passed: 46 passed.
Full suite passed: 153 passed.
TRX files emitted under TestResults/Gate34/Schema, TestResults/Gate34/BackCompat, and TestResults/Gate34/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.10.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueSkillGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/skillGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue perk gate skeleton. | Open | Gate 35 |
| Decide exact game skill taxonomy and key format. | Open | Later dialogue validation gate |
| Decide GECK condition mapping for skill checks. | Open | Later dialogue validation gate |
| Decide skill threshold ranges and comparison semantics. | Open | Later dialogue validation gate |
| Integrate skill gates with Speech Challenge authoring. | Open | Later dialogue validation gate |
| Add faction/reputation gates. | Open | Later narrative gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 35 should add the dialogue perk gate skeleton.
