# Gate 306 - forge release verify Evidence Collection Plan

Status: Complete
Date: 2026-07-05

## Goal

Add a local release dry-run evidence collection plan that orders the
release-publish preflight evidence collection steps without executing any
commands.

Default path:

```text
dist/release-dry-run/release-evidence-collection-plan.json
```

## Research grounding

- Documented: ADR-010/R006 keep `forge release verify`,
  `forge capabilities scan`, `forge package`, and `forge release publish` in
  the canonical command surface.
- Documented: ADR-011 requires deterministic local release evidence, local
  build manifests, checksums, and AI-optional release correctness.
- Documented: Gates 282 through 285 make `forge release publish` consume
  local schema-validation, capability/environment, package-validation, and
  release-verification evidence files under `dist/release-dry-run`.
- Documented: Gate 302 made `forge release verify` emit an evidence index for
  those release-publish preflight files.
- Documented: Gate 304 made `forge release verify` emit a local file-presence
  status projection for the indexed evidence.
- Documented: Gate 305 made `forge release verify` emit manual action hints
  for missing indexed evidence.
- Inferred: The next safe release slice is a local ordered collection plan
  that ties index, status, and actions together before any command fan-out or
  external execution.

## Implemented behavior

`forge release verify` now writes:

```text
dist/release-dry-run/staging/
dist/release-dry-run/validation.json
dist/release-dry-run/release-summary.json
dist/release-dry-run/release-verify.json
dist/release-dry-run/release-evidence-index.json
dist/release-dry-run/release-evidence-status.json
dist/release-dry-run/release-evidence-actions.json
dist/release-dry-run/release-evidence-collection-plan.json
dist/release-dry-run/release-evidence-handoff.md
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

The collection plan records:

- `formatVersion: 0.1`,
- `kind: wastelandforge.release-dry-run-evidence-collection-plan`,
- links to the evidence index, evidence status projection, action checklist,
  and handoff summary,
- total, present, missing, action, step, manual-step, and available-step
  counts,
- ordered collection steps for schema validation, capability/environment
  validation, package validation, and release verification,
- collection mode for each step,
- action IDs for missing manual steps,
- explicit disabled execution boundaries.

In the normal synthetic fixture run, the plan contains four steps. Schema
validation and release verification are `available` current-command outputs.
Capability/environment and package validation are `manual-required` steps
linked to the Gate 305 action checklist.

`release-evidence-collection-plan.json` is included in `release-verify.json`
output maps, CLI JSON output, human/plain output, help output,
explain-output metadata, `release-evidence-index.json`,
`release-evidence-status.json`, `release-evidence-actions.json`,
`release-evidence-handoff.md`, `build-manifest.json`, and `checksums.sha256`.

## Explicit non-goals

Gate 306 does not implement:

- command fan-out,
- capability scan execution,
- package verification execution,
- evidence content validation,
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
| Collection plan emitted | Complete | `forge release verify` writes `dist/release-dry-run/release-evidence-collection-plan.json`. |
| Steps ordered | Complete | The plan orders schema validation, capability/environment validation, package validation, and release verification steps. |
| Missing steps linked to actions | Complete | Missing capability/environment and package-validation steps link to Gate 305 action IDs. |
| Execution boundaries explicit | Complete | The collection plan records command fan-out, capability scan execution, package verify execution, publish, runtime probe, and AI flags as false. |
| Index/status/actions link plan | Complete | `release-evidence-index.json`, `release-evidence-status.json`, and `release-evidence-actions.json` include the collection plan path. |
| Handoff summary includes plan | Complete | The Markdown handoff includes the collection plan path, step count, and plan rows. |
| Manifest coverage added | Complete | `build-manifest.json` includes `dist/release-dry-run/release-evidence-collection-plan.json`. |
| Checksum coverage added | Complete | `checksums.sha256` includes `release-evidence-collection-plan.json`. |
| Help/explain metadata updated | Complete | Help and explain-output metadata include the collection plan path. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-restore --filter FullyQualifiedName~ReleaseDryRunVerifierTests
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter ReleaseVerifyJsonWritesDryRunEvidence
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 307 should add `forge release publish` local collection-plan evidence
evaluation from `dist/release-dry-run/release-evidence-collection-plan.json`,
while still stopping before command fan-out, capability scan execution,
package verify execution, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
