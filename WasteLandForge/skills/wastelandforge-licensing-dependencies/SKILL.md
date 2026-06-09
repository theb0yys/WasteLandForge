---
name: wastelandforge-licensing-dependencies
description: Use this skill for WastelandForge dependency policy, third-party FNV tool licensing, redistribution, bundling, provider adoption, ecosystem norms, WFG-001, and ADR-002A. It should trigger whenever a task mentions xNVSE, JIP LN, JIP PP LN, JohnnyGuitar, ShowOff, UIO, MCM, MCM Extender, GECK Extender, xEdit, MO2, bundling, licenses, Nexus permissions, GPL, MPL, LGPL, MIT, proprietary dependencies, or hard requirements.
---

# WastelandForge Licensing And Dependencies

## Research base

Use:

- `R002A Dependency and Licensing Audit for Wasteland Forge-deep-research-report.md`.
- `Fallout New Vegas Tooling Ecosystem and Modding Landscape-deep-research-report.md`.
- `R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`.

## ADR-002A decision

Adopt a capability-driven, hybrid orchestration model.

Forge should build the project layer: scaffolding, registry generation, asset validation, dialogue/menu generation, release pipelines, documentation, compatibility reports, and diagnostics.

Forge should integrate with existing tools and runtime layers. It should avoid replacing xNVSE, xEdit, MO2, GECK, MCM, UIO, and the broader script-extender ecosystem.

## WFG-001

No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core, and no third-party runtime binary should be rehosted by default.

Dependencies should be acquired from official sources where possible. Runtime features should be capability-detected. External integrations should prefer public APIs, plugin systems, scripts, or generated configuration over replacement.

## Dependency posture

Use the audit posture:

- xNVSE: hard baseline to depend on; never replace; never repackage official binaries.
- JIP LN: strongly supported; do not mirror official Nexus archive by default.
- JIP PP LN: optional compatibility target, not hard baseline yet.
- JohnnyGuitar: recommended optional dependency; comply with LGPL if redistributing anything derived.
- ShowOff: recommended optional dependency; prefer detect-and-adapt over bundle.
- UIO: depend for UI workflows; do not rehost official archive by default.
- MCM: optional UI layer; never bundle; core gameplay must not depend on MCM presence.
- MCM Extender: excellent generated-menu target; do not bundle or modify core assets.
- GECK Extender: integrate with GECK workflows; detect externally; do not replace or rehost by default.
- xEdit: integrate and automate; never replace.
- MO2: integrate through plugin/API surfaces; do not ship a replacement mod manager.

## Legal caution

Public repository license and permission to rehost an official Nexus archive are not the same thing. When sources diverge, use the safer policy: link to official downloads, detect installed providers, and avoid bundling.

This skill is not legal advice. For actual redistribution of third-party binaries or modified assets, require maintainer permission or formal legal review.

## Design consequence

Model dependencies as capabilities and providers. Do not encode "install this bundled binary" as the core path. The platform should remain useful with official external installations and local deterministic detection.
