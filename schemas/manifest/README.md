# Manifest Schemas

The manifest is the mandatory root document for a WastelandForge project.

It answers:

- project identity,
- target game,
- schema compatibility,
- registry roots,
- top-level outputs.

The manifest schema version and the mod/package version are separate values.

Gate 12 validates the normalized manifest object against the embedded
`0.1.0/schema.json` Draft 2020-12 schema at runtime.

The `registries.assets`, `registries.dialogue`, and `registries.quests` paths
remain optional in v0.1. When present, the validation pipeline validates the
referenced registry documents against their embedded schemas before semantic
cross-registry checks run.
