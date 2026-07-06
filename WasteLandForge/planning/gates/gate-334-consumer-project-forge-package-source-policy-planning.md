# Gate 334 - Consumer-Project Forge Package-Source Policy Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-010, ADR-011, Gates 328, 333, R006, R008

## Goal

Plan the package-source policy for consumer projects created by `forge init`.
The policy must explain how generated projects obtain a known `forge` command
without assuming the WastelandForge source repository exists.

Gate 334 is planning only. It does not mutate generated task or workflow
templates, publish a NuGet package, add a root `NuGet.config`, create a
consumer `.config/dotnet-tools.json`, change CLI runtime behavior, or execute
external tools.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Git hooks, editor tasks, and CI should be thin wrappers around the stable `forge` CLI. | R006 / ADR-010 |
| Documented | Local correctness must stay offline-first and AI-optional. | R008 / ADR-011 |
| Documented | Repository-owned bootstrap can restore the checked-in local tool from source-repository package output. | Gates 329-333 |
| Inferred | Generated consumer projects cannot safely restore `WastelandForge.Cli` from source because they do not contain `src/WastelandForge.Cli`. | Gate 328 |
| Open | Public package publication, signed restore, package feed selection, and standalone executable distribution policy remain unresolved. | Gate 324 / Gate 333 |

## Policy Decision

For the current implementation lane, generated consumer projects should remain
source-agnostic and invoke `forge` as an existing user-provided command.

Generated consumer projects should not yet emit:

- `.config/dotnet-tools.json`,
- root `NuGet.config`,
- local package-source restore commands,
- `eng/Restore-ForgeTool.ps1`,
- `dotnet pack src/WastelandForge.Cli/...`,
- hidden NuGet restore from an unpublished package,
- standalone executable download or install steps.

This keeps generated projects honest: they can validate, build reports, and run
release dry-runs when `forge` is installed or otherwise available, but they do
not pretend to know a package source that has not been governed.

## Compared Options

| Option | Current policy | Reason |
|---|---|---|
| Published NuGet local tool | Defer | No publication, signing, or feed governance is gated yet. |
| Explicit package-source config | Defer | A generated root `NuGet.config` would commit a feed/source policy before governance exists. |
| Standalone executable distribution | Defer | Useful later, but installer/checksum/update policy is not selected. |
| Source-repository restore helper | Reject for consumer scaffolds | Consumer projects do not contain WastelandForge source. |
| Existing `forge` on `PATH` | Keep | It is source-agnostic and matches current generated workflow/task assumptions. |

## Planned Follow-Up

Gate 335 should improve generated consumer-project command availability
guidance without adding restore behavior. The implementation should update the
generated `forge init` README, VS Code tasks, and GitHub Actions workflow
messages so they clearly say that `forge` must already be available on `PATH`
or through the user's chosen installation method.

Gate 335 may mutate generated consumer task/workflow templates for clearer
checks and instructions, but it must still not add `.config/dotnet-tools.json`,
root `NuGet.config`, package restore, package publication, provider
installation, external tool execution, runtime probes, signing, attestation,
or AI behavior.

## Not Implemented

Gate 334 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- generated `.vscode/tasks.json` mutation,
- generated `.github/workflows/wastelandforge.yml` mutation,
- generated README mutation,
- generated `.config/dotnet-tools.json`,
- root `NuGet.config`,
- NuGet publication,
- package signing,
- package attestation,
- standalone executable distribution,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
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
| Consumer-project package-source policy is explicit | Complete | This gate keeps generated projects source-agnostic until publication/feed policy is gated. |
| Source-repository restore is kept out of consumer scaffolds | Complete | Policy rejects `dotnet pack src/WastelandForge.Cli/...` and `eng/Restore-ForgeTool.ps1` for generated projects. |
| Package-source governance gap remains visible | Complete | NuGet publication, root `NuGet.config`, signing, and standalone distribution remain deferred. |
| Next implementation slice is routed | Complete | Gate 335 is routed to generated command availability guidance. |
| Runtime behavior is unchanged | Complete | This gate is docs and prompt routing only. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 334|Gate 334: consumer-project Forge package-source policy planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 335|Gate 335" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests eng .github .vscode
```

Runtime build/test is not required for Gate 334 because it does not change
source code, generated templates, project metadata, task behavior, workflow
behavior, or CLI runtime behavior.

## Next Gate

Gate 335 should update generated consumer-project Forge command availability
guidance in `forge init` scaffolds, still stopping before generated local tool
manifest emission, root `NuGet.config`, NuGet publication, package restore,
provider installation, external tool execution, MO2/GECK automation, runtime
probes, real third-party plugin fixtures, release publication, remote
repository calls, signing or attestation, plugin mutation, VS Code extension
generation, language-server process startup, or AI behavior.
