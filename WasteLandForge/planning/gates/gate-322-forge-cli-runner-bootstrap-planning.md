# Gate 322 - Forge CLI Runner Bootstrap Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-010, ADR-011, Gate 321, R006/R008 tooling and CI guidance

## Goal

Define the bootstrap path for obtaining a known `forge` command locally and in
CI before generated tasks and workflows run.

Gate 322 is planning only. It does not install Forge, publish a package, or
change runtime command behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and GitHub Actions should be thin wrappers around the real `forge` CLI. | R006 / ADR-010 |
| Documented | Repository-pinned tooling through `.config/dotnet-tools.json` reduces version drift and supports reproducible CI. | Supplemental DX report |
| Documented | CI should use explicit .NET setup through `global.json` rather than relying on hosted runner preinstalls. | ADR-011 / R008 |
| Documented | Forge correctness must remain offline-first and AI-optional. | ADR-010 / ADR-011 |
| Inferred | Until a packaged local tool exists, a source-built runner shim is the safest first bootstrap artifact for this repository. | Current repo shape plus R006/R008 |
| Open | The long-term consumer-project install path may be .NET local tool package, standalone executable artifact, source-built fallback, or a staged combination. | Gate 322 boundary |

## Bootstrap Model

Gate 322 defines a staged bootstrap model:

1. Source-built runner shim for the WastelandForge repository.
2. Local tool package metadata and local package-source testing.
3. Repository-pinned `.config/dotnet-tools.json` flow for consumer projects.
4. Generated workflow/task integration once a stable restore path exists.
5. Optional standalone executable artifact flow for manual/offline testing.

The immediate path starts with the source-built runner shim because the repo
already contains `src/WastelandForge.Cli/WastelandForge.Cli.csproj` and
`global.json`, while no NuGet/local-tool package publication lane exists yet.

## Planned Bootstrap Contract

The bootstrap contract should preserve these boundaries:

- a canonical command remains `forge`, not a new alias,
- wrapper scripts are implementation adapters and should pass through the
  ADR-010 command surface unchanged,
- source-built bootstrap should use `dotnet` and `global.json`,
- generated workflows should not assume hosted runner SDK state,
- local-tool restore may be added only when package metadata and package source
  behavior are explicitly gated,
- core validation/build correctness must not require network access,
- no provider, MO2, GECK, xEdit, runtime probe, package publish, signing,
  attestation, or AI step belongs in bootstrap.

## Not Implemented

Gate 322 does not implement:

- runner scripts,
- CLI runtime behavior changes,
- new command names or aliases,
- `forge init` scaffold changes,
- `.config/dotnet-tools.json`,
- .NET local tool package metadata,
- local NuGet package creation,
- NuGet publication,
- workflow bootstrap steps,
- workflow execution,
- GitHub remote calls,
- standalone executable packaging beyond existing local builds,
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
- signing or attestation,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Bootstrap problem is documented | Complete | This gate defines the `forge` command availability gap. |
| Source-built runner shim is selected as first concrete step | Complete | Gate 323 is routed to runner shim scaffold. |
| Local-tool flow remains planned, not assumed | Complete | Package metadata and tool manifest remain future gates. |
| Generated workflow changes are deferred | Complete | Workflow bootstrap integration waits for a stable restore path. |
| No runtime behavior changes are made | Complete | This gate is docs/prompt routing only. |

## Validation

Required validation:

```text
git diff --check
rg -n "next .*Gate 322|route .*Gate 322|Route the next .*Gate 322" docs WasteLandForge/planning/README.md WasteLandForge/skills WasteLandForge/agents .agents src tests
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests
```

Runtime build/test is not required for Gate 322 because it does not change
source code or runtime behavior.

## Next Gate

Gate 323 should add a source-built Forge runner shim scaffold for the
WastelandForge repository, still stopping before package publication,
`.config/dotnet-tools.json`, local tool package creation, network dependency
for core correctness, generated workflow mutation, provider installation,
external tool execution, MO2/GECK automation, runtime probes, real third-party
plugin fixtures, release publication, remote repository calls, signing,
attestation, plugin mutation, VS Code extension generation, language-server
process startup, or AI behavior.
