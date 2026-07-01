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

Gate 69 adds:

- `projects/BrokenCases/InvalidMcmImageAssetReferences` - deterministic
  `WF-ASSET-010` and `WF-ASSET-011` failure cases for MCM image filename
  path shape and required texture asset target resolution.

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

Gate 23 updates `projects/ExampleMod` to quest registry schema `0.5.0` with a
synthetic stage result-script declaration, and adds:

- `projects/BrokenCases/InvalidQuestResultScriptRegistry` - runtime quest
  registry schema failure case for invalid result-script shape.
- `projects/BrokenCases/MissingQuestResultScriptConditionReference` -
  deterministic `WF-SEM-020` failure case for a result-script condition
  reference that is not declared in the same quest.

Gate 24 updates `projects/ExampleMod` to quest registry schema `0.6.0` with a
synthetic integer quest variable declaration and variable-equals condition,
and adds:

- `projects/BrokenCases/InvalidQuestVariableRegistry` - runtime quest registry
  schema failure case for invalid variable shape.
- `projects/BrokenCases/MissingQuestConditionVariableReference` -
  deterministic `WF-SEM-021` failure case for a condition variable reference
  that is not declared in the same quest.

Gate 25 updates `projects/ExampleMod` to dialogue registry schema `0.2.0` with
synthetic line-local quest-stage and quest-variable conditions, and adds:

- `projects/BrokenCases/InvalidDialogueConditionRegistry` - runtime dialogue
  registry schema failure case for invalid condition shape.
- `projects/BrokenCases/MissingDialogueConditionStageReference` -
  deterministic `WF-SEM-022` failure case for a dialogue condition stage
  reference that is not declared in the line's referenced quest.
- `projects/BrokenCases/MissingDialogueConditionVariableReference` -
  deterministic `WF-SEM-023` failure case for a dialogue condition variable
  reference that is not declared in the line's referenced quest.

Gate 26 updates `projects/ExampleMod` to dialogue registry schema `0.3.0` with
a synthetic dialogue result-script declaration, and adds:

- `projects/BrokenCases/InvalidDialogueResultScriptRegistry` - runtime
  dialogue registry schema failure case for invalid result-script shape.

Gate 27 updates `projects/ExampleMod` to dialogue registry schema `0.4.0` with
synthetic topic declarations and a minimal `linkTo` topic link, and adds:

- `projects/BrokenCases/InvalidDialogueTopicRegistry` - runtime dialogue
  registry schema failure case for invalid topic shape.
- `projects/BrokenCases/MissingDialogueTopicReference` - deterministic
  `WF-SEM-024` failure case for a dialogue line topic reference that is not
  declared in dialogue topics.
- `projects/BrokenCases/MissingDialogueTopicLinkReference` - deterministic
  `WF-SEM-025` failure case for a dialogue `linkTo` target topic reference
  that is not declared in dialogue topics.

Gate 28 updates `projects/ExampleMod` to dialogue registry schema `0.5.0` with
a synthetic quest-level dialogue gate, and adds:

- `projects/BrokenCases/InvalidDialogueQuestGateRegistry` - runtime dialogue
  registry schema failure case for invalid quest gate shape.
- `projects/BrokenCases/MissingDialogueQuestGateReference` - deterministic
  `WF-SEM-026` failure case for a dialogue quest gate `questId` that is not
  declared in the quest registry.
- `projects/BrokenCases/MissingDialogueQuestGateStageReference` -
  deterministic `WF-SEM-027` failure case for a dialogue quest gate condition
  stage reference that is not declared in the gate's referenced quest.
- `projects/BrokenCases/MissingDialogueQuestGateVariableReference` -
  deterministic `WF-SEM-028` failure case for a dialogue quest gate condition
  variable reference that is not declared in the gate's referenced quest.

Gate 29 updates `projects/ExampleMod` to dialogue registry schema `0.6.0` with
a synthetic dialogue result-script quest-variable increment mutation, and adds:

- `projects/BrokenCases/InvalidDialogueResultScriptMutationRegistry` -
  runtime dialogue registry schema failure case for invalid mutation shape.
- `projects/BrokenCases/MissingDialogueResultScriptMutationVariableReference`
  - deterministic `WF-SEM-029` failure case for a dialogue result-script
  mutation variable reference that is not declared in the dialogue line's
  referenced quest.

Gate 30 updates `projects/ExampleMod` to dialogue registry schema `0.7.0` with
a synthetic dialogue `linkFrom` source topic declaration, and adds:

- `projects/BrokenCases/InvalidDialogueLinkFromRegistry` - runtime dialogue
  registry schema failure case for invalid Link From shape.
