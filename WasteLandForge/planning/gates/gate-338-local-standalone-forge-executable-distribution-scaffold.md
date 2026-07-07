# Gate 338 - Local Standalone Forge Executable Distribution Scaffold

Status: Complete
Phase: CLI bootstrap implementation
Decision base: ADR-009, ADR-010, ADR-011, ADR-012, Gates 322, 334-337, R006, R008

## Goal

Scaffold the source-repository local standalone `forge.exe` distribution lane
planned by Gate 337.

This gate provides a manual/offline handoff path for users who need generated
consumer projects to find a `forge` command before public package/feed,
installer, signing, or update-channel governance is ready.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | The canonical command remains `forge`; generated workflows and editor tasks are thin wrappers around that CLI. | R006 / ADR-010 |
| Documented | Generated outputs and distribution outputs need local manifest and checksum evidence before release governance. | R008 / ADR-011 |
| Documented | Generated consumer projects stay source-agnostic and expect an existing `forge` command. | Gates 334-336 |
| Documented | Gate 337 selects a local standalone `dist/local/forge/` style artifact with `forge.exe`, README, manifest, and checksums. | Gate 337 |
| Documented | `WastelandForge.exe` app-shell packaging remains a separate lane. | ADR-012 / Gates 313-314 |
| Open | Public NuGet publication, installer technology, code signing, update channels, and app-shell bundling remain unresolved. | Gate 337 / ADR-012 |

## Implementation

Gate 338 adds:

- `eng/Publish-StandaloneForge.ps1`
- source-repository VS Code task `Forge: Publish Standalone`
- documentation and prompt-route updates for the standalone distribution lane

The script publishes `src/WastelandForge.Cli/WastelandForge.Cli.csproj` as a
Windows framework-dependent `forge.exe` apphost into the ignored local
distribution folder:

```text
dist/local/forge/
  forge.exe
  supporting .NET publish files
  README.txt
  build-manifest.json
  checksums.sha256
```

The script deliberately uses `dotnet publish --no-restore`, so it does not
perform package restore. If restore or SDK preparation is required, it must
happen through an already-gated repository bootstrap path before this script is
run.

The script has an explicit `-SelfContained` option, but that still uses
`--no-restore` and therefore only works after the repository has already been
restored for the selected runtime identifier.

For validation or temporary runs, the script may also write under:

```text
artifacts/standalone-forge/<name>/
```

It refuses to clean or write outside `dist/local/forge` or
`artifacts/standalone-forge/<name>` to keep cleanup bounded to ignored local
distribution evidence.

## Local Evidence

The generated README explains the manual command-availability options:

```text
add this folder to PATH, or set FORGE_COMMAND to this forge.exe path
```

The generated `build-manifest.json` records:

- gate number,
- configuration,
- runtime identifier,
- framework-dependent/self-contained settings,
- no-restore status,
- source project path,
- smoke-tested `forge.exe --version` output unless `-SkipSmoke` is used,
- explicit false boundary fields for installer, package restore, NuGet
  publication, signing, attestation, update channel, generated consumer
  template mutation, app-shell packaging, external game-tool execution, runtime
  probes, and AI requirement,
- output file digests.

The generated `checksums.sha256` covers the output files, including
`build-manifest.json`, in canonical relative-path order.

## Boundary

Gate 338 does not implement:

- installer creation,
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
- external game-tool execution,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- release publication,
- remote repository calls,
- WPF app-shell packaging,
- VS Code extension generation,
- language-server process startup,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Source-repository standalone publish helper exists | Complete | `eng/Publish-StandaloneForge.ps1` publishes the CLI project into an ignored distribution folder. |
| Generated consumer projects remain source-agnostic | Complete | No generated `forge init` template files are changed by this gate. |
| Local build evidence is produced | Complete | The script writes `build-manifest.json` and `checksums.sha256`. |
| Cleanup is bounded | Complete | The script refuses output roots outside `dist/local/forge` and `artifacts/standalone-forge/<name>`. |
| Source-repository task exists | Complete | `.vscode/tasks.json` includes `Forge: Publish Standalone`. |
| No package restore or publication is added | Complete | The script invokes `dotnet publish --no-restore` and does not call `dotnet pack`, `dotnet tool restore`, or repository APIs. |

## Validation

Required validation:

```text
PowerShell parser check for eng/Publish-StandaloneForge.ps1
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Publish-StandaloneForge.ps1 -OutputRoot artifacts/standalone-forge/gate-338
artifacts/standalone-forge/gate-338/forge.exe --version
Test-Path artifacts/standalone-forge/gate-338/build-manifest.json
Test-Path artifacts/standalone-forge/gate-338/checksums.sha256
dotnet build WastelandForge.sln -c Release --no-restore
git diff --check
rg -n "Route the next development step to Gate 338|Gate 338: local standalone Forge executable distribution scaffold" WasteLandForge/skills WasteLandForge/agents .agents
rg -n "Route the next development step to Gate 339|Gate 339" WasteLandForge/skills WasteLandForge/agents .agents WasteLandForge/planning/README.md
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

## Next Gate

Gate 339 should close the local standalone Forge executable distribution lane
and route the next value slice, still stopping before installer creation,
update-channel logic, signing, attestation, NuGet publication, package restore,
generated local tool manifest emission, root package-source config, generated
consumer template mutation, provider installation, external tool execution,
MO2/GECK automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, app-shell packaging, plugin mutation,
VS Code extension generation, language-server process startup, or AI behavior.
