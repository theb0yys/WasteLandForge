# Gate 332 - Repository-Owned Developer Task Local-Tool Bootstrap Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-010, ADR-011, Gate 331, R006, R008

## Goal

Create the source-repository VS Code task scaffold planned in Gate 331 so
developers can restore and invoke the checked-in Forge local tool manifest from
editor tasks without relying on a published package.

Gate 332 creates repository-owned tasks only. It does not mutate generated
`forge init` task or workflow templates, publish a package, add a root
`NuGet.config`, install providers, run external game tools, or change CLI
runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the stable `forge` CLI. | R006 / ADR-010 |
| Documented | VS Code workspace tasks are the first editor integration target for v0.1, with problem-matcher support for local diagnostics. | R006 editor integrations |
| Documented | CI and validation must stay offline-first, AI-optional, fixture-backed, and auditable. | R008 / ADR-011 |
| Documented | Repository-owned task bootstrap should use `eng/Restore-ForgeTool.ps1` and the checked-in local tool manifest. | Gate 331 |
| Inferred | Source-repository tasks may assume the WastelandForge CLI source project exists because this checkout owns `src/WastelandForge.Cli`. | Gate 328 / Gate 331 |
| Open | Consumer-project generated task and workflow templates still need a published package or explicit package-source policy before they can restore Forge without this source tree. | Gate 328 / Gate 331 |

## Implemented Task Scaffold

Gate 332 adds:

```text
.vscode/tasks.json
```

The task file defines:

- `Forge: Restore Local Tool`, which runs
  `eng/Restore-ForgeTool.ps1 -RestoreRoot artifacts/local-tool/restore/dev`,
- `Forge: Help`, which depends on restore and runs
  `dotnet tool run forge -- help`,
- `Forge: Validate ExampleMod`, which depends on restore and runs
  `dotnet tool run forge -- validate fixtures/projects/ExampleMod --format plain --no-input`,
- `Forge: Capabilities List`, which depends on restore and runs
  `dotnet tool run forge -- capabilities list --format plain --no-input`.

Every Forge task sets `NUGET_PACKAGES` to
`${workspaceFolder}/artifacts/local-tool/restore/dev/packages`, matching the
restore helper's developer restore root. The validation task includes a local
plain-output problem matcher for current Forge diagnostics.

## Boundary

The repository-owned task file is distinct from generated consumer-project
task templates:

- repository-owned tasks may assume `src/WastelandForge.Cli` exists,
- generated `forge init` tasks continue to invoke `forge` as a user-provided
  command,
- no generated task or workflow template changes in this gate,
- no VS Code extension or language-server process is introduced.

## Not Implemented

Gate 332 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- generated `.vscode/tasks.json` mutation,
- generated `.github/workflows/wastelandforge.yml` mutation,
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
| Source-repository task file exists | Complete | `.vscode/tasks.json` is added. |
| Tasks restore the checked-in local tool before Forge invocation | Complete | Forge tasks depend on `Forge: Restore Local Tool`. |
| Tasks use the isolated developer package cache | Complete | Forge tasks set `NUGET_PACKAGES` to `artifacts/local-tool/restore/dev/packages`. |
| Tasks use canonical ADR-010 commands | Complete | Tasks invoke `help`, `validate`, and `capabilities list` through `dotnet tool run forge --`. |
| Generated scaffolds remain unchanged | Complete | No generated VS Code task or GitHub Actions template is modified. |

## Validation

Required validation:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Restore-ForgeTool.ps1 -RestoreRoot artifacts/local-tool/restore/dev
$env:NUGET_PACKAGES = (Resolve-Path -LiteralPath artifacts/local-tool/restore/dev/packages).Path
dotnet tool run forge -- help
dotnet tool run forge -- validate fixtures/projects/ExampleMod --format plain --no-input
dotnet tool run forge -- capabilities list --format plain --no-input
Remove-Item Env:\NUGET_PACKAGES
dotnet build WastelandForge.sln -c Release --no-restore
dotnet test WastelandForge.sln -c Release --no-build --no-restore
git diff --check
rg -n "Route the next development step to Gate 332|Gate 332: repository-owned developer task local-tool bootstrap scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 333|Gate 333" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng .github .vscode
```

The task smoke writes ignored local package, NuGet config, and package cache
artifacts under `artifacts/local-tool/`.

## Next Gate

Gate 333 should close the repository-owned local-tool bootstrap lane and route
the next value slice, with the consumer-project package-source policy gap kept
explicit before generated workflow or task templates are mutated.
