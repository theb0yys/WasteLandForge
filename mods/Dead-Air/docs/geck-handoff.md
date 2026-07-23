# Dead Air — Initial GECK Handoff

This handoff covers only the opening playable slice. It does not authorize changes to unrelated vanilla records.

## Local identity verification

Before editing, verify through the local game catalogue or xEdit:

- the Lone Wolf Radio exterior cell identity;
- the Lone Wolf Radio location/map-marker identity;
- the existing trailer, radio clutter, containers, loot, wildlife, and water-source references;
- safe placement coordinates for Mara and the quest-start interaction;
- whether any chosen reference is persistent, initially disabled, owned, or linked by vanilla scripts.

Public reference notes identify the cell name as `SLLoneWolfRadio` and a location reference as `000E19F5`, but these values remain provisional until checked against the installed `FalloutNV.esm`.

## Plugin

Create a new plugin:

- Filename: `DeadAir.esp`
- Master: `FalloutNV.esm`
- No additional masters
- Do not make the plugin an ESM

## Quest record

Create or verify:

- Quest EditorID: `DAQDeadAir`
- Quest name: `Dead Air`
- Start game enabled: false
- Priority: choose during GECK review; do not infer from the Forge dialogue priority
- Stages required for the first slice: 10, 20, 30, and 40
- Quest variables: `EvidenceCount`, `MaraTrust`, `OracleDisposition`, `FinalNamesSaved`

## Opening trigger

Use a new quest-owned activator or trigger placed near the existing radio equipment. Do not convert an existing vanilla radio, container, marker, or clutter reference into a quest-owned object.

First-slice behaviour:

1. The player investigates the emergency frequency.
2. Vanilla radio/static audio may play.
3. Quest stage 10 starts and the objective `Go to Lone Wolf Radio` becomes active.
4. Reaching the intended area advances to stage 20.
5. Speaking with Mara and accepting the investigation advances to stage 30 and then exposes the first-prediction objective.

The trigger must be idempotent and must not restart or regress the quest.

## Mara Voss

Create a new human NPC using vanilla assets only.

Provisional EditorIDs:

- NPC: `DANPCMaraVoss`
- Reference: `DAREFMaraVoss`
- Voice type: select an appropriate vanilla-compatible voice during GECK review; do not invent a voice FormID in Forge

Initial placement:

- outdoors at Lone Wolf Radio;
- outside the trailer's narrow interior footprint;
- clear of the existing containers, mattress, skill book, bottle cap, radio clutter, and player movement path;
- no ownership changes to vanilla objects;
- a small quest-owned sandbox radius only.

## Dialogue

Create the four declared topics and six opening lines from `src/registries/dialogue/main.json`.

Required first-slice routes:

- greeting;
- ask what the broadcast said;
- ask whether Lone Wolf is transmitting it;
- Science 45 deduction;
- accept the investigation;
- decline temporarily without failing the quest.

The Science route provides information but must not bypass the first investigation scene.

## Scripts

Use vanilla GECK quest/dialogue scripts only.

Required script intent:

- start stage 10 once;
- advance stage 20 when the player reaches the approved Lone Wolf trigger area;
- advance stage 30 after the accepted Mara route;
- leave Mara available after temporary refusal;
- initialize all four quest variables to zero;
- avoid polling when event-driven conditions are sufficient.

Exact script text must be authored and compiled in GECK, then reviewed separately.

## Vanilla preservation checks

Before saving:

- no vanilla reference was deleted or disabled;
- no vanilla base object was edited;
- no existing container ownership or contents changed;
- no existing loot was moved;
- no landscape or navmesh edit was introduced for the opening slice;
- no new master was added;
- no persistent reference was unintentionally overridden.

## xEdit review

After saving `DeadAir.esp`, inspect it in xEdit and confirm:

- master list contains only `FalloutNV.esm`;
- expected new records only;
- no ITM records against Lone Wolf Radio;
- no deleted references;
- no accidental cell/worldspace or navmesh overrides;
- quest, NPC, placed reference, dialogue, and script records belong to `DeadAir.esp`;
- plugin bytes are imported into Forge only after this review evidence is recorded.
