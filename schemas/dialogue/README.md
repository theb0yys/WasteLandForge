# Dialogue Registry Schemas

Dialogue registry schemas validate source-controlled dialogue lines and voice
worklist declarations for WastelandForge projects.

Gate 18 adds `0.1.0/schema.json` as an immutable Draft 2020-12 public schema.
The schema is a skeleton: it captures dialogue line identity, quest/topic
ownership, response text, and optional voice worklist fields. Full GECK
condition language, result script modeling, and plugin record compilation
remain later narrative gates.

Gate 19 validates dialogue `questId` references against declared quest IDs when
the project manifest declares a quest registry.

Gate 25 adds `0.2.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local dialogue `conditions` declarations for quest-stage and
quest-variable checks against the line's `questId`. Full GECK dialogue
condition language, boolean composition, quest-level dialogue gates, result
scripts, topic linking, and plugin record compilation remain later narrative
gates.

Gate 26 adds `0.3.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local dialogue `resultScripts` declarations with
`dialogueResult` type. Raw script bodies, mutation semantics, result-script
execution, topic linking, dialogue graph traversal, and plugin record
compilation remain later narrative gates.

Gate 27 adds `0.4.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal dialogue `topics` declarations and line-local `links` arrays that
model `linkTo` target topic references. Full `Link From` semantics, dialogue
graph traversal, cycle checks, ordering, priority, response routing, and
plugin record compilation remain later narrative gates.

Gate 28 adds `0.5.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal top-level `questGates` declarations keyed by `questId`, with
quest-stage and quest-variable condition skeletons. Full GECK dialogue
condition language, boolean composition, gate execution ordering, generated
plugin records, and condition compilation remain later narrative gates.

Gate 29 adds `0.6.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal dialogue result-script `mutations` declarations for
`questVariableIncrement` against the dialogue line's referenced quest. Raw
script bodies, full result-script mutation semantics, quest stage mutation,
execution ordering, generated plugin records, and condition compilation remain
later narrative gates.

Gate 30 adds `0.7.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal dialogue `linkFrom` declarations with `sourceTopicId` references.
Full dialogue graph traversal, cycle checks, ordering, priority, response
routing, generated plugin records, and exact GECK link field behavior remain
later narrative and tooling gates.

Gate 31 adds no new schema. It validates a derived dialogue link graph over
schema-valid `0.7.0` documents by checking that declared `linkTo` target topics
and declared `linkFrom` source topics also have authored dialogue line
endpoints. Reciprocal link requirements, traversal, cycle checks, ordering,
priority, response routing, generated plugin records, and exact GECK link field
behavior remain later narrative and tooling gates.

Gate 32 adds `0.8.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds explicit line `priority` declarations and requires `priority` when a line
declares `promptText`, so prompt routes are explicit source state rather than
implicit defaults. Exact GECK priority range, priority ordering, prompt routing
execution, condition-aware prompt selection, response routing, and generated
plugin records remain later narrative and tooling gates.

Gate 33 adds `0.9.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `speechChallenge` declarations with `skill: "speech"`
and an explicit integer `threshold`. Exact GECK threshold ranges, success and
failure routing, challenge display behavior, evaluation semantics, non-Speech
skill gates, and generated plugin records remain later narrative and tooling
gates.

Gate 34 adds `0.10.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `skillGates` declarations with explicit skill keys and
integer thresholds. Exact game skill taxonomy, GECK condition mapping,
threshold ranges, evaluation semantics, Speech Challenge integration,
non-skill gates, and generated plugin records remain later narrative and
tooling gates.

Gate 35 adds `0.11.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `perkGates` declarations with explicit authored perk
keys. Exact perk taxonomy, perk registry resolution, GECK condition mapping,
evaluation semantics, non-perk gates, and generated plugin records remain
later narrative and tooling gates.

Gate 36 adds `0.12.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `factionGates` and `reputationGates` declarations,
keeping operational faction relation separate from social reputation standing.
Exact faction registry resolution, relation taxonomy, reputation standing
taxonomy, GECK condition mapping, evaluation semantics, and generated plugin
records remain later narrative and tooling gates.

Gate 37 adds `0.13.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `identityGates` declarations with explicit authored
identity keys. Exact identity taxonomy, actor/player identity mapping, GECK
condition mapping, evaluation semantics, and generated plugin records remain
later narrative and tooling gates.

Gate 38 adds `0.14.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `worldFlagGates` declarations with explicit authored
local world flag keys and expected states. Exact world-state registry
resolution, local world flag taxonomy, boolean/value modeling, scope semantics,
GECK condition mapping, evaluation semantics, and generated plugin records
remain later narrative and tooling gates.

Gate 39 adds `0.15.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `eventHistoryGates` declarations with explicit
authored event history signal keys and expected states. Exact event-history
registry resolution, event signal taxonomy, lifecycle modeling, consumer
routing, time horizon semantics, GECK condition mapping, evaluation semantics,
and generated plugin records remain later narrative and tooling gates.

Gate 40 adds `0.16.0/schema.json` as a new immutable Draft 2020-12 schema. It
adds minimal line-local `companionStateGates` declarations with explicit
authored companion keys and companion state keys. Exact companion registry
resolution, companion-specific observer models, companion state taxonomy,
trust/history/trigger value semantics, GECK condition mapping, evaluation
semantics, and generated plugin records remain later narrative and tooling
gates.
