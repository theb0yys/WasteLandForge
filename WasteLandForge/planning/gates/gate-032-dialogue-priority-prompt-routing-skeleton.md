# Gate 32 - Dialogue Priority And Prompt Routing Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 31, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 32 adds the first dialogue priority and prompt routing skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, and `0.7.0` and adds immutable Draft 2020-12
dialogue registry schema `0.8.0`. The new schema adds optional line
`priority` and requires explicit `priority` when a line declares `promptText`.

Gate 32 adds `WF-SEM-033` for duplicate authored prompt routes with the same
`topicId`, `promptText`, and `priority`.

This gate does not model exact GECK priority ranges, priority ordering, prompt
routing execution, condition-aware prompt selection, response routing, Speech
Challenge handling, generated plugin records, or exact GECK info selection
behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | GECK dialogue infos have priority, prompt text, optional Speech Challenge handling, link fields, and result scripts. | FNV narrative systems research |
| Documented | Dialogue is a gameplay state query and state transition surface, not only text storage. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema defaults are annotations, not implicit authored state. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-level integer `priority` field is the smallest safe source-contract skeleton for the researched dialogue info priority surface. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | Requiring explicit priority when `promptText` is present avoids implicit routing state and follows the no-silent-defaults contract rule. | R004 / ADR-007 plus FNV narrative systems research |
| Inferred | Duplicate `topicId` + `promptText` + `priority` prompt routes are a semantic authoring ambiguity that can be checked without deciding GECK's full selection algorithm. | R004 / ADR-007 plus Gate 31 derived graph model |
| Open | Exact GECK priority ranges, ordering semantics, condition interactions, prompt routing execution, Speech Challenge handling, response routing, plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.8.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue080`.
- Built-in schema catalog entry for dialogue schema `0.8.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, `0.7.0`, and `0.8.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.8.0` with
  synthetic priority and prompt route declarations.
- `InvalidDialoguePromptRouteRegistry` schema broken fixture.
- `DuplicateDialoguePromptRoute` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 32.
- Documentation and planning updates recording Gate 32 and Gate 33.

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
    0.6.0, and 0.7.0
  - validate dialogue registry schema 0.8.0 priority and prompt route shape

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate32/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate32/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate32/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialoguePromptRouteRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/DuplicateDialoguePromptRoute --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 54 passed.
Focused back-compat tests passed: 20 passed.
Focused semantic tests passed: 44 passed.
Full suite passed: 143 passed.
TRX files emitted under TestResults/Gate32/Schema, TestResults/Gate32/BackCompat, and TestResults/Gate32/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.8.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialoguePromptRouteRegistry returned exit 1 with WF-SCHEMA-001.
DuplicateDialoguePromptRoute returned exit 1 with WF-SEM-033.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue speech challenge skeleton. | Open | Gate 33 |
| Decide exact GECK priority range and ordering semantics. | Open | Later dialogue validation gate |
| Add condition-aware prompt selection validation. | Open | Later dialogue validation gate |
| Add full dialogue graph traversal and cycle checks. | Open | Later dialogue validation gate |
| Add response routing and plugin output. | Open | Later narrative/tooling gate |
| Add raw dialogue result-script payload modeling. | Open | Later narrative/generator gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |

## Next Gate

Gate 33 should add the dialogue speech challenge skeleton.
