# Gate 9 - Release Dry-Run and Build Manifest Evidence

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 8, ADR-009, ADR-010, ADR-011

## Gate Definition

Gate 9 implements the first real release command: `forge release verify`.

This gate validates a project first, refuses release output outside project
`dist/`, writes deterministic dry-run staging evidence, writes Forge-owned
`build-manifest.json`, writes `checksums.sha256`, writes validation and release
summary reports, and updates CI to run the real CLI command in the
`release-dry-run` job.

This gate does not implement `release prepare`, `release publish`, ZIP/FOMOD
package creation, JsonSchema.Net runtime validation, YAML ingestion, real
generator execution, or capability scanning. Gate 10 implements canonical SARIF
diagnostic projection.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | The mandatory local output of every successful build should be a build manifest. | R009 / ADR-009 and R008 / ADR-011 |
| Documented | Release artifacts carry a local build manifest plus optional external provenance. | R008 / ADR-011 |
| Documented | The MVP spine includes build-manifest generation and a release dry-run. | R008 / ADR-011 |
| Documented | `forge release verify` is the canonical release readiness command. | R006 / ADR-010 |
| Documented | Release publish waits until governance is locked down. | R006 / ADR-010 and R008 / ADR-011 |
| Documented | Generated and distributable outputs belong under `generated/` and `dist/`. | R006 / ADR-010 and R009 / ADR-009 |
| Documented | Public fixtures must be synthetic and redistributable. | R008 / ADR-011 |
| Inferred | Gate 9 implements `release verify` before `release prepare` because R006 places verification before publish and Gate 8 already created the `release-dry-run` CI check. | R006 / Gate 8 |
| Inferred | The Gate 9 dry-run stages only source contracts and metadata because high-risk package creation and game-facing outputs remain out of scope. | R009 / Gate 9 scope |
| Open | Release-grade ZIP/FOMOD packaging and `release prepare` remain future work. | Gate 9 scope |

## Deliverables

- `src/WastelandForge.Provenance/ReleaseDryRunVerifier.cs`
- release dry-run result and digest model types in `WastelandForge.Provenance`
- `forge release verify` CLI handler
- release dry-run JSON and human renderers
- release dry-run unit and CLI tests
- CI `release-dry-run` job updated to call `forge release verify`
- fixture `dist/` ignore rule

## Command Scope

Implemented:

```text
forge release verify [project-root] [--project <path>] [--output dist/<name>] [--format human|plain|json] [--dry-run] [--no-input]
```

Default output:

```text
dist/release-dry-run/
```

Written evidence:

```text
staging/source/
validation.json
release-summary.json
build-manifest.json
checksums.sha256
```

The command rejects output paths outside project `dist/` with `WF-REL-001`.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate9
copy fixtures/projects/ExampleMod to a temp directory
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- release verify <temp-examplemod> --format json --no-input
git diff --check
```

Results:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
WastelandForge.UnitTests: 8 passed.
WastelandForge.SchemaTests: 2 passed.
WastelandForge.SemanticTests: 2 passed.
WastelandForge.GoldenTests: 6 passed.
WastelandForge.WindowsTests: 2 passed.
WastelandForge.BackCompatTests: 2 passed.
Total: 22 passed.
TRX files emitted under TestResults/Gate9.
Release verify temp fixture command succeeded with status passed.
Fixture project dist output was not left in the repository.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Replace placeholder SARIF with canonical diagnostic SARIF projection. | Complete | Gate 10 |
| Implement release prepare and deterministic archive creation. | Open | Later release gate |
| Implement release publish with explicit human approval and repository governance checks. | Open | Later release gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |
| Resolve or keep documenting solution-level parallel VSTest/xUnit hang. | Open | Later test gate |

## Next Gate

Gate 10 creates canonical SARIF diagnostic projection.
