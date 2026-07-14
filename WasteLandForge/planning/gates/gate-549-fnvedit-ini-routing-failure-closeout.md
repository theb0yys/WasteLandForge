# Gate 549 - FNVEdit INI Routing Failure Closeout

Status: Complete - no-launch research and Gate 550 contract ready
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R005, R009, and Gates 545-548

## Goal

Close the Gate 548 compatibility failure without launching FNVEdit, identify
the evidence-supported contract defect, preserve unresolved provider behavior as
fail-closed checks, and define the smallest synthetic implementation gate before
any new real-provider preview can be considered.

This gate performs research and planning only. It does not change production
code or schemas, prepare another private run, execute or clean up a provider,
import game knowledge, or authorize a retry.

## Evidence classification

- **Documented:** the official
  [xEdit command-line reference](https://tes5edit.github.io/docs/2-overview.html#s_2-8-1)
  defines `-I:<path>` for game INI files alongside `-D`, `-P`, `-S`, `-C`,
  `-T`, `-B`, and `-R`.
- **Documented:** [official xEdit repository guidance](https://github.com/TES5Edit/TES5Edit/issues/1043)
  for an observed `Fatal: Could not find ini` startup failure supplies `-I:`
  with an exact game INI filename, not a plugin-list or Data path.
- **Documented:** Gate 546 fixed a 12-argument allowlist containing `-D`, then
  `-P`, but omitted the documented `-I` argument entirely.
- **Observed:** the exact Gate 548 plan contained no `-I` argument. FNVView
  reported the approved Data and Scripts paths, then `Using Cache Path:` and
  `Using ini:` with empty values before `Fatal: Could not find ini`.
- **Observed:** Gate 545's earlier default-state provider log resolved a local
  `Fallout.ini` under the redirected My Documents `My Games/FalloutNV` tree.
  Read-only Gate 549 inspection found that same regular file still present at
  19,959 bytes with SHA-256
  `a701c3a96af26f83ba6399b4a579af59fa075868949519f4dec45bf47bf7f95d`.
- **Observed:** the local `FalloutPrefs.ini` is a separate file. The provider's
  successful default-state startup log named `Fallout.ini`, not
  `FalloutPrefs.ini`, as its selected INI.
- **Inferred:** binding the exact `Fallout.ini` through `-I` is necessary to
  remediate the Gate 548 failure because the approved contract omitted the
  documented input and the provider reported that exact input missing.
- **Open:** adding `-I` has not been proved sufficient for FNVEdit 4.1.5f. Gate
  548 ended before module load or script execution, so private cache, plugin
  list, log routing, automatic script execution, and export completion remain
  compatibility-unproved.

## Failure closeout

Gate 548 is not a producer or parser failure. The process did not reach the
point where the generated script could establish its single-master precondition
or emit `raw-export.json`.

The immediate Forge contract defect is precise:

```text
documented provider input: -I:<game INI file>
Gate 546/547 plan input: absent
Gate 548 provider observation: Using ini: <empty>
Gate 548 terminal provider message: Fatal: Could not find ini
```

Exit code `0` does not alter that result. Gate 546 explicitly requires output,
log, and protected-root validation after exit, and Gate 548 correctly failed
closed when those checks failed.

The empty cache line and default provider-root `FNVView_log.txt` are retained as
observations, not assigned an unsupported cause. The provider aborted before a
complete startup, so Gate 549 does not claim that `-C` or `-R` is incompatible.
It also does not relax their private-routing requirements.

## Gate 550 implementation contract

Gate 550 is one substantial synthetic implementation gate. It must not execute
real FNVEdit. It must:

1. Preserve the published `0.1.0` execution-plan and execution-receipt schemas
   unchanged and add immutable `0.2.0` versions for INI-bound execution.
2. Add an exact `Fallout.ini` file to the plan's bound inputs with absolute
   path, basename, length, last-write UTC, and SHA-256.
3. Source the INI from an explicit local desktop setting. The UI may suggest
   the current user's `My Documents/My Games/FalloutNV/Fallout.ini` only when
   that exact regular file exists; preparation must refuse blank, missing,
   directory, reparse, wrong-basename, or unreadable inputs.
4. Display the exact INI path and digest in the operator preview before the
   approval token can be armed.
5. Change the fixed argument array from 12 to 13 entries by inserting this
   exact file argument after `-D` and before `-P`:

   ```text
   -I:<absolute approved Fallout.ini file>
   ```

   The file argument has no synthetic trailing separator. Process creation
   continues to use `ProcessStartInfo.ArgumentList` without shell execution.
6. Treat the INI as a protected read-only input. Validation must rehash it
   immediately before process creation and postflight must require byte identity.
7. Add the INI before/after evidence and `iniUnchanged` result to the `0.2.0`
   receipt. Any INI drift fails closed and preserves evidence.
8. Keep the Data, provider-installation, user-state, private-input, backups,
   output, and changed-path checks from Gate 547 at equal or greater strictness.
9. Keep `-C`, `-T`, `-B`, and `-R` routed to the approved private run. A future
   empty route, provider-root log, missing private log, or other external write
   remains a failure; Gate 550 must not accept `FNVView_log.txt` as a substitute
   for the declared private log.
10. Keep the `0.2.0` read-only producer and raw-export/index lineage unchanged
    except where plan/receipt provenance must reference the new schema versions.
11. Preserve the process runner's no-shell, no-elevation, no-environment-
    injection, no-UI-automation, no-timeout-kill, and no-retry behavior.
12. Add synthetic coverage for exact argument order, approval digest binding,
    missing/tampered/reparse/wrong INI evidence, preflight drift, postflight
    drift, old-schema compatibility, failed-closed receipts, and successful
    controlled-provider import.
13. Update the installed Game Knowledge workflow and prove preview, explicit
    approval, controlled synthetic execution, and responsive status without
    launching a real editor or using proprietary fixtures.

The retained Gate 545 and Gate 548 provider logs are evidence only. Gate 550
must not delete, overwrite, baseline-normalize, copy into fixtures, or otherwise
consume them as test inputs.

## Acceptance boundary

Gate 550 may claim only that Forge can prepare and synthetically enforce an
INI-bound private plan. It cannot claim FNVEdit 4.1.5f compatibility.

Successful synthetic execution is not evidence that the real provider will:

- accept all 13 arguments together;
- route cache, settings, backups, and logs privately;
- load exactly `FalloutNV.esm` from the private plugin list;
- apply the generated Pascal producer;
- emit a complete raw export; or
- leave every protected external root unchanged.

Those remain exact postflight requirements for a separately approved real run.

## Actions withheld

- No production code, schema, test, fixture, app, package, installer, or
  generated product output changed.
- No FNVEdit/xEdit, GECK, MO2, FalloutNV, game, or other provider process ran.
- No private run, provider log, view settings, INI, plugin list, game Data,
  master, project source, index, or repository history changed.
- No Gate 545 or Gate 548 cleanup occurred.
- No network dependency, AI, signing, timestamping, or publication action
  occurred. Official upstream documentation was read through public web pages.
- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.

## Next route

Gate 550: implement, test, publish, and installed-prove the versioned INI-bound
private execution contract above using only synthetic redistributable evidence
and a controlled provider. Stop before preparing or launching a real FNVEdit
process.

A later Gate 551 may prepare a fresh exact real-provider preview only after Gate
550 passes. Any execution after that preview requires a new user approval naming
its exact digest. Gate 548 Approval B is consumed and cannot be reused.
