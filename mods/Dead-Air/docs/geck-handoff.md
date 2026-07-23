# Dead Air — Opening Slice GECK Handoff

This handoff authorizes only the opening playable slice on branch `mod/dead-air-foundation`. It does not authorize unrelated vanilla edits or the full Eli Venn encounter.

## Local identity verification

Before editing, verify through the installed `FalloutNV.esm`, the local game catalogue, or xEdit:

- the Lone Wolf Radio exterior cell identity;
- the existing trailer, map marker, radio clutter, containers, loot, wildlife, water source, and environmental references;
- a clear approach-trigger volume that does not overlap the fast-travel arrival point;
- a safe receiver position beside the existing equipment without replacing any vanilla reference;
- a safe standing and sandbox area for Mara;
- the exact vanilla static sound and visual assets selected for the quest-owned receiver;
- whether any nearby vanilla reference is persistent, initially disabled, owned, or script-linked.

Public notes may call the cell `SLLoneWolfRadio`, but all names and FormIDs remain provisional until checked locally.

## Plugin

Create or continue:

- Filename: `DeadAir.esp`
- Master: `FalloutNV.esm`
- No additional masters
- ESP flag only
- No script-extender dependency
- No custom model, texture, animation, interface, or voice asset

## Authorized new records

| Record | EditorID | Purpose |
|---|---|---|
| Quest | `DAQDeadAir` | Main quest and variables |
| Message | `DAMESGBroadcast` | Text presentation of the broken emergency transmission |
| Approach trigger/reference | `DAREFBroadcastApproach` | Starts stage 10 once when the player approaches |
| Receiver activator | `DAACTEmergencyReceiver` | Quest-owned radio receiver using a verified vanilla model |
| Receiver reference | `DAREFEmergencyReceiver` | Inspectable object beside the existing radio equipment |
| Start script | `DAStartBroadcastSCRIPT` | Idempotent quest-start logic |
| Receiver script | `DAEmergencyReceiverSCRIPT` | Advances stage 10 to stage 20 |
| NPC | `DANPCMaraVoss` | Mara's new actor base |
| Actor reference | `DAREFMaraVoss` | Persistent Mara placement |
| Package | `DAPKMaraLoneWolfSandbox` | Small local sandbox package |
| Dialogue topics/infos | GECK-generated IDs under the declared topics | Opening conversation |
| Future target identity only | `DANPCEliVenn` / `DAREFEliVenn` | Reserved for the later first-prediction build; do not create or place yet |

Do not duplicate or edit an existing vanilla radio base merely to attach quest state. The new receiver may reuse a vanilla model path after local verification, but it must remain a new `DeadAir.esp` base and reference.

## Quest record

Create or verify:

- Quest EditorID: `DAQDeadAir`
- Quest name: `Dead Air`
- Start game enabled: false
- Priority: 55
- Stages for this slice: 10, 20, 30, and stage-40 handoff
- Objective indices:
  - 10 — `Investigate the emergency transmission at Lone Wolf Radio.`
  - 20 — `Speak to Mara Voss.`
  - 30 — `Reach Corporal Eli Venn before the predicted time.`
- Quest variables:
  - `EvidenceCount = 0`
  - `MaraTrust = 0`
  - `OpeningPhase = 0`
  - `OracleDisposition = 0`
  - `FinalNamesSaved = 0`

## Stage-result intent

### Stage 10 — The broadcast

- initialize all five variables only when the quest has not previously started;
- show `DAMESGBroadcast`;
- play one locally verified vanilla radio-static cue;
- display objective 10;
- do not set stage 20 automatically.

Message text:

> EMERGENCY CHANNEL // PARTIAL SIGNAL  
> VENN, ELI — 18:40 — LINE FAILURE  
> WELLS, IMANI — 02:15 — RESPIRATORY  
> VOSS, MARA — [CORRUPTED]  
> COURIER — [SIGNAL LOST]

### Stage 20 — Lone Wolf Radio

- complete objective 10;
- display objective 20;
- enable Mara's opening dialogue conditions;
- do not change `OpeningPhase`.

### Stage 30 — Mara Voss

- complete objective 20;
- display objective 30;
- leave the objective without a map target until the Eli Venn scene is authored;
- do not create a misleading marker at an arbitrary location.

### Stage 40 — First prediction handoff

- reserved for completion of the later Eli Venn encounter;
- no stage-40 trigger or objective completion is authorized in this slice.

## Approach trigger

Place `DAREFBroadcastApproach` far enough from the trailer that stage 10 fires before the player reaches the receiver or Mara, but outside the fast-travel arrival footprint.

