# WastelandForge Inno Setup Source

Status: Gate 343 source scaffold

This directory contains source metadata for the future WastelandForge Windows
installer lane.

The current script consumes an existing local app-shell publish folder:

```text
dist/app/WastelandForge.Desktop/
```

The input folder must contain:

- `WastelandForge.exe`
- `ForgeBackend/forge.exe`
- `DemoProjects/ExampleMod/wastelandforge.json`
- `app-build-manifest.json`
- `checksums.sha256`

Validate the input folder before any future compiler run:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1
```

For temporary app-shell artifacts:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1 -AppShellRoot artifacts/app-shell/gate-341
```

Future gated compiler invocation shape:

```text
iscc /DAppShellSource="D:\documents\GitHub\WasteLandForge\dist\app\WastelandForge.Desktop" installer\inno\WastelandForge.iss
```

Do not run that compiler command as part of Gate 343. Gate 343 does not create
a setup executable, sign binaries, timestamp artifacts, add an update channel,
publish releases, commit Heat source assets, execute game tools, run runtime
probes, or require AI.
