# Gate 476 - Desktop Candidate Release Preparation Integration

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 460-475

## Goal

Make the prepared FOMOD release archive part of desktop Candidate readiness and
expose its contained local evidence without changing the MO2 test-copy path.

## Delivered

- Extended the ordered Candidate pipeline to five stages:
  validation, combined package, FOMOD distributable, release verification, and
  release preparation.
- Candidate-ready now requires successful `forge release prepare`; verification
  or preparation failure blocks readiness and preserves exact diagnostics.
- Added typed prepared root, release ZIP, staging payload, build-manifest, and
  checksum evidence to the Candidate result.
- Added contained `Open Prepared Release` and `Open Release ZIP` actions.
- Retained separate FOMOD candidate, combined payload, release verification,
  and MO2 test-copy evidence/actions.
- Added ordering, evidence projection, verification short-circuit, preparation
  failure, stale-source, cancellation, and containment tests.
- Hardened installed WPF combo automation for popup list items rendered outside
  the ComboBox subtree.

## Verification

- Full solution: 798 tests passed serially with MSBuild node reuse disabled.
- Focused Candidate suite: 10 tests passed from the rebuilt test assembly.
- App shell and backend republished; unsigned Inno Setup installer rebuilt.
- Installed regression proved FOMOD recreation, `staged-fomod` release
  preparation, prepared archive/evidence actions, existing MO2 test-copy,
  uninstall, and cleanup.

## Boundaries

Candidate preparation remains local and deterministic. No remote publication,
upload, signing, timestamp service, attestation, installer execution,
MO2/Vortex profile mutation, game Data write, plugin mutation, runtime probe,
or AI behavior is added.

## Next route

Gate 477: add a digest-bound, preview-gated local release handoff that copies
the prepared archive to a user-selected folder with a versioned deterministic
filename, while refusing overwrite and stopping before remote publication.
