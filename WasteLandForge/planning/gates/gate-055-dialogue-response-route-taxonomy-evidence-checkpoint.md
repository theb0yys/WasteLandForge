# Gate 55 - Dialogue Response Route Taxonomy Evidence Checkpoint

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 50, Gate 51, Gate 52, Gate 53, Gate 54, ADR-003, ADR-007, ADR-011

## Definition

Gate 55 is a stop-and-check evidence gate for dialogue response route
taxonomy. It adds no schema version, runtime validator, diagnostic rule,
fixture, generated output, or CLI behavior.

The gate records that Forge has enough response route support to validate
source shape, target-topic references, target endpoints, line-local route
identity, and line-local route-key ambiguity. It does not yet have enough
GECK or shipped-record evidence to define allowed `routeKey` vocabulary,
response route selection behavior, Speech Challenge routing integration, or
plugin output mapping.

Gate 55 preserves dialogue registry schema `0.23.0` and semantic rules
`WF-SEM-036` through `WF-SEM-039`.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Dialogue is gameplay state exposure: GECK infos include priority, prompt text, Speech Challenge handling, link fields, result scripts, and condition lists. | Documented | R001 / ADR-003 |
| Source truth is YAML/JSON normalized to canonical JSON and validated by JSON Schema plus deterministic semantic validators. | Documented | R004 / ADR-007 |
| Semantic validators handle cross-document references, uniqueness, and authored invariants that JSON Schema cannot fully express. | Documented | R008 / ADR-011 |
| Public fixtures must remain synthetic and redistributable rather than using Bethesda assets or third-party mod files. | Documented | R008 / ADR-011 |
| Exact narrative schema details should be verified against representative `FalloutNV.esm` records before Forge commits to concrete validators or code generators. | Documented | Narrative reactivity report open implementation warning |
| A route taxonomy checkpoint is the smallest safe next step after Gates 50 through 54 because those gates validate local route shape and ambiguity but leave route meaning unresolved. | Inferred | Gates 50 through 54 plus the narrative report's direct-record verification warning |

## Evidence checkpoint outcome

Gate 55 explicitly blocks implementation of:

- allowed `responseRoutes[].routeKey` enum values or reserved taxonomy,
- default, success, failure, skill, Speech Challenge, or other route meanings,
- response route selection ordering and tie-breaking,
- condition-aware response route execution,
- Speech Challenge success and failure route integration,
- GECK dialogue `Link To` or prompt export mapping for response routes,
- plugin record generation from response route declarations.

Those topics remain open until Forge has a documented evidence pack based on
representative GECK documentation and direct record inspection.

## Deliverables

- Record the Gate 55 evidence checkpoint.
- Update project status docs to show that response route taxonomy remains
  open.
- Preserve current schema, fixture, validation, and CLI behavior.
- Record the next gate as an evidence pack skeleton before taxonomy
  implementation.

## Validation mapping

Gate 55 preserves the existing validation stack:

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
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --logger "trx;LogFileName=semantic.trx" --results-directory TestResults/Gate55/Semantic
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
- Passed: protected-term scan across project docs, schemas, fixtures, source,
  and tests found no matches.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Response route taxonomy evidence pack | Open | Needs a documented evidence pack before route-key taxonomy or selection behavior is implemented. |
| Response route taxonomy and selection behavior | Open | Requires representative GECK and shipped-record evidence. |
| Speech Challenge branching integration | Open | Gate 33 kept exact evaluation and routing semantics open. |
| GECK response mapping and plugin output | Open | Out of scope until generator/export gates. |

## Next gate

Gate 56 should create a dialogue response route taxonomy evidence pack
skeleton. It should still avoid implementing taxonomy, selection, or GECK
output behavior until the evidence checklist is populated.
