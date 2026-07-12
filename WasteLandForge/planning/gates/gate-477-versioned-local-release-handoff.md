# Gate 477 - Versioned Local Release Handoff

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 471-476

## Goal

Turn a fresh desktop Candidate result into an explicit local release handoff by
copying the prepared FOMOD release archive to a user-selected folder with a
deterministic versioned name, checksum, and handoff evidence.

## Delivered

- Added a digest-bound preview that performs no writes and exposes the exact
  archive, checksum, and evidence destinations before creation.
- Added explicit local handoff creation for fresh Candidate-ready results.
- Derived the deterministic filename from validated project name and version,
  producing `Combined-Mod-Example-0.1.0.zip` for the synthetic fixture.
- Revalidated the Candidate source fingerprint, prepared archive path, length,
  digest, staging payload status, and destination immediately before copying.
- Verified copied archive bytes and emitted a SHA-256 sidecar plus structured
  local handoff evidence.
- Refused overwrite, missing or reparse-point destinations, stale previews,
  stale source, and changed prepared archives.
- Added desktop destination selection, preview, create, status/evidence, and
  contained destination-opening controls.
- Added focused helper coverage and installed-app automation for preview-no-write
  and successful handoff creation.

## Verification

- Full solution: 800 tests passed serially with MSBuild node reuse disabled.
- Focused local handoff suite: 2 tests passed.
- App shell and backend republished; unsigned Inno Setup installer rebuilt.
- Installed regression proved preview-no-write, explicit creation, versioned
  archive, checksum/evidence files, destination action, uninstall, and cleanup.

## Boundaries

This is a local file handoff only. It does not upload, remotely publish, sign,
timestamp, attest, install into a mod manager, mutate a profile or load order,
write to game Data, alter plugins, run external modding tools, or use AI.

## Next route

Gate 478: close the local release handoff lane by adding read-only verification
of an existing handed-off archive against its checksum and evidence, then route
to the next major value function rather than extending release edge cases.
