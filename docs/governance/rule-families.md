# Rule Families

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

WastelandForge diagnostic rule IDs use reserved families:

| Family | Scope |
|---|---|
| `WF-LOAD-*` | File discovery, parsing, encoding, duplicate file IDs |
| `WF-SCHEMA-*` | Schema and contract shape |
| `WF-SEM-*` | Semantic and cross-registry rules |
| `WF-CAP-*` | Capability/provider rules |
| `WF-ASSET-*` | Asset and path rules |
| `WF-GEN-*` | Generator rules |
| `WF-BUILD-*` | Build graph and cache rules |
| `WF-REL-*` | Release rules |
| `WF-GOV-*` | Governance rules |
| `WF-SEC-*` | Security and policy rules |

Gate 4 defines the issue model. Gate 5 assigns the first concrete load,
schema, and semantic rule IDs for the loader and validation pipeline.
Gate 12 adds YAML-specific load diagnostics, including unsupported YAML
features and duplicate YAML mapping keys, while preserving the existing
`WF-LOAD-*` family.
Gate 13 applies `WF-SCHEMA-*` diagnostics to dependency and capability registry
contract shape through runtime JSON Schema validation.
Gate 14 applies `WF-SCHEMA-*` diagnostics to asset registry contract shape.
Path existence, file-type, and packaging checks remain future `WF-ASSET-*`
semantic diagnostics.
Gate 15 adds the first concrete `WF-ASSET-*` diagnostics:
`WF-ASSET-001` for asset source escape, `WF-ASSET-002` for missing required
source files, `WF-ASSET-003` for non-game-relative target paths, and
`WF-ASSET-004` for target extension mismatch.
Gate 16 adds `WF-ASSET-005` for source signature mismatch and `WF-ASSET-006`
for target root mismatch.
Gate 17 adds `WF-ASSET-007` for invalid voice target shape, `WF-ASSET-008`
for incomplete WAV/OGG voice pairs, and `WF-ASSET-009` for missing LIP pairs.
Gate 18 adds `WF-SEM-015` for dialogue voice worklist entries missing declared
voice/lip assets.
Gate 19 adds `WF-SEM-016` for dialogue `questId` references that are not
declared in the quest registry.
Gate 20 adds `WF-SEM-017` for quest objective stage references that are not
declared in the same quest.
Gate 21 adds `WF-SEM-018` for quest transition stage references that are not
declared in the same quest.
Gate 22 adds `WF-SEM-019` for quest condition stage references that are not
declared in the same quest.
Gate 23 adds `WF-SEM-020` for quest result-script condition references that
are not declared in the same quest.
Gate 24 adds `WF-SEM-021` for quest condition variable references that are not
declared in the same quest.
Gate 25 adds `WF-SEM-022` for dialogue condition quest-stage references and
`WF-SEM-023` for dialogue condition quest-variable references that are not
declared inside the dialogue line's referenced quest.
Gate 26 adds no new `WF-SEM-*` rule; invalid dialogue result-script shape is
covered by `WF-SCHEMA-001`.
Gate 27 adds `WF-SEM-024` for dialogue line topic references that are not
declared in dialogue topics and `WF-SEM-025` for dialogue `linkTo` target
topic references that are not declared in dialogue topics.
Gate 28 adds `WF-SEM-026` for dialogue quest gates that reference undeclared
quests, `WF-SEM-027` for dialogue quest gate stage references that are not
declared inside the gate quest, and `WF-SEM-028` for dialogue quest gate
variable references that are not declared inside the gate quest.
Gate 29 adds `WF-SEM-029` for dialogue result-script mutation variable
references that are not declared inside the dialogue line's referenced quest.
Gate 30 adds `WF-SEM-030` for dialogue `linkFrom` source topic references
that are not declared in dialogue topics.
Gate 31 adds `WF-SEM-031` for dialogue `linkTo` target topics with no authored
dialogue line endpoint and `WF-SEM-032` for dialogue `linkFrom` source topics
with no authored dialogue line endpoint.
Gate 32 adds `WF-SEM-033` for duplicate dialogue prompt routes with the same
`topicId`, `promptText`, and `priority`.
Gate 33 adds no new `WF-SEM-*` rule; invalid dialogue Speech Challenge shape
is covered by `WF-SCHEMA-001`.
Gate 34 adds no new `WF-SEM-*` rule; invalid dialogue skill gate shape is
covered by `WF-SCHEMA-001`.
Gate 35 adds no new `WF-SEM-*` rule; invalid dialogue perk gate shape is
covered by `WF-SCHEMA-001`.
Gate 36 adds no new `WF-SEM-*` rule; invalid dialogue faction and reputation
gate shape is covered by `WF-SCHEMA-001`.
Gate 37 adds no new `WF-SEM-*` rule; invalid dialogue identity gate shape is
covered by `WF-SCHEMA-001`.
Gate 38 adds no new `WF-SEM-*` rule; invalid dialogue local world flag gate
shape is covered by `WF-SCHEMA-001`.
Gate 39 adds no new `WF-SEM-*` rule; invalid dialogue event history gate shape
is covered by `WF-SCHEMA-001`.
Gate 40 adds no new `WF-SEM-*` rule; invalid dialogue companion state gate
shape is covered by `WF-SCHEMA-001`.
