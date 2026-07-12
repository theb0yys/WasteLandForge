# Gate 479 - Explicit GECK Launch Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-002, ADR-004, ADR-008, ADR-009, ADR-010, ADR-011 and Gates 399-400, 449-453

## Goal

Define one safe desktop integration that launches a user-selected `GECK.exe`
from a fresh guided GECK handoff without claiming to automate editor work,
plugin loading, or a mod-manager environment.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge should orchestrate existing ecosystem tools rather than replace GECK. | Documented | FNV Tooling Ecosystem report / ADR-002 |
| GECK remains authoritative for plugin records and raw record editing remains external. | Documented | FNV Asset Pipeline report / ADR-004 |
| Provider detection is local-first and deterministic, install scope is first-class, and capability state gates launch. | Documented | R005 / ADR-008 |
| GECK is detected as the `editor.geck` capability through an executable-tool provider and may be supplied by game root or explicit tool path. | Documented project behavior | Built-in capability catalogue/scanner and Doctor planner |
| GECK Extender has mixed-scope installation and the current project has no safe built-in marker for it. | Open | R005 and current capability scanner |
| A direct process launch is useful when the operator has a physical GECK setup, provided the exact executable and empty argument list are previewed and revalidated. | Inferred | ADR-002, ADR-008, existing local settings and guided handoff |
| Research does not establish a supported GECK command-line argument for opening a plugin or project. | Open | Research reports and Gate 399 unresolved-action model |
| Direct launch does not establish MO2 VFS visibility; MO2 exposes a separate profile-aware launch surface. | Documented | R005 |

## Product surface

Extend the existing desktop `GECK Handoff` workspace. Do not add a top-level
CLI command, slash alias, schema version, project field, or generated artifact.

The workspace adds:

- configured executable identity and validation status;
- `Preview GECK Launch`;
- a complete launch preview showing executable, working directory, empty
  arguments, handoff freshness, pending task count, and launch-context warnings;
- `Launch GECK`, disabled until the exact preview is approved;
- transient started/refused/failed status with process ID when available.

The existing Settings `GECK` executable path remains the sole v0.1 source for
launch selection. It is private LocalAppData state and never canonical project
truth. Automatic executable selection from multiple detected candidates is not
introduced.

## Prerequisites

Preview requires all of the following:

1. A successfully loaded `Fresh` guided GECK handoff.
2. All handoff source digests, worklist digest, and false safety flags already
   accepted by `GeckHandoffWorkspace`.
3. A non-empty absolute settings path to an existing regular file named
   `GECK.exe`, compared ordinal case-insensitively.
4. The executable and its parent directory are not reparse points.
5. The executable is readable and its byte length and SHA-256 can be captured.

The selected executable may be Bethesda GECK or a replacement carrying the
same filename. Forge reports only `configured GECK executable`; it must not
identify or endorse GECK Extender from filename alone.

## Preview and approval token

Preview writes nothing and captures:

```text
project root and source fingerprint
handoff root and handoff manifest SHA-256
executable full path, length, last-write UTC, and SHA-256
working directory: executable parent
arguments: none
shell execution: false
elevation: false
MO2/VFS launch: false
```

The approval token is SHA-256 over those canonical fields. Any settings change,
source/handoff drift, task-worklist drift, executable replacement, metadata or
digest change, or selected-project change invalidates approval.

The preview must state:

- GECK will open without a project or plugin argument;
- plugin/master selection and all editor actions remain manual;
- direct launch does not use MO2 VFS, even when MO2 is configured;
- Forge cannot confirm GECK Extender, loaded plugins, editor readiness, or
  successful plugin save from process creation.

## Launch behavior

`Launch GECK` re-runs every prerequisite and recomputes the token immediately
before process creation. It then uses a typed injectable process launcher with:

```text
FileName = exact approved GECK.exe path
WorkingDirectory = exact approved parent directory
UseShellExecute = false
ArgumentList = empty
CreateNoWindow = false
Verb = empty
```

Forge does not invoke `cmd.exe`, PowerShell, a URI, file association, batch
file, shortcut, MO2, or an elevation verb. It adds no environment variables,
redirects no streams, supplies no credentials, and performs no quoting or
string-built command composition.

Successful `Process.Start` means only `process created`. Forge may display the
PID but does not wait for readiness, inspect windows, inject input, retry,
restart, terminate, or monitor the process. Early exit is informational only
when observed without blocking the desktop.

## Refusals

Refuse preview or launch when:

- the handoff is missing, stale, invalid, or unsafe;
- the configured path is relative, missing, not `GECK.exe`, a directory, or a
  reparse point;
- the executable parent is a reparse point;
- the executable cannot be read or hashed;
- the approved executable or handoff evidence changed;
- a launch is already being submitted by Forge;
- process creation throws or returns no process.

Failures preserve the guided session, task ledger, canonical source, generated
handoff, settings, and executable bytes. There is no automatic fallback to a
game-root executable or MO2.

## Safety boundaries

- No plugin/project/master arguments.
- No GECK record creation, editing, saving, or script compilation.
- No xEdit, MO2, game, or runtime launch.
- No MO2 profile selection, VFS integration, mod enablement, load-order change,
  game Data write, plugin mutation, or generated-output mutation.
- No download, install, update, GECK Extender detection claim, network call,
  telemetry, release operation, or AI behavior.
- The existing handoff manifest continues truthfully recording that generation
  itself launched no tool; transient desktop launch state is not written back
  into deterministic build evidence.

## Acceptance criteria for Gate 480

- Fresh synthetic handoff plus a controlled `GECK.exe` test stub produces the
  exact no-argument preview and an approval token.
- A fake injectable launcher receives exact filename, working directory, empty
  arguments, no shell, and no elevation.
- Stale handoff, unsafe handoff flags, wrong filename, relative path, reparse
  point, missing executable, unreadable executable, and post-preview drift are
  refused without process creation.
- Repeated launch clicks cannot create concurrent submissions.
- Settings/project/handoff changes invalidate approval.
- UI remains responsive and clearly distinguishes `process created` from GECK
  readiness or successful mod authoring.
- Focused Windows tests, full solution, app publication, and installed desktop
  regression pass. Installed automation may launch only a controlled synthetic
  stub, never a real GECK, game, MO2, xEdit, or third-party binary.

## Next route

Gate 480: implement the complete preview-gated direct GECK launch slice in the
guided handoff workspace with injectable process creation, refusal tests,
published app proof, and controlled installed-stub regression.
