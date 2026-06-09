# Gate 2 - Solution and SDK Baseline

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 1, ADR-006, ADR-007, ADR-010, ADR-011

## Gate Definition

Gate 2 creates the .NET SDK and solution baseline. It does not implement domain models, schemas, validation logic, CLI commands, fixtures, or CI workflows.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | WastelandForge uses a stable offline-first CLI and thin wrappers around the CLI for CI/editor/hook integration. | R006 / ADR-010 |
| Documented | The first implementation spine starts with repository structure, solution/project layout, ADR files, schema package skeleton, core C# models, CLI skeleton, manifest schema, validation pipeline, fixture project, and GitHub Actions baseline. | R008 / ADR-011 |
| Documented | R008 identifies the .NET target as an implementation choice and records .NET 10 LTS as the planning preference unless a required dependency blocks it. | R008 / ADR-011 |
| Inferred | Gate 2 should create buildable plain SDK test project shells without test framework package references because real test behavior is Gate 7 work. | Gate 0 |

## SDK Decision

Target SDK:

```text
.NET SDK 10.0.300
Target framework net10.0
```

Local evidence:

```text
dotnet --list-sdks
10.0.300 [C:\Program Files\dotnet\sdk]
```

The SDK is pinned in `global.json`, and the target framework is centralized in `eng/versions.props` through `Directory.Build.props`.

## Dependency Compatibility Snapshot

Package references are not introduced in Gate 2. The planned dependency set was checked for visible .NET 10 compatibility signals before accepting `net10.0` as the baseline.

| Package | Checked version | Gate 2 finding | Source |
|---|---|---|---|
| `System.CommandLine` | 2.0.8 | Targets .NET 8.0 and .NET Standard 2.0; NuGet computes `net10.0` compatibility. No Gate 2 blocker. | https://www.nuget.org/packages/System.CommandLine/ |
| `YamlDotNet` | 18.0.0 | Targets .NET 8.0, .NET Standard 2.0, and .NET Framework 4.7; NuGet marks .NET 8.0 compatible with higher frameworks. No Gate 2 blocker. | https://www.nuget.org/packages/YamlDotNet/ |
| `JsonSchema.Net` | 9.2.1 | Targets .NET 8.0 and .NET Standard 2.0; NuGet marks .NET 8.0 compatible with higher frameworks. No Gate 2 blocker. | https://www.nuget.org/packages/JsonSchema.Net/ |
| `Microsoft.NET.Test.Sdk` | 18.6.0 | Targets .NET 8.0 and is marked compatible with higher frameworks; NuGet computes `net10.0` compatibility. No Gate 2 blocker. | https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/ |
| `xunit.v3` | 3.2.2 | Targets .NET 8.0 or later; NuGet computes `net10.0` compatibility. No Gate 2 blocker. | https://www.nuget.org/packages/xunit.v3/ |

Final package versions and test framework wiring remain Gate 7 work.

## Deliverables

- `global.json`
- `Directory.Build.props`
- `eng/versions.props`
- `WastelandForge.sln`
- Source project skeletons:
  - `src/WastelandForge.Core`
  - `src/WastelandForge.Schema`
  - `src/WastelandForge.Registry`
  - `src/WastelandForge.Validation`
  - `src/WastelandForge.Generation`
  - `src/WastelandForge.Provenance`
  - `src/WastelandForge.Cli`
- Plain buildable test project skeletons:
  - `tests/WastelandForge.UnitTests`
  - `tests/WastelandForge.SchemaTests`
  - `tests/WastelandForge.SemanticTests`
  - `tests/WastelandForge.GoldenTests`
  - `tests/WastelandForge.WindowsTests`
  - `tests/WastelandForge.BackCompatTests`

## Project Boundary

Project references establish only the coarse implementation architecture:

- `Schema` depends on `Core`.
- `Registry` depends on `Core` and `Schema`.
- `Validation` depends on `Core`, `Schema`, and `Registry`.
- `Provenance` depends on `Core`.
- `Generation` depends on `Core`, `Registry`, and `Provenance`.
- `Cli` depends on all source projects.
- Test shells reference the relevant source projects but do not yet use a test framework.

## Verification

Command:

```text
dotnet build WastelandForge.sln -c Release
```

Result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

## Out of Scope

Gate 2 does not add:

- third-party package references,
- domain model code,
- schema files,
- validation logic,
- CLI command handlers,
- test framework dependencies,
- CI workflows.

## Next Gate

Gate 3 creates the schema package skeleton:

1. schema directories,
2. immutable `$id` convention,
3. manifest schema stub,
4. local schema resolver shape,
5. schema version policy documentation.
