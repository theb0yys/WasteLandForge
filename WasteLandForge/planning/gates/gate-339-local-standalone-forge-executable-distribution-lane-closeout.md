# Gate 339 - Local Standalone Forge Executable Distribution Lane Closeout

Status: Complete
Phase: CLI bootstrap closeout and next-value routing
Decision base: ADR-009, ADR-010, ADR-011, ADR-012, Gates 337-338, R006, R008

## Goal

Close the local standalone `forge.exe` distribution lane and route the next
value slice.

Gate 339 is documentation and prompt routing only. It does not change CLI
runtime behavior, installer behavior, package publication, generated consumer
templates, provider detection, external tool execution, app-shell packaging, or
AI behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | The canonical command remains `forge`; wrappers and app shells must not invent aliases outside ADR-010. | R006 / ADR-010 |
| Documented | Local build manifests and checksums are required evidence for generated and distribution outputs. | R008 / ADR-011 |
| Documented | Gate 337 selected a local standalone executable handoff lane before installer, signing, update channel, or public package governance exists. | Gate 337 |
| Documented | Gate 338 scaffolded the source-repository local standalone executable distribution through `eng/Publish-StandaloneForge.ps1`. | Gate 338 |
| Documented | `WastelandForge.exe` is a separate WPF app-shell lane over `forge.exe`, with installer, signing, update channel, and attribution checks still open. | ADR-012 |
| Open | Public installer technology, code signing, update channel, and app-shell release packaging remain unresolved. | ADR-012 / docs/app-shell |

## Closeout

The local standalone executable lane now covers:

- source-repository publish helper `eng/Publish-StandaloneForge.ps1`,
- ignored local distribution folder `dist/local/forge/`,
- framework-dependent `forge.exe` handoff for manual local use,
- generated distribution README text,
- generated `build-manifest.json`,
- generated `checksums.sha256`,
- source-repository VS Code task `Forge: Publish Standalone`,
- prompt and documentation routing for standalone command availability.

That is enough for the pre-installer local CLI handoff. Further work in this
lane should reopen only for a documented product gap such as self-contained
runtime policy, signed executable verification, archive packaging, or real
installer distribution.

## Next Route

Gate 340 should plan premium Windows app-shell distribution and installer
readiness.

The target is to decide how `WastelandForge.exe` should be distributed as a
proper Windows application while preserving ADR-012's boundary that the app
shell remains a presentation/orchestration layer over `forge.exe`.

Gate 340 should compare:

- local publish folder handoff,
- single-folder or self-contained .NET desktop publish,
- installer technology,
- code-signing requirements,
- update-channel policy,
- bundled backend `ForgeBackend/` evidence,
- bundled demo-project source boundary,
- Heat attribution and restricted-asset checks,
- CI coverage for app publish without making Heat mandatory.

Gate 340 should stay planning-only and stop before installer creation, signing,
update-channel implementation, release publication, or external tool execution.

## Not Implemented

Gate 339 does not implement:

- CLI runtime behavior changes,
- new command names or aliases,
- installer creation,
- app-shell packaging,
- archive creation,
- update-channel logic,
- code signing,
- attestation,
- NuGet publication,
- package restore,
- generated local tool manifest emission,
- root package-source config,
- generated consumer template mutation,
- provider installation,
- external tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
- runtime probes,
- real third-party plugin fixtures,
- release publication,
- remote repository calls,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Local standalone executable lane is explicitly closed | Complete | This gate documents the closeout boundary. |
| Existing standalone artifacts are listed | Complete | Closeout section records publish helper, distribution folder, manifest, checksums, README, and VS Code task. |
| App-shell distribution gap remains explicit | Complete | Next route is Gate 340 app-shell distribution and installer readiness planning. |
| Canonical command surface is preserved | Complete | No new command or alias is introduced. |
| Runtime behavior is unchanged | Complete | This gate is docs and prompt routing only. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 339|Gate 339: local standalone Forge executable distribution lane closeout" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 340|Gate 340" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime build/test is not required for Gate 339 because it does not change
source code, project metadata, task behavior, workflow behavior, or CLI
runtime behavior.

## Next Gate

Gate 340 should plan premium Windows app-shell distribution and installer
readiness, still stopping before installer creation, update-channel logic,
signing, attestation, NuGet publication, package restore, generated consumer
template mutation, provider installation, external tool execution, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, plugin mutation, VS Code extension
generation, language-server process startup, or AI behavior.
