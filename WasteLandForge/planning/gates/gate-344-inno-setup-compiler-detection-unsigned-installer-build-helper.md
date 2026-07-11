# Gate 344 - Inno Setup Compiler Detection And Unsigned Installer Build Helper

Status: Complete
Phase: app-shell installer local build helper
Decision base: ADR-011, ADR-012, Gates 341-343, R008

## Goal

Add local Inno Setup compiler detection and an unsigned local installer build
helper scaffold while preserving the app-shell evidence and release boundaries.

Gate 344 adds a repository-owned helper that can detect `ISCC.exe`, validate
the app-shell input folder, and build an unsigned local installer under
ignored `artifacts/` when the compiler is available. It does not install Inno
Setup, sign binaries, timestamp artifacts, publish releases, add an update
channel, execute game tools, run runtime probes, mutate generated consumer
templates, commit Heat source assets, or require AI.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `WastelandForge.exe` is a native .NET/WPF app shell over bundled `forge.exe`. | ADR-012 |
| Documented | App-shell correctness must remain in Forge core and CLI JSON contracts. | ADR-012 / docs/app-shell |
| Documented | Local distribution outputs need build-manifest and checksum evidence. | R008 / ADR-011 |
| Documented | Gate 341 writes app-shell local manifest and checksum evidence. | Gate 341 |
| Documented | Gate 343 adds the Inno Setup source script and input preflight while stopping before compiler execution. | Gate 343 |
| Documented | Gate 342 selected Inno Setup as the first installer lane and deferred MSIX/WiX. | Gate 342 |
| Open | Code signing certificate, timestamping, update channel, public release package policy, and final Heat attribution bundle remain unresolved. | ADR-012 / docs/app-shell |

## Implementation

Gate 344 adds:

- `eng/Build-AppShellInstaller.ps1`
- source-repository VS Code task `Forge: Build App Installer`
- documentation and prompt-route updates for the local installer helper lane

The helper:

- validates app-shell installer inputs through
  `eng/Test-AppShellInstallerInputs.ps1`,
- accepts an explicit `-InnoCompilerPath`,
- otherwise detects `iscc.exe` / `iscc` on `PATH`,
- checks standard machine and per-user Inno Setup 5/6 install locations,
- supports `-DetectOnly` for non-mutating detection and validation,
- writes only under `artifacts/installer/inno/<name>`,
- refuses installer scripts outside `installer/inno/`,
- refuses output roots outside `artifacts/installer/inno/<name>`,
- builds only an unsigned local installer if the compiler is available,
- writes `installer-build-manifest.json`, `checksums.sha256`, and
  `README.txt` beside a successful local installer output.

The default app-shell input is:

```text
dist/app/WastelandForge.Desktop
```

The default local installer output root is:

```text
artifacts/installer/inno/local
```

## Current Local Compiler Status

Initial validation on this machine did not find Inno Setup compiler on `PATH`
or under standard Inno Setup 5/6 installation folders.

After maintainer request, Inno Setup 6.7.3 was installed through `winget` under
the per-user install path:

```text
C:\Users\kane0\AppData\Local\Programs\Inno Setup 6\ISCC.exe
```

The helper now detects that per-user compiler path and can produce the local
unsigned setup executable in this environment. This does not make the output a
signed release artifact.

## Boundary

Gate 344 does not implement:

- Inno Setup installation,
- signed installer creation,
- timestamping,
- attestation,
- update-channel logic,
- release publication,
- public archive packaging,
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

If `ISCC.exe` is available, the helper may produce a local unsigned installer
under ignored `artifacts/installer/inno/<name>`. That local artifact is not a
signed release artifact and must not be treated as release publication.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Compiler detection exists | Complete | `eng/Build-AppShellInstaller.ps1 -DetectOnly` checks explicit path, PATH, and standard machine/per-user Inno Setup install locations. |
| Installer input preflight is reused | Complete | The helper runs `eng/Test-AppShellInstallerInputs.ps1` before detection/build. |
| Output containment exists | Complete | The helper writes only under `artifacts/installer/inno/<name>`. |
| Unsigned build helper exists | Complete | The helper can invoke `ISCC.exe` and emit manifest/checksum evidence when the compiler is available. |
| Missing compiler is honest | Complete | Missing compiler cases still report no-build status; current local validation now finds the per-user compiler. |
| Release boundaries remain gated | Complete | Signing, timestamping, update channel, release publication, and AI remain disabled. |

## Validation

Required validation:

```text
PowerShell parser check for eng/Build-AppShellInstaller.ps1
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-AppShellInstaller.ps1 -DetectOnly
Get-Content .vscode/tasks.json | ConvertFrom-Json
rg -n "Gate 344|Gate 345|Build-AppShellInstaller|Forge: Build App Installer" installer docs eng WasteLandForge/planning/README.md WasteLandForge/skills WasteLandForge/agents .agents .vscode/tasks.json
git diff --check
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime .NET build/test is not required for Gate 344 because it does not change
.NET source code, project metadata, CLI runtime behavior, app runtime behavior,
generated consumer templates, package restore behavior, or release behavior.

## Next Gate

Gate 345 should close the current installer helper lane and route back to the
next app-shell value slice unless Inno Setup is installed and maintainers
explicitly choose to validate a local unsigned installer build on this machine.
That closeout should still stop before signing, timestamping, attestation,
update-channel logic, release publication, NuGet publication, package restore
policy changes, generated consumer template mutation, external game-tool
execution, MO2/GECK automation, runtime probes, real third-party plugin
fixtures, Heat source asset commits, remote repository calls, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
