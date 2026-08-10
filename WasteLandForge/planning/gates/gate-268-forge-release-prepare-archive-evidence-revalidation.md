# Gate 268 - forge release prepare Archive Evidence Revalidation

Status: Complete

## Purpose

Emit a local release archive evidence sidecar:

```text
dist/release-prepare/release-archive-evidence.json
```

This gate revalidates the deterministic ZIP skeleton created by Gate 267 while
keeping FOMOD installer assembly, publishing, signing, external tooling, and
runtime integration out of scope.

## Research grounding

- Documented: R006 and ADR-010 define `forge release verify|prepare|publish`
  as the canonical release command surface.
- Documented: R006 says `release prepare` and `release verify` should follow
  stable package/build-manifest work, while `release publish` should wait
  until release governance is locked down.
- Documented: ADR-009 says generated and release artifacts are downstream,
  disposable, rebuildable, and not canonical source truth.
- Documented: ADR-009 says packaged archives require deterministic assembly
  with sorted entries, controlled timestamps, and final digests.
- Documented: ADR-011 requires release flows to remain deterministic,
  manifest-backed, offline-first, and AI-optional, with optional external
  provenance added later.
- Documented: Gate 267 established deterministic local release archive
  creation under `dist/release-prepare/archives/release.zip`.
- Inferred: The next safe slice is a local sidecar that reopens the created
  archive and records archive digest, entry-name, compression, and timestamp
  evidence before FOMOD installer assembly, release publishing, signing, or
  external tool execution.

## Implemented

Gate 268 implements:

- release archive evidence emission at
  `dist/release-prepare/release-archive-evidence.json`,
- unchanged deterministic release archive emission at
  `dist/release-prepare/archives/release.zip`,
- unchanged release archive planning metadata at
  `dist/release-prepare/release-archive-plan.json`,
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
- archive SHA-256 and length revalidation,
- expected and actual ZIP entry-name evidence,
- ordinal ZIP entry-order evidence,
- stored ZIP compression evidence,
- deterministic ZIP timestamp evidence,
- build-manifest digest coverage for the archive evidence sidecar,
- checksum coverage for the archive evidence sidecar,
- CLI JSON/text reporting for all eight written release evidence files,
- `output.releaseArchiveEvidence` metadata in CLI JSON and release evidence
  JSON,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 268 does not implement:

- FOMOD installer assembly,
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
| Archive evidence emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/release-archive-evidence.json`. |
| Archive digest revalidated | Complete | Evidence sidecar records the created archive SHA-256 and byte length. |
| Archive entries revalidated | Complete | Evidence sidecar records expected and actual ZIP entry names. |
| Archive ordering revalidated | Complete | Evidence sidecar records ordinal entry-order status. |
| Archive timestamp metadata revalidated | Complete | Evidence sidecar records deterministic ZIP timestamp metadata. |
| Stored compression revalidated | Complete | Evidence sidecar records stored-entry status. |
| Build manifest preserved | Complete | Normal execution records archive, archive evidence, archive plan, release plan, release summary, and staging payload digests. |
| Checksum sidecar preserved | Complete | Normal execution records archive, archive evidence, archive plan, release plan, release summary, staging payload, and build manifest checksums. |
| CLI output reports all writes | Complete | JSON output includes `status: prepared`, release archive evidence output paths, and eight `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| FOMOD/publish boundary preserved | Complete | No FOMOD, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 268 requires build, targeted release golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 269 should close out the `forge release prepare` lane and record the next
release-governance boundary before any FOMOD installer assembly, release
publishing, remote repository calls, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
