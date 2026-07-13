# WastelandForge Inno Setup Source

Status: Gate 344 source scaffold and local build helper

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
- `DemoProjects/GeckAuthoringPlanExample/wastelandforge.json`
- `DemoProjects/GeckAuthoringPlanExample/README.md`
- `DemoProjects/GeckAuthoringPlanExample/evidence/valid-first-slice.json`
- `app-build-manifest.json`
- `checksums.sha256`

Validate the input folder before any future compiler run:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1
```

Detect the local Inno Setup compiler without building:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-AppShellInstaller.ps1 -DetectOnly
```

For temporary app-shell artifacts:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1 -AppShellRoot artifacts/app-shell/gate-341
```

Build an unsigned local installer when Inno Setup is installed:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-AppShellInstaller.ps1
```

Gate 344 does not install Inno Setup. If `ISCC.exe` is missing, the helper
reports that honestly and does not create a setup executable.

After maintainer request, this machine installed Inno Setup 6.7.3 through
`winget`. The compiler was installed at:

```text
C:\Users\kane0\AppData\Local\Programs\Inno Setup 6\ISCC.exe
```

The helper detects that per-user compiler path.

Any setup executable created by the helper is local, unsigned, ignored under
`artifacts/installer/inno/<name>`, and not a published release. Gate 344 does
not sign binaries, timestamp artifacts, add an update channel, publish
releases, commit Heat source assets, execute game tools, run runtime probes, or
require AI.
