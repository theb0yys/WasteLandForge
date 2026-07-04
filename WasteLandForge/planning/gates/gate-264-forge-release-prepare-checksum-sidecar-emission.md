# Gate 264 - forge release prepare Checksum Sidecar Emission

Status: Complete

## Purpose

Emit local release-prepare checksum evidence:

```text
dist/release-prepare/checksums.sha256
```

This gate records SHA-256 checksums for the local release-prepare evidence
files while keeping staging payloads, archives, and publishing out of scope.

## Research grounding

- Documented: R006 and ADR-010 define `forge release verify|prepare|publish`
  as the canonical release command surface.
- Documented: R006 says `release prepare` and `release verify` should follow
  stable package/build-manifest work, while `release publish` should wait
  until release governance is locked down.
- Documented: ADR-009 says generated and release artifacts are downstream,
  disposable, rebuildable, and not canonical source truth.
- Documented: ADR-011 requires release flows to remain deterministic,
  manifest-backed, offline-first, and AI-optional, with optional external
  provenance added later.
- Documented: Gate 263 established local release-prepare build-manifest
  emission, `writtenOutputs`, dry-run no-write behavior, and `dist/`
  containment.
- Inferred: The next safe slice is local checksum sidecar emission for the
  release-plan, release-summary, and build-manifest evidence before staging
  payloads, archives, signing, publishing, or external tool execution.

## Implemented

Gate 264 implements:

- checksum sidecar file emission at
  `dist/release-prepare/checksums.sha256`,
- checksum entries for `release-plan.json`, `release-summary.json`, and
  `build-manifest.json`,
- unchanged release-plan file emission at
  `dist/release-prepare/release-plan.json`,
- unchanged release-summary file emission at
  `dist/release-prepare/release-summary.json`,
- unchanged build-manifest file emission at
  `dist/release-prepare/build-manifest.json`,
- CLI JSON/text reporting for all four written release evidence files,
- `output.checksums` metadata in CLI JSON, release-plan JSON,
  release-summary JSON, and build-manifest JSON,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 264 does not implement:

- staging payload writes,
- release archive creation,
- deterministic ZIP/FOMOD assembly,
- package verification reads,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- release publishing,
- remote repository calls,
- attestation or signing,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Checksum sidecar emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/checksums.sha256`. |
| Release plan preserved | Complete | Normal execution still writes `dist/release-prepare/release-plan.json`. |
| Release summary preserved | Complete | Normal execution still writes `dist/release-prepare/release-summary.json`. |
| Build manifest preserved | Complete | Normal execution still writes `dist/release-prepare/build-manifest.json`. |
| CLI output reports all writes | Complete | JSON output includes `status: prepared`, `output.releasePlan`, `output.releaseSummary`, `output.buildManifest`, `output.checksums`, and four `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| Archive/publish boundary preserved | Complete | No staging, archive, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 264 requires build, targeted release golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 265 should add a local staging payload skeleton for
`forge release prepare` under `dist/release-prepare/staging/`, then update the
manifest/checksum evidence accordingly. It should stop before release archive
creation, deterministic ZIP/FOMOD assembly, release publishing, remote
repository calls, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
