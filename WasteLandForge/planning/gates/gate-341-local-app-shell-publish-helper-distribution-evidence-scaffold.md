# Gate 341 - Local App-Shell Publish Helper Distribution Evidence Scaffold

Status: Complete
Phase: app-shell distribution scaffold
Decision base: ADR-011, ADR-012, Gates 313-314, Gates 339-340, R008

## Goal

Scaffold a repository-owned local publish helper for the premium Windows app
shell and add local distribution evidence.

Gate 341 remains a local developer distribution helper. It does not create an
installer, sign binaries, publish releases, add an update channel, execute
external game tools, run runtime probes, mutate generated consumer templates,
or require AI.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `WastelandForge.exe` is a native .NET/WPF app shell over bundled `forge.exe`. | ADR-012 |
| Documented | App-shell correctness must stay in Forge core and CLI JSON contracts, not duplicate in the GUI. | ADR-012 / docs/app-shell |
| Documented | Local distribution outputs need manifest and checksum evidence. | R008 / ADR-011 |
| Documented | Heat source assets must stay outside the repo and can only be optional local publish inputs. | Gate 314 / docs/app-shell/asset-license-audit.md |
| Documented | Gate 340 selected a local app-shell publish helper before installer, signing, update-channel, or public archive work. | Gate 340 |
| Open | Installer technology, code signing, update channel, and final public release package policy remain unresolved. | ADR-012 / Gate 340 |

## Implementation

Gate 341 adds:

- `eng/Publish-AppShell.ps1`
- source-repository VS Code task `Forge: Publish App Shell`
- documentation and prompt-route updates for local app-shell distribution

The script publishes `src/WastelandForge.Desktop/WastelandForge.Desktop.csproj`
into an ignored local app-shell distribution folder:

```text
dist/app/WastelandForge.Desktop/
  WastelandForge.exe
  ForgeBackend/forge.exe
  DemoProjects/ExampleMod/
  README.txt
  app-build-manifest.json
  checksums.sha256
```

For validation or temporary runs, the script may also write under:

```text
artifacts/app-shell/<name>/
```

It refuses to clean or write outside `dist/app/WastelandForge.Desktop` or
`artifacts/app-shell/<name>`.

## Local Evidence

The generated `app-build-manifest.json` records:

- gate number,
- configuration,
- runtime identifier,
- framework-dependent/self-contained settings,
- no-restore status,
- source project path,
- backend root,
- bundled backend smoke-test version,
- bundled demo-project path,
- Heat local-input status without recording or committing raw Heat assets,
- explicit false boundary fields for installer creation, archive creation,
  signing, attestation, update channel, release publication, NuGet
  publication, package-restore policy changes, generated consumer template
  mutation, external game-tool execution, runtime probes, and AI requirement,
- output file digests.

The generated `checksums.sha256` covers the output files, including
`app-build-manifest.json`, in canonical relative-path order.

## Boundary

Gate 341 does not implement:

- installer creation,
- public archive packaging,
- update-channel logic,
- code signing,
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
| Local app-shell publish helper exists | Complete | `eng/Publish-AppShell.ps1` publishes the WPF app shell into a bounded ignored output folder. |
| Backend is bundled | Complete | The helper publishes or requires `dist/local/forge` and checks `ForgeBackend/forge.exe`. |
| Demo source is bundled without generated output | Complete | The WPF project publish target copies `DemoProjects/ExampleMod` while excluding fixture `generated` and `dist` roots. |
| Local build evidence is produced | Complete | The helper writes `app-build-manifest.json` and `checksums.sha256`. |
| Installer/signing/update-channel remain gated | Complete | Manifest boundary flags and this gate record those as false. |
| Source-repository task exists | Complete | `.vscode/tasks.json` includes `Forge: Publish App Shell`. |

## Validation

Required validation:

```text
PowerShell parser check for eng/Publish-AppShell.ps1
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Publish-AppShell.ps1 -OutputRoot artifacts/app-shell/gate-341
Test-Path artifacts/app-shell/gate-341/WastelandForge.exe
Test-Path artifacts/app-shell/gate-341/ForgeBackend/forge.exe
Test-Path artifacts/app-shell/gate-341/DemoProjects/ExampleMod/wastelandforge.json
Test-Path artifacts/app-shell/gate-341/app-build-manifest.json
Test-Path artifacts/app-shell/gate-341/checksums.sha256
artifacts/app-shell/gate-341/ForgeBackend/forge.exe --version
dotnet build WastelandForge.sln -c Release --no-restore
git diff --check
rg -n "Route the next development step to Gate 341|Gate 341: local app-shell publish helper" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 342|Gate 342" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

## Next Gate

Gate 342 should plan Windows installer technology for the app shell, still
stopping before installer creation, update-channel logic, signing,
attestation, NuGet publication, package restore policy changes, generated
consumer template mutation, provider installation, external tool execution,
MO2/GECK automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, plugin mutation, VS Code extension
generation, language-server process startup, or AI behavior.
