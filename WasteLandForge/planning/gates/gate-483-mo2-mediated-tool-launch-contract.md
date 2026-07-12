# Gate 483 - MO2-Mediated External-Tool Launch Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-002, ADR-004, ADR-008, ADR-009, ADR-010, ADR-011 and Gates 395-398, 479-482

## Goal

Define the minimum safe optional MO2 integration that can launch an approved
GECK or xEdit process inside an explicitly selected MO2 profile without using
undocumented `ModOrganizer.exe` arguments or making MO2 a core requirement.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| MO2 changes effective visibility through profiles and VFS-backed process launch. | Documented | R005 / ADR-008 |
| MO2 exposes `startApplication(executable, args, cwd, profile, ...)`, profile enumeration, current profile, and executable-list APIs to in-process plugins. | Documented | Official MO2 `OrganizerProxy` source |
| `OrganizerProxy::startApplication` delegates to MO2's process runner and does not wait for completion. | Documented | Official MO2 `src/organizerproxy.cpp` |
| The caller owns the returned process handle unless `waitForApplication` is called. | Documented | Official MO2 `src/organizerproxy.cpp` comment |
| The official Python binding returns the native handle as an integer, publishes `INVALID_HANDLE_VALUE`, and exposes blocking `waitForApplication`. | Documented | Official `modorganizer-plugin_python` `basic_classes.cpp` and tests |
| Current Forge discovery identifies bounded portable/global instance configuration and mods roots, but does not inspect or choose profiles. | Documented project behavior | Gates 395-398 and `Mo2InstanceDiscovery` |
| Local and official evidence does not establish a supported external CLI for profile-aware launch. | Open | R005 and Gate 483 evidence review |
| A user-approved request consumed by an optional in-process MO2 companion is the smallest supported bridge from the desktop to `startApplication`. | Inferred | ADR-002, ADR-008, official plugin API, Gates 480-482 |

## Primary evidence

Official repositories:

- `https://github.com/ModOrganizer2/modorganizer`
- `https://github.com/ModOrganizer2/modorganizer-plugin_python`

Pinned MO2 source revision:

```text
efe2a02d5dc641946baaa8db1440800f38d07837
```

Relevant source:

- `src/organizerproxy.h`: `instanceName`, `profileName`, `profileNames`,
  `getProfile`, `executablesList`, `startApplication`, and
  `waitForApplication` interface implementation.
- `src/organizerproxy.cpp`: process-runner delegation and returned-handle
  ownership.
- `modorganizer-plugin_python/src/mobase/wrappers/basic_classes.cpp`: Python
  handle conversion, `INVALID_HANDLE_VALUE`, and launch/wait bindings.
- `modorganizer-plugin_python/tests/python/test_organizer.py`: binding behavior
  for valid and invalid handles.

The previously referenced generated Python documentation URL returned 404 in
this gate. The source declarations above are therefore the authority for this
contract; no external MO2 command line is authorized.

## Architecture decision

Use two optional components:

1. The existing WastelandForge desktop creates a short-lived, digest-bound
   **MO2 launch request** after the normal GECK or xEdit launch preview.
2. A source-distributed WastelandForge MO2 Python companion imports that file,
   validates it, requires explicit profile selection and launch approval, then
   calls the in-process `IOrganizer.startApplication` API.

There is no socket, named pipe, localhost server, process injection, window
automation, command-line control of `ModOrganizer.exe`, or background polling.
The user explicitly opens MO2, opens the companion, selects the request, selects
a profile, reviews the resolved launch, and clicks launch.

The companion is optional and separately installable. Core validation,
generation, packaging, direct GECK/xEdit launch, and release workflows continue
to work without MO2 or Python.

## Request contract

Desktop requests are private transient JSON under:

```text
%LOCALAPPDATA%/WastelandForge/Mo2LaunchRequests/
  <request-id>.json
```

The user may open this folder and select a request from the companion. Requests
are never written to canonical project source, generated distributions, MO2
configuration, or public fixtures.

Version `0.1` requires:

```text
formatVersion: 0.1
kind: wastelandforge.mo2-launch-request
requestId: random 128-bit lowercase hex
createdUtc
expiresUtc: createdUtc + 15 minutes
project:
  root
  contextKind: geck-handoff | pending-plugin-review
  contextId
  contextSha256
tool:
  kind: geck | xedit
  executablePath
  workingDirectory
  length
  sha256
  arguments: []
safety:
  shellExecution: false
  elevation: false
  profileMutation: false
  executableRegistration: false
  automaticLaunch: false
```

The request filename must equal `<requestId>.json`. JSON is UTF-8 without BOM.
The request ID prevents filename collision; exact request bytes are SHA-256
hashed by the companion. Time limits prevent stale machine-path requests from
remaining launchable but do not establish artifact identity.

Request creation reuses and revalidates Gate 480/482 approval inputs. GECK
requests bind the fresh handoff manifest digest. xEdit requests bind the exact
pending plugin digest and registry context. Executable path, working directory,
length, digest, and empty arguments must match the direct-launch preview.

