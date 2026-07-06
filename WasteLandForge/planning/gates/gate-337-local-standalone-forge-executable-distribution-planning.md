# Gate 337 - Local Standalone Forge Executable Distribution Planning

Status: Complete
Phase: CLI bootstrap planning
Decision base: ADR-009, ADR-010, ADR-011, ADR-012, Gates 322, 334-336, R006, R008

## Goal

Plan a source-agnostic local standalone `forge.exe` distribution path that can
satisfy generated consumer-project `forge` on `PATH` prerequisites before
public package/feed governance is ready.

Gate 337 is planning only. It does not publish, package, sign, install, mutate
generated scaffolds, or change CLI runtime behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | The canonical user command remains `forge`, with editor tasks and CI as thin wrappers around the same CLI. | R006 / ADR-010 |
| Documented | Core correctness must stay offline-first and AI-optional. | R006 / ADR-011 |
| Documented | Release and build artifacts need local manifests, checksums, and governance before publication. | R008 / ADR-011 |
| Documented | Generated consumer projects remain source-agnostic and expect an existing `forge` command. | Gates 334-336 |
| Documented | `WastelandForge.exe` is the WPF app shell and uses `forge.exe` as a backend worker, but app-shell packaging has separate license/installer gates. | ADR-012 / Gates 313-314 |
| Open | Public NuGet publication, package feed selection, signing, update channel, and installer technology remain unresolved. | Gates 334-336 / ADR-012 |

## Policy Decision

The next implementation lane should create a repository-owned local standalone
Forge executable distribution scaffold for manual/offline use.

The scaffold should be source-agnostic for consumers:

- produce or stage a local `forge.exe` artifact from this repository;
- include a local build manifest;
- include SHA-256 checksums;
- include minimal command-availability instructions;
- avoid NuGet/package-source assumptions;
- avoid installer behavior;
- avoid signing and update channels;
- avoid modifying generated `forge init` scaffolds again.

The intended consumer action remains explicit and manual until later install
governance exists:

```text
put the standalone Forge folder on PATH, or call forge.exe by absolute path
```

## Planned Artifact Shape

Gate 338 should scaffold a local-only standalone distribution under an ignored
distribution tree such as:

```text
dist/local/forge/
  forge.exe
  README.txt
  build-manifest.json
  checksums.sha256
```

If the implementation uses `dotnet publish`, it should publish
`src/WastelandForge.Cli/WastelandForge.Cli.csproj` for a Windows runtime and
keep the details inside a repository-owned engineering script or task rather
than generated consumer projects.

## Difference From Existing Bootstrap Paths

| Path | Scope | Gate 337 policy |
|---|---|---|
| `eng/forge.ps1` / `eng/forge.cmd` | Source checkout only | Keep for repository development. |
| `.config/dotnet-tools.json` + `eng/Restore-ForgeTool.ps1` | Source checkout restore from local package output | Keep for repository CI/developer bootstrap. |
| Generated consumer `forge init` scaffolds | Consumer project without WastelandForge source | Keep source-agnostic and expect existing `forge` on `PATH`. |
| Standalone `forge.exe` folder | Manual/offline consumer command availability | Plan as Gate 338 scaffold. |
| `WastelandForge.exe` app shell | Premium WPF shell over backend Forge | Keep separate from CLI standalone distribution. |

## Deferred Decisions

Gate 337 keeps these open:

- public NuGet publication,
- signed restore,
- package feed/source selection,
- installer technology,
- code signing,
- update channels,
- release publication,
- app-shell bundling of `forge.exe`,
- generated consumer scaffold install helpers.

## Boundary

Gate 337 does not implement:

- executable publishing,
- archive creation,
- installer generation,
- generated `forge init` template changes,
- generated `.config/dotnet-tools.json`,
- root `NuGet.config`,
- package restore,
- NuGet publication,
- signing,
- attestation,
- update-channel logic,
- provider installation,
- external game-tool execution,
- MO2 automation,
- GECK automation,
- runtime probes,
- plugin mutation,
- release publication,
- remote repository calls,
- WPF app-shell packaging,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Standalone `forge.exe` distribution lane is planned | Complete | This gate defines a local-only `dist/local/forge/` style artifact shape. |
| Consumer source-agnostic boundary remains intact | Complete | Generated projects still expect an existing `forge` command and do not restore from source. |
| NuGet and installer policy stays deferred | Complete | Public feed, signing, installer, and update-channel decisions remain open. |
| App-shell packaging stays separate | Complete | `WastelandForge.exe` bundling remains under ADR-012/app-shell gates, not this CLI distribution lane. |
| Next implementation route is selected | Complete | Gate 338 is routed to local standalone Forge executable distribution scaffold. |
| Runtime behavior is unchanged | Complete | This gate is docs and prompt routing only. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 337|Gate 337: local standalone Forge executable distribution planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 338|Gate 338" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime build/test is not required for Gate 337 because it does not change
source code, executable packaging scripts, generated templates, project
metadata, task behavior, workflow behavior, or CLI runtime behavior.

## Next Gate

Gate 338 should scaffold local standalone Forge executable distribution for the
source repository, still stopping before installer creation, update-channel
logic, signing, attestation, NuGet publication, package restore, generated
local tool manifest emission, root package-source config, generated consumer
template mutation, provider installation, external tool execution, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, app-shell packaging, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
