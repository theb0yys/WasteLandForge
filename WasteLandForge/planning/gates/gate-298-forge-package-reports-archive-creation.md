# Gate 298 - forge package reports Archive Creation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge package --target reports` so package runs create a deterministic
local `dist/reports-package/package.zip` archive from staged report payloads
and package plan/layout evidence.

## Research grounding

- Documented: ADR-009 requires deterministic build graph outputs, disposable
  generated/dist artifacts, provenance, and explicit generated/dist
  containment.
- Documented: ADR-010/R006 keep `forge package` in the canonical CLI command
  surface and require deterministic, composable behavior without aliases.
- Documented: ADR-011 requires deterministic fixture-backed tests, local build
  manifests, and AI-optional correctness.
- Documented: Gate 267 established the local release archive pattern: sorted
  ZIP entries, stored compression, deterministic ZIP-compatible timestamps,
  manifest/checksum coverage, and no publishing.
- Inferred: After Gate 297 copies present reports evidence into staging, the
  next safe package slice is deterministic archive creation before archive
  evidence revalidation.

## Implemented behavior

`forge package --target reports` now writes:

```text
dist/reports-package/package-plan.json
dist/reports-package/staging/package-layout.json
dist/reports-package/package.zip
dist/reports-package/build-manifest.json
dist/reports-package/checksums.sha256
```

The archive is created with:

- ordinal sorted entries,
- stored ZIP compression,
- deterministic ZIP-compatible timestamps,
- `package-plan.json`,
- `package-layout.json`,
- copied staged report payload entries under `reports/*` when present.

The package build manifest and checksum sidecar include `package.zip`. The
archive itself intentionally does not contain `build-manifest.json` or
`checksums.sha256`, so those sidecars can cover the archive digest without a
circular dependency.

## Explicit non-goals

Gate 298 does not implement:

- archive evidence sidecar emission,
- archive digest revalidation,
- archive entry revalidation,
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
| Archive is emitted | Complete | Normal package runs write `dist/reports-package/package.zip`. |
| Archive entries are deterministic | Complete | Unit tests assert sorted entry names and the deterministic ZIP timestamp. |
| Evidence-only runs work | Complete | Clean fixture package runs create an archive with package plan/layout evidence only. |
| Staged payload runs work | Complete | Runs after `forge build --target reports` include `reports/*` payload entries. |
| Archive is fingerprinted | Complete | Package build manifest and checksum sidecar include `package.zip`. |
| Revalidation remains deferred | Complete | Archive digest/entry revalidation remains out of scope for this gate. |
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

Gate 299 should add local reports package archive evidence revalidation for
archive digest, entry names, entry ordering, and deterministic timestamp
metadata, while still stopping before package verify-existing behavior,
release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
