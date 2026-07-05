# Gate 305 - forge release verify Missing-Evidence Actions

Status: Complete
Date: 2026-07-05

## Goal

Add a local release dry-run missing-evidence action checklist that turns
missing indexed release-publish preflight evidence into explicit manual
operator actions.

Default path:

```text
dist/release-dry-run/release-evidence-actions.json
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
- Documented: Gate 303 made `forge release verify` emit a Markdown handoff
  summary for the same indexed evidence.
- Documented: Gate 304 made `forge release verify` emit a local file-presence
  status projection for the indexed evidence.
- Inferred: The next safe release slice is a local action checklist for
  missing evidence, before any command fan-out or external execution.

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
dist/release-dry-run/release-evidence-handoff.md
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

The action checklist records:

- `formatVersion: 0.1`,
- `kind: wastelandforge.release-dry-run-missing-evidence-actions`,
- links to the evidence index, evidence status projection, and handoff
  summary,
- total, present, missing, and action counts,
- one action per missing required evidence file,
- producer command and command hint for each missing evidence item,
- manual execution status for every action,
- explicit disabled execution boundaries.

In the normal synthetic fixture run, schema-validation and
release-verification evidence are present, while capability/environment and
package-validation evidence are missing. The checklist therefore contains two
manual actions:

- `produce-capability-environment-validation`,
- `produce-package-validation`.

`release-evidence-actions.json` is included in `release-verify.json` output
maps, CLI JSON output, human/plain output, help output, explain-output
metadata, `release-evidence-index.json`, `release-evidence-status.json`,
`release-evidence-handoff.md`, `build-manifest.json`, and `checksums.sha256`.

## Explicit non-goals

Gate 305 does not implement:

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
| Action checklist emitted | Complete | `forge release verify` writes `dist/release-dry-run/release-evidence-actions.json`. |
| Missing evidence actions projected | Complete | The normal synthetic fixture run emits two actions for missing capability/environment and package-validation evidence. |
| Execution boundaries explicit | Complete | The action checklist records command fan-out, capability scan execution, package verify execution, publish, runtime probe, and AI flags as false. |
| Index and status link checklist | Complete | `release-evidence-index.json` and `release-evidence-status.json` include `actionChecklist`. |
| Handoff summary includes actions | Complete | The Markdown handoff includes the action checklist path, missing action count, and action rows. |
| Manifest coverage added | Complete | `build-manifest.json` includes `dist/release-dry-run/release-evidence-actions.json`. |
| Checksum coverage added | Complete | `checksums.sha256` includes `release-evidence-actions.json`. |
| Help/explain metadata updated | Complete | Help and explain-output metadata include the action checklist path. |

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

Gate 306 should add a local release dry-run evidence collection plan skeleton,
while still stopping before command fan-out, capability scan execution,
package verify execution, release uploads, attestation/signing, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
