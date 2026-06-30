# Gate 56 - Dialogue Response Route Taxonomy Evidence Pack Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 50, Gate 51, Gate 52, Gate 53, Gate 54, Gate 55, ADR-003, ADR-007, ADR-011

## Definition

Gate 56 creates a durable evidence-pack skeleton for dialogue response route
taxonomy.

It adds no schema version, runtime validator, diagnostic rule, fixture,
generated output, CLI behavior, route-key vocabulary, response selection
behavior, Speech Challenge route behavior, or GECK/plugin mapping.

The gate preserves dialogue registry schema `0.23.0` and semantic rules
`WF-SEM-036` through `WF-SEM-039`.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Dialogue is gameplay state exposure: GECK infos include priority, prompt text, Speech Challenge handling, link fields, result scripts, and condition lists. | Documented | R001 / ADR-003 |
| Dialogue schemas should not be treated as final until representative records are verified directly. | Documented | Narrative reactivity report |
| Source truth remains YAML/JSON contracts normalized to canonical JSON and validated through schema plus deterministic semantic validators. | Documented | R004 / ADR-007 |
| Public fixtures and public evidence must avoid redistributed game assets or third-party mod files without permission. | Documented | R008 / ADR-011 |
| A docs-only evidence pack is the smallest safe next step after Gate 55 because route taxonomy is explicitly evidence-blocked. | Inferred | Gate 55 plus ADR-011 validation discipline |

## Deliverables

- Add `docs/dialogue/` as the durable location for dialogue evidence packs.
- Add `docs/dialogue/response-route-taxonomy-evidence-pack.md`.
- Define required evidence classes, evidence item template, implementation
  blockers, unlock checklist, and open questions for route taxonomy.
- Update project status docs and planning index.

## Validation mapping

Gate 56 preserves the existing validation stack:

```text
load / source validation
  -> schema validation
  -> semantic validation
  -> report output
```

No new validator is inserted into the stack. Existing response route
diagnostics remain:

- `WF-SEM-036` for unknown response route target topics,
- `WF-SEM-037` for declared response route target topics without authored
  dialogue line endpoints,
- `WF-SEM-038` for duplicate response route IDs on the same line,
- `WF-SEM-039` for duplicate response route keys on the same line.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate56/Semantic
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/DuplicateDialogueResponseRouteKey
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers`
  completed with 0 warnings and 0 errors.
- Passed: `WastelandForge.SemanticTests` completed with 68 passed, 0
  failed, and 0 skipped tests.
- Passed: full solution tests completed with 230 passed, 0 failed, and 0
  skipped tests.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors.
- Passed: CLI validation for
  `fixtures/projects/BrokenCases/DuplicateDialogueResponseRouteKey` returned
  exit code 1 with `WF-SEM-039` at
  `/lines/0/responseRoutes/1/routeKey`.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Passed: protected-term scan across project docs, planning, schemas,
  fixtures, source, and tests found no matches.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Populate GECK documentation evidence | Open | Deferred to a later dialogue evidence gate. |
| Representative shipped-record inspection | Open | Requires local tool/user-owned install evidence and must not commit shipped content. |
| Response route taxonomy and selection behavior | Open | Requires evidence pack review before implementation. |
| Speech Challenge branching integration | Open | Gate 33 and Gate 55 keep exact evaluation and routing semantics open. |
| GECK response mapping and plugin output | Open | Out of scope until generator/export evidence exists. |

## Next gate

A later dialogue evidence gate should populate the GECK documentation evidence
item for response route taxonomy. It should still avoid schema, validation,
generator, or CLI implementation unless the evidence pack unlock checklist is
satisfied.
