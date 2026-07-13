# Gate 496 - Real BSArch Compatibility Smoke Deferred

Status: Deferred
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gates 492-495

## Goal

Perform a separately authorized compatibility smoke with a user-supplied
upstream `bsarch.exe`, or record the gate as deferred without claiming real BSA
compatibility when no authorized provider is available.

## Read-only preflight

- Checked `PATH` resolution for `bsarch.exe`.
- Checked repository configuration and documentation references for a supplied
  provider path.
- Checked the documented standard candidate locations under `C:\Tools`,
  machine xEdit folders, and the per-user Programs xEdit folder.
- Reconfirmed Gate 492's earlier read-only discovery result.

No real `bsarch.exe` was found. The repository-owned
`artifacts/synthetic-bsarch/bsarch.exe` fixture was intentionally excluded: it
proves orchestration only and is not an upstream provider.

## Claim classification

- `Documented`: Gate 492 requires a user-supplied local provider and explicit
  authorization before a real compatibility smoke.
- `Documented`: Gate 495 requires this gate to be deferred when no authorized
  provider is available.
- `Open`: accepted upstream BSArch versions, hashes, banner behavior, FNV pack
  output, repeatability, list output, and unpack behavior remain unverified.

## Actions deliberately not taken

- Did not search unrelated drives recursively.
- Did not download, install, copy, redistribute, or execute a real provider.
- Did not execute the synthetic fixture and describe it as real compatibility.
- Did not create or promote new BSA output.
- Did not mutate plugins, game Data, MO2 profiles, INIs, or load order.

## Validation

- Read-only provider discovery: completed; no real provider found.
- Repository changes: documentation only.
- Build and tests: not rerun because production code, schemas, fixtures, and
  executable behavior did not change from the fully validated Gate 495 state.

## Unblock condition

The gate can resume only when the user supplies the exact absolute path to a
real upstream `bsarch.exe` and explicitly authorizes launching that file against
an isolated synthetic project. Forge must preview and report its path, length,
SHA-256, version metadata, signature status, and approval token before launch.

## Next route

Gate 497: close the optional BSA execution lane for v0.1 with real-provider
compatibility explicitly deferred, prevent unverified BSA output from becoming
a mandatory release input, and select the next repository-local value slice.
