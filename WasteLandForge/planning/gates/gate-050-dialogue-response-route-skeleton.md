# Gate 50 - Dialogue Response Route Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 27, Gate 32, Gate 49, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 50 adds the first dialogue response route source-contract skeleton.

This gate preserves dialogue registry schemas `0.1.0` through `0.22.0` and
adds immutable Draft 2020-12 dialogue registry schema `0.23.0`. The new schema
adds optional line-local `responseRoutes` declarations with stable route IDs,
authored route keys, and target topic IDs.

Gate 50 adds no new `WF-SEM-*` rule. Invalid response route source shape is
reported by runtime schema validation as `WF-SCHEMA-001`.

This gate does not decide route taxonomy, response selection behavior, Speech
Challenge branching, target reference validation, GECK condition/list mapping,
generated plugin records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue is gameplay state exposure and should support a condition language over quest stages, quest variables, faction reputation, skills, perks, identity, local world flags, event history, companion state, and result-script side effects. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | GECK dialogue infos have priority, prompt text, optional Speech Challenge handling, link fields, and result scripts. | FNV narrative systems research / Gate 32 |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Published schema IDs are immutable and include the full semantic version. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local `responseRoutes[]` array with `id`, `routeKey`, and `targetTopicId` is the smallest safe additive skeleton for authored response route intent because it records source intent without deciding GECK response selection or branching semantics. | ADR-003 dialogue state model plus Gates 27, 32, and 49 |
| Open | Route taxonomy, response selection behavior, Speech Challenge branching, target reference validation, GECK mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `schemas/dialogue/0.23.0/schema.json`.
- `WastelandForgeSchemaIds.Dialogue0230`.
- Built-in schema catalog entry for dialogue schema `0.23.0`.
- Runtime dialogue schema dispatch for dialogue schema `0.23.0`.
- Valid `ExampleMod` dialogue registry updated to schema `0.23.0` with a
  synthetic response route declaration.
- `InvalidDialogueResponseRouteRegistry` schema broken fixture.
- Schema, back-compat, and semantic fixture tests for Gate 50.
- Documentation and planning updates recording Gate 50 and Gate 51.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - validate manifest, dependency, capability, declared asset registries,
    declared quest registries, and declared dialogue registries
  - preserve dialogue registry schemas 0.1.0 through 0.22.0
  - validate dialogue registry schema 0.23.0 response route shape

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, condition
    logic reference, condition logic identity, and cross-registry semantic
    validators
  - no new response route semantic rule in this gate
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --logger "trx;LogFileName=schema.trx" --results-directory TestResults/Gate50/Schema
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --logger "trx;LogFileName=backcompat.trx" --results-directory TestResults/Gate50/BackCompat
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate50/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/InvalidDialogueResponseRouteRegistry --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused schema tests passed: 99 passed.
Focused back-compat tests passed: 35 passed.
Focused semantic tests passed: 64 passed.
Full suite passed: 226 passed.
TRX files emitted under TestResults/Gate50/Schema, TestResults/Gate50/BackCompat, and TestResults/Gate50/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.23.0 enabled.
InvalidDialogueResponseRouteRegistry returned exit 1 with WF-SCHEMA-001 at /lines/0/responseRoutes/0/routeKey.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue response route target validation. | Open | Gate 51 |
| Decide response route taxonomy. | Open | Later narrative validation gate |
| Decide response selection behavior and Speech Challenge branching. | Open | Later narrative validation gate |
| Decide GECK response route mapping and plugin output. | Open | Later dialogue/tooling gate |

## Next Gate

Gate 51 should add dialogue response route target validation.
