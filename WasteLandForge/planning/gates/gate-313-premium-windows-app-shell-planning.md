# Gate 313 - Premium Windows App Shell Planning

Status: Complete
Phase: app-shell planning
Decision base: ADR-012, ADR-006, ADR-010, ADR-011, WFG-001

## Goal

Start the premium Windows GUI lane without disrupting the existing Gate 312
`forge init` route.

Gate 313 records the app-shell plan for a native .NET/WPF `WastelandForge.exe`
using the licensed Heat - Complete Modern UI 1.1.8 kit as the visual system,
with `forge.exe` bundled beside the app as the deterministic backend worker.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Forge owns contracts, registries, validation, generation, documentation, packaging metadata, release automation, and capability rules outside the game. | ADR-006 |
| Documented | Forge exposes a stable offline-first CLI with machine-readable output contracts. | ADR-010 / R006 |
| Documented | Validation, build, release, and contribution correctness must remain offline-first and AI-optional. | ADR-011 / R008 |
| Documented | No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core. | WFG-001 |
| Inferred | A GUI app shell is acceptable when it is a thin presentation/orchestration layer over the same Forge core and CLI JSON contracts. | ADR-006 / ADR-010 / ADR-011 |
| Inferred | WPF is an acceptable first app-shell route because it produces a native Windows `.exe` while still allowing selected Heat visual resources to be embedded under the license boundary. | ADR-012 / user-provided Heat 1.1.8 asset evidence |
| Open | Installer technology, signing, update channel, Heat attribution, and CI strategy still require follow-up gates. | ADR-012 open checks |

## Planned App-Shell Shape

The first app-shell lane targets:

- `WastelandForge.exe` as the double-clickable Windows app,
- .NET/WPF as the native desktop shell implementation,
- `forge.exe` as a bundled backend worker,
- Heat 1.1.8 as the initial visual system,
- JSON output consumption rather than human console parsing,
- no external provider installation,
- no GECK automation,
- no MO2 automation,
- no xEdit execution,
- no runtime probes,
- no plugin mutation,
- no AI requirement.

## First Backend Bridge

The first backend bridge is limited to:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
```

Later gates may add `forge capabilities scan --format json`,
`forge doctor export --format json`, and report bundle readers, but this gate
does not open that implementation scope.

## MVP Views

The first MVP view set is:

- branded splash/application chrome,
- project selector,
- Doctor/capability dashboard shell,
- validation report view shell,
- advanced log panel.

## Asset And Redistribution Boundary

Heat source assets must not be committed to the public repository or
redistributed as a raw asset pack.

Before public release, the app-shell lane must audit:

- Heat Asset Store license tier and Restricted Asset status,
- contributor seat coverage,
- third-party notices named by the Heat readme,
- CC BY 3.0 controller graphic attribution if those graphics ship,
- whether the app build keeps licensed assets embedded rather than reusable.

## Deliverables For This Gate

- ADR-012 accepted.
- App-shell brief under `docs/app-shell/`.
- Planning index route that preserves Gate 312 as `forge init`.
- WPF app-shell direction recorded.
- No raw Heat asset import into source control.
- No installer yet.
- No runtime behavior change.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Premium app-shell direction is recorded | Complete | ADR-012 and this gate define the WPF/Heat shell over `forge.exe`. |
| Gate 312 remains intact | Complete | Planning index keeps Gate 312 routed to `forge init`. |
| Core correctness path remains CLI-backed | Complete | ADR-012 keeps JSON contracts and `forge.exe` as the backend worker. |
| Asset redistribution boundary is explicit | Complete | Heat source assets remain outside the public repository and may only be embedded under audit. |
| Runtime behavior is unchanged | Complete | This gate adds no code, commands, schemas, assets, installer, or generated outputs. |

## Not Implemented

Gate 313 does not implement:

- a Unity project,
- raw Heat asset import into source control,
- app shell code,
- app-to-CLI process execution,
- installer generation,
- code signing,
- update channels,
- new Forge CLI commands or aliases,
- command fan-out from the CLI,
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

Gate 313 is a planning gate. Required validation is document consistency:

```text
git diff --check
```

Normal build/test smoke checks are optional because no code or project files
are changed by this gate.

## Next App-Shell Gates

The next app-shell gates should be:

1. Gate 314 - Heat asset and WPF build boundary.
2. Gate 315 - WPF app-shell scaffold and backend bridge.
3. Gate 316 - splash and project selector MVP.
4. Gate 317 - capability/Doctor dashboard MVP.
5. Gate 318 - validation report view MVP.
6. Later gate - installer, signing, and release evidence.
