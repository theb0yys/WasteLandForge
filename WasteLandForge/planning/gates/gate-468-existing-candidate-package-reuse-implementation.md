# Gate 468 - Existing-Candidate Package Reuse Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 467, ADR-004, ADR-009, ADR-010, ADR-011

## Implemented

- Added opt-in `forge package --target mod-package --reuse-existing-package`
  for named MO2 exports; rebuild remains the default.
- Added a typed existing-package loader that verifies tool/project identity,
  caller-bound package/build manifest SHA-256 and length, source files, staged
  entries, complete checksum coverage, ZIP digest, entry order/content, and
  reparse/missing/unexpected entry failures before export.
- Reconstructed the normal `ModPackageResult`, so Gate 392 atomic copy,
  destination refusal, digest verification, rollback, and evidence behavior is
  reused without a second copy engine.
- Added `packageSource: existing-verified` to structured CLI results.
- Release Candidate preview and creation now pass approved package evidence to
  the backend. Successful test-copy creation leaves the four candidate-bound
  files byte-identical and preserves `Candidate ready`.
- Added `WF-BUILD-015` for existing-package reuse evidence failures.

## Verification

- Solution build passed.
- Unit 119, Schema 140, Semantic 87, Golden 304, Windows 96, and Backwards
  compatibility 45 passed: 791 tests total.
- Focused reuse tests prove byte stability and source-tamper refusal.
- Self-contained app/backend publication and unsigned Inno Setup build passed.
- Installer size: 48,523,598 bytes.
- Installer SHA-256:
  `35864448e088335537b2ec099021bce7af3245b7321360202bd3ac2c98c9dd8c`.
- Installed UI Automation proved fresh candidate gating, no-write preview,
  direct-child loose export, evidence handoff, preserved `Candidate ready`,
  uninstall, and isolated cleanup.

## Immutable evidence decision

The immutable `mo2-export-manifest/0.1.0` schema was not modified. It already
records the exact package manifest/archive/build-manifest digests. A first-class
`packageSource` field in export evidence requires a new schema version; only
the CLI result exposes that field in this gate.

## Boundaries

- No package repair or evidence rewriting.
- No MO2 launch/profile/priority/load-order mutation, game Data write, game
  launch, overwrite, external tools, network correctness path, or AI.

## Next route

Gate 469: harden existing-package reuse with immutable schema evaluation,
build-manifest cross-reference checks, CLI help/golden coverage, and a new
versioned MO2 export manifest contract for explicit package-source provenance.
