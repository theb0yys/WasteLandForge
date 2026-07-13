# Gate 495 - Bounded BSArch Runner and Approval UX

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gates 492-494

## Goal

Expose Gate 494 execution through a bounded local Windows process runner and an
explicit preview-then-approve CLI/desktop workflow, proving the installed path
with a controlled synthetic executable before any real-provider smoke.

## Implemented

- Added a no-shell process runner using `ProcessStartInfo.ArgumentList`, hidden
  process creation, redirected output, bounded capture, timeout, cancellation,
  and process-tree termination.
- Enabled `forge package --target bsa-bsarch --packer <path> --approve <sha>`.
- Requires an exact 64-hex approval SHA and refuses approval in dry-run mode.
- Recomputes the preview inside the coordinator before any provider execution.
- Added desktop approval state that is invalidated by project/provider changes.
- Added a distinct `Build BSA` action that remains disabled until a successful
  preview and presents provider plus approval identity in a confirmation dialog.
- Added runner output/timeout tests and CLI approval-shape regressions.
- Added a repository-owned synthetic BSArch fixture executable implementing
  only the exact probe/pack/list/unpack test contract.
- Extended installed regression to verify preview remains write-free, execution
  becomes available only after preview, and the installed backend promotes the
  verified synthetic BSA evidence.

## Claim classification

- `Documented`: Gate 492 requires explicit provider identity, preview approval,
  exact arguments, bounded process execution, verification, and provenance.
- `Documented`: Gate 494 implements and tests the injectable coordinator and
  retains the concrete process/UX boundary for this gate.
- `Inferred`: a repository-owned deterministic executable is appropriate proof
  of process orchestration and installed wiring, not proof of BSA compatibility.
- `Open`: compatibility with an actual upstream TES5Edit BSArch executable has
  not been tested.

## Validation

- Release solution build: passed with zero errors.
- Unit tests: 133 passed.
- Schema tests: 145 passed.
- Semantic tests: 87 passed.
- Golden tests: 307 passed.
- Windows tests: 142 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 859 passed, 0 failed, 0 skipped.
- Synthetic BSArch fixture restore/publish: passed.
- PowerShell installed-regression syntax: passed.
- App/backend publication and unsigned local installer build: passed.
- Installed WPF/backend synthetic BSArch regression, uninstall, and isolated
  cleanup: passed.
- NuGet vulnerability-feed lookup remained unavailable and emitted `NU1900`;
  offline build and tests passed.

## Boundaries

- No real BSArch provider was installed, downloaded, redistributed, or run.
- Synthetic ZIP bytes are not claimed to be BSA-format compatibility evidence.
- No plugin, game Data directory, MO2 profile, INI, or load order was mutated.
- The installer remains unsigned and local; no release was published.

## Next route

Gate 496: perform a separately authorized real-provider compatibility smoke
against a user-supplied upstream `bsarch.exe`, using only an isolated synthetic
project and verifying probe, pack, repeatability, list, unpack, and cleanup. If
no authorized provider is available, record the gate as deferred rather than
claim compatibility.
