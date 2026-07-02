# Gate 125 - MCM Extender Verify-Existing Malformed Evidence Diagnostic Projection Coverage

Status: Complete

## Purpose

Gate 125 strengthens projection coverage for malformed JSON evidence
diagnostics in `forge package --target mcm-json --verify-existing`.

Gate 124 added explicit malformed JSON and non-object JSON diagnostics at the
file-reader boundary. Gate 125 does not add another verifier behavior. It
proves that the existing SARIF, GitHub workflow-command annotation, and
Markdown diagnostic summary projections carry malformed package-verification
JSON diagnostics correctly.

## Research grounding

- Documented: ADR-010 keeps existing package evidence verification under
  `forge package --target mcm-json --verify-existing` and forbids new aliases.
- Documented: ADR-011 identifies canonical issue JSON, SARIF diagnostic
  exchange, Markdown summaries, and deterministic fixture-backed tests.
- Documented: R008 treats SARIF generation as local and canonical, with
  GitHub upload as an optional publishing surface rather than a correctness
  requirement.
- Inferred: Malformed package evidence diagnostics should flow through the
  same canonical `DiagnosticReport` projections as other verify-existing
  package diagnostics.

## Implemented

Gate 125 implements:

- SARIF coverage for malformed existing `package-verification.json`,
- GitHub workflow-command annotation coverage for malformed existing
  `package-verification.json`,
- Markdown diagnostic summary coverage for malformed existing
  `package-verification.json`,
- documentation and slash-command routing updates for the projection coverage.

## Not implemented

Gate 125 does not implement:

- new diagnostic formats,
- new command aliases,
- new schema IDs,
- changed verifier behavior,
- non-object JSON projection coverage,
- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| SARIF malformed JSON diagnostic coverage | Complete | Golden test asserts `WF-BUILD-006`, title, command, URI, and message. |
| GitHub annotation malformed JSON diagnostic coverage | Complete | Golden test asserts file, title, message, and suggested fix. |
| Markdown summary malformed JSON diagnostic coverage | Complete | Golden test asserts command, summary count, rule, location, title, and message. |
| Command surface | Complete | No new command or alias was added. |
| Install behavior | Not implemented | Forge still verifies reports only and does not install files. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmJson
dotnet test WastelandForge.sln --no-build --no-restore -m:1
```

## Next gate

Gate 126 should close the current MCM Extender slice and move the next
implementation lane back to broader Forge value. Non-object JSON projection
coverage can be reopened later if it proves more valuable than capability and
Doctor environment work.
