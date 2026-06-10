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

Run the full local suite serially:

```text
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
```
