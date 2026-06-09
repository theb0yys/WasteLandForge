# Security Policy

Status: Gate 8 baseline
Research classification: Documented
Source: R008 / ADR-011

## Supported Versions

WastelandForge has no public release stream yet.

Until the first release exists, security fixes target:

- `main`,
- active `release/*` branches once created.

## Reporting

Use GitHub private vulnerability reporting when it is enabled for the repository.

Do not publish exploit details, private install paths, secrets, Bethesda assets,
third-party mod files, or unreleased vulnerability details in public issues.

## Project Security Rules

- The correctness path must remain local, deterministic, offline-first, and AI-optional.
- Public fixtures must be synthetic and redistributable.
- CI workflows use least-privilege permissions.
- Protected workflows should use full commit SHAs for action references.
- Secrets must not be required for build, validation, tests, or release verification.
- Any future AI feature must be outside the mandatory correctness path.
