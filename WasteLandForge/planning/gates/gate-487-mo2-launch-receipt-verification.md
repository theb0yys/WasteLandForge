# Gate 487 - MO2 Launch Receipt Verification

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-008, ADR-009, ADR-011 and Gates 483-486

## Goal

Add explicit, read-only desktop discovery and strict verification of private MO2
process-created receipts without polling, retrying, launching, or mutating MO2.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| A companion receipt records request/digest, instance, selected profile, tool, executable digest, PID when available, process-created status, and UTC. | Documented | Gate 483, lines 181-195 |
| Successful receipt evidence proves only that MO2 created a process for the selected profile. | Documented | Gate 483, lines 177-179 |
| The desktop may display receipts read-only in a later gate. | Documented | Gate 483, lines 197-198 |
| Gate 487 must add read-only linked receipt discovery without polling or MO2 mutation. | Documented | Gate 486, lines 60-65 |
| Historical valid receipts may remain viewable after request expiry because they are evidence of a past process-created event, not a new launch authorization. | Inferred | Gate 483 request-expiry and receipt roles |

## Implemented

- `Mo2LaunchReceiptService` enumerates only direct-child `*.receipt.json` files
  after an explicit desktop refresh.
- It refuses reparse roots/files, path escape, invalid filenames/IDs, missing
  matching requests, BOM/invalid UTF-8, duplicate JSON keys, non-exact request,
  project/tool/safety/receipt shapes, non-empty arguments, unsafe flags, request
  byte drift, request/executable/tool mismatch, invalid process-created state,
  invalid PID, malformed UTC, and invalid instance/profile values.
- Valid receipts are sorted newest first and expose process-created status,
  instance, profile, PID when present, UTC, request digest, executable digest,
  and handle-close warning.
- The Project Outputs panel adds **Refresh Receipts**, selection, aggregate
  verified/refused counts, and read-only details. There is no timer, watcher,
  retry, launch, deletion, acknowledgement, or MO2 state action.
- Details state that receipt evidence does not prove VFS contents, editor
  readiness, module load, review, save, game runtime, or mod correctness.

## Verification

- Focused receipt tests: 10 passed, covering valid historical evidence plus
  request drift, digest/tool/executable mismatch, invalid PID/process state,
  unsafe request, missing request, and duplicate-key refusal.
- Full .NET suite: 839 passed, 0 failed.
- PowerShell syntax and diff whitespace checks passed.
- Self-contained `win-x64` publication, installer preflight, and unsigned Inno
  Setup compilation passed.
- Installed regression created a synthetic receipt beside the generated xEdit
  request, refreshed explicitly, verified instance `Synthetic FNV`, profile
  `Testing`, PID `9001`, and limitation text, then removed receipt/request and
  isolated test state.

## Boundaries

- No live MO2, companion Python runtime, GECK, xEdit, game, or third-party tool
  was launched by receipt verification.
- Receipt discovery reads only the private LocalAppData request root and never
  changes receipt/request bytes.
- A verified receipt is process-created evidence only and is not release or
  mod-correctness evidence.

## Next route

Gate 488: define and execute a user-mediated live MO2 compatibility smoke only
after the user explicitly selects a disposable/test MO2 instance and authorizes
companion installation and one GECK or xEdit test launch. The gate must capture
MO2/Python versions, package digest, selected profile, request/receipt evidence,
cleanup, and limitations without changing profile/load order or claiming editor
or mod correctness. If no test instance is authorized, stop before mutation.

