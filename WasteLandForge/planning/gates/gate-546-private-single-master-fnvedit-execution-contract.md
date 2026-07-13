# Gate 546 - Private Single-Master FNVEdit Execution Contract

Status: Complete - implementation-ready no-launch remediation contract
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R005, R009, and Gates 481-484 and
543-545

## Goal

Define the Forge-side remediation for Gate 545 without launching FNVEdit:
prepare an exact private xEdit execution bundle that requests only
`FalloutNV.esm`, routes documented provider byproducts into the Forge-owned run,
executes the generated read-only script through a digest-approved process
boundary, exposes honest process/output readiness, and rejects every unexpected
external write before import.

This gate changes planning only. It does not rerun FNVEdit, modify the Gate 545
producer or retained evidence, delete the observed provider log, restore user
settings, write game Data, or claim compatibility with FNVEdit 4.1.5f.

## Gate 545 failure evidence

- **Observed:** the exact approved FNVEdit 4.1.5f process accepted the generated
  scripts path but loaded the user's active `Plugins.txt`, including DLC and
  preorder masters.
- **Observed:** the provider exited with code `0` without applying the script or
  emitting `raw-export.json`; a process exit code is therefore not export
  success evidence.
- **Observed:** game Data, the provider executable, `FalloutNV.esm`, the script,
  and the run manifest remained byte-identical.
- **Observed:** FNVEdit added `FNVEdit_log.txt` in its installation and rewrote
  the pre-existing user-local `Plugins.fnvviewsettings`.
- **Observed:** the Gate 544 producer would report a refusal when `FileCount` is
  not exactly one, but that check cannot run when the script is never applied.
- **Open:** the exact configured provider's behavior with a private plugin list,
  automatic script mode, view mode, private logs/cache, and automatic exit has
  not been compatibility-proved.

## Authoritative external evidence

