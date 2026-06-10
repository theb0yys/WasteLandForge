# Asset Registry Schemas

Asset registry schemas validate source-controlled asset declarations for
WastelandForge projects.

R004 strongly recommends the asset registry in v0.1 after the mandatory
manifest, dependency, and capability bootstrap schemas. Gate 14 adds
`0.1.0/schema.json` as an immutable Draft 2020-12 public schema.

Gate 15 validates schema-valid asset paths semantically. The schema still
defines contract shape only; path existence, traversal, and extension checks
belong to validation rules.

Gate 16 adds type-specific semantic validation for minimal source signatures
and target root conventions. The schema remains contract-shape only; content
inspection and game-data placement rules stay in validation code.
