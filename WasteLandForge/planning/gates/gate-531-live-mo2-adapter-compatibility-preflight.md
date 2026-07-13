# Gate 531 - Live MO2 Adapter Compatibility Preflight

Status: Deferred - required operator-controlled MO2 environment unavailable
Phase: post-v0.1 provider discovery
Decision base: ADR-004, ADR-008, ADR-013, R005, and Gates 520, 528-530

## Goal

Perform the separately approved read-only compatibility preflight for Gate
530's packaged `virtualFileTree()` adapter only when an initialized FNV MO2
instance, already-current probe profile, and installed companion are available.
Otherwise, record the exact blockers without installing, configuring, staging,
or launching anything.

## Evidence classification

- **Documented:** Gate 530 permits only read-only inspection of an already
  initialized FNV MO2 instance and already-current profile through the packaged
  companion.
- **Documented:** Gate 528 keeps companion installation, instance/profile
  creation or selection, probe staging, request creation, and GECK launch under
  separate approvals.
- **Observed:** the configured MO2 2.5.2 root exists, but no portable
  `ModOrganizer.ini` or portable `profiles` root exists.
- **Observed:** the bounded LocalAppData MO2 root contains only cache and
  Fallout 4 London-labelled directories; no FNV instance was found.
- **Observed:** `plugins/wastelandforge_bridge/plugin.py` is absent from the
  configured MO2 root.
- **Observed:** the configured FNV MO2 mods root exists and contains zero
  entries.
- **Observed:** no `ModOrganizer` process is running.
- **Open:** real MO2 2.5.2/Python compatibility, current-profile identity,
  virtual-tree traversal, winner resolution, and effective-provider evidence
  remain untested.

## Preflight result

The adapter cannot be invoked because there is no in-process `IOrganizer`
session for an initialized FNV instance and no installed Forge companion. No
profile can be named or treated as current from the available evidence.

Creating an FNV instance/profile, installing the companion, creating or
enabling a dedicated probe mod, or starting MO2 would be mutation or external
execution outside this gate. The compatibility preflight therefore stops
before any `virtualFileTree()` call and is deferred rather than passed or
failed.

## Resume conditions

Gate 531 may resume only after the operator independently supplies all of:

1. an initialized FNV MO2 instance;
2. an explicitly named probe profile that is already current;
3. the Gate 530 companion installed through MO2's supported plugin workflow;
4. explicit approval for a read-only in-process compatibility check; and
5. confirmation that no staging, profile mutation, request creation, or launch
   is part of that approval.

The resumed check must report exact MO2/Python/companion identities, call the
adapter against the already-current profile, preserve refusal output, and make
no compatibility claim unless the real API traversal completes.

## Validation

- Rechecked the configured MO2 root, portable configuration/profile paths,
  bounded LocalAppData instance names, companion entrypoint, configured mods
  root, and running process list read-only.
- Confirmed the configured mods root still contains zero entries and no
  `ModOrganizer` process is running.
- Passed all seven isolated Python companion/adapter regressions.
- Confirmed the retained companion ZIP and package manifest still agree on
  SHA-256
  `527ffff36b936428b0eafdce680d709a0c2cf5f90a5e8d84cf36629c2cb12ccd`.
- Confirmed package evidence remains `liveMo2Executed=false` and
  `mo2StateChanged=false`.
- Passed `git diff --check`; only existing Git line-ending conversion warnings
  were reported.
- The full .NET suite was not rerun because Gate 531 changes planning only. The
  latest Gate 530 run remains 892/892 passing.

## Actions withheld

- No companion installation, update, removal, or package extraction.
- No MO2 instance, profile, mod, mod-list, plugin-list, or settings change.
- No probe build, authorization, staging, effective-provider assertion, or
  protected baseline capture.
- No MO2, GECK, xEdit, game, or provider process launch.
- No request, receipt, observation, ESP/ESM, game Data, or external-state
  write.

## Next route

No Gate 532 authoring-execution work is authorized by the current evidence.
Resume Gate 531 after the operator-controlled prerequisites above exist, or
obtain an explicit research/planning decision to close this provider lane and
select a different repository-local value slice.
