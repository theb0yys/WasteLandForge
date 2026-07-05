# Gate 300 - forge package reports Lane Closeout and Release Verify Routing

Status: Complete
Date: 2026-07-05

## Goal

Close the current `forge package --target reports` lane and route the next
implementation slice toward release dry-run evidence that can be consumed by
existing local release governance checks.

## Research grounding

- Documented: ADR-009 requires deterministic build graph outputs, disposable
  generated/dist artifacts, and local provenance.
- Documented: ADR-010/R006 keep `forge package` and `forge release verify`
  in the canonical CLI command surface without undocumented aliases.
- Documented: ADR-011 requires offline-first validation, deterministic
  fixture-backed tests, local build manifests, and AI-optional correctness.
- Documented: Gates 295 through 299 implemented the current local reports
  package lane: package planning, input discovery, staging copy,
  deterministic archive creation, and archive evidence revalidation.
- Documented: Gate 285 and the release policy consume
  `dist/release-dry-run/release-verify.json` as prior local
  `forge release verify --format json` evidence for release-publish
  preflight.
- Inferred: After reports package archive evidence exists, the next higher
  value local slice is to make `forge release verify` emit self-contained
  release-verification evidence under `dist/release-dry-run`, rather than
  adding more reports package edge cases or implementing reports
  verify-existing behavior.

## Closeout decision

The accepted current reports package evidence set is:

```text
dist/reports-package/package-plan.json
dist/reports-package/staging/package-layout.json
dist/reports-package/staging/reports/*
dist/reports-package/package.zip
dist/reports-package/package-archive-evidence.json
dist/reports-package/build-manifest.json
dist/reports-package/checksums.sha256
```

This lane now has:

- package planning and staging layout evidence,
- expected `dist/build` input discovery and missing-input classification,
- staging-copy behavior for present reports build evidence,
- deterministic local ZIP archive creation,
- archive digest, entry-name, ordering, timestamp, and stored-compression
  evidence,
- local build-manifest and checksum coverage,
- CLI, help, explain-output, unit, golden, governance, and routing coverage.

## Implemented behavior

Gate 300 is a planning/routing closeout only. It records the reports package
lane state, updates documentation and slash-command routing, and corrects the
high-level CLI summary for the already-implemented reports package behavior.

It does not change runtime CLI behavior, generated outputs, schemas,
diagnostics, package execution, release execution, tests, or external
side-effect boundaries.

## Next-value route

Gate 301 should update `forge release verify` so normal release dry-run runs
write a local release-verification self-report under:

```text
dist/release-dry-run/release-verify.json
```

The next slice should:

- reuse the existing `forge release verify --format json` shape as the local
  evidence contract,
- keep stdout behavior stable for the selected `--format`,
- include the self-report in `dist/release-dry-run/build-manifest.json` and
  `dist/release-dry-run/checksums.sha256`,
- keep output containment under project `dist/`,
- stay local-only and offline-first,
- avoid reports package verify-existing behavior,
- avoid release uploads and repository calls,
- avoid attestation/signing,
- avoid external tool execution,
- avoid plugin mutation,
- avoid MO2 or GECK automation,
- avoid runtime probes,
- avoid real third-party plugin fixtures,
- avoid AI behavior.

## Deferred reports package backlog

The following reports package work remains out of scope unless a later gate
explicitly reopens it:

- `reports` package verify-existing behavior,
- reports package semantic payload validation beyond archive evidence,
- reports package install-plan or installer metadata,
- reports package FOMOD generation,
- release-prepare ingestion of reports package artifacts,
- release-publish ingestion of reports package-specific validation evidence,
- external tool execution,
- MO2 or GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI-assisted packaging explanations.

## Not implemented

Gate 300 does not implement:

- new `forge package --target reports` behavior,
- new `forge release verify` behavior,
- package verify-existing behavior for `reports`,
- semantic archive payload validation,
- release uploads,
- remote repository calls,
- attestation or signing,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Reports package lane is closed | Complete | Gates 295 through 299 are recorded as the accepted current reports package lane. |
| Deferred reports package backlog is recorded | Complete | Verify-existing and deeper package validation are parked unless explicitly reopened. |
| Next value slice is routed | Complete | Gate 301 is routed to `forge release verify` local self-report evidence. |
| Canonical command surface is preserved | Complete | No new command or alias is introduced. |
| Runtime behavior is unchanged | Complete | No CLI code, schemas, diagnostics, package execution, or release execution are changed. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

## Validation

Gate 300 is a planning/routing closeout gate. Required validation is document
and routing consistency plus normal local build/test smoke checks:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 301 should add local `forge release verify` release-verification
self-report emission under `dist/release-dry-run/release-verify.json`, with
build-manifest and checksum coverage, while still stopping before reports
package verify-existing behavior, release uploads, attestation/signing,
external tool execution, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, or AI behavior.
