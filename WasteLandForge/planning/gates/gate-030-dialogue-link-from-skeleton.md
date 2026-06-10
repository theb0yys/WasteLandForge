# Gate 30 - Dialogue Link From Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 29, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 30 adds the first dialogue Link From skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, and `0.6.0` and adds immutable Draft 2020-12 dialogue
registry schema `0.7.0`. The new schema allows line-local `links` entries to
declare `linkType: "linkFrom"` with a required `sourceTopicId`. Existing
`linkType: "linkTo"` entries keep using `targetTopicId`.

This gate adds `WF-SEM-030` for dialogue `linkFrom` source topic references
that do not resolve to declared dialogue topics.

This gate does not model full dialogue graph traversal, cycle checks, ordering,
priority, response routing, generated plugin records, or exact GECK link field
behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | GECK advanced conversation guidance describes short scenes that can chain topics with "Link To" and "Link From". | FNV narrative systems research |
| Documented | Dialogue is a state query and state transition surface, not only text storage. | FNV narrative systems research |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A minimal `linkFrom` declaration with `sourceTopicId` is the smallest safe skeleton before full graph traversal semantics. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | Link From source topic checks belong in `WF-SEM-*` because they validate authored cross-object meaning after schema shape succeeds. | R004 / ADR-007 plus R008 / ADR-011 |
| Open | Exact GECK link field behavior, graph traversal, cycle checks, ordering, priority, plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.7.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue070`.
- Built-in schema catalog entry for dialogue schema `0.7.0`.
- Runtime dialogue schema dispatch for `0.1.0`, `0.2.0`, `0.3.0`, `0.4.0`,
  `0.5.0`, `0.6.0`, and `0.7.0`.
- Dialogue semantic validation for `linkFrom` source topic references.
- Valid `ExampleMod` dialogue registry updated to schema `0.7.0` with a
  synthetic Link From declaration.
- `InvalidDialogueLinkFromRegistry` schema broken fixture.
- `MissingDialogueLinkFromReference` semantic broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 30.

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
    and 0.6.0
  - validate dialogue registry schema 0.7.0 Link From shape

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate30/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate30/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate30/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueLinkFromRegistry --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueLinkFromReference --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 51 passed.
Focused back-compat tests passed: 19 passed.
Focused semantic tests passed: 40 passed.
Full suite passed: 135 passed.
TRX files emitted under TestResults/Gate30/Schema, TestResults/Gate30/BackCompat, and TestResults/Gate30/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.7.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
InvalidDialogueLinkFromRegistry returned exit 1 with WF-SCHEMA-001 schema diagnostics for the invalid Link From shape.
MissingDialogueLinkFromReference returned exit 1 with WF-SEM-030.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue link graph validation skeleton. | Open | Gate 31 |
| Add full dialogue graph traversal and cycle checks. | Open | Later dialogue validation gate |
| Add dialogue topic ordering, priority, prompt routing, and response routing. | Open | Later narrative/tooling gate |
| Add raw dialogue result-script payload modeling. | Open | Later narrative/generator gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Validate external dialogue/topic/result-script references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 31 should add the dialogue link graph validation skeleton.