- `projects/BrokenCases/MissingDialogueLinkFromReference` - deterministic
  `WF-SEM-030` failure case for a dialogue `linkFrom` source topic reference
  that is not declared in dialogue topics.

Gate 31 preserves dialogue registry schema `0.7.0` and adds:

- `projects/BrokenCases/MissingDialogueLinkTargetLine` - deterministic
  `WF-SEM-031` failure case for a dialogue `linkTo` target topic that is
  declared but has no authored dialogue line endpoint.
- `projects/BrokenCases/MissingDialogueLinkSourceLine` - deterministic
  `WF-SEM-032` failure case for a dialogue `linkFrom` source topic that is
  declared but has no authored dialogue line endpoint.

Gate 32 updates `projects/ExampleMod` to dialogue registry schema `0.8.0` with
synthetic line priority and prompt route declarations, and adds:

- `projects/BrokenCases/InvalidDialoguePromptRouteRegistry` - runtime dialogue
  registry schema failure case for prompt text without explicit priority.
- `projects/BrokenCases/DuplicateDialoguePromptRoute` - deterministic
  `WF-SEM-033` failure case for duplicate authored prompt routes with the same
  topic, prompt text, and priority.

Gate 33 updates `projects/ExampleMod` to dialogue registry schema `0.9.0` with
a synthetic Speech Challenge declaration, and adds:

- `projects/BrokenCases/InvalidDialogueSpeechChallengeRegistry` - runtime
  dialogue registry schema failure case for Speech Challenge shape without the
  required threshold.

Gate 34 updates `projects/ExampleMod` to dialogue registry schema `0.10.0`
with a synthetic line-local skill gate declaration, and adds:

- `projects/BrokenCases/InvalidDialogueSkillGateRegistry` - runtime dialogue
  registry schema failure case for skill gate shape without the required
  threshold.

Gate 35 updates `projects/ExampleMod` to dialogue registry schema `0.11.0`
with a synthetic line-local perk gate declaration, and adds:

- `projects/BrokenCases/InvalidDialoguePerkGateRegistry` - runtime dialogue
  registry schema failure case for perk gate shape without the required perk
  key.

Gate 36 updates `projects/ExampleMod` to dialogue registry schema `0.12.0`
with synthetic line-local faction relation and reputation standing gate
declarations, and adds:

- `projects/BrokenCases/InvalidDialogueFactionGateRegistry` - runtime dialogue
  registry schema failure case for faction gate shape without the required
  relation key.
- `projects/BrokenCases/InvalidDialogueReputationGateRegistry` - runtime
  dialogue registry schema failure case for reputation gate shape without the
  required standing key.

Gate 37 updates `projects/ExampleMod` to dialogue registry schema `0.13.0`
with a synthetic line-local identity gate declaration, and adds:

- `projects/BrokenCases/InvalidDialogueIdentityGateRegistry` - runtime
  dialogue registry schema failure case for identity gate shape without the
  required identity key.

Gate 38 updates `projects/ExampleMod` to dialogue registry schema `0.14.0`
with a synthetic line-local local world flag gate declaration, and adds:

- `projects/BrokenCases/InvalidDialogueWorldFlagGateRegistry` - runtime
  dialogue registry schema failure case for local world flag gate shape
  without the required state key.

Gate 39 updates `projects/ExampleMod` to dialogue registry schema `0.15.0`
with a synthetic line-local event history gate declaration, and adds:

- `projects/BrokenCases/InvalidDialogueEventHistoryGateRegistry` - runtime
  dialogue registry schema failure case for event history gate shape without
  the required state key.

Gate 40 updates `projects/ExampleMod` to dialogue registry schema `0.16.0`
with a synthetic line-local companion state gate declaration, and adds:

- `projects/BrokenCases/InvalidDialogueCompanionStateGateRegistry` - runtime
  dialogue registry schema failure case for companion state gate shape without
  the required state key.

Gate 41 updates `projects/ExampleMod` to dialogue registry schema `0.17.0`
with a synthetic line-local result-script side-effect gate declaration, and
adds:

- `projects/BrokenCases/InvalidDialogueResultScriptSideEffectGateRegistry` -
  runtime dialogue registry schema failure case for result-script side-effect
  gate shape without the required state key.

Gate 42 updates `projects/ExampleMod` to dialogue registry schema `0.18.0`
with a synthetic line-local condition boolean composition declaration, and
adds:

- `projects/BrokenCases/InvalidDialogueConditionLogicRegistry` - runtime
  dialogue registry schema failure case for condition boolean composition
  shape without the required operator key.

Gate 43 preserves dialogue registry schema `0.18.0` and adds:

