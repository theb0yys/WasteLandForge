# Gate 10 - SARIF Diagnostic Projection

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 8, Gate 9, ADR-007, ADR-010, ADR-011

## Gate Definition

Gate 10 implements SARIF 2.1.0 projection from WastelandForge canonical
diagnostics.

This gate adds a deterministic SARIF serializer, enables
`forge validate --format sarif`, supports `--output` for validation machine
artifacts, updates CI to generate SARIF through the real CLI command, and
removes the Gate 8 placeholder SARIF generation path from the CI artifact
script.

This gate does not implement GitHub workflow-command annotations, Markdown
diagnostic summaries, YAML ingestion, JsonSchema.Net runtime validation, or
SARIF for unimplemented reserved commands.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | JSON is the canonical diagnostic model and SARIF is a projection from that model. | R008 / ADR-011 |
| Documented | SARIF 2.1.0 is the diagnostics exchange format for CI and code scanning. | R006 / ADR-010 and R008 / ADR-011 |
| Documented | SARIF generation must be local and canonical; GitHub upload is an optional publishing surface. | R008 / ADR-011 |
| Documented | Rule IDs must remain stable across console, JSON, SARIF, and future editor outputs. | R004 / ADR-007 and R006 / ADR-010 |
| Documented | `--format` is the canonical output selector; avoid one-off flags such as `--sarif`. | R006 / ADR-010 |
| Inferred | Gate 10 should update the Ubuntu validation lane to run `forge validate --format sarif` before upload because Gate 8 only created a placeholder upload surface. | Gate 8 / R008 |
| Inferred | `validate --output` is implemented for Gate 10 because R006 uses `--output` for file-oriented machine artifacts. | R006 / ADR-010 |
| Open | GitHub workflow-command annotations remain a separate output projection. | Gate 10 scope; completed in Gate 11 |
| Open | Markdown diagnostic summaries remain a separate output projection. | Gate 10 scope; completed in Gate 11 |

## Deliverables

- `src/WastelandForge.Core/DiagnosticReportSarifSerializer.cs`
- `forge validate --format sarif`
- `forge validate --format sarif --output <path>`
- CI SARIF export through the CLI
- removal of placeholder SARIF generation from `eng/ci/New-CiArtifacts.ps1`
- SARIF unit and CLI contract tests

## SARIF Mapping

Gate 10 maps diagnostics as follows:

```text
DiagnosticReport       -> SARIF run
DiagnosticIssue.RuleId -> tool.driver.rules[].id and results[].ruleId
DiagnosticSeverity     -> result.level
SourceLocation.File    -> artifactLocation.uri
SourceLocation.Pointer -> physicalLocation.properties.jsonPointer
Diagnostic fingerprint -> partialFingerprints.wastelandforgeFingerprint
```

The serializer emits SARIF 2.1.0 with `WastelandForge` as the tool driver and
keeps locations repository-relative when the validation pipeline provides
relative source locations.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate10
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingCapability --format sarif --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format sarif --output <temp-wf-gate10.sarif> --no-input
git diff --check
```

Results:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
WastelandForge.UnitTests: 9 passed.
WastelandForge.SchemaTests: 2 passed.
WastelandForge.SemanticTests: 2 passed.
WastelandForge.GoldenTests: 8 passed.
WastelandForge.WindowsTests: 2 passed.
WastelandForge.BackCompatTests: 2 passed.
Total: 25 passed.
TRX files emitted under TestResults/Gate10.
Broken MissingCapability fixture emitted SARIF 2.1.0 with WF-SEM-014 and returned exit 1.
ExampleMod SARIF output-file command succeeded using a temp output path.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add GitHub workflow-command annotation output. | Complete | Gate 11 |
| Add Markdown diagnostic summaries. | Complete | Gate 11 |
| Add SARIF contract tests for line-precise YAML diagnostics once YAML ingestion exists. | Open | Later loader gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |

## Next Gate

Gate 11 creates Markdown summaries and GitHub annotations. Gate 12 should move
the loader from JSON-only fixtures toward YAML ingestion and runtime schema
validation.
