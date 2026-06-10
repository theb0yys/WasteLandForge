# Diagnostic Model

Status: Skeleton
Research classification: Documented
Source: R004 / ADR-007 and R008 / ADR-011

WastelandForge diagnostic issue JSON is the canonical diagnostic model. Other outputs such as console text, Markdown, SARIF, and GitHub annotations are projections from the same issue data.

## Fields

- `ruleId`: stable WastelandForge rule ID, such as `WF-SEM-014`.
- `severity`: `error`, `warning`, or `note`.
- `category`: diagnostic category, such as `semantic`.
- `title`: short human-readable title.
- `message`: detailed diagnostic message.
- `projectId`: optional stable dotted lowercase project ID.
- `primaryLocation`: required source location.
- `relatedLocations`: optional supporting source locations.
- `suggestedFix`: optional remediation text.
- `docsUri`: optional documentation URI for the rule.
- `fingerprint`: optional stable identity for repeat diagnostics.

## Locations

JSON Pointer is the canonical location format for values inside source contracts. Human renderers may add friendlier displays later, but those are not the source of truth.

Locations can include:

- `file`
- `pointer`
- `line`
- `column`

## Diagnostic Reports

Gate 5 adds a deterministic report wrapper for validation runs:

- `formatVersion`: machine-readable output contract version.
- `tool`: tool metadata for the emitting command, including version once defined.
- `command`: command name that produced the payload.
- `project`: optional project metadata, including stable dotted lowercase project ID when available.
- `summary`: error, warning, and note counts.
- `issues`: sorted diagnostic issue objects.

Reports are the machine-readable output for `forge validate --format json`.

## Gate Ownership

Gate 4 creates the core C# model and deterministic JSON serialization.

Gate 10 adds SARIF 2.1.0 projection from the same canonical issue data.

Gate 11 adds Markdown summaries and GitHub workflow-command annotations from
the same canonical issue data. GitHub annotations include line and column only
when canonical `SourceLocation` includes line and column values; JSON Pointer
remains the canonical value-level location.

Gate 12 adds YAML source location mapping for YAML-backed source contracts.
Manifest schema diagnostics are emitted from runtime JSON Schema evaluation
against the normalized canonical JSON object, while locations still use the
canonical JSON Pointer and optional source line and column.

Gate 13 extends runtime schema diagnostics to dependency and capability
registry documents before semantic cross-registry validation runs.

Gate 14 extends runtime schema diagnostics to optional asset registry documents
when the manifest declares an asset registry root.

Gate 15 adds `WF-ASSET-*` semantic diagnostics for asset source and target path
rules after asset registry schema validation succeeds.

Gate 16 adds type-specific `WF-ASSET-*` semantic diagnostics for minimal source
file signatures and target-root conventions after asset path validation
succeeds.

Gate 17 adds voice and dialogue asset `WF-ASSET-*` semantic diagnostics for
voice/lip target shape and WAV/OGG/LIP pair checks using the asset registry.

Gate 18 extends runtime schema diagnostics to optional dialogue registry
documents and adds `WF-SEM-015` for dialogue voice worklist entries whose
declared voice/lip assets are missing.

Gate 19 extends runtime schema diagnostics to optional quest registry documents
and adds `WF-SEM-016` for dialogue `questId` values that do not resolve to a
declared quest ID.

Gate 20 adds quest registry schema `0.2.0` for stage and objective skeletons
and adds `WF-SEM-017` for quest objective stage references that do not resolve
inside the declaring quest.

Gate 21 adds quest registry schema `0.3.0` for transition skeletons and adds
`WF-SEM-018` for quest transition stage references that do not resolve inside
the declaring quest.

Gate 22 adds quest registry schema `0.4.0` for condition skeletons and adds
`WF-SEM-019` for quest condition stage references that do not resolve inside
the declaring quest.

Gate 23 adds quest registry schema `0.5.0` for stage result-script skeletons
and adds `WF-SEM-020` for quest result-script condition references that do not
resolve inside the declaring quest.

Gate 24 adds quest registry schema `0.6.0` for quest variable skeletons and
adds `WF-SEM-021` for quest condition variable references that do not resolve
inside the declaring quest.

Gate 25 adds dialogue registry schema `0.2.0` for line-local dialogue
condition skeletons and adds `WF-SEM-022` and `WF-SEM-023` for dialogue
condition quest-stage and quest-variable references that do not resolve inside
the dialogue line's referenced quest.

