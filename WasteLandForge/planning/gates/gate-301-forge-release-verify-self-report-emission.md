# Gate 301 - forge release verify Self-Report Emission

Status: Complete
Date: 2026-07-05

## Goal

Update `forge release verify` so successful release dry-run runs write local
release-verification self-report evidence under:

```text
dist/release-dry-run/release-verify.json
```

## Research grounding

- Documented: ADR-010/R006 keep `forge release verify` in the canonical
  release command surface.
- Documented: ADR-011 requires local build manifests, checksums, deterministic
  fixture-backed tests, and AI-optional release correctness.
- Documented: Gate 9 made `forge release verify` the first real release
  dry-run command and established `dist/release-dry-run` output containment.
- Documented: Gate 285 and the release policy consume
  `dist/release-dry-run/release-verify.json` as prior local
  release-verification evidence.
- Documented: Gate 300 routed the next value slice to release-verify
  self-report emission rather than more reports package edge cases.

## Implemented behavior

`forge release verify` now writes:

```text
dist/release-dry-run/staging/
dist/release-dry-run/validation.json
dist/release-dry-run/release-summary.json
dist/release-dry-run/release-verify.json
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

The self-report:

- uses the existing `forge release verify --format json` report shape,
- records `command: release verify`,
- records `dryRun: true`,
- records release verification status and diagnostic summary,
- records the same output paths exposed by CLI JSON,
- remains under the selected release dry-run output root.

`build-manifest.json` and `checksums.sha256` now cover
`release-verify.json`. CLI JSON output reports `outputs.releaseVerification`.
Human/plain output reports that `release-verify.json` was written. `forge help
release verify` and `forge explain output` know the self-report path.

## Explicit non-goals

Gate 301 does not implement:

- reports package verify-existing behavior,
- package-validation evidence generation,
- capability scan execution,
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
| Release self-report emitted | Complete | `forge release verify` writes `dist/release-dry-run/release-verify.json`. |
| JSON shape is reused | Complete | The self-report uses the existing release verify JSON report shape. |
| Manifest coverage added | Complete | `build-manifest.json` includes `dist/release-dry-run/release-verify.json`. |
| Checksum coverage added | Complete | `checksums.sha256` includes `release-verify.json`. |
| CLI output surfaces path | Complete | JSON output includes `outputs.releaseVerification`; text output reports the file. |
| Help/explain metadata updated | Complete | Help and explain-output metadata include the self-report path. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

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

Gate 302 should add a local release dry-run evidence index that lists expected
release-publish preflight evidence paths and command hints under
`dist/release-dry-run`, while still stopping before command fan-out,
capability scan execution, package verify execution, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