- `projects/BrokenCases/MissingDialogueConditionLogicReference` -
  deterministic `WF-SEM-034` failure case for a dialogue condition logic
  reference that is not authored as a condition on the same line.

Gate 44 updates `projects/ExampleMod` to dialogue registry schema `0.19.0`
with a synthetic nested condition group declaration, and adds:

- `projects/BrokenCases/InvalidDialogueNestedConditionGroupRegistry` - runtime
  dialogue registry schema failure case for nested condition group shape with
  an unsupported operator value.
- `projects/BrokenCases/MissingNestedDialogueConditionLogicReference` -
  deterministic `WF-SEM-034` failure case for a nested dialogue condition
  group reference that is not authored as a condition on the same line.

Gate 45 adds:

- `geck/dialogue/synthetic-quest-dialogue-export.txt` - synthetic
  redistributable text fixture for the file-based GECK dialogue export
  validation bridge.

Gate 46 updates `projects/ExampleMod` to dialogue registry schema `0.20.0`
with a synthetic negated condition reference, and adds:

- `projects/BrokenCases/InvalidDialogueConditionNegationRegistry` - runtime
  dialogue registry schema failure case for invalid empty negated condition
  reference shape.
- `projects/BrokenCases/MissingNegatedDialogueConditionLogicReference` -
  deterministic `WF-SEM-034` failure case for a negated dialogue condition
  logic reference that is not authored as a condition on the same line.

Gate 47 updates `projects/ExampleMod` to dialogue registry schema `0.21.0`
with synthetic condition precedence ranks, and adds:

- `projects/BrokenCases/InvalidDialogueConditionPrecedenceRegistry` - runtime
  dialogue registry schema failure case for invalid condition precedence
  shape.

Gate 48 updates `projects/ExampleMod` to dialogue registry schema `0.22.0`
with synthetic condition short-circuit intent, and adds:

- `projects/BrokenCases/InvalidDialogueConditionShortCircuitRegistry` -
  runtime dialogue registry schema failure case for invalid condition
  short-circuit shape.

Gate 49 preserves dialogue registry schema `0.22.0` and adds:

- `projects/BrokenCases/DuplicateDialogueConditionLogicIdentity` -
  deterministic `WF-SEM-035` failure case for a duplicate root/nested
  condition logic ID inside one dialogue line.

Gate 50 updates `projects/ExampleMod` to dialogue registry schema `0.23.0`
with a synthetic response route declaration, and adds:

- `projects/BrokenCases/InvalidDialogueResponseRouteRegistry` - runtime
  dialogue registry schema failure case for invalid response route shape.

Gate 51 preserves dialogue registry schema `0.23.0` and adds:

- `projects/BrokenCases/MissingDialogueResponseRouteTargetReference` -
  deterministic `WF-SEM-036` failure case for a response route target topic
  that is not declared in dialogue topics.

Gate 52 preserves dialogue registry schema `0.23.0` and adds:

- `projects/BrokenCases/MissingDialogueResponseRouteTargetLine` -
  deterministic `WF-SEM-037` failure case for a declared response route target
  topic with no authored dialogue line endpoint.

Gate 53 preserves dialogue registry schema `0.23.0` and adds:

- `projects/BrokenCases/DuplicateDialogueResponseRouteIdentity` -
  deterministic `WF-SEM-038` failure case for duplicate response route IDs
  authored on the same dialogue line.

Gate 54 preserves dialogue registry schema `0.23.0` and adds:

- `projects/BrokenCases/DuplicateDialogueResponseRouteKey` -
  deterministic `WF-SEM-039` failure case for duplicate response route keys
  authored on the same dialogue line.

Gate 55 adds no fixture. It is an evidence checkpoint that keeps public
fixtures synthetic and blocks route taxonomy work until representative evidence
is documented.

Gate 56 adds no fixture. It creates a docs-only evidence pack skeleton and
continues to require any future public route examples to be synthetic and
redistributable.

Gate 57 adds no fixture. Capability catalogue tests use built-in registry data
and do not inspect local game installs or third-party mod files.

Gate 58 adds no committed fixture. Capability scan tests create temp-only
synthetic folders and empty marker files at runtime to exercise root-file,
data-file, and executable-tool detectors.

Gate 59 adds no committed fixture. Capability explanation tests reuse
temp-only synthetic folders and empty marker files at runtime to explain
capability and provider status from scan evidence.

Gate 60 adds no committed fixture. Capability requirement resolution tests use
the existing synthetic ExampleMod dependency registry plus temp-only FNV marker
files created at runtime.

Gate 61 adds no committed fixture. Metadata report generator tests copy the
existing synthetic ExampleMod project to temp folders and write generated or
dist outputs only in those temp folders.

