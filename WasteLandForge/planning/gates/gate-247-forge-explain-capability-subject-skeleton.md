# Gate 247 - forge explain Capability Subject Skeleton

Status: Complete

## Purpose

Implement the fourth executable top-level `forge explain` subject:

```text
forge explain capability <capability-id>
```

This gate explains built-in capability catalogue metadata deterministically. It
does not inspect a project, run capability scans, resolve provider status,
read local provider evidence, execute runtime probes, or call external tools.

## Research grounding

- Documented: R005 and ADR-008 say projects depend on capabilities, providers
  satisfy capabilities, install scope is first-class, and detection must be
  local-first and deterministic.
- Documented: R006 and ADR-010 define `forge explain` as a canonical
  offline-first CLI command for diagnostic IDs, targets, output paths,
  capability IDs, provenance, and reasoned explanations.
- Documented: ADR-011 requires deterministic fixture-backed tests,
  offline-first behavior, and no required AI.
- Documented: Gate 246 completed deterministic output path classification and
  selected `forge explain capability <capability-id>` as the next subject
  skeleton.
- Inferred: Top-level capability explanation should use built-in catalogue
  metadata only because evidence-aware status already belongs to
  `forge capabilities explain`.

## Implemented

Gate 247 implements:

- `forge explain capability <capability-id>` parsing,
- deterministic built-in FNV capability catalogue lookup,
- plain/human output with capability title, description, catalogue ID/version,
  satisfying providers, provider type, install scope, detector kinds, provider
  version declaration metadata, provider notes, related `WF-CAP-*` rules,
  recovery commands, and boundaries,
- JSON output with `catalog`, `capability`, `providers`, `relatedRules`,
  `recoveryCommands`, and explicit false execution flags,
- usage JSON for unknown capability IDs,
- help text that marks `diagnostic`, `target`, `output`, and `capability`
  implemented while leaving `provenance` reserved.

## Not implemented

Gate 247 does not implement:

- provider ID subject support,
- project file reads,
- generated manifest reads,
- generated artifact existence checks,
- provenance sidecar reads,
- local provider evidence reads,
- capability scan behavior changes,
- provider resolution,
- runtime probes,
- build planning changes,
- generator execution,
- package execution,
- release execution,
- graph visualization formats,
- provenance subject execution,
- xEdit process execution,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Capability subject executes | Complete | `forge explain capability runtime.ui.mcm_json` returns exit code 0 and deterministic catalogue metadata. |
| JSON capability metadata emitted | Complete | `forge explain capability runtime.scripting.xnvse --format json` returns catalogue/capability/provider metadata and false execution flags. |
| Unknown capability IDs rejected | Complete | Unknown capability IDs return usage exit code 2. |
| Remaining explain subject stays reserved | Complete | `provenance` still uses reserved status metadata. |
| Runtime mutation avoided | Complete | No project files, generated manifests, provenance sidecars, artifacts, provider evidence, external tools, runtime probes, or AI calls are used. |

## Validation

Gate 247 requires build, targeted explain golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 248 should implement a planning skeleton for the final top-level
`forge explain` subject:
`forge explain provenance <manifest-or-output-path>`. It should define
deterministic provenance subject boundaries and stop before build manifest
reads, provenance sidecar reads, generated artifact existence checks, build
planning changes, generator execution, package execution, release execution,
provider resolution, capability scan behavior changes, graph visualization
formats, external tool execution, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
