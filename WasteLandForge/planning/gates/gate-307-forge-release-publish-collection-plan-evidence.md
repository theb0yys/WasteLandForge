# Gate 307 - forge release publish Collection-Plan Evidence

Status: Complete
Date: 2026-07-05

## Goal

Add local `forge release publish` evidence evaluation for the release dry-run
collection plan written by Gate 306.

Default path:

```text
dist/release-dry-run/release-evidence-collection-plan.json
```

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  offline-first CLI surface.
- Documented: ADR-011 requires layered validation, deterministic local
  evidence, release validation, build manifests, checksums, and governance
  paths that do not require AI.
- Documented: Gates 282 through 285 made `forge release publish` evaluate
  local schema-validation, capability/environment, package-validation, and
  release-verification evidence under `dist/release-dry-run`.
- Documented: Gate 306 made `forge release verify` emit
  `release-evidence-collection-plan.json` with ordered release-publish
  preflight evidence steps and disabled execution boundaries.
- Inferred: The next safe publish slice is to consume that collection plan as
  another local evidence precondition before any command fan-out or publish
  behavior.

## Implemented behavior

`forge release publish` now reads:

```text
dist/release-dry-run/release-evidence-collection-plan.json
```

The preflight validates:

- `formatVersion`, `kind`, `command`, `dryRun`, and `status` identity fields,
- `outputRoot: dist/release-dry-run`,
- links to `release-evidence-index.json`,
  `release-evidence-status.json`, `release-evidence-actions.json`, and
  `release-evidence-handoff.md`,
- summary counters for total evidence, present evidence, missing evidence,
  actions, steps, manual steps, and available steps,
- the four ordered collection steps for schema validation,
  capability/environment validation, package validation, and release
  verification,
- manual step/action shape,
- disabled command fan-out, capability scan execution, package verify
  execution, release publish execution, uploads, signing, external tools,
  plugin mutation, MO2/GECK automation, runtime probes, real third-party
  plugin fixtures, and AI.

`release publish` reports the result as `collectionPlanEvidence` in JSON,
adds `collectionPlanEvidenceStatus` and step counters to the
`releasePrepareEvidence` summary, adds
`release-dry-run-collection-plan` to required evidence/readiness checks, and
prints the evidence in plain output.

## Explicit non-goals

Gate 307 does not implement:

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
| Collection plan is evaluated | Complete | `release publish` reads `dist/release-dry-run/release-evidence-collection-plan.json`. |
| Contract identity is checked | Complete | The reader validates kind, command, dry-run, status, and output root. |
| Ordered steps are checked | Complete | The reader validates the four release-publish preflight evidence steps. |
| Links are checked | Complete | The reader checks index, status, action checklist, and handoff links. |
| No-execution boundary is checked | Complete | The reader requires all collection-plan execution flags to remain false. |
| Readiness includes the plan | Complete | Required evidence includes `release-dry-run-collection-plan`. |
| CLI output exposes the result | Complete | JSON and plain output include collection-plan evidence status and detail. |
| Tests cover the slice | Complete | Golden tests assert clean collection-plan evidence without publishing. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter ReleasePublishDryRunEvaluatesCollectionPlanEvidenceWithoutPublishing
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter ReleasePublish
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 308 should add `forge release publish` local release dry-run evidence
cross-link consistency evaluation across `release-evidence-index.json`,
`release-evidence-status.json`, `release-evidence-actions.json`,
`release-evidence-collection-plan.json`, and `release-evidence-handoff.md`,
while still stopping before command fan-out, capability scan execution,
package verify execution, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
