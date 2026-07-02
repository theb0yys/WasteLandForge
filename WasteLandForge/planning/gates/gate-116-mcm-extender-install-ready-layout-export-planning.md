# Gate 116 - MCM Extender Install-Ready Layout And Export Planning

Status: Complete

## Purpose

Gate 116 adds install-ready export planning evidence to the MCM Extender JSON
slice. The goal is to make the package tree easier to hand to a human or later
installer adapter without turning Forge into an installer in this gate.

## Research grounding

- Documented: ADR-009 requires generated outputs to remain disposable,
  deterministic, and traceable through local manifests.
- Documented: ADR-010 keeps `forge generate`, `forge build`, and
  `forge package` on the canonical command surface without ad hoc aliases.
- Documented: ADR-011 requires layered validation, local build manifests,
  checksums, and deterministic fixture-backed tests.
- Documented: R006/R008 keep the correctness path offline-first and
  AI-optional.
- Inferred: An install-ready plan can be generated as local evidence because
  earlier gates already generate `install-preview`, package manifests,
  package verification, manifests, and checksums without mutating Data or MO2.

## Implemented

Gate 116 implements:

- `schemas/install-plan/0.1.0/schema.json`,
- built-in schema catalog and schema ID entries for `install-plan/0.1.0`,
- generated `install-plan.json` for `forge generate|build|package --target mcm-json`,
- generated `install-plan.md` human summary evidence,
- CLI JSON/text output paths for install-plan evidence,
- build/generation manifest `installPlan` evidence,
- output digest and distribution checksum coverage for install-plan files,
- verify-existing output path surfacing for install-plan files,
- build-manifest and checksum expected-entry coverage for install-plan files.

## Not implemented

Gate 116 does not implement:

- copying files into a game `Data` folder,
- creating or mutating an MO2 mod/profile,
- launching the game,
- inspecting MO2 VFS conflicts,
- FOMOD generation,
- install-plan content revalidation in `--verify-existing` beyond manifest and
  checksum coverage.

## Output shape

`install-plan.json` records:

```text
kind: wastelandforge.install-plan
planType: wastelandforge/mcm-json-loose-file-install-plan/v1
package.mode: export-plan
package.requiresManualApproval: true
package.writesToGameData: false
package.writesToMo2Profile: false
package.launchesGame: false
entries[*].action: copy-loose-file-if-user-approved
```

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Install-plan schema | Complete | `install-plan/0.1.0` is embedded and catalogued. |
| Generator output | Complete | Generate, build, and package write JSON/Markdown install-plan evidence. |
| Manifest/checksum coverage | Complete | Install-plan files are recorded in local manifests and distribution checksums. |
| CLI output paths | Complete | JSON and text outputs surface install-plan paths. |
| Verify-existing coverage | Partial | Existing verifier expects install-plan files in checksums/build manifests; full content revalidation is deferred. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj --no-build --no-restore --filter FullyQualifiedName~ManifestSchemaTests
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter FullyQualifiedName~MetadataReportGeneratorTests
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmPackageVerificationEvidence
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~McmJson
```

## Next gate

Gate 117 should add install-plan verify-existing content revalidation.
