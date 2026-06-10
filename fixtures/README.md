# Fixtures

This directory will hold public synthetic fixtures.

Fixture policy:

- synthetic and redistributable only,
- no Bethesda game assets,
- no third-party mod files without explicit permission,
- no private user installs in public fixtures.

Gate 5 adds the first JSON-only synthetic fixture projects used by the loader
and validation pipeline:

- `projects/ExampleMod` - valid bootstrap manifest plus dependency and
  capability registry documents.
- `projects/BrokenCases/MissingCapability` - deterministic `WF-SEM-014`
  failure case for an unknown capability reference.

Gate 7 adds golden outputs under `golden/` for CLI help and validation JSON
contract tests.

Gate 12 adds:

- `projects/YamlExample` - valid YAML manifest plus YAML dependency and
  capability registry documents.
- `projects/BrokenCases/InvalidManifestSchema` - runtime manifest schema
  failure case for an invalid project ID.
- `projects/BrokenCases/UnsupportedYamlFeature` - deterministic
  `WF-LOAD-007` failure case for unsupported YAML anchors.

Gate 13 updates the capability fixtures to the schema-backed `satisfiedBy`
provider data shape and adds:

- `projects/BrokenCases/InvalidDependencyRegistry` - runtime dependency
  registry schema failure case.
- `projects/BrokenCases/InvalidCapabilityRegistry` - runtime capability
  registry schema failure case.

Gate 14 adds synthetic asset registry documents to `projects/ExampleMod` and
`projects/YamlExample`, plus:

- `projects/BrokenCases/InvalidAssetRegistry` - runtime asset registry schema
  failure case.

Gate 15 adds tiny synthetic asset source files for valid fixtures and:

- `projects/BrokenCases/InvalidAssetPaths` - deterministic `WF-ASSET-001`
  through `WF-ASSET-004` failure cases for source escape, missing source,
  target traversal, and target extension mismatch.

Gate 16 adds:

- `projects/BrokenCases/InvalidAssetTypes` - deterministic `WF-ASSET-005`
  and `WF-ASSET-006` failure cases for source signature mismatch and target
  root mismatch.
