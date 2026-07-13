# Gate 547 - Private Single-Master FNVEdit Execution Implementation

Status: Complete - synthetic and installed proof boundary
Phase: post-v0.1 local compatibility remediation
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R005, R009, and Gates 481-484 and
543-546

## Goal

Implement the Gate 546 private single-master xEdit execution contract, prove it
with only a controlled synthetic provider in unit and installed tests, preserve
the manual `0.1.0` workflow, and stop before any retry against real FNVEdit.

## Evidence classification

- **Documented:** Gate 546 requires immutable automated `0.2.0` evidence,
  exact private provider paths, the 12 documented xEdit arguments, digest-bound
  approval, no shell/elevation/retry/kill/UI automation, postflight side-effect
  audits, and synthetic installed proof.
- **Documented:** the official xEdit command-line and change-history references
  cited by Gate 546 remain the authority for `-view`, `-autoload`, `-script`,
  `-autoexit`, and `-D/-P/-S/-C/-T/-B/-R`.
- **Inferred:** a visible two-click in-workspace confirmation is a stronger
  implementation of explicit digest approval than the initial modal dialog,
  because the first click only arms the displayed token and the second click
  executes only if that same preview remains current.
- **Open:** compatibility of the exact local FNVEdit 4.1.5f provider with this
  argument combination and generated Pascal producer remains unproved. No real
  provider retry occurred in this gate.

## Implemented contracts

Gate 547 adds and registers:

```text
fnv-game-knowledge-export/0.2.0
fnv-game-knowledge-index/0.2.0
fnv-game-knowledge-receipt/0.2.0
fnv-game-knowledge-execution-plan/0.1.0
fnv-game-knowledge-execution-receipt/0.1.0
```

The automated lineage requires `forgeExecutedXEdit: true`. The existing manual
`0.1.0` lineage remains registered, validated, loadable, searchable, and able
to produce receipts without relabelling its false execution assertion.

The execution plan binds the exact provider, master, generated script, run
manifest, private `Plugins.txt`, all absolute private paths, the ordered
12-argument array, process flags, allowed private root, protected Data/provider/
user-state roots, preflight tree inventories, and an empty-backup policy. The
approval token is the SHA-256 of the validated plan bytes.

The execution receipt records process identity, UTC start/exit, elapsed
milliseconds, exit code, output/log digests, pre/post inventory digests and
counts, exact observed changed paths, audit booleans, and final state.

## Runtime behavior

- Preparation creates a unique non-reparse private run and does not start a
  process.
- `state/Plugins.txt` contains exactly `FalloutNV.esm` plus CRLF.
- Process creation uses `ProcessStartInfo.ArgumentList`, no shell, no verb, no
  redirection, no elevation, and no added environment variables.
- A same-name running provider, stale token, bound-input drift, dirty reserved
  output, or concurrent submission refuses process creation.
- Cancellation before process creation refuses; cancellation while waiting
  does not terminate or retry the provider.
- Exit code zero is insufficient. Forge requires unchanged Data/provider/user
  inventories, immutable bound inputs, allowed private writes only, an empty
  backup directory, a bounded log, and a strict valid `0.2.0` export before
  atomic index promotion.
- Any failure writes observed failure evidence where possible and leaves an
  existing index byte-identical.

## Desktop behavior

Game Knowledge now exposes **Preview Private xEdit Run** and **Run Export**.
Preview displays the exact executable, working directory, arguments, output
paths, protected-root policy, and approval digest.

The first Run click changes the action to **Confirm Run Export** and states that
no provider has started. The second click executes only the unchanged armed
digest. Preparing or refreshing disarms the confirmation. This replaced an
initial modal implementation after installed testing proved the modal could be
logically active without an accessible window.

An isolated `WASTELANDFORGE_FNV_USER_STATE` override exists for controlled
installed tests. It must be an absolute non-root path; production defaults to
the local FalloutNV user-state directory.

## Synthetic provider proof

`eng/stubs/WastelandForge.FakeFNVEdit` builds a test-only `FNVEdit.exe`. It:

- accepts exactly the approved 12 arguments in their fixed order;
- validates the automated `0.2.0` script and exact private plugin list;
- writes one synthetic CELL record plus only the declared raw export, log,
  private view settings, cache, and temp outputs;
- leaves backups empty and does not write Data, provider, project, or isolated
  user-state evidence.

The installed regression publishes this stub into an isolated temporary tool
root. It does not substitute for or claim compatibility with upstream xEdit.

## Validation evidence

- Release build: passed with zero warnings and zero errors.
- Focused catalogue tests: 11 passed.
- Focused Game Knowledge Windows tests: 9 passed.
- Focused schema and golden/back-compat tests: passed.
- Full serial solution suite: 962 passed, zero failed, zero skipped.
- App-shell publication: passed, including bundled backend verification.
- Inno Setup 6.7.3 unsigned local installer build: passed.
- Installed regression: passed at 960x640 and 1180x760.
- Installed proof covered preview without execution, inline digest confirmation,
  exact arguments/private master list, synthetic process execution, receipt and
  `0.2.0` index, search/receipt/intent handoff, unchanged canonical project,
  Data/provider/user-state trees, empty backups, contained cleanup, uninstall,
  and isolated temporary-root removal.

Final local installer:

```text
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
SHA-256: 676522e0e4c41de2de4cf99b81f0d31844a93449cba8601a321ac0db5970b4a2
Size: 3,985,720 bytes
```

During test iteration, an initial `dotnet test` invocation created excessive
MSBuild workers and was stopped; single-node/no-server runs then passed. Early
installed harness runs failed on transient modal discovery and one incorrect
test-side JSON path. The modal behavior exposed a product defect and was
replaced by inline confirmation. The final rebuilt installer and final suite
both passed.

## Safety evidence

- No real FNVEdit/xEdit, GECK, MO2, FalloutNV, game, or Bethesda plugin ran.
- No plugin, master, game Data, load order, real provider installation, or Gate
  545 retained evidence changed.
- No shell execution, elevation, provider UI automation, retry, timeout kill,
  signing, timestamping, publication, or AI behavior was added.
- No Tales from the Age of Men, Age of Men, or overhaul file was read for
  implementation or changed.

## Next route

Gate 548 may prepare a new exact preview for the configured real FNVEdit
provider and request a separate execution approval. Gate 545 Approval B is not
reusable, Gate 547 grants no real-provider authorization, and no Gate 548 run
may occur without a new explicit user instruction.