The official
[xEdit command-line reference](https://tes5edit.github.io/docs/2-overview.html#s_2-8-1)
documents:

- `-view` for view mode;
- `-C`, `-T`, `-B`, and `-S` for cache, temporary, backup, and script paths;
- `-D` for the Data directory;
- `-P` for a custom `Plugins.txt` file;
- `-R` for a custom xEdit log filename; and
- path arguments with explicit paths rather than implicit working-state lookup.

The official
[xEdit change history](https://tes5edit.github.io/whatsnew.html)
documents that `-autoload` suppresses module selection and loads active modules
from the selected plugins list, `-script:"..."` runs a named Pascal script,
and `-autoexit` can close xEdit after script mode. It also states that view
settings use the same path/name basis as the selected plugins list. These are
supported integration surfaces, not proof that the local 4.1.5f build will
produce a valid Forge export with the proposed combination.

The official
[xEdit scripting reference](https://tes5edit.github.io/docs/13-Scripting-Functions.html)
continues to be authority for the read-only producer functions. No mutation API
is added by this remediation.

## Decision

Gate 547 will implement a **private, single-master, preview-gated execution
bundle** alongside the existing manual Gate 544 workflow.

The manual `0.1.0` contracts remain immutable and supported. Automated Forge
execution must not reuse their false `forgeExecutedXEdit` assertion. Gate 547
therefore adds new immutable versions for the automated lane:

```text
fnv-game-knowledge-export/0.2.0
fnv-game-knowledge-index/0.2.0
fnv-game-knowledge-receipt/0.2.0
fnv-game-knowledge-execution-plan/0.1.0
fnv-game-knowledge-execution-receipt/0.1.0
```

The `0.2.0` export/index/receipt lineage records
`forgeExecutedXEdit: true`. Existing manually produced `0.1.0` evidence keeps
`forgeExecutedXEdit: false`. Forge must never relabel one lineage as the other.

No new top-level CLI verb is introduced. This remains the desktop Game
Knowledge provider workflow under ADR-010.

## Private run layout

Gate 547 extends each unique prepared run with only contained, non-reparse
paths:

```text
%LOCALAPPDATA%/WastelandForge/game-knowledge/fnv/runs/<run-id>/
  WastelandForgeFNVGameKnowledge.pas
  run-manifest.json
  raw-export.json                         reserved output
  execution-plan.json
  execution-receipt.json                 reserved output
  state/
    Plugins.txt
    Plugins.fnvviewsettings              provider-owned optional output
  cache/                                  provider-owned bounded output
  temp/                                   provider-owned bounded output
  backups/                                must remain empty
  logs/
    FNVEdit.log.txt                       provider-owned expected output
```

`state/Plugins.txt` is deterministic ASCII-compatible UTF-8 without BOM and
contains exactly:

```text
FalloutNV.esm
```

plus one CRLF. Its digest is part of the approval. No installed/user plugin or
load-order file is copied, edited, renamed, or replaced.

The private run may be cleared only through the existing contained
preview-token cleanup after no provider process uses it. Provider files outside
this tree are never included in ordinary Forge cache cleanup.

## Exact argument allowlist

The execution plan stores arguments as an ordered JSON array and process
creation uses `ProcessStartInfo.ArgumentList`; Forge must not build a shell
command string. The only allowed argument semantics are:

```text
-FNV
-view
-autoload
-script:<absolute contained producer path>
-autoexit
-D:<absolute configured game Data directory with trailing separator>
-P:<absolute contained state/Plugins.txt path>
-S:<absolute contained run directory with trailing separator>
-C:<absolute contained cache directory with trailing separator>
-T:<absolute contained temp directory with trailing separator>
-B:<absolute contained backups directory with trailing separator>
-R:<absolute contained logs/FNVEdit.log.txt path>
```

The fixed order above is part of the execution-plan digest. `FNVEdit.exe` and
`xEdit.exe` are the only accepted provider basenames. `-FNV` is explicit for
both so a generic executable cannot select another game mode.

The allowlist excludes edit mode, master editing, direct save, cleaning,
conflict modes, LOD generation, game link, MO2 profile selection, installed
plugin/load-order files, undocumented switches, elevation, shell execution,
and every module argument other than the private plugin-list evidence.

Gate 547 must source the Data directory from the configured, revalidated parent
of the exact `FalloutNV.esm`. It must not infer a second installation from the
registry, Steam, MO2, or provider defaults.

## Execution plan and approval

Preparation remains non-executing. The plan records:

- run/schema/script identity and all absolute private paths;
- provider executable path, version metadata, length, last-write UTC, SHA-256,
  parent working directory, and accepted basename;
- master and Data directory path, master length/last-write UTC/SHA-256;
- plugin-list, script, manifest, and plan lengths/SHA-256;
- the exact ordered argument array and process flags;
- expected/forbidden write roots and expected output names;
- a preflight inventory commitment for game Data, provider installation, and
  the user-local FalloutNV load-order/settings tree;
- safety declarations for view-only, no shell, no elevation, no retry, no
  plugin writes, no load-order writes, and no game Data writes.

The approval token is SHA-256 over canonical plan bytes plus every bound input
digest. Preview must display the complete executable, working directory,
argument list, private write set, protected external roots, and limitations.

Immediately before process creation Forge revalidates containment, reparse
status, all bound bytes, empty reserved outputs, private directory ownership,
external preflight inventories, and the approval token. Drift refuses the run;
Forge does not regenerate or silently accept a new plan.

## Process boundary and visible state

The approved process shape is:

```text
FileName = exact approved FNVEdit.exe or xEdit.exe
WorkingDirectory = exact provider parent
UseShellExecute = false
ArgumentList = exact approved ordered arguments
CreateNoWindow = false
Verb = empty
redirection = none
environment additions = none
retry = none
```

Game Knowledge must expose these transient states:

```text
Prepared
ApprovalRequired
Starting
Running (PID and elapsed time)
ProcessExited (exit code)
AuditingSideEffects
OutputReady
FailedClosed
```

Process creation proves only that a process was created. Exit code `0` proves
only process exit. Forge imports only after output and side-effect validation.
No window automation, keystrokes, hidden-dialog dismissal, process restart, or
automatic retry is permitted. A long-running process remains visibly running;
Forge must not terminate it merely because an arbitrary duration elapsed.

Concurrent execution, stale approval, an existing provider process, process
creation failure, lost process identity, or cancellation before creation is a
refusal. Cancellation after creation stops waiting and reports that manual
process resolution is required; it does not terminate the provider or enable a
retry.

## Postflight and import gate

After exit, Forge recomputes the same bounded inventories and requires:

- game Data is byte-identical;
- provider executable and installation are byte-identical;
- user `Plugins.txt`, `loadorder.txt`, and existing view-settings evidence are
  byte-identical;
- the private plugin list, producer, run manifest, and execution plan are
  byte-identical;
- `backups/` is empty;
- every new/changed provider byproduct is contained under the approved private
  run and belongs to the declared `state`, `cache`, `temp`, `logs`, raw-export,
  or receipt set;
- `raw-export.json` is a regular, non-reparse, bounded file;
- the `0.2.0` script reports exactly one loaded file, `FalloutNV.esm`, complete
  traversal, no omissions/refusals, and `forgeExecutedXEdit: true`.

Any external drift, unexpected private path, missing output, backup, malformed
log/output, nonzero exit, script refusal, incomplete traversal, or import error
fails closed and preserves evidence. A successful process does not bypass the
existing strict schema, digest, ordering, count, containment, and atomic index
promotion checks.

`execution-receipt.json` records only observed process and postflight evidence:
plan digest, PID, start/exit UTC, exit code, elapsed time, output/log digests,
pre/post inventory digests, exact changed paths, and final state. It is not
canonical truth and does not prove plugin authoring capability.

## Gate 545 side-effect cleanup decision

Gate 546 makes no cleanup change.

- The test-created provider `FNVEdit_log.txt` is retained as failure evidence.
  A later one-time operator cleanup may delete only that exact path after a
  fresh preview proves its length and SHA-256 still match Gate 545. It is never
  included in broad or recursive Forge cleanup.
- The pre-existing `Plugins.fnvviewsettings` is never deleted, restored, or
  rewritten by Forge because its pre-run bytes were not captured. Its Gate 545
  modification remains an explicitly unresolved external side effect.

Neither action is implied by continuing to Gate 547 or by approving a future
compatibility smoke.

## Gate 547 implementation acceptance

Gate 547 is one substantial synthetic implementation gate. It must:

- preserve the complete manual `0.1.0` workflow and immutable schemas;
- add the automated `0.2.0` export/index/receipt lineage plus execution-plan and
  execution-receipt schemas;
- prepare the exact private layout, custom one-master plugin list, allowlisted
  arguments, canonical plan, and digest approval without launching a provider;
- add a typed injectable process runner and transient desktop status with no
  shell, elevation, environment injection, UI automation, retry, or kill;
- audit bounded pre/post trees and refuse every unexpected external/private
  change before import;
- add explicit **Preview Private xEdit Run** and approval-bound **Run Export**
  controls while retaining the manual instructions;
- prove argument order, path trailing separators, token staleness, tampering,
  reparse/escape, concurrent runs, process failures, cancellation, missing or
  malformed output, multiple masters, provider/user/Data drift, unexpected
  files, non-empty backups, and rollback behavior;
- use only synthetic redistributable master/provider/output fixtures and a
  controlled executable stub in automated and installed tests;
- run focused tests, release build, full serial suite, publication, installer,
  installed responsive UI regression, uninstall, and isolated cleanup;
- prove no real FNVEdit/xEdit, GECK, MO2, FalloutNV, Bethesda plugin, network,
  or protected overhaul content was executed or changed.

Gate 547 stops after controlled synthetic execution. It must not claim local
FNVEdit compatibility or consume Gate 545's retained run.

## Actions withheld

- No production code, schema, fixture, test, package, installer, or generated
  output changed in Gate 546.
- No FNVEdit/xEdit, GECK, MO2, FalloutNV, game, provider, or cleanup process ran.
- No Gate 545 private run, provider log, view settings, game Data, load order,
  plugin, master, project source, or external installation changed.
- No network download, AI, signing, timestamping, or publication occurred.
- No Tales from the Age of Men, Age of Men, or overhaul file changed.

## Next route

Gate 547: implement and synthetic/installed prove the private single-master
FNVEdit execution bundle and automated `0.2.0` evidence lineage above, then stop
before any real-provider retry. A later Gate 548 requires a new exact preview
and separate execution approval; Gate 545 Approval B cannot be reused.
