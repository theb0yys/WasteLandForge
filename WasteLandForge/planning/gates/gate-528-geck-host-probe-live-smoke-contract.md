# Gate 528 - GECK Host-Probe Staging and Live-Smoke Contract

Status: Complete - contract defined; execution blocked
Phase: post-v0.1 provider discovery
Decision base: ADR-004, ADR-008, ADR-009, ADR-013, R005, R009, and Gates
483-488 and 520-527

## Goal

Define the separately approved staging and no-mutation live-smoke contract for
the Gate 527 xNVSE GECK host probe. Perform only bounded read-only environment
preflight and stop before an authorized probe build, MO2 staging/profile use,
request creation, or GECK launch.

## Evidence classification

- **Documented:** Gate 526 requires a pre-existing operator-selected FNV MO2
  profile, exact virtual probe tree, protected-file hashes, exact provider
  identities, empty GECK arguments, explicit launch approval, refusal of
  unexpected effective providers, and a manual shutdown/recovery plan.
- **Documented:** Gates 483-487 provide a 15-minute digest-bound request,
  explicit profile selection inside the optional companion, the supported MO2
  `startApplication` boundary, handle cleanup, and process-created receipts.
- **Documented:** R005 requires physical scope and effective MO2 visibility to
  be evaluated separately and identifies MO2 `virtualFileTree()` as the
  supported read-only environment surface.
- **Documented:** Gate 527 produced a reproducible x86 host-probe DLL with only
  query/load exports, but that build has `launchAuthorized=false` and the run
  identity `build-only-unapproved`.
- **Inferred:** a probe-specific request context and read-only effective-tree
  snapshot are required because a normal GECK handoff does not bind the probe
  build, staging destination, expected providers, or protected baseline.
- **Open:** no local FNV MO2 instance/profile currently exists, and the
  effective provider set cannot be established without one.

## Current read-only preflight

Forge settings now identify a concrete FNV tool and mods-root pair:

| Evidence | Observed value |
| --- | --- |
| MO2 executable | `D:/downlods/Development folder/dev/FalloutNV/wasteland forge supporitng systems/MO2/ModOrganizer.exe` |
| MO2 version | `2.5.2` |
| MO2 length | 5,028,352 bytes |
| MO2 SHA-256 | `442B354A8F34754DA0048654C44D27F51628FEBA54CE46C3187CF58D6C43E622` |
| Configured mods root | `D:/downlods/Development folder/dev/FalloutNV/MODS/MO2` |
| Mods-root state | Existing, non-reparse, empty |
| Python support | `plugin_python/plugin_python.dll` present, 244,736 bytes |
| Python plugin SHA-256 | `6B08110C2E35F72B48D747D46859E7B4AE7445D7168008A23184119903055826` |
| Forge companion | Not installed in the configured MO2 plugin root |
| Portable MO2 INI | Missing beside `ModOrganizer.exe` |
| Portable profile root | Missing |
| FNV global instance/profile | Not found under the bounded LocalAppData instance root |

The existing LocalAppData MO2 instances remain Fallout 4 London-labelled and
are not valid substitutes for an FNV test instance.

The pinned game/editor identities still match Gate 523 exactly:

| Provider | SHA-256 |
| --- | --- |
| `Geck.exe` | `F66A3625E3F65C5CF1ECA1470C6E3B7A9CE1B11ADCEF6C0E8B04C4644FE4E8C7` |
| `FalloutNV.exe` | `518C87F58A6C4D9826E9EF8FBB7F4213882FA70822675610D45AEA2464502A57` |
| `nvse_loader.exe` | `1EAE1DB6E68ADE6DDA04E7DF589850B7102808101597D1A4CCC3B03E2B0B946D` |
| `nvse_1_4.dll` | `A8DCB0C05F4089E37A3071B28E5419E8FC1B59D8F2A4762D6AB9C93AC49211F5` |
| `nvse_editor_1_4.dll` | `12EB5B9BDA9F3EC1CCDBA6D9857F1FD2ECD637906D61B5EDF66336BB2642B56B` |

The current physical Data environment is not a clean host-probe environment:

