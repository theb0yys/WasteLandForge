# Gate 123 - MCM Extender Verify-Existing Missing Evidence Diagnostics

Status: Complete

## Purpose

Gate 123 makes missing required package evidence files explicit in
`forge package --target mcm-json --verify-existing`.

Earlier verify-existing gates already revalidated generated MCM package
evidence content, schemas, checksums, manifests, and diagnostic projections.
Gate 123 tightens the first file-read boundary so absent required JSON or
Markdown evidence files report clear blocking `WF-BUILD-006`
missing-evidence diagnostics at the expected file path instead of relying on
generic read-failure wording.

## Research grounding

- Documented: ADR-010 keeps existing package evidence verification under
  `forge package --target mcm-json --verify-existing` and forbids new aliases.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed tests, and canonical diagnostics that can be projected through
  CLI output formats.
- Inferred: Missing required package evidence is a package validation failure
  and should continue to use the existing `WF-BUILD-006` package evidence rule.

## Implemented

Gate 123 implements:

- explicit missing-file checks in the package evidence JSON reader,
- explicit missing-file checks in the package evidence Markdown/text reader,
- unit coverage for missing `package-verification.json`,
- unit coverage for missing `package-verification.md`,
- golden CLI JSON coverage for missing existing `package-verification.json`,
- documentation and slash-command routing updates for the new diagnostic
  behavior.

## Not implemented

Gate 123 does not implement:

- new command aliases,
- new diagnostic rule IDs,
- new schema IDs,
- changed package generation behavior,
- install behavior for Data or MO2,
- malformed JSON or malformed Markdown-specific diagnostic wording beyond
  existing read/schema/content validation paths,
- FOMOD generation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Missing JSON evidence diagnostic | Complete | Missing package-verification JSON reports `MCM package package verification evidence is missing`. |
| Missing Markdown evidence diagnostic | Complete | Missing package-verification summary reports `MCM package package verification summary evidence is missing`. |
| CLI JSON projection | Complete | `forge package --target mcm-json --verify-existing --format json` returns exit code `1` and one `WF-BUILD-006` issue for missing package-verification evidence. |
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

Gate 124 should add focused verify-existing diagnostics for malformed evidence
files that cannot be parsed or are the wrong top-level evidence shape.
