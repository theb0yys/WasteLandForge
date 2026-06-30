# Gate 45 - GECK Dialogue Export Validation Bridge

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 44, ADR-002, ADR-004, ADR-007, ADR-010, ADR-011

## Gate Definition

Gate 45 adds the first GECK-adjacent validation bridge without controlling,
automating, importing into, or mutating an open GECK session.

The bridge is exposed through the existing ADR-010 command surface as:

```text
forge validate --geck-dialogue-export <path>
```

The option validates that a GECK dialogue export text file exists, can be read
as text, and is non-empty. It appends GECK export load diagnostics to the same
canonical diagnostic report produced by the project validation pipeline.

This gate does not parse GECK dialogue rows into canonical registries, import
dialogue back into GECK, inspect plugin records, automate the open GECK
process, reconcile GECK voice asset calculations, or decide exact GECK
condition-list mapping.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Forge integrates with GECK and the surrounding FNV tooling ecosystem but does not replace those tools. | Fallout New Vegas Tooling Ecosystem and Modding Landscape |
| Documented | GECK remains canonical for forms, quests, dialogue, terminals, and world data. | Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow |
| Documented | Forge owns production-layer validation, workflow integration, dialogue manifests, and voice manifests, not GECK record editing or raw plugin editing. | Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow / ADR-004 |
| Documented | GECK Quest Data exposes authoring workflow actions including Export Quest Dialogue and Calculate Voice Assets. | Fallout New Vegas Narrative Systems and Reactive World Design |
| Documented | `forge validate` is part of the canonical ADR-010 command surface and must remain offline-first and deterministic. | R006 / ADR-010 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | A file-based export check is the smallest safe GECK bridge because it validates an artifact the user explicitly saved from GECK without assuming live editor control or record mutation authority. | ADR-002, ADR-004, ADR-010, ADR-011 |
| Open | Exact GECK export row format parsing, canonical registry import, plugin record mapping, MO2 launch context, xEdit audit integration, and GECK voice calculation reconciliation still require later evidence. | FNV tooling and narrative research open implementation questions |

## Deliverables

- `forge validate --geck-dialogue-export <path>` CLI option.
- `GeckDialogueExportValidator`.
- `WF-LOAD-009` for missing GECK dialogue export files.
- `WF-LOAD-010` for unreadable or binary-looking GECK dialogue export files.
- `WF-LOAD-011` for empty GECK dialogue export files.
- `fixtures/geck/dialogue/synthetic-quest-dialogue-export.txt`.
- CLI golden tests for valid, missing, and empty GECK dialogue export cases.
- `/forge validate --geck-dialogue-export <path>` routing notes in the
  project-local slash command plugin and command prompts.
- Documentation and planning updates recording Gate 45 and moving dialogue
  condition negation to Gate 46.

## Validation Mapping

```text
load/source validation
  - discover and validate the Forge project manifest and source contracts
  - optionally load the user-specified GECK dialogue export text file
  - report missing, unreadable/binary-looking, and empty export files

schema validation
  - unchanged from Gate 44

semantic validation
  - unchanged from Gate 44
```

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --logger "trx;LogFileName=golden.trx" --results-directory TestResults/Gate45/Golden
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --geck-dialogue-export fixtures/geck/dialogue/synthetic-quest-dialogue-export.txt --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --geck-dialogue-export fixtures/geck/dialogue/missing-export.txt --format json --no-input
git diff --check
```

Results:

```text
Build succeeded with 0 warnings and 0 errors.
Focused golden tests passed: 15 passed.
Full suite passed: 204 passed.
TRX file emitted under TestResults/Gate45/Golden/golden.trx.
Synthetic GECK dialogue export validation returned exit 0 with no diagnostics.
Missing GECK dialogue export validation returned exit 1 with WF-LOAD-009.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add dialogue condition negation skeleton. | Open | Gate 46 |
| Parse a representative GECK dialogue export into a typed intermediate model. | Open | Later GECK bridge gate |
| Decide canonical registry import from GECK dialogue export rows. | Open | Later GECK bridge gate |
| Reconcile GECK Calculate Voice Assets output with Forge voice worklists. | Open | Later voice/tooling gate |
| Decide MO2-aware GECK launch and profile context detection. | Open | Later tooling integration gate |
| Decide xEdit-backed plugin audit handoff. | Open | Later tooling integration gate |

## Next Gate

Gate 46 should add a dialogue condition negation skeleton.
