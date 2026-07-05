# Gate 295 - forge package reports Package Plan Staging Skeleton

Status: Complete
Date: 2026-07-05

## Goal

Add the first local `forge package --target reports` implementation slice.
This gate turns the accepted `dist/build` reports evidence set from Gate 294
into deterministic package planning and staging-layout evidence without yet
copying files or creating an archive.

## Research grounding

- Documented: ADR-009 requires deterministic, capability-aware build graph
  outputs, disposable rebuildable artifacts, provenance, and generated/dist
  containment.
- Documented: ADR-010/R006 include `forge package` in the canonical CLI
  command surface and require command behavior to avoid undocumented aliases.
- Documented: ADR-011 requires layered validation, deterministic
  fixture-backed tests, local build manifests, and AI-optional correctness.
- Documented: The generator/build research says `forge package` should
  assemble a distributable staging tree and that early v0.1 value includes
  package metadata, build manifests, provenance, JSON/Markdown reports, and
  deterministic staging before high-risk plugin generation or patching.
- Inferred: Because Gate 294 closed the reports build-evidence lane, the
  first package slice can safely plan around that evidence set while leaving
  existence checks, copying, digest revalidation, and archive assembly to
  later gates.

## Implemented behavior

`forge package --target reports` now validates the project and writes:

```text
dist/reports-package/package-plan.json
dist/reports-package/staging/package-layout.json
dist/reports-package/build-manifest.json
dist/reports-package/checksums.sha256
```

The package plan records the expected Gate 294 input set:

```text
dist/build/validation.json
dist/build/dependency-report.json
dist/build/capability-report.json
dist/build/build-plan.json
dist/build/build-plan.md
dist/build/build-report.json
dist/build/build-report-index.json
dist/build/build-report-index.md
dist/build/build-manifest.json
dist/build/checksums.sha256
```

The staging layout records planned package paths under `reports/` and planned
staging paths under `dist/reports-package/staging/reports/`.

The command supports human/plain and JSON output. `--dry-run` reports the same
planned outputs without writing files. Custom `--output` values must resolve
under project `dist/` and reuse `WF-BUILD-001` for containment failures.

## Explicit non-goals

Gate 295 does not implement:

- package input existence checks,
- copying existing `dist/build` evidence into staging,
- reading existing build manifests,
- checksum digest revalidation,
- package archive creation,
- package verify-existing behavior for `reports`,
- release uploads,
- remote repository calls,
- attestation or signing,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Canonical command surface is preserved | Complete | `reports` is wired through existing `forge package --target <id>`. |
| Package evidence is local and deterministic | Complete | Outputs are under `dist/reports-package` with a local build manifest and checksum sidecar. |
| Gate 294 input set is represented | Complete | `package-plan.json` and `package-layout.json` list the accepted `dist/build` evidence paths. |
| Dry-run remains no-write | Complete | `ReportsPackageEmitterTests.PackageDryRunPlansWithoutWriting`. |
| Dist containment is enforced | Complete | `ReportsPackageEmitterTests.PackageRejectsOutputOutsideDist`. |
| External side effects remain disabled | Complete | Package policy and execution flags are false; no external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-restore --filter FullyQualifiedName~ReportsPackageEmitterTests
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter PackageReportsWritesPackagePlanAndStagingSkeleton
dotnet test WastelandForge.sln --no-restore --logger "console;verbosity=minimal"
git diff --check
```

## Next gate

Gate 296 should add reports package input discovery and missing-input
classification. It should read the expected local `dist/build` evidence paths
and classify each planned input as present or missing, but still stop before
file copying, archive creation, release uploads, attestation/signing, external
tool execution, plugin mutation, MO2 automation, GECK automation, runtime
probes, real third-party plugin fixtures, or AI behavior.
