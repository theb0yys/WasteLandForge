# Gate 281 - forge release publish Governance Check Evaluation

Status: Complete
Date: 2026-07-05

## Goal

Extend `forge release publish` preflight so it evaluates local governance
evidence while remaining no-publish by default.

## Research grounding

- Documented: ADR-010/R006 keep `forge release publish` in the canonical
  release command surface.
- Documented: ADR-011 requires layered validation, immutable published
  schemas, SemVer-governed version streams, local build manifests, checksums,
  governance checks, least-privilege repository governance, contribution rules
  that do not require AI, and explicit human approval before release
  publication.
- Documented: Gate 280 validates local release-prepare evidence semantics and
  stops before governance-check evaluation, archive payload validation, or real
  publish behavior.
- Inferred: The next safe release-publish slice is local governance-check
  evaluation, before accepting publish, calling remote repositories, uploading
  releases, or signing/attesting artifacts.

## Implemented behavior

Gate 281 keeps the existing command surface:

```text
forge release publish [project-root] [--project <path>]
  [--format human|plain|json] [--dry-run] [--no-input]
```

The command now evaluates six local governance checks:

- `immutable-schema-policy`,
- `semver-version-stream`,
- `least-privilege-release-permissions`,
- `codeowner-sensitive-path-review`,
- `redistributable-fixture-policy`,
- `ai-optional-release-path`.

Each governance check reports:

- `status`,
- `checkedInCurrentGate`,
- `evidencePath`,
- `detail`.

The aggregate `requiredEvidence[governance-checks]` status is:

- `complete-governance-evaluated` when every governance check passes,
- `incomplete-governance-evaluated` when local governance evidence is missing,
- `failed-governance-evaluated` when local governance evidence exists but does
  not satisfy the check.

`execution.governanceCheckExecution` is now `true`.

## Evaluation rules

Gate 281 evaluates local files only:

- immutable schema policy: `docs/governance/schema-version-policy.md` must
  mention schema immutability and `$id`,
- SemVer stream: the Forge tool version must have `major.minor.patch` shape,
- least-privilege workflow permissions: local `.github/workflows/*.yml` or
  `.yaml` files must declare `permissions:` without broad `write-all` or
  `contents: write`,
- CODEOWNERS review: `.github/CODEOWNERS` or `CODEOWNERS` must cover
  governance docs, GitHub workflow paths, and schemas,
- redistributable fixture policy: `docs/governance/fixture-policy.md` must
  mention synthetic and redistributable fixtures,
- AI-optional release path: `AGENTS.md` or governance policy files must state
  that AI is optional or not required for release correctness.

## Explicit non-goals

Gate 281 does not implement:

- release publication,
- remote repository calls,
- release uploads,
- attestation or signing,
- human approval acceptance,
- schema-validation evidence acceptance,
- capability/environment evidence acceptance,
- package-validation evidence acceptance,
- release-verification evidence acceptance,
- archive payload content validation,
- FOMOD installer assembly,
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
| Missing local governance evidence is evaluated | Complete | Bare temp projects report `incomplete-governance-evaluated` and per-check `missing-local-evidence` where policy files are absent. |
| Passing local governance evidence is evaluated | Complete | Temp-only synthetic governance files satisfy all six checks and report `complete-governance-evaluated`. |
| Governance execution is explicit | Complete | JSON reports `execution.governanceCheckExecution: true`. |
| Non-goals remain disabled | Complete | Publish, remote, upload, signing, tool, runtime, and AI execution flags remain false. |
| CLI help updated | Complete | `forge help release publish` describes Gate 281 governance-check evaluation. |

## Validation

Required validation:

```text
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-restore
dotnet test WastelandForge.sln --no-restore
```

## Next gate

Gate 282 should move to release-publish schema-validation evidence evaluation.
It must still stop before remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
