# Gate 329 - Repo-Local Forge Tool Restore Helper Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-010, ADR-011, Gate 328, R006, R008

## Goal

Add a repository-local helper that packs `WastelandForge.Cli`, restores the
checked-in `.config/dotnet-tools.json` manifest from ignored local package
output, and verifies the restored `forge` local tool.

Gate 329 implements the helper only. It does not mutate generated task or
workflow templates, publish a package, add a root NuGet configuration, install
providers, run external game tools, or change CLI runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the same stable `forge` CLI. | R006 / ADR-010 |
| Documented | CI should be GitHub Actions-first with Windows and Ubuntu lanes, local SARIF generation, artifacts, least-privilege permissions, and pinned actions. | R008 / ADR-011 |
| Documented | The checked-in local tool manifest pins `wastelandforge.cli` version `0.1.0` and command `forge`. | Gate 327 |
| Inferred | A repository-local helper can be the stable bootstrap primitive for source-repository tasks and CI before generated scaffolds are changed. | Gate 328 |
| Open | Consumer-project local tool bootstrap still needs a published package or an explicit package-source policy before generated workflows can restore Forge without the source tree. | Gate 328 |

## Implemented Helper

Gate 329 adds:

```text
eng/Restore-ForgeTool.ps1
```

The helper:

- resolves the repository root from its own script path,
- validates that the CLI project and checked-in tool manifest exist,
- packs `src/WastelandForge.Cli/WastelandForge.Cli.csproj`,
- writes an ignored local-only NuGet config under
  `artifacts/local-tool/restore/gate-329/NuGet.config`,
- sets `NUGET_PACKAGES` to an ignored isolated package cache under
  `artifacts/local-tool/restore/gate-329/packages`,
- restores `.config/dotnet-tools.json` using the local package output source,
- verifies `dotnet tool run forge -- --version` unless `-NoVerify` is passed,
- restores the caller's previous `NUGET_PACKAGES` environment value.

The script uses PowerShell path joins so it can run under both Windows and
Ubuntu PowerShell.

## Usage

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Restore-ForgeTool.ps1
$env:NUGET_PACKAGES = (Resolve-Path -LiteralPath artifacts/local-tool/restore/gate-329/packages).Path
dotnet tool run forge -- help
Remove-Item Env:\NUGET_PACKAGES
```

Optional parameters:

```text
-Configuration Debug|Release
-RestoreRoot <path>
-NoVerify
```

`-RestoreRoot` may be relative to the repository root or absolute. The default
restore root stays under ignored `artifacts/local-tool/restore/gate-329/`.

## Not Implemented

Gate 329 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- generated `.vscode/tasks.json` mutation,
- generated `.github/workflows/wastelandforge.yml` mutation,
- repository CI workflow mutation,
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
| Restore helper exists | Complete | `eng/Restore-ForgeTool.ps1` is added. |
| Restore remains local-source only | Complete | The helper writes a local-only NuGet config under ignored `artifacts/local-tool/restore/gate-329/`. |
| Package cache is isolated | Complete | The helper sets `NUGET_PACKAGES` to an ignored gate-specific package cache during restore. |
| Tool command remains canonical | Complete | The helper verifies `dotnet tool run forge -- --version`. |
| Generated scaffolds remain unchanged | Complete | No generated VS Code task or GitHub Actions template is modified. |

## Validation

Required validation:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Restore-ForgeTool.ps1
$env:NUGET_PACKAGES = (Resolve-Path -LiteralPath artifacts/local-tool/restore/gate-329/packages).Path
dotnet tool run forge -- help
Remove-Item Env:\NUGET_PACKAGES
git diff --check
rg -n "Route the next development step to Gate 329|Gate 329: repo-local Forge local-tool restore helper scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 330|Gate 330" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

The restore helper writes ignored local package, NuGet config, and package
cache artifacts under `artifacts/local-tool/`.

## Next Gate

Gate 330 should integrate the restore helper into the repository-owned CI
bootstrap path, still stopping before generated workflow/task mutation, NuGet
publication, provider installation, external tool execution, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls beyond normal CI runner action checkout
and setup, signing, attestation, plugin mutation, VS Code extension
generation, language-server process startup, or AI behavior.