Creation is preview-gated, create-only, atomic, and refuses overwrite. It does
not launch or require MO2. Changing project, context, tool settings, executable,
or source evidence invalidates request approval.

## Companion validation

The optional Python companion must:

1. Require an explicitly selected regular `.json` file under the exact private
   request root; refuse path escape, reparse points, wrong filename, and aliases.
2. Parse with duplicate-key rejection and require the exact versioned shape,
   enum values, empty arguments, and all safety flags.
3. Require current UTC to be within `[createdUtc, expiresUtc]` and the interval
   to be exactly 15 minutes.
4. Re-read and hash the executable, require exact length/SHA-256, accepted
   basename (`GECK.exe`, `FNVEdit.exe`, or `xEdit.exe` by tool kind), regular
   non-reparse executable and parent, and exact parent working directory.
5. Display `organizer.instanceName()`, `organizer.profileName()`, and sorted
   `organizer.profileNames()`; never derive or invent a profile.
6. Require the user to select one currently reported profile and explicitly
   approve a preview naming instance, profile, tool, executable, empty
   arguments, project context, request digest, and limitations.
7. Revalidate request bytes, expiry, executable bytes, instance, and selected
   profile immediately before launch.

The companion does not need filesystem access to the original project context;
that context is provenance and user guidance. Executable digest and request
digest are the machine-local launch identity. Canonical project correctness
remains owned by Forge before request creation.

## MO2 launch behavior

The only authorized call is:

```python
handle = organizer.startApplication(
    executable_path,
    [],
    working_directory,
    selected_profile,
    "",
    False,
)
```

The companion must refuse `0`, `None`, and `mobase.INVALID_HANDLE_VALUE`.
For a valid handle it obtains the PID through a narrow injected/native adapter,
then closes its owned Windows handle in `finally`. It must not call blocking
`waitForApplication`, retain the handle, terminate the process, or trigger an
MO2 refresh. Handle close failure is reported but does not imply the launched
process failed.

Successful API return means only `MO2 process created for selected profile`.
It does not prove VFS contents, editor readiness, module load, review, plugin
save, report generation, game runtime correctness, or release approval.

## Consumption and receipt

After valid process creation, the companion atomically writes a private receipt
beside the request:

```text
<request-id>.receipt.json
```

It records request ID/digest, instance name, selected profile, tool kind,
executable digest, PID when available, `processCreated: true`, and UTC. It makes
no correctness claim. Existing receipt causes replay refusal. Receipt creation
failure is visible; the request remains unconsumed but the UI must warn that a
process may already have started and require manual resolution rather than
automatic retry.

The desktop may display receipts read-only in a later gate. Gate 484 need only
create and validate them in the companion.

## Refusals

Refuse request creation or companion launch for invalid/stale direct preview,
unsafe request path/shape, duplicate JSON keys, expiry, request drift,
executable drift, wrong basename, non-empty arguments, unsafe flags, missing or
changed instance/profile, existing receipt, concurrent submission, invalid
handle, API exception, or receipt collision.

No fallback profile, executable, direct desktop launch, MO2 executable launch,
or automatic retry is permitted.

## Safety boundaries

- No MO2 install, update, executable registration, configuration write, profile
  creation/rename/removal, mod enablement, priority/load-order change, Overwrite
  selection, virtual-tree inspection, or plugin-list mutation.
- No GECK/xEdit arguments, automation, report processing, plugin mutation, game
  Data write, game launch, network operation, telemetry, release action, or AI.
- No MO2 or Bethesda binaries in the repository or fixtures.
- The companion source is optional and redistributable; WFG-001 remains intact.
- Automated tests use fake organizer/native adapters and synthetic requests.
  They must never launch a real MO2, GECK, xEdit, game, or third-party binary.

## Acceptance criteria for Gate 484

- Desktop creates an atomic 15-minute request from valid Gate 480 GECK and Gate
  482 xEdit previews without launching MO2 or changing canonical/generated data.
- Optional companion package validates synthetic requests with duplicate-key,
  containment, expiry, digest, filename, arguments, flags, profile, replay, and
  drift refusal coverage.
- Fake organizer receives exact executable, empty arguments, parent working
  directory, explicit profile, empty overwrite, and `False` ignore-overwrite.
- Fake native adapter proves valid handle PID lookup and exactly-once handle
  close; invalid handles, API errors, and concurrent submissions are refused.
- Receipt is atomic, create-only, digest-bound, and makes only a process-created
  claim.
- Core app has no Python/MO2 runtime dependency and all GECK/xEdit direct-launch
  tests remain passing.
- Full .NET suite plus isolated Python companion tests pass. App publication and
  installed desktop regression prove request generation only; no live MO2 or
  third-party process is executed.

## Next route

Gate 484: implement the optional MO2 launch-request and companion-plugin
vertical slice with desktop request generation, strict Python validation,
explicit profile preview, fake-organizer launch tests, handle cleanup, receipt
evidence, publication, and installed request-generation proof.
