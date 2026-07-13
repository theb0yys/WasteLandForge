# Gate 545 - Local FNVEdit Game Knowledge Compatibility Smoke Approval Pending

Status: Failed closed - provider ran, active plugin list loaded, no export emitted
Phase: post-v0.1 local compatibility evidence
Decision base: ADR-004, ADR-008, ADR-009, ADR-010, ADR-011, ADR-013,
WFG-001, R001, R005, R009, and Gates 503, 534, 543, and 544

## Goal

Prove or disprove that the Gate 544 read-only game-knowledge Pascal producer can
run against the configured local FNVEdit provider with only `FalloutNV.esm`
selected, emit a complete bounded raw export into the Forge-owned private run,
and pass the existing strict Forge import/index validation.

This gate does not authorize plugin authoring, plugin mutation, automated module
selection, script installation, load-order changes, game Data writes, or a
general xEdit compatibility claim.

## Evidence map

- **Documented:** Gate 543 requires a separate local compatibility smoke and an
  exact preview of provider, master, script, output path, lengths, and SHA-256
  values before execution.
- **Documented:** Gate 543 selects the manual xEdit workflow: the operator loads
  only `FalloutNV.esm`, applies the generated script, waits for completion, and
  returns to Forge for import.
- **Documented:** Gate 544 implements the producer, run manifest, bounded import,
  sealed index, stale detection, receipt workflow, and owned-cache cleanup, but
  explicitly leaves real FNVEdit compatibility open.
- **Observed:** the configured provider is an existing regular FNVEdit 4.1.5.0
  executable with length `24620544` and SHA-256
  `895ce936fead6da9b6a0dde4b1e88c73332f5ec0ed319add03328fd89cb040d9`.
- **Observed:** the configured `FalloutNV.esm` is an existing regular file with
  length `245650747` and SHA-256
  `50991d36804b7d1e70df1afd7471b72f0e29d1b456ee2516a9717c002564e7c1`.
- **Observed:** no FNVEdit/xEdit, GECK, FalloutNV, or MO2 process was running
  before or after Approval A.
- **Observed:** Approval A created run
  `20260713T190559820Z-4a2277f7eb0446e78f14bfa7214918c7` with exactly the
  generated script and run manifest; `raw-export.json` remains absent and
  reserved for the smoke.
- **Observed:** the script length is `6316` with SHA-256
  `4cac82dce952e3e89669cd4dbd3b225dccfc48760642564a54f7f3a2b3f40df4`;
  the manifest length is `1596` with SHA-256
  `3f988b08ec75874d7597da882fdc73a3dd6ee0874ab11cd5dcc6262c25ebfe1a`.
- **Observed:** all run path components are non-reparse, provider/master
  identities still match preflight, and the run approval token is
  `ec774c594eb73d41a735f7f149b0282077adeb7ddc4eba8eec0ce343bb96f918`.
- **Observed:** Approval B launched the exact FNVEdit executable with only the
  documented `-S:<path>` scripts-directory switch. The log confirms the exact
  private script path was active.
- **Observed:** FNVEdit loaded the user's active plugin list, including DLC and
  preorder masters, instead of establishing the required `FalloutNV.esm`-only
  selection. It exited with code `0` before the script was applied and emitted
  no `raw-export.json`.
- **Observed:** the pre/post byte-level digest for all 721 files under game Data
  remained `f688f6f505c214d43c21cf91ce3baeccfbf3ff321795cb5c2887bafe001df89f`.
  Provider executable, master, generated script, and manifest digests remained
  unchanged.
- **Observed:** FNVEdit added a `92696`-byte `FNVEdit_log.txt` to its installation
  with SHA-256
  `179a98fb56fb01fdd8a6bdb22e553d4c6bcfae2c52bf5d67b39a012c6a8f6f0d`
  and wrote the existing user-local `Plugins.fnvviewsettings`. Pre-run bytes for
  the view-settings file were not captured, so its exact content delta cannot
  be established.
- **Open:** FNVEdit traversal completeness, script compilation/runtime behavior,
  full-master export duration/size, optional context coverage, and import
  acceptance remain unproved.

