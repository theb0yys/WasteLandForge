# Gate 297 - forge package reports Staging Copy

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge package --target reports` so present local reports build
evidence is copied into package staging while missing expected inputs remain
classified and skipped.

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
- Inferred: After Gate 296 records expected input existence, the next safe
  slice is copying only expected present reports evidence into package staging.
  Archive assembly and digest revalidation remain separate gates.

## Implemented behavior

`forge package --target reports` now copies each present expected Gate 294
input from `dist/build` into `dist/reports-package/staging/reports`:

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

Each package entry now includes source classification and staging
classification:

```text
sourceExists: true|false
inputStatus: present|missing
staged: true|false
stageStatus: staged|planned-copy|not-staged-missing-input
```

Non-dry-run package evidence now records `stagedInputs`, `unstagedInputs`, and
staging summary counts. Copied staged payloads are included in the package
`build-manifest.json` output digests and `checksums.sha256`. Input discovery
metadata marks copied runs as `contentRead: true` with `contentPurpose:
copy-only` and `contentValidation: false`. Missing expected inputs remain
visible and skipped.

The command still writes only package evidence under `dist/reports-package`
and does not write to game `Data`, MO2 profiles, GECK, or any external tool.

## Explicit non-goals

Gate 297 does not implement:

- parsing existing build manifests,
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
| Present inputs are copied | Complete | Runs after `forge build --target reports` copy expected files into `dist/reports-package/staging/reports`. |
| Missing inputs are skipped | Complete | Clean fixture package runs report zero staged inputs and ten skipped missing inputs. |
| Staging state is visible | Complete | Entries report `staged` and `stageStatus`; summaries report staged/unstaged counts. |
| Content-read purpose is explicit | Complete | Staged package runs report copy-only content reads with content validation disabled. |
| Staged payloads are fingerprinted | Complete | Package build manifest and checksum sidecar include copied staged report files. |
| Archive behavior remains absent | Complete | No `package.zip` is created. |
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

Gate 298 should create a deterministic local reports package archive from the
staged `reports/*` payload and package evidence, while still stopping before
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
