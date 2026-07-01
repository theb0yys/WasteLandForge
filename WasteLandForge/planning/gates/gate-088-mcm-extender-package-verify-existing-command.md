# Gate 88 - MCM Extender Package Verify-Existing Command

Status: Complete

## Purpose

Gate 88 implements the first public command skeleton for verifying existing
MCM Extender package evidence.

The command shape follows the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

It reads existing generated package evidence and runs the internal
file-based verifier. It does not regenerate package outputs.

## Research Grounding

- Documented: ADR-010 defines the canonical command surface and says not to
  introduce undocumented convenience aliases.
- Documented: ADR-009 says `forge package` assembles distributable staging
  trees and package evidence, while generated artifacts remain disposable and
  traceable through local manifests.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed testing, local build manifests, and offline-first release
  evidence.
- Inferred: Existing generated package evidence verification belongs under
  `forge package` because it validates package staging evidence rather than
  source contracts or release publication.
- Open: SARIF/GitHub output for package verification remains a later gate.

## Scope

Gate 88 implements:

- `forge package --target mcm-json --verify-existing`,
- default evidence root `dist/mcm-json`,
- `--output <path>` evidence-root selection under project `dist/`,
- `--project <path>`,
- `--format human|plain|json`,
- file-based verification using
  `McmPackageVerificationEvidenceFileVerifier`,
- JSON output with command, target, mode, status, summary, output paths, and
  issues,
- human/plain text output for the same verification result,
- exit code `0` for no blocking diagnostics,
- exit code `1` for blocking package-verification diagnostics,
- exit code `2` for parse or usage errors,
- golden CLI coverage for successful verification and edited payload failure.

Gate 88 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- SARIF/GitHub output for package verification,
- package regeneration in verify-existing mode,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation.

## Validation Mapping

The command now:

1. Parses `--verify-existing` under `forge package`.
2. Rejects `--verify-existing` with `--dry-run`.
3. Resolves the evidence root under project `dist/`.
4. Builds a `McmPackageVerificationEvidenceFileVerificationRequest` from
   existing evidence files.
5. Runs `McmPackageVerificationEvidenceFileVerifier`.
6. Renders human/plain/json output.
7. Returns `0` or `1` based on blocking diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors before
  documentation updates.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 33 tests before documentation updates.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| Verify-existing command skeleton | Complete | Implemented under `forge package --target mcm-json --verify-existing`. |
| Successful verify-existing coverage | Complete | Golden CLI test verifies existing generated package evidence. |
| Edited payload failure coverage | Complete | Golden CLI test expects `WF-BUILD-006`. |
| SARIF/GitHub package diagnostics | Open | Candidate for Gate 89. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 89 should add SARIF and GitHub diagnostic output support for
`forge package --target mcm-json --verify-existing`.
