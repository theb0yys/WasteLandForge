# Gate 552 - GECK Provider API Discovery and Compatibility Proof

Status: Complete - host compatibility revalidated; authoring API compatibility not established
Phase: post-v0.1 provider discovery
Decision base: ADR-004, ADR-008, ADR-009, ADR-013, R009, and Gates 520-534

## Goal

Re-open the bounded GECK provider lane against exact pinned source and current
upstream evidence. Determine whether Forge can implement the first-slice
`CONT`/`REFR` writer through a supported editor API, without launching GECK,
calling record APIs, writing plugin bytes, or treating reverse-engineered
engine declarations as a stable provider contract.

## Evidence classification

- **Documented:** ADR-013 permits approval-bound authoring through a bounded
  external provider, but requires provider and verifier compatibility evidence
  before execution.
- **Documented:** Gate 527 built a deterministic Win32 xNVSE GECK host probe
  whose only exports are `NVSEPlugin_Query` and `NVSEPlugin_Load`.
- **Observed:** the pinned xNVSE 6.4.4 checkout remains clean at commit
  `694cdde6cbfa5e75afa661df587c73e8f0f6f441` and tree
  `e80453c217027f47979d2dfca03665de0a93fa6f`.
- **Observed:** pinned `PluginAPI.h` is 68,128 bytes with SHA-256
  `b0dcc54c04a796cd0f7c452d5e6cb7d9e284d5373f2db8f847483a7e90c39168`.
- **Observed:** `PluginAPI.h` exposes the plugin host, command registration,
  interface query, messaging, console, and script-extension surfaces. It does
  not expose editor operations for active-file selection, form creation, cell
  placement, or plugin save.
- **Observed:** xNVSE's current public `PluginAPI.h` also contains no
  `CreateForm` authoring method. The latest published release is 6.4.8, while
  the installed and reproducibly pinned Forge provider remains 6.4.4.
- **Observed:** current GECK Extender 0.52 binaries remain newer than the
  separately published 2020 source package; no matching public source/API
  contract was found for the selected binary.
- **Open:** a supported GECK-side authoring API for the complete first slice
  has not been identified.

Primary upstream evidence:

- [xNVSE public plugin API](https://github.com/xNVSE/NVSE/blob/master/nvse/nvse/PluginAPI.h)
- [xNVSE releases](https://github.com/xNVSE/NVSE/releases)
- [GECK Extender distribution](https://www.nexusmods.com/newvegas/mods/64888?tab=files)

## Compatibility matrix

| Required operation | Pinned public API result | Compatibility |
| --- | --- | --- |
| Load a Win32 module in GECK and identify editor mode | `NVSEPlugin_Query`, `NVSEPlugin_Load`, and `isEditor` | Build-compatible; runtime smoke remains separate |
| Observe editor data-load completion | No documented editor-readiness event | Not established |
| Enumerate loaded masters and plugins | No editor provider method | Not established |
| Read or select the active plugin | No editor provider method | Not established |
| Create a `CONT` base record | No editor provider method | Not established |
| Populate base-container inventory | Runtime inventory helpers are not an editor authoring contract | Not established |
| Resolve a cell and create a placed `REFR` | No editor provider method | Not established |
| Apply transform, ownership, persistence, and encounter policy | No editor provider method | Not established |
| Save the active plugin and observe success/failure | No editor provider method | Not established |

## Internal-symbol boundary

The pinned xNVSE source contains reverse-engineered engine declarations such
as `TESForm::AppendForm`, `TESForm::SaveForm`, `TESForm::CreateForm`,
`TESForm::MarkAsModified`, `CreateFormInstance`, `TESObjectCONT`, and
`TESObjectREFR`. They do not satisfy the provider contract because:

- they are not exported through `NVSEInterface` or another documented
  editor-facing interface;
- `CreateForm` includes unknown `void*` arguments and undocumented map/form-ID
  ownership;
- the declarations do not establish active-file routing, editor transaction
  semantics, UI/main-thread requirements, rollback, or save completion;
- compiling against a reverse-engineered declaration would prove only symbol
  compatibility, not safe or deterministic authoring behavior.

Gate 552 therefore does not convert these declarations into Forge APIs and
does not authorize an address table, vtable call, binary patch, UI-coordinate
fallback, keystroke macro, or raw ESP writer.

## Proof performed

- Revalidated the ignored Gate 527 build manifest and probe DLL through
  `eng/Test-GeckProbeBuild.ps1`.
- Revalidated the exact xNVSE commit, tree, clean state, public API header
  length, and SHA-256.
- Compared every Gate 520 first-slice operation with the pinned public API.
- Checked current upstream xNVSE public API and release evidence for a newer
  supported editor authoring surface.
- Rechecked the current GECK Extender distribution/source relationship.

The proof is deliberately static and no-mutation. It establishes that the
known supported host contract is insufficient for the writer; it does not
claim that no private or reverse-engineered implementation could exist.

## Decision

Provider-host compatibility is proven only at the existing build/static
boundary. Provider authoring compatibility is **not established**, so the
bounded GECK writer remains blocked by ADR-013 and Gate 524.

Implementing a writer now would require at least one unsupported assumption
about GECK internals. Forge must not disguise that assumption as a completed
provider lane or expose an execution button that cannot meet the approved
correctness model.

## Validation

- Passed: `eng/Test-GeckProbeBuild.ps1`.
- Passed: pinned xNVSE commit/tree/clean-state verification.
- Passed: pinned `PluginAPI.h` length and SHA-256 verification.
- Passed: required-operation/public-interface comparison.
- Not run: native rebuild; the existing byte-identical Gate 527 artifact was
  unchanged and passed its static regression.
- Not run: GECK, xNVSE runtime, MO2, FNVEdit, xEdit, game, or provider process.
- Not performed: staging, installation, plugin parsing, record mutation, save,
  game Data write, load-order change, or external-state cleanup.

## Next route

Gate 553 is the independent verifier compatibility proof. It must resume Gate
534's evidence requirements: an explicitly supplied redistributable synthetic
FNV plugin, exact FNVEdit identity, matching Gate 533 observer bundle, contained
raw-observation destination, immutable pre/post plugin digests, and explicit
approval of the exact execution preview. It must run read-only and once only.

A successful Gate 553 would prove the verifier side only. Writer implementation
would remain blocked until a supported authoring provider API is supplied and
passes its own no-mutation and bounded-operation compatibility gates.
