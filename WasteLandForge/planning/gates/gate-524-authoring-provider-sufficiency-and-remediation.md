# Gate 524 - Authoring Provider Sufficiency and Remediation

Status: Complete - execution not authorized
Phase: post-v0.1 research and planning
Decision base: ADR-004, ADR-008, ADR-013, R009, and Gates 520-523

## Goal

Apply Gate 520's sufficiency condition to the completed verifier and provider
discovery work. When the condition is not met, define the exact bounded
remediation and no-mutation probe required before any GECK authoring execution
contract can be considered.

This gate does not define record mutation, add an execution command, launch
GECK, install a provider, or write an ESP/ESM.

## Sufficiency decision

| Gate 520 prerequisite | Result |
| --- | --- |
| Deterministic semantic verifier contract | Satisfied synthetically by Gate 522 |
| Real xEdit/FNV verifier compatibility | Open; no real report has been produced |
| Supported editor-resident provider host | Partially satisfied by xNVSE's documented GECK plugin builds |
| Supported high-level `CONT`/`REFR` authoring API | Not established by Gate 523 |
| Exact local GECK Extender identity and source/binary contract | Not established |
| No-mutation editor-resident probe | Not built or run |

Gate 520 requires both Gate 522 and Gate 523 to provide sufficient evidence.
That condition is false. Bounded authoring execution and the former Gate 525
writer route remain unauthorized.

## Evidence classification

- **Documented:** xNVSE release 6.4.4 is tagged at commit `694cdde`; the
  configured game-root binaries report the same version.
- **Open:** version equality alone does not prove that the installed binary
  bytes were produced by that tagged source revision.
- **Documented:** xNVSE supports GECK-specific plugin configurations and an
  editor/runtime discriminator, but its public plugin interface does not
  provide the required high-level authoring contract.
- **Observed:** the configured local xNVSE 6.4.8 distribution contains binaries
  and symbols only; no headers, C/C++ source, solution, or project files were
  found.
- **Observed:** Visual Studio Community 2026 has the x86/x64 C++ workload and
  `v145` platform toolset, but no `v120` or `v120_xp` toolset marker.
- **Documented:** the current GECK Extender files page lists version 0.52
  binaries uploaded in June 2026, while its listed source package was uploaded
  in February 2020 and says it was compiled with VS2019 using the VS2013
  toolset.
- **Open:** no evidence proves that the listed 2020 source corresponds to the
  current 0.52 binaries, the configured `Geck.exe`, or an installed local
  extender plugin.

Primary external evidence:

- [xNVSE 6.4.4 release](https://github.com/xNVSE/NVSE/releases/tag/6.4.4)
- [xNVSE development configurations](https://github.com/xNVSE/NVSE/blob/master/DEVELOPMENT.md)
- [xNVSE public plugin interface](https://github.com/xNVSE/NVSE/blob/master/nvse/nvse/PluginAPI.h)
- [GECK Extender files and source listing](https://www.nexusmods.com/newvegas/mods/64888?tab=files)

## Required pinned inputs

A future probe gate must refuse to build or run until all of these inputs are
explicitly selected and digest-pinned:

1. An xNVSE source revision proven to match the selected installed editor
   loader. Tag `6.4.4` / commit `694cdde` is the candidate pin for the current
   physical installation, but its release asset must first be digest-correlated
   with the installed binaries. Alternatively, a separately approved update
   may supply matching source and binaries.
2. The intended GECK Extender executable, NVSE plugin, configuration, and
   source identities, including version, length, SHA-256, origin, and evidence
   that the source corresponds to those binaries.
3. A compatible 32-bit native toolchain and exact platform toolset. Toolset
   substitution is not permitted without a successful reproducible build and
   compatibility evidence.
4. An isolated probe output root outside the game `Data` tree and an explicit
   list of every file the probe may create.
5. Pre-run hashes for `Geck.exe`, xNVSE/GECK Extender provider files, and every
   ESP/ESM visible to the launch context.

Third-party source and binaries remain external dependencies. They must not be
committed to or redistributed by WastelandForge.

## No-mutation probe contract

The first editor-resident probe may establish provider hosting only. It must:

- build as a 32-bit xNVSE GECK plugin against the pinned source/toolchain;
- export only the documented xNVSE query/load entry points needed for loading;
- require editor mode and reject runtime mode;
- emit a versioned provider-identity/readiness observation only to the approved
  isolated probe output root;
- avoid `TESForm` creation, save, modification, cell, reference, inventory,
  active-file, and Data-write calls;
- add no script commands and perform no UI input or coordinate automation;
- run only after an exact launch preview and separate operator approval;
- compare all pre-run provider and ESP/ESM hashes after shutdown;
- classify a crash, modal, unexpected write, missing observation, or hash drift
  as failed or indeterminate, never successful;
- never claim authoring API compatibility from successful plugin loading.

The allowed success claim is limited to: the exact probe binary loaded through
the exact xNVSE editor host and emitted the expected identity observation
without detected plugin-byte changes.

## Stop conditions

The probe must not be implemented or launched when any of these remain true:

- selected source does not match selected provider binaries;
- the GECK Extender binary/source relationship is unknown;
- the required toolset is unavailable or substituted without evidence;
- any output path resolves under the game `Data` tree;
- the launch context or visible plugin set cannot be snapshotted;
- the approved observation contract would require reverse-engineered form
  mutation APIs;
- the operator has not separately approved the exact launch preview.

Current blockers are the unproven xNVSE source/binary correspondence, unknown
GECK Extender binary/source relationship, unavailable requested legacy
toolset, undefined approved probe output/launch context, and missing separate
operator launch approval. No probe was therefore created or run.

## Validation

- Confirmed the candidate xNVSE 6.4.4 source pin from the upstream release;
  binary/source correspondence remains unproven.
- Searched the configured supporting-tools xNVSE distribution for source/build
  markers; none were found.
- Confirmed a native Visual Studio workload is installed and `v120`/`v120_xp`
  is not present.
- Compared the current GECK Extender binary dates/version with its listed
  source package date and toolset statement.
- No network artifact was downloaded, no dependency was installed, no process
  was launched, and no game/provider/plugin file was changed.

## Next route

The next gate is an external-provider acquisition and compatibility preflight,
not authoring execution. It requires explicit authorization to obtain the
selected upstream source/provider artifacts and must either produce matching
digest evidence plus a reproducible no-mutation build plan or record the lane
blocked. GECK launch and probe installation remain separately authorized work.
