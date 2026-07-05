# Gate 291 - forge doctor export Release Readiness Lane Closeout

Status: Complete
Date: 2026-07-05

## Goal

Close the local `forge doctor export` release-readiness handoff lane and route
the next implementation slice toward broader deterministic build value.

## Research grounding

- Documented: ADR-009 requires generated outputs to come from a deterministic,
  capability-aware build graph and to carry provenance through local build
  manifests.
- Documented: ADR-010/R006 keep `forge build` and `forge doctor export` in
  the canonical command surface without undocumented aliases.
- Documented: ADR-011 requires offline-first validation, deterministic
  fixture-backed testing, local build manifests, and governance that does not
  require AI.
- Documented: The generator/build research says v0.1 should prioritize
  deterministic text and metadata artifacts, including capability reports,
  dependency reports, build manifests, provenance, package metadata, JSON, and
  Markdown reports before high-risk plugin generation or patching.
- Inferred: After Gates 289 and 290 exposed release-readiness evidence through
  Doctor export and triage/worklists, further release-readiness Doctor
  micro-gates have lower value than returning to the build graph and report
  index layer.

## Implemented

Gate 291 records that the current local Doctor export release-readiness lane
now includes:

- top-level `releaseReadiness` JSON projection,
- plain and Markdown release-readiness sections,
- bundle `release-readiness/index.json` and `release-readiness/index.md`,
- bundle README, index, summary, manifest, and checksum references,
- Doctor triage blocking item projection,
- release-readiness blocker worklist entries,
- dry-run command hints back to `forge release publish`.

It also updates planning and routing documents so the next implementation
slice starts with local `forge build --target reports` build-plan/report-index
evidence instead of continuing Doctor export release-readiness edge cases.

## Deferred Doctor release-readiness backlog

The following Doctor release-readiness work remains out of scope unless a
later gate explicitly reopens it:

- more release-readiness triage grouping variants,
- release-readiness severity customization,
- release-readiness bundle schema publication,
- publish approval UX beyond the existing dry-run command hint,
- release notes or changelog drafting,
- remote repository integration,
- release uploads,
- signing or attestation,
- external tool execution,
- runtime probes,
- MO2 or GECK automation,
- real third-party plugin fixtures,
- AI-assisted release explanation.

## Not implemented

Gate 291 does not implement:

- new `forge doctor export` behavior,
- new `forge release publish` behavior,
- new `forge build` behavior,
- new command aliases,
- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- archive payload content validation,
- FOMOD installer assembly,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Doctor release-readiness lane closeout recorded | Complete | Gates 289 and 290 are treated as the complete current Doctor release-readiness lane. |
| Deferred Doctor release-readiness backlog recorded | Complete | Further release-readiness Doctor edge cases are parked unless explicitly reopened. |
| Next development direction changed | Complete | Next gate moves to local build-plan/report-index evidence for `forge build --target reports`. |
| Command surface preserved | Complete | No new command or alias is introduced. |
| Runtime mutation avoided | Complete | No release publish, remote repository call, external tool, MO2/GECK automation, runtime probe, plugin mutation, or AI call is added. |

## Validation

Gate 291 is a planning/routing closeout gate. Required validation is document
and routing consistency plus normal local build/test smoke checks.

## Next gate

Gate 292 should start a `forge build --target reports`
build-plan/report-index skeleton under local build evidence. It should
summarize validation, capability, generator-target, planned-output,
build-manifest, and checksum roles while stopping before remote repository
calls, release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