Gate 26 adds dialogue registry schema `0.3.0` for line-local dialogue
result-script skeletons. It adds no new semantic rule because the Gate 26
result-script fields do not reference other authored state; invalid
result-script shape is reported through `WF-SCHEMA-001`.

Gate 27 adds dialogue registry schema `0.4.0` for topic declarations and
minimal `linkTo` topic link declarations. It adds `WF-SEM-024` for dialogue
line topic references that do not resolve to declared topics, and
`WF-SEM-025` for dialogue link target topic references that do not resolve to
declared topics.

Gate 28 adds dialogue registry schema `0.5.0` for quest-level dialogue gate
declarations. It adds `WF-SEM-026` for dialogue quest gates whose `questId`
does not resolve to a declared quest, `WF-SEM-027` for quest-level dialogue
gate condition stage references that do not resolve inside the gate quest, and
`WF-SEM-028` for quest-level dialogue gate condition variable references that
do not resolve inside the gate quest.

Gate 29 adds dialogue registry schema `0.6.0` for dialogue result-script
quest-variable increment mutation declarations. It adds `WF-SEM-029` for
dialogue result-script mutation variable references that do not resolve inside
the dialogue line's referenced quest.

Gate 30 adds dialogue registry schema `0.7.0` for minimal dialogue `linkFrom`
source topic declarations. It adds `WF-SEM-030` for dialogue `linkFrom` source
topic references that do not resolve to declared dialogue topics.

Gate 31 adds no new schema. It adds derived dialogue link graph endpoint
validation over schema-valid dialogue `0.7.0` documents: `WF-SEM-031` for
`linkTo` target topics with no authored dialogue line, and `WF-SEM-032` for
`linkFrom` source topics with no authored dialogue line.

Gate 32 adds dialogue registry schema `0.8.0` for explicit line priority and
prompt route declarations. It adds `WF-SEM-033` for duplicate prompt routes
with the same `topicId`, `promptText`, and `priority`.

Gate 33 adds dialogue registry schema `0.9.0` for explicit line Speech
Challenge skeleton declarations. It adds no new semantic rule because the Gate
33 Speech Challenge fields do not yet reference other authored state and exact
threshold/evaluation semantics remain open; invalid Speech Challenge shape is
reported through `WF-SCHEMA-001`.

Gate 34 adds dialogue registry schema `0.10.0` for explicit line-local skill
gate skeleton declarations. It adds no new semantic rule because the Gate 34
skill gate fields do not yet reference a skill registry or other authored
state; invalid skill gate shape is reported through `WF-SCHEMA-001`.

Gate 35 adds dialogue registry schema `0.11.0` for explicit line-local perk
gate skeleton declarations. It adds no new semantic rule because the Gate 35
perk gate fields do not yet reference a perk registry or other authored state;
invalid perk gate shape is reported through `WF-SCHEMA-001`.

Gate 36 adds dialogue registry schema `0.12.0` for explicit line-local faction
relation and reputation standing gate skeleton declarations. It adds no new
semantic rule because the Gate 36 faction and reputation gate fields do not
yet reference faction or reputation registries; invalid faction/reputation gate
shape is reported through `WF-SCHEMA-001`.

Gate 37 adds dialogue registry schema `0.13.0` for explicit line-local
identity gate skeleton declarations. It adds no new semantic rule because the
Gate 37 identity gate fields do not yet reference an identity registry or other
authored state; invalid identity gate shape is reported through
`WF-SCHEMA-001`.

Gate 38 adds dialogue registry schema `0.14.0` for explicit line-local local
world flag gate skeleton declarations. It adds no new semantic rule because
the Gate 38 world flag gate fields do not yet reference a world-state registry
or other authored state; invalid local world flag gate shape is reported
through `WF-SCHEMA-001`.

Gate 39 adds dialogue registry schema `0.15.0` for explicit line-local event
history gate skeleton declarations. It adds no new semantic rule because the
Gate 39 event history gate fields do not yet reference an event-history
registry or other authored state; invalid event history gate shape is reported
through `WF-SCHEMA-001`.

Gate 40 adds dialogue registry schema `0.16.0` for explicit line-local
companion state gate skeleton declarations. It adds no new semantic rule
because the Gate 40 companion state gate fields do not yet reference a
companion registry or other authored state; invalid companion state gate shape
is reported through `WF-SCHEMA-001`.

Gate 5 creates the diagnostic report aggregate and uses it for loader and
validation pipeline output.
