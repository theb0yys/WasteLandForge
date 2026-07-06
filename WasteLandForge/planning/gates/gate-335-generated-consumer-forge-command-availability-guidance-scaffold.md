# Gate 335 - Generated Consumer-Project Forge Command Availability Guidance Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-009, ADR-010, ADR-011, Gate 334, R006, R008

## Goal

Make generated `forge init` consumer-project scaffolds honest about the local
Forge command prerequisite. The scaffold should clearly check and document that
`forge` must already be available through the user's chosen installation method
before local tasks or generated CI workflows run.

Gate 335 may mutate generated README, VS Code task, and GitHub Actions workflow
templates. It must not add package restore behavior or decide package-source,
publication, signing, or standalone distribution policy.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and GitHub Actions should be thin wrappers around the same stable `forge` commands. | R006 / ADR-010 |
| Documented | CI should use GitHub Actions with a mandatory Windows lane and local deterministic validation. | R008 / ADR-011 |
| Documented | Generated consumer projects remain source-agnostic and use an existing `forge` command until package/feed governance is gated. | Gate 334 |
| Inferred | A generated task can check `forge --version` before running validation/build commands without introducing package restore behavior. | R006 / Gate 334 |
| Open | Public package publication, signed restore, package feed selection, and standalone executable distribution policy remain unresolved. | Gate 334 |

## Implemented

`forge init` generated scaffolds now include:

- README guidance that generated tasks and workflows expect `forge` on `PATH`;
- README guidance that the scaffold does not restore Forge, publish packages,
  add `NuGet.config`, or assume the WastelandForge source repository exists;
- a VS Code `Forge: Check Command` task that runs `forge --version`;
- generated validate, capability scan, and reports build tasks that depend on
  `Forge: Check Command`;
- GitHub Actions verification messages that name the missing command, explain
  `FORGE_COMMAND`, and state that the generated workflow does not restore Forge
  or build `WastelandForge.Cli`;
- golden test coverage proving the generated scaffold includes the check and
  does not emit source-repository restore commands in the generated workflow.

## Boundary

Gate 335 does not implement:

- generated `.config/dotnet-tools.json`,
- root `NuGet.config`,
- package restore,
- NuGet publication,
- standalone executable download or install,
- package signing,
- package attestation,
- provider installation,
- external game-tool execution,
- MO2 automation,
- GECK automation,
- runtime probes,
- plugin mutation,
- release publication,
- remote repository calls,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Generated README documents the command prerequisite | Complete | `CreateReadme` records that generated tasks/workflows expect `forge` on `PATH`. |
| Generated VS Code tasks perform a source-agnostic command check | Complete | `.vscode/tasks.json` includes `Forge: Check Command` running `forge --version`. |
| Generated task commands stay thin wrappers around canonical Forge commands | Complete | Existing validate, capabilities scan, and reports build tasks depend on the check and still call `forge`. |
| Generated CI failure message is actionable | Complete | Workflow checks `FORGE_COMMAND` and explains that the generated workflow does not restore Forge or build source. |
| Package-source policy remains deferred | Complete | No generated restore helper, tool manifest, root package config, package packing, package restore, publication, signing, or attestation is added. |

## Validation

Required validation:

```text
dotnet build WastelandForge.sln -c Release --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --filter "FullyQualifiedName~CliGoldenTests.Init"
git diff --check
rg -n "Route the next development step to Gate 335|Gate 335: generated consumer-project Forge command availability guidance scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 336|Gate 336" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
```

## Next Gate

Gate 336 should close the generated consumer-project Forge command availability
guidance lane and route to the next value slice. It should still stop before
generated local tool manifest emission, root package-source config, NuGet
publication, package restore, provider installation, external tool execution,
MO2/GECK automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing or attestation, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