Required behaviour:

1. react only to the player;
2. if `DAQDeadAir` is below stage 10, start the quest and set stage 10;
3. never restart, reset, or regress the quest;
4. disable or permanently mark itself complete after firing;
5. perform no vanilla reference edits.

The receiver must provide a fallback start path if the approach trigger is bypassed.

## Emergency receiver

Create `DAACTEmergencyReceiver` as a new quest-owned activator using a locally verified vanilla radio/receiver model. Place `DAREFEmergencyReceiver` beside the existing equipment with enough separation that the player can clearly identify it as a new inspectable object.

Activation behaviour:

1. if the quest is below stage 10, start it and set stage 10;
2. if the quest is exactly stage 10, set stage 20;
3. at stage 20 or higher, show a short non-progressing status message;
4. never activate, disable, move, or take ownership of the existing vanilla radio clutter.

Suggested repeat text:

> The receiver carries only a thin wash of static. Someone has already pulled the useful part of the signal apart.

## Mara Voss

Create `DANPCMaraVoss` with vanilla assets only.

Opening-slice actor contract:

- adult human woman;
- unaffiliated former NCR technician, not an active NCR faction member;
- initially enabled;
- essential for the opening slice so random wildlife cannot kill her before her later predicted-death branch exists;
- neutral toward the player;
- unaggressive;
- average confidence;
- small vanilla utility/repair outfit;
- modest vanilla sidearm and ammunition;
- no unique model, texture, hairstyle, armour, weapon, animation, or voice asset;
- development dialogue remains unvoiced.

Place `DAREFMaraVoss` outdoors near the trailer, outside the narrow trailer footprint and clear of existing containers, mattress, skill book, bottle cap, radio clutter, water source, and player path.

Reference contract:

- persistent;
- no enable parent tied to a vanilla object;
- linked only to `DAQDeadAir` records created by this plugin;
- sandbox radius no greater than 256 units;
- sandbox package must keep her away from the road and existing loot;
- no navmesh edit for this slice.

## Dialogue implementation

Create the five declared topics and eight lines from `src/registries/dialogue/main.json`.

Opening conditions:

- `GetStage DAQDeadAir >= 20`
- `DAQDeadAir.OpeningPhase == 0`

Acceptance result intent:

1. guard against repeat execution;
2. set `DAQDeadAir.OpeningPhase` from 0 to 1;
3. add 1 to `DAQDeadAir.MaraTrust`;
4. set `DAQDeadAir` to stage 30;
5. route to the first-victim briefing.

Temporary refusal:

- changes no quest variable;
- changes no quest stage;
- leaves Mara's offer available in the next conversation.

Science route:

- player Science at least 45;
- reveals that the prediction system must be consuming movement, medical, supply, and radio data;
- does not change the stage;
- does not bypass acceptance;
- does not repeatably award trust in this slice.

First-victim briefing conditions:

- `GetStage DAQDeadAir >= 30`
- `DAQDeadAir.OpeningPhase == 1`

The briefing identifies Corporal Eli Venn, the predicted time `18:40`, and the phrase `line failure`. It activates no encounter logic beyond stage 30 and objective 30.

## Script safety rules

- all start and acceptance paths are idempotent;
- all stage changes move forward only;
- no polling quest script when an activation, trigger, or dialogue result can perform the change;
- no `GameMode` loop for the opening slice;
- no vanilla base-object script attachment;
- no disable/delete call against a vanilla reference;
- compile all scripts in GECK and resolve syntax errors there;
- record the final compiled script identities in the xEdit review notes.

## Vanilla preservation checks

Before saving:

- no vanilla reference was deleted, disabled, moved, renamed, or ownership-edited;
- no vanilla base object was edited;
- no existing container ownership or contents changed;
- no existing loot was moved;
- no landscape, worldspace, navmesh, encounter-zone, or map-marker edit was introduced;
- no new master was added;
- no persistent vanilla reference was unintentionally overridden;
- no Eli Venn scene record was accidentally included.

## xEdit review

After saving `DeadAir.esp`, confirm:

- master list contains only `FalloutNV.esm`;
- expected new records only;
- no ITM records against Lone Wolf Radio;
- no deleted references;
- no accidental cell/worldspace or navmesh overrides;
- quest, message, trigger, receiver, Mara, package, dialogue, and script records belong to `DeadAir.esp`;
- `DAQDeadAir` contains stages 10, 20, 30, and the unused stage-40 handoff;
- objective indices and text match this handoff;
- `OpeningPhase` prevents repeated acceptance;
- plugin bytes are imported into Forge only after review evidence is recorded.
