# Gate 475 - FOMOD Release Prepare Payload Staging

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 471-474

## Goal

Turn the verified FOMOD candidate into the concrete local release-prepare
payload without signing, uploading, publishing, or executing an installer.

## Delivered

- `forge release prepare` detects a complete `dist/fomod` candidate evidence
  set and verifies package, FOMOD manifest, build manifest, and checksum
  consistency before writing.
- Verified artifacts are copied byte-for-byte into
  `dist/release-prepare/staging/distributable/`.
- The deterministic local release ZIP now contains eight ordinal entries for a
  FOMOD-aware project: four release evidence files and four staged
  distributable/evidence files.
- `release-payload.json` records `staged-fomod`, the required-files 5.0 package
  type, staged archive path, and four immutable source digests.
- Release archive plans and archive evidence validate the exact dynamic entry
  set, stored compression, deterministic timestamps, and archive digest.
- Release build provenance records the four FOMOD source digests while the
  outer checksum set remains stable and covers the containing release ZIP.
- Partial, malformed, checksum-mismatched, or manifest-mismatched FOMOD
  evidence is refused before writes; an existing prepared release is
  preserved.
- Legacy projects with no `dist/fomod` directory retain the previous
  evidence-only skeleton behavior for backwards compatibility.
- Release-publish dry-run semantic validation now strictly supports both
  legacy and FOMOD-aware evidence contracts without publishing anything.
- Release preparation uses the established Windows output fallback for nested
  directories, copies, UTF-8 files, and archive promotion.

## Verification

- Full solution: 796 tests passed serially with MSBuild node reuse disabled.
- Focused tests prove byte-identical staging, exact eight-entry ZIP layout,
  source digest evidence, tamper refusal, and prior-output preservation.
- App shell and standalone backend were republished.
- The unsigned local installer was rebuilt.
- Bundled `ForgeBackend/forge.exe` independently packaged and prepared an
  isolated synthetic project, reported `staged-fomod`, produced eight exact
  release entries, and cleaned all smoke state.

## Boundaries

No release is uploaded or published. No signing, timestamp service,
attestation, remote API, installer execution, MO2/Vortex automation, game Data
write, plugin mutation, runtime probe, or AI behavior is introduced.

## Next route

Gate 476: add release preparation as the final desktop Release Candidate stage
and expose the prepared release archive/evidence actions while preserving the
combined-package MO2 test-copy path.
