---
name: wastelandforge-capabilities-providers
description: Use this skill for WastelandForge capability detection, provider catalogues, dependency requirements, install scopes, MO2 visibility, runtime probes, version constraints, WF-CAP diagnostics, and ADR-008. It should trigger whenever a task mentions dependencies, tools, providers, xNVSE plugins, MCM, UIO, JIP, GECK Extender, xEdit, MO2, or environment scanning.
---

# WastelandForge Capabilities And Providers

## Research base

Use:

- `R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`.
- `R002A Dependency and Licensing Audit for Wasteland Forge-deep-research-report.md`.
- `WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`.
- `R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`.
- `WastelandForge Validation Testing CI Release and Governance-deep-research-report.md` for `WF-CAP-*` release/CI gating and fixture policy.

## ADR-008 decision

Projects depend on capabilities, not directly on provider names. Providers are versioned registry data that satisfy one or more capabilities. Detection is local-first and deterministic. Runtime checks are secondary confirmation or enrichment. Install scope is first-class.

## ID policy

Capability IDs:

- global,
- lowercase,
- dotted,
- stable,
- semantic at the provider-feature level.

Provider IDs:

- global for built-ins,
- project-namespaced for local additions,
- explicitly typed,
- allowed to carry aliases and compatibility notes.

Use namespaces such as:

```text
game.*
runtime.*
editor.*
tool.*
wf.*
provider.*
```

## MVP catalogue

Model at least:

- `game.falloutnv`
- `runtime.scripting.xnvse`
- `runtime.scripting.jip_ln`
- `runtime.scripting.jip_pp_ln`
- `runtime.scripting.johnnyguitar`
- `runtime.scripting.showoff`
- `runtime.ui.uio`
- `runtime.ui.mcm`
- `runtime.ui.mcm_json`
- `runtime.animation.knvse`
- `editor.geck`
- `editor.geck_extender`
- `editor.hot_reload`
- `tool.xedit`
- `tool.xedit.record_inspection`
- `tool.xedit.cleaning`
- `tool.mo2`
- `tool.mo2.profile`
- `tool.mo2.vfs_launch`

Keep the catalogue as data, not hard-coded if/else logic.

## Detector model

Support detector families for:

- root files,
- Data-managed files,
- runtime commands,
- executable/tool detectors,
- MO2 effective environment detectors,
- manual or declared overrides.

Track physical scope and effective scope. A provider can exist physically but not be visible through the launch context that matters.

Use these scopes unless research updates them:

```text
root
data-managed
mo2-managed
mo2-profile
editor-root
editor-data-managed
external-tool
runtime-session
generated
unknown
```

Use these detection result states:

```text
confirmed
confirmed-in-session
probable
declared
missing
wrong-scope
unsupported-version
unsupported-platform
unknown
```

## Versioning

Do not use one universal version parser. Support:

- `semver`
- `integer`
- `scaled-integer`
- `string`
- `unknown`
- `custom`

Use structured constraints such as:

```yaml
version:
  scheme: semver
  minInclusive: 6.4.0
  maxExclusive: 7.0.0
```

## Phase-aware requirements

Requirements can apply to generation, validation, launch, packaging, release, or optional presentation.

MCM Extender JSON generation is hard-required only for that generator path, not for Forge core. If missing, fail clearly or use a declared fallback such as an INI template when the project policy allows it.

JIP Script Runner generation must declare event prefixes, size budget, provider requirements, and FormID or Editor ID resolution strategy.

## Diagnostics

Reserve `WF-CAP-*` diagnostics for missing required capability, optional capability unavailable, unsupported provider version, wrong-scope install, unknown capability, detector failure, conflicting providers, declared-but-unused capability, missing fallback, and runtime-only capability unverifiable offline.

Explain why a capability is unavailable, including transitive provider evidence. Do not emit only "missing dependency".

In CI and public fixtures, capability checks must use synthetic providers, declared fixtures, or local deterministic detectors. Do not require public CI to include Bethesda assets, third-party mod files, or a real game install.
