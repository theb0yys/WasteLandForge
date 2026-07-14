# Gate 550 - INI-Bound Private Execution Contract

Status: Complete - synthetic implementation, publication, and installed proof
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R005, R009, and Gates 545-549

## Goal

Implement the Gate 549 `Fallout.ini` remediation as a versioned,
digest-approved private execution contract, prove it with redistributable
synthetic evidence and a controlled provider, publish the installed desktop
package, and stop before any fresh real FNVEdit preview or execution.

## Evidence classification

- **Documented:** Gate 549 requires immutable `0.2.0` plan and receipt schemas,
  exact `Fallout.ini` identity, `-I` after `-D` and before `-P`, protected INI
  preflight/postflight checks, synthetic tests, publication, and installed proof.
- **Documented:** Gate 549 preserves the `0.1.0` schemas unchanged and forbids
  real FNVEdit execution in Gate 550.
- **Implemented:** Forge now prepares and validates a 13-argument INI-bound
  private plan, refuses legacy `0.1.0` plans at execution, and emits a `0.2.0`
  receipt containing INI before/after evidence and `iniUnchanged`.
- **Implemented:** the desktop stores an explicit local `Fallout.ini` path,
  suggests the current-user path only when the exact file exists, and displays
  the bound path and digest before approval.
- **Proved synthetically:** the controlled provider accepted the exact argument
  order, read the approved INI, produced the declared private outputs, and left
  every protected input byte-identical.
- **Open:** real FNVEdit 4.1.5f compatibility with the complete 13-argument
  contract remains unproved. Gate 550 does not resolve private cache, settings,
  log routing, module selection, script execution, or export completion for the
  real provider.

## Implementation

The published `0.1.0` execution plan and receipt schemas remain unchanged. New
immutable `0.2.0` resources add:

- exact INI path, basename, length, last-write UTC, and SHA-256 evidence;
- one protected-file write-policy entry;
- a fixed 13-entry argument array containing `-I:<absolute Fallout.ini>`;
- receipt-level INI before/after states and `iniUnchanged`.

Preparation refuses blank, relative, missing, directory, reparse, wrong-name,
empty, oversized, or unreadable INI evidence. Validation rehashes the approved
file before process creation. Postflight requires byte and timestamp identity,
adds drift to changed paths, fails closed, and preserves the receipt.

The process runner remains unchanged: no shell, elevation, environment
injection, UI automation, timeout termination, or automatic retry was added.
The producer, export, index, and record-receipt lineage remains `0.2.0`.

## Validation

The following checks passed:

- Release solution build with zero warnings and zero errors.
- Controlled synthetic-provider build with zero warnings and zero errors.
- Focused Game Knowledge unit tests: 15 passed.
- Focused Windows runner/settings/workspace tests: 11 passed.
- Focused schema test: 1 passed.
- Focused schema-catalog compatibility theory: 53 passed.
- Complete solution tests: 969 passed, 0 failed, 0 skipped.
- `git diff --check` passed; only existing line-ending notices were emitted.
- App-shell publication and bundled-backend verification passed.
- Unsigned local installer build passed with Inno Setup 6.7.3.
- Installed synthetic regression passed at 960x640 and 1180x760.

The installed proof verified preview path/digest visibility, explicit two-click
approval, exact 13-argument order, controlled synthetic execution, `0.2.0`
receipt evidence, successful import, responsive search/handoff, protected-root
and INI identity, no new editor/game process, private-cache cleanup, uninstall,
and temporary-root cleanup.

Two earlier installed-proof attempts stopped before provider execution due to
test-harness route discovery defects. The first lacked a wait for the expanded
combo item; the second exposed a PowerShell variable-name collision in that
wait. Both isolated installations were removed. The corrected third run passed,
and a final repeat after moving INI validation ahead of synthetic output writes
also passed.

Published local artifacts:

```text
dist/app/WastelandForge.Desktop/WastelandForge.exe
SHA-256: f4fbcae8736d6db54b52586a37f08be709a6eb7320a7e4775f850daba3c164ec

artifacts/installer/inno/local/WastelandForge-Setup-local.exe
SHA-256: e839ee59804a63c3cba490df78bd2185b6a626f987861537efc400ac0e7a51ea
```

## Safety boundary

- No real FNVEdit/xEdit, MO2, FalloutNV, game, or new GECK process was launched.
- The already-running user GECK process was observed only by name during the
  installed regression's before/after process inventory and was not touched.
- No real game INI, Data directory, provider installation, user-state root,
  plugin, project source, Gate 545/548 evidence, or repository history changed.
- No proprietary fixture, network dependency, AI, signing, timestamping, or
  release publication was used.
- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.

## Next route

Gate 551 may prepare a fresh exact real-provider preview using the configured
`Fallout.ini` only after explicit user authorization for that preview. It must
not execute the plan. Any later execution requires a separate approval naming
the fresh plan digest; Gate 548 Approval B remains consumed and cannot be reused.
