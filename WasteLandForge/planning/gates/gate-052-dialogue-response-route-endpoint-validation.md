# Gate 52 - Dialogue Response Route Endpoint Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 27, Gate 31, Gate 50, Gate 51, ADR-003, ADR-007, ADR-011

## Definition

Gate 52 keeps dialogue registry schema `0.23.0` and adds semantic validation
for dialogue response route target line endpoints.

The gate validates schema-valid `responseRoutes[].targetTopicId` values after
Gate 51 target-topic resolution. If the target topic is declared but no
dialogue line uses that topic, validation emits `WF-SEM-037`.

This gate does not create a new schema version. It does not decide response
route taxonomy, response selection behavior, Speech Challenge branching, GECK
mapping, plugin output, generated artifact shape, or route ID uniqueness.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Dialogue is a state query and transition surface, not only a text table. | Documented | R001 / ADR-003 |
| GECK dialogue infos include priority, prompt text, Speech Challenge handling, link fields, and result scripts. | Documented | R001 dialogue and GECK notes |
| Source truth is YAML/JSON normalized to canonical JSON and validated by schema plus deterministic semantic validators. | Documented | R004 / ADR-007 |
| Semantic validators handle reference resolution and other registry meaning that JSON Schema cannot prove alone. | Documented | R004 / ADR-007 and R008 / ADR-011 |
| Response route endpoint validation should mirror the existing Gate 31 link endpoint pattern without deciding selection behavior. | Inferred | Gate 31 endpoint validation plus Gate 50/51 response route source-contract boundaries. |

## Deliverables

- Add `WF-SEM-037` in the validation pipeline for declared response route
  target topics with no authored dialogue line endpoint.
- Add a deterministic broken fixture for a missing response route target line.
- Add semantic test coverage for the new diagnostic.
- Update docs and planning records for Gate 52 and the next gate.

## Validation mapping

Gate 52 preserves the existing validation stack:

```text
load / source validation
  -> schema validation
  -> semantic validation
  -> report output
```

`WF-SEM-037` runs only after the response route target topic is declared.
Undeclared response route target topics remain `WF-SEM-036`. Invalid response
route shape remains `WF-SCHEMA-001`.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate52/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingDialogueResponseRouteTargetLine
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers`
  completed with 0 warnings and 0 errors.
- Passed: `WastelandForge.SemanticTests` completed with 66 passed, 0
  failed, and 0 skipped tests.
- Passed: full solution tests completed with 228 passed, 0 failed, and 0
  skipped tests.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors.
- Passed: CLI validation for
  `fixtures/projects/BrokenCases/MissingDialogueResponseRouteTargetLine`
  returned exit code 1 with `WF-SEM-037` at
  `/lines/0/responseRoutes/0/targetTopicId`.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Response route identity validation | Open | Proposed Gate 53. |
| Response route taxonomy and selection behavior | Open | Needs later design after more dialogue route evidence. |
| Speech Challenge branching integration | Open | Gate 33 kept exact evaluation semantics open. |
| GECK response mapping and plugin output | Open | Out of scope until generator/export gates. |

## Next gate

Gate 53 should add response route identity validation without changing route
selection behavior or GECK/plugin output.
