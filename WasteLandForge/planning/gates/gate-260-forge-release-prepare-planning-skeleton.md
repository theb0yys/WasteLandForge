# Gate 260 - forge release prepare Planning Skeleton

Status: Complete

## Purpose

Start the `forge release prepare` lane with a deterministic, usage-safe
planning skeleton.

This gate defines the command contract, local output boundary, planned report
shape, and explicit non-execution flags for release preparation. It does not
create release archives or write release-preparation evidence.

## Research grounding

- Documented: R006 and ADR-010 define `forge release verify|prepare|publish`
  as the canonical release command surface.
- Documented: R006 says `release prepare` and `release verify` should follow
  stable package/build-manifest work, while `release publish` should wait
  until release governance is locked down.
- Documented: ADR-009 says release assets must remain downstream generated
  artifacts and must not become canonical source truth.
- Documented: ADR-011 requires release flows to remain deterministic,
  manifest-backed, offline-first, and AI-optional, with optional external
  provenance added later.
- Inferred: The first safe release-prepare slice should expose planning
  metadata and output containment before any archive creation, signing,
  attestation, publishing, or external tool execution.

## Implemented

Gate 260 implements:

- `forge help release prepare`,
- `forge release prepare [project-root] --format human|plain|json`,
- `forge release prepare --project <path>`,
- `forge release prepare --output dist/<name>`,
- `--dry-run` and `--no-input` acceptance,
- default planned output root `dist/release-prepare`,
- unsafe-operation refusal when the planned output root escapes project
  `dist/`,
- machine-readable `plannedOutputs`, `reportContract`, `outputSafety`, and
  false `execution` flags,
- human/plain planning output,
- unchanged `forge release publish` reserved behavior.

The planned future outputs are:

```text
dist/release-prepare/staging/
dist/release-prepare/release-plan.json
dist/release-prepare/release-summary.json
dist/release-prepare/build-manifest.json
dist/release-prepare/checksums.sha256
```

Gate 260 writes none of those files.

## Not implemented

Gate 260 does not implement:

- release-preparation evidence writes,
- release archive creation,
- deterministic ZIP/FOMOD assembly,
- release candidate staging,
- release summary file emission,
- build manifest emission for release prepare,
- checksum sidecar emission for release prepare,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- package verification reads,
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
| Release prepare help added | Complete | `forge help release prepare` describes the Gate 260 planning boundary and planned future outputs. |
| JSON planning skeleton added | Complete | `forge release prepare --format json` returns `status: planned`, `plannedOutputs`, `reportContract`, `outputSafety`, and false execution flags. |
| Dist containment enforced | Complete | `--output ../outside` returns exit code 6 with `refused-output-outside-dist` and no filesystem writes. |
| Runtime mutation avoided | Complete | Tests verify no `dist/` directory is created by the planning skeleton. |
| Publish remains reserved | Complete | Gate 260 does not alter `forge release publish`. |

## Validation

Gate 260 requires build, targeted release golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 261 should implement local release-plan file emission for
`forge release prepare` under `dist/release-prepare/release-plan.json`, plus
the minimal JSON/text CLI reporting needed to describe that written plan. It
should stop before release archive creation, deterministic ZIP/FOMOD assembly,
release publishing, remote repository calls, attestation/signing, external
tool execution, plugin mutation, MO2 automation, GECK automation, runtime
probes, real third-party plugin fixtures, or AI behavior.
