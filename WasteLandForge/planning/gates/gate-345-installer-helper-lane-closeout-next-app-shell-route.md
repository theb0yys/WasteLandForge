# Gate 345 - Installer Helper Lane Closeout And Next App-Shell Route

Status: Complete
Phase: app-shell installer helper lane closeout
Decision base: ADR-011, ADR-012, Gates 341-344, R008

## Goal

Close the current app-shell installer helper lane and route the next work back
to app-shell product value.

Gate 345 records that the repository now has local app-shell publish evidence,
Inno Setup source metadata, installer input preflight, and an unsigned local
installer helper that can build only when `ISCC.exe` is available. This gate
does not add runtime behavior, build an installer, install Inno Setup, sign
binaries, timestamp artifacts, publish releases, add an update channel, execute
external game tools, run runtime probes, mutate generated consumer templates,
commit Heat source assets, or require AI.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `WastelandForge.exe` is a native .NET/WPF shell over bundled `forge.exe`. | ADR-012 |
| Documented | App-shell correctness must remain in Forge core and CLI JSON contracts. | ADR-012 / docs/app-shell |
| Documented | Local distribution outputs need build-manifest and checksum evidence. | R008 / ADR-011 |
| Documented | Gate 344 added Inno Setup compiler detection and an unsigned local installer helper. | Gate 344 |
| Documented | The first app-shell setup fields are project root, Fallout: New Vegas game root, Data root, MO2 path, and external tool paths. | docs/app-shell |
| Documented | ADR-012 lists local settings surface and first-run setup wizard as later app-shell work. | ADR-012 |
| Open | Code signing certificate, timestamping, update channel, public release package policy, and final Heat attribution bundle remain unresolved. | ADR-012 / docs/app-shell |

## Closeout

The current installer helper lane includes:

- local app-shell publish output with manifest/checksum evidence,
- Inno Setup script source,
- app-shell installer input preflight,
- Inno Setup compiler detection,
- bounded unsigned local installer helper output under
  `artifacts/installer/inno/<name>`,
- source-repository VS Code task for local installer helper execution.

Original Gate 345 machine status was:

```text
Inno Setup compiler not found.
No setup executable created here.
```

After maintainer request, Inno Setup 6.7.3 was installed through `winget` under
the per-user install path:

```text
C:\Users\kane0\AppData\Local\Programs\Inno Setup 6\ISCC.exe
```

The local unsigned installer helper has now been validated with that compiler
and produced:

```text
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
```

Further installer work should wait until at least one of these is true:

- Inno Setup is installed and maintainers explicitly want local unsigned
  installer build validation on this machine,
- signing/timestamp/update-channel policy is settled,
- public release packaging policy is reopened.

## Next App-Shell Value Route

Gate 346 should implement the app-shell local setup/settings surface skeleton.

The planned first slice should expose and persist local-only paths for:

- project root,
- Fallout: New Vegas game root,
- Data root,
- MO2 path,
- external tool paths.

Gate 346 should keep settings local and outside canonical project truth. It
should not run provider probes, install providers, automate MO2 or GECK,
execute xEdit, mutate plugins, build installers, publish releases, sign or
timestamp artifacts, add telemetry, or require AI.

## Boundary

Gate 345 does not implement:

- WPF runtime behavior changes,
- CLI runtime behavior changes,
- installer build execution,
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

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Installer helper lane is closed for now | Complete | Gate 345 records the app-shell publish, Inno source, preflight, and helper state. |
| Missing compiler remains explicit | Complete | Original missing compiler state is recorded; current local validation records the per-user compiler and local unsigned setup output. |
| Next app-shell value route is selected | Complete | Gate 346 routes to local setup/settings surface skeleton. |
| Canonical truth boundary is preserved | Complete | Gate 346 is scoped to local-only settings, not project source truth. |
| Release boundaries remain gated | Complete | Signing, timestamping, update channel, release publication, and AI remain disabled. |

## Validation

Required validation:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-AppShellInstaller.ps1 -DetectOnly
rg -n "Gate 345|Gate 346|local setup/settings surface" docs WasteLandForge/planning/README.md WasteLandForge/skills WasteLandForge/agents .agents
git diff --check
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

Runtime .NET build/test is not required for Gate 345 because it does not change
.NET source code, project metadata, CLI runtime behavior, app runtime behavior,
workflow behavior, generated consumer templates, package restore behavior, or
release behavior.

## Next Gate

Gate 346 should implement the app-shell local setup/settings surface skeleton
for project root, Fallout: New Vegas game root, Data root, MO2 path, and
external tool paths, while preserving the backend boundary and keeping those
settings local-only.
