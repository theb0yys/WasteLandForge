# Gate 27 - Dialogue Topic Link Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 26, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 27 adds the first dialogue topic/link skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, and `0.3.0`
and adds immutable Draft 2020-12 dialogue registry schema `0.4.0`. The new
schema supports optional top-level `topics` declarations and minimal
line-local `links` arrays. Gate 27 only models `linkTo` target topic
references with stable IDs and optional summaries.

This gate adds `WF-SEM-024` for dialogue line `topicId` values that do not
resolve to declared dialogue topics when topics are declared, and `WF-SEM-025`
for dialogue `linkTo` target topic references that do not resolve to declared
dialogue topics when topics are declared.

This gate does not model `Link From`, full dialogue graph traversal, cycle
checks, topic ordering, info priority, prompt routing, response routing,
conversation-scene authoring, topic compilation into plugin records, or
generated dialogue output.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | GECK quests have dialogue tabs including Topics and Conversation, and infos have link fields and result scripts. | FNV narrative systems research |
| Documented | GECK advanced conversation guidance says short scenes can chain topics with `Link To` and `Link From`. | FNV narrative systems research |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs deterministic semantic validators. | R008 / ADR-011 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A minimal declared-topic list plus `linkTo` target reference is the smallest safe source-contract skeleton before full dialogue graph semantics are verified. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | Topic and link reference checks belong in `WF-SEM-*` because they validate cross-object meaning after schema shape succeeds. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | `Link From` behavior, ordering, priority, repeated-topic rules, topic cycles, prompt/response routing, plugin record output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.4.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue040`.
- Built-in schema catalog entry for dialogue schema `0.4.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, and
  `0.4.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.4.0` with
  synthetic topics and a `linkTo` declaration.
- `InvalidDialogueTopicRegistry` schema broken fixture.
- `MissingDialogueTopicReference` semantic broken fixture.
- `MissingDialogueTopicLinkReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 27.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0, 0.2.0, and 0.3.0
  - validate dialogue registry schema 0.4.0 topic and link shape

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
  - validate dialogue quest references resolve to declared quest IDs
  - validate dialogue condition stage references resolve inside the line quest
  - validate dialogue condition variable references resolve inside the line quest
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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate27/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate27/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate27/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueTopicRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueTopicReference --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueTopicLinkReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 42 passed.
Focused back-compat tests passed: 16 passed.
Focused semantic tests passed: 32 passed.
Full suite passed: 115 passed.
TRX files emitted under TestResults/Gate27/Schema, TestResults/Gate27/BackCompat, and TestResults/Gate27/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.4.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueTopicRegistry returned exit 1 with WF-SCHEMA-001.
MissingDialogueTopicReference returned exit 1 with WF-SEM-024.
MissingDialogueTopicLinkReference returned exit 1 with WF-SEM-025.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue quest-level gate skeletons. | Open | Gate 28 |
| Add `Link From` modeling. | Open | Later dialogue gate |
| Add dialogue graph traversal and cycle checks. | Open | Later dialogue validation gate |
| Add topic ordering, priority, prompt routing, and response routing. | Open | Later narrative/tooling gate |
| Add dialogue result-script variable mutation modeling. | Open | Later dialogue/generator gate |
| Validate external dialogue/topic/result-script references against plugin/tooling data. | Open | Later tool/provider gate |
| Add dialogue/topic compilation into plugin records. | Open | Later generator/tooling gate |

## Next Gate

Gate 28 should add the dialogue quest-level gate skeleton.
