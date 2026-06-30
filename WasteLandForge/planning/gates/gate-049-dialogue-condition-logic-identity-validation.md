# Gate 49 - Dialogue Condition Logic Identity Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 42, Gate 44, Gate 46, Gate 47, Gate 48, ADR-003, ADR-007, ADR-011

## Gate Definition

Gate 49 adds semantic validation for dialogue condition logic identity.

This gate preserves dialogue registry schema `0.22.0` and adds `WF-SEM-035`.
The validator checks schema-valid line-local `conditionLogic` trees and reports
duplicate root or nested `conditionLogic.id` values inside the same dialogue
line. The rule is scoped to one dialogue line's condition logic tree.

Gate 49 adds no new schema version.

This gate does not decide global logical ID uniqueness across every registry
object type, condition evaluation behavior, precedence execution ordering,
short-circuit execution behavior, GECK condition-list mapping, generated
plugin records, or exact GECK info selection behavior.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Dialogue is gameplay state exposure and should support a condition language over quest stages, quest variables, faction reputation, skills, perks, identity, local world flags, event history, companion state, and result-script side effects. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | Source truth is versioned YAML/JSON normalized to canonical JSON and validated with Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | Stable dotted lowercase logical IDs are the primary identity system. | R004 / ADR-007 |
| Documented | Schema validation is not enough; Forge needs semantic validators and synthetic redistributable fixtures. | R008 / ADR-011 |
| Inferred | A line-local duplicate `conditionLogic.id` validator is the smallest safe identity check for authored condition logic trees because it prevents ambiguous local logic nodes without deciding global registry-wide ID indexing. | ADR-007 identity policy plus Gates 42, 44, 46, 47, and 48 |
| Open | Global logical ID uniqueness, evaluation behavior, GECK condition-list mapping, generated plugin output, and representative shipped-record patterns still require later validation. | FNV narrative systems research open questions |

## Deliverables

- `WF-SEM-035` semantic validator for duplicate line-local condition logic IDs.
- `DuplicateDialogueConditionLogicIdentity` semantic broken fixture.
- Semantic fixture test coverage for `WF-SEM-035`.
- Documentation and planning updates recording Gate 49 and Gate 50.

## Validation Mapping

```text
load/source validation
  - discover manifest
  - parse JSON or YAML source contracts
  - normalize YAML into canonical JSON

schema validation
  - preserve dialogue registry schema 0.22.0
  - require schema-valid conditionLogic trees before semantic checks

semantic validation
  - preserve existing asset, voice, dialogue, quest, prompt route, condition
    logic reference, and cross-registry semantic validators
  - add WF-SEM-035 for duplicate conditionLogic.id values inside one dialogue
    line's condition logic tree
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate49/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/DuplicateDialogueConditionLogicIdentity --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused semantic tests passed: 63 passed.
Full suite passed: 221 passed.
TRX file emitted under TestResults/Gate49/Semantic.
ExampleMod returned exit 0 with dialogue schema 0.22.0 enabled.
DuplicateDialogueConditionLogicIdentity returned exit 1 with WF-SEM-035 at /lines/0/conditionLogic/groups/0/id and related root /lines/0/conditionLogic/id.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue response route skeleton. | Open | Gate 50 |
| Decide global logical ID uniqueness across registry object types. | Open | Later registry validation gate |
| Decide condition evaluation behavior. | Open | Later narrative validation gate |
| Decide precedence and short-circuit execution behavior. | Open | Later narrative validation gate |
| Decide GECK condition-list mapping for boolean composition, negation, precedence, and short-circuit intent. | Open | Later dialogue/tooling gate |
| Add plugin output. | Open | Later narrative/tooling gate |

## Next Gate

Gate 50 should add a dialogue response route skeleton.
