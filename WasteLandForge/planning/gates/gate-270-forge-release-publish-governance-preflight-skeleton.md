# Gate 270 - forge release publish Governance Preflight Skeleton

Status: Complete
Date: 2026-07-04

## Goal

Start the guarded `forge release publish` lane with a deterministic
no-publish governance preflight. The command must report required local
evidence, governance checks, and explicit human approval requirements without
publishing a release.

## Research grounding

- Documented: R006 and ADR-010 define `forge release verify|prepare|publish`
  as the canonical release command surface and state that `release publish`
  waits until release governance is locked down.
- Documented: R008 and ADR-011 require layered validation, mandatory local
  build manifests, checksums, governance checks, offline-first correctness,
  optional AI, and explicit governance before release publication.
- Documented: `docs/governance/release-policy.md` requires schema validation,
  semantic validation, capability/environment validation, package validation,
  release verification, local build manifest, checksums, governance checks,
  and explicit human approval before release publish exists.
- Inferred: The first safe `release publish` implementation slice should be a
  preflight/refusal report, not a real publication path.

## Implemented behavior

Gate 270 implements:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

Normal execution:

- returns exit code 6,
- reports `status: refused`,
- reports `publishReady: false`,
- lists required local evidence,
- lists governance checks,
- reports missing explicit human approval,
- reports false execution flags for publish, remote calls, uploads, signing,
  external tools, plugin mutation, MO2/GECK automation, runtime probes, and AI.

Dry-run execution:

- returns exit code 0,
- reports `status: planned`,
- reports the same no-publish preflight,
- writes no files.

## Required evidence reported

- schema validation,
- semantic validation,
- capability/environment validation,
- package validation,
- release verification,
- release-prepare `build-manifest.json`,
- release-prepare `checksums.sha256`,
- release archive evidence,
- governance checks.

## Governance checks reported

- immutable schema policy,
- SemVer-governed version stream,
- least-privilege release permissions,
- CODEOWNERS/sensitive-path review,
- redistributable fixture policy,
- AI-optional release correctness path.

## Explicit non-goals

Gate 270 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- release artifact reads,
- evidence verification,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Acceptance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Command callable | Complete | `forge release publish` routes to the new preflight path instead of reserved-command status. |
| Normal publish refused | Complete | Normal JSON output reports `status: refused`, `publishReady: false`, and exit code 6. |
| Dry-run preflight available | Complete | `--dry-run --format json` reports `status: planned` and exit code 0. |
| Governance/evidence requirements reported | Complete | JSON and text output list required evidence, governance checks, and approval requirements. |
| No publish side effects | Complete | Execution flags remain false for publishing, remote calls, uploads, signing, filesystem mutation, external tools, runtime probes, and AI. |
| CLI help updated | Complete | `forge help release publish` describes Gate 270 usage and boundaries. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
```

## Next gate

Gate 271 should add `forge release publish` local evidence discovery. It
should inspect expected release-prepare evidence paths and report present,
missing, and not-yet-validated artifacts while remaining no-publish by
default. It must still stop before remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
