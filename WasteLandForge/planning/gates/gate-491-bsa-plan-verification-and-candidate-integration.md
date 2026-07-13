# Gate 491 - BSA Plan Verification and Candidate Integration

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-007, ADR-009, ADR-011 and Gates 489-490

## Goal

Add read-only deterministic verification of existing BSA-plan evidence and make
that verification visible in the Release Candidate and local release
preparation workflows without creating a BSA or changing the installable FOMOD
payload contract.

## Implemented

- Added `forge package --target bsa-plan --verify-existing`.
- Verification covers the immutable plan schema, expected seven-file evidence
  set, checksum coverage, source package manifest/archive digests, reviewed
  association-plugin identity, exact staged entry bytes, and the explicit
  no-BSA/no-external-tool boundary.
- Missing, malformed, stale, tampered, unsafe, or unexpected-BSA evidence is
  refused with `WF-BUILD-016`, `WF-BUILD-017`, or `WF-SCHEMA-001` diagnostics.
- Expanded the desktop Candidate pipeline to seven ordered stages: validation,
  combined package, FOMOD, BSA plan generation, BSA plan verification, release
  verification, and release preparation.
- Added typed BSA plan evidence projection and contained `Open BSA Plan` and
  `Open BSA Report` actions.
- Release preparation now re-verifies existing BSA-plan evidence and records
  its seven exact source digests in `release-payload.json` and the release build
  manifest.
- The BSA plan remains evidence only. It is not inserted into the installable
  FOMOD archive or represented as a generated BSA.

## Validation

- Release solution build: passed with zero errors.
- Unit tests: 131 passed.
- Schema tests: 142 passed.
- Semantic tests: 87 passed.
- Golden tests: 306 passed.
- Windows tests: 142 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 853 passed, 0 failed, 0 skipped.
- Focused BSA generator/verifier tests: 11 passed.
- Focused Candidate pipeline tests: 12 passed.
- Installed regression script syntax: passed.

## Boundaries

- No BSA parsing, creation, extraction, deployment, or runtime test.
- No packer discovery, download, process launch, or command line.
- No change to the FOMOD distributable ZIP entry contract.
- No plugin, MO2 profile, game Data, INI, load order, or third-party file
  mutation.
- No signing, remote publication, runtime probe, or AI requirement.

## Next route

Gate 492: research and define one explicit third-party BSA packer adapter
contract. Select a provider only from verified local/official interface
evidence, bind its exact executable/version/arguments and output verification,
and retain preview plus human approval before execution. If provider evidence
is unavailable, defer execution rather than inventing a command line.
