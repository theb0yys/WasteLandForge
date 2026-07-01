# Gate 89 - MCM Extender Package Verify-Existing SARIF and GitHub Output

Status: Complete

## Purpose

Gate 89 adds diagnostic output projections for existing MCM Extender package
evidence verification.

The command shape stays under the Gate 87 decision:

```text
forge package --target mcm-json --verify-existing
```

This gate lets that diagnostic mode emit SARIF 2.1.0 and GitHub
workflow-command annotations from the same canonical `DiagnosticReport` used by
the JSON and human outputs.

## Research Grounding

- Documented: ADR-010 defines the canonical command surface and says not to
  introduce undocumented convenience aliases.
- Documented: ADR-010 and ADR-011 identify SARIF as a machine diagnostic
  projection and GitHub workflow-command annotations as a CI convenience
  projection.
- Documented: ADR-011 requires canonical diagnostic issue data, stable rule
  IDs, deterministic fixture-backed testing, and offline-first correctness.
- Inferred: `forge package --target mcm-json --verify-existing` is a
  diagnostic package evidence mode, so it can reuse the Gate 10 SARIF and Gate
  11 GitHub projections without changing normal package generation output.
- Open: File-output support for package verification summaries remains a
  separate later gate.

## Scope

Gate 89 implements:

- `forge package --target mcm-json --verify-existing --format sarif`,
- `forge package --target mcm-json --verify-existing --format github`,
- SARIF `run.properties.command` value `package verify-existing`,
- GitHub workflow-command annotations for package evidence diagnostics,
- GitHub step-summary append when `GITHUB_STEP_SUMMARY` is present,
- continued rejection of `forge package --format sarif|github` when
  `--verify-existing` is not set,
- golden CLI coverage for SARIF, GitHub annotations, GitHub step-summary
  append, and non-diagnostic package format rejection.

Gate 89 does not implement:

- standalone verifier commands or aliases,
- `forge package verify`,
- package regeneration in verify-existing mode,
- `--summary <path>` for package verification,
- Data or MO2 installation,
- MO2 VFS/profile conflict inspection,
- game launch or runtime probe verification,
- FOMOD package creation,
- plugin record generation,
- new diagnostic rule families, rule IDs, or schemas.

## Validation Mapping

The command now:

1. Parses `--format sarif|github` for package verify-existing mode.
2. Rejects `--format sarif|github` for normal package generation.
3. Runs the existing file-based verifier over generated package evidence.
4. Renders the verifier `DiagnosticReport` through the existing SARIF or
   GitHub diagnostic projection.
5. Preserves exit code `0` for no blocking diagnostics and `1` for blocking
   diagnostics.

## Validation Results

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore` passed 37 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 301 tests.
- `git diff --check` passed with Git line-ending normalization warnings only.
- Targeted trailing-whitespace scan over changed files returned no matches.
- Stale current-Gate-88 wording scan returned no matches.
- Protected-file scan for Tales from the Age of Men / Age of Men / overhaul
  terms returned no matches outside ignored build output trees.

## Open Checks

| Check | Status | Notes |
|---|---|---|
| SARIF package verification output | Complete | Uses existing canonical SARIF projection. |
| GitHub package verification annotations | Complete | Uses existing GitHub annotation projection. |
| GitHub step-summary append | Complete | Reuses Gate 11 summary behavior when `GITHUB_STEP_SUMMARY` is present. |
| Normal package generation SARIF/GitHub rejection | Complete | `--verify-existing` is required for package diagnostic formats. |
| Package verification summary file output | Open | Candidate for Gate 90. |
| Data/MO2 install verification | Open | Requires separate safety and environment model. |
| In-game MCM Extender visibility | Open | Requires runtime evidence and launch/probe design. |

## Next Gate

Gate 90 should add Markdown summary file output for
`forge package --target mcm-json --verify-existing`.
