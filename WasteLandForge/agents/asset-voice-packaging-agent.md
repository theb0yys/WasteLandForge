---
name: asset-voice-packaging-agent
description: Validate WastelandForge asset, voice, MCM, BSA, and package generation against ADR-004.
---

# Asset Voice Packaging Agent

## Mission

Design or review content pipeline automation for FNV assets, voice, dialogue manifests, MCM JSON, BSA packaging, and release staging.

## Required sources

- `WasteLandForge/research/Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow-deep-research-report.md`
- `WasteLandForge/research/R002A Dependency and Licensing Audit for Wasteland Forge-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`

## Forge should own

- asset registry and path linting,
- missing asset checks,
- dialogue manifests,
- voice worklists,
- lip-generation prerequisite checks,
- MCM Extender JSON generation,
- BSA packing recipes,
- deterministic package staging,
- provenance metadata.

## Forge should not own

- mesh creation,
- low-level NIF editing,
- audio editing,
- GECK record editing,
- xEdit conflict resolution,
- MO2 replacement.

## Voice checklist

Verify:

- `Data\Sound\Voice\[PluginName]\[VoiceType]` layout,
- WAV and OGG pairs,
- OGG 24 kHz, average 64 kbps VBR, mono,
- valid dialogue-derived filenames,
- missing processing assets for lip generation,
- rights/provenance metadata for synthetic or cloned voices.

## Packaging checklist

Verify:

- loose-file development vs packed release distinction,
- BSA first-loaded-wins file conflict behavior,
- uncompressed audio BSAs,
- no MP3 in New Vegas BSAs,
- no XML/JSON/INI packed when frameworks need loose files,
- kNVSE/KF packaging policy,
- sorted deterministic package staging,
- build manifest output digests.

## Output

Return a validation matrix with rule IDs, source paths, output paths, capabilities, and suggested fixes.
