# Gate 303 - forge release verify Evidence Handoff Summary

Status: Complete
Date: 2026-07-05

## Goal

Add a local release dry-run evidence handoff summary that renders the
release-publish preflight evidence index for operators.

Default path:

```text
dist/release-dry-run/release-evidence-handoff.md
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
- Documented: Gate 302 made `forge release verify` emit
  `dist/release-dry-run/release-evidence-index.json` with local evidence
  paths, command hints, and disabled execution boundaries.
- Inferred: The next safe release slice is a human-readable handoff over that
  local index, before any command fan-out or external execution.

## Implemented behavior

`forge release verify` now writes:

```text
dist/release-dry-run/staging/
dist/release-dry-run/validation.json
dist/release-dry-run/release-summary.json
dist/release-dry-run/release-verify.json
dist/release-dry-run/release-evidence-index.json
dist/release-dry-run/release-evidence-handoff.md
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

The handoff summary renders:

- project, command, mode, status, output root, and evidence-index path,
- required release-publish evidence rows,
- each evidence path,
- whether the evidence is produced by the current `release verify` run,
- command hints for evidence not produced by the current run,
- the release-publish dry-run preflight command hint,
- explicit disabled execution boundaries.

`release-evidence-handoff.md` is included in `release-verify.json` output
maps, CLI JSON output, human/plain output, help output, explain-output
metadata, `release-evidence-index.json`, `build-manifest.json`, and
`checksums.sha256`.

## Explicit non-goals

Gate 303 does not implement:

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
| Handoff summary emitted | Complete | `forge release verify` writes `dist/release-dry-run/release-evidence-handoff.md`. |
| Evidence index rendered | Complete | The Markdown summary lists validation, capability scan, package verify, and release verify evidence rows. |
| Command hints rendered | Complete | The Markdown summary includes producer command hints and the release-publish dry-run hint. |
| Execution boundaries rendered | Complete | The Markdown summary records command fan-out, external execution, publish, runtime probe, and AI boundaries as false. |
| Manifest coverage added | Complete | `build-manifest.json` includes `dist/release-dry-run/release-evidence-handoff.md`. |
| Checksum coverage added | Complete | `checksums.sha256` includes `release-evidence-handoff.md`. |
| Help/explain metadata updated | Complete | Help and explain-output metadata include the handoff path. |

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

Gate 304 should add a local release dry-run evidence status projection that
marks indexed evidence paths as present or missing for operators while still
stopping before command fan-out, capability scan execution, package verify
execution, release uploads, attestation/signing, external tool execution,
plugin mutation, MO2 automation, GECK automation, runtime probes, real
third-party plugin fixtures, or AI behavior.
