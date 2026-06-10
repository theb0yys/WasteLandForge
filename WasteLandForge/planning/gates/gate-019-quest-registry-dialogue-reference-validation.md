# Gate 19 - Quest Registry and Dialogue Reference Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 18, ADR-003, ADR-004, ADR-007, ADR-011

## Gate Definition

Gate 19 adds the first quest registry contract and a deterministic dialogue
quest reference validation rule.

This gate creates an immutable Draft 2020-12 quest registry schema, adds
optional `registries.quests` manifest wiring, validates quest registry
documents at runtime, and emits `WF-SEM-016` when a dialogue line `questId`
does not resolve to a declared quest ID.

This gate does not model quest stages, objectives, conditions, result scripts,
lockouts, quest variables, fallback routes, dialogue topic references, plugin
record compilation, or external tool-backed quest inspection.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | New Vegas quests are editor containers whose principal parts are objectives and dialogue. | FNV narrative systems research |
| Documented | Quest-level conditions gate dialogue before info-level dialogue conditions. | FNV narrative systems research |
| Documented | Quest state should own progression, objectives, lockouts, and local branching in the hybrid state model. | ADR-003 / FNV narrative systems research |
| Documented | Dialogue production is tightly coupled to quests and records; GECK can export quest dialogue and calculate voice assets. | ADR-004 / FNV asset pipeline research |
| Documented | Quest and dialogue registries belong to the narrative layer after the bootstrap contract layer stabilizes. | R004 / ADR-007 |
| Documented | Logical IDs are primary identity; FormIDs, plugin names, and editor IDs are external implementation references. | R004 / ADR-007 |
| Documented | Semantic validators handle cross-document rules such as reference resolution. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal quest registry with stable quest IDs and titles is safe before exact GECK record-shape schemas are verified. | ADR-003 quest-hub model plus R004 staged registry rollout |
| Inferred | Checking dialogue `questId` references against declared quest IDs is a semantic cross-registry rule and belongs under `WF-SEM-*`. | R004 semantic validation model and R008 rule families |
| Open | Exact quest stage, objective, condition, result-script, lockout, quest-variable, and fallback-route schema shape still requires representative `FalloutNV.esm` inspection. | FNV narrative systems research open questions |

## Deliverables

- `schemas/quests/0.1.0/schema.json`.
- `WastelandForgeSchemaIds.Quest010`.
- Built-in schema catalog entry for quest schema.
- Optional `registries.quests` manifest field.
- Runtime quest registry schema validation.
- `WF-SEM-016` for dialogue `questId` references missing declared quest IDs.
- Valid `ExampleMod` quest registry.
- `InvalidQuestRegistry` schema broken fixture.
- `MissingDialogueQuestReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 19.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries

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
  - validate dialogue quest references resolve to declared quest IDs
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=gate19-schema.trx" --results-directory TestResults/Gate19
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=gate19-backcompat.trx" --results-directory TestResults/Gate19
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=gate19-semantic.trx" --results-directory TestResults/Gate19
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal" --logger "trx;LogFileName=gate19-full.trx" --results-directory TestResults/Gate19
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidQuestRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueQuestReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueVoiceAssets --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 18 passed.
Focused back-compat tests passed: 8 passed.
Focused semantic tests passed: 15 passed.
Full suite passed: 66 passed.
TRX files emitted under TestResults/Gate19.
ExampleMod returned exit 0 with quest and dialogue registries enabled.
YamlExample returned exit 0 without a quest registry.
InvalidQuestRegistry returned exit 1 with WF-SCHEMA-001.
MissingDialogueQuestReference returned exit 1 with WF-SEM-016.
MissingDialogueVoiceAssets still returned exit 1 with WF-SEM-015.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add quest stages and objectives skeleton. | Open | Gate 20 |
| Add quest condition and result-script modeling. | Open | Later narrative gate |
| Add quest lockout, fallback route, and soft point-of-no-return semantics. | Open | Later narrative gate |
| Add dialogue topic/reference validation. | Open | Later dialogue gate |
| Validate external quest references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 20 should add the quest stages and objectives skeleton.
