# Tests

This directory holds WastelandForge test projects.

Gate 2 creates plain buildable SDK projects only.

Gate 4 added a package-free executable verification harness in
`WastelandForge.UnitTests` for core domain and diagnostic model behavior.

Gate 6 verifies the CLI skeleton through direct `dotnet run` commands. Formal
CLI parser/help snapshot tests remain Gate 7 work.

Gate 7 replaces the package-free harness with xUnit v3 and VSTest wiring across
the existing test projects:

- `WastelandForge.UnitTests` - core domain and diagnostic report behavior.
- `WastelandForge.SchemaTests` - manifest schema and schema catalog behavior.
- `WastelandForge.SemanticTests` - validation pipeline fixture behavior.
- `WastelandForge.GoldenTests` - CLI help and JSON golden output contracts.
- `WastelandForge.WindowsTests` - Windows path and filesystem behavior.
- `WastelandForge.BackCompatTests` - immutable schema/catalog compatibility.

Gate 9 adds release dry-run tests for Forge-owned build manifest and checksum
evidence through `WastelandForge.UnitTests` and CLI coverage through
`WastelandForge.GoldenTests`.

Gate 10 adds SARIF projection tests in `WastelandForge.UnitTests` and CLI SARIF
contract/output-file tests in `WastelandForge.GoldenTests`.

Gate 11 adds Markdown summary and GitHub annotation projection tests in
`WastelandForge.UnitTests` and CLI coverage for `--format github`, `--summary`,
and `GITHUB_STEP_SUMMARY` behavior in `WastelandForge.GoldenTests`.

Gate 12 adds runtime manifest schema validation coverage in
`WastelandForge.SchemaTests` and YAML fixture coverage in
`WastelandForge.SemanticTests`.

Gate 13 adds dependency and capability registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus broken
registry fixture coverage in `WastelandForge.SemanticTests`.

Gate 14 adds asset registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus broken
asset registry fixture coverage in `WastelandForge.SemanticTests`.

Gate 15 adds asset path semantic fixture coverage in
`WastelandForge.SemanticTests`.

Gate 16 adds asset type-specific semantic fixture coverage in
`WastelandForge.SemanticTests` for source signatures and target root
conventions.

Gate 17 adds voice and dialogue asset fixture coverage in
`WastelandForge.SemanticTests` for voice target shape and WAV/OGG/LIP pair
diagnostics.

Gate 18 adds dialogue registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue registry and voice worklist fixture coverage in
`WastelandForge.SemanticTests`.

Gate 19 adds quest registry schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus quest
registry and dialogue quest reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 20 adds quest registry schema `0.2.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
stage/objective schema and objective stage-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 21 adds quest registry schema `0.3.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
transition schema and transition stage-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 22 adds quest registry schema `0.4.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
condition schema and condition stage-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 23 adds quest registry schema `0.5.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
result-script schema and result-script condition-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 24 adds quest registry schema `0.6.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
variable schema and condition variable-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 25 adds dialogue registry schema `0.2.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue condition schema and quest-state reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 26 adds dialogue registry schema `0.3.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue result-script schema fixture coverage in
`WastelandForge.SemanticTests`.

Gate 27 adds dialogue registry schema `0.4.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue topic schema and topic/link reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 28 adds dialogue registry schema `0.5.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue quest gate schema and quest-state reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 29 adds dialogue registry schema `0.6.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue result-script mutation schema and mutation variable-reference fixture
coverage in `WastelandForge.SemanticTests`.

Gate 30 adds dialogue registry schema `0.7.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus
dialogue Link From schema and source topic-reference fixture coverage in
`WastelandForge.SemanticTests`.

Gate 31 adds semantic fixture coverage in `WastelandForge.SemanticTests` for
dialogue link graph endpoints: `linkTo` targets and `linkFrom` sources that are
declared topics but have no authored dialogue line.

Gate 32 adds dialogue registry schema `0.8.0` schema/catalog coverage in
`WastelandForge.SchemaTests` and `WastelandForge.BackCompatTests`, plus prompt
route schema and duplicate prompt route fixture coverage in
`WastelandForge.SemanticTests`.

Run the full local suite serially:

```text
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
```
