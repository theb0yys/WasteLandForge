# Gate 18 - Dialogue Registry and Voice Worklist Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 17, ADR-004, ADR-007, ADR-011

## Gate Definition

Gate 18 adds the first dialogue registry contract and a deterministic voice
worklist validation skeleton.

This gate creates an immutable Draft 2020-12 dialogue registry schema, adds
optional `registries.dialogue` manifest wiring, validates dialogue registry
documents at runtime, and emits `WF-SEM-015` when a dialogue line voice work
item does not have matching `.wav`, `.ogg`, and `.lip` assets declared in the
asset registry.

This gate does not create a full GECK dialogue condition language, compile
plugin records, validate quest references, inspect audio metadata, run GECK,
invoke external encoders, or detect local lip processing prerequisites.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue production is tightly coupled to quests and records; GECK can export quest dialogue and calculate voice assets. | FNV asset pipeline research |
| Documented | Forge should generate dialogue manifests, per-voice worklists, and CI checks from the record layer rather than maintain them manually. | FNV asset pipeline research |
| Documented | Forge should own voice manifests and verify WAV/OGG pairs, LIP prerequisites, and voice folder mappings. | ADR-004 / FNV asset pipeline research |
| Documented | WastelandForge source truth is YAML/JSON contracts normalized to canonical JSON, validated by JSON Schema plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Dialogue registry belongs to the narrative layer after the bootstrap registry layer stabilizes. | R004 / ADR-007 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal dialogue registry can safely model dialogue line identity, quest/topic ownership, response text, and voice worklist fields before full GECK condition support exists. | ADR-004 voice workflow and ADR-007 registry model |
| Inferred | Cross-checking dialogue voice work items against declared asset target stems is a deterministic semantic validation layer and belongs under `WF-SEM-*`. | R008 rule families and ADR-007 cross-registry validation |
| Open | Quest registry references, condition language, result scripts, dialogue record compilation, audio metadata validation, and GECK lip processing prerequisites remain later work. | Later narrative, audio, and capability gates |

## Deliverables

- `schemas/dialogue/0.1.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue010`.
- Built-in schema catalog entry for dialogue schema.
- Optional `registries.dialogue` manifest field.
- Runtime dialogue registry schema validation.
- `WF-SEM-015` for dialogue voice work items missing declared `.wav`, `.ogg`,
  or `.lip` assets.
- Valid `ExampleMod` dialogue registry and matching synthetic voice/lip assets.
- `InvalidDialogueRegistry` schema broken fixture.
- `MissingDialogueVoiceAssets` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 18.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    and declared dialogue registries

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
  - preserve WF-SEM-014 for dependency references to undefined capabilities
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate18
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueVoiceAssets --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed.
Focused back-compat tests passed.
Focused semantic tests passed.
Full suite passed.
TRX files emitted under TestResults/Gate18.
ExampleMod returned exit 0 with dialogue registry and voice worklist enabled.
InvalidDialogueRegistry returned exit 1 with WF-SCHEMA-001.
MissingDialogueVoiceAssets returned exit 1 with WF-SEM-015.
git diff --check passed.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add quest registry skeleton and validate dialogue `questId` references. | Open | Gate 19 |
| Add topic/reference validation once narrative registries exist. | Open | Later narrative gate |
| Add GECK-style condition language and result script modeling. | Open | Later dialogue gate |
| Add WAV/OGG sample-rate, bitrate, channel, and codec metadata validation. | Open | Later audio gate |
| Detect GECK lip processing prerequisites in local environment or capability scans. | Open | Later capability/environment gate |

## Next Gate

Gate 19 should add the quest registry skeleton and dialogue quest reference
validation.
