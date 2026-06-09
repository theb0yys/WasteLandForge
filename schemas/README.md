# Schemas

This directory holds WastelandForge schema packages.

Gate 3 creates the first schema skeleton and immutable `$id` convention.

## Version Policy

Published schema IDs are immutable and include the full semantic version:

```text
https://schemas.wastelandforge.dev/fnv/<schema-kind>/<schema-version>/schema.json
```

Example:

```text
https://schemas.wastelandforge.dev/fnv/manifest/0.1.0/schema.json
```

Do not overwrite a released schema in place. Schema migrations are explicit tool work, not silent replacement.

## Current Schema Roots

- `manifest/0.1.0/schema.json` - root `wastelandforge.yaml` manifest schema.
- `dependencies/` - reserved for dependency registry schemas.
- `capabilities/` - reserved for capability registry schemas.
- `assets/` - reserved for asset registry schemas.
