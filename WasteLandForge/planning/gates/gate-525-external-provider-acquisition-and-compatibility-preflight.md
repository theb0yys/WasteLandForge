# Gate 525 - External Provider Acquisition and Compatibility Preflight

Status: Complete - provider lane blocked
Phase: post-v0.1 research and planning
Decision base: ADR-004, ADR-008, ADR-013, R009, and Gates 520-524

## Goal

Acquire selected upstream provider evidence outside the repository, correlate it
with the configured local installation, and test the unmodified no-mutation
build prerequisite. Either produce matching digest evidence and a reproducible
probe build path or record the provider lane blocked.

This gate did not install a provider, launch GECK, build a Forge probe, invoke
record APIs, or write game/plugin data.

## xNVSE acquisition result

The official xNVSE 6.4.4 source was cloned to a temporary preflight directory
at the upstream release commit:

| Evidence | Value |
| --- | --- |
| Release/tag | `6.4.4` |
| Commit | `694cdde6cbfa5e75afa661df587c73e8f0f6f441` |
| Source tree | `e80453c217027f47979d2dfca03665de0a93fa6f` |
| Deterministic source TAR length | 3,870,720 bytes |
| Deterministic source TAR SHA-256 | `51140056A76777DDCD76DF9A493E19F57C1A5ECE7C229ACCAADDA02FC9DCA30F` |
| Release archive length | 4,770,280 bytes |
| Release archive SHA-256 | `854B506349A39BA1D472E82DEF4F465D989692CEA5593E626685AC128FD164A6` |

The temporary source checkout remained clean after inspection and preflight.
No third-party source or binary was copied into WastelandForge.

## Installed binary correlation

All selected official release binaries match the configured physical
installation exactly:

| File | Length | SHA-256 | Match |
| --- | ---: | --- | --- |
| `nvse_1_4.dll` | 1,335,296 | `A8DCB0C05F4089E37A3071B28E5419E8FC1B59D8F2A4762D6AB9C93AC49211F5` | Exact |
| `nvse_editor_1_4.dll` | 716,288 | `12EB5B9BDA9F3EC1CCDBA6D9857F1FD2ECD637906D61B5EDF66336BB2642B56B` | Exact |
| `nvse_loader.exe` | 155,136 | `1EAE1DB6E68ADE6DDA04E7DF589850B7102808101597D1A4CCC3B03E2B0B946D` | Exact |
| `nvse_steam_loader.dll` | 37,888 | `4F695C2B4A4AD8DB605E5225EF33C7A6A8C0A9679937C64F714293394D169690` | Exact |

This closes the Gate 524 xNVSE release-asset/installed-binary provenance gap.
It does not prove a GECK authoring API.

## Unmodified build preflight

The pinned source provides a `Release GECK|Win32` example that:

- exports `NVSEPlugin_Query` and `NVSEPlugin_Load`;
- requires `isEditor`;
- rejects an editor older than `CS_VERSION_1_4_0_518`;
- targets Windows SDK `10.0` and platform toolset `v143`.

MSBuild 18.7.8 with the installed native workload compiled the referenced
`common_vc9` project using its declared `v145` toolset, producing only a
temporary static library. The example then stopped with `MSB8020` because the
declared Visual Studio 2022 `v143` toolset is not installed.

No project was edited or retargeted. The build result is therefore a failed
compatibility preflight, not evidence that `v145` is a valid substitution.

## GECK Extender acquisition result

The authoritative Nexus files page exposes these current identities:

| Artifact | Version | File ID | Published evidence |
| --- | --- | --- | --- |
| Modified GECK executable archive | 0.52 | `1000176528` | 13 June 2026, 5.2 MB |
| GECK Extender NVSE plugin archive | 0.52 | `1000176527` | 13 June 2026, 3.1 MB |
| GECK Extender configuration | 0.46 | `1000135537` | 3 July 2024, 8 KB |
| GECK Extender source | 0.31 | `1000059598` | 23 February 2020, 1.6 MB |

The page describes the 0.31 source as compiled with VS2019 using the VS2013
toolset. It does not publish source corresponding to 0.52. The available source
therefore cannot satisfy Gate 524's current binary/source correspondence rule.

The Nexus session was not authenticated for downloads. No GECK Extender file
was downloaded. Authentication would not resolve the more important 0.31 versus
0.52 source-version mismatch.

Primary external evidence:

- [xNVSE 6.4.4 release](https://github.com/xNVSE/NVSE/releases/tag/6.4.4)
- [xNVSE source](https://github.com/xNVSE/NVSE/tree/6.4.4)
- [GECK Extender files](https://www.nexusmods.com/newvegas/mods/64888?tab=files)

## Decision

The external-provider preflight is blocked from advancing to a no-mutation
editor probe:

- xNVSE 6.4.4 source, release assets, and installed binaries are now pinned;
- the unmodified xNVSE GECK example cannot build without `v143`;
- current GECK Extender 0.52 has no corresponding source listed by its
  authoritative distribution page;
- the older GECK Extender source requests a separate unavailable legacy
  toolset and cannot establish 0.52 compatibility;
- no evidence authorizes substituting toolsets, using source 0.31 with binaries
  0.52, or removing GECK Extender from the provider requirement.

No probe DLL, GECK launch preview, installation plan, or record-authoring
execution contract is authorized from this result.

## External state changed

Temporary preflight-only artifacts were created under the user temporary
directory:

- xNVSE 6.4.4 source checkout;
- official xNVSE 6.4.4 release archive and extracted release files;
- deterministic source TAR;
- ignored MSBuild intermediate/output files from the failed build preflight.

They are not repository inputs, generated Forge outputs, release artifacts, or
installed providers. The game root, game `Data`, MO2, and WastelandForge trees
received no third-party files.

## Validation

- Verified the full source commit and source tree.
- SHA-256 compared four official release binaries with four installed files;
  all matched exactly.
- Ran the unmodified `Release GECK|Win32` example build.
- Passed: shared native library compilation.
- Failed: plugin example with `MSB8020`, missing `v143` toolset.
- Inspected the current GECK Extender file IDs, versions, dates, and published
  source relationship without downloading or installing them.
- No GECK/xEdit process ran and no ESP/ESM hash or byte content changed.

## Next route

Gate 526 must be an explicit provider-baseline decision, not execution. It must
choose one evidence-backed route:

1. require matching GECK Extender 0.52 source from its maintainer and install
   the exact required toolchains before retrying the no-mutation probe; or
2. amend the first host probe to xNVSE-only, while keeping GECK Extender as an
   environment enhancement and preserving the ban on record mutation until
   exact authoring APIs are independently proven.

Until that decision is recorded, the GECK authoring provider lane remains
blocked.

