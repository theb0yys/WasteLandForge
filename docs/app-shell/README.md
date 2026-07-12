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

The implementation uses `forge.exe` as a backend worker. Local publishes bundle
the ignored `dist/local/forge/` backend distribution under
`ForgeBackend/` inside the app output, so build the Forge backend before
publishing the app shell:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Publish-AppShell.ps1
```

The helper writes `README.txt`, `app-build-manifest.json`, and
`checksums.sha256` into the local app output beside `WastelandForge.exe`.

The first bridge commands are:

```text
forge.exe --version
forge.exe capabilities list --format json
forge.exe validate <project-root> --format json
forge.exe generate <project-root> --target mcm-json --format json
forge.exe package <project-root> --target mcm-json --format json
forge.exe package <project-root> --target mcm-json --verify-existing --format json
```

The GUI consumes JSON and other machine-readable reports. Human console text is
acceptable in advanced logs only.

## MVP Views

The first MVP view set is:

- splash/application chrome,
- project selector,
- Doctor/capability dashboard,
- MCM package builder,
- bundled demo source copied to `%LOCALAPPDATA%\WastelandForge\DemoProjects\ExampleMod`,
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

## Installer Route

Gate 342 selects Inno Setup script scaffolding as the next installer lane. The
first installer step should author source installer metadata over the existing
local app output without producing a setup executable, signing binaries, or
adding an update channel.

Gate 343 adds that source scaffold under `installer/inno/` and a local
preflight helper:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Test-AppShellInstallerInputs.ps1
```

The preflight checks the app publish folder for `WastelandForge.exe`,
`ForgeBackend/forge.exe`, bundled demo source, `app-build-manifest.json`, and
`checksums.sha256` before any future installer compiler step.

Gate 344 adds `eng/Build-AppShellInstaller.ps1`, which detects the local Inno
Setup compiler and can build an unsigned local installer under ignored
`artifacts/installer/inno/<name>` when `ISCC.exe` is available:

```text
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-AppShellInstaller.ps1 -DetectOnly
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File eng/Build-AppShellInstaller.ps1
```

