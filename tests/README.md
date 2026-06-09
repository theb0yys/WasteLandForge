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

Run the full local suite serially:

```text
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
```
