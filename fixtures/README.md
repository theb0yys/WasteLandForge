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

Formal fixture test project wiring remains Gate 7 work.
