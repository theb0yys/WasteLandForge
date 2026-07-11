# Gate 395 - MO2 Export Settings and Discovery Contract

Status: Complete with discovery implementation gated
Phase: v0.1 implementation planning
Decision base: ADR-008, ADR-009, ADR-012, Gates 392-394, R005

## Goal

Define safe persistence and precedence for the named-MO2 export root, and state
the evidence required before Forge implements automatic MO2 instance discovery.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Capability detection is local-first and deterministic. | Documented | R005 / ADR-008 |
| MO2 executable, profile, and VFS launch are separate capabilities. | Documented | R005 |
| MO2 remains authoritative for profile installation and effective visibility. | Documented | Asset Pipeline report / ADR-004 |
| The configured MO2 executable path is not a mods root. | Documented project contract | Gate 392 |
| Current research does not define stable MO2 instance paths, INI filenames/keys, path expansion, portable/global precedence, or version compatibility. | Open | R005 and Gate 392 |

## Persisted settings contract

The desktop local settings model gains a distinct nullable/empty
`Mo2ModsRoot`. It is separate from existing `Mo2Path`, which remains the MO2
executable path used by capability scanning.

- Stored only in `%LOCALAPPDATA%/WastelandForge/app-settings.json`.
- Never written into canonical project source, generated output, manifests, or
  distributable fixtures.
- Saved and reset through the existing local Settings workflow.
- Normalized to an absolute path only after user confirmation.
- A missing, moved, inaccessible, reparse, `Overwrite`, or game-Data-contained
  persisted path is shown as invalid and is never used for export.
- Reset removes the persisted value and clears the Project Outputs field.
- Loading settings performs no directory creation or external write.

Project Outputs initializes `MO2 mods folder` from the valid persisted value.
Browsing changes the current field but does not silently save settings. The
user must use the existing Save Settings action to persist it.

## Selection precedence

For one app session:

1. current explicit Project Outputs browse/text value;
2. valid persisted `Mo2ModsRoot`;
3. a user-confirmed discovery suggestion;
4. empty, requiring explicit selection.

Discovery never overrides a current or persisted value. A discovered path is
a suggestion until the user selects it. The backend CLI continues to require
explicit `--mo2-mods-root`; it does not read desktop settings or discover MO2.

Any path or mod-name change invalidates the export preview token. Persistence
does not weaken Gate 392 destination checks.

## Discovery result contract

A future discovery adapter may return candidates only as structured local
evidence:

```text
candidateId
instanceKind: portable | global | unknown
instanceName
executablePath
configurationPath
baseDirectory
modsRoot
evidence[]
status: candidate | invalid | unsupported
reason
```

Candidate ordering must be deterministic by normalized configuration path and
must not depend on timestamps, process state, network access, or AI. Discovery
is read-only and must not launch MO2, load its plugins, inspect VFS state,
select profiles, or create missing directories.

## Discovery implementation gate

The repository research and local machine provide no authoritative evidence
for exact MO2 portable/global configuration locations or INI keys. Therefore
Gate 395 does not authorize hard-coded `%LOCALAPPDATA%` paths, wildcard scans,
registry assumptions, or ad hoc INI parsing.

Before implementation, Gate 396 must establish fixture-backed evidence for:

- supported MO2 configuration filenames and versions;
- portable versus global instance roots;
- exact base/mods-directory keys and relative-path expansion;
- environment-variable and token expansion policy;
- malformed/missing configuration behavior;
- duplicate/case-alias candidate handling;
- redistributable synthetic fixture shape.

If that evidence cannot be established, Forge implements persistence only and
keeps discovery unavailable with an explicit open status.

## Acceptance criteria for persistence implementation

- `Mo2Path` and `Mo2ModsRoot` remain distinct through load/save/reset.
- A valid persisted root populates Project Outputs without creating files.
- Invalid persisted roots are visible but cannot enable preview/export.
- Browse remains authoritative for the current session.
- Preview token invalidates on settings/root/name changes.
- Local settings tests cover old files without `Mo2ModsRoot`, round-trip,
  reset, invalid path, and executable/mods-root separation.
- No profile, VFS, priority, load-order, plugin, Data, or Overwrite mutation.

## Next route

Gate 396: research and validate the exact MO2 portable/global instance
configuration contract using authoritative evidence and redistributable
synthetic fixtures, then either authorize deterministic discovery or formally
defer it and route directly to persisted-settings implementation.
