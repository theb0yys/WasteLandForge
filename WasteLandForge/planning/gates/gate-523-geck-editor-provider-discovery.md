# Gate 523 - GECK Editor Provider Discovery

Status: Complete - insufficient evidence for execution
Phase: post-v0.1 research
Decision base: ADR-004, ADR-008, ADR-013, R009, and Gates 520-522

## Goal

Perform the separately authorized, read-only local GECK/xNVSE/GECK Extender
provider-discovery spike required by Gate 520. Record exact local identities,
supported public APIs, missing authoring operations, and whether Gate 524 may
define bounded real execution.

This gate did not launch GECK, install or update a provider, write game data,
or mutate an ESP/ESM.

## Evidence classification

- **Documented:** xNVSE supports `Debug GECK` and `Release GECK` plugin builds,
  exposes editor/runtime identity during plugin query, and provides plugin
  query/load, interface query, command registration, and messaging surfaces.
- **Documented:** ADR-013 and R009 prefer an editor-resident provider over
  UIA-only mutation, but require exact local API discovery before execution.
- **Observed:** the configured game root contains GECK 1.4.0.518 and xNVSE
  6.4.4, including `nvse_editor_1_4.dll`.
- **Observed:** no GECK Extender/GaryHax marker was found in the bounded game,
  NVSE plugin, configured MO2 mod, or supporting-tool roots.
- **Observed:** GECK was not running, so no runtime module or readiness evidence
  was available.
- **Open:** the inspected public xNVSE interface does not establish supported
  high-level APIs for loading editor data, creating records, placing
  references, selecting the active plugin, saving, or observing save success.

Primary API evidence:

- [xNVSE development configurations](https://github.com/xNVSE/NVSE/blob/master/DEVELOPMENT.md)
- [xNVSE plugin example](https://github.com/xNVSE/NVSE/blob/master/nvse_plugin_example/main.cpp)
- [xNVSE public plugin interface](https://github.com/xNVSE/NVSE/blob/master/nvse/nvse/PluginAPI.h)
- [xNVSE reverse-engineered form declarations](https://github.com/xNVSE/NVSE/blob/master/nvse/nvse/GameForms.h)
- [GECK Extender distribution page](https://www.nexusmods.com/newvegas/mods/64888)

## Bounded local discovery

The read-only search used the configured physical game root, its
`Data/NVSE/Plugins` directory, the configured MO2 mods root, and the configured
supporting-tools root. It did not perform a drive-wide search.

| Component | Observed identity | SHA-256 |
| --- | --- | --- |
| GECK | `Geck.exe` 1.4.0.518, 12,949,504 bytes | `F66A3625E3F65C5CF1ECA1470C6E3B7A9CE1B11ADCEF6C0E8B04C4644FE4E8C7` |
| Fallout: New Vegas | `FalloutNV.exe` 1.4.0.525, 16,549,704 bytes | `518C87F58A6C4D9826E9EF8FBB7F4213882FA70822675610D45AEA2464502A57` |
| xNVSE loader | `nvse_loader.exe` 6.4.4, 155,136 bytes | `1EAE1DB6E68ADE6DDA04E7DF589850B7102808101597D1A4CCC3B03E2B0B946D` |
| xNVSE runtime | `nvse_1_4.dll` 6.4.4, 1,335,296 bytes | `A8DCB0C05F4089E37A3071B28E5419E8FC1B59D8F2A4762D6AB9C93AC49211F5` |
| xNVSE editor loader | `nvse_editor_1_4.dll` 6.4.4, 716,288 bytes | `12EB5B9BDA9F3EC1CCDBA6D9857F1FD2ECD637906D61B5EDF66336BB2642B56B` |
| xEdit | `FNVEdit.exe` 4.1.5.0, 24,620,544 bytes | `895CE936FEAD6DA9B6A0DDE4B1E88C73332F5EC0ED319ADD03328FD89CB040D9` |

An xNVSE 6.4.8 distribution is present under the configured supporting-tools
root, but it is not the installed game-root version. It was not copied,
installed, or treated as provider evidence.

The GECK executable hash alone cannot prove whether the executable has been
replaced or patched for a particular GECK Extender release. No matching
extender plugin, configuration, source package, or runtime-loaded module was
found, so GECK Extender compatibility is unverified rather than absent with
absolute certainty.

## API coverage result

| Required provider operation | Evidence result |
| --- | --- |
| Load an editor plugin and distinguish GECK from runtime | Supported by xNVSE plugin query/load and `isEditor` |
| Register a provider command or receive documented messages | Supported by the public xNVSE interface |
| Observe GECK data-load completion/readiness | Not established |
| Enumerate loaded masters/plugins | Not established |
| Read or select the active plugin | Not established |
| Create and populate a `CONT` record | Not established |
| Select/load a cell and place a `REFR` | Not established |
| Set transform, ownership, persistence, or encounter policy | Not established |
| Save a new plugin and observe completion/failure | Not established |

`GameForms.h` contains reverse-engineered `TESForm` virtual declarations such
as `CreateForm`, `SaveForm`, and `MarkAsModified`. They are version-coupled
editor internals, include unknown parameters or behavior, and are not exposed
as a supported high-level contract by `NVSEInterface`. Their presence is not
sufficient evidence to authorize a correctness-critical authoring provider.

Binary string observations such as `NVSEPlugin_Query`, `NVSEPlugin_Load`, and
`CreateForm` likewise establish only symbol/text presence, not callable
semantics, compatibility, or safe authoring behavior.

## Decision

Gate 523 succeeds as a discovery gate and fails the sufficiency condition for
real execution:

- an editor-resident xNVSE plugin is a technically supported provider host;
- the exact authoring API needed by the first `CONT`/`REFR` slice remains
  unproven;
- the configured local provider stack does not contain a verifiable GECK
  Extender installation;
- Gate 524 must not define or enable bounded real GECK mutation from this
  evidence.

No fallback to UI coordinates, keystroke automation, raw ESP writing, or
invented GECK APIs is authorized.

## Validation

- Read configured Forge tool identities without changing settings.
- Captured file version, length, and SHA-256 for the bounded provider set.
- Searched bounded configured roots for GECK Extender/GaryHax markers.
- Confirmed GECK was not running; no process was started.
- Compared the required operation set with the documented public xNVSE API.
- No source code, schema, fixture, executable, provider, game data, or plugin
  bytes changed.

## Next route

Gate 524 remains blocked by Gate 520's sufficiency condition. The next work
must be a separately scoped provider-feasibility remediation gate that pins the
exact xNVSE source matching the selected local version, obtains and identifies
the intended GECK Extender source/binary contract, and proves a no-mutation
editor-resident discovery probe before any record-authoring operation is
designed or executed.

