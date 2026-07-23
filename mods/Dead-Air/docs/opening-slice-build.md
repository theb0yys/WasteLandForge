# Dead Air — Opening Playable Slice Build Contract

## Milestone boundary

This slice begins when the Courier approaches Lone Wolf Radio and ends when stage 30 displays the objective to reach Corporal Eli Venn. The actual first-prediction encounter is not part of this build.

## Player flow

1. Enter the approved approach volume.
2. Hear vanilla radio static and receive the broken emergency message.
3. Stage 10 begins and objective 10 appears.
4. Inspect the new emergency receiver beside the trailer.
5. Stage 20 begins; objective 10 completes and objective 20 appears.
6. Speak with Mara.
7. Ask about the names, the relay, or use Science 45.
8. Accept or refuse temporarily.
9. Acceptance sets `OpeningPhase` to 1, raises `MaraTrust` by 1, and sets stage 30.
10. Mara identifies Corporal Eli Venn and objective 30 appears.

## State matrix

| State | Stage | OpeningPhase | Active objective | Mara behaviour |
|---|---:|---:|---|---|
| Not started | below 10 | 0 | none | no quest dialogue |
| Broadcast heard | 10 | 0 | 10 — inspect receiver | waiting at Lone Wolf |
| Receiver inspected | 20 | 0 | 20 — speak to Mara | opening offer available |
| Refused temporarily | 20 | 0 | 20 — speak to Mara | offer remains available |
| Investigation accepted | 30 | 1 | 30 — reach Eli Venn | first-victim briefing available |
| Future encounter | 40 | 1 | later Act II state | outside this slice |

No valid route may decrease the stage or change `OpeningPhase` from 1 back to 0.

## Record ownership

Every new record belongs to `DeadAir.esp`. No existing `FalloutNV.esm` record is converted into a quest object.

The only allowed exterior-cell changes are new placed references:

- approach trigger;
- emergency receiver;
- Mara;
- any quest-owned marker needed solely for local authoring inspection.

No marker for Eli Venn is included until the encounter location is selected and verified.

## Objective mapping

| GECK index | Text | Displayed | Completed |
|---:|---|---|---|
| 10 | Investigate the emergency transmission at Lone Wolf Radio. | stage 10 | stage 20 |
| 20 | Speak to Mara Voss. | stage 20 | stage 30 |
| 30 | Reach Corporal Eli Venn before the predicted time. | stage 30 | stage 40, in a later build |

## Start logic contract

The approach trigger is the normal start. Receiver activation is the fallback.

Pseudo-logic:

```text
on player entry:
    when quest stage < 10:
        start DAQDeadAir
        set stage 10
        retire this trigger
```

Receiver pseudo-logic:

```text
on activate:
    when quest stage < 10:
        start DAQDeadAir
        set stage 10
    when quest stage == 10:
        set stage 20
    otherwise:
        show repeat static text only
```

The implementation must not rely on a continuously running quest script.

## Acceptance logic contract

Pseudo-logic:

```text
when OpeningPhase == 0:
    OpeningPhase += 1
    MaraTrust += 1
    set DAQDeadAir stage 30
```

The GECK dialogue result must preserve this order: close the repeatable opening route, record trust, then advance the stage.

## Dialogue topology

```text
Greeting
├─ Ask about names ─┐
├─ Ask about relay ─┼─> Accept ─> First-victim briefing
├─ Science 45 ──────┘
└─ Refuse temporarily ─> conversation closes, offer remains
```

The first-victim briefing states:

- victim: Corporal Eli Venn;
- role: NCR signal corps;
- destination: a field repeater on the old highway;
- predicted time: 18:40;
- phrase: `line failure`.

The briefing does not create, kill, move, or enable Eli Venn.

## Mara placement contract

Mara must be visible from the receiver without blocking the receiver, trailer entrance, road, containers, or existing loot. Her 256-unit sandbox must be tested for:

- pathing into clutter;
- wandering onto the road;
- combat with nearby wildlife;
- collision trapping;
- leaving conversation range;
- occupying the player's activation position.

If the existing navmesh cannot support the placement cleanly, move the new reference. Do not edit the navmesh for this slice.

## Development presentation

- All dialogue uses text and subtitles during development.
- No custom audio files are added.
- A locally verified vanilla static sound may play for the broadcast.
- No claim is made that a vanilla voice type contains matching spoken lines.

## Completion evidence

The slice is complete only when the saved plugin, Forge registries, GECK screenshots/notes, and xEdit inspection all describe the same:

- quest stages;
- variable names and initial values;
- objective indices and text;
- new record EditorIDs;
- Mara placement;
- dialogue route;
- stage-30 ending boundary.
