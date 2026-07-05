# Gate 294 - forge build reports Build Evidence Closeout

Status: Complete
Date: 2026-07-05

## Goal

Close the local `forge build --target reports` build-evidence lane and route
the next implementation slice toward deterministic package planning/staging
value.

## Research grounding

- Documented: ADR-009 requires deterministic, capability-aware build graph
  outputs with generated artifacts treated as disposable and rebuildable.
- Documented: ADR-010/R006 keep `forge build` and `forge package` in the
  canonical command surface and require deterministic, composable CLI
  behavior without undocumented aliases.
- Documented: ADR-011 requires offline-first validation, deterministic
  fixture-backed tests, local build manifests, and AI-optional correctness.
- Documented: R006 says `package` should arrive as soon as build manifests
  and staging are stable.
- Documented: The generator/build research says `forge package` should
  assemble a distributable staging tree and that early v0.1 value includes
  package metadata, build manifests, provenance, JSON, Markdown reports, and
  deterministic staging before high-risk plugin generation or patching.
- Inferred: Gates 292 and 293 make the reports build target inspectable
  enough for the next low-risk slice to consume that evidence through a
  local reports package plan/staging skeleton.

## Closeout decision

The current `forge build --target reports` evidence lane is complete enough
for local operator use.

The accepted local evidence set is:

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

This lane now has:

- machine-readable validation, dependency, capability, build-plan,
  build-report, and build-report-index evidence,
- human-readable build-plan and build-report-index summaries,
- local build-manifest provenance,
- checksum sidecar coverage,
- help, explain-output, graph metadata, unit, and golden CLI coverage.

## Implemented behavior

Gate 294 is a planning/routing closeout only. It records the current lane
state and changes the next gate route.

It does not change runtime CLI behavior, generated outputs, schemas,
diagnostics, package execution, release execution, or tests.

## Next-value route

Gate 295 should start `forge package --target reports` with a package-plan
and staging skeleton.

The first package slice should:

- stay under `dist/`,
- consume or plan around the existing reports build evidence set,
- write local package planning/staging evidence only,
- avoid release uploads and repository calls,
- avoid attestation/signing,
- avoid external tool execution,
- avoid plugin mutation,
- avoid MO2 or GECK automation,
- avoid runtime probes,
- avoid real third-party plugin fixtures,
- avoid AI behavior.

## Explicit non-goals

Gate 294 does not implement:

- `forge package --target reports`,
- package archive creation,
- package manifest or install-plan schemas for reports,
- generated artifact existence checks,
- generated manifest reads,
- build-manifest digest revalidation,
- checksum digest revalidation,
- package validation,
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
| Reports build evidence lane is closed | Complete | This gate records the accepted `dist/build` evidence set. |
| Next route leaves build-report edge cases | Complete | Gate 295 is routed to `forge package --target reports`. |
| Canonical command surface is preserved | Complete | No new command alias or non-ADR-010 verb is introduced. |
| Runtime behavior is unchanged | Complete | No CLI code, schemas, diagnostics, package execution, or release execution are changed. |
| External side effects remain disabled | Complete | No external tools, runtime probes, publishing, repository calls, plugin mutation, MO2/GECK automation, or AI are added. |

## Validation

Required validation:

```text
dotnet test WastelandForge.sln --no-restore
git diff --check
```

## Next gate

Gate 295 should add a local `forge package --target reports` package-plan
and staging skeleton. It should stay local-only and still stop before release
uploads, attestation/signing, external tool execution, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin
fixtures, or AI behavior.
