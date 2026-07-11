# Gate 348 - Local App Installer Refresh And UI Smoke

Status: Complete
Phase: app-shell local distribution validation
Decision base: ADR-011, ADR-012, Gates 341, 344, 346, 347

## Goal

Produce a current local app-shell distribution and unsigned installer that
contain the Settings and settings-backed Doctor scan work, then verify the
published GUI launches and exposes that workflow.

## Implementation And Evidence

- Hardened standalone CLI and app-shell publish helpers with single-node
  MSBuild (`-m:1`) after their parallel invocation failed on this machine.
- Refreshed `dist/app/WastelandForge.Desktop` with 36 manifest-tracked payload
  files and bundled `WastelandForge 0.1.0` backend.
- Passed installer input preflight.
- Built the unsigned local installer with Inno Setup 6.7.3.
- Verified the published app creates a responsive top-level window.
- Used Windows UI Automation to select Capabilities, invoke Scan Environment,
  and observe a visible Doctor readiness summary.

Local test outputs:

```text
dist/app/WastelandForge.Desktop/WastelandForge.exe
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
```

SHA-256:

```text
WastelandForge.exe
8a0506083000c861d55185271d5aab6898612e99dcd94039c294e56b5dda234f

WastelandForge-Setup-local.exe
5b4d50f05e833940707d30ef5b2841f0191f7cce1282a99e06d2effe1d185426
```

## Boundary

The installer is unsigned, not timestamped, and local-only. Gate 348 does not
install the setup executable, publish a release, add an update channel, upload
artifacts, execute third-party modding tools, run runtime probes, mutate
plugins, add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Current app source is published | Complete | App manifest and current executable under `dist/app`. |
| Installer inputs are verified | Complete | `Test-AppShellInstallerInputs.ps1` passed. |
| Current unsigned installer is built | Complete | Inno Setup produced the local setup executable. |
| Published app launches | Complete | Process remained alive and responsive with a `WastelandForge` window. |
| Gate 346/347 UI is exposed | Complete | UI Automation found Settings and invoked Scan Environment. |
| Doctor result becomes visible | Complete | UI exposed `0 of 4 areas ready; 0 need action; 4 unknown.` in the smoke environment. |

## Next Gate

Gate 349 should replace raw-JSON-first Doctor consumption with a structured
app-shell Doctor results view: readiness area rows, statuses, and immediate
actions derived exclusively from existing scan JSON. It must keep raw JSON in
the advanced Capabilities view and must not duplicate scanner semantics,
install providers, execute external tools, mutate plugins, or require AI.
