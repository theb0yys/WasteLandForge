# Gate 57 - Capability Catalogue List Foundation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 5, Gate 13, Gate 56, ADR-006, ADR-008, ADR-010, ADR-011

## Definition

Gate 57 starts the capability scanner and Doctor foundation arc by
implementing the first real `forge capabilities` command:
`forge capabilities list`.

It adds a built-in Fallout: New Vegas capability/provider catalogue and
machine-readable plus human-readable listing output. The catalogue is the data
foundation for later local scans; this gate does not inspect the local
machine.

Gate 57 does not implement local provider detection, runtime probes, MO2 VFS
inspection, GECK or xEdit execution, capability resolution against project
requirements, Doctor export bundles, generator/build gating, MCM Extender JSON
generation, or build manifests. `forge capabilities scan` and
`forge capabilities explain` remain reserved.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Projects depend on capabilities, not provider names. Providers are versioned registry data that satisfy capabilities. | Documented | R005 / ADR-008 |
| Detection must be local-first and deterministic; runtime probes only enrich detection results. | Documented | R005 / ADR-008 |
| Provider detection has distinct families, including root-file, data-file, runtime-probe, executable-tool, MO2 environment, and manual override detectors. | Documented | R005 |
| The canonical CLI surface includes `forge capabilities list`, `forge capabilities scan`, and `forge capabilities explain`. | Documented | R006 / ADR-010 |
| Listing a built-in catalogue before scanning is the smallest useful implementation slice for the capability scanner arc. | Inferred | R005 detector model plus ADR-011 layered validation-first delivery |

## Deliverables

- Add registry-domain records for built-in capability catalogues, capability
  definitions, and provider definitions.
- Add the built-in FNV capability/provider catalogue covering the initial
  game, runtime, UI, animation, editor, xEdit, and MO2 capability families.
- Implement `forge capabilities list` with `--kind all|capabilities|providers`.
- Support `--format human|plain|json`, `--output <path>`, and `--no-input`.
- Preserve reserved status for `forge capabilities scan` and
  `forge capabilities explain`.
- Add CLI golden tests for catalogue JSON and provider filtering.
- Update current-gate documentation.

## Validation mapping

Gate 57 adds no diagnostic rule and no new validation pipeline stage.

The command reports catalogue data only:

```text
capability catalogue data
  -> text or JSON command output
```

Future capability scan gates will insert detector evidence and `WF-CAP-*`
diagnostics after project/source validation and before generation planning.

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --logger "trx;LogFileName=golden.trx" --results-directory TestResults/Gate57/Golden
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- capabilities list --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- capabilities list --kind providers --format plain
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod
git diff --check
```

Results:

- Passed: `dotnet build WastelandForge.sln -c Release -m:1
  --disable-build-servers --no-restore` completed with 0 warnings and 0
  errors.
- Passed: `WastelandForge.GoldenTests` completed with 18 passed, 0 failed,
  and 0 skipped tests. This includes catalogue JSON, provider filtering,
  output-file writing, and reserved `capabilities scan` status coverage.
- Passed: full solution tests completed with 233 passed, 0 failed, and 0
  skipped tests.
- Passed: `forge capabilities list --format json` returned the built-in
  catalogue with 19 capabilities and 15 providers.
- Passed: `forge capabilities list --kind providers --format plain` returned
  provider-only text output.
- Passed: `forge capabilities list --format json --output` wrote a valid JSON
  catalogue file under the local temp directory. Repo-local apphost output
  under `TestResults/` was blocked by the current sandbox, so the direct
  smoke check used temp output while the in-process regression test covered
  CLI output writing.
- Passed: `forge capabilities scan --format json` returned the reserved
  command JSON status. The shell reports this as nonzero, as expected for a
  reserved command.
- Passed: CLI validation for `fixtures/projects/ExampleMod` returned 0
  errors.
- Passed: `git diff --check` returned exit code 0. Git reported CRLF
  normalization warnings only.
- Passed: protected-term scan across project docs, planning, schemas,
  fixtures, source, and tests found no matches.
- Passed: stale Gate 57 dialogue-reference scan found no matches.
- Repository state: no files were staged or committed.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Local game-root detector | Open | Proposed Gate 58 work. |
| Runtime and data-file detectors | Open | Requires deterministic root/profile inputs. |
| Executable tool detectors for GECK, xEdit, and MO2 | Open | Requires local path handling and explainable evidence. |
| Capability scan report shape | Open | Should be defined before `forge capabilities scan` exits reserved state. |
| Capability explanation command | Open | Depends on scan evidence and catalogue IDs. |
| Build/generate gating against capabilities | Open | Belongs after scan evidence exists. |
| MCM Extender JSON generator | Open | Best first game-facing generator after capability scan and build manifest foundations. |

## Next gate

Gate 58 should implement the first local capability scan skeleton with
root-file, data-file, and executable-tool detector evidence. It should still
avoid runtime process probes, MO2 VFS launch, generated mod outputs, and MCM
Extender JSON generation until the scan report shape is stable.
