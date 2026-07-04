# Gate 267 - forge release prepare Archive Creation Skeleton

Status: Complete

## Purpose

Emit a deterministic local release archive skeleton:

```text
dist/release-prepare/archives/release.zip
```

This gate creates a ZIP archive from local release-prepare evidence while
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
- Documented: Gate 266 established local release archive planning metadata,
  checksum evidence, build-manifest digest coverage, dry-run no-write
  behavior, and `dist/` containment.
- Inferred: The next safe slice is a deterministic ZIP skeleton over local
  release-prepare evidence before FOMOD installer assembly, release
  publishing, signing, or external tool execution.

## Implemented

Gate 267 implements:

- deterministic release archive emission at
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
- ZIP entries for `release-archive-plan.json`, `release-plan.json`,
  `release-summary.json`, and `staging/release-payload.json`,
- ordinal entry ordering,
- stored ZIP entries with deterministic ZIP-compatible timestamps,
- build-manifest digest coverage for the release archive, archive plan,
  staging payload, release plan, and release summary,
- checksum coverage for the release archive, archive plan, staging payload,
  release plan, release summary, and build manifest,
- CLI JSON/text reporting for all seven written release evidence files,
- `output.releaseArchive` metadata in CLI JSON and release evidence JSON,
- `archiveCreation: true` only for normal non-dry-run execution,
- `--dry-run` preservation as planning-only/no-write behavior,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 267 does not implement:

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
| Release archive emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/archives/release.zip`. |
| Archive entries deterministic | Complete | ZIP entries are sorted, stored, and timestamped with the ZIP-compatible deterministic floor when default epoch is used. |
| Archive plan preserved | Complete | Normal execution still writes `dist/release-prepare/release-archive-plan.json`. |
| Staging payload preserved | Complete | Normal execution still writes `dist/release-prepare/staging/release-payload.json`. |
| Release evidence preserved | Complete | Normal execution still writes release plan, release summary, build manifest, and checksum sidecar. |
| Build manifest preserved | Complete | Normal execution records archive, archive plan, release plan, release summary, and staging payload digests. |
| Checksum sidecar preserved | Complete | Normal execution records archive, archive plan, release plan, release summary, staging payload, and build manifest checksums. |
| CLI output reports all writes | Complete | JSON output includes `status: prepared`, release archive output paths, and seven `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| FOMOD/publish boundary preserved | Complete | No FOMOD, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 267 requires build, targeted release golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 268 should add release archive evidence revalidation for archive digest,
entry names, and deterministic timestamp metadata. It should stop before FOMOD
installer assembly, release publishing, remote repository calls,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
