# Gate 526 - GECK Provider Host-Probe Baseline Decision

Status: Complete - build-only probe route selected
Phase: post-v0.1 architecture and planning
Decision base: ADR-004, ADR-008, ADR-013, R005, R009, and Gates 483-484,
520-525

## Goal

Resolve Gate 525's provider-baseline fork without weakening the authoring
correctness model. Select the exact environment and dependency boundary for a
no-mutation editor host probe, while keeping record authoring and GECK launch
unauthorized.

## Decision

Select the xNVSE-only route for the first **host-loading probe**.

This is not the final authoring-provider baseline. It proves only whether a
Forge-owned native module can be loaded through the exact pinned xNVSE editor
host. GECK Extender remains a preferred future editor-environment enhancement
and must be resolved independently before it can be claimed as part of an
authoring provider.

The selected probe baseline is:

| Element | Decision |
| --- | --- |
| Game/editor | Physical `Geck.exe` 1.4.0.518, exact Gate 523 digest |
| Plugin host | xNVSE 6.4.4, source/release/installed identities pinned by Gate 525 |
| Host mode | GECK editor mode only; runtime mode refused |
| GECK Extender | Optional and unverified; not required or claimed by the host probe |
| Build platform | Win32 |
| Build configuration | Release GECK |
| Toolset | Upstream-declared `v143`; no retargeting in the first probe |
| Effective plugin path | MO2 virtual `Data/NVSE/Plugins` only |
| Physical Data writes | Forbidden |
| Observation root | Private `%LOCALAPPDATA%/WastelandForge/GeckProbe/` |
| Record APIs | Forbidden |

## Evidence classification

- **Documented:** R009 and upstream xNVSE evidence establish GECK-only native
  plugin builds and editor/runtime discrimination.
- **Documented:** Gate 525 pins xNVSE source/release/installed binaries and
  confirms the upstream example declares Win32, `Release GECK`, editor
  1.4.0.518, and `v143`.
- **Documented:** R005 and Gate 483 establish that MO2 changes effective file
  visibility and provides an explicitly selected profile-aware VFS launch.
- **Inferred:** xNVSE alone is sufficient for a host-loading probe because the
  probe tests xNVSE module discovery and entrypoint invocation, not editor
  enhancement or record authoring.
- **Open:** no live local evidence yet proves the exact probe loads, the
  selected MO2 profile exposes it, or the editor remains stable after loading.

## Why GECK Extender is not a probe prerequisite

Gate 525 found no published source corresponding to GECK Extender 0.52. Making
0.52 a host-probe prerequisite would prevent testing the independently pinned
xNVSE host and would not provide an authoring API contract.

Removing GECK Extender from this probe does not:

- remove it from the preferred future authoring environment;
- claim that xNVSE alone can create or save records;
- authorize source 0.31 with binaries 0.52;
- authorize GECK executable replacement or extender installation;
- satisfy ADR-013's execution conditions.

## MO2 isolation decision

xNVSE plugins are Data-scoped providers, but Gate 524 forbids physical game
`Data` writes. The host probe must therefore be made visible through a
user-controlled MO2 profile. The separate probe mod's physical content tree is:

```text
NVSE/Plugins/WastelandForge.GeckProbe.dll
```

MO2 exposes that file to GECK at the effective virtual path
`Data/NVSE/Plugins/WastelandForge.GeckProbe.dll`.

Forge must not create, rename, select, enable, reorder, or otherwise mutate an
MO2 profile or mod. A future live smoke requires a pre-existing operator-chosen
test profile and separate approval for staging/enabling the probe mod.

Launch must use the existing Gate 483/484 companion path:

- exact GECK executable and empty arguments;
- current MO2 instance and explicitly selected profile;
- previewed request digest and explicit user action inside MO2;
- no undocumented `ModOrganizer.exe` arguments;
- process-created receipt treated only as process evidence.

## Probe behavior contract

The first probe source may implement only:

- `NVSEPlugin_Query`;
- `NVSEPlugin_Load`;
- editor-mode and minimum-editor-version refusal;
- a versioned identity observation written under the private observation root;
- deterministic diagnostic logging for query/load success or refusal.

It must not:

- register scripting commands or events;
- call `TESForm`, active-file, cell, reference, inventory, save, or mutation
  functions;
- inspect or modify plugin records;
- send UI input or use window coordinates;
- write beneath game root, game `Data`, MO2 mods, MO2 Overwrite, or project
  source;
- launch a process, network client, or child tool;
- bundle xNVSE, GECK Extender, GECK, Bethesda files, or third-party source.

The observation must bind probe version/digest, xNVSE interface/editor version,
process ID, query/load result, UTC, and the approved probe run ID. It is runtime
evidence, never canonical truth or authoring success.

## Build gate

Gate 527 may implement and compile the probe only after `v143` is available.
It must:

1. Keep upstream xNVSE source external and verify commit/tree digests before
   every build.
2. Build Win32 `Release GECK` without changing upstream projects or toolsets.
3. Keep native intermediates and binaries under ignored/generated build roots.
4. Inspect the resulting DLL architecture and exported query/load symbols.
5. Produce a local build manifest with source, toolchain, output length, and
   SHA-256 evidence.
6. Use synthetic/static tests only; do not copy the DLL into game Data or MO2.

Gate 527 must stop before probe staging, MO2 profile use, GECK launch, runtime
observation, or record mutation.

## Live-smoke prerequisites

A later, separately authorized live gate requires all of:

- successful Gate 527 build and static verification;
- explicit operator-selected MO2 instance and pre-existing test profile;
- exact virtual probe-mod tree preview and separate staging approval;
- pre/post hashes for physical ESP/ESM and physical NVSE plugin files;
- exact GECK/xNVSE/probe/MO2 identities;
- empty GECK arguments and approved MO2 launch request;
- manual shutdown/recovery plan;
- refusal if GECK Extender or any unexpected provider is effectively visible.

A successful live smoke may claim only that the exact probe loaded and emitted
the expected observation without detected protected-file drift.

## Validation

- Rechecked Gate 525's pinned xNVSE source and toolset evidence.
- Rechecked the Gate 483/484 profile-aware MO2 launch boundary.
- Confirmed current Forge settings do not identify a profile; none was guessed
  or selected.
- No source, native project, dependency, probe binary, MO2 mod, profile, game
  file, or process was created or changed by this gate.

## Next route

Gate 527: acquire the exact `v143` build tools, implement the minimal
Forge-owned xNVSE host probe, compile it against the pinned external xNVSE
6.4.4 source, statically verify the DLL, and emit a local build manifest. Stop
before installation, MO2 staging, or GECK launch.