| Protected set | File count | Aggregate SHA-256 |
| --- | ---: | --- |
| Direct physical ESP/ESM files | 295 | `a1a3d124d7c5d8d0b5d1d8d504f728394f88b11ffac73c534f3c9cf2a67d8fe4` |
| Recursive physical `Data/NVSE/Plugins` files | 410 | `9401ec11db857b8cd02210dc8412fc629780dca8312f7d4445038baa2f119448` |
| Native DLLs under physical `Data/NVSE/Plugins` | 53 | Per-file hashes inspected; effective visibility unresolved |

These aggregate values are read-only preflight observations, not reusable live
baselines. A live gate must recapture complete per-file manifests immediately
before launch and again after GECK exits. Physical presence alone does not prove
selected-profile visibility, so Gate 528 does not claim that all 53 DLLs would
load. It does require refusal unless the future effective-tree check proves the
exact allowed set.

## Versioned run-plan contract

Gate 529 should implement an immutable private plan with:

```text
formatVersion: 0.1
kind: wastelandforge.geck-host-probe-run-plan
runId: random 128-bit lowercase hex
createdUtc
projectRoot: null
probeBuild:
  buildManifestPath / length / sha256
  dllPath / length / sha256
  probeVersion
  xnvseCommit / xnvseTree
  launchAuthorized: true
  approvedProbeRunId: exact runId
environment:
  gameRoot / dataRoot
  geck / falloutnv / xnvse file identities
  mo2 executable identity / instance / profile
  pythonPlugin identity
  companionPackage identity
staging:
  operatorApprovedModRoot
  relativePath: NVSE/Plugins/WastelandForge.GeckProbe.dll
  effectivePath: Data/NVSE/Plugins/WastelandForge.GeckProbe.dll
  createOnly: true
effectiveProviders:
  required: WastelandForge.GeckProbe.dll exact digest
  forbidden: every other effective NVSE plugin DLL and GECK Extender marker
protectedBaseline:
  physical plugin manifest digest
  physical NVSE plugin-tree manifest digest
  fixed game/provider identities
  selected-profile state manifest digest
observation:
  root: %LOCALAPPDATA%/WastelandForge/GeckProbe
  expected query/load filenames and fields
launch:
  contextKind: geck-host-probe
  contextId: runId
  contextSha256: run-plan digest
  executable: exact Geck.exe
  arguments: []
recovery:
  manual close, no automatic retry, evidence preservation
planSha256
```

The plan is private local evidence, not canonical mod source. It must be UTF-8
without BOM, duplicate-key rejecting, canonicalized before digest approval, and
create-only. Missing or unresolved instance, profile, destination, identity,
effective visibility, or baseline values make the plan non-executable.

## Authorization model

Four approvals remain separate and cannot be collapsed:

1. **Plan approval:** approves the exact run-plan digest and exact authorized
   rebuild identity.
2. **Staging approval:** approves one create-only copy from the exact authorized
   DLL to the exact operator-chosen dedicated MO2 mod path.
3. **Environment approval:** inside MO2, selects one currently reported FNV
   profile after a read-only effective-provider snapshot passes.
4. **Launch approval:** inside MO2, approves the 15-minute request digest and
   invokes `startApplication` with exact GECK, empty arguments, and the selected
   profile.

Source, DLL, build manifest, destination, game/editor/xNVSE/MO2/Python/
companion bytes, instance, profile, profile state, provider visibility,
protected baseline, request bytes, expiry, or observation-root drift invalidates
all downstream approvals.

The current Gate 527 DLL can never satisfy plan approval because it is compiled
with live launch authorization false and does not contain an approved run ID.

## Staging boundary

The operator must first create and select a dedicated test mod in an initialized
FNV MO2 instance. Forge must not create, rename, enable, disable, reorder, or
remove that mod or mutate the profile.

After separate staging approval, a future helper may copy only the exact
authorized DLL to the absent target:

```text
<operator-approved-mod-root>/NVSE/Plugins/WastelandForge.GeckProbe.dll
```

The helper must refuse an existing target, reparse points, path escape,
non-empty unexpected content, destination drift, hash mismatch, and any target
inside game Data or MO2 Overwrite. It must re-read and hash the promoted file.
No INI, script, ESP, ESM, xNVSE, GECK Extender, or third-party file may be
staged with it.

Profile enablement and later cleanup remain explicit operator actions. Forge
may verify their result read-only but must not edit `modlist.txt`, plugin/load
order, MO2 settings, or profile files.

