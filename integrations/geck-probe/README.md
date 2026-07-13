# WastelandForge GECK Host Probe

This Forge-owned native module is the Gate 527 build-only xNVSE host probe. It
tests only the xNVSE GECK plugin boundary and is compiled with live launch
authorization disabled.

The source is intentionally limited to `NVSEPlugin_Query`, `NVSEPlugin_Load`,
editor/version refusal, and private LocalAppData observation output. It does not
register commands or events, use record APIs, launch tools, send UI input, or
write to game Data, MO2, projects, ESP, or ESM files.

Build against an external clean checkout of xNVSE 6.4.4 at commit
`694cdde6cbfa5e75afa661df587c73e8f0f6f441` and tree
`e80453c217027f47979d2dfca03665de0a93fa6f`:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-GeckProbe.ps1 -XnvseSourceRoot <path>
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-GeckProbeBuild.ps1
```

Outputs are local generated evidence under `generated/geck-probe/`. Building
does not install or stage the DLL and does not authorize MO2 or GECK execution.
