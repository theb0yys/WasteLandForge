# Schema Version Policy

Status: Skeleton
Research classification: Documented
Source: R004 / ADR-007

Schemas are public API.

## Rules

- Every public schema must declare `$schema`.
- Every public schema must declare an absolute `$id`.
- The full semantic version belongs in the immutable `$id`.
- Released schema contents must not be overwritten in place.
- Historical schema IDs should remain resolvable.
- Migrations are explicit tool behavior, not disappearing-schema behavior.
- Schema defaults are annotations only; validation must not silently materialize default values as source truth.

## ID Convention

Use:

```text
https://schemas.wastelandforge.dev/fnv/<schema-kind>/<schema-version>/schema.json
```

Example:

```text
https://schemas.wastelandforge.dev/fnv/manifest/0.1.0/schema.json
```

## Gate Ownership

Gate 3 establishes this convention. Gate 12 evaluates the embedded manifest
schema at runtime during source validation. Gate 13 evaluates the embedded
dependency and capability registry schemas at runtime. Gate 14 evaluates the
embedded asset registry schema at runtime. Later gates may add resolver
behavior, editor companion schemas, and migration commands without weakening
immutability.
