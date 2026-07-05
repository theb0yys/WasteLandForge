# Gate 292 - forge build reports Build Plan Index Skeleton

Status: Complete
Date: 2026-07-05

## Goal

Add local build-plan and build-report-index JSON evidence to
`forge build --target reports` without changing the canonical command surface
or executing external tools.

## Research grounding

- Documented: ADR-009 requires deterministic, capability-aware build graph
  outputs with generated artifacts treated as disposable and rebuildable.
- Documented: ADR-010/R006 keep `forge build` in the canonical command
  surface and require deterministic, composable CLI behavior.
- Documented: ADR-011 requires offline-first validation, deterministic
  fixture-backed tests, local build manifests, and AI-optional correctness.
- Documented: The generator/build research says v0.1 should prioritize
  deterministic text and metadata artifacts such as capability reports,
  dependency reports, build manifests, provenance, package metadata, JSON, and
  Markdown reports before high-risk plugin generation or patching.
- Inferred: After Gate 291 routed away from Doctor release-readiness edge
  cases, the next useful build-graph slice is making the existing reports
  build target describe its planned phases, generator target, outputs,
  manifest role, and checksum role as local JSON evidence.

## Implemented behavior

`forge build --target reports` now writes two additional local JSON evidence
files under the selected `dist/` output root:

```text
dist/build/build-plan.json
dist/build/build-report-index.json
```

`build-plan.json` summarizes:

- local build phases,
- the scheduled `wf.metadata_reports` generator target,
- required and optional declared capability counts,
- planned outputs,
- source document count,
- validation status,
- false execution flags for external tools, runtime probes, release
  publishing, repository calls, MO2/GECK automation, plugin mutation, and AI.

`build-report-index.json` indexes the local reports and evidence files:

- `validation.json`,
- `dependency-report.json`,
- `capability-report.json`,
- `build-plan.json`,
- `build-report.json`,
- `build-manifest.json`,
- `checksums.sha256`.

The new files are included in:

- CLI JSON output paths,
- human/plain output,
- `build-manifest.json` output digests,
- `checksums.sha256`,
- `forge help build` documented outputs,
- `forge explain output dist/build/build-plan.json`,
- `forge graph` reports-target artifact expectations and manifest
  provenance edges.

`forge generate --target reports` is unchanged.

## Explicit non-goals

Gate 292 does not implement:

- Markdown build-plan or report-index summaries,
- a new `forge plan` command,
- new build phases or command aliases,
- incremental cache reads,
- generated manifest reads,
- generated artifact existence checks,
- provider resolution beyond declared capability requirements,
- capability scans,
- MCM/JIP/xEdit generator behavior changes,
- package or release behavior changes,
- release publication,
- remote repository calls,
- release uploads,
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
| Build plan JSON is emitted | Complete | `forge build --target reports` writes `dist/build/build-plan.json`. |
| Report index JSON is emitted | Complete | `forge build --target reports` writes `dist/build/build-report-index.json`. |
| Provenance includes new files | Complete | Build manifest output digests and checksum sidecar include both new files. |
| CLI output exposes new paths | Complete | JSON and human/plain output list the build plan and report index. |
| Metadata catalogues stay aligned | Complete | Help, explain output, and graph artifact expectations include the new paths. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-restore --filter FullyQualifiedName~MetadataReportGeneratorTests
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore --filter "BuildJsonWritesBuildManifestAndChecksums|ProjectSourceGraph|ExplainOutput"
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 293 should add Markdown summaries for the Gate 292 build-plan and
build-report-index evidence under `forge build --target reports`. It should
stay local-only and still stop before remote repository calls, release
uploads, attestation/signing, external tool execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
or AI behavior.
