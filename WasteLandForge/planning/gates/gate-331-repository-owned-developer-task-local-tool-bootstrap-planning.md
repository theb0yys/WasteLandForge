# Gate 331 - Repository-Owned Developer Task Local-Tool Bootstrap Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-010, ADR-011, Gate 330, R006, R008

## Goal

Plan repository-owned developer task bootstrap for the checked-in Forge local
tool manifest so contributors can invoke the same `forge` command surface from
editor tasks without relying on a published package.

Gate 331 is planning only. It does not create `.vscode/tasks.json`, mutate
generated `forge init` workflow or task templates, publish a package, add a
root `NuGet.config`, install providers, run external game tools, or change CLI
runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the stable `forge` CLI. | R006 / ADR-010 |
| Documented | VS Code workspace tasks are the first editor integration target for v0.1, with problem-matcher support for local diagnostics. | R006 editor integrations |
| Documented | CI and validation must stay offline-first, AI-optional, fixture-backed, and auditable. | R008 / ADR-011 |
| Documented | Repository-owned CI can restore the checked-in local tool manifest through `eng/Restore-ForgeTool.ps1` before invoking `dotnet tool run forge`. | Gate 330 |
| Inferred | Source-repository developer tasks may use the repo-local restore helper because this checkout contains `src/WastelandForge.Cli` and `.config/dotnet-tools.json`. | Gate 329 / Gate 330 |
| Open | Consumer-project generated task templates still need a published package or explicit package-source policy before they can restore Forge without this source tree. | Gate 328 |

## Planned Developer Task Bootstrap

Gate 332 should add a repository-owned `.vscode/tasks.json` for this source
repository only. The task file should:

- define a `Forge: Restore Local Tool` task that runs
  `eng/Restore-ForgeTool.ps1 -RestoreRoot artifacts/local-tool/restore/dev`,
- define read-only Forge tasks that depend on local-tool restore and invoke
  `dotnet tool run forge -- ...`,
- set `NUGET_PACKAGES` for Forge tasks to
  `${workspaceFolder}/artifacts/local-tool/restore/dev/packages`,
- keep task commands thin and use canonical ADR-010 command names,
- include a local problem matcher for plain Forge validation diagnostics,
- avoid tasks that write release dry-run output by default.

Recommended first repository-owned tasks:

```text
Forge: Restore Local Tool
Forge: Help
Forge: Validate ExampleMod
Forge: Capabilities List
```

Release dry-run, package, generated-output, watch/background, provider, and
external-tool tasks should stay out of the first task scaffold.

## Boundary

The repository-owned task file is distinct from generated consumer-project
task templates:

- repository-owned tasks may assume `src/WastelandForge.Cli` exists,
- generated `forge init` tasks must continue to invoke `forge` as a user tool
  until package-source policy is resolved,
- no generated task or workflow template should be changed in Gate 332.

## Not Implemented

Gate 331 does not implement:

- `.vscode/tasks.json` creation,
- generated `.vscode/tasks.json` mutation,
- generated `.github/workflows/wastelandforge.yml` mutation,
- CLI runtime behavior changes,
- new command names or aliases,
- root `NuGet.config`,
- NuGet publication,
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
| Developer task bootstrap route is planned | Complete | Gate 331 records the source-repository task boundary. |
| Generated scaffolds remain out of scope | Complete | Gate 331 explicitly keeps generated task and workflow templates unchanged. |
| Package-source gap remains open | Complete | Consumer-project restore remains blocked on package publication or package-source policy. |
| Next implementation slice is routed | Complete | Gate 332 is routed to repository-owned developer task local-tool bootstrap scaffold. |

## Validation

Required validation:

```text
Test-Path -LiteralPath .vscode
git diff --check
rg -n "Route the next development step to Gate 331|Gate 331: repository-owned developer task local-tool bootstrap planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 332|Gate 332" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng .github
```

## Next Gate

Gate 332 should scaffold repository-owned developer task local-tool bootstrap
through `.vscode/tasks.json`, still stopping before generated workflow/task
mutation, NuGet publication, provider installation, external tool execution,
MO2/GECK automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing or attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
