# Gate 124 - MCM Extender Verify-Existing Malformed Evidence Diagnostics

Status: Complete

## Purpose

Gate 124 makes malformed required JSON evidence files explicit in
`forge package --target mcm-json --verify-existing`.

Gate 123 made absent evidence files clear. Gate 124 covers the next file-read
boundary: evidence files that exist but cannot be parsed as JSON, and evidence
files that parse as JSON but are not the expected top-level object shape.

## Research grounding

- Documented: ADR-010 keeps existing package evidence verification under
  `forge package --target mcm-json --verify-existing` and forbids new aliases.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed tests, and canonical diagnostics that can be projected through
  CLI output formats.
- Documented: R008 reserves `WF-BUILD-*` for build/package validation
  failures and treats package validation as part of the layered validation
  stack.
- Inferred: Malformed package evidence is a package validation failure and
  should continue to use the existing `WF-BUILD-006` package evidence rule.

## Implemented

Gate 124 implements:

- explicit malformed JSON diagnostics in the package evidence JSON reader,
- explicit non-object JSON diagnostics with evidence path context,
- unit coverage for malformed `package-verification.json`,
- unit coverage for non-object `package-verification.json`,
- golden CLI JSON coverage for malformed existing `package-verification.json`,
- documentation and slash-command routing updates for the new diagnostic
  behavior.

## Not implemented

Gate 124 does not implement:

- new command aliases,
- new diagnostic rule IDs,
- new schema IDs,
- changed package generation behavior,
- install behavior for Data or MO2,
- SARIF/GitHub/Markdown projection-specific coverage for malformed evidence,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Malformed JSON evidence diagnostic | Complete | Malformed package-verification JSON reports `MCM package package verification evidence is malformed JSON`. |
| Non-object JSON evidence diagnostic | Complete | Array-shaped package-verification JSON reports `MCM package package verification evidence is not a JSON object`. |
| CLI JSON projection | Complete | `forge package --target mcm-json --verify-existing --format json` returns exit code `1` and one `WF-BUILD-006` issue for malformed package-verification evidence. |
| Command surface | Complete | No command or alias was added. |
| Install behavior | Not implemented | Forge still verifies local evidence only and does not mutate Data or MO2. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmPackageVerificationEvidence
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmJson
dotnet test WastelandForge.sln --no-build --no-restore -m:1
```

## Next gate

Gate 125 should add focused SARIF, GitHub annotation, and Markdown diagnostic
summary projection coverage for malformed evidence failures.
