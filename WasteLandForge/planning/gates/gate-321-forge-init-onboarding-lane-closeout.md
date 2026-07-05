# Gate 321 - Forge Init Onboarding Lane Closeout

Status: Complete
Phase: CLI onboarding closeout and next-value routing
Decision base: ADR-010, ADR-011, Gate 320, R006/R008 tooling and CI guidance

## Goal

Close the current `forge init` onboarding lane and route the next development
slice toward the highest-value gap left by the scaffolded workflow and editor
integration.

Gate 321 is documentation and prompt routing only. It does not change CLI
runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge init` is part of the stable ADR-010 command surface and should create a usable starter project. | ADR-010 / R006 |
| Documented | Git hooks, editor tasks, and GitHub Actions should be thin wrappers around the real `forge` CLI. | R006 / ADR-010 |
| Documented | Repository-pinned tooling through .NET local tool manifests reduces version drift and supports reproducible CI. | Supplemental DX report |
| Documented | CI should use explicit .NET setup and avoid relying on hosted runner preinstalls. | ADR-011 / R008 |
| Inferred | After Gate 320, the main onboarding blocker is not another scaffold file; it is making the assumed `forge` runner/bootstrap story explicit for local and CI use. | Gates 319-320 plus R006/R008 |
| Open | The exact delivery shape for Forge bootstrap remains unresolved: local tool package, standalone executable artifact, source-built runner, or a staged combination. | Gate 321 boundary |

## Closeout

The current `forge init` onboarding lane now covers:

- project scaffold planning and refusal behavior,
- minimal manifest and dependency/capability registry emission,
- repo-local Forge config and README scaffold content,
- VS Code task and problem-matcher scaffold content,
- GitHub Actions validation workflow scaffold content,
- VS Code schema association scaffold content,
- golden CLI coverage, dry-run no-write coverage, existing-path refusal
  coverage, and validation smoke coverage.

That is enough to stop adding `forge init` scaffold files by default. Further
init behavior should be reopened only for a clearly documented product gap,
such as overwrite policy, template variants, interactive prompts, or local
tool/bootstrap integration.

## Next Route

Gate 322 should start a Forge CLI runner/bootstrap planning lane.

The target is to define how a WastelandForge project obtains a known `forge`
command locally and in CI before the generated tasks/workflow run. The first
step should stay planning-only and should evaluate:

- repository-pinned .NET local tool manifest flow,
- standalone executable artifact flow,
- source-built CLI fallback flow,
- generated workflow integration points,
- local developer command hints,
- version pinning and upgrade policy,
- no network or package-publish requirement for core correctness.

## Not Implemented

Gate 321 does not implement:

- CLI runtime changes,
- new command names or aliases,
- new `forge init` scaffold files,
- Forge runner installation or provisioning,
- .NET local tool package creation,
- NuGet publication,
- standalone executable packaging beyond existing local builds,
- workflow execution,
- GitHub remote calls,
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
| `forge init` lane is explicitly closed | Complete | This gate documents the closeout boundary. |
| Next value route is explicit | Complete | Gate 322 is routed to Forge CLI runner/bootstrap planning. |
| No command aliases are introduced | Complete | Routing keeps the ADR-010 command surface unchanged. |
| No runtime behavior changes are made | Complete | This gate is docs/prompt routing only. |
| Runner/provisioning remains future work | Complete | Gate 322 is planning-only and does not provision runners yet. |

## Validation

Required validation:

```text
git diff --check
rg -n "next .*Gate 321|route .*Gate 321|Route the next .*Gate 321" docs WasteLandForge/planning/README.md WasteLandForge/skills WasteLandForge/agents .agents src tests
rg -n "Tales from the Age of Men|Tales From The Age Of Men|Age of Men|overhaul" .agents WasteLandForge docs src tests
```

Runtime build/test is not required for Gate 321 because it does not change
source code or runtime behavior.

## Next Gate

Gate 322 should begin Forge CLI runner/bootstrap planning, still stopping
before package publication, runner provisioning, network dependency for core
correctness, external tool execution, MO2/GECK automation, runtime probes,
real third-party plugin fixtures, release publication, remote repository
calls, signing, attestation, plugin mutation, VS Code extension generation,
language-server process startup, or AI behavior.
