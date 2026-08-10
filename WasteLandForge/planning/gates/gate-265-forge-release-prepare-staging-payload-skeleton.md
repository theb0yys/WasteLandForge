# Gate 265 - forge release prepare Staging Payload Skeleton

Status: Complete

## Purpose

Emit a local release-prepare staging payload skeleton:

```text
dist/release-prepare/staging/release-payload.json
```

This gate gives the release-prepare staging root a deterministic local payload
record while keeping archives, installers, publishing, and external tooling
out of scope.

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
- Documented: Gate 264 established local checksum sidecar emission,
  `writtenOutputs`, dry-run no-write behavior, and `dist/` containment.
- Inferred: The next safe slice is a local staging payload skeleton before
  archive planning, release archive creation, signing, publishing, or external
  tool execution.

## Implemented

Gate 265 implements:

- staging payload skeleton emission at
  `dist/release-prepare/staging/release-payload.json`,
- unchanged release-plan file emission at
  `dist/release-prepare/release-plan.json`,
- unchanged release-summary file emission at
  `dist/release-prepare/release-summary.json`,
- unchanged build-manifest file emission at
  `dist/release-prepare/build-manifest.json`,
- unchanged checksum sidecar file emission at
  `dist/release-prepare/checksums.sha256`,
- build-manifest digest coverage for the staging payload, release plan, and
  release summary,
- checksum coverage for the staging payload, release plan, release summary,
  and build manifest,
- CLI JSON/text reporting for all five written release evidence files,
- `output.stagingRoot` and `output.stagingPayload` metadata in CLI JSON and
  release evidence JSON,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 265 does not implement:

- release archive planning,
- release archive creation,
- deterministic ZIP/FOMOD assembly,
- installer metadata,
- real mod payload staging,
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
| Staging payload skeleton emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/staging/release-payload.json`. |
| Release plan preserved | Complete | Normal execution still writes `dist/release-prepare/release-plan.json`. |
| Release summary preserved | Complete | Normal execution still writes `dist/release-prepare/release-summary.json`. |
| Build manifest preserved | Complete | Normal execution still writes `dist/release-prepare/build-manifest.json` and records the staging payload digest. |
| Checksum sidecar preserved | Complete | Normal execution still writes `dist/release-prepare/checksums.sha256` and records the staging payload checksum. |
| CLI output reports all writes | Complete | JSON output includes `status: prepared`, staging output paths, release evidence paths, and five `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| Archive/publish boundary preserved | Complete | No archive, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 265 requires build, targeted release golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 266 should add local release archive planning metadata for
`forge release prepare`, derived from the staged payload and release evidence.
It should stop before release archive creation, deterministic ZIP/FOMOD
assembly, release publishing, remote repository calls, attestation/signing,
external tool execution, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, or AI behavior.