This machine has Inno Setup 6.7.3 installed in the per-user program location.
The helper detects it and can produce the ignored unsigned local installer at
`artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.

Gate 345 closes the current installer helper lane. Gate 346 adds the local
Settings tab and persists project, game, Data, MO2, GECK, and xEdit paths to
`%LOCALAPPDATA%\WastelandForge\app-settings.json`. These paths remain outside
canonical project truth and do not trigger provider probes or external tools.

Gate 347 adds explicit settings-backed environment scanning. The Dashboard and
Capabilities views invoke `forge capabilities scan` with saved project, game,
Data, MO2, GECK, and xEdit paths, show Doctor readiness counts, and retain the
full JSON report. The scan does not execute those external tools.

Gate 348 refreshes the local app distribution and unsigned installer with the
Gate 346/347 UI. Launch and UI Automation smoke checks confirmed the Settings
tab, Capabilities scan action, visible Doctor summary, and responsive window.

Gate 349 renders Doctor readiness areas, statuses, coverage counts, and
immediate actions directly in the Capabilities view. Full scan JSON remains
available under `Advanced JSON`.

Gate 350 adds per-area provider evidence drill-down with provider status,
install scope, detector kind, inspected path, and the backend evidence message.

Gate 351 adds `Configure Paths` navigation to Settings and per-provider
`Explain` actions backed by canonical `forge capabilities explain` JSON using
the same persisted path context as environment scanning.

Gate 352 renders explanation target status, description, next actions, related
capabilities, and grouped evidence directly in the app. Full output remains
under `Advanced Explanation JSON`.

Gate 353 refreshes the local app and unsigned installer with Gates 349-352.
The complete published-app Doctor workflow passed UI Automation regression,
including Settings navigation and structured provider explanations.

Gate 354 adds first-run routing to Settings when local settings are absent,
live core/tool path readiness counts, and a local `Save & Scan` workflow that
persists settings before invoking the existing deterministic scan.

Gate 355 validates project, game, and Data roots as existing directories and
supplied MO2, GECK, and xEdit paths as existing files. Drafts may still save,
but invalid inputs block Save & Scan with explicit local messages.

Gate 356 adds non-blocking warnings for game roots under Program Files and for
configured paths at or above the inferred 240-character pressure threshold.

Gate 357 adds explicit existing-only `Use Game\\Data` derivation. Missing
derived directories are refused without changing the current Data root or
saving settings.

Gate 358 refreshes the local app and unsigned installer with Gates 354-357.
The guarded first-run regression passed readiness, validation, warnings,
derivation, draft blocking, valid Save & Scan, and cleanup assertions.

Gate 359 adds a `New Project` surface over canonical `forge init`. Users select
one of the four documented templates, preview the exact dry-run plan, and can
create only while that preview still matches the current inputs. Existing
planned paths remain refused. Published-app automation verified no-write
preview, all eight scaffold files, refusal without mutation, and cleanup.

Gate 360 runs canonical `forge validate` immediately after successful project
creation, shares its structured result with the existing validation views and
Mod Builder output, and opens the created project in Mod Builder. The handoff
does not persist settings or add files beyond the eight-file init scaffold.

Gate 361 refreshes the unsigned installer and validates the complete workflow
from an isolated installed copy. Preview, eight-file creation, post-create
validation, Mod Builder handoff, and uninstall passed with no residual project,
install, registry, process, or settings state.

Gate 362 audits template semantics. All four currently produce the same valid
eight-file baseline scaffold, so New Project now states that specialization is
not yet defined instead of implying different framework, quest-pack, or
docs-only contents.

Gate 363 adds ordered canonical next-step commands to `forge init` JSON and
human output. The app's bundled backend is refreshed with that contract; init
still does not execute the guidance commands itself.

Gate 364 exposes capability scan and docs actions in Mod Builder only after a
successful init result advertises their exact canonical commands. The actions
remain user-triggered; capability evidence updates existing Doctor views and
docs output stays under the selected project's `generated/docs` tree.

Gate 365 renders canonical docs summary counts and the resolved generated docs
path in Mod Builder. Open Docs Folder remains disabled until successful JSON
evidence reports the exact selected-project `generated/docs` directory and that
directory exists.

Gate 366 adds a read-only Docs Index tab to Mod Builder. It strictly parses the
generated reference index and groups schema, registry, rule, capability,
provider, and command entries while preserving raw command output separately.

Gate 367 maps every selectable index entry to its canonical generated Markdown
reference array entry. The detail view enables Open Reference only for an
existing `.md` file contained by the selected project's generated docs root.

Gate 368 previews the selected generated Markdown reference as uninterpreted
plain text in a read-only in-app control after the same mapping, containment,
extension, and existence checks pass.

Gate 369 adds case-insensitive in-memory filtering across reference metadata,
with match counts, empty-section removal, stale-selection clearing, and full
restoration without rereading or modifying generated files.

Gate 370 adds the first real source-authoring workflow. MCM Author creates a
minimal toggle registry, updates only required manifest/dependency declarations,
then validates and generates deterministic MCM Extender JSON through Forge.
Existing MCM source is refused rather than overwritten.

Gate 371 adds preview-gated append editing for an existing MCM registry. The
preview binds current source bytes and proposed setting data; changed inputs or
source refuse save until previewed again, and successful append validates and
regenerates.

MSIX remains a later option after signing, package identity, and update policy
are settled. WiX/MSI remains a later option if enterprise MSI governance becomes
necessary.

## Open Implementation Checks

- Gate 461 desktop Release Candidate workspace implementation and regression.
- Code signing and update channel.
- Final Heat Restricted Asset status before public release.
- Seat/license coverage for every contributor who uses the Heat asset source.
- Third-party attribution bundle.
- CI path that does not make Heat mandatory for CLI/core validation.
