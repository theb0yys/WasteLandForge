# Gate 304 - forge release verify Evidence Status Projection

Status: Complete
Date: 2026-07-05

## Goal

Add a local release dry-run evidence status projection that marks indexed
release-publish preflight evidence paths as present or missing for operators.

Default path:

```text
dist/release-dry-run/release-evidence-status.json
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
- Inferred: The next safe release slice is a local file-presence projection
  for indexed evidence, before any command fan-out or external execution.

## Implemented behavior

`forge release verify` now writes:

```text
dist/release-dry-run/staging/
dist/release-dry-run/validation.json
dist/release-dry-run/release-summary.json
dist/release-dry-run/release-verify.json
dist/release-dry-run/release-evidence-index.json
dist/release-dry-run/release-evidence-status.json
dist/release-dry-run/release-evidence-handoff.md
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

The status projection records:

- `schema-validation` status for `dist/release-dry-run/validation.json`,
- `capability-environment-validation` status for
  `dist/release-dry-run/capabilities-scan.json`,
- `package-validation` status for
  `dist/release-dry-run/package-verify.json`,
- `release-verification` status for
  `dist/release-dry-run/release-verify.json`,
- `exists: true|false`,
- `status: present|missing`,
- total, present, and missing evidence counts,
- explicit disabled execution boundaries.

The status projection is local file-presence evidence only. It does not parse
or validate the evidence content and does not run the command hints.

`release-evidence-status.json` is included in `release-verify.json` output
maps, CLI JSON output, human/plain output, help output, explain-output
metadata, `release-evidence-index.json`, `release-evidence-handoff.md`,
`build-manifest.json`, and `checksums.sha256`.

## Explicit non-goals

Gate 304 does not implement:

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
| Status projection emitted | Complete | `forge release verify` writes `dist/release-dry-run/release-evidence-status.json`. |
| Indexed evidence statuses projected | Complete | The status JSON marks validation/release verification present and capability/package evidence missing in the normal synthetic fixture run. |
| Handoff summary includes statuses | Complete | The Markdown handoff includes a status column and present/missing counts. |
| Index references status projection | Complete | `release-evidence-index.json` includes `statusProjection` and present/missing counts. |
| Execution boundaries explicit | Complete | The status JSON records command fan-out, external execution, publish, runtime probe, and AI flags as false. |
| Manifest coverage added | Complete | `build-manifest.json` includes `dist/release-dry-run/release-evidence-status.json`. |
| Checksum coverage added | Complete | `checksums.sha256` includes `release-evidence-status.json`. |
| Help/explain metadata updated | Complete | Help and explain-output metadata include the status projection path. |

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

Gate 305 should add a local missing-evidence action checklist for release
dry-run evidence status, while still stopping before command fan-out,
capability scan execution, package verify execution, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
