# WastelandForge App Shell

Status: Planning
Research classification: Mixed
Source: ADR-012, ADR-006, ADR-010, ADR-011, Unity Asset Store Terms/EULA, and
user-provided Heat 1.1.8 ownership evidence

WastelandForge will provide a premium Windows app shell as a polished front end
over the deterministic Forge backend.

## Product Shape

The app shell is `WastelandForge.exe`, built with Unity and the licensed Heat -
Complete Modern UI 1.1.8 kit. It is intended to feel like a real desktop
product, not a command prompt wrapper.

The app shell owns:

- splash and first-run experience,
- project selection,
- status dashboards,
- workflow navigation,
- report presentation,
- advanced logs,
- local app settings.

The app shell does not own:

- canonical project truth,
- schema or semantic validation rules,
- capability resolution semantics,
- build graph correctness,
- package or release correctness,
- external provider installation,
- GECK, MO2, xEdit, or game automation.

## Backend Bridge

The first implementation uses `forge.exe` beside the app as a backend worker.
The first bridge commands are:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
```

The GUI must consume JSON and other machine-readable reports. Human console text
is acceptable in advanced logs only.

## MVP Views

The first MVP view set is:

- splash screen,
- project selector,
- Doctor/capability dashboard,
- validation report view,
- advanced log panel.

The first setup fields are:

- project root,
- Fallout: New Vegas game root,
- Data root,
- MO2 path,
- external tool paths.

## Asset Boundary

Heat source assets stay outside the public repository unless a later gate
explicitly defines a private asset workspace and redistribution-safe export
process. Compiled app output may embed licensed assets after audit.

Do not use Heat assets for AI/ML training, datasets, scraping, or reusable asset
distribution.

See `asset-license-audit.md` for the current Heat 1.1.8 import boundary. The
approved local import path is ignored by git:

```text
apps/WastelandForge.App/Assets/ThirdParty/Heat - Complete Modern UI/
```

## Open Implementation Checks

- Unity version and build target.
- App-shell project location.
- Final Heat Restricted Asset status before public release.
- Seat/license coverage for every contributor who opens or modifies the Unity
  project.
- Third-party attribution bundle.
- Installer technology.
- Code signing and update channel.
- CI path that does not make Unity mandatory for CLI/core validation.
