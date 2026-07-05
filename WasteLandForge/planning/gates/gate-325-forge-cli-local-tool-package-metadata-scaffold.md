# Gate 325 - Forge CLI Local-Tool Package Metadata Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-010, ADR-011, Gate 324, Microsoft .NET CLI local-tool docs

## Goal

Add the first local-tool package metadata scaffold to the real Forge CLI
project and prove that a local package can install and run as `forge` from a
temporary local-tool manifest.

Gate 325 implements package metadata only. It does not check in
`.config/dotnet-tools.json`, publish a package, mutate generated workflows, or
change CLI runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | WastelandForge should provide a deterministic, offline-first, AI-optional CLI. | R006 / ADR-010 |
| Documented | Repository-pinned tooling through .NET local tool manifests reduces version drift and supports reproducible CI. | R006 / ADR-010 |
| Documented | .NET tools are NuGet packages; tool projects use metadata such as `PackAsTool`, `ToolCommandName`, and `PackageOutputPath`. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Documented | Local tools are recorded in a tool manifest and can be invoked with `dotnet tool run <command>`. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Inferred | The CLI project should explicitly opt in to packaging because the repository default keeps projects non-packable. | Gate 324 and `Directory.Build.props` |
| Open | Public NuGet publication, checked-in tool manifest timing, package license expression, signing, and attestation remain future decisions. | Gate 325 boundary |

## Implemented Package Metadata

Gate 325 updates `src/WastelandForge.Cli/WastelandForge.Cli.csproj` with:

- `IsPackable=true`,
- `PackAsTool=true`,
- `ToolCommandName=forge`,
- `PackageId=WastelandForge.Cli`,
- package authors, description, tags, and readme metadata,
- local package output under `artifacts/local-tool/nupkg`.

Gate 325 also adds `src/WastelandForge.Cli/PACKAGE-README.md` as the package
readme included in the local NuGet package.

The package ID avoids claiming the generic package name `forge`, while the
installed command remains `forge` to match ADR-010.

## Local Package-Source Smoke

Validation creates ignored local artifacts only:

- `artifacts/local-tool/nupkg/WastelandForge.Cli.0.1.0.nupkg`,
- temporary local-tool manifests and local NuGet config files under
  `artifacts/local-tool/smoke/`.

The final smoke uses a temporary manifest and a local-only NuGet config that
clears package sources before adding `artifacts/local-tool/nupkg`. It installs
`WastelandForge.Cli` version `0.1.0`, then runs:

```text
dotnet tool run forge -- --version
dotnet tool run forge -- help
```

Both commands execute the installed local tool and print the expected Forge
version/help output.

## Not Implemented

Gate 325 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- checked-in `.config/dotnet-tools.json`,
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
| CLI project opts in to tool packaging | Complete | `WastelandForge.Cli.csproj` sets `IsPackable`, `PackAsTool`, and `ToolCommandName`. |
| Tool command remains canonical | Complete | Local install reports command `forge`. |
| Package ID avoids generic command name | Complete | Package ID is `WastelandForge.Cli`. |
| Package output is local and ignored | Complete | Package writes under ignored `artifacts/local-tool/nupkg`. |
| Local package install smoke passes | Complete | `dotnet tool run forge -- --version` and `dotnet tool run forge -- help` pass from a temporary manifest. |
| Checked-in tool manifest remains future work | Complete | No committed `.config/dotnet-tools.json` is added. |

## Validation

Required validation:

```text
dotnet pack src\WastelandForge.Cli\WastelandForge.Cli.csproj -c Release
dotnet new tool-manifest --output artifacts\local-tool\smoke\gate-325-final
dotnet tool install WastelandForge.Cli --tool-manifest artifacts\local-tool\smoke\gate-325-final\dotnet-tools.json --configfile artifacts\local-tool\smoke\gate-325-final\NuGet.config --version 0.1.0 --create-manifest-if-needed=false
dotnet tool run forge -- --version
dotnet tool run forge -- help
git diff --check
rg -n "Route the next development step to Gate 325|Gate 325: Forge CLI local-tool package metadata scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 326|Gate 326" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

The first local-tool smoke attempt used the wrong manifest path shape after
`dotnet new tool-manifest --output`; the manifest was created as
`dotnet-tools.json` directly under the output directory for this SDK. The
final smoke used that manifest path and passed.

## Next Gate

Gate 326 should plan the checked-in repository-pinned `.config/dotnet-tools.json`
flow, still stopping before committing a tool manifest, NuGet publication,
generated workflow mutation, provider installation, external tool execution,
MO2/GECK automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing, attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI
behavior.
