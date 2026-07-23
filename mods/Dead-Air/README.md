# Dead Air

**Dead Air** is a Fallout: New Vegas mystery quest mod built with Wasteland Forge.

A midnight emergency transmission predicts three deaths and names the Courier as its final target. The signal trail begins at Lone Wolf Radio and leads toward the buried pre-war Station CINDER and its casualty-prediction system, ORACLE.

## Project identity

- Planned plugin: `DeadAir.esp`
- Quest EditorID: `DAQDeadAir`
- Forge project: `io.github.theboyyss.deadair`
- Base location: Lone Wolf Radio
- Content rule: vanilla Fallout: New Vegas models, textures, clutter, architecture, sounds, weapons, clothing, and effects
- Required runtime extensions: none declared

## Current foundation

The first Forge slice contains:

- the complete main-quest stage spine;
- objectives from the first broadcast through the ORACLE decision;
- evidence, Mara trust, final-choice, and survivor-count variables;
- Mara Voss's opening conversation at Lone Wolf Radio;
- a Science 45 investigation route;
- GECK and xEdit authoring capabilities without script-extender requirements.

## Authority boundary

Forge owns the source registries, validation, worklists, provenance, packaging, and review evidence. GECK remains responsible for creating `DeadAir.esp`, the quest/NPC/dialogue records, placements, packages, and compiled vanilla scripts. xEdit remains responsible for independent plugin inspection.

## Vanilla-location rule

The mod may add deliberately placed quest content around Lone Wolf Radio, but it must not remove, relocate, disable, or repurpose its existing loot, containers, radio clutter, map marker, wildlife, water source, or environmental storytelling. Exact vanilla record identities must be verified locally before GECK authoring.
