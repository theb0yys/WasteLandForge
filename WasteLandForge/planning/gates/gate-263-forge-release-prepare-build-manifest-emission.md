# Gate 263 - forge release prepare Build Manifest Emission

Status: Complete

## Purpose

Emit local release-prepare build-manifest evidence:

```text
dist/release-prepare/build-manifest.json
```

This gate records the release-plan and release-summary evidence in a local
manifest while keeping checksum sidecars, staging payloads, archives, and
publishing out of scope.

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
- Documented: Gate 262 established local release-summary emission beside
  release-plan emission, `writtenOutputs`, dry-run no-write behavior, and
  `dist/` containment.
- Inferred: The next safe slice is local build-manifest emission for the
  existing release-plan and release-summary evidence before checksum sidecars,
  staging payloads, archives, signing, publishing, or external tool execution.

## Implemented

Gate 263 implements:

- build-manifest file emission at
  `dist/release-prepare/build-manifest.json`,
- unchanged release-plan file emission at
  `dist/release-prepare/release-plan.json`,
- unchanged release-summary file emission at
  `dist/release-prepare/release-summary.json`,
- CLI JSON/text reporting for all three written release evidence files,
- `output.buildManifest` metadata in CLI JSON, release-plan JSON, and
  release-summary JSON,
- build-manifest JSON with reproducible timestamp metadata, output digests
  for the release plan and release summary, execution flags, and boundaries,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 263 does not implement:

- checksum sidecar emission for release prepare,
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
| Build manifest emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/build-manifest.json`. |
| Release plan preserved | Complete | Normal execution still writes `dist/release-prepare/release-plan.json`. |
| Release summary preserved | Complete | Normal execution still writes `dist/release-prepare/release-summary.json`. |
| CLI output reports all writes | Complete | JSON output includes `status: prepared`, `output.releasePlan`, `output.releaseSummary`, `output.buildManifest`, and three `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| Archive/publish boundary preserved | Complete | No checksum, staging, archive, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 263 requires build, targeted release golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 264 should add local checksum sidecar emission for
`forge release prepare` under `dist/release-prepare/checksums.sha256`,
covering the current release-plan, release-summary, and build-manifest
evidence. It should stop before staging payload writes, release archive
creation, deterministic ZIP/FOMOD assembly, release publishing, remote
repository calls, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
