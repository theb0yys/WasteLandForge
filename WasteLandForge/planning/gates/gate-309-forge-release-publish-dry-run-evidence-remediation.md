# Gate 309 - forge release publish Dry-Run Evidence Remediation

Status: Complete
Date: 2026-07-05

## Goal

Add local `forge release publish` remediation summaries for missing,
malformed, or cross-link-mismatched release dry-run evidence.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  offline-first CLI surface.
- Documented: ADR-011 requires layered validation, deterministic local
  evidence, release validation, build manifests, checksums, and governance
  paths that do not require AI.
- Documented: Gate 308 made `forge release publish` evaluate cross-links
  across the local release dry-run evidence set.
- Inferred: The next safe publish slice is to summarize local manual
  remediation for failed dry-run evidence cross-link states before any command
  fan-out or publish behavior.

## Implemented behavior

`forge release publish` now reports `dryRunEvidenceRemediation` in JSON and
plain output.

The remediation summary derives from `dryRunCrossLinkEvidence` and reports:

- `no-action-required` when cross-links are complete,
- `action-required` for missing dry-run evidence files,
- `action-required` for malformed dry-run evidence files,
- `action-required` for cross-link-mismatched dry-run evidence,
- manual command hints using the release verify dry-run command,
- affected release dry-run evidence paths,
- blocker action items that remain manual and local.

The preflight also exposes `dryRunEvidenceRemediationStatus` and
`dryRunEvidenceRemediationActions` in the `releasePrepareEvidence` summary,
and `execution.dryRunEvidenceRemediationEvaluation` in JSON.

## Explicit non-goals

Gate 309 does not implement:

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
| Missing evidence remediation is summarized | Complete | Empty project publish preflight reports one manual missing-files remediation item. |
| Malformed evidence remediation is summarized | Complete | Corrupted release dry-run evidence reports one manual malformed-files remediation item. |
| Cross-link mismatch remediation is summarized | Complete | Mutated release dry-run links report one manual cross-link-mismatch remediation item. |
| Clean evidence has no remediation | Complete | Complete release dry-run evidence reports `no-action-required`. |
| JSON exposes the result | Complete | JSON includes `dryRunEvidenceRemediation`. |
| Plain output exposes the result | Complete | Plain output includes a release dry-run evidence remediation section. |
| No execution boundary is preserved | Complete | Remediation items are manual command hints and execution flags remain false. |
| Tests cover the slice | Complete | Golden tests assert missing, malformed, mismatch, and clean remediation states. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter ReleasePublish
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter Doctor
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 310 should add `forge doctor export` release dry-run evidence remediation
handoff projection from the local `forge release publish --dry-run` preflight,
while still stopping before command fan-out, automatic remediation, capability
scan execution, package verify execution, release uploads, attestation/signing,
external tool execution, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, or AI behavior.
