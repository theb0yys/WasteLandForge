# Gate 458 - Plugin Review-Evidence Promotion Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 457, ADR-004, ADR-005, ADR-007, ADR-009, ADR-011

## Delivered

- Immutable `plugin-review-evidence/0.1.0` schema and catalog registration.
- Shared reviewed-state validation binding artifact ID, Data filename, plugin
  SHA-256/length, evidence identity, snapshotted report SHA-256/length, report
  target, approval decision/reviewer/statement, and disabled safety flags.
- Plugin Intake review controls for loading pending artifacts, choosing existing
  xEdit evidence, entering reviewer identity, explicit approval, previewing exact
  digests/effect, and promoting transactionally.
- Promotion snapshots external evidence byte-for-byte under canonical project
  review source, writes a human approval attestation, updates only the selected
  registry entry, revalidates, and rolls back all created files on failure.
- Package build provenance includes plugin review evidence and report digests.
- Valid reviewed evidence removes the pending package warning and allows release
  verification to continue; any plugin/report/evidence drift blocks validation
  and release again.

## Published regression

- Published WPF app imported a synthetic opaque plugin and release verification
  returned exit code 1 while review was pending.
- UI loaded the pending artifact, bound a synthetic external xEdit review report,
  explicit reviewer `gate458-reviewer`, and the fixed responsibility statement.
- Promotion completed while the app remained responsive.
- Validation and release verification then returned exit code 0.
- Tampering the canonical report snapshot restored release exit code 1.
- Plugin bytes remained unchanged at SHA-256
  `7899c477a8c9b5dbb145542f5276fcaa2e2839cbfe3904ea25196f9d01736847`.
- Isolated project, external plugin, and external report were removed.

## Boundaries

- External reports are human-reviewed evidence; Forge does not interpret their
  findings or claim universal plugin validity.
- No xEdit/GECK launch, plugin mutation, game Data/MO2 write, external-tool
  execution, network operation, release publication, or AI.

## Validation

- Focused Windows promotion/rollback tests: 2 passed.
- Focused schema test: 1 passed.
- Full release suite: 770 passed, zero failures, zero skips.
- App publication and published UI/release/tamper regression passed.
- NuGet emitted `NU1900` because api.nuget.org vulnerability metadata was
  unavailable; cached builds completed.

## Next route

Gate 459: rebuild the unsigned installer and run isolated installed plugin
review promotion covering pending refusal, explicit approval, restart-visible
reviewed state, release pass, report/evidence tamper re-blocking, rollback,
uninstall, and cleanup.
