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
