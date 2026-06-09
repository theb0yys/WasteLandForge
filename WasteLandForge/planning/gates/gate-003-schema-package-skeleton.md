# Gate 3 - Schema Package Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 2, ADR-007, ADR-011

## Gate Definition

Gate 3 creates the first schema package skeleton. It establishes schema directories, immutable `$id` convention, the first manifest schema stub, local schema catalog shape, and schema version policy documentation.

This gate does not implement runtime schema validation, YAML loading, semantic validation, diagnostics, or migration commands.

## Research Decisions Used

| Classification | Decision | Source |
|---|---|---|
| Documented | Source truth is YAML/JSON normalized to canonical JSON and validated with JSON Schema Draft 2020-12 plus deterministic semantic validators. | R004 / ADR-007 |
| Documented | The manifest is mandatory and remains the indispensable root document. | R004 / ADR-007 |
| Documented | Dependency and capability registries are mandatory bootstrap registries; asset registry is strongly recommended in v0.1. | R004 / ADR-007 |
| Documented | Every public schema must declare `$schema`, an absolute `$id`, and use a full semantic version in the immutable `$id`. | R004 / ADR-007 |
| Documented | Schemas are public API and should be schema-first, not type-first. | R004 / ADR-007 |
| Inferred | Gate 3 can add a local schema catalog shape without adding JsonSchema.Net because runtime validation is a later gate. | Gate 0 |

## Deliverables

- `schemas/manifest/0.1.0/schema.json`
- `schemas/manifest/README.md`
- `schemas/dependencies/README.md`
- `schemas/capabilities/README.md`
- `schemas/assets/README.md`
- `docs/governance/schema-version-policy.md`
- `src/WastelandForge.Schema/SchemaResource.cs`
- `src/WastelandForge.Schema/WastelandForgeSchemaIds.cs`
- `src/WastelandForge.Schema/WastelandForgeSchemaCatalog.cs`
- schema project resource inclusion for JSON schemas

## Manifest Schema Scope

The first manifest schema covers:

- `schemaVersion`
- `kind`
- stable dotted lowercase project `id`
- project `name`
- project `version`
- target `game`
- optional `metadata`
- mandatory dependency and capability registry roots
- optional asset registry root
- declared generated output paths

The schema keeps `schemaVersion` separate from the project release `version`.

## Local Resolver Shape

Gate 3 introduces a buildable catalog shape only:

- schema ID constants,
- schema resource metadata,
- built-in schema catalog lookup by absolute `$id`.

It does not parse, evaluate, or validate schemas.

## Verification

Checks performed:

```text
Get-Content -Raw schemas/manifest/0.1.0/schema.json | ConvertFrom-Json
dotnet build WastelandForge.sln -c Release
git diff --check
```

Expected result:

```text
schema JSON parses
Build succeeded.
0 Warning(s)
0 Error(s)
```

## Open Checks

| Check | Status | Gate |
|---|---|---|
| Decide whether Draft 7 editor companion schemas are generated in v0.1 or deferred behind an editor-extension plan. | Open | 3 or later |
| Add dependency registry schema. | Open | Later schema gate |
| Add capability registry schema. | Open | Later schema gate |
| Add asset registry schema. | Open | Later schema gate |
| Add JsonSchema.Net runtime validation. | Open | Gate 5 |

## Next Gate

Gate 4 creates core domain and diagnostics:

1. IDs,
2. source locations,
3. version constraints,
4. diagnostics,
5. severities,
6. JSON Pointer locations,
7. issue serialization.
