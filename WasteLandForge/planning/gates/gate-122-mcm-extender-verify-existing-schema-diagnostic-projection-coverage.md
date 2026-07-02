# Gate 122 - MCM Extender Verify-Existing Schema Diagnostic Projection Coverage

Status: Complete

## Purpose

Gate 122 strengthens test coverage for schema-gated diagnostics in
`forge package --target mcm-json --verify-existing`.

Gates 118 through 121 added schema-first verification for install-plan,
package-manifest, install-preview, and package-verification evidence. Gate 122
does not add a new schema gate. It proves that the existing SARIF, GitHub
workflow-command annotation, and Markdown diagnostic summary projections carry
schema-gated `WF-BUILD-006` package-verification diagnostics correctly.

## Research grounding

- Documented: ADR-010 keeps the command surface stable and routes existing
  package evidence checks through `forge package --target mcm-json
  --verify-existing`.
- Documented: ADR-011 identifies canonical issue JSON, SARIF diagnostic
  exchange, Markdown summaries, and deterministic fixture-backed tests.
- Documented: Gate 89 added SARIF and GitHub output for package
  verify-existing diagnostics.
- Documented: Gate 90 added Markdown summary output for package
  verify-existing diagnostics.
- Inferred: Schema-gated package verification failures should flow through
  the same canonical `DiagnosticReport` projections as other verify-existing
  package diagnostics.

## Implemented

Gate 122 implements:

- a shared golden-test helper that makes temp-generated
  `package-verification.json` schema-invalid and refreshes local
  build/checksum evidence,
- SARIF coverage for a package-verification schema failure,
- GitHub workflow-command annotation coverage for a package-verification
  schema failure,
- Markdown diagnostic summary coverage for a package-verification schema
  failure.

## Not implemented

Gate 122 does not implement:

- new diagnostic formats,
- new command aliases,
- new schema IDs,
- changed verifier behavior,
- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| SARIF schema diagnostic coverage | Complete | Golden test asserts `WF-BUILD-006`, title, command, URI, and message. |
| GitHub annotation schema diagnostic coverage | Complete | Golden test asserts file, title, message, and suggested fix. |
| Markdown summary schema diagnostic coverage | Complete | Golden test asserts command, summary count, rule, location, title, and message. |
| Command surface | Complete | No new command or alias was added. |
| Install behavior | Not implemented | Forge still produces and verifies reports only and does not install files. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmJson
dotnet test WastelandForge.sln --no-build --no-restore -m:1
```

## Next gate

Gate 123 should add focused verify-existing diagnostics for missing required
package evidence files.
