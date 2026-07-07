# Gate 342 - Windows Installer Technology Planning

Status: Complete
Phase: app-shell installer planning
Decision base: ADR-011, ADR-012, Gates 313-314, Gates 340-341, R008

## Goal

Plan the Windows installer technology route for the premium WPF app shell after
the local app-shell publish helper.

Gate 342 is planning and routing only. It does not create an installer, add an
installer project, sign binaries, publish releases, add an update channel,
execute external game tools, run runtime probes, or change app/runtime
behavior.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `WastelandForge.exe` is a native .NET/WPF app shell over bundled `forge.exe`. | ADR-012 |
| Documented | App-shell distribution must preserve the backend boundary and not replace Forge core correctness. | ADR-012 / docs/app-shell |
| Documented | Local distribution outputs need build-manifest and checksum evidence. | R008 / ADR-011 |
| Documented | Gate 341 produces a local app-shell folder with `WastelandForge.exe`, `ForgeBackend/`, demo source, manifest, checksums, and README. | Gate 341 |
| Documented | .NET supports framework-dependent, self-contained, and single-file deployment models. | [Microsoft .NET deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/) / [single-file deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview) |
| Documented | MSIX is the modern Windows app package format with package identity, clean install/uninstall, and update support. | [Microsoft MSIX overview](https://learn.microsoft.com/en-us/windows/msix/overview) |
| Documented | MSIX packages must be signed before installation. | [Microsoft MSIX signing overview](https://learn.microsoft.com/en-us/windows/msix/package/signing-package-overview) |
| Documented | WiX builds Windows Installer packages and supports command-line, MSBuild, Visual Studio, and CI usage. | [WiX Toolset documentation](https://docs.firegiant.com/wix/) |
| Documented | Inno Setup is an open-source Windows installation builder. | [Inno Setup documentation](https://jrsoftware.org/ishelp.php) |
| Open | Production signing certificate, update channel, public archive policy, and final Heat attribution bundle remain unresolved. | ADR-012 / docs/app-shell |

## Installer Options

| Option | Fit now | Tradeoff |
|---|---:|---|
| Local publish folder only | Complete | Already works through Gate 341, but not a polished installer experience. |
| Inno Setup script | Best next step | Simple installer script over the existing app folder; can add shortcuts and install location without committing binaries. Installer build can stay gated until tool availability and signing are checked. |
| WiX/MSI | Later candidate | Strong enterprise/MSI story and CI/MSBuild fit, but higher authoring complexity for the first local installer lane. |
| MSIX | Later candidate | Modern package identity, clean uninstall, and update path, but signing is mandatory and package identity/update policy must be settled first. |
| Self-contained app publish | Later input option | Reduces runtime dependency but increases output size and needs explicit restore/runtime policy. |
| Single-file app publish | Later input option | Needs WPF resource, `ForgeBackend/`, and demo-source extraction validation before installer use. |

## Decision

Use **Inno Setup script scaffolding** as the next installer lane.

This is a pragmatic first installer step because the app already has a complete
local publish folder. The next gate can scaffold an `.iss` script that points at
`dist/app/WastelandForge.Desktop/`, installs the app folder, creates Start Menu
and optional Desktop shortcuts, and records installer-boundary metadata without
building the installer yet.

MSIX stays open for a later signed package/update-channel lane. WiX/MSI stays
open for a later enterprise installer lane if MSI governance becomes important.

## Planned Installer Script Boundary

The first installer script scaffold should:

- consume `dist/app/WastelandForge.Desktop/` as input,
- require `WastelandForge.exe`, `ForgeBackend/forge.exe`,
  `DemoProjects/ExampleMod/wastelandforge.json`,
  `app-build-manifest.json`, and `checksums.sha256`,
- author deterministic installer metadata in source form,
- install under a WastelandForge application directory,
- add Start Menu shortcut metadata,
- keep Desktop shortcut optional,
- avoid embedding raw Heat source assets,
- avoid building an installer executable unless a later gate explicitly does
  so,
- avoid signing, timestamping, update channel, release publication, runtime
  probes, external tool execution, or AI behavior.

## Next Route

Gate 343 should scaffold the Inno Setup installer script source and installer
input validation notes.

Gate 343 should stop before:

- running Inno Setup,
- producing a setup executable,
- code signing,
- timestamping,
- update-channel implementation,
- release publication,
- public archive packaging,
- Heat source asset commits,
- external tool execution,
- MO2 or GECK automation,
- runtime probes,
- AI behavior.

## Not Implemented

Gate 342 does not implement:

- source code changes,
- CLI runtime behavior changes,
- app runtime behavior changes,
- installer project or script,
- installer creation,
- archive creation,
- update-channel logic,
- code signing,
- attestation,
- release publication,
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
- Heat source asset commits,
- remote repository calls,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Installer technologies are compared | Complete | This gate compares local folder, Inno Setup, WiX/MSI, MSIX, self-contained publish, and single-file publish. |
| First installer route is selected | Complete | Inno Setup script scaffolding is selected as the next lane. |
| MSIX signing dependency remains explicit | Complete | MSIX is deferred because signing and identity/update policy are unresolved. |
| Installer creation remains gated | Complete | Gate 343 is routed to script source only, not setup executable creation. |
| Heat boundary is preserved | Complete | Planned installer input consumes app publish output and does not commit raw Heat assets. |

## Validation

Required validation:

```text
git diff --check
rg -n "Route the next development step to Gate 342|Gate 342: Windows installer technology planning" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 343|Gate 343" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime build/test is not required for Gate 342 because it does not change
source code, project metadata, task behavior, workflow behavior, app runtime
behavior, installer behavior, or CLI runtime behavior.

## Next Gate

Gate 343 should scaffold the Inno Setup installer script source and installer
input validation notes, still stopping before installer creation,
update-channel logic, signing, timestamping, attestation, NuGet publication,
package restore policy changes, generated consumer template mutation, provider
installation, external tool execution, MO2/GECK automation, runtime probes,
real third-party plugin fixtures, release publication, remote repository calls,
plugin mutation, VS Code extension generation, language-server process
startup, or AI behavior.
