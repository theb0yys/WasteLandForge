# Dead Air

**Dead Air** is a Fallout: New Vegas mystery quest mod built with Wasteland Forge.

A broken emergency transmission predicts several deaths and names the Courier. The first signal is intercepted at Lone Wolf Radio, where former NCR radio technician Mara Voss is trying to trace a stronger relay hidden elsewhere in the Mojave.

## Project identity

- Planned plugin: `DeadAir.esp`
- Quest EditorID: `DAQDeadAir`
- Forge project: `io.github.theboyyss.deadair`
- Base location: Lone Wolf Radio
- First predicted victim: Corporal Eli Venn, NCR signal corps
- Content rule: vanilla Fallout: New Vegas models, textures, clutter, architecture, sounds, weapons, clothing, and effects
- Required runtime extensions: none declared

## Opening playable slice

Version `0.2.0` defines the first playable milestone:

1. a one-shot approach trigger starts the quest at stage 10;
2. the player inspects a quest-owned emergency receiver at Lone Wolf Radio and reaches stage 20;
3. Mara's opening conversation becomes available;
4. accepting her investigation sets the opening phase, increases Mara's trust, and advances the quest to stage 30;
5. the objective `Reach Corporal Eli Venn before the predicted time` becomes active;
6. temporary refusal leaves the offer available and does not regress or fail the quest.

The source registries now contain the stage-result intent, objective flow, opening-state guard, dialogue links, response routes, Science 45 route, acceptance mutations, and first-victim briefing.

## Authority boundary

Forge owns the source registries, validation, worklists, provenance, packaging, and review evidence. GECK remains responsible for creating `DeadAir.esp`, the quest/NPC/dialogue records, placements, packages, objective targets, and compiled vanilla scripts. xEdit remains responsible for independent plugin inspection.

## Vanilla-location rule

The mod may add deliberately placed quest content around Lone Wolf Radio, but it must not remove, relocate, disable, or repurpose its existing loot, containers, radio clutter, map marker, wildlife, water source, or environmental storytelling. Exact vanilla record identities and safe coordinates must be verified locally before GECK authoring.

## Build documents

- `docs/quest-design.md` — narrative authority and act structure
- `docs/geck-handoff.md` — record-level GECK worklist
- `docs/opening-slice-build.md` — stage, objective, script, NPC, and dialogue implementation contract
- `docs/opening-slice-test-plan.md` — in-game and xEdit acceptance checks
