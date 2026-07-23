# Dead Air — Quest Design

## Premise

A dormant emergency network begins broadcasting names shortly before those people die. The opening transmission names an NCR communications officer, a Followers doctor, Mara Voss, Jonas Creed, and the Courier.

The system is not supernatural. ORACLE combines intercepted movement, medical, supply, military, and radio data to estimate casualty risk. Broadcasting the predictions changes human behaviour and can make the predicted deaths more likely.

## Tone

- Mojave conspiracy thriller
- grounded pre-war technology rather than magic
- morally credible NCR and Followers positions
- investigation before combat
- choices that change evidence, trust, and endings

## Main cast

### Mara Voss

A former NCR radio technician. Sharp, guarded, and practical. She recognizes the broadcast format and understands how the Mojave's dead relay network can be abused.

### Jonas Creed

A former Followers analyst operating ORACLE. He believes public predictions reveal that fear governs the Mojave more reliably than politics or morality.

### Lieutenant Hale

Leader of the NCR recovery team. Hale wants ORACLE contained and used for military casualty prevention. His case should remain understandable even when the player opposes him.

### Doctor Imani Wells

A Followers physician who wants ORACLE converted into a public early-warning system. She accepts that predictions can cause panic but believes secrecy is worse.

### Corporal Eli Venn

An NCR signal-corps technician assigned to maintain an isolated field repeater on the old highway. He is the first named victim investigated by the Courier. The broadcast gives a time—18:40—and the phrase `line failure`, which can describe communications equipment, electrical infrastructure, or a death.

## Act structure

### Act I — Voices After Midnight

1. A broken emergency transmission plays as the Courier approaches Lone Wolf Radio.
2. The Courier inspects a quest-owned receiver beside the abandoned trailer.
3. Mara explains that Lone Wolf is catching spill from a stronger relay.
4. The Courier may question the signal, make a Science 45 deduction, accept the investigation, or refuse temporarily.
5. Accepting reveals Corporal Eli Venn as the first target and activates the objective to reach him before the predicted time.

### Act II — The Quiet Frequency

1. Three prediction scenes are investigated.
2. Each scene supports both a mundane explanation and the appearance of prophecy.
3. Evidence reveals deliberate surveillance and a shared relay path.
4. Mara's trust changes according to the Courier's methods and conclusions.

### Act III — Station CINDER

1. The Courier locates the buried command bunker.
2. ORACLE's casualty model and pre-war purpose are uncovered.
3. Jonas Creed explains the experiment.
4. An NCR recovery team arrives as ORACLE issues its final broadcast.

### Act IV — Dead Air

The Courier decides whether to destroy, expose, surrender, repurpose, or control ORACLE. A separate high-difficulty route allows the Courier to save every person named in the final broadcast.

## Opening-state contract

`openingphase` is reserved as:

- `0` — Mara's investigation offer remains open;
- `1` — the Courier accepted, the first-victim briefing is available, and acceptance cannot repeat.

Temporary refusal does not change `openingphase`. Acceptance increases `maratrust` by one and advances `DAQDeadAir` to stage 30 in GECK.

## Ending values

`oracledisposition` is reserved as:

- `0` — unresolved
- `1` — destroyed
- `2` — exposed publicly
- `3` — surrendered to NCR
- `4` — surrendered to Followers
- `5` — controlled by the Courier
- `6` — prophecy deliberately fulfilled
- `7` — final prediction broken

## Asset policy

Use only vanilla architecture kits, doors, terminals, radios, antenna pieces, furniture, clutter, lighting, effects, weapons, outfits, creatures, and sounds. New source content may include plugin records, scripts, quest text, terminal text, notes, and dialogue declarations, but no new models or textures are planned.

The opening slice is intentionally unvoiced during development. It does not add custom voice audio or silently claim that a compatible vanilla voice asset exists. Voice presentation remains a later production decision.

## Opening playable slice

The first playable milestone ends when stage 30 activates the Eli Venn objective:

1. create `DAQDeadAir` with stages 10, 20, 30, and the stage-40 handoff;
2. start stage 10 once from a quest-owned approach trigger;
3. inspect a quest-owned emergency receiver to reach stage 20;
4. place Mara at Lone Wolf Radio without altering existing content;
5. provide the linked opening interaction and Science 45 line;
6. leave temporary refusal reopenable;
7. make acceptance idempotent through `openingphase`;
8. advance to stage 30 and display `Reach Corporal Eli Venn before the predicted time`;
9. stop before building the actual Eli Venn encounter;
10. verify the plugin in xEdit before expanding Act II.
