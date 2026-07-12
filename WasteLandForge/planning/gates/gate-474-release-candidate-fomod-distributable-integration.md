# Gate 474 - Release Candidate FOMOD Distributable Integration

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 471-473

## Goal

Make the deterministic FOMOD archive a first-class Release Candidate artifact
without breaking the verified loose combined-package path used for MO2 test
copies.

## Delivered

- Added `FOMOD distributable` as a mandatory fourth-stage candidate pipeline:
  validate, combined package, FOMOD distributable, release verification.
- Candidate readiness now requires successful FOMOD generation; a blocked
  FOMOD stage prevents release verification.
- Added typed FOMOD root, archive, manifest, build-manifest, and checksum
  evidence to the candidate result.
- Added preferred `Open Distributable Folder` and `Open FOMOD ZIP` actions.
- Retained explicit combined-payload folder/ZIP actions for inspection and MO2
  test-copy reuse.
- Preserved the existing digest-bound MO2 export path against
  `dist/mod-package`; it does not consume or execute the FOMOD installer.
- Added stage order, short-circuit, evidence projection, and containment tests.
- Published the app shell, rebuilt the unsigned installer, and proved that the
  installed Candidate workflow recreates a deliberately removed FOMOD output
  before enabling distributable actions.
- Passed all 795 solution tests serially with MSBuild node reuse disabled.

## Boundaries

This gate creates and exposes a local candidate distributable. It does not
publish, upload, sign, timestamp, run a mod manager installer, mutate MO2
profiles, write game Data, alter plugins, use remote APIs, or use AI.

The FOMOD archive is preferred for human distribution. The combined package
remains an internal loose-payload and MO2 test-copy artifact; the two evidence
roles are intentionally separate.

## Next route

Gate 475: make `forge release prepare` stage the verified FOMOD archive and its
evidence as the concrete local release payload, still stopping before signing,
remote publication, or mod-manager execution.