Gate 62 updates the synthetic ExampleMod fixture with manifest schema `0.2.0`,
a synthetic MCM registry, and a declared `runtime.ui.mcm_json` generation
dependency. It does not add Bethesda assets, third-party MCM/MCM Extender
files, or generated outputs to the committed fixture corpus.

Gate 63 updates the synthetic ExampleMod MCM registry with `minMCMVersion` and
slider scale metadata so the generator can emit and validate the Gate 63
runtime-shaped MCM Extender JSON subset. It still adds no Bethesda assets,
third-party MCM/MCM Extender files, or generated outputs to the committed
fixture corpus.

Gate 64 updates the synthetic ExampleMod MCM registry with runtime
`requirements` and `$...` translation keys plus translation text. It still
adds no Bethesda assets, third-party MCM/MCM Extender files, or generated
outputs to the committed fixture corpus.

Gate 65 updates the synthetic ExampleMod MCM registry with `checkbox` and
`stringToggle` settings plus matching synthetic translation text. It still
adds no Bethesda assets, third-party MCM/MCM Extender files, or generated
outputs to the committed fixture corpus.

Gate 66 updates the synthetic ExampleMod MCM registry with a `keybind` setting
and matching synthetic translation text. It still adds no Bethesda assets,
third-party MCM/MCM Extender files, or generated outputs to the committed
fixture corpus.

Gate 67 updates the synthetic ExampleMod MCM registry with a `header` setting
and matching synthetic translation text. It still adds no Bethesda assets,
third-party MCM/MCM Extender files, or generated outputs to the committed
fixture corpus.

Gate 68 updates the synthetic ExampleMod MCM registry with an `image` setting
and matching synthetic translation text.

Gate 69 updates the synthetic ExampleMod asset registry with a tiny
handcrafted DDS-header fixture for that MCM image reference. It still adds no
Bethesda assets, third-party MCM/MCM Extender files, or generated outputs to
the committed fixture corpus.

Gate 70 uses that synthetic texture fixture as a generate/build input and
stages it only into generated or dist output trees during local runs. It still
adds no generated outputs to the committed fixture corpus.

Gate 71 uses the same fixture to test package-manifest metadata generated
during local runs. It still adds no generated package manifests or archives to
the committed fixture corpus.

Gate 72 uses the same fixture to test generated ZIP archive output during
local build runs. It still adds no generated ZIP archives to the committed
fixture corpus.

Gate 73 uses the same fixture to test canonical `forge package` output during
local package runs. It still adds no generated package outputs or archives to
the committed fixture corpus.

Gate 74 uses the same fixture to test package-manifest schema validation and
archive entry validation. It still adds no generated package manifests,
archives, or package command output trees to the committed fixture corpus.

Gate 75 uses the same fixture to test generated install-preview JSON reports.
It still adds no generated install previews, archives, or package command
output trees to the committed fixture corpus.

Gate 76 uses the same fixture to test install-preview schema validation. It
still adds no generated install previews, archives, or package command output
trees to the committed fixture corpus.

Gate 77 uses the same fixture to test generated install-preview Markdown
summaries. It still adds no generated install previews, archives, or package
command output trees to the committed fixture corpus.

Gate 78 uses the same fixture to test generated package-verification reports.
It still adds no generated package verification reports, archives, or package
command output trees to the committed fixture corpus.

Gate 79 uses the same fixture to test package-verification schema validation.
It still adds no generated package verification reports, archives, or package
command output trees to the committed fixture corpus.

Gate 80 uses the same fixture to test generated package-verification Markdown
summaries. It still adds no generated package verification reports, summaries,
archives, or package command output trees to the committed fixture corpus.

Gate 81 uses the same fixture to test package-verification evidence
cross-checks. It still adds no generated package verification reports,
summaries, archives, manifests, or package command output trees to the
committed fixture corpus.

Gate 82 adds reusable validator tests with synthetic in-memory JSON evidence.
It still adds no Bethesda assets, third-party MCM/MCM Extender files,
generated package verification reports, summaries, archives, manifests, or
package command output trees to the committed fixture corpus.

Gate 83 uses temp-only generated output from the synthetic `ExampleMod`
fixture to test the file-based package-verification verifier. It still adds no
generated package verification reports, summaries, archives, manifests, or
package command output trees to the committed fixture corpus.

Gate 84 uses the same temp-only generated output to test payload digest
recomputation after editing a generated MCM JSON payload file. It still adds
no generated package verification reports, summaries, archives, manifests, or
package command output trees to the committed fixture corpus.

Gate 85 uses the same temp-only generated output to test archive digest
recomputation after editing generated `package.zip`. It still adds no
generated package verification reports, summaries, archives, manifests, or
package command output trees to the committed fixture corpus.
