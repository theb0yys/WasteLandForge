# App Shell Asset And License Audit

Status: Initial audit complete
Research classification: Mixed
Source: ADR-012, Gate 313, Unity Asset Store Terms/EULA, Michsky Heat product
and documentation pages, user-provided ownership evidence, and local Heat 1.1.8
asset folder inspection.

This audit records the boundary for using Heat - Complete Modern UI 1.1.8 in
the premium WastelandForge Windows app shell.

## Local Asset Evidence

| Classification | Finding | Evidence |
|---|---|---|
| Documented | The local kit identifies itself as Heat - Complete Modern UI 1.1.8. | Local `Read Me.txt`. |
| Documented | The kit contains Unity scenes, prefabs, scripts, textures, fonts, audio, localization, resources, editor scripts, and presets. | Local folder inspection. |
| Documented | The local file mix includes 342 PNG files, 123 C# files, 59 prefabs, 49 animations, 21 assets, 16 controllers, 6 fonts, 6 scenes, 3 WAV files, and Unity `.meta` files. | Local file count. |
| Documented | The readme credits placeholder imagery and controller graphics, with the controller graphics under CC BY 3.0. | Local `Read Me.txt`. |
| Documented | User-provided screenshot evidence shows the asset in the Unity Asset Store account with Heat version 1.1.8. | Attached screenshot evidence, not committed to repo. |
| Inferred | The expanded folder can be copied into a local Unity project as `Assets/ThirdParty/Heat - Complete Modern UI` without requiring a `.unitypackage` import step. | Local folder already includes Unity `.meta` files. |

## External License Evidence

| Classification | Finding | Evidence |
|---|---|---|
| Documented | Unity Asset Store assets are licensed under the Unity Asset Store EULA unless separate provider terms apply. | Unity Asset Store Terms/EULA. |
| Documented | Michsky documentation states Michsky assets are under the default Asset Store ToS and EULA. | Michsky docs. |
| Documented | The Unity EULA allows Asset use as embedded components of a larger licensed product, subject to the EULA restrictions. | Unity Asset Store Terms/EULA. |
| Documented | The Unity EULA prohibits AI/ML training, scraping, aggregation, dataset, and extraction-style uses without required consent. | Unity Asset Store Terms/EULA. |
| Documented | Extension Assets are seat-based under the Unity EULA. | Unity Asset Store Terms/EULA. |
| Open | Final Restricted Asset status must be confirmed from the current Asset Store listing and package materials before public release. | Dynamic store/account evidence should be rechecked at release time. |
| Open | Exact contributor seat coverage must be confirmed before any additional contributor opens or modifies the Unity project. | Requires team/license records outside the repo. |

## Import Boundary

Heat source assets may be copied into the local Unity project only under an
ignored path:

```text
apps/WastelandForge.App/Assets/ThirdParty/Heat - Complete Modern UI/
```

That folder is intentionally ignored by git. The app shell may use those assets
locally, but the raw Heat source asset pack must not be committed, rehosted, or
redistributed as repository content.

Compiled app output may embed licensed Heat-derived assets only after later
release gates verify:

- Restricted Asset status,
- seat/license coverage,
- third-party attribution bundle,
- CC BY 3.0 attribution if controller graphics ship,
- no reusable raw asset extraction path in the app package,
- installer/signing/release evidence.

## App-Shell Dependency Boundary

Unity and Heat are allowed for the premium app shell. They are not allowed to
become hard requirements for Forge core, CLI validation, CI correctness, build
graph correctness, or release verification.

The deterministic backend remains `forge.exe`. The first GUI bridge remains:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
```

## Approved Current Action

This audit approves local Heat import into the ignored Unity project asset
folder for app-shell development.

This audit does not approve:

- committing raw Heat source assets,
- publishing an installer,
- publishing a compiled app package,
- sharing the Heat asset folder with unlicensed contributors,
- using Heat assets for AI/ML, dataset, scraping, or extraction workflows,
- making Unity or Heat required for Forge CLI/core contribution.

