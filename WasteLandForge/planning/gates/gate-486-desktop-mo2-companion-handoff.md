# Gate 486 - Desktop MO2 Companion Package Handoff

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gates 483-485

## Goal

Expose the verified optional MO2 companion package and install/remove guide as
a read-only desktop handoff, include that evidence in app/installer publication,
and refuse stale or tampered evidence without detecting or modifying MO2.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Gate 486 must surface the package and guide through a read-only desktop handoff and publication. | Documented | Gate 485, lines 82-87 |
| The package is optional, manually installed, and must not locate or alter MO2. | Documented | Gate 485, lines 52-61 and 74-80 |
| Open actions may target only evidence revalidated under the published package root. | Inferred | ADR-009 provenance requirements and Gate 485 integrity evidence |
| Exact live MO2/Python compatibility remains unproven. | Open | Gate 485, lines 69-72 |

## Implemented

- App publication deterministically rebuilds the Gate 485 package and copies
  the ZIP, external checksum, build manifest, and standalone `INSTALL.md` under
  `Integrations/MO2/Package` before app manifest/checksum generation.
- Installer preflight makes all four files mandatory and verifies they are
  covered by the app distribution checksum sidecar.
- `Mo2CompanionPackageHandoff` requires direct-child, non-reparse evidence;
  exact manifest identity and no-execution boundaries; matching ZIP length and
  SHA-256; exact external sidecar; and matching install-guide digest.
- The Project Outputs desktop panel enables **Open Package Folder**, **Open
  Package ZIP**, and **Open Install Guide** only after verification. File actions
  revalidate immediately and accept only the resolved archive or guide path.
- No install, removal, MO2 discovery, configuration, profile/load-order change,
  executable registration, or MO2 launch behavior was added.

## Verification

- Full .NET suite: 829 passed, 0 failed.
- Focused handoff service tests: 5 passed, covering valid evidence plus archive,
  sidecar, guide, and boundary tamper refusal.
- Gate 485 deterministic package regression passed.
- PowerShell syntax and diff whitespace checks passed.
- Self-contained `win-x64` publication passed after the explicit runtime restore.
- Installer input preflight and unsigned Inno Setup compilation passed.
- Installed UI regression verified all four contained files, recomputed the ZIP
  digest, observed the verified status, found all three handoff actions enabled,
  completed prior release/tool regressions, uninstalled, and cleaned test state.

## Boundaries

- No live MO2, GECK, xEdit, game, Python runtime, or third-party binary was
  launched by Gate 486.
- The desktop does not extract the ZIP or write outside its existing private
  request creation and established project workflows.
- Checksums prove byte integrity only, not publisher identity, compatibility,
  editor readiness, VFS contents, or mod correctness.

## Next route

Gate 487: implement read-only MO2 launch-receipt discovery and verification in
the desktop so a request can show process-created status, selected instance and
profile, PID, and digest linkage. Receipt display must make no editor/VFS/mod
correctness claim and must not poll, retry, launch, or mutate MO2.

