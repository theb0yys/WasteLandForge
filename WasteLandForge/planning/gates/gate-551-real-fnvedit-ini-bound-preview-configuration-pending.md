# Gate 551 - Real FNVEdit INI-Bound Preview Configuration Pending

Status: Approval pending - required explicit INI setting is not saved
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-008, ADR-009, ADR-010, ADR-011, ADR-013,
WFG-001, and Gates 545-550

## Goal

Prepare one fresh digest-bound real-provider preview using the Gate 550 `0.2.0`
INI-bound execution contract, then stop before process creation. Do not reuse the
consumed Gate 548 approval and do not execute FNVEdit.

## Read-only preflight

The user routed work to Gate 551. Read-only inspection of the current desktop
configuration established:

- the configured `FalloutNV.esm` exists;
- the configured FNVEdit provider exists;
- the bounded FalloutNV user-state root exists;
- the current My Documents suggestion resolves to an existing regular file
  named exactly `Fallout.ini`; and
- the saved desktop `FNVIniPath` setting is empty.

No provider, master, INI, user-state, or prior private-run digest was calculated
because the Gate 550 contract requires the INI to come from an explicit saved
desktop setting. The existing suggestion was not silently promoted into that
setting.

## Result

Gate 551 stopped before preview preparation. No run directory, script, manifest,
plugin list, execution plan, approval token, or receipt was created. No external
process was started.

This is a configuration gate, not evidence of provider incompatibility. The
other required configured paths were present during the bounded check, but they
must be revalidated and hashed when a fresh preview is actually prepared.

## Required operator action

Open WastelandForge **Settings**, confirm the suggested `Fallout.ini` path, and
save the settings. Then explicitly authorize Gate 551 to prepare one fresh
preview.

The resumed gate may only:

1. read and validate the exact saved provider, master, INI, and user-state roots;
2. create one fresh private `0.2.0` plan and display its exact digest; and
3. stop without launching FNVEdit.

Any execution requires a later, separate approval that names the fresh plan
digest. Gate 548 Approval B remains consumed.

## Safety boundary

- No real FNVEdit/xEdit, MO2, FalloutNV, game, or GECK process was launched.
- No app setting, INI, game Data, provider installation, user state, private run,
  project source, plugin, retained Gate 545/548 evidence, or repository history
  was changed.
- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.

## Next route

Resume Gate 551 only after the explicit `Fallout.ini` setting has been saved and
the user authorizes preparation of one fresh preview. Stop again at the displayed
plan digest; do not execute it.
