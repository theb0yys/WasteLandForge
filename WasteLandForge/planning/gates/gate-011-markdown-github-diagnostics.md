# Gate 11 - Markdown and GitHub Diagnostic Projections

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 8, Gate 10, ADR-007, ADR-010, ADR-011

## Gate Definition

Gate 11 implements Markdown diagnostic summaries and GitHub workflow-command
annotations from WastelandForge canonical diagnostics.

This gate adds deterministic Markdown and GitHub annotation renderers, enables
`forge validate --format github`, enables `forge release verify --format
github`, adds `--summary <path>` for Markdown diagnostic summaries, appends the
same Markdown summary to `GITHUB_STEP_SUMMARY` when GitHub annotation format is
used, and updates CI to produce Markdown validation and release summaries.

This gate does not implement YAML ingestion, JsonSchema.Net runtime validation,
line-precise YAML locations, VS Code problem matchers, a language server, or
GitHub repository ruleset API automation.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Diagnostic JSON is canonical; Markdown, SARIF, console text, and GitHub annotations are projections from the same issue data. | R008 / ADR-011 |
| Documented | Rule IDs must remain stable across JSON, SARIF, Markdown, and GitHub outputs. | R004 / ADR-007 and R008 / ADR-011 |
| Documented | GitHub annotations are convenience output; local validation remains the correctness path. | R008 / ADR-011 |
| Documented | `--format github` is part of the ADR-010 output format set. | R006 / ADR-010 |
| Inferred | Markdown summaries use `--summary <path>` instead of `--format markdown` because R006 lists `human`, `plain`, `json`, `sarif`, and `github` as the output format set. | R006 / ADR-010 and Gate 11 scope |
| Inferred | GitHub format appends to `GITHUB_STEP_SUMMARY` only when that environment file is present, preserving local offline behavior. | R008 / ADR-011 and GitHub Actions environment behavior |
| Open | Line-precise GitHub annotations require loader support for source line and column data. | Later loader gate |
| Open | VS Code problem matchers and editor diagnostics remain separate editor integration work. | Later editor gate |

## Deliverables

- `src/WastelandForge.Core/DiagnosticReportMarkdownRenderer.cs`
- `src/WastelandForge.Core/DiagnosticReportGitHubAnnotationRenderer.cs`
- `forge validate --format github`
- `forge validate --summary <path>`
- `forge release verify --format github`
- `forge release verify --summary <path>`
- CI Markdown validation summary artifact
- CI release verify Markdown summary artifact
- Markdown and GitHub annotation unit and CLI tests

## Projection Mapping

Gate 11 maps diagnostics as follows:

```text
DiagnosticReport       -> Markdown summary document
DiagnosticIssue.RuleId -> Markdown table rule and GitHub annotation title
DiagnosticSeverity     -> Markdown severity and GitHub error/warning/notice
SourceLocation.File    -> Markdown location and GitHub file property
SourceLocation.Pointer -> Markdown location suffix and GitHub message text
SuggestedFix           -> Markdown detail and GitHub message text
DocsUri                -> Markdown detail and GitHub message text
```

GitHub annotations include line and column only when canonical
`SourceLocation` includes line and column values. Current JSON fixture
diagnostics preserve JSON Pointer locations but do not yet provide line and
column data.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate11
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/BrokenCases/MissingCapability --format github --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format github --summary <temp-wf-gate11.md> --no-input
git diff --check
```

Results:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
WastelandForge.UnitTests: 11 passed.
WastelandForge.SchemaTests: 2 passed.
WastelandForge.SemanticTests: 2 passed.
WastelandForge.GoldenTests: 12 passed.
WastelandForge.WindowsTests: 2 passed.
WastelandForge.BackCompatTests: 2 passed.
Total: 31 passed.
TRX files emitted under TestResults/Gate11.
Broken MissingCapability fixture emitted GitHub annotation output with WF-SEM-014 and returned exit 1.
ExampleMod Markdown summary output-file command succeeded using a temp output path.
Release verify unsafe-output command emitted GitHub annotation output with WF-REL-001 and returned exit 1.
git diff --check passed with CRLF normalization warnings only.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Add line and column mapping for YAML diagnostics once YAML ingestion exists. | Open | Later loader gate |
| Add VS Code problem matcher output once editor integration begins. | Open | Later editor gate |
| Replace capability placeholder in build manifest with real resolved capability set. | Open | Capability/build gate |
| Apply GitHub repository rulesets for `main` and `release/*`. | Open | Repository settings |

## Next Gate

Gate 12 should move the loader from JSON-only fixtures toward YAML ingestion and
runtime schema validation.
