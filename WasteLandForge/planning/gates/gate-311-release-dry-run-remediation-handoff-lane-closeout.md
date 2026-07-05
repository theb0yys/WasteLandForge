# Gate 311 - Release Dry-Run Remediation Handoff Lane Closeout

Status: Complete
Date: 2026-07-05

## Goal

Close the local release dry-run remediation handoff lane and route the next
implementation slice to broader Forge value.

## Research grounding

- Documented: ADR-010/R006 keep `forge init`, `forge release verify`,
  `forge release publish`, and `forge doctor export` in the canonical
  offline-first CLI surface without undocumented aliases.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, release governance, and
  contribution rules that do not require AI.
- Documented: ADR-009/R006 say the v0.1 build path should prefer
  deterministic text and metadata outputs, local provenance, and explicit
  capability checks before external tool or game-facing mutation.
- Documented: Gate 309 added local manual remediation summaries to
  `forge release publish --dry-run` for missing, malformed, or
  cross-link-mismatched release dry-run evidence.
- Documented: Gate 310 projected that remediation evidence into
  `forge doctor export` release-readiness, triage, Markdown, and bundle
  handoff artifacts.
- Inferred: More release dry-run remediation micro-gates now have lower value
  than making the standalone CLI able to create a valid project scaffold
  through the already-canonical `forge init` command.

## Closeout decision

The accepted current release dry-run remediation handoff lane includes:

- `forge release verify` local release dry-run evidence files under
  `dist/release-dry-run`,
- `forge release publish --dry-run` cross-link and manual remediation
  summaries,
- `forge doctor export` release-readiness projection of that remediation
  state,
- Doctor triage and handoff worklist coverage through the manual
  `restore-release-dry-run-evidence-files` blocker,
- local command hints back to
  `forge release verify <project-root> --format json --no-input`,
- no command fan-out, automatic remediation, publish execution, remote calls,
  external tool execution, runtime probes, MO2/GECK automation, or AI
  behavior.

Gate 311 is a planning/routing closeout only. It records the lane state and
updates docs, prompt routing, and governance ledgers so the next slice moves
to `forge init` project scaffold planning instead of more release dry-run
remediation edge cases.

## Deferred release dry-run remediation backlog

The following remediation work remains out of scope unless a later gate
explicitly reopens it:

- automatic evidence regeneration,
- command fan-out from Doctor export,
- capability scan execution from release publish or Doctor export,
- package verification execution from release publish or Doctor export,
- evidence content repair,
- release dry-run evidence schema publication,
- publish approval UX beyond existing explicit approval checks,
- release uploads,
- remote repository calls,
- signing or attestation,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI-assisted release explanation.

## Not implemented

Gate 311 does not implement:

- new `forge release verify` behavior,
- new `forge release publish` behavior,
- new `forge doctor export` behavior,
- `forge init` behavior,
- command aliases,
- command fan-out,
- automatic remediation,
- capability scan execution,
- package verification execution,
- release publication,
- remote repository calls,
- release uploads,
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
| Release dry-run remediation handoff lane is closed | Complete | Gates 309 and 310 are recorded as the accepted current remediation handoff lane. |
| Deferred remediation backlog is recorded | Complete | Automatic remediation, command fan-out, package/capability execution, publication, external tools, runtime probes, and AI are parked. |
| Next value slice is routed | Complete | Gate 312 is routed to `forge init` project scaffold planning. |
| Canonical command surface is preserved | Complete | No new command or alias is introduced. |
| Runtime behavior is unchanged | Complete | No CLI code, schemas, diagnostics, release execution, Doctor execution, or init execution are changed. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

## Validation

Gate 311 is a planning/routing closeout gate. Required validation is document
and routing consistency plus normal local build/test smoke checks:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 312 should start `forge init` project scaffold planning. It should define
the local project scaffold contract, help/reserved JSON or dry-run planning
shape, source-file and directory expectations, overwrite/refusal boundaries,
and validation path into `forge validate`, while still stopping before
external tool execution, provider installation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, release
publication, remote repository calls, signing or attestation, plugin
mutation, or AI behavior.
