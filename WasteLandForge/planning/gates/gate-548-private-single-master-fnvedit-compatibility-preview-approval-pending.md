# Gate 548 - Private Single-Master FNVEdit Compatibility Preview Approval Pending

Status: Approval A complete - Approval B pending
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R005, R009, and Gates 543-547

## Goal

Prepare and independently inspect a fresh Gate 547 private single-master plan
for the exact configured real FNVEdit provider, then stop before either desktop
Run confirmation. This gate does not inherit Gate 545 Approval B and does not
authorize process creation.

## Evidence classification

- **Documented:** Gate 547 permits Gate 548 to prepare a new exact preview and
  request separate execution approval, but states that no real-provider run may
  occur without a new explicit user instruction.
- **Observed:** the configured provider and `FalloutNV.esm` still match the
  lengths and SHA-256 values recorded by Gate 545.
- **Observed:** WastelandForge created one new private run and returned
  `ApprovalRequired`; the Run action remained unarmed as **Run Export**.
- **Observed:** no FNVEdit/xEdit, GECK, MO2, FalloutNV, or game process was
  running before preparation, after preparation, or after inspection.
- **Open:** FNVEdit 4.1.5f compatibility with the exact private argument set and
  generated `0.2.0` producer remains unproved until separately approved
  execution and postflight validation complete.

## Approval A evidence

```text
run id:
  20260713T204439325Z-3d14fb1b38604db7aeb7ec0b3029cdfd

execution plan:
  length: 280551
  sha256 / approval token:
    0520816506384ed2570efb5a686fc2a9a327edbae8de077bb48ede013715798e

provider:
  file: FNVEdit.exe
  version: 4.1.5.0
  length: 24620544
  sha256: 895ce936fead6da9b6a0dde4b1e88c73332f5ec0ed319add03328fd89cb040d9

master:
  file: FalloutNV.esm
  length: 245650747
  sha256: 50991d36804b7d1e70df1afd7471b72f0e29d1b456ee2516a9717c002564e7c1

automated producer:
  length: 6315
  sha256: 693c9f4f340d7ea7f89e1ce02bcf0f8f85908426637773516177702374802426

run manifest:
  length: 1595
  sha256: 5606b4c9c4fbf0731c6319f180ae8cd9ccd07426b3389fa1de1e91d232431e87

private Plugins.txt:
  length: 15
  sha256: 1e489a6abc63f6f892f017e02740ac8dd425bbcff6e0676f4518e367074b8ee7
  bytes: FalloutNV.esm plus CRLF
```

All bound lengths, last-write UTC values, and SHA-256 values were independently
recomputed after preparation and matched the plan exactly.

## Ordered process request

The plan contains exactly:

```text
-FNV
-view
-autoload
-script:<private absolute producer path>
-autoexit
-D:<configured Data path with trailing separator>
-P:<private state/Plugins.txt path>
-S:<private run path with trailing separator>
-C:<private cache path with trailing separator>
-T:<private temp path with trailing separator>
-B:<private backups path with trailing separator>
-R:<private logs/FNVEdit.log.txt path>
```

Absolute local paths remain deliberately outside this tracked planning record.
They are present in the private execution plan and must be shown in the
operator-facing approval preview.

## Protected-root commitments

```text
game Data:
  files: 721
  bytes: 9795152094
  composite sha256:
    1e0437da7475cc9e10d2a6b212d566368bc57b6cf8cefec4ec91ad773832f6e9

provider installation:
  files: 255
  bytes: 133543338
  composite sha256:
    07ecaae100517935c85ecddd2dcf5444775761fe113ed325db6887d88e56a7a3

user-local FalloutNV state:
  files: 4
  bytes: 13720
  composite sha256:
    102c2aff14ad0208ff49a09d76437d6ae1cd25227ddba3d94fff80f2959dcd30
```

The raw export, private log, execution receipt, and private view-settings file
are absent. Private cache, temp, and backups are empty. The only allowed write
root is this new run. Data, provider installation, and user-local state remain
protected roots.

## Approval boundary

Approval A performed only:

- read-only configured-path and process discovery;
- provider/master identity hashing;
- WastelandForge desktop preview preparation;
- private plan/script/manifest/plugin-list creation; and
- read-only post-preparation inspection.

Approval A did not click **Run Export**, did not arm **Confirm Run Export**, and
did not launch FNVEdit. It did not consume or modify the Gate 545 retained run,
provider log, or pre-existing view-settings evidence.

## Next route

Gate 548 Approval B remains pending. The exact approval request is:

```text
Approve Gate 548 Approval B: execute plan
0520816506384ed2570efb5a686fc2a9a327edbae8de077bb48ede013715798e
once, with no retry, and perform the declared postflight audit.
```

Any provider/master/input/inventory drift before process creation invalidates
this token and requires a new preview. Approval B, if granted, authorizes only
this exact one-time process request and no plugin authoring, cleanup, retry,
GECK, MO2, game launch, signing, publication, or follow-on gate.

## Protected areas

- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.
- No plugin, master, game Data, provider installation, load-order state,
  project source, Gate 545 evidence, or repository history changed.
