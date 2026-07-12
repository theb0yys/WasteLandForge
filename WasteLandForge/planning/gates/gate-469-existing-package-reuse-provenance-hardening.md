# Gate 469 - Existing-Package Reuse Provenance Hardening

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 468, ADR-009, ADR-010, ADR-011

## Implemented

- Existing mod-package reuse now evaluates the immutable
  `mod-package-manifest/0.1.0` schema using an isolated schema registry.
- Build-manifest source digests and included components are cross-checked
  against package-manifest evidence.
- Complete checksum coverage and ZIP entry order/content remain mandatory.
- Added immutable `mo2-export-manifest/0.2.0` with required `packageSource`
  provenance (`rebuilt` or `existing-verified`) and plugin-artifact entry
  support. Version `0.1.0` remains unchanged and resolvable.
- New exports use `0.2`; reuse tests assert `existing-verified` evidence.
- `forge package --help` documents reuse constraints and includes an example.

## Verification

- Solution build passed.
- Unit 119, Schema 140, Semantic 87, Golden 304, Windows 96, and Backwards
  compatibility 45 passed: 791 tests total.
- Focused schema/reuse/provenance tests passed after the full suite.
- Self-contained publication and unsigned installer build passed.
- Installer size: 48,527,644 bytes.
- Installer SHA-256:
  `8d0be9ebe57aec7890bd8ee7c6298a5ca2da980a15fb3e09e301fc1cb63118e2`.
- Installed test-copy regression passed with preserved Candidate-ready state,
  uninstall, and isolated cleanup.

## Boundaries

- No existing immutable schema was modified.
- No MO2 profile/load-order automation, game Data write, launch behavior,
  package repair, network correctness path, or AI was added.

## Next route

Gate 470: close the verified MO2 test-deployment lane, audit its user-facing
readiness and remaining non-blocking debt, then route the next major product
value slice rather than adding more MO2 edge-case gates.
