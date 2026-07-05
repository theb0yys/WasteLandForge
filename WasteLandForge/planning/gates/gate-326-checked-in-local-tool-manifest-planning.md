# Gate 326 - Checked-In Local Tool Manifest Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-010, ADR-011, Gate 325, Microsoft .NET CLI local-tool docs

## Goal

Plan the checked-in repository-pinned `.config/dotnet-tools.json` flow for the
Forge CLI now that `WastelandForge.Cli` can pack and install as a local .NET
tool.

Gate 326 is planning only. It does not commit `.config/dotnet-tools.json`,
publish a package, mutate workflows, or change CLI runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Repository-pinned tooling through .NET local tool manifests reduces version drift and supports reproducible CI. | R006 / ADR-010 |
| Documented | Local tools are recorded in `.config/dotnet-tools.json` and are available to the manifest directory and subdirectories. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Documented | `dotnet tool restore` installs local tools listed in the manifest that is in scope for the current directory. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Documented | Starting in .NET 10, tool install can auto-create a manifest if none is found, so bootstrap commands should opt out when exact manifest placement matters. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Inferred | Until `WastelandForge.Cli` is published to a stable feed, a checked-in manifest must be paired with an explicit local package-source restore contract. | Gate 325 local package smoke |
| Open | Public NuGet publication, signed package restore, remote package sources, workflow integration, and consumer-project manifest generation remain future decisions. | Gate 326 boundary |

Current external references:

- `https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use`
- `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install`
- `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-restore`

## Manifest Flow Decision

The next implementation gate may check in a root local tool manifest with:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "wastelandforge.cli": {
      "version": "0.1.0",
      "commands": [
        "forge"
      ],
      "rollForward": false
    }
  }
}
```

That manifest should live at `.config/dotnet-tools.json` and should be treated
as the repository-pinned `forge` command contract. The package ID is lower-case
in the manifest because the .NET tool install smoke wrote
`wastelandforge.cli` for package `WastelandForge.Cli`.

## Package Source Constraint

A checked-in manifest alone is not enough while the package remains local. On
a clean checkout, `dotnet tool restore` cannot resolve `WastelandForge.Cli`
unless one of these is true:

- the package exists in an enabled NuGet source,
- the caller passes `--add-source artifacts/local-tool/nupkg` after running
  `dotnet pack`,
- a future gate adds a reviewed package-source configuration,
- a future release gate publishes the package to a stable feed.

Gate 326 therefore rejects an implicit restore path. Future restore commands
must be explicit about local package source until publication is intentionally
gated.

## Planned Gate 327 Contract

Gate 327 should add the checked-in manifest scaffold and document this
bootstrap order for source-built/local-package use:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet tool restore --add-source artifacts\local-tool\nupkg
dotnet tool run forge -- --version
dotnet tool run forge -- help
```

The implementation should still avoid:

- root `NuGet.config` changes that clear or override normal package sources,
- generated workflow mutation,
- NuGet publication,
- provider installation,
- external game-tool execution,
- runtime probes,
- signing or attestation.

## Not Implemented

Gate 326 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- checked-in `.config/dotnet-tools.json`,
- root `NuGet.config`,
- `dotnet tool restore`,
- NuGet publication,
- release publication,
- generated workflow mutation,
- workflow execution,
- standalone executable packaging changes,
- package signing or attestation,
- package license expression selection,
- CODEOWNERS creation,
- Dependabot creation,
- VS Code extension generation,
- language-server behavior,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
- runtime probes,
- plugin mutation,
- remote repository calls,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Checked-in manifest shape is planned | Complete | This gate defines the future `.config/dotnet-tools.json` content. |
| Local package-source constraint is explicit | Complete | Restore must use an explicit local source until publication is gated. |
| Gate 327 is scoped | Complete | Gate 327 is routed to checked-in local tool manifest scaffold. |
| No manifest is committed | Complete | `.config/dotnet-tools.json` remains absent. |
| No runtime behavior changes are made | Complete | This gate is docs and prompt routing only. |

## Validation

Required validation:

```text
if (Test-Path -LiteralPath ".config\dotnet-tools.json") { exit 1 }
git diff --check
rg -n "Route the next development step to Gate 326|Gate 326: checked-in local tool manifest planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 327|Gate 327" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

Runtime build/test is not required for Gate 326 because it does not change
source code, project metadata, package output, manifest files, or runtime
behavior.

## Next Gate

Gate 327 should add the checked-in local tool manifest scaffold and explicit
local package-source restore documentation, still stopping before NuGet
publication, generated workflow mutation, provider installation, external tool
execution, MO2/GECK automation, runtime probes, real third-party plugin
fixtures, release publication, remote repository calls, signing, attestation,
plugin mutation, VS Code extension generation, language-server process
startup, or AI behavior.
