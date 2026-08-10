# Gate 261 - forge release prepare Release Plan Emission

Status: Complete

## Purpose

Emit the first local release-prepare evidence file:

```text
dist/release-prepare/release-plan.json
```

This gate moves `forge release prepare` beyond stdout-only planning while
keeping the release lane tightly bounded. The command writes only the local
release plan file and reports that write in CLI output.

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
- Documented: Gate 260 established the release-prepare planning contract,
  planned output root, planned future outputs, and `dist/` containment.
- Inferred: The next safe slice is a single local release-plan file before
  release summaries, build manifests, checksums, archives, signing,
  publishing, or external tool execution.

## Implemented

Gate 261 implements:

- release-plan file emission at `dist/release-prepare/release-plan.json`,
- `--dry-run` preservation as planning-only/no-write behavior,
- machine-readable `writtenOutputs` metadata,
- CLI JSON/text reporting for the written release plan,
- release-plan JSON with planned outputs, execution flags, and boundaries,
- unchanged refusal for output roots outside project `dist/`,
- unchanged `forge release publish` reserved behavior.

## Not implemented

Gate 261 does not implement:

- release summary file emission,
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
| Release plan emitted | Complete | `forge release prepare <project> --format json` writes `dist/release-prepare/release-plan.json`. |
| CLI output reports write | Complete | JSON output includes `status: prepared`, `output.releasePlan`, and `writtenOutputs`. |
| Dry-run remains no-write | Complete | `--dry-run` reports `status: planned` and does not create `dist/`. |
| Dist containment preserved | Complete | `--output ../outside` returns exit code 6 with no filesystem writes. |
| Archive/publish boundary preserved | Complete | No archive, publish, remote, signing, external tool, runtime probe, or AI behavior is added. |

## Validation

Gate 261 requires build, targeted release golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 262 should add local release-summary file emission for
`forge release prepare` under `dist/release-prepare/release-summary.json`,
derived from the existing release-plan metadata. It should stop before build
manifest emission, checksum sidecar emission, staging payload writes, release
archive creation, deterministic ZIP/FOMOD assembly, release publishing, remote
repository calls, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
