# Gate 308 - forge release publish Dry-Run Evidence Cross-Links

Status: Complete
Date: 2026-07-05

## Goal

Add local `forge release publish` cross-link consistency evaluation for the
release dry-run evidence set written by Gates 302 through 306.

Default paths:

```text
dist/release-dry-run/release-evidence-index.json
dist/release-dry-run/release-evidence-status.json
dist/release-dry-run/release-evidence-actions.json
dist/release-dry-run/release-evidence-collection-plan.json
dist/release-dry-run/release-evidence-handoff.md
```

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  offline-first CLI surface.
- Documented: ADR-011 requires layered validation, deterministic local
  evidence, release validation, build manifests, checksums, and governance
  paths that do not require AI.
- Documented: Gates 302 through 306 made `forge release verify` emit the local
  dry-run evidence index, handoff, status projection, action checklist, and
  collection plan.
- Documented: Gate 307 made `forge release publish` consume
  `release-evidence-collection-plan.json` as local publish-readiness evidence.
- Inferred: The next safe publish slice is to verify those five dry-run
  evidence files agree with each other before any command fan-out or publish
  behavior.

## Implemented behavior

`forge release publish` now reads the local dry-run evidence set:

```text
dist/release-dry-run/release-evidence-index.json
dist/release-dry-run/release-evidence-status.json
dist/release-dry-run/release-evidence-actions.json
dist/release-dry-run/release-evidence-collection-plan.json
dist/release-dry-run/release-evidence-handoff.md
```

The preflight validates:

- JSON identity fields for `formatVersion`, `kind`, `command`, `dryRun`,
  `status`, and `outputRoot`,
- required link fields across index, status, action checklist, and collection
  plan JSON,
- handoff references to the index, status, action checklist, and collection
  plan files,
- required evidence entries across index/status and collection-plan
  `releasePublishPreflight` consumers,
- missing-evidence actions against missing required evidence entries,
- collection steps against required evidence and action references,
- summary counters across index, status, actions, and collection plan,
- disabled execution boundaries across all JSON files,
- handoff Markdown rows for required evidence, actions, and collection steps.

`release publish` reports the result as `dryRunCrossLinkEvidence` in JSON,
adds `dryRunCrossLinkEvidenceStatus`, file counters, and mismatched-link
counters to `releasePrepareEvidence`, adds `release-dry-run-cross-links` to
required evidence/readiness checks, and prints the evidence in plain output.

## Explicit non-goals

Gate 308 does not implement:

- command fan-out,
- capability scan execution,
- package verification execution,
- release-publish execution,
- release uploads,
- remote repository calls,
- attestation or signing,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Cross-link evidence is evaluated | Complete | `release publish` reads the five local release dry-run evidence files. |
| Identity is checked | Complete | The reader validates kind, command, dry-run, status, and output root. |
| JSON links are checked | Complete | The reader checks index/status/actions/collection-plan references. |
| Handoff references are checked | Complete | The reader checks Markdown references to the JSON evidence files. |
| Required evidence is cross-checked | Complete | Index/status evidence rows are compared with collection-plan consumers. |
| Actions and steps are cross-checked | Complete | Missing-evidence actions and collection steps are compared with required evidence. |
| No-execution boundary is checked | Complete | The reader requires release dry-run execution flags to remain false. |
| Readiness includes the cross-links | Complete | Required evidence includes `release-dry-run-cross-links`. |
| CLI output exposes the result | Complete | JSON and plain output include dry-run cross-link status and detail. |
| Tests cover the slice | Complete | Golden tests assert clean cross-link evidence without publishing. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter ReleasePublishDryRunEvaluatesDryRunEvidenceCrossLinksWithoutPublishing
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter ReleasePublish
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Doctor
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 309 should add `forge release publish` local evidence remediation
summaries for missing, malformed, or cross-link-mismatched dry-run evidence,
while still stopping before command fan-out, capability scan execution,
package verify execution, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
