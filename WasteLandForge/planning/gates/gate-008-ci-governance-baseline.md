# Gate 8 - CI and Governance Baseline

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 7, ADR-011

## Gate Definition

Gate 8 creates the first GitHub Actions and repository governance baseline.

This gate wires the Gate 7 test suite into GitHub Actions, keeps the mandatory
Windows lane, adds an Ubuntu fast-validation lane, emits TRX artifacts, writes
local CI governance artifacts, creates placeholder SARIF for the upload surface,
and adds CODEOWNERS, SECURITY.md, Dependabot, a PR template, and repository
ruleset guidance.

Gate 10 supersedes the placeholder SARIF surface with
`forge validate --format sarif`.

This gate does not implement canonical SARIF diagnostic projection, YAML
ingestion, JsonSchema.Net runtime validation, real release packaging, release
publishing, or the Forge-owned build-manifest writer.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | CI is GitHub Actions-first with a mandatory Windows lane and an Ubuntu fast-validation lane. | R008 / ADR-011 |
| Documented | TRX is the native .NET test record and SARIF 2.1.0 is the diagnostics exchange format. | R008 / ADR-011 |
| Documented | SARIF generation must be local and canonical; GitHub upload is an optional publishing surface. | R008 / ADR-011 |
| Documented | Repository governance should use least-privilege workflow permissions, CODEOWNERS, SECURITY.md, Dependabot, and GitHub rulesets. | R008 / ADR-011 |
| Documented | Contribution rules must not require AI. | R008 / ADR-011 |
| Documented | Production workflows should pin third-party actions to full commit SHAs. | R008 / ADR-011 |
| Inferred | Gate 8 uses full commit SHAs for first-party GitHub actions because R008 requires immutable action references for production workflows. | R008 / Gate 8 implementation |
| Inferred | Gate 8 keeps `dotnet test -m:1` in CI because Gate 7 verified serial solution-level testing and recorded the parallel test hang as open. | Gate 7 verification |
| Inferred | The `release-dry-run` CI job is a governance placeholder until Gate 9 implements real release verification and Forge build manifests. | R008 / Gate 7 open checks |
| Open | The canonical SARIF projection from WastelandForge diagnostic JSON is not implemented yet. | Gate 8 scope |
| Open | GitHub rulesets must be applied in repository settings after this file baseline exists. | Gate 8 scope |

## Deliverables

- `.github/workflows/ci.yml`
- `.github/CODEOWNERS`
- `.github/dependabot.yml`
- `.github/pull_request_template.md`
- `SECURITY.md`
- `docs/governance/repository-rulesets.md`
- `eng/ci/New-CiArtifacts.ps1`

## CI Scope

Gate 8 CI provides these status checks:

- `validate-ubuntu`
- `build-test-windows`
- `release-dry-run` on `main`, `release/*`, and manual dispatch

The Ubuntu lane restores, builds, tests, emits TRX, and attempts SARIF upload
as an optional publishing surface. Gate 10 replaces the placeholder SARIF file
with CLI-generated SARIF.

The Windows lane restores, builds, tests, emits TRX, and writes CI governance
artifacts.

The release dry-run lane validates the synthetic fixture and writes CI
governance artifacts. It does not package or publish releases.

## Governance Scope

Gate 8 records:

- code-owner review baseline for sensitive paths;
- Dependabot checks for NuGet and GitHub Actions;
- security reporting and supported branch policy;
- PR template research grounding, validation, fixture, and AI-assist disclosure;
- intended GitHub rulesets for `main` and `release/*`.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate8
./eng/ci/New-CiArtifacts.ps1 -Lane local-gate8 -OutputDirectory artifacts/gate8-local
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json --no-input
git diff --check
```

Results:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
WastelandForge.UnitTests: 6 passed.
WastelandForge.SchemaTests: 2 passed.
WastelandForge.SemanticTests: 2 passed.
WastelandForge.GoldenTests: 4 passed.
WastelandForge.WindowsTests: 2 passed.
WastelandForge.BackCompatTests: 2 passed.
Total: 18 passed.
TRX files emitted under TestResults/Gate8.
CI governance artifacts emitted under artifacts/gate8-local:
  build-manifest.json
  checksums.sha256
Gate 10 now emits wastelandforge-validation.sarif through forge validate.
Release fixture validation command succeeded.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Apply GitHub repository rulesets for `main` and `release/*`. | Open | Repository settings |
| Replace placeholder SARIF with canonical diagnostic SARIF projection. | Complete | Gate 10 |
| Replace release-dry-run placeholder with real release verification and Forge build-manifest writer. | Complete | Gate 9 |
| Resolve or keep documenting solution-level parallel VSTest/xUnit hang. | Open | Later test gate |

## Next Gate

Gate 9 creates release dry-run and build manifest evidence.
