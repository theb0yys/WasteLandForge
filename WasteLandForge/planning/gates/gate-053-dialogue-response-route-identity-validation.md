# Gate 53 - Dialogue Response Route Identity Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 50, Gate 51, Gate 52, ADR-003, ADR-007, ADR-011

## Definition

Gate 53 keeps dialogue registry schema `0.23.0` and adds semantic validation
for line-local response route identity.

The gate validates schema-valid `responseRoutes[].id` values. If the same
dialogue line declares the same response route ID more than once, validation
emits `WF-SEM-038`.

This gate does not create a new schema version. It does not decide global
logical ID uniqueness, response route key taxonomy, response route key
ambiguity validation, response selection behavior, Speech Challenge branching,
GECK mapping, plugin output, or generated artifact shape.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Dialogue is a state query and transition surface, not only a text table. | Documented | R001 / ADR-003 |
| Source truth is YAML/JSON normalized to canonical JSON and validated by schema plus deterministic semantic validators. | Documented | R004 / ADR-007 |
| Stable dotted logical IDs are the primary Forge identity mechanism. | Documented | R004 / ADR-007 |
| Semantic validators handle uniqueness and semantic invariants that JSON Schema cannot fully express for authored meaning. | Documented | R008 / ADR-011 |
| Line-local response route ID uniqueness is the smallest safe identity check after Gate 52. | Inferred | Gate 50 introduced line-local response routes and Gate 52 left route ID uniqueness open. |

## Deliverables

- Add `WF-SEM-038` in the validation pipeline for duplicate response route IDs
  authored on the same dialogue line.
- Add a deterministic broken fixture for duplicate response route identity.
- Add semantic test coverage for the new diagnostic.
- Update docs and planning records for Gate 53 and the next gate.

## Validation mapping

Gate 53 preserves the existing validation stack:

```text
load / source validation
  -> schema validation
  -> semantic validation
  -> report output
```

`WF-SEM-038` runs after dialogue registry schema validation succeeds. Invalid
response route shape remains `WF-SCHEMA-001`; unknown target topics remain
`WF-SEM-036`; target topics with no authored endpoint remain `WF-SEM-037`.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate53/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/DuplicateDialogueResponseRouteIdentity
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers`
  completed with 0 warnings and 0 errors.
- Passed: `WastelandForge.SemanticTests` completed with 67 passed, 0
  failed, and 0 skipped tests.
- Passed: full solution tests completed with 229 passed, 0 failed, and 0
  skipped tests.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors.
- Passed: CLI validation for
  `fixtures/projects/BrokenCases/DuplicateDialogueResponseRouteIdentity`
  returned exit code 1 with `WF-SEM-038` at
  `/lines/0/responseRoutes/1/id`.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Response route key ambiguity validation | Open | Proposed Gate 54. |
| Response route taxonomy and selection behavior | Open | Needs later design after more dialogue route evidence. |
| Speech Challenge branching integration | Open | Gate 33 kept exact evaluation semantics open. |
| GECK response mapping and plugin output | Open | Out of scope until generator/export gates. |

## Next gate

Gate 54 should add response route key ambiguity validation without deciding the
full route taxonomy, route selection behavior, or GECK/plugin output.
