# Gate 314 - App Shell Asset And License Audit

Status: Complete
Phase: app-shell planning
Decision base: ADR-012, Gate 313, ADR-006, ADR-010, ADR-011, WFG-001

## Goal

Complete the first Heat asset/license audit and open a safe local-only import
lane for app-shell development.

Gate 314 permits copying Heat - Complete Modern UI 1.1.8 into the Unity app
project under a git-ignored local asset path. It does not permit committing raw
Heat source assets, releasing an installer, or making Unity/Heat a hard
dependency of Forge core.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Forge core owns contracts, validation, generation, release automation, and capability rules outside the game. | ADR-006 |
| Documented | Forge exposes stable machine-readable CLI output and must avoid undocumented command aliases. | ADR-010 / R006 |
| Documented | Validation and release correctness remain offline-first and AI-optional. | ADR-011 / R008 |
| Documented | No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core. | WFG-001 |
| Documented | Heat 1.1.8 is present locally with Unity scenes, prefabs, scripts, textures, fonts, audio, and metadata. | Local Heat folder inspection |
| Documented | Unity Asset Store EULA and Michsky docs place Heat under the default Asset Store EULA unless separate terms apply. | Unity Asset Store Terms/EULA, Michsky docs |
| Inferred | A local ignored Heat import is acceptable for app-shell development because the raw source asset pack is not committed or redistributed. | ADR-012 and asset audit |
| Open | Restricted Asset status, exact seat coverage, and release attribution package still require release-time confirmation. | Asset audit open checks |

## Local Import Boundary

The approved local import path is:

```text
apps/WastelandForge.App/Assets/ThirdParty/Heat - Complete Modern UI/
```

The repository `.gitignore` must keep that path ignored. App-shell project code
may reference locally imported Heat assets, but raw Heat files must stay out of
git.

## Deliverables For This Gate

- Asset/license audit under `docs/app-shell/`.
- `.gitignore` entries for Unity generated folders and the raw Heat import.
- Local-only Heat import path defined.
- Gate 313 closed as the planning gate.
- No Forge CLI command changes.
- No Gate 312 route changes.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Heat asset/license boundary recorded | Complete | `docs/app-shell/asset-license-audit.md`. |
| Raw Heat source assets remain non-public repo content | Complete | `.gitignore` excludes the local Heat import path. |
| Gate 312 remains intact | Complete | Planning index keeps Gate 312 routed to `forge init`. |
| Core correctness path remains CLI-backed | Complete | ADR-012 and app-shell docs keep `forge.exe` as the backend worker. |
| Runtime behavior is unchanged | Complete | This gate adds planning/import scaffolding only and does not alter Forge CLI behavior. |

## Not Implemented

Gate 314 does not implement:

- app-to-CLI process execution,
- `forge.exe` bundling,
- a production Unity scene,
- installer generation,
- code signing,
- update channels,
- new Forge CLI commands or aliases,
- external tool execution,
- provider installation,
- GECK automation,
- MO2 automation,
- xEdit execution,
- runtime probes,
- plugin mutation,
- real third-party plugin fixtures,
- AI behavior.

## Validation

Required validation is document/import consistency:

```text
git diff --check
```

Normal .NET build/test smoke checks are optional because this gate does not
change Forge CLI/core code.

## Next App-Shell Gate

Gate 315 should define the Unity project layout and app-shell scaffold in a way
that keeps Unity and Heat optional for Forge core contributors.
