# Gate 328 - Generated Workflow And Task Bootstrap Integration Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-010, ADR-011, Gates 318, 319, 327, R006, R008

## Goal

Plan how generated VS Code tasks and GitHub Actions workflow scaffolds should
bootstrap the repository-pinned Forge local tool flow now that
`.config/dotnet-tools.json` is checked in.

Gate 328 is planning only. It does not mutate generated task or workflow
templates, publish a package, add a root NuGet configuration, install
providers, run external tools, or change CLI runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the same stable `forge` CLI. | R006 / ADR-010 |
| Documented | CI should be GitHub Actions-first with Windows and Ubuntu lanes, local SARIF generation, artifacts, least-privilege permissions, and pinned actions. | R008 / ADR-011 |
| Documented | The checked-in local tool manifest pins `wastelandforge.cli` version `0.1.0` and command `forge`. | Gate 327 |
| Inferred | The WastelandForge source repository can pack and restore its own CLI tool from local package output before invoking `dotnet tool run forge`. | Gate 327 validation |
| Inferred | A generated consumer project cannot assume the WastelandForge source tree exists, so `forge init` task and workflow templates must not blindly call `dotnet pack src/WastelandForge.Cli/...`. | Gates 318, 319, 327 |
| Open | Consumer-project local tool bootstrap still needs a published package or an explicit package-source policy before generated workflows can restore Forge without the source tree. | Gate 328 boundary |

## Current Scaffold State

Gate 318 generated `.vscode/tasks.json` with tasks that call `forge`
directly:

```text
forge validate . --format plain --no-input
forge capabilities scan --project . --format plain --no-input
forge build . --target reports --format plain --no-input
```

Gate 319 generated `.github/workflows/wastelandforge.yml` with `FORGE_COMMAND:
forge` and explicit runner checks that fail if `forge` is not available on
`PATH`.

Gate 327 checked in `.config/dotnet-tools.json`, but restore still requires
this source-repository-only sequence while the package remains unpublished:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet tool restore --add-source artifacts\local-tool\nupkg
dotnet tool run forge -- --version
```

## Bootstrap Split

Generated workflow/task integration must split two cases:

1. **WastelandForge source repository bootstrap.** This repository contains the
   CLI source project, package metadata, local package output convention, and
   checked-in tool manifest. It may pack `WastelandForge.Cli`, restore the
   manifest from `artifacts/local-tool/nupkg`, verify `dotnet tool run forge
   -- --version`, then call Forge through `dotnet tool run forge -- ...`.
2. **Consumer mod project bootstrap.** A project created by `forge init` has
   Forge source contracts and generated task/workflow scaffolds, but it should
   not contain WastelandForge source code. Until publication or an explicit
   local package-source policy exists, generated consumer workflows should keep
   the current "Forge is available" check rather than pretending they can pack
   Forge from source.

This keeps R006's thin-wrapper model intact without creating an invalid
consumer-project assumption.

## Planned Integration Sequence

The next implementation should add a repository-local restore helper as the
stable bootstrap primitive:

```text
eng\Restore-ForgeTool.ps1
```

That helper should:

- run from the WastelandForge repository root,
- pack `src\WastelandForge.Cli\WastelandForge.Cli.csproj` in Release mode,
- restore `.config\dotnet-tools.json` from `artifacts\local-tool\nupkg`,
- support an ignored isolated package cache under `artifacts\local-tool\`,
- optionally verify `dotnet tool run forge -- --version`,
- avoid remote package sources unless a later gate explicitly enables them.

After that helper is validated, a later gate can update repository-owned
workflow/task guidance or generated scaffolds to call the helper where the
WastelandForge source tree is known to exist. Consumer-project generated
workflows should remain source-agnostic until NuGet publication or another
reviewed package-source contract is implemented.

## Not Implemented

Gate 328 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- generated `.vscode/tasks.json` mutation,
- generated `.github/workflows/wastelandforge.yml` mutation,
- root `NuGet.config`,
- NuGet publication,
- workflow execution,
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
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Generated scaffold bootstrap split is documented | Complete | This gate separates source-repository bootstrap from consumer-project bootstrap. |
| Existing task/workflow assumptions are recorded | Complete | This gate records current `forge`/`FORGE_COMMAND` usage and runner availability checks. |
| Next implementation gate is concrete | Complete | Gate 329 is routed to a repo-local local-tool restore helper scaffold. |
| Consumer-project source-pack assumption is rejected | Complete | This gate keeps generated consumer workflows from assuming `src/WastelandForge.Cli` exists. |
| No generated scaffolds are mutated | Complete | This gate changes planning and routing documentation only. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 328|Gate 328: generated workflow and task bootstrap integration planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 329|Gate 329" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

Runtime build/test is not required for Gate 328 because it does not change
source code, project metadata, generated scaffold templates, package metadata,
tool manifests, or runtime behavior.

## Next Gate

Gate 329 should add the repository-local Forge local-tool restore helper
scaffold, still stopping before generated workflow/task mutation, NuGet
publication, provider installation, external tool execution, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing, attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI
behavior.
