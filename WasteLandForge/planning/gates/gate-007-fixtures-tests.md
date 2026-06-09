# Gate 7 - Fixtures and Tests

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 5, Gate 6, ADR-010, ADR-011

## Gate Definition

Gate 7 creates formal fixture-backed test coverage for the current v0.1 spine.

This gate wires xUnit v3 and VSTest into the existing test projects, converts
the package-free Gate 4 harness into unit tests, adds schema tests, semantic
fixture tests, CLI golden output tests, Windows path/filesystem tests, and
backwards-compatibility tests for immutable schema IDs.

This gate does not implement YAML parsing, JsonSchema.Net runtime schema
evaluation, SARIF/GitHub diagnostic projections, CI workflows, generated output
tests, release dry-runs, or real capability scanning.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Public fixtures must be synthetic and redistributable, not Bethesda assets or third-party mod files. | R008 / ADR-011 |
| Documented | The project should use unit tests, schema tests, semantic tests, golden output tests, fixture tests, Windows path/filesystem tests, backwards-compatibility tests, and release dry-run tests. | R008 / ADR-011 |
| Documented | JSON is canonical for issue data and machine output; TRX is the native .NET test record. | R008 / ADR-011 |
| Documented | CLI JSON payloads use stable machine contracts and help output should preserve the ADR-010 command surface. | R006 / ADR-010 |
| Documented | Published schema URLs are immutable and versioned schema IDs must remain resolvable. | R004 / ADR-007 and R008 / ADR-011 |
| Inferred | Gate 7 should use xUnit v3 with VSTest because Gate 2 checked xUnit v3 and `Microsoft.NET.Test.Sdk`, and R008 needs TRX-capable .NET test output for CI. | Gate 2 / R008 |
| Inferred | Solution-level test execution should be serial with `-m:1` in this gate because parallel solution-level VSTest execution hung locally while every test project passed individually. | Local Gate 7 verification |

## Deliverables

- `tests/Directory.Build.props`
- xUnit v3 tests in:
  - `tests/WastelandForge.UnitTests`
  - `tests/WastelandForge.SchemaTests`
  - `tests/WastelandForge.SemanticTests`
  - `tests/WastelandForge.GoldenTests`
  - `tests/WastelandForge.WindowsTests`
  - `tests/WastelandForge.BackCompatTests`
- public golden fixtures:
  - `fixtures/golden/cli/top-level-help.txt`
  - `fixtures/golden/validation/examplemod.json`
- `src/WastelandForge.Cli/AssemblyInfo.cs` for test-only internal CLI access.

## Test Scope

Gate 7 covers:

- core domain model and diagnostic report JSON behavior;
- manifest schema JSON and schema catalog lookup;
- valid fixture validation;
- deterministic `WF-SEM-014` missing capability fixture validation;
- CLI top-level help golden output;
- CLI validation JSON golden output;
- reserved command JSON shape;
- rejection of non-canonical `scan` alias;
- Windows registry path escaping;
- Windows path separator normalization;
- immutable manifest schema ID compatibility.

## Fixture Scope

All Gate 7 fixtures are synthetic:

- JSON project contracts under `fixtures/projects`;
- CLI help golden text under `fixtures/golden/cli`;
- validation JSON golden output under `fixtures/golden/validation`.

The fixture corpus contains no Bethesda assets, third-party mod files, private
install paths, or generated plugin binaries.

## Verification

Commands:

```text
dotnet restore WastelandForge.sln
dotnet build WastelandForge.sln -c Release
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger "console;verbosity=minimal"
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1 --logger trx --results-directory TestResults/Gate7
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
TRX files emitted under TestResults/Gate7.
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Keep solution-level `dotnet test` serial with `-m:1` unless the VSTest/xUnit parallel hang is resolved. | Open | Gate 8 |
| Add SARIF/GitHub diagnostic projection tests once those formats exist. | Open | Gate 8 |
| Add JsonSchema.Net runtime validation tests once runtime schema evaluation exists. | Open | Later schema validation gate |
| Add YAML safe-subset loader tests once YAML ingestion exists. | Open | Later loader gate |
| Add release dry-run tests once build manifests and release verification exist. | Open | Gate 9 |

## Next Gate

Gate 8 creates the CI and governance baseline:

1. GitHub Actions Windows lane,
2. Ubuntu fast-validation lane,
3. restore/build/test using the Gate 7 suite,
4. TRX artifact collection,
5. SARIF placeholder or later projection wiring,
6. governance files such as CODEOWNERS, SECURITY.md, and Dependabot baseline.
