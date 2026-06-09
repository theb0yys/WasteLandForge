---
name: wastelandforge-narrative-reactivity
description: Use this skill for WastelandForge quest, dialogue, faction, reputation, world state, companion memory, radio/news, codex, ending, and reactive narrative systems. It should trigger whenever a task mentions New Vegas-style branching, quest registries, dialogue registries, faction state, reputation events, world-state flags, consequence surfacing, or ADR-003.
---

# WastelandForge Narrative Reactivity

## Research base

Use:

- `Fallout New Vegas Narrative Systems and Reactive World Design-deep-research-report.md`.
- `Fallout New Vegas Engine and Technical Architecture-deep-research-report.md` for engine verification needs.
- `R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md` when shaping registry contracts.

## ADR-003 decision

Use hybrid state architecture. New Vegas reactivity is consequence-first, not simulation-first, and it is not one monolithic world-state ledger.

Model at least four layers:

- Quest State: progression, objectives, stages, lockouts, fallback routes, local branching.
- Faction State: social reputation and operational hostility as separate subdomains.
- World State: location-scale and regional resolutions consumed by later systems.
- Event History: consequential acts and observations recorded once, then consumed by dialogue, companions, radio/news, codex, achievements, and endings.

## Quest registry

Model quests as stateful hubs, not linear checklists.

Support:

- entry conditions,
- stage transitions,
- objectives,
- result scripts,
- lockout rules,
- soft points of no return,
- fallback paths,
- cross-arc reuse of local quest content,
- downstream claims on factions, locations, companions, or world outcomes.

Use the Yes Man path as the researched pattern for fault-tolerant branching with a fallback route, not as a generic "anything goes" excuse.

## Dialogue registry

Dialogue is gameplay state exposure, not just text storage.

Support a condition language over:

- quest stages,
- quest variables,
- faction reputation,
- faction hostility or relation,
- skills,
- perks,
- identity checks,
- local world flags,
- event history,
- companion state,
- result-script side effects.

Preserve the GECK pattern where quest-level conditions gate dialogue before info-level conditions. Support variable-driven conversations where a single topic advances through quest variables.

## Faction and reputation

Do not collapse faction relation and reputation.

- Factions govern actor reactions, combat hostility, crime, and relations.
- Reputation tracks fame/infamy and social standing.

WastelandForge should model social standing separately from operational hostility.

## Event history and consequence surfacing

Event history should be authored consequence signals, not simulated gossip by default.

Consumers include:

- conditional dialogue,
- companion observers,
- radio/news,
- codex entries,
- terminal/note/book content,
- achievements,
- ending summaries.

Support multiple time horizons: immediate feedback, delayed local response, broadcast/news, and deferred ending-slide style summaries.

## Open implementation warning

The narrative report says concrete schemas should still be verified against representative records in `FalloutNV.esm`: a major faction arc, a regional side-faction hub, a companion quest, a radio quest, and a multi-ending side quest. Do not treat exact record-shape schemas as final until that direct plugin inspection is done.
