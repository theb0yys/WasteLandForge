# Gate 293 - forge build reports Build Plan Index Markdown

Status: Complete
Date: 2026-07-05

## Goal

Add human-readable Markdown summaries beside the Gate 292 build-plan and
build-report-index JSON evidence for `forge build --target reports`.

## Research grounding

- Documented: ADR-009 requires deterministic, capability-aware build graph
  outputs with generated artifacts treated as disposable and rebuildable.
- Documented: ADR-010/R006 keep `forge build` in the canonical command
  surface and require deterministic, composable CLI behavior.
- Documented: ADR-011 requires offline-first validation, deterministic
  fixture-backed tests, local build manifests, and AI-optional correctness.
- Documented: The generator/build research prioritizes deterministic text and
  metadata artifacts such as capability reports, dependency reports, build
  manifests, provenance, package metadata, JSON, and Markdown reports before
  high-risk plugin generation or patching.
- Inferred: After Gate 292 added machine-readable build-plan and report-index
  JSON, the next useful local operator slice is Markdown companion evidence
  that is still recorded by the same manifest and checksum path.

## Implemented behavior

`forge build --target reports` now writes two additional local Markdown
evidence files under the selected `dist/` output root:

```text
dist/build/build-plan.md
dist/build/build-report-index.md
```

`build-plan.md` summarizes:

- command, target, project, output root, source count, and capability counts,
- local validation/planning/provenance phases,
- the scheduled `wf.metadata_reports` generator target,
- planned outputs, including JSON and Markdown build evidence,
- false execution flags for external tools, runtime probes, release
  publishing, repository calls, MO2/GECK automation, plugin mutation, and AI.

`build-report-index.md` summarizes the local build evidence index in a table
for human review, including the Markdown build-plan and report-index summary
files.

The new files are included in:

- CLI JSON output paths,
- human/plain output,
- `build-manifest.json` output digests,
- `checksums.sha256`,
- `forge help build` documented outputs,
- `forge explain output dist/build/build-plan.md`,
- `forge explain output dist/build/build-report-index.md`,
- `forge graph` reports-target artifact expectations and manifest
  provenance edges.

`forge generate --target reports` is unchanged.

## Explicit non-goals

Gate 293 does not implement:

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
| Build-plan Markdown is emitted | Complete | `forge build --target reports` writes `dist/build/build-plan.md`. |
| Report-index Markdown is emitted | Complete | `forge build --target reports` writes `dist/build/build-report-index.md`. |
| Provenance includes new files | Complete | Build manifest output digests and checksum sidecar include both Markdown files. |
| CLI output exposes new paths | Complete | JSON and human/plain output list both Markdown files. |
| Metadata catalogues stay aligned | Complete | Help, explain output, and graph artifact expectations include the new paths. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

## Validation

Required validation:

```text
dotnet build src\WastelandForge.Cli\WastelandForge.Cli.csproj --no-restore
dotnet test tests\WastelandForge.UnitTests\WastelandForge.UnitTests.csproj --no-restore --filter FullyQualifiedName~MetadataReportGeneratorTests
dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-restore --filter "BuildJsonWritesBuildManifestAndChecksums|ProjectSourceGraph|ExplainOutput"
dotnet test WastelandForge.sln --no-restore
git diff --check
```

## Next gate

Gate 294 should close the local `forge build --target reports` build-evidence
lane and route the next implementation slice toward broader build or package
value. It should stay local-only and still stop before remote repository
calls, release uploads, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
