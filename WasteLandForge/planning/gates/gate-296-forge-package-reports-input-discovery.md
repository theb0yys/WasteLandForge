# Gate 296 - forge package reports Input Discovery

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge package --target reports` so the package plan and staging-layout
evidence classify the expected local `dist/build` reports inputs as present or
missing before any package copying or archive creation happens.

## Research grounding

- Documented: ADR-009 requires deterministic build graph outputs, disposable
  generated/dist artifacts, provenance, and explicit generated/dist
  containment.
- Documented: ADR-010/R006 keep `forge package` in the canonical CLI command
  surface and require deterministic, composable behavior without aliases.
- Documented: ADR-011 requires deterministic fixture-backed tests, local build
  manifests, and AI-optional correctness.
- Documented: The generator/build research says package should follow build,
  package staging should be careful, and v0.1 value should focus on
  deterministic text/metadata artifacts before higher-risk plugin work.
- Inferred: After Gate 295 records the expected reports package input set, the
  next safe slice is existence-only input discovery. Copying, digest
  revalidation, and archive assembly remain separate gates.

## Implemented behavior

`forge package --target reports` now records input discovery for each expected
Gate 294 input:

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

Each package entry now includes:

```text
sourceExists: true|false
inputStatus: present-not-copied|missing
```

The package plan, staging layout, build manifest, and CLI JSON summary now
include present/missing input counts. `inputExistenceChecks` is now `true`.

The command still writes only:

```text
dist/reports-package/package-plan.json
dist/reports-package/staging/package-layout.json
dist/reports-package/build-manifest.json
dist/reports-package/checksums.sha256
```

## Explicit non-goals

Gate 296 does not implement:

- copying existing `dist/build` evidence into staging,
- reading or parsing existing build manifests,
- reading package input file contents,
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
| Input existence is classified | Complete | Entries report `sourceExists` and `inputStatus`. |
| Missing inputs are visible | Complete | Clean fixture package runs report `missingInputs: 10`. |
| Present inputs are visible | Complete | Package runs after `forge build --target reports` report `presentInputs: 10`. |
| No package input content is read | Complete | Input discovery records presence only; content read remains false. |
| No package files are copied | Complete | Staged `reports/*` payload files are not written. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

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

Gate 297 should copy present reports build evidence from `dist/build` into
`dist/reports-package/staging/reports`. It should copy only present expected
inputs, keep missing inputs classified, and still stop before archive
creation, build-manifest digest revalidation, checksum digest revalidation,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
