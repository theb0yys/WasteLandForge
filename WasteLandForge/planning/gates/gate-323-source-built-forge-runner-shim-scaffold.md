# Gate 323 - Source-Built Forge Runner Shim Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-010, ADR-011, Gate 322, R006/R008 tooling and CI guidance

## Goal

Add a source-built repository-local adapter so developers working in the
WastelandForge repository can invoke the real CLI through a stable script path
before local-tool packaging exists.

Gate 323 implements only the source-built runner shim. It does not install
Forge, publish a package, create a tool manifest, or change runtime command
behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and GitHub Actions should be thin wrappers around the real `forge` CLI. | R006 / ADR-010 |
| Documented | CI and local bootstrap should use explicit .NET setup through `global.json`. | ADR-011 / R008 |
| Documented | Forge correctness must remain offline-first and AI-optional. | ADR-010 / ADR-011 |
| Inferred | A source-built repo-local runner is the safest first concrete bootstrap artifact because the CLI project and SDK policy are already checked in. | Gate 322 |
| Open | Consumer-project installation still needs a gated local-tool package and repository-pinned manifest path. | Gate 322 boundary |

## Implemented Bootstrap Contract

Gate 323 adds:

- `eng/forge.ps1`, a PowerShell source-built runner that resolves the
  repository root from the script location, verifies the CLI project exists,
  verifies `dotnet` is available, runs
  `src/WastelandForge.Cli/WastelandForge.Cli.csproj` with `dotnet run`, and
  passes Forge arguments through unchanged.
- `eng/forge.cmd`, a Windows command adapter that invokes `eng/forge.ps1`
  through `pwsh` when available and Windows PowerShell otherwise.
- documentation for the local runner under `eng/README.md`.

The canonical command surface remains ADR-010 `forge`. The scripts are
implementation adapters for source-built development and do not introduce a
new Forge command.

## Not Implemented

Gate 323 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- `.config/dotnet-tools.json`,
- .NET local tool package metadata,
- local NuGet package creation,
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
| Source-built runner exists | Complete | `eng/forge.ps1` invokes the checked-in CLI project through `dotnet run`. |
| Windows adapter exists | Complete | `eng/forge.cmd` delegates to the PowerShell runner and preserves exit code. |
| Arguments pass through unchanged | Complete | `--version` and `help` smoke checks invoke the real CLI. |
| SDK policy remains repository-owned | Complete | Runner executes from repository root so `global.json` applies. |
| Local-tool packaging remains future work | Complete | No tool manifest, package metadata, or package source was added. |

## Validation

Required validation:

```text
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng\forge.ps1 --version
cmd /c eng\forge.cmd --version
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng\forge.ps1 help
git diff --check
rg -n "Route the next development step to Gate 323|Gate 323: source-built Forge runner shim scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 324|Gate 324" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng
```

Full runtime suite execution is optional for Gate 323 because the gate adds
scripts and documentation only. The runner smoke checks cover the new
execution path.

## Next Gate

Gate 324 should plan the Forge CLI local-tool package metadata and local
package-source test lane, still stopping before `.config/dotnet-tools.json`,
NuGet publication, generated workflow mutation, provider installation,
external tool execution, MO2/GECK automation, runtime probes, real
third-party plugin fixtures, release publication, remote repository calls,
signing, attestation, plugin mutation, VS Code extension generation,
language-server process startup, or AI behavior.
