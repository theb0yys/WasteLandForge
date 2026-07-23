# Dead Air — Opening Slice Test Plan

## Clean-save tests

Run each test on a save that has never loaded `DeadAir.esp`.

### Normal approach

1. Approach Lone Wolf Radio through the intended trigger volume.
2. Confirm the broadcast appears once.
3. Confirm stage 10 and objective 10.
4. Inspect the new receiver.
5. Confirm stage 20, objective 10 complete, and objective 20 active.
6. Speak to Mara.
7. Accept.
8. Confirm `OpeningPhase = 1`, `MaraTrust = 1`, stage 30, objective 20 complete, and objective 30 active.

### Trigger bypass

1. Reach the receiver without crossing the normal approach trigger.
2. Activate it.
3. Confirm the fallback starts the quest and reaches stage 20 without duplicate messages or stage regression.

### Temporary refusal

1. Reach stage 20.
2. Refuse Mara.
3. End dialogue.
4. Confirm stage remains 20 and `OpeningPhase = 0`.
5. Speak to Mara again and confirm the offer remains available.
6. Accept and confirm the normal stage-30 result.

### Repeated activation

At stages 20 and 30, activate the receiver repeatedly.

Confirm:

- no objective is redisplayed as new;
- no variable is reset;
- no stage regresses;
- no repeated broadcast reward or message stack occurs;
- only the repeat static text appears.

### Repeated conversation

After acceptance, speak to Mara repeatedly.

Confirm:

- acceptance is not available again;
- `OpeningPhase` remains 1;
- `MaraTrust` remains 1;
- first-victim information remains available;
- stage remains 30.

### Science gate

At Science 44, confirm the Science line is unavailable.

At Science 45 or greater, confirm it appears, provides the intended explanation, and does not change stage, `OpeningPhase`, or `MaraTrust`.

## World-preservation tests

Compare the location with and without `DeadAir.esp`.

Confirm:

- all vanilla loot remains present and reachable;
- no vanilla radio or container changed;
- the map marker and fast-travel arrival are unobstructed;
- Mara does not block the trailer or receiver;
- Mara's sandbox does not move her onto the road;
- no landscape seam or navmesh change exists;
- nearby wildlife cannot permanently kill Mara during this slice.

## Console-state checks

Use console inspection only for testing:

- `GetStage DAQDeadAir`
- inspect `DAQDeadAir.OpeningPhase`
- inspect `DAQDeadAir.MaraTrust`
- confirm objective display state after stages 10, 20, and 30

Do not ship debug scripts, debug messages, or console-dependent progression.

## xEdit acceptance

The plugin fails review if any of these appear:

- master other than `FalloutNV.esm`;
- deleted reference;
- ITM record;
- vanilla base-object override;
- navmesh record;
- landscape record;
- worldspace override;
- map-marker override;
- existing container or loot override;
- accidental Eli Venn encounter records.

Expected new-record families include the quest, message, scripts, activator, placed trigger/receiver, Mara NPC/reference, package, dialogue topics, and dialogue infos.

## Milestone pass condition

The milestone passes when all normal, bypass, refusal, repeat, Science, preservation, and xEdit tests succeed and stage 30 is the furthest reachable stage.
