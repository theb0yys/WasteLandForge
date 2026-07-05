# Gate 299 - forge package reports Archive Evidence Revalidation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge package --target reports` so package runs reopen the created
reports package archive and emit local archive evidence under:

```text
dist/reports-package/package-archive-evidence.json
```

## Research grounding

- Documented: ADR-009 requires deterministic build graph outputs, disposable
  generated/dist artifacts, provenance, and explicit generated/dist
  containment.
- Documented: ADR-010/R006 keep `forge package` in the canonical CLI command
  surface and require deterministic, composable behavior without aliases.
- Documented: ADR-011 requires deterministic fixture-backed tests, local build
  manifests, and AI-optional correctness.
- Documented: Gate 268 established the local release archive evidence pattern:
  reopen the deterministic ZIP and record digest, entry-name, compression, and
  timestamp evidence before any publish behavior.
- Inferred: After Gate 298 creates `dist/reports-package/package.zip`, the
  next safe reports package slice is local archive evidence revalidation before
  any `reports` verify-existing command behavior.

## Implemented behavior

`forge package --target reports` now writes:

```text
dist/reports-package/package-plan.json
dist/reports-package/staging/package-layout.json
dist/reports-package/package.zip
dist/reports-package/package-archive-evidence.json
dist/reports-package/build-manifest.json
dist/reports-package/checksums.sha256
```

The archive evidence sidecar records:

- archive SHA-256 and byte length,
- expected and actual ZIP entry names,
- ordinal entry-order status,
- stored compression status,
- deterministic ZIP timestamp status,
- per-entry length, compressed length, timestamp, and stored flags,
- execution flags showing archive revalidation happened locally.

The package build manifest and checksum sidecar include
`package-archive-evidence.json`. The sidecar remains outside `package.zip` to
avoid circular archive evidence.

## Explicit non-goals

Gate 299 does not implement:

- package verify-existing behavior for `reports`,
- semantic archive payload validation beyond local entry/digest/timestamp
  evidence,
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
| Archive evidence emitted | Complete | Normal package runs write `dist/reports-package/package-archive-evidence.json`. |
| Archive digest revalidated | Complete | Sidecar records recomputed archive SHA-256 and byte length. |
| Entry names revalidated | Complete | Sidecar records expected and actual ZIP entry names and match status. |
| Entry ordering revalidated | Complete | Sidecar records ordinal ordering status. |
| Timestamp metadata revalidated | Complete | Sidecar records deterministic timestamp status and per-entry timestamps. |
| Stored compression revalidated | Complete | Sidecar records stored compression status. |
| Evidence is fingerprinted | Complete | Package build manifest and checksum sidecar include `package-archive-evidence.json`. |
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

Gate 300 should close the current reports package lane and route the next
value slice, while still stopping before `reports` package verify-existing
behavior, release uploads, attestation/signing, external tool execution,
plugin mutation, MO2 automation, GECK automation, runtime probes, real
third-party plugin fixtures, or AI behavior.
