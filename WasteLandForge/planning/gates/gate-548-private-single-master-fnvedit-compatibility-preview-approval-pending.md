# Gate 548 - Private Single-Master FNVEdit Compatibility Smoke

Status: Failed closed - approved one-time execution and postflight audit complete
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R005, R009, and Gates 543-547

## Goal

Prepare and independently inspect a fresh Gate 547 private single-master plan
for the exact configured real FNVEdit provider, obtain approval for that exact
digest, execute it once without retry, and perform the declared postflight
audit. This gate does not inherit Gate 545 Approval B.

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
- **Observed:** the user approved the exact plan digest for one execution with
  no retry and the declared postflight audit.
- **Observed:** FNVView 4.1.5f started once, accepted the declared Data and
  Scripts paths, reported an empty cache path and INI path, and ended its own
  log with `Fatal: Could not find ini`.
- **Observed:** the provider process returned exit code `0`, but no raw export
  or declared private log was created; the protected provider installation also
  gained `FNVView_log.txt`.
- **Documented:** Gate 546 states that exit code `0` proves only process exit;
  missing output, malformed/missing log evidence, or protected-root drift must
  fail closed and preserve evidence.
- **Open:** the exact FNVEdit 4.1.5f argument/INI routing needed for this private
  execution contract remains unresolved. This result does not prove producer
  compatibility and does not authorize another launch.

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

## Approval B result

The user supplied the exact Approval A digest and authorized one execution with
no retry. Input and protected-root inventories were revalidated before process
creation. FNVEdit/FNVView then launched exactly once.

```text
approved plan sha256:
  0520816506384ed2570efb5a686fc2a9a327edbae8de077bb48ede013715798e
provider launches: 1
process id: 12276
provider exit code: 0
elapsed milliseconds: 353223
wait cancelled: no
retry: none
timeout termination: none
UI automation: none
final state: FailedClosed
execution receipt sha256:
  90aa9f3157475346b3ace48b58e035503ccd11421df751b212b103e73a4f599a
```

The first orchestration host failed before process creation because its loaded
.NET dependency set could not satisfy the product assembly's Humanizer version.
That failure consumed no provider launch. A clean, ignored private .NET runner
then invoked the Gate 547 product validation and completion services and created
the single approved provider process above. Its build produced two `NU1900`
warnings because the NuGet vulnerability feed was unavailable; no dependency
or product change resulted.

## Provider evidence

The provider created `FNVView_log.txt` in its installation root:

```text
length: 644
sha256: 1d1f323df6c3625c8571c71c084e7dc55df9149a62d25beb5de12fb9c2711fd2
session start: 2026-07-13 22:01:28
reported Data path: configured Fallout New Vegas Data root
reported Scripts path: approved private run root
reported Cache path: empty
reported ini: empty
terminal provider message: Fatal: Could not find ini
```

The log is retained as failure evidence. Its content proves the provider's own
reported failure, not the semantics of any individual command-line switch. No
cleanup or argument reinterpretation is authorized by this gate.

## Postflight audit

```text
game Data:
  unchanged: yes
  files before/after: 721 / 721
  composite sha256 before/after:
    1e0437da7475cc9e10d2a6b212d566368bc57b6cf8cefec4ec91ad773832f6e9

provider installation:
  unchanged: no
  files before/after: 255 / 256
  composite sha256 before:
    07ecaae100517935c85ecddd2dcf5444775761fe113ed325db6887d88e56a7a3
  composite sha256 after:
    c3b2ea2830dfb95e7e2a39b2e556e85b9e4abe1f3bd08053988112882e871cf0
  changed path: FNVView_log.txt added

provider executable:
  unchanged: yes
  sha256:
    895ce936fead6da9b6a0dde4b1e88c73332f5ec0ed319add03328fd89cb040d9

FalloutNV.esm:
  unchanged: yes
  sha256:
    50991d36804b7d1e70df1afd7471b72f0e29d1b456ee2516a9717c002564e7c1

user-local FalloutNV state:
  unchanged: yes
  files before/after: 4 / 4
  composite sha256 before/after:
    102c2aff14ad0208ff49a09d76437d6ae1cd25227ddba3d94fff80f2959dcd30

approved private inputs:
  unchanged: yes
backups empty: yes
raw-export.json: absent
declared private FNVEdit log: absent
private index promotion: not run
tracked provider/editor/game processes after audit: none
```

The private run retains the approved plan, execution receipt, producer, run
manifest, and one-master plugin list. No raw export or index exists. The product
correctly refused import because the provider installation changed and both
required output artifacts were missing.

## Current decision

Gate 548 failed closed. The approved one-time launch has been consumed. Exit
code `0` does not override the provider fatal message or the Gate 546 postflight
requirements. This evidence does not permit a retry, relaxed inventory checks,
index promotion, or a real-provider compatibility claim.

## Next route

Gate 549 is a no-launch compatibility-failure closeout and argument/INI routing
research gate. It must preserve the Gate 548 private run and provider log,
determine the documented meaning and supported combination of the observed
FNVEdit 4.1.5f switches, and define any remediation before a new preview can be
considered. Gate 548 Approval B is not reusable; Gate 549 authorizes no provider
execution, cleanup, plugin authoring, GECK, MO2, game launch, or import.

## Protected areas

- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.
- No plugin, master, game Data, load-order state, project source, Gate 545
  evidence, or repository history changed.
- The provider installation gained only the retained `FNVView_log.txt` failure
  evidence recorded above. No cleanup was performed.
