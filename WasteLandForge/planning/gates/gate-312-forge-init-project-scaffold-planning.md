# Gate 312 - Forge Init Project Scaffold Planning

Status: Complete
Phase: CLI onboarding planning
Decision base: ADR-010, ADR-011, ADR-007, ADR-008, ADR-009, R006, R008

## Goal

Open the canonical `forge init` command with a planning-only scaffold report.

Gate 312 makes `forge init` useful for project onboarding without writing a
new project yet. It emits the project root, selected template, project ID,
planned scaffold paths, conflict status, validation command hint, and explicit
no-execution flags.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface. | ADR-010 / R006 |
| Documented | Forge must remain offline-first, AI-optional, and governed by local validation and tests. | ADR-011 / R008 |
| Documented | Source truth is YAML/JSON contracts normalized to canonical JSON and validated through schemas and semantic validators. | ADR-007 |
| Documented | Capabilities and providers are versioned registry data, not hard-coded provider assumptions. | ADR-008 |
| Documented | Generated/distribution outputs are disposable and must remain separated from canonical source truth. | ADR-009 |
| Inferred | The first safe `init` step should plan files and safety boundaries before creating manifests, registries, workflows, or editor files. | ADR-010 / ADR-011 |
| Open | Real scaffold file content, overwrite policy, interactive prompts, and post-write validation should be handled by later gates. | Gate 312 boundary |

## Implemented Behavior

`forge init` now supports:

```text
forge init [project-root] [--project <path>] [--template fnv-basic|fnv-framework|fnv-quest-pack|fnv-docs-only] [--name <name>] [--game falloutnv] [--format human|plain|json] [--dry-run] [--no-input]
```

The command emits a planning-only report for:

- `wastelandforge.json`,
- `src/registries/dependencies/main.json`,
- `src/registries/capabilities/runtime.json`,
- `generated/`,
- `dist/`,
- `.wastelandforge/config.jsonc`,
- `.wastelandforge/cache/`,
- `.vscode/tasks.json`,
- `.github/workflows/wastelandforge.yml`,
- `README.md`.

Existing empty project roots are allowed. Existing planned scaffold paths are
reported as conflicts and return exit code `6`.

## Not Implemented

Gate 312 does not implement:

- scaffold file writes,
- manifest or registry content emission,
- overwrite or force behavior,
- interactive prompts,
- automatic validation after scaffold creation,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
- runtime probes,
- plugin mutation,
- release publication,
- remote repository calls,
- signing or attestation,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| `forge init` routes to implemented command behavior | Complete | `ForgeCli.RunInitCommand` handles `init`. |
| Init help documents the planning-only contract | Complete | `CliHelpWriter.WriteInitHelp`. |
| JSON output exposes scaffold paths and execution boundaries | Complete | `InitPlanJsonSerializer`. |
| Plain/human output is available | Complete | `InitPlanTextRenderer`. |
| Existing planned paths are refused without writes | Complete | Golden CLI test coverage. |
| Diagnostic-only formats remain unavailable | Complete | `--format sarif` returns usage exit code `2`. |
| No scaffold files are written | Complete | Tests assert temp project roots remain unwritten. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Init
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next Forge Init Gate

Gate 316 should implement the first safe scaffold file emission for
`forge init`, because Gates 313 through 315 are already reserved for the
app-shell lane. Gate 316 should start with minimal manifest and registry file
emission under the selected project root, still refusing existing planned
paths and still stopping before provider installation, external tool
execution, MO2/GECK automation, runtime probes, plugin mutation, remote calls,
release publication, signing, attestation, or AI behavior.
