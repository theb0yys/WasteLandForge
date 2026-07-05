# Gate 324 - Forge CLI Local-Tool Package Metadata Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-010, ADR-011, Gate 323, R006/R008 tooling and CI guidance

## Goal

Plan the local-tool package metadata lane for the real Forge CLI so the
bootstrap path can advance beyond source-built repository adapters.

Gate 324 is planning only. It does not change project metadata, pack a NuGet
package, create `.config/dotnet-tools.json`, install a local tool, or publish
Forge.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | WastelandForge should provide a small, deterministic, offline-first, AI-optional CLI. | R006 / ADR-010 |
| Documented | Repository-pinned tooling through .NET local tool manifests reduces version drift and supports reproducible CI. | R006 / ADR-010 |
| Documented | CI and local bootstrap should use explicit .NET setup through `global.json`. | ADR-011 / R008 |
| Documented | .NET tools are NuGet packages, and local tools are recorded in `.config/dotnet-tools.json`. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Documented | Tool packages use project metadata such as `PackAsTool`, `ToolCommandName`, and optional `PackageOutputPath`. | Microsoft .NET CLI docs, checked 2026-07-05 |
| Inferred | The first implementation step should add package metadata to `src/WastelandForge.Cli/WastelandForge.Cli.csproj` while keeping repo-wide projects non-packable. | Current repo shape plus Gate 323 |
| Open | The long-term public package ID, NuGet publication policy, signing/attestation policy, and checked-in consumer manifest timing remain future decisions. | Gate 324 boundary |

Current external references:

- `https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create`
- `https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use`
- `https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install`

## Planned Metadata Lane

Gate 324 defines this staged lane:

1. Add local-tool package metadata to the CLI project only.
2. Pack the CLI to a local package output directory.
3. Smoke install the package from that local source into a temporary tool
   manifest outside committed project configuration.
4. Run `forge --version` and `forge help` through the installed local tool.
5. Only after local package-source behavior is proven, plan a checked-in
   repository-pinned `.config/dotnet-tools.json` flow.

The metadata scaffold should remain scoped to
`src/WastelandForge.Cli/WastelandForge.Cli.csproj`. The root
`Directory.Build.props` currently keeps projects non-packable by default with
`<IsPackable>false</IsPackable>`, so the CLI project must explicitly opt in
when the metadata gate is implemented.

## Candidate Metadata For Gate 325

Gate 325 should evaluate adding:

```xml
<IsPackable>true</IsPackable>
<PackAsTool>true</PackAsTool>
<ToolCommandName>forge</ToolCommandName>
<PackageId>WastelandForge.Cli</PackageId>
<PackageOutputPath>..\..\artifacts\local-tool\nupkg</PackageOutputPath>
```

The package command should stay `forge`, matching ADR-010. The package ID
should avoid claiming the generic `forge` package name. License metadata must
be handled carefully: the repository root license is GNU GPL v3 text, but Gate
324 does not select a NuGet license expression or publication policy.

## Planned Local Package-Source Smoke

The local package-source test lane should use temporary or artifact paths and
must not commit `.config/dotnet-tools.json` yet. A future smoke should use the
shape below, adjusted by the implementation gate:

```text
dotnet pack src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release
dotnet new tool-manifest --output <temporary-tool-root>
dotnet tool install WastelandForge.Cli --tool-manifest <temporary-tool-root>/.config/dotnet-tools.json --add-source artifacts/local-tool/nupkg --version 0.1.0 --create-manifest-if-needed=false
dotnet tool run forge --tool-manifest <temporary-tool-root>/.config/dotnet-tools.json -- --version
dotnet tool run forge --tool-manifest <temporary-tool-root>/.config/dotnet-tools.json -- help
```

The smoke should use a trusted temporary directory under the repository's test
or artifact space and should clean up only that directory.

## Not Implemented

Gate 324 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- C# project metadata changes,
- `dotnet pack`,
- local NuGet package creation,
- local tool installation,
- checked-in `.config/dotnet-tools.json`,
- NuGet publication,
- generated workflow mutation,
- workflow execution,
- standalone executable packaging changes,
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
- release publication,
- remote repository calls,
- signing or attestation,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Local-tool metadata lane is documented | Complete | This gate defines the metadata, pack, local-source smoke, and later manifest sequence. |
| Gate 325 is scoped | Complete | Gate 325 is routed to CLI local-tool package metadata scaffold. |
| `.config/dotnet-tools.json` remains future work | Complete | The local manifest is explicitly deferred until package behavior is proven. |
| No runtime behavior changes are made | Complete | This gate is docs and prompt routing only. |
| Publication remains out of scope | Complete | NuGet publication, remote calls, signing, and attestation remain future gates. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 324|Gate 324: Forge CLI local-tool package metadata planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 325|Gate 325" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

Runtime build/test is not required for Gate 324 because it does not change
source code, project metadata, package output, or runtime behavior.

## Next Gate

Gate 325 should add the Forge CLI local-tool package metadata scaffold to
`src/WastelandForge.Cli/WastelandForge.Cli.csproj`, still stopping before
checked-in `.config/dotnet-tools.json`, NuGet publication, generated workflow
mutation, provider installation, external tool execution, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing, attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI
behavior.
