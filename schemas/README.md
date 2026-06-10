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
- `dependencies/0.1.0/schema.json` - dependency registry schema.
- `capabilities/0.1.0/schema.json` - capability registry schema.
- `assets/0.1.0/schema.json` - asset registry schema.
- `quests/0.1.0/schema.json` - quest registry schema.
- `quests/0.2.0/schema.json` - quest registry schema with stage and objective skeletons.
- `quests/0.3.0/schema.json` - quest registry schema with transition skeletons.
- `quests/0.4.0/schema.json` - quest registry schema with condition skeletons.
- `quests/0.5.0/schema.json` - quest registry schema with result-script skeletons.
- `quests/0.6.0/schema.json` - quest registry schema with variable skeletons.
- `dialogue/0.1.0/schema.json` - dialogue registry schema.
- `dialogue/0.2.0/schema.json` - dialogue registry schema with condition skeletons.
- `dialogue/0.3.0/schema.json` - dialogue registry schema with result-script skeletons.
- `dialogue/0.4.0/schema.json` - dialogue registry schema with topic/link skeletons.
- `dialogue/0.5.0/schema.json` - dialogue registry schema with quest-level gate skeletons.
- `dialogue/0.6.0/schema.json` - dialogue registry schema with result-script variable mutation skeletons.
- `dialogue/0.7.0/schema.json` - dialogue registry schema with Link From skeletons.
- `dialogue/0.8.0/schema.json` - dialogue registry schema with priority and prompt routing skeletons.

Gate 12 evaluates the embedded manifest schema at runtime. Gate 13 evaluates
embedded dependency and capability registry schemas at runtime. Gate 14
evaluates embedded asset registry schemas at runtime. Gate 18 evaluates
embedded dialogue registry schemas at runtime. Gate 19 evaluates embedded quest
registry schemas at runtime. Gate 20 adds quest registry schema `0.2.0` while
preserving quest registry schema `0.1.0`. Gate 21 adds quest registry schema
`0.3.0` while preserving earlier quest registry schemas. Gate 22 adds quest
registry schema `0.4.0` while preserving earlier quest registry schemas. Gate
23 adds quest registry schema `0.5.0` while preserving earlier quest registry
schemas. Gate 24 adds quest registry schema `0.6.0` while preserving earlier
quest registry schemas. Gate 25 adds dialogue registry schema `0.2.0` while
preserving dialogue registry schema `0.1.0`. Gate 26 adds dialogue registry
schema `0.3.0` while preserving earlier dialogue registry schemas. Gate 27
adds dialogue registry schema `0.4.0` while preserving earlier dialogue
registry schemas. Gate 28 adds dialogue registry schema `0.5.0` while
preserving earlier dialogue registry schemas. Gate 29 adds dialogue registry
schema `0.6.0` while preserving earlier dialogue registry schemas. Gate 30
adds dialogue registry schema `0.7.0` while preserving earlier dialogue
registry schemas. Gate 31 adds no new schema; it preserves dialogue registry
schema `0.7.0` and adds semantic validation over the derived dialogue link
graph. Gate 32 adds dialogue registry schema `0.8.0` while preserving earlier
dialogue registry schemas.
