# Gate 4 - Core Domain and Diagnostics

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 3, ADR-007, ADR-011

## Gate Definition

Gate 4 creates the core domain and diagnostic model needed before loader and validation pipeline work begins.

This gate does not implement source loading, YAML parsing, schema evaluation, semantic validation, SARIF output, CLI commands, fixtures, or CI.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Logical IDs should be stable dotted lowercase identifiers; reverse-DNS is preferred but not mandatory. | R004 / ADR-007 |
| Documented | Diagnostics should use JSON Pointer as the canonical location format. | R004 / ADR-007 |
| Documented | Diagnostic output should use stable rule IDs, explicit severities, related locations, documentation URIs, and structured JSON. | R004 / ADR-007 |
| Documented | Reserved rule families are `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, and `WF-SEC-*`. | R008 / ADR-011 |
| Documented | Canonical issue JSON maps outward to console, SARIF, Markdown, and GitHub annotations later. | R008 / ADR-011 |
| Inferred | Gate 4 can use a package-free executable unit verification harness because Gate 2 deferred formal test framework package wiring to Gate 7. | Gate 0, Gate 2 |

## Deliverables

- `src/WastelandForge.Core/LogicalId.cs`
- `src/WastelandForge.Core/JsonPointer.cs`
- `src/WastelandForge.Core/SemanticVersion.cs`
- `src/WastelandForge.Core/VersionConstraint.cs`
- `src/WastelandForge.Core/RuleId.cs`
- `src/WastelandForge.Core/DiagnosticSeverity.cs`
- `src/WastelandForge.Core/SourceLocation.cs`
- `src/WastelandForge.Core/DiagnosticIssue.cs`
- `src/WastelandForge.Core/DiagnosticIssueJsonSerializer.cs`
- `docs/governance/diagnostic-model.md`
- package-free Gate 4 verification harness in `tests/WastelandForge.UnitTests`

## Model Scope

Gate 4 implements:

- stable dotted lowercase logical ID parsing,
- JSON Pointer validation and escaping,
- semantic version parsing,
- minimum/exclusive version constraints,
- reserved WastelandForge rule ID parsing,
- diagnostic severity mapping,
- source locations with file, pointer, line, and column,
- canonical diagnostic issue shape,
- deterministic camel-case issue JSON serialization.

## Test Scope

Gate 0 requested unit evidence for core model behavior. Gate 2 kept formal xUnit/Microsoft.NET.Test.Sdk wiring out of scope until Gate 7.

Gate 4 therefore uses a package-free executable harness in `tests/WastelandForge.UnitTests` to verify:

- logical ID validation,
- JSON Pointer validation and escaping,
- version constraint comparison,
- reserved rule ID validation,
- diagnostic issue JSON shape.

Formal test framework conversion remains Gate 7 work.

## Verification

Commands:

```text
dotnet build WastelandForge.sln -c Release
dotnet run --project tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build
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
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Convert package-free verification harness to formal test framework. | Open | Gate 7 |
| Add diagnostic report aggregate shape once validation emits multiple issues. | Open | Gate 5 |
| Add SARIF projection from issue JSON. | Open | Gate 8 |
| Add Markdown and console projections. | Open | Gate 6 or Gate 8 |

## Next Gate

Gate 5 creates the loader and validation pipeline:

1. load/source validation,
2. schema validation placeholder,
3. semantic placeholder stage,
4. capability placeholder stage,
5. deterministic issue JSON from a synthetic input.
