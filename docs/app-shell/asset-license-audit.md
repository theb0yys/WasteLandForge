# App Shell Asset And License Audit

Status: Initial audit complete
Research classification: Mixed
Source: ADR-012, Gate 313, Unity Asset Store Terms/EULA, Heat 1.1.8 readme,
user-provided ownership evidence, and local Heat 1.1.8 folder inspection

This audit records the boundary for using Heat - Complete Modern UI 1.1.8 in
the native WPF WastelandForge Windows app shell.

## Local Asset Evidence

| Classification | Finding | Evidence |
|---|---|---|
| Documented | The local kit identifies itself as Heat - Complete Modern UI 1.1.8. | Local `Read Me.txt`. |
| Documented | The kit contains Unity-oriented scenes, prefabs, scripts, textures, fonts, audio, localization, resources, editor scripts, and presets. | Local folder inspection. |
| Documented | The readme credits placeholder imagery and controller graphics, with the controller graphics under CC BY 3.0. | Local `Read Me.txt`. |
| Documented | User-provided screenshot evidence shows the asset in the Unity Asset Store account with Heat version 1.1.8. | Attached screenshot evidence, not committed to repo. |
| Inferred | WPF can use selected Heat textures and fonts as licensed visual inputs without using Unity Player. | Local PNG and TTF inspection plus WPF resource support. |

## External License Evidence

| Classification | Finding | Evidence |
|---|---|---|
| Documented | Unity Asset Store assets are licensed under the Unity Asset Store EULA unless separate provider terms apply. | Unity Asset Store Terms/EULA. |
| Documented | The Unity EULA allows Asset use as embedded components of a larger licensed product, subject to the EULA restrictions. | Unity Asset Store Terms/EULA. |
| Documented | The Unity EULA prohibits AI/ML training, scraping, aggregation, dataset, and extraction-style uses without required consent. | Unity Asset Store Terms/EULA. |
| Open | Final Restricted Asset status must be confirmed from the current Asset Store listing and package materials before public release. | Dynamic store/account evidence should be rechecked at release time. |
| Open | Exact contributor seat coverage must be confirmed before any additional contributor uses the Heat asset source. | Requires team/license records outside the repo. |

## Local Build Boundary

Heat source assets must stay outside the public repository. The WPF app shell
may embed selected Heat-derived resources into a local build by passing
`HeatSourceRoot` during publish:

```text
dotnet publish src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release -r win-x64 --self-contained true -o dist/app/WastelandForge.Desktop /p:HeatSourceRoot="<local Heat folder>"
```

The source tree must not commit, rehost, or distribute the raw Heat folder.

Compiled app output may embed licensed Heat-derived assets only after release
gates verify:

- Restricted Asset status,
- seat/license coverage,
- third-party attribution bundle,
- CC BY 3.0 attribution if controller graphics ship,
- no reusable raw asset extraction path in the app package,
- installer/signing/release evidence.

## App-Shell Dependency Boundary

Heat is allowed as the premium app-shell visual system. Unity Player is not part
of the accepted app-shell implementation. Heat is not allowed to become a hard
requirement for Forge core, CLI validation, CI correctness, build graph
correctness, or release verification.

The deterministic backend remains `forge.exe`. The first GUI bridge remains:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
```

## Approved Current Action

This audit approves optional local Heat resource embedding into the WPF app
shell for app-shell development only.

This audit does not approve:

- committing raw Heat source assets,
- publishing an installer,
- sharing the Heat asset folder with unlicensed contributors,
- using Heat assets for AI/ML, dataset, scraping, or extraction workflows,
- making Heat required for Forge CLI/core contribution.