## Effective-provider preflight

Before launch approval, the optional MO2 companion must perform a read-only,
selected-profile inspection through MO2's supported virtual-tree API. It must
enumerate the effective native DLLs beneath `Data/NVSE/Plugins`, bind their
paths/lengths/SHA-256 values into the preview, and require exactly the approved
probe DLL for this smoke.

It must refuse:

- inability to inspect the effective tree;
- any additional effective native NVSE plugin DLL;
- a missing, duplicate, or digest-mismatched probe;
- any GECK Extender/GaryHax marker;
- profile, instance, virtual-tree, or staged-file drift before launch.

This is a read-only extension to the companion for this probe. It does not
authorize general virtual-tree browsing, profile mutation, provider
installation, or record inspection.

## Protected baselines

Immediately before request creation, capture deterministic per-file manifests
for:

- all regular direct-child physical `.esp` and `.esm` files under game Data;
- all regular files recursively under physical `Data/NVSE/Plugins`;
- `Geck.exe`, `FalloutNV.exe`, `nvse_loader.exe`, `nvse_1_4.dll`, and
  `nvse_editor_1_4.dll`;
- the selected profile's existing `modlist.txt`, `plugins.txt`, `loadorder.txt`,
  and settings files, recording explicit absence where applicable;
- the staged probe and exact observation/request/receipt inputs.

Reject reparse points and record normalized relative path, length, SHA-256, and
an aggregate manifest digest. Recapture the same sets after manual GECK shutdown
and require exact equality. The only permitted new files during the smoke are
the run-bound private query/load observations and the MO2 process-created
receipt.

## Execution and success criteria

A future live gate may perform this sequence only after every prerequisite is
resolved:

1. Rebuild the probe from the pinned clean source with the exact approved run
   ID and live authorization true; repeat static verification.
2. Stage the exact DLL after separate staging approval and verify its digest.
3. Have the operator enable the dedicated mod in the selected test profile.
4. Pass the effective-provider preflight and capture protected baselines.
5. Create the probe-specific 15-minute MO2 request.
6. Require explicit selected-profile preview and launch approval in MO2.
7. Wait for the operator to close GECK manually; do not send UI input.
8. Verify request/receipt, query/load observations, PID, run ID, probe digest,
   xNVSE/editor versions, UTC ordering, and protected post-state.

Success requires both `query-accepted` and `load-accepted` observations for the
same approved run, exact probe digest, and receipt PID, with no protected drift.
It proves only that the exact module loaded through xNVSE editor mode in the
selected MO2 profile and emitted its bounded observations.

It does not prove editor readiness, active-file state, record APIs, authoring,
save behavior, plugin correctness, GECK Extender compatibility, or ADR-013
authoring-provider sufficiency.

## Abort and recovery

- Receipt without valid query/load observations is a failed/ambiguous smoke;
  never retry automatically.
- Query refusal, load refusal, unexpected provider, crash, hang, missing
  receipt, PID mismatch, or any protected drift blocks success.
- Forge must not terminate GECK. The operator closes it normally or explicitly
  ends an unresponsive process after recording the condition.
- Preserve the plan, request, receipt, observations, manifests, and failure
  report before cleanup.
- Profile disablement and dedicated-mod removal are manual operator actions.
  Cleanup verification is read-only and must not erase failure evidence.
- A rerun requires a new run ID, newly authorized probe build, new staging and
  launch approvals, and fresh baselines.

## Gate 528 result

The contract is complete, but live execution is blocked because:

- no initialized FNV MO2 instance and pre-existing test profile was found;
- the Forge MO2 companion is not installed in the configured MO2 instance;
- the current probe is intentionally build-only and unapproved for launch;
- effective visibility of the 53 physical NVSE plugin DLLs is unresolved and
  must fail closed;
- no operator-selected dedicated test mod or staging approval exists.

No profile, mod, request, receipt, observation, game file, plugin file, or
external process was created or changed by this gate.

## Next route

Gate 529: implement the private host-probe run-plan schema/parser, authorized-
build preview, create-only staging preview, protected-manifest capture, and a
synthetic read-only MO2 effective-provider preflight. It must remain no-launch
and must refuse executable output while instance/profile/effective-provider
evidence is unresolved. A real profile, staging action, companion installation,
or GECK launch remains separately authorized future work.
