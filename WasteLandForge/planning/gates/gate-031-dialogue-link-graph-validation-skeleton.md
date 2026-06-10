# Gate 31 - Dialogue Link Graph Validation Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 30, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 31 adds the first dialogue link graph validation skeleton.

This gate preserves dialogue registry schemas `0.1.0`, `0.2.0`, `0.3.0`,
`0.4.0`, `0.5.0`, `0.6.0`, and `0.7.0`. It adds no new schema because the
source contract shape for `linkTo` and `linkFrom` already exists in dialogue
schema `0.7.0`.

Gate 31 derives a minimal topic graph from schema-valid dialogue documents:
dialogue line `topicId` values are authored graph nodes, `linkTo` references
are target-topic edges, and `linkFrom` references are source-topic edges.

This gate adds `WF-SEM-031` for a `linkTo` target topic that is declared but
has no authored dialogue line endpoint. It adds `WF-SEM-032` for a `linkFrom`
source topic that is declared but has no authored dialogue line endpoint.

This gate does not require reciprocal Link To/Link From declarations, traverse
the dialogue graph, check cycles, model ordering or priority, route prompts or
responses, generate plugin records, or settle exact GECK link field behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | GECK advanced conversation guidance describes short scenes that can chain topics with "Link To" and "Link From". | FNV narrative systems research |
| Documented | Dialogue is a state query and state transition surface, not only text storage. | FNV narrative systems research |
| Documented | The contract layer separates canonical source from derived recomputable views such as graphs and reports. | R004 / ADR-007 |
| Documented | Semantic validators handle cross-document meaning, reference resolution, and semantic invariants after schema validation. | R004 / ADR-007 and R008 / ADR-011 |
| Documented | Public fixtures should be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Treating dialogue line `topicId` values as graph nodes is the smallest safe derived graph skeleton over the existing dialogue contract. | ADR-003 dialogue state model plus FNV narrative systems research |
| Inferred | Checking that linked declared topics have at least one authored line endpoint is a semantic graph invariant that does not require exact GECK traversal semantics. | R004 / ADR-007 plus Gate 30 open checks |
| Open | Exact GECK link field behavior, reciprocal link requirements, graph traversal, cycle checks, ordering, priority, response routing, plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- No new dialogue schema version.
- Derived dialogue graph endpoint validation over schema-valid dialogue
  `0.7.0` documents.
- `WF-SEM-031` for `linkTo` target topics with no authored dialogue line.
- `WF-SEM-032` for `linkFrom` source topics with no authored dialogue line.
- `MissingDialogueLinkTargetLine` semantic broken fixture.
- `MissingDialogueLinkSourceLine` semantic broken fixture.
- Semantic fixture tests for Gate 31.
- Documentation and planning updates recording Gate 31 and Gate 32.

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
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate31/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate31/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate31/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/YamlExample --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueLinkTargetLine --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueLinkSourceLine --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 51 passed.
Focused back-compat tests passed: 19 passed.
Focused semantic tests passed: 42 passed.
Full suite passed: 137 passed.
TRX files emitted under TestResults/Gate31/Schema, TestResults/Gate31/BackCompat, and TestResults/Gate31/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.7.0 enabled.
YamlExample returned exit 0 without a dialogue registry.
MissingDialogueLinkTargetLine returned exit 1 with WF-SEM-031.
MissingDialogueLinkSourceLine returned exit 1 with WF-SEM-032.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue priority and prompt routing skeleton. | Open | Gate 32 |
| Decide whether Forge should require reciprocal Link To/Link From declarations. | Open | Later dialogue validation gate |
| Add full dialogue graph traversal and cycle checks. | Open | Later dialogue validation gate |
| Add dialogue topic ordering, priority, prompt routing, and response routing. | Open | Later narrative/tooling gate |
| Add raw dialogue result-script payload modeling. | Open | Later narrative/generator gate |
| Add full GECK dialogue condition language and boolean composition. | Open | Later narrative gate |
| Validate external dialogue/topic/result-script references against plugin/tooling data. | Open | Later tool/provider gate |

## Next Gate

Gate 32 should add the dialogue priority and prompt routing skeleton.
