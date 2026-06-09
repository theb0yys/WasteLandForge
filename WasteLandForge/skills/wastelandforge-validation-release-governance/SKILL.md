---
name: wastelandforge-validation-release-governance
description: Use this skill for WastelandForge validation, testing, CI, release, governance, repository protection, rule families, fixtures, SARIF, TRX, build manifests, checksums, schema immutability, SemVer streams, SECURITY.md, CODEOWNERS, GitHub rulesets, and ADR-011. Use it whenever the user mentions R008, validation stack, tests, CI, GitHub Actions, release dry-runs, governance, or implementation readiness.
---

# WastelandForge Validation Release Governance

## Research base

Use:

- `WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`.
- `R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md`.
- `R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`.
- `WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`.
- `R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`.

## ADR-011 decision

WastelandForge uses layered validation, deterministic fixture-backed testing, GitHub Actions CI centered on Windows and .NET, immutable published schemas, SemVer-governed version streams, mandatory local build manifests, optional SLSA-style release provenance, least-privilege repository governance, and contribution rules that do not require AI.

## Validation stack

Use the R008 stack:

```text
load / source validation
  -> schema validation
  -> semantic validation
  -> capability and environment validation
  -> generation planning
  -> output validation
  -> package validation
  -> release validation
  -> build manifest and reports
```

Schema validation is not enough. Semantic, capability, output, package, release, governance, and security validators are first-class parts of the operating model.

## Reserved rule families

Use these prefixes:

```text
WF-LOAD-*     file discovery, parsing, encoding
WF-SCHEMA-*   schema and contract shape
WF-SEM-*      semantic and cross-registry rules
WF-CAP-*      capability/provider rules
WF-ASSET-*    asset/path rules
WF-GEN-*      generator rules
WF-BUILD-*    build graph/cache rules
WF-REL-*      release rules
WF-GOV-*      governance rules
WF-SEC-*      security/policy rules
```

Do not continue older `WF-MAN-*` or `WF-DEPS-*` as primary families unless mapping legacy diagnostics forward. Manifest and dependency issues should be categorized under `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, or `WF-REL-*` as appropriate.

## Diagnostic outputs

The canonical diagnostic model is JSON. Map it outward to:

- human console,
- plain text,
- SARIF 2.1.0,
- Markdown summaries,
- GitHub annotations.

SARIF is generated locally and uploaded to GitHub only as an optional publishing surface. GitHub code scanning is not required for correctness.

## Test strategy

Use:

- unit tests,
- schema tests,
- semantic tests,
- golden output tests,
- fixture tests,
- Windows path/filesystem tests,
- backwards-compatibility tests,
- release dry-run tests.

JSON is canonical for issue data, SARIF is the diagnostics exchange format, TRX is the native .NET test record, and JUnit is only a derived compatibility artifact where needed.

## Fixture policy

Public fixtures must be synthetic and redistributable:

- synthetic YAML/JSON registries,
- synthetic asset paths,
- tiny handcrafted binary fixtures or record fragments with no shipped game content,
- golden docs,
- SARIF examples,
- build manifests,
- failure cases.

Private extended fixtures can use user-supplied real installs or mods, but they must stay out of public CI and public repositories unless explicit permissions exist.

## CI baseline

Use GitHub Actions as the default CI substrate.

Baseline:

- mandatory GitHub-hosted Windows lane,
- Ubuntu lane for fast validation,
- optional macOS later,
- self-hosted Windows only for later heavyweight integration tests,
- SARIF output generated locally,
- TRX-native .NET test output,
- release dry-run,
- `build-manifest.json`,
- checksums,
- optional attestations later.

Production workflows should use least-privilege permissions, explicit SDK versions via `global.json`, `actions/setup-dotnet@v5` or current official equivalent, `actions/upload-artifact@v4` or current official equivalent, and full commit SHA pinning for third-party actions.

## Governance

Prefer GitHub rulesets over only classic branch protection. Use:

- protected `main` and `release/*`,
- pull requests required,
- required Windows CI status check,
- required schema/validation status checks,
- CODEOWNERS review for sensitive paths,
- SECURITY.md,
- Dependabot for NuGet and GitHub Actions,
- least-privilege `GITHUB_TOKEN` permissions,
- no secrets in public fixtures,
- no AI requirement for contribution, build, validation, or release.

## Versioning and provenance

Use separate SemVer streams for:

- CLI/core packages,
- schemas,
- provider catalogue,
- generators,
- rule packs.

Published schema URLs are immutable. Local schema caches keep validation offline-first.

Every build writes a mandatory local `build-manifest.json`. CI release attestations and SLSA-style provenance are optional additions, not replacements.

## .NET target guidance

R008 identifies the .NET target as an open implementation choice. As of June 9, 2026, .NET 8 and .NET 9 both end support on November 10, 2026, while .NET 10 LTS is active until November 14, 2028.

For implementation planning, use .NET 10 LTS unless a required dependency blocks it. If a dependency blocks .NET 10, record the blocker and fallback explicitly.
