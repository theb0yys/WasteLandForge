# Gate 327 - Checked-In Local Tool Manifest Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-010, ADR-011, Gate 326, Microsoft .NET CLI local-tool docs

## Goal

Add the checked-in repository-pinned local tool manifest for the Forge CLI and
document the explicit local package-source restore flow required while
`WastelandForge.Cli` remains unpublished.

Gate 327 implements the manifest scaffold only. It does not publish a package,
mutate generated workflows, add a root NuGet configuration, or change CLI
runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Repository-pinned tooling through .NET local tool manifests reduces version drift and supports reproducible CI. | R006 / ADR-010 |
| Documented | Local tools are recorded in `.config/dotnet-tools.json` and are available to the manifest directory and subdirectories. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Documented | `dotnet tool restore` installs local tools listed in the manifest in scope for the current directory. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Inferred | Until `WastelandForge.Cli` is published to a stable feed, repository restore must explicitly point at local package output. | Gate 326 |
| Open | NuGet publication, signed package restore, generated workflow integration, and consumer-project manifest generation remain future decisions. | Gate 327 boundary |

Current external references:

- `https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use`
- `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install`
- `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-restore`

## Implemented Manifest

Gate 327 adds `.config/dotnet-tools.json` at the repository root:

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

The manifest pins the package ID written by the .NET local-tool install smoke
and exposes only the canonical ADR-010 command `forge`.

## Restore Contract

Because `WastelandForge.Cli` is not published to a stable feed yet, restore
must be explicit about local package output:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet tool restore --add-source artifacts\local-tool\nupkg
dotnet tool run forge -- --version
dotnet tool run forge -- help
```

The stricter validation path uses a temporary NuGet config and isolated
`NUGET_PACKAGES` directory under ignored `artifacts/local-tool/restore/` so
the package is restored from the local package output rather than relying on a
user/global cache or remote feed.

## Not Implemented

Gate 327 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- root `NuGet.config`,
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
| Checked-in manifest exists | Complete | `.config/dotnet-tools.json` pins `wastelandforge.cli` version `0.1.0`. |
| Tool command remains canonical | Complete | Manifest command list contains only `forge`. |
| Restore source constraint is documented | Complete | Docs require local package output until publication is gated. |
| Local-source restore passes | Complete | Validation restores from a temporary local-only NuGet config and isolated package cache. |
| Publication remains out of scope | Complete | No NuGet publication, remote call, signing, or attestation is added. |

## Validation

Required validation:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet tool restore --tool-manifest .config\dotnet-tools.json --configfile artifacts\local-tool\restore\gate-327\NuGet.config --no-cache
dotnet tool run forge -- --version
dotnet tool run forge -- help
git diff --check
rg -n "Route the next development step to Gate 327|Gate 327: checked-in local tool manifest scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 328|Gate 328" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

The restore validation must set `NUGET_PACKAGES` to an ignored
`artifacts/local-tool/restore/` path before running `dotnet tool restore` and
`dotnet tool run` so it does not depend on the user's global package cache.

## Next Gate

Gate 328 should plan generated workflow and task bootstrap integration for
the repository-pinned local tool flow, still stopping before generated
workflow mutation, NuGet publication, provider installation, external tool
execution, MO2/GECK automation, runtime probes, real third-party plugin
fixtures, release publication, remote repository calls, signing, attestation,
plugin mutation, VS Code extension generation, language-server process
startup, or AI behavior.
