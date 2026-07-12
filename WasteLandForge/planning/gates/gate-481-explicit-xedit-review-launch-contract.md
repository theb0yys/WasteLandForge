# Gate 481 - Explicit xEdit Review Launch Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-002, ADR-004, ADR-005, ADR-008, ADR-009, ADR-010, ADR-011 and Gates 218-228, 380-381, 454-459, 479-480

## Goal

Define a safe desktop launch from a selected pending plugin into the existing
xEdit review-evidence workflow without inventing command-line switches,
automating module selection, or claiming that process creation proves review.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge should orchestrate xEdit for inspection and conflict work rather than replace it or silently author patches. | Documented | FNV Tooling Ecosystem report, Generator/Build report, ADR-002 and ADR-004 |
| xEdit is a multi-capability external tool for inspection, cleaning, conflict analysis, and scripting. | Documented | R005 / ADR-008 |
| The built-in provider scanner recognises `FNVEdit.exe` and `xEdit.exe` as the xEdit executable-tool provider. | Documented project behavior | Built-in capability catalogue/scanner |
| Pending plugin review is bound to exact contained plugin bytes and later requires explicit human-approved report evidence. | Documented project behavior | Gates 454-459 |
| Local research states that xEdit has command-line switches but does not define a supported plugin-selection invocation. | Open | R005 and available local research |
| Official-source lookup in this gate returned no usable command-line source content, so a plugin argument cannot be justified. | Open | Gate 481 research attempt |
| A no-argument launch with visible selected-plugin guidance is useful and preserves manual module selection. | Inferred | ADR-002, ADR-004, Gate 457 workflow, Gate 479 launch discipline |

## Decision

Gate 481 selects **no-argument direct launch** for v0.1.

The selected pending plugin is displayed as the review target and bound into
the approval token, but is not passed to xEdit. The operator selects the module,
masters, and any xEdit actions manually after launch.

Plugin-selection switches, autoload, scripts, quick-auto-clean modes, game-mode
overrides, report paths, and unattended execution remain excluded until an
authoritative source establishes exact semantics and safety.

## Product surface

Extend `Plugin Intake` beside `Attach xEdit review evidence`:

- reuse `Load Pending Plugins` and the selected pending artifact;
- show the configured xEdit executable and selected review target;
- add `Preview xEdit Launch`;
- show exact executable, working directory, empty arguments, plugin identity,
  plugin SHA-256/length, and direct-launch warnings;
- add `Launch xEdit`, disabled until exact preview approval;
- report transient process-created/refused/failed state and PID.

Do not add a CLI command, slash alias, project field, schema version, generated
artifact, or automatic report ingestion. Existing `Process Existing Report`
and review promotion remain separate explicit user actions.

## Prerequisites

Preview requires:

1. A selected project that loads through `PluginArtifactRegistryReader` with no
   blocking diagnostics.
2. Exactly one selected artifact whose `reviewStatus` remains `pending`.
3. The plugin source path resolves inside the selected project, exists as a
   regular non-reparse file, and matches registry filename, length, and SHA-256.
4. An absolute configured xEdit path to an existing regular non-reparse file
   named `FNVEdit.exe` or `xEdit.exe`, ordinal case-insensitive.
5. The executable parent exists and is not a reparse point; executable bytes
   are readable and hashable.

Forge does not auto-select a detected candidate or fall back between executable
names. The existing private LocalAppData Settings path is the sole v0.1 source.

## Preview and approval token

Preview writes nothing and captures:

```text
project root
plugin artifact ID, registry file, contained source path
plugin Data path, length, and SHA-256
plugin registry source digest
xEdit executable path, parent, length, last-write UTC, and SHA-256
arguments: none
shell execution: false
elevation: false
MO2/VFS launch: false
```

The token is SHA-256 over those canonical fields. Project selection, plugin
selection, registry/plugin drift, review-status change, settings change,
executable drift, or path change invalidates approval.

Preview must state:

- xEdit opens without module-selection or automation arguments;
- the named plugin is guidance only and must be selected manually;
- xEdit may allow the human operator to modify plugin files, but Forge does not
  request, perform, observe, or approve edits;
- direct launch does not use MO2 VFS;
- process creation does not prove module load, Check for Errors completion,
  conflict review, report creation, plugin validity, or release approval;
- review promotion still requires separately selected evidence and explicit
  human approval through the existing Gate 458 transaction.

## Launch behavior

Launch re-reads the plugin registry and bytes, revalidates the executable, and
recomputes the token immediately before process creation. It uses the same
typed injectable process boundary as Gate 480, generalized only when doing so
does not change GECK launch behavior:

```text
FileName = exact approved FNVEdit.exe or xEdit.exe path
WorkingDirectory = exact executable parent
UseShellExecute = false
ArgumentList = empty
CreateNoWindow = false
Verb = empty
```

No shell, URI, shortcut, batch file, elevation, environment addition, stream
redirection, credential, string-built command, MO2 mediation, or retry is used.

Successful `Process.Start` means only `process created`. Forge may display PID
but does not wait, inspect windows, send input, monitor, restart, or terminate
xEdit. Launch state is transient and is never written into canonical review or
build evidence.

## Refusals

Refuse preview or launch for missing/invalid project state, no or ambiguous
selection, non-pending status, registry/plugin digest drift, escaped or
reparse-point plugin path, wrong/missing/relative/reparse-point executable,
stale approval, concurrent submission, process exception, or null process.

Failure preserves plugin, registry, reports, review evidence, generated output,
settings, and executable bytes. There is no fallback launch.

## Safety boundaries

- No module/plugin/master arguments or automatic selection.
- No xEdit script, cleaning mode, Check for Errors, conflict scan, patching,
  save, report generation, parsing, or promotion triggered by launch.
- No plugin mutation by Forge and no claim that xEdit or the human did not edit.
- No MO2/profile/VFS integration, game Data write, load-order mutation, GECK or
  game launch, network call, release operation, telemetry, or AI.
- Existing review evidence remains explicit, digest-bound, human-approved, and
  separate from transient process creation.

## Acceptance criteria for Gate 482

- A synthetic pending plugin plus controlled `FNVEdit.exe` and `xEdit.exe`
  stubs each produce a no-write, no-argument preview.
- Injected launcher receives exact executable, parent working directory, empty
  arguments, no shell, and no elevation.
- Wrong executable name, relative/missing/reparse path, escaped/missing/tampered
  plugin, registry drift, reviewed status, selection change, executable drift,
  null process, and concurrent submission are refused without launch.
- Preview clearly names the review target while stating it is not passed as an
  argument and that review evidence remains a separate human action.
- GECK launch tests remain unchanged and passing after any shared process
  boundary extraction.
- Focused Windows tests, full solution, publication, installer build, and a
  controlled installed-stub regression pass without launching real xEdit,
  GECK, MO2, game, or third-party binaries.

## Next route

Gate 482: implement the complete preview-gated no-argument xEdit review launch
slice in Plugin Intake with digest-bound pending-plugin selection, injectable
process creation, strict refusal coverage, and controlled installed-stub proof.
