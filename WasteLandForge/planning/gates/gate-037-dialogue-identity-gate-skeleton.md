# Gate 37 - Dialogue Identity Gate Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 36, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 37 adds the first line-local dialogue identity gate source-contract
skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
and `0.12.0` and adds immutable Draft 2020-12 dialogue registry schema
`0.13.0`. The new schema adds optional line-local `identityGates`
declarations with explicit `id` and `identity` fields.

Gate 37 adds no new `WF-SEM-*` rule. Invalid identity gate source shape is
reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not model identity taxonomy, actor/player identity mapping,
identity registry resolution, GECK condition function mapping, evaluation
semantics, generated plugin records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Skill checks, faction checks, reputation checks, quest checks, perk checks, and identity checks are merged into dialogue authoring. | FNV narrative systems research |
| Documented | The Dialogue Registry needs a condition language over quest stages, quest variables, faction reputation, faction relation, skills, perks, identity checks, local world flags, event history, companion state, and result-script side effects. | WastelandForge narrative reactivity skill / FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `identityGates` array with explicit `id` and `identity` fields is the smallest safe source-contract skeleton for researched dialogue identity checks. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | No semantic rule is needed in this gate because the new fields do not yet reference an identity registry or other authored state. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact identity taxonomy, actor/player mapping, identity registry shape, GECK condition mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.13.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0130`.
- Built-in schema catalog entry for dialogue schema `0.13.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`, `0.10.0`, `0.11.0`,
  `0.12.0`, and `0.13.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.13.0` with a
  synthetic identity gate declaration.
- `InvalidDialogueIdentityGateRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 37.
- Documentation and planning updates recording Gate 37 and Gate 38.

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
    0.6.0, 0.7.0, 0.8.0, 0.9.0, 0.10.0, 0.11.0, and 0.12.0
  - validate dialogue registry schema 0.13.0 identity gate shape

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate37/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate37/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate37/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueIdentityGateRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 69 passed.
Focused back-compat tests passed: 25 passed.
Focused semantic tests passed: 50 passed.
Full suite passed: 169 passed.
TRX files emitted under TestResults/Gate37/Schema, TestResults/Gate37/BackCompat, and TestResults/Gate37/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.13.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueIdentityGateRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/identityGates/0.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue local world flag gate skeleton. | Open | Gate 38 |
| Decide exact identity taxonomy and key format. | Open | Later dialogue validation gate |
| Add identity registry resolution. | Open | Later registry validation gate |
| Decide actor/player identity mapping. | Open | Later dialogue validation gate |
| Decide GECK condition mapping for identity checks. | Open | Later dialogue validation gate |
| Add event history gates. | Open | Later narrative gate |
| Add companion state gates. | Open | Later narrative gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 38 should add the dialogue local world flag gate skeleton.
