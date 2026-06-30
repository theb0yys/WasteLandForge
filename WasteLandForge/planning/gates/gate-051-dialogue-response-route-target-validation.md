# Gate 51 - Dialogue Response Route Target Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 27, Gate 32, Gate 50, ADR-003, ADR-007, ADR-011

## Definition

Gate 51 keeps dialogue registry schema `0.23.0` and adds semantic validation
for dialogue response route target topics.

The gate validates schema-valid `responseRoutes[].targetTopicId` values
against declared dialogue topics when dialogue topics are declared. It emits
`WF-SEM-036` for response routes that reference a topic ID that is not present
in the dialogue registry.

This gate does not create a new schema version. It does not decide response
route endpoint validation, response selection behavior, route taxonomy, Speech
Challenge branching, GECK mapping, plugin output, or generated artifact shape.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Dialogue is a state query and transition surface, not a flat text table. | Documented | R001 / ADR-003 |
| GECK dialogue infos include prompt, priority, response text, result script, and link fields. | Documented | R001 dialogue and GECK notes |
| Canonical source truth is YAML/JSON normalized to canonical JSON and validated by schema plus deterministic semantic validators. | Documented | R004 / ADR-007 |
| Validation should use stable diagnostic families and deterministic fixture-backed tests. | Documented | R008 / ADR-011 |
| Response route target topic reference validation is the smallest safe semantic check after Gate 50. | Inferred | Gate 50 left response route target validation open while defining `targetTopicId` as authored source state. |

## Deliverables

- Add `WF-SEM-036` in the validation pipeline for unknown response route target topics.
- Add a deterministic broken fixture for a missing response route target topic.
- Add semantic test coverage for the new diagnostic.
- Update docs and planning records for Gate 51 and the next gate.

## Validation mapping

Gate 51 preserves the existing validation stack:

```text
load / source validation
  -> schema validation
  -> semantic validation
  -> report output
```

`WF-SEM-036` runs after dialogue registry schema validation succeeds. Schema
invalid response route shape remains `WF-SCHEMA-001`.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate51/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release -- validate fixtures/projects/ExampleMod
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release -- validate fixtures/projects/BrokenCases/MissingDialogueResponseRouteTargetReference
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers`
  completed with 0 warnings and 0 errors.
- Passed: `WastelandForge.SemanticTests` completed with 65 passed, 0
  failed, and 0 skipped tests.
- Passed: full solution tests completed with 227 passed, 0 failed, and 0
  skipped tests.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors.
- Passed: CLI validation for
  `fixtures/projects/BrokenCases/MissingDialogueResponseRouteTargetReference`
  returned exit code 1 with `WF-SEM-036` at
  `/lines/0/responseRoutes/0/targetTopicId`.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Response route endpoint validation | Open | Proposed Gate 52. |
| Response route taxonomy and selection behavior | Open | Needs later design after more dialogue route evidence. |
| Speech Challenge branching integration | Open | Gate 33 kept exact evaluation semantics open. |
| GECK response mapping and plugin output | Open | Out of scope until generator/export gates. |

## Next gate

Gate 52 should add response route endpoint validation without changing response
selection behavior or GECK/plugin output.
