# Gate 262 - forge release prepare Release Summary Emission

Status: Complete

## Purpose

Emit the second local release-prepare evidence file:

```text
dist/release-prepare/release-summary.json
```

This gate keeps `forge release prepare` local and deterministic while adding a
small machine-readable summary beside the Gate 261 release plan.

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
- Documented: Gate 261 established local release-plan emission,
  `writtenOutputs`, dry-run no-write behavior, and `dist/` containment.
- Inferred: The next safe slice is local release-summary emission derived
  from release-prepare plan metadata, before build manifests, checksums,
  staging payloads, archives, signing, publishing, or external tool execution.

## Implemented

Gate 262 implements:

- release-summary file emission at
  `dist/release-prepare/release-summary.json`,
- unchanged release-plan file emission at
  `dist/release-prepare/release-plan.json`,
- CLI JSON/text reporting for both written release evidence files,
- `output.releaseSummary` metadata in CLI JSON and release-plan JSON,
- release-summary JSON with local output, summary, execution, and boundary
  metadata,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 262 does not implement:

- build manifest emission for release prepare,
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
| Release summary emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/release-summary.json`. |
| Release plan preserved | Complete | Normal execution still writes `dist/release-prepare/release-plan.json`. |
| CLI output reports both writes | Complete | JSON output includes `status: prepared`, `output.releasePlan`, `output.releaseSummary`, and two `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| Archive/publish boundary preserved | Complete | No archive, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 262 requires build, targeted release golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 263 should add local build-manifest file emission for
`forge release prepare` under `dist/release-prepare/build-manifest.json`,
covering the current release-plan and release-summary evidence. It should stop
before checksum sidecar emission, staging payload writes, release archive
creation, deterministic ZIP/FOMOD assembly, release publishing, remote
repository calls, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
