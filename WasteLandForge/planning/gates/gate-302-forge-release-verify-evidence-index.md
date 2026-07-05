# Gate 302 - forge release verify Evidence Index

Status: Complete
Date: 2026-07-05

## Goal

Add a local release dry-run evidence index that lists expected
release-publish preflight evidence paths and command hints under the selected
release dry-run output root.

Default path:

```text
dist/release-dry-run/release-evidence-index.json
```

## Research grounding

- Documented: ADR-010/R006 keep `forge release verify`,
  `forge capabilities scan`, `forge package`, and `forge release publish` in
  the canonical command surface.
- Documented: ADR-011 requires layered validation, deterministic local
  release evidence, local build manifests, checksums, and AI-optional release
  correctness.
- Documented: Gates 282 through 285 make `forge release publish` consume
  local schema-validation, capability/environment, package-validation, and
  release-verification evidence files under `dist/release-dry-run`.
- Documented: Gate 301 made `forge release verify` emit
  `dist/release-dry-run/release-verify.json` as first-class local
  release-verification evidence.
- Inferred: The next safe release slice is an operator-facing local evidence
  index with command hints, before any automatic command fan-out or external
  execution.

## Implemented behavior

`forge release verify` now writes:

```text
dist/release-dry-run/staging/
dist/release-dry-run/validation.json
dist/release-dry-run/release-summary.json
dist/release-dry-run/release-verify.json
dist/release-dry-run/release-evidence-index.json
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

The evidence index records:

- `schema-validation` at `dist/release-dry-run/validation.json`,
- `capability-environment-validation` at
  `dist/release-dry-run/capabilities-scan.json`,
- `package-validation` at `dist/release-dry-run/package-verify.json`,
- `release-verification` at `dist/release-dry-run/release-verify.json`,
- command hints for producing each evidence file,
- the release-publish dry-run preflight command hint,
- explicit disabled execution boundaries for command fan-out, capability scan
  execution, package verification execution, release publishing, uploads,
  signing/attestation, external tools, plugin mutation, MO2/GECK automation,
  runtime probes, real third-party fixtures, and AI.

`release-evidence-index.json` is included in `release-verify.json` output
maps, CLI JSON output, human/plain output, help output, explain-output
metadata, `build-manifest.json`, and `checksums.sha256`.

## Explicit non-goals

Gate 302 does not implement:

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
| Evidence index emitted | Complete | `forge release verify` writes `dist/release-dry-run/release-evidence-index.json`. |
| Expected preflight paths listed | Complete | The index lists validation, capability scan, package verify, and release verify evidence paths. |
| Command hints listed | Complete | The index includes producer command hints and the release-publish dry-run hint. |
| Execution boundaries explicit | Complete | The index records command fan-out, external execution, publish, runtime probe, and AI flags as false. |
| Manifest coverage added | Complete | `build-manifest.json` includes `dist/release-dry-run/release-evidence-index.json`. |
| Checksum coverage added | Complete | `checksums.sha256` includes `release-evidence-index.json`. |
| Help/explain metadata updated | Complete | Help and explain-output metadata include the evidence index path. |

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

Gate 303 should add a local release dry-run evidence handoff summary that
renders the evidence index for operators while still stopping before command
fan-out, capability scan execution, package verify execution, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
