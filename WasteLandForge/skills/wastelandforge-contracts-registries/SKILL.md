---
name: wastelandforge-contracts-registries
description: Use this skill for WastelandForge manifests, YAML/JSON source files, canonical JSON normalization, JSON Schema, registry design, diagnostics, provenance, IDs, schema versioning, and ADR-007. It should trigger whenever a task touches source contracts, registries, validation output, schema packages, or generated artifact traceability.
---

# WastelandForge Contracts And Registries

## Research base

Use:

- `R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md`.
- `Wasteland Forge Platform Architecture & System Design-deep-research-report.md`.
- `WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md` for generated outputs and provenance.

## Contract decision

Apply ADR-007:

WastelandForge source truth is represented as versioned YAML/JSON registry documents normalized to canonical JSON, validated by JSON Schema Draft 2020-12 plus deterministic semantic validators. Generated artifacts are outputs, not truth.

## v0.1 stack

Use the research-backed .NET stack:

- `System.Text.Json` for canonical JSON and DOM work.
- `YamlDotNet` for YAML ingestion.
- `JsonSchema.Net` for runtime schema validation.

Avoid using Json.NET Schema as the default core validator because the report identifies AGPL/commercial licensing constraints. Treat Corvus.JsonSchema as a possible later extension for typed models, not a v0.1 requirement.

## YAML and JSON policy

- Authors may write YAML or JSON.
- Forge normalizes everything into canonical JSON before validation, indexing, generation, hashing, or provenance.
- YAML v0.1 should be a safe JSON-shaped subset: mappings, sequences, scalar values, and comments.
- Reject anchors, aliases, custom tags, merge-key indirection, and multi-document streams in canonical source contracts.
- Allow comments in source, but do not promise comment-preserving rewrites outside explicit `fmt` or `migrate` commands.
- Schema defaults are annotations, not implicit authored state. If defaulting exists, it must be explicit and provenance-recorded.

## Registry model

Use three layers:

- Canonical: human-authored source truth.
- Generated: deterministic implementation outputs.
- Derived: recomputable views such as search indexes, graphs, reports, matrices, docs, and agent context packs.

The v0.1 registry order is:

- Manifest: mandatory.
- Dependency registry: mandatory.
- Capability registry: mandatory.
- Asset registry: strongly recommended.
- Quest, dialogue, faction, reputation event, and release registries: later after the contract layer stabilizes.

Logical registries may be one file or a directory. The manifest points to registry roots, not one hard-coded aggregate format.

## Identity policy

Use stable dotted lowercase logical IDs as primary identity. Reverse-DNS is preferred but not required.

Use FormIDs, plugin names, and Editor IDs only as external implementation references. Do not use FormIDs as primary Forge IDs because load-order-corrected FormIDs are brittle as canonical cross-registry identity.

Reserve platform namespaces such as `game.*`, `tool.*`, `editor.*`, `runtime.*`, and `wf.*`.

## Diagnostics

Emit diagnostics with stable rule IDs, severities, categories, JSON Pointer locations, related locations, suggested fixes, and docs URIs.

Required output views are:

- console text,
- JSON,
- SARIF 2.1.0,
- Markdown summary.

Keep rule IDs stable across console, JSON, SARIF, GitHub annotations, and future LSP diagnostics.

## Provenance

Every successful build must write a local `build-manifest.json` with Forge version, project ID, schema versions, source digests, generator versions, resolved capability set, output digests, and enough data to explain how artifacts were produced.

Release-grade attestations can be added later, but local provenance is mandatory even offline.
