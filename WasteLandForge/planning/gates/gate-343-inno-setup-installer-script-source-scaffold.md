# Gate 343 - Inno Setup Installer Script Source Scaffold

Status: Complete
Phase: app-shell installer source scaffold
Decision base: ADR-011, ADR-012, Gates 341-342, R008

## Goal

Scaffold the source metadata for a future WastelandForge Windows installer
without building a setup executable.

Gate 343 adds the Inno Setup script source and a local installer-input
preflight. It does not run Inno Setup, create an installer executable, sign
binaries, timestamp artifacts, publish releases, add an update channel, execute
external game tools, run runtime probes, mutate generated consumer templates,
commit Heat source assets, or require AI.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `WastelandForge.exe` is a native .NET/WPF app shell over bundled `forge.exe`. | ADR-012 |
| Documented | App-shell distribution must preserve the backend boundary and keep core correctness in Forge. | ADR-012 / docs/app-shell |
| Documented | Local distribution outputs need build-manifest and checksum evidence. | R008 / ADR-011 |
| Documented | Gate 341 local app-shell output writes `app-build-manifest.json` and `checksums.sha256`. | Gate 341 |
| Documented | Gate 342 selected Inno Setup script scaffolding as the first installer lane and stopped before installer creation. | Gate 342 |
| Documented | Inno Setup scripts describe setup metadata, files, tasks, and icons in source form. | Inno Setup documentation |
| Open | Code signing certificate, timestamping, update channel, public release package policy, and final Heat attribution bundle remain unresolved. | ADR-012 / docs/app-shell |

## Implementation

Gate 343 adds:

- `installer/inno/WastelandForge.iss`
- `installer/inno/README.md`
- `eng/Test-AppShellInstallerInputs.ps1`
- documentation and prompt-route updates for the app-shell installer lane

The `.iss` source defaults to:

```text
dist/app/WastelandForge.Desktop/
```

as the app-shell input folder and uses:

```text
artifacts/installer/inno/
```

as the future ignored installer output directory if a later gate invokes the
Inno compiler.

The script source installs the app publish folder under a WastelandForge app
directory and defines Start Menu shortcut metadata plus an optional Desktop
shortcut task. It does not embed raw Heat source assets and does not add update
or signing behavior.

## Input Preflight

`eng/Test-AppShellInstallerInputs.ps1` validates app-shell installer inputs
without invoking Inno Setup.

Allowed input roots are:

```text
dist/app/WastelandForge.Desktop
artifacts/app-shell/<name>
```

The preflight requires:

- `WastelandForge.exe`
- `ForgeBackend/forge.exe`
- `DemoProjects/ExampleMod/wastelandforge.json`
- `app-build-manifest.json`
- `checksums.sha256`

It also checks:

- app manifest schema and kind,
- manifest boundary flags for no installer, no signing, no update channel, no
  release publication, and no AI requirement,
- required checksum sidecar entries,
- SHA-256 digest matches for required installer inputs.

## Boundary

Gate 343 does not implement:

- Inno Setup compiler execution,
- setup executable creation,
- installer smoke execution,
- public archive packaging,
- update-channel logic,
- code signing,
- timestamping,
- attestation,
- release publication,
- NuGet publication,
- package restore policy changes,
- generated consumer template mutation,
- provider installation,
- external game-tool execution,
- MO2 automation,
- GECK automation,
- xEdit execution,
- runtime probes,
- real third-party plugin fixtures,
- Heat source asset commits,
- remote repository calls,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Inno Setup source scaffold exists | Complete | `installer/inno/WastelandForge.iss` records setup metadata, file input, and shortcut metadata. |
| Installer input validation exists | Complete | `eng/Test-AppShellInstallerInputs.ps1` validates required files, manifest boundary flags, and required checksums. |
| Installer build remains gated | Complete | No Inno compiler invocation or setup executable output is added. |
| Evidence boundary is preserved | Complete | The preflight requires `app-build-manifest.json` and `checksums.sha256`. |
| Heat boundary is preserved | Complete | The installer source consumes only the local app publish folder and does not commit raw Heat assets. |

## Validation

Required validation:

```text
PowerShell parser check for eng/Test-AppShellInstallerInputs.ps1
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1 -AppShellRoot artifacts/app-shell/gate-341
rg -n "Gate 343|Gate 344|Inno Setup installer script source scaffold" installer docs eng WasteLandForge/planning/README.md WasteLandForge/skills WasteLandForge/agents .agents
git diff --check
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime .NET build/test is not required for Gate 343 because it does not change
.NET source code, project metadata, CLI runtime behavior, app runtime behavior,
workflow behavior, generated consumer templates, package restore behavior, or
release behavior.

## Next Gate

Gate 344 should add local Inno Setup compiler detection and an unsigned local
installer build helper scaffold, still stopping before signing, timestamping,
attestation, update-channel logic, release publication, NuGet publication,
package restore policy changes, generated consumer template mutation, provider
installation, external tool execution, MO2/GECK automation, runtime probes,
real third-party plugin fixtures, Heat source asset commits, remote repository
calls, plugin mutation, VS Code extension generation, language-server process
startup, or AI behavior.
