# Gate 340 - Premium Windows App Shell Distribution And Installer Readiness Planning

Status: Complete
Phase: app-shell distribution planning
Decision base: ADR-012, Gates 313-314, Gate 339, R006, R008

## Goal

Plan the next premium Windows app-shell distribution step after the local
standalone `forge.exe` lane closeout.

Gate 340 is planning and routing only. It does not create an installer, sign
binaries, publish releases, add an update channel, execute external game tools,
or change CLI/runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `WastelandForge.exe` is a native .NET/WPF app shell over the deterministic `forge.exe` backend. | ADR-012 |
| Documented | The app shell must not replace Forge core truth, validation, build, capability, package, or release correctness. | ADR-012 / docs/app-shell |
| Documented | The app shell may embed licensed Heat-derived resources locally, but raw Heat source assets must not be committed or redistributed. | Gate 314 / docs/app-shell/asset-license-audit.md |
| Documented | Local builds and releases need manifest and checksum evidence. | R008 / ADR-011 |
| Documented | Gate 339 closed the standalone CLI handoff and routed app-shell distribution readiness as the next value slice. | Gate 339 |
| Open | Installer technology, code signing, update channel, final Heat Restricted Asset status, contributor license coverage, and third-party attribution bundle remain unresolved. | ADR-012 / docs/app-shell |

## Distribution Options

| Option | Use now | Reason |
|---|---:|---|
| Local publish folder handoff | Yes | Already matches the current working app and preserves offline local testing. |
| Framework-dependent app publish | Yes | Smaller local artifact and aligned with the current .NET SDK/dev-machine state. |
| Self-contained app publish | Later | Useful for end-user distribution, but needs explicit runtime/size and restore policy. |
| Single-file publish | Later | Needs WPF/resource/backend extraction validation before use. |
| ZIP archive | Later | Useful once local publish evidence is stable. |
| Installer | Later | Requires installer technology decision, signing policy, and release evidence. |
| Code signing | Later | Requires certificate and trust-chain policy outside this gate. |
| Update channel | Later | Requires release/feed governance outside this gate. |

## Planned App Distribution Shape

The next safe implementation step is a repository-owned local app-shell publish
helper that writes to an ignored local distribution folder:

```text
dist/app/WastelandForge.Desktop/
  WastelandForge.exe
  ForgeBackend/forge.exe
  DemoProjects/ExampleMod/
  app-build-manifest.json
  checksums.sha256
  README.txt
```

The helper should:

- build or require the standalone backend distribution first,
- publish `src/WastelandForge.Desktop/WastelandForge.Desktop.csproj`,
- bundle `ForgeBackend/` from `dist/local/forge/`,
- bundle the synthetic demo project source only, excluding generated and dist
  outputs,
- optionally accept `HeatSourceRoot` for licensed local resource embedding,
- write local app distribution evidence,
- smoke test `WastelandForge.exe` file presence,
- smoke test bundled `ForgeBackend/forge.exe --version`,
- avoid installer creation, signing, update-channel logic, release
  publication, external tool execution, runtime probes, or AI behavior.

## Next Route

Gate 341 should scaffold the local app-shell publish helper and distribution
evidence.

The target is a script such as `eng/Publish-AppShell.ps1` that produces the
local app folder, README, manifest, and checksums. It should remain a local
developer distribution helper, not an installer or release package.

Gate 341 should stop before:

- installer creation,
- code signing,
- update-channel implementation,
- release publication,
- public archive packaging,
- Heat source asset commits,
- external tool execution,
- MO2 or GECK automation,
- runtime probes,
- AI behavior.

## Not Implemented

Gate 340 does not implement:

- source code changes,
- CLI runtime behavior changes,
- app runtime behavior changes,
- app publish script,
- installer creation,
- archive creation,
- update-channel logic,
- code signing,
- attestation,
- NuGet publication,
- package restore policy changes,
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
| App-shell distribution options are compared | Complete | Distribution options table records current and deferred choices. |
| Local publish helper is selected as the next safe step | Complete | Planned app distribution shape defines the next local artifact. |
| Installer/signing/update-channel work remains gated | Complete | Open checks and not-implemented sections keep those out of Gate 340. |
| Heat asset boundary is preserved | Complete | Planned helper may accept local `HeatSourceRoot` but must not commit raw Heat assets. |
| Next route is explicit | Complete | Gate 341 is routed to app-shell local publish helper and evidence scaffold. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 340|Gate 340: premium Windows app-shell distribution and installer readiness planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 341|Gate 341" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime build/test is not required for Gate 340 because it does not change
source code, project metadata, task behavior, workflow behavior, app runtime
behavior, or CLI runtime behavior.

## Next Gate

Gate 341 should scaffold the local app-shell publish helper and distribution
evidence, still stopping before installer creation, update-channel logic,
signing, attestation, NuGet publication, package restore policy changes,
generated consumer template mutation, provider installation, external tool
execution, MO2/GECK automation, runtime probes, real third-party plugin
fixtures, release publication, remote repository calls, plugin mutation, VS
Code extension generation, language-server process startup, or AI behavior.
