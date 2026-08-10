# Gate 266 - forge release prepare Archive Plan Skeleton

Status: Complete

## Purpose

Emit local release-prepare archive planning metadata:

```text
dist/release-prepare/release-archive-plan.json
```

This gate records the future release archive target while keeping archive
creation, deterministic ZIP/FOMOD assembly, publishing, and external tooling
out of scope.

## Research grounding

- Documented: R006 and ADR-010 define `forge release verify|prepare|publish`
  as the canonical release command surface.
- Documented: R006 says `release prepare` and `release verify` should follow
  stable package/build-manifest work, while `release publish` should wait
  until release governance is locked down.
- Documented: ADR-009 says generated and release artifacts are downstream,
  disposable, rebuildable, and not canonical source truth.
- Documented: ADR-009 says packaged archives require deterministic assembly
  with sorted entries, controlled timestamps, and final digests once archive
  creation exists.
- Documented: ADR-011 requires release flows to remain deterministic,
  manifest-backed, offline-first, and AI-optional, with optional external
  provenance added later.
- Documented: Gate 265 established local staging payload skeleton emission,
  `writtenOutputs`, dry-run no-write behavior, checksum evidence, and `dist/`
  containment.
- Inferred: The next safe slice is local archive planning metadata before
  archive creation, deterministic ZIP/FOMOD assembly, signing, publishing, or
  external tool execution.

## Implemented

Gate 266 implements:

- release archive planning metadata emission at
  `dist/release-prepare/release-archive-plan.json`,
- planned future archive target metadata at
  `dist/release-prepare/archives/release.zip`,
- unchanged staging payload skeleton emission at
  `dist/release-prepare/staging/release-payload.json`,
- unchanged release-plan file emission at
  `dist/release-prepare/release-plan.json`,
- unchanged release-summary file emission at
  `dist/release-prepare/release-summary.json`,
- unchanged build-manifest file emission at
  `dist/release-prepare/build-manifest.json`,
- unchanged checksum sidecar file emission at
  `dist/release-prepare/checksums.sha256`,
- build-manifest digest coverage for the archive plan, staging payload,
  release plan, and release summary,
- checksum coverage for the archive plan, staging payload, release plan,
  release summary, and build manifest,
- CLI JSON/text reporting for all six written release evidence files,
- `output.releaseArchivePlan` and `output.plannedArchive` metadata in CLI JSON
  and release evidence JSON,
- a planned `release-archive` output with `wouldWriteInCurrentGate: false`,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 266 does not implement:

- release archive creation,
- deterministic ZIP/FOMOD assembly,
- archive directory creation,
- installer metadata generation,
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
| Archive plan emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/release-archive-plan.json`. |
| Planned archive target recorded | Complete | CLI JSON and archive-plan JSON report `dist/release-prepare/archives/release.zip`. |
| Archive not created | Complete | The planned archive path and `archives/` directory are not created. |
| Staging payload preserved | Complete | Normal execution still writes `dist/release-prepare/staging/release-payload.json`. |
| Release evidence preserved | Complete | Normal execution still writes release plan, release summary, build manifest, and checksum sidecar. |
| Build manifest preserved | Complete | Normal execution records archive plan, release plan, release summary, and staging payload digests. |
| Checksum sidecar preserved | Complete | Normal execution records archive plan, release plan, release summary, staging payload, and build manifest checksums. |
| CLI output reports all writes | Complete | JSON output includes `status: prepared`, archive planning output paths, and six `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| Archive/publish boundary preserved | Complete | No archive creation, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 266 requires build, targeted release golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 267 should add deterministic local release archive creation skeleton
derived from `release-archive-plan.json` and local release evidence. It should
stop before FOMOD installer assembly, release publishing, remote repository
calls, attestation/signing, external tool execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
or AI behavior.
