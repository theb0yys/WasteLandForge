# Gate 6 - CLI Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 5, ADR-010, ADR-011

## Gate Definition

Gate 6 creates the first canonical CLI skeleton.

This gate implements command dispatch for the ADR-010 command surface, top-level
and command help, `forge --version`, stable exit-code mapping, `--format`
plumbing, a real `forge validate` handler backed by the Gate 5 validation
pipeline, and reserved skeleton responses for commands that are not implemented
yet.

This gate does not implement capability scanning, generation, builds, package
staging, release verification, release publishing, Doctor export bundles, output
file writing, response files, shell completion, full configuration precedence,
SARIF/GitHub diagnostic projections, or formal CLI snapshot tests.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | The CLI is a stable public API and must keep names consistent, discoverable, and scriptable. | R006 / ADR-010 |
| Documented | `forge`, `forge --help`, `forge -h`, `forge help`, `forge help validate`, and `forge validate --help` should behave like a modern Git-style help system. | R006 / ADR-010 |
| Documented | The canonical command surface is `init`, `validate`, `capabilities list|scan|explain`, `generate`, `build`, `package`, `release verify|prepare|publish`, `docs`, `graph`, `explain`, `clean`, `doctor export`, `help`, and `--version`. | R006 / ADR-010 |
| Documented | Do not introduce convenience aliases such as top-level `scan`; grouped namespaces preserve future command room. | R006 / ADR-010 |
| Documented | Machine output uses explicit stable contracts, including `formatVersion`, `tool`, `command`, and `summary`. | R006 / ADR-010 |
| Documented | Exit codes are bounded and stable: `0` success, `1` blocking diagnostics, `2` usage, through `8` internal error. | R006 / ADR-010 |
| Documented | Core workflows remain offline-first and AI-optional. | R006 / ADR-010 and R008 / ADR-011 |
| Inferred | Gate 6 can reserve unimplemented canonical commands with stable help/status output rather than pretending to perform generation, packaging, release, or capability scanning. | Gate 0 / ADR-010 |
| Inferred | Gate 6 can stay package-free and defer `System.CommandLine` until the formal CLI test gate, because Gate 2 did not introduce package references and Gate 6 only needs the first skeleton. | Gate 2 / R006 |

## Deliverables

- `src/WastelandForge.Cli/Program.cs`
- `src/WastelandForge.Cli/ForgeCli.cs`
- `src/WastelandForge.Cli/CliHelpWriter.cs`
- `src/WastelandForge.Cli/CliConstants.cs`
- `src/WastelandForge.Cli/CliExitCode.cs`
- `src/WastelandForge.Cli/CliStatusJsonSerializer.cs`
- `src/WastelandForge.Cli/DiagnosticReportTextRenderer.cs`
- CLI version metadata in `src/WastelandForge.Cli/WastelandForge.Cli.csproj`
- CLI contract note in `docs/cli/README.md`

## Command Scope

Gate 6 supports:

- `forge`, `forge --help`, `forge -h`, and `forge help`;
- `forge help <command>`;
- `<command> --help` and `<command> -h`;
- `forge --version`;
- `forge validate [project-root]`;
- `forge validate --project <path>`;
- `forge validate --format human|plain|json`;
- `forge capabilities`, `forge release`, and `forge doctor` namespace help;
- reserved skeleton status for every non-validate canonical command;
- rejection of non-canonical command names.

Gate 6 accepts `--no-input` on `validate` as a no-op because validation is
already non-interactive in this slice.

## Format Scope

Gate 6 implements:

- human/plain validation summary and issue text;
- JSON validation report with `formatVersion`, `tool.name`, `tool.version`,
  `command`, `project`, `summary`, and `issues`;
- JSON status payloads for reserved canonical commands.

SARIF and GitHub annotation formats are explicitly rejected with exit code `2`
until the diagnostics projection gate.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet run --project tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- --version
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- --help
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- help validate
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- capabilities
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- capabilities scan --help
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate --project fixtures/projects/BrokenCases/MissingCapability --format plain
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build -- validate fixtures/projects/ExampleMod --format sarif
git diff --check
```

Results:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
PASS logical IDs enforce dotted lowercase identity
PASS JSON Pointer validates and escapes canonical locations
PASS semantic version constraints compare stable versions
PASS rule IDs use reserved WastelandForge families
PASS diagnostic issue JSON follows the canonical shape
--version: WastelandForge 0.1.0, exit 0.
--help: lists only the ADR-010 command surface, exit 0.
help validate: shows validate help, exit 0.
capabilities: shows namespace help, exit 0.
capabilities scan --help: shows command help, exit 0.
validate ExampleMod JSON: summary errors 0, exit 0.
validate MissingCapability plain: emits WF-SEM-014, expected exit 1.
capabilities scan --format json: stable reserved status, expected exit 2.
scan --format json: rejected non-canonical alias, expected exit 2.
validate --format sarif: rejected reserved projection, expected exit 2.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Decide whether to introduce `System.CommandLine` before formal parser/help snapshot tests. | Open | Gate 7 or later |
| Add the full global option set from R006, including output paths, verbosity, color, config, explain, dry-run, and interactivity semantics. | Open | Later CLI gate |
| Add `--output` file writing for machine outputs. | Open | Later CLI gate |
| Add SARIF and GitHub annotation projections. | Open | Gate 8 |
| Add real capability catalogue, scan, and explain handlers. | Open | Capability gate |
| Add real generate, build, package, docs, graph, explain, clean, release, and doctor handlers. | Open | Later workflow gates |
| Add formal CLI parser/help/golden tests. | Open | Gate 7 |

## Next Gate

Gate 7 creates fixture and test coverage:

1. formal test framework wiring,
2. fixture validation tests,
3. CLI help and JSON contract tests,
4. Windows path/filesystem tests,
5. golden diagnostic output checks.