Absolute local installation paths are deliberately omitted from this tracked
planning record. They must appear in the private operator preview before either
approval and must be re-resolved immediately before each action.

## Approval sequence

Gate 545 uses two approvals. Neither approval implies the other.

### Approval A - prepare private evidence

After explicit approval, Forge may invoke only its existing **Prepare Export**
operation. The allowed writes are a unique direct child beneath:

```text
%LOCALAPPDATA%/WastelandForge/game-knowledge/fnv/runs/
```

The operation may create only:

```text
WastelandForgeFNVGameKnowledge.pas
run-manifest.json
```

and reserve the exact sibling `raw-export.json` destination. It must not launch
FNVEdit, copy the script into the provider installation, write game Data, or
change the load order.

After preparation, stop and show the exact absolute provider, master, script,
manifest, run directory, and raw-output paths plus every file length and
SHA-256. Recheck that all path components are non-reparse regular paths and
that provider/master identities still match this preflight.

### Approval B - run the compatibility smoke

Only a second explicit approval against that exact preview may authorize the
operator-facing FNVEdit launch/manual script run. The approved procedure is:

1. capture provider and master pre-run length/SHA-256 evidence;
2. launch the exact approved provider without undocumented selection switches;
3. load only `FalloutNV.esm`;
4. apply the exact generated script without installing or modifying it;
5. wait for script completion and close FNVEdit without saving plugin changes;
6. verify provider and master post-run length/SHA-256 values are unchanged;
7. verify no new or changed file exists under game Data;
8. import the exact contained `raw-export.json` through Gate 544;
9. record completion counts, omissions, refusals, raw-export/index lengths and
   digests, elapsed time, and final catalogue state;
10. treat every script/runtime/import error or omission as a failed smoke.

If FNVEdit prompts for any unapproved file, save, master, load-order, recovery,
or write action, abort rather than accepting the prompt.

## Success criteria

The smoke succeeds only when:

- exactly the approved provider ran and exactly `FalloutNV.esm` was selected;
- the export reports complete with no refusal or omission;
- every emitted source file is `FalloutNV.esm`;
- all safety flags remain false except the required read-only flag;
- Forge imports, seals, reopens, and reports a `Ready` index;
- provider, master, game Data, project source, load order, and plugin bytes are
  unchanged;
- the resulting raw export and index remain private local evidence;
- the report limits compatibility to this exact provider/master/script digest
  tuple.

Any failure records Gate 545 as failed or deferred. It does not permit relaxing
schema, safety, containment, completeness, provenance, or mutation checks.

## Current decision

The user explicitly authorized Approval B against the exact preview. The smoke
failed the single-master, no-provider-installation-write, export-completion, and
Forge-import criteria. Gate 545 is failed closed. This evidence does not permit
another launch, automatic module selection, relaxed import checks, or a real
FNVEdit compatibility claim.

## Approval B result

```text
provider exit code: 0
documented script path accepted: yes
FalloutNV.esm-only load: no
active plugin list loaded: yes
raw-export.json emitted: no
Forge import/index: not run
game Data byte changes: 0
provider executable changed: no
master changed: no
script/manifest changed: no
provider installation changes: FNVEdit_log.txt added
user-local provider settings: Plugins.fnvviewsettings written
compatibility result: failed
```

## Actions withheld

- No raw export or index was created; the retained private run contains only
  the Approval A script and manifest.
- FNVEdit ran once under the exact Approval B boundary; no GECK, MO2, FalloutNV,
  game, or other provider process was launched.
- No plugin, master, game Data, load-order, project, provider installation, or
  canonical project mutation was authorized. The unapproved provider log and
  provider view-settings writes above are retained as failure evidence pending
  an explicit cleanup decision.
- No network, AI, signing, publication, or dependency action.
- No Tales from the Age of Men, Age of Men, or overhaul file changed.

## Next route

Gate 546 is a no-launch remediation contract for a supported deterministic
`FalloutNV.esm`-only startup/selection mechanism, private routing of provider
logs/cache/settings, visible process readiness, and an exact cleanup decision
for the two observed provider byproducts. It must not rerun FNVEdit, modify the
prepared script, or relax the Gate 545 success criteria during planning.
