# Gate 330 - Repository-Owned CI Local-Tool Bootstrap Integration

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-010, ADR-011, Gate 329, R006, R008

## Goal

Integrate the repo-local Forge local-tool restore helper into the
repository-owned GitHub Actions CI workflow so CI exercises the checked-in
`.config/dotnet-tools.json` manifest before invoking Forge validation and
release dry-run commands.

Gate 330 changes the repository CI workflow only. It does not mutate generated
`forge init` workflow or task templates, publish a package, add a root
`NuGet.config`, install providers, run external game tools, or change CLI
runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the stable `forge` CLI. | R006 / ADR-010 |
| Documented | CI should be GitHub Actions-first with Windows and Ubuntu lanes, local SARIF generation, TRX test output, artifacts, least-privilege permissions, and pinned actions. | R008 / ADR-011 |
| Documented | The repo-local restore helper packs `WastelandForge.Cli`, restores `.config/dotnet-tools.json` from ignored local package output, and verifies `dotnet tool run forge`. | Gate 329 |
| Inferred | Repository-owned CI can use the source-tree restore helper because the source tree is present after checkout. | Gate 328 / Gate 329 |
| Open | Consumer-project generated workflow and task templates still need a published package or explicit package-source policy before they can restore Forge without this source tree. | Gate 328 |

## Implemented CI Bootstrap

Gate 330 updates:

```text
.github/workflows/ci.yml
```

The workflow now restores the repo-local Forge local tool in these lanes:

- `validate-ubuntu` runs `eng/Restore-ForgeTool.ps1`, stores the isolated
  package cache path in `GITHUB_ENV`, and uses `dotnet tool run forge --`
  for SARIF validation and GitHub/Markdown validation output.
- `build-test-windows` runs `eng/Restore-ForgeTool.ps1` after tests as a
  mandatory Windows-lane local-tool bootstrap smoke.
- `release-dry-run` runs `eng/Restore-ForgeTool.ps1`, stores the isolated
  package cache path in `GITHUB_ENV`, and uses `dotnet tool run forge --`
  for `release verify`.

The generated `forge init` workflow and task templates remain unchanged.

## Not Implemented

Gate 330 does not implement:

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
- remote repository calls beyond normal GitHub Actions checkout and setup,
- signing or attestation,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Ubuntu validation restores the local tool | Complete | `validate-ubuntu` runs `eng/Restore-ForgeTool.ps1`. |
| Ubuntu validation invokes the restored tool | Complete | SARIF and GitHub validation steps call `dotnet tool run forge -- validate`. |
| Windows PR lane restores the local tool | Complete | `build-test-windows` runs `eng/Restore-ForgeTool.ps1`. |
| Release dry-run restores the local tool | Complete | `release-dry-run` runs `eng/Restore-ForgeTool.ps1`. |
| Release dry-run invokes the restored tool | Complete | The dry-run command calls `dotnet tool run forge -- release verify`. |
| Generated scaffolds remain unchanged | Complete | No generated VS Code task or GitHub Actions template is modified. |

## Validation

Required validation:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Restore-ForgeTool.ps1 -RestoreRoot artifacts/local-tool/restore/gate-330
$env:NUGET_PACKAGES = (Resolve-Path -LiteralPath artifacts/local-tool/restore/gate-330/packages).Path
dotnet tool run forge -- validate fixtures/projects/ExampleMod --format sarif --output artifacts/gate-330/wastelandforge-validation.sarif --no-input
dotnet tool run forge -- validate fixtures/projects/ExampleMod --format github --summary artifacts/gate-330/wastelandforge-validation.md --no-input
$json = & dotnet tool run forge -- release verify fixtures/projects/ExampleMod --format json --summary artifacts/gate-330/release-verify.md --no-input
$json | Set-Content -LiteralPath artifacts/gate-330/release-verify.json -Encoding utf8
Remove-Item Env:\NUGET_PACKAGES
git diff --check
rg -n "Route the next development step to Gate 330|Gate 330: repository-owned CI local-tool bootstrap integration" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 331|Gate 331" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng .github
```

The workflow-equivalent smoke writes ignored local package, NuGet config,
package cache, validation, and release dry-run artifacts under `artifacts/`.

## Next Gate

Gate 331 should plan repository-owned developer task local-tool bootstrap,
still stopping before generated workflow/task mutation, NuGet publication,
provider installation, external tool execution, MO2/GECK automation, runtime
probes, real third-party plugin fixtures, release publication, remote
repository calls, signing or attestation, plugin mutation, VS Code extension
generation, language-server process startup, or AI behavior.
