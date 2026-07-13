# Gate 502 - xEdit Check Report Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-005, ADR-009, ADR-010, ADR-011 and Gate 501

## Goal

Implement the first deterministic, read-only xEdit `Check(e)` report slice from
validated source declaration through generated Pascal, provenance-bound report
ingestion, typed diagnostics, desktop review, publication, and installed proof.

## Implemented

- Added immutable `xedit-audit/0.2.0` and
  `xedit-check-report/0.1.0` schemas while preserving audit `0.1.0` routing.
- Generated a deterministic manual Pascal script using the documented
  `Initialize`, `Process`, `Finalize`, `Check`, `Signature`, `EditorID`,
  `FixedFormID`, `GetLoadOrderFormID`, and file-name APIs.
- Added bounded report parsing with current script and plugin digest binding,
  subject/filter checks, duplicate refusal, deterministic ordering, and no
  plugin mutation.
- Added `WF-GEN-015` for malformed or untrusted report evidence and
  `WF-SEM-045` for accepted xEdit record errors.
- Projected report provenance and typed findings through the existing xEdit
  audit handoff, CLI explain surface, desktop outputs, and Candidate evidence.
- Added synthetic schema, unit, golden, Windows, publication, installer, and
  installed-package regression coverage.

## Evidence classification

- **Documented:** xEdit exposes `Check(e)` and the record identity APIs used by
  the generated script. Gate 501 records the authoritative documentation.
- **Documented:** WastelandForge owns validation, report ingestion, generated
  evidence, diagnostics, and workflow integration, but not plugin mutation or
  xEdit conflict resolution.
- **Inferred:** a Forge-owned, digest-bound JSON envelope is the narrowest
  deterministic bridge between manual xEdit execution and Forge diagnostics.
- **Open:** the generated Pascal has not been compiled or run in a real user
  xEdit/FNV installation. Exact runtime compatibility remains an explicitly
  authorized manual smoke rather than a core or installed test claim.

## Validation

- Full repository test suite: 864 passed, zero failed, zero skipped.
- Release build and desktop/backend publication passed; the build reported only
  offline NuGet vulnerability-feed warnings (`NU1900`).
- Rebuilt installer:
  `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Installer SHA-256:
  `606FEA8258BA7BFA9151880A498275B510DC368C38CA02556F0249CEDE5853E7`.
- Installed release-candidate regression generated the xEdit audit script,
  ingested an exact digest-bound synthetic report, projected `WF-SEM-045`, and
  verified the opaque plugin bytes remained unchanged.

## Boundaries preserved

- No xEdit process was started and no xEdit binary was bundled.
- No plugin was parsed, rewritten, promoted, patched, or copied into fixtures.
- No load order, MO2 profile, game Data directory, or external installation was
  changed.
- Synthetic public evidence contains no Bethesda or third-party mod content.

## Next route

Gate 503: perform a read-only local xEdit/FNV compatibility preflight and either
run the generated script only with an exact user-supplied executable/project
and explicit authorization, or record the live smoke deferred and close the
xEdit Check lane without weakening Gate 502's deterministic synthetic proof.
