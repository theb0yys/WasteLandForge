# Gate 35 - Dialogue Perk Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 34, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 35 adds the first line-local dialogue perk gate source-contract skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, and `0.10.0` and adds
immutable Draft 2020-12 dialogue registry schema `0.11.0`. The new schema adds
optional line-local `perkGates` declarations with explicit `id` and `perk`
fields.

Gate 35 adds no new `WF-SEM-*` rule. Invalid perk gate source shape is
reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model the exact perk taxonomy, perk registry resolution,
GECK condition function mapping, evaluation semantics, generated plugin
records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Shipped content layers perk and dialogue checks into multiple outcomes. | FNV narrative systems research |
| Documented | Skill checks, faction checks, reputation checks, quest checks, perk checks, and identity checks are merged into dialogue authoring. | FNV narrative systems research |
| Documented | The Dialogue Registry needs first-class support for skill gates, perk gates, faction/reputation gates, quest-stage gates, variable-driven conversations, and result scripts. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema defaults are annotations, not implicit authored state. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `perkGates` array with explicit `id` and `perk` fields is the smallest safe source-contract skeleton for researched dialogue perk gates. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference a perk registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact perk taxonomy, registry resolution, GECK condition mapping, evaluation behavior, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.11.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0110`.
- Built-in schema catalog entry for dialogue schema `0.11.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, and `0.11.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.11.0` with a
  synthetic perk gate declaration.
- `InvalidDialoguePerkGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 35.
- Documentation and planning updates recording Gate 35 and Gate 36.

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
    0.6.0, 0.7.0, 0.8.0, 0.9.0, and 0.10.0
  - validate dialogue registry schema 0.11.0 perk gate shape

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate35/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate35/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate35/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialoguePerkGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 63 passed.
Focused back-compat tests passed: 23 passed.
Focused semantic tests passed: 47 passed.
Full suite passed: 158 passed.
TRX files emitted under TestResults/Gate35/Schema, TestResults/Gate35/BackCompat, and TestResults/Gate35/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.11.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialoguePerkGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/perkGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue faction and reputation gate skeleton. | Open | Gate 36 |
| Decide exact perk taxonomy and key format. | Open | Later dialogue validation gate |
| Add perk registry resolution. | Open | Later registry validation gate |
| Decide GECK condition mapping for perk checks. | Open | Later dialogue validation gate |
| Add perk gate evaluation semantics. | Open | Later dialogue validation gate |
| Add identity checks. | Open | Later narrative gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 36 should add the dialogue faction and reputation gate skeleton.
