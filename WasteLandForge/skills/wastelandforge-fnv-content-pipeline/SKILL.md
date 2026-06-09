---
name: wastelandforge-fnv-content-pipeline
description: Use this skill for Fallout New Vegas asset, voice, dialogue, terminal, MCM, BSA, MO2, GECK, xEdit, packaging, and content-production workflows in WastelandForge. It should trigger whenever a task mentions assets, meshes, textures, audio, voice, lip files, BSAs, MCM JSON, loose files, generated manifests, or ADR-004.
---

# WastelandForge FNV Content Pipeline

## Research base

Use:

- `Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow-deep-research-report.md`.
- `Fallout New Vegas Tooling Ecosystem and Modding Landscape-deep-research-report.md`.
- `R002A Dependency and Licensing Audit for Wasteland Forge-deep-research-report.md`.

## ADR-004 decision

Forge owns extensive production-layer automation, not creative-tool replacement.

Own:

- generation,
- validation,
- packaging,
- release automation,
- workflow integration,
- dialogue manifests,
- voice manifests,
- terminal/note content manifests,
- MCM Extender JSON,
- packaging manifests,
- release metadata.

Do not own:

- Blender,
- NifSkope,
- DAWs or audio editors,
- GECK record editing,
- xEdit conflict resolution,
- raw plugin editing,
- MO2 replacement,
- xNVSE/UIO/MCM replacement.

## Workflow backbone

Preserve the researched division of labor:

- GECK remains canonical for forms, quests, dialogue, terminals, and world data.
- GECK Extender is the modern baseline for serious GECK authoring.
- Hot Reload is valuable for script iteration.
- MO2 is the workflow hub for profiles, virtual filesystem visibility, and test contexts.
- xEdit inspects, validates, cleans, scripts, and analyzes plugin conflicts.

## Asset rules

Use deterministic validation for:

- missing meshes and textures,
- invalid paths,
- wrong texture suffixes,
- missing normal maps or material references,
- NIF/KF/RDT/DDS/WAV/OGG path conventions,
- capability-sensitive animation handling such as kNVSE/KF packaging.

Do not assume a modern import pipeline. FNV asset production is path-sensitive and record-driven.

## Voice and dialogue

Treat voice as a first-class build problem.

Forge should generate and validate:

- dialogue export/import manifests,
- per-quest voice worklists,
- `Data\Sound\Voice\[PluginName]\[VoiceType]` mappings,
- WAV/OGG pair presence,
- OGG constraints identified by the report: 24 kHz, average 64 kbps variable bitrate, mono,
- lip-generation prerequisites,
- missing processing assets required for New Vegas GECK lip generation.

Do not treat AI voice output as safe by default. Pair this skill with the AI/governance and licensing skills for voice provider, permission, provenance, and disclosure work.

## MCM and UI

MCM Extender JSON is the best first game-facing generator surface because it is structured, deterministic, script-free where possible, and does not require ESP/ESM menus. Treat MCM and MCM Extender as optional presentation layers, not core gameplay state.

Use UIO for UI coexistence. Do not replace it.

## Packaging

Development is easiest with loose files; release quality is best with packed archives when rules are followed.

Validate BSA rules from the research:

- BSA file conflict behavior is first-loaded-wins, unlike plugin override behavior.
- Pack DDS, NIF, WAV, OGG, EGM, EGT, LIP, LST, and SPT where appropriate.
- Do not pack MP3.
- Do not pack XML, JSON, or INI when downstream frameworks need them loose.
- Do not pack KF files where kNVSE may be involved unless the project proves it is safe.
- Keep audio BSAs uncompressed.
- Use backslashes in packed NIF paths.

If Forge writes into an MO2 workflow, materialize named output mods or workspaces. Do not leave anonymous artifacts in MO2 overwrite.
