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

Gate 17 adds:

- `projects/BrokenCases/InvalidVoiceAssets` - deterministic `WF-ASSET-007`
  through `WF-ASSET-009` failure cases for voice target shape, WAV/OGG pair,
  and LIP pair validation.

Gate 18 updates `projects/ExampleMod` with a synthetic dialogue registry and
matching voice/lip assets, and adds:

- `projects/BrokenCases/InvalidDialogueRegistry` - runtime dialogue registry
  schema failure case.
- `projects/BrokenCases/MissingDialogueVoiceAssets` - deterministic
  `WF-SEM-015` failure case for a dialogue voice work item whose declared
  voice/lip assets are absent.

Gate 19 updates `projects/ExampleMod` with a synthetic quest registry and adds:

- `projects/BrokenCases/InvalidQuestRegistry` - runtime quest registry schema
  failure case.
- `projects/BrokenCases/MissingDialogueQuestReference` - deterministic
  `WF-SEM-016` failure case for a dialogue `questId` that is not declared in
  the quest registry.

Gate 20 updates `projects/ExampleMod` to quest registry schema `0.2.0` with
synthetic stage and objective declarations, and adds:

- `projects/BrokenCases/InvalidQuestStageObjectiveRegistry` - runtime quest
  registry schema failure case for invalid objective shape.
- `projects/BrokenCases/MissingQuestObjectiveStageReference` - deterministic
  `WF-SEM-017` failure case for an objective stage reference that is not
  declared in the same quest.

Gate 21 updates `projects/ExampleMod` to quest registry schema `0.3.0` with a
synthetic transition declaration, and adds:

- `projects/BrokenCases/InvalidQuestTransitionRegistry` - runtime quest
  registry schema failure case for invalid transition shape.
- `projects/BrokenCases/MissingQuestTransitionStageReference` - deterministic
  `WF-SEM-018` failure case for a transition stage reference that is not
  declared in the same quest.

Gate 22 updates `projects/ExampleMod` to quest registry schema `0.4.0` with a
synthetic stage-done condition declaration, and adds:

- `projects/BrokenCases/InvalidQuestConditionRegistry` - runtime quest
  registry schema failure case for invalid condition shape.
- `projects/BrokenCases/MissingQuestConditionStageReference` - deterministic
  `WF-SEM-019` failure case for a condition stage reference that is not
  declared in the same quest.
