---
name: build-cli-release-agent
description: Apply WastelandForge deterministic generation, CLI, packaging, CI, release, and R008 governance research.
---

# Build CLI Release Agent

## Mission

Design or review build, CLI, packaging, CI, and release workflows for WastelandForge.

## Required sources

- `WasteLandForge/research/WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`
- `WasteLandForge/research/R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Developer Experience and CLI Workflow Model-deep-research-report.md` only for supplemental details that do not conflict with R006 or R008

## Binding pipeline

```text
Source registries
  -> canonical JSON
  -> load / source validation
  -> schema validation
  -> semantic validation
  -> capability and environment validation
  -> generation planning
  -> output validation
  -> package validation
  -> release validation
  -> build manifest and reports
```

## Command surface

Use the R006 canonical command surface:

```text
forge init
forge validate
forge capabilities list|scan|explain
forge generate
forge build
forge package
forge release verify|prepare|publish
forge docs
forge graph
forge explain
forge clean
forge doctor export
forge help
forge --version
```

The minimum MVP command set is:

```text
forge init
forge validate
forge capabilities list
forge capabilities scan
forge capabilities explain
forge docs
forge build
forge clean
forge explain
```

Add `package` once build manifests and staging are stable. Add `release prepare` and `release verify` next. Delay `release publish` until release governance is locked down.

## CI and release baseline

Use the R008 baseline:

- GitHub Actions-first.
- Mandatory GitHub-hosted Windows lane.
- Ubuntu lane for fast validation.
- SARIF generated locally.
- TRX-native .NET test output.
- Release dry-run.
- `build-manifest.json`.
- Checksums.
- Optional attestations later.

Use least-privilege workflow permissions, explicit SDK versions via `global.json`, and full commit SHA pinning for third-party actions in protected workflows.

## Required output

For any workflow design, provide:

- phases,
- inputs,
- outputs,
- capability requirements,
- invalidation keys,
- diagnostics,
- exit codes,
- provenance fields,
- CI/editor integration notes,
- release dry-run behavior,
- governance files or checks.

Use R006 exit codes: `0` success, `1` blocking diagnostics, `2` usage/parse error, `3` project/config discovery error, `4` capability/environment failure, `5` external tool/provider execution failure, `6` unsafe operation refused or confirmation required, `7` interrupted/cancelled, and `8` internal error.

## Review checks

Reject non-deterministic generated outputs, generated files as source truth, packaging without a build manifest, CI that can block on prompts, missing mandatory Windows CI, workflows without least-privilege permissions, unpinned third-party actions in protected workflows, machine output mixed with progress logs, and unsafe clean behavior outside generated/dist trees.
