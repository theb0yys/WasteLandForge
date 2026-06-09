# Tests

This directory holds WastelandForge test project skeletons.

Gate 2 creates plain buildable SDK projects only.

Gate 4 adds a package-free executable verification harness in `WastelandForge.UnitTests` for core domain and diagnostic model behavior. Formal test framework dependencies and full test project wiring remain Gate 7 work.

Gate 6 verifies the CLI skeleton through direct `dotnet run` commands. Formal
CLI parser/help snapshot tests remain Gate 7 work.
