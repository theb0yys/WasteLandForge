# WastelandForge App Shell

Status: MVP implementation
Research classification: Mixed
Source: ADR-012, ADR-006, ADR-010, ADR-011, Unity Asset Store Terms/EULA,
Heat 1.1.8 readme, and user-provided Heat ownership evidence

WastelandForge provides a premium Windows app shell as a polished front end over
the deterministic Forge backend.

## Product Shape

The app shell is `WastelandForge.exe`, built as a native .NET/WPF desktop app.
It keeps Heat - Complete Modern UI 1.1.8 as the visual direction and optional
licensed build input, but it is not a Unity Player app.

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

The implementation uses `forge.exe` beside the app as a backend worker. The
first bridge commands are:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
```

The GUI consumes JSON and other machine-readable reports. Human console text is
acceptable in advanced logs only.

## MVP Views

The first MVP view set is:

- splash/application chrome,
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

Heat source assets stay outside the public repository. The WPF project can
optionally embed selected Heat assets during a local publish by passing a
licensed local asset root:

```text
dotnet publish src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release -r win-x64 --self-contained true -o dist/app/WastelandForge.Desktop /p:HeatSourceRoot="<local Heat folder>"
```

Raw Heat files must not be committed or redistributed as repo content.

Do not use Heat assets for AI/ML training, datasets, scraping, or reusable asset
distribution.

See `asset-license-audit.md` for the current Heat 1.1.8 boundary.

## Open Implementation Checks

- Installer technology.
- Code signing and update channel.
- Final Heat Restricted Asset status before public release.
- Seat/license coverage for every contributor who uses the Heat asset source.
- Third-party attribution bundle.
- CI path that does not make Heat mandatory for CLI/core validation.
