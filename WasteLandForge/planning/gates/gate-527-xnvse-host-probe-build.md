# Gate 527 - xNVSE GECK Host Probe Build

Status: Complete - build and static verification only
Phase: post-v0.1 provider discovery
Decision base: ADR-004, ADR-008, ADR-009, ADR-013, R005, R009, and Gates
483-484 and 520-526

## Goal

Acquire the exact Gate 526 toolchain, implement the minimal Forge-owned xNVSE
GECK host probe, compile it against the pinned external xNVSE 6.4.4 source, and
produce static/provenance evidence without staging or executing the probe.

## Toolchain result

The exact upstream-declared platform toolset is now available:

| Evidence | Value |
| --- | --- |
| Visual Studio instance | Community 2026 `18.7.11925.98` |
| Component | `Microsoft.VisualStudio.Component.VC.14.44.17.14.x86.x64` |
| Platform toolset | `v143` |
| MSVC tools | `14.44.35207` |
| Compiler SHA-256 | `7e970b6b42e87a5b4b4e4ee41034cf20f3573d948a4a92055d1b1bbe5e473cb1` |
| Reboot required | No |

The component was added to the existing Visual Studio instance through the
installer configuration-file modify path. Independent `vswhere` component
resolution, the Win32 `v143` platform-toolset files, and the 14.44 x86 compiler
were all present after installation.

## Implementation

The new Forge-owned integration contains:

- a single Win32 `Release GECK` native project using `v143` and the static
  multithreaded CRT;
- `NVSEPlugin_Query` and `NVSEPlugin_Load` as its only exports;
- editor-mode, exact xNVSE 6.4.4, minimum GECK 1.4.0.518, and launch-approval
  refusal checks;
- SHA-256-bound JSON observations beneath private
  `%LOCALAPPDATA%/WastelandForge/GeckProbe/` if a later gate executes it;
- compile-time `launchAuthorized=false` and
  `approvedProbeRunId=build-only-unapproved` controls;
- no xNVSE interface registration, record APIs, process launch, UI input,
  network access, or game/MO2/project writes.

The build helper requires a clean external xNVSE checkout and verifies before
every build:

| xNVSE evidence | Required value |
| --- | --- |
| Version | `6.4.4` |
| Commit | `694cdde6cbfa5e75afa661df587c73e8f0f6f441` |
| Tree | `e80453c217027f47979d2dfca03665de0a93fa6f` |

No xNVSE source or binary is copied into the repository or generated probe
folder.

## Build evidence

The local ignored build is under `generated/geck-probe/`:

| Output evidence | Value |
| --- | --- |
| DLL | `bin/WastelandForge.GeckProbe.dll` |
| Length | 123,904 bytes |
| SHA-256 | `baa44a424eaa335341d2f35511f07ffe6533c2336b5d38da561cc9e29888c2b2` |
| PE machine | x86 (`0x014C`) |
| Exports | `NVSEPlugin_Load`, `NVSEPlugin_Query` |
| DLL dependencies | `ADVAPI32.DLL`, `KERNEL32.DLL` |
| Repeat build | Byte-identical SHA-256 |

`build-manifest.json` binds the clean external source commit/tree and header
digests, exact compiler/MSBuild identities, Forge-owned input digests, DLL
length/digest, static reports, exports, architecture, dependencies, and all
no-execution/no-mutation flags.

## Validation

- Passed exact component discovery through `vswhere`.
- Passed physical `v143` platform-toolset and MSVC 14.44 compiler checks.
- Passed clean xNVSE commit/tree verification before each build.
- Passed Win32 `Release GECK` compilation with the pinned 14.44 tools.
- Passed `dumpbin` architecture, export, and dependency inspection.
- Passed Forge-owned forbidden API-token inspection.
- Passed `Test-GeckProbeBuild.ps1` manifest, PE, source-boundary, and native
  project regression.
- Passed a second clean build with the same DLL SHA-256.

Development-time failures were not treated as passing evidence. The first
installer command was a no-op; the configuration-file path completed the
installation. The first compile exposed duplicate `PATH`/`Path` process
variables and the next exposed missing upstream forced-include/configuration
settings. The checked-in build process now launches MSBuild with a normalized
child environment and matches the upstream GECK compile settings.

## Boundary result

Gate 527 proves only that the exact Forge-owned build-only xNVSE module compiles
and has the statically bounded shape required by Gate 526. It does not prove
that xNVSE discovers it, MO2 exposes it, GECK loads it, the observation writer
runs, or the editor remains stable.

No DLL was copied into physical game Data, MO2 mods, MO2 Overwrite, or an MO2
profile. No MO2/GECK/xEdit process ran. No runtime observation was produced. No
ESP/ESM was inspected or mutated.

## External state changed

- Added the exact MSVC v143 14.44 x64/x86 build-tools component to the existing
  Visual Studio Community 2026 installation.
- Created ignored generated native build outputs and static reports under
  `generated/geck-probe/`.

The game root, game Data, MO2, GECK/xNVSE installation, project source data,
and all ESP/ESM files were not changed.

## Next route

Gate 528: define the separately approved no-mutation host-probe staging and
live-smoke contract. It must bind a maintainer-selected FNV MO2 instance and
pre-existing test profile, an exact virtual probe-mod tree, an approved run ID,
pre/post protected-file hashes, expected provider visibility, shutdown and
recovery steps, and a digest-bound launch preview. Gate 528 remains planning
and preflight only; it must not stage the DLL or launch GECK.
