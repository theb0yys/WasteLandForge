# Gate 310 - forge doctor export Dry-Run Evidence Remediation Handoff

Status: Complete
Date: 2026-07-05

## Goal

Add `forge doctor export` release dry-run evidence remediation handoff
projection from the local `forge release publish --dry-run` preflight.

## Research grounding

- Documented: ADR-010/R006 keep `forge doctor export`, `forge release
  verify`, and `forge release publish` in the canonical offline-first CLI
  surface.
- Documented: ADR-011 requires layered validation, deterministic local
  release evidence, build manifests, checksums, and governance paths that do
  not require AI.
- Documented: Gate 289 projected local release-publish readiness into Doctor
  export artifacts.
- Documented: Gate 290 turned release-readiness blockers into Doctor triage
  and operator worklist items.
- Documented: Gate 309 added manual release dry-run evidence remediation
  summaries to `forge release publish`.
- Inferred: The next safe Doctor slice is to expose that existing remediation
  summary in Doctor export release-readiness and triage artifacts before any
  command fan-out or automatic remediation.

## Implemented behavior

`forge doctor export` now includes
`releaseReadiness.dryRunEvidenceRemediation` in primary JSON and
`release-readiness/index.json` bundle output.

Plain and Markdown Doctor export output now render a release dry-run evidence
remediation section with:

- status and source status,
- checked/current-gate status,
- operator-action requirement,
- action, command-hint, affected-path, and blocking-issue counts,
- recommended local `forge release verify` command,
- manual remediation item rows.

Doctor triage now projects remediation-required dry-run evidence as a blocking
handoff item:

```text
restore-release-dry-run-evidence-files
```

The work item points to the existing local command hint:

```text
forge release verify <project-root> --format json --no-input
```

Bundle handoff artifacts also include the projection through
`handoff-summary.md`, `triage/index.json`, and `triage/index.md`.

## Explicit non-goals

Gate 310 does not implement:

- command fan-out,
- automatic remediation,
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
| Doctor JSON exposes remediation | Complete | `releaseReadiness.dryRunEvidenceRemediation` is present in primary JSON. |
| Release-readiness bundle exposes remediation | Complete | `release-readiness/index.json` and `.md` include dry-run evidence remediation. |
| Plain and Markdown output expose remediation | Complete | Main Doctor text and summary Markdown render the remediation section. |
| Triage creates a manual work item | Complete | Remediation-required evidence creates `restore-release-dry-run-evidence-files`. |
| Command hint remains local and manual | Complete | The hint is `forge release verify <project-root> --format json --no-input`. |
| Bundle handoff exposes the work | Complete | `handoff-summary.md` and `triage/index.*` include the remediation work item. |
| No execution boundary is preserved | Complete | The projection does not run remediation or publish behavior. |
| Tests cover the slice | Complete | Doctor golden tests assert JSON, plain, Markdown, and bundle projections. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Doctor
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 311 should close the local release dry-run remediation handoff lane and
route the next value slice, while still stopping before command fan-out,
automatic remediation, capability scan execution, package verify execution,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior unless a later gate explicitly opens one of
those scopes.
