# Gate 319 - Forge Init GitHub Actions Workflow Scaffold

Status: Complete
Phase: CLI onboarding implementation
Decision base: ADR-010, ADR-011, Gate 318, R008 CI and governance

## Goal

Extend `forge init` with a GitHub Actions workflow scaffold while keeping the
command offline-first and no-execution.

Gate 319 adds `.github/workflows/wastelandforge.yml` to the safe `forge init`
write set. `--dry-run` still emits the plan without writing files.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface. | ADR-010 / R006 |
| Documented | CI is GitHub Actions-first with a mandatory Windows lane and a fast Ubuntu lane. | ADR-011 / R008 |
| Documented | SARIF generation is local and canonical; GitHub upload is an optional publishing surface. | R008 |
| Documented | GitHub workflows should use least-privilege permissions and pinned third-party actions. | R008 |
| Inferred | A repo-local GitHub Actions workflow is safe after source, README, config, and editor task scaffold writes validate. | Gate 318 |
| Open | Runner installation/provisioning for the Forge CLI, schema associations, a VS Code extension, and language-server behavior remain later gates. | Gate 319 boundary |

## Implemented Behavior

`forge init <project-root>` now writes, when safe:

- `wastelandforge.json`,
- `src/registries/dependencies/main.json`,
- `src/registries/capabilities/runtime.json`,
- `.wastelandforge/config.jsonc`,
- `README.md`,
- `.vscode/tasks.json`,
- `.github/workflows/wastelandforge.yml`,
- required parent directories.

The workflow scaffold contains:

- least-privilege top-level `contents: read`,
- an Ubuntu validation lane with local SARIF export and optional SARIF upload,
- a Windows validation/build lane,
- a Windows release dry-run lane,
- pinned GitHub action commit SHAs,
- artifact upload surfaces,
- explicit checks that `forge` is available on the runner.

Generating the workflow file does not execute the workflow, contact GitHub,
install Forge, install providers, or run external tools.

## Not Implemented

Gate 319 does not implement:

- generated or distribution root creation,
- `.wastelandforge/cache/` creation,
- runner provisioning or Forge CLI installation in CI,
- GitHub remote calls,
- workflow execution,
- GitHub ruleset configuration,
- CODEOWNERS creation,
- Dependabot creation,
- VS Code schema associations,
- VS Code extension generation,
- language-server behavior,
- watch/background tasks,
- overwrite or force behavior,
- interactive prompts,
- automatic post-write validation execution,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
- runtime probes,
- plugin mutation,
- release publication,
- signing or attestation,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| `forge init` writes `.github/workflows/wastelandforge.yml` when safe | Complete | Golden CLI test checks the file. |
| Workflow includes Windows and Ubuntu lanes | Complete | Golden CLI test checks runner markers. |
| Workflow uses least-privilege permissions and pinned actions | Complete | Golden CLI test checks permissions and action SHAs. |
| `--dry-run` remains no-write | Complete | Golden CLI dry-run test asserts project root remains absent. |
| Existing planned paths are refused | Complete | Golden CLI refusal test returns exit code `6`. |
| Generated/dist/cache paths remain unwritten | Complete | Golden CLI test asserts they are absent. |
| Created scaffold validates | Complete | Golden CLI test runs `forge validate` on the temp scaffold and gets zero errors. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Init
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next Forge Init Gate

Gate 320 should extend `forge init` with editor schema association scaffold
generation, still preserving the same refusal policy and still stopping before
Forge runner installation/provisioning, provider installation, external tool
execution, MO2/GECK automation, runtime probes, real third-party plugin
fixtures, release publication, remote repository calls, signing, attestation,
plugin mutation, or AI behavior.
