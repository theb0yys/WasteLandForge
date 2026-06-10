# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 43 establishes
dialogue condition logic reference validation.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 43 creates:

- optional `registries.quests` manifest wiring,
- immutable Draft 2020-12 dialogue registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, `0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`,
  `0.10.0`, `0.11.0`, `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`,
  `0.17.0`, and `0.18.0`,
- immutable Draft 2020-12 quest registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, `0.4.0`, `0.5.0`, and `0.6.0`,
- runtime dialogue registry schema validation,
- runtime quest registry schema validation,
- a valid synthetic `ExampleMod` dialogue registry with line-local
  quest-stage and quest-variable condition declarations plus dialogue
  result-script declarations, topic declarations, `linkTo` and `linkFrom`
  topic links, explicit priority, prompt route, Speech Challenge, skill gate,
  perk gate, faction gate, reputation gate, identity gate, local world flag
  gate, event history gate, companion state gate, and result-script
  side-effect gate declarations plus condition boolean composition
  declarations, and a quest-level dialogue gate declaration plus a dialogue
  result-script quest-variable increment declaration,
- a valid synthetic `ExampleMod` quest registry with stage and objective
  declarations plus transition, condition, and stage result-script
  declarations plus quest variable declarations,
- `WF-SEM-016` cross-registry validation from dialogue `questId` values to
  declared quest IDs,
- `WF-SEM-017` semantic validation for quest objectives whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-018` semantic validation for quest transitions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-019` semantic validation for quest conditions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-020` semantic validation for quest stage result scripts whose
  optional condition references do not resolve to conditions declared in the
  same quest,
- `WF-SEM-021` semantic validation for quest variable conditions whose
  variable references do not resolve to variables declared in the same quest,
- `WF-SEM-022` semantic validation for dialogue conditions whose quest stage
  references do not resolve inside the dialogue line's referenced quest,
- `WF-SEM-023` semantic validation for dialogue conditions whose quest
  variable references do not resolve inside the dialogue line's referenced
  quest,
- `WF-SEM-024` semantic validation for dialogue lines whose `topicId`
  references do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-025` semantic validation for dialogue `linkTo` target topic
  references that do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-026` semantic validation for dialogue quest-level gates whose
  `questId` references do not resolve to declared quest IDs,
- `WF-SEM-027` semantic validation for dialogue quest-level gate conditions
  whose quest stage references do not resolve inside the gate's referenced
  quest,
- `WF-SEM-028` semantic validation for dialogue quest-level gate conditions
  whose quest variable references do not resolve inside the gate's referenced
  quest,
- `WF-SEM-029` semantic validation for dialogue result-script variable
  mutations whose variable references do not resolve inside the dialogue
  line's referenced quest,
- `WF-SEM-030` semantic validation for dialogue `linkFrom` source topic
  references that do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-031` semantic validation for dialogue `linkTo` target topics that
  are declared but have no authored dialogue line endpoint,
- `WF-SEM-032` semantic validation for dialogue `linkFrom` source topics that
  are declared but have no authored dialogue line endpoint,
- `WF-SEM-033` semantic validation for duplicate authored prompt routes with
  the same `topicId`, `promptText`, and `priority`,
- `WF-SEM-034` semantic validation for dialogue condition logic entries whose
  `conditionIds` references do not resolve to conditions authored on the same
  dialogue line,
- deterministic fixtures for invalid quest registry shape, invalid
  stage/objective shape, missing dialogue quest references, and missing quest
  objective stage references, plus invalid transition shape and missing
  transition stage references, invalid condition shape, and missing condition
  stage references, invalid result-script shape, and missing result-script
  condition references, invalid variable shape, and missing condition variable
  references, invalid dialogue condition shape, missing dialogue condition
  stage references, missing dialogue condition variable references, and invalid
  dialogue result-script shape, invalid dialogue topic shape, missing dialogue
  topic references, missing dialogue topic link references, invalid dialogue
  quest gate shape, missing dialogue quest gate references, missing dialogue
  quest gate stage references, missing dialogue quest gate variable references,
  invalid dialogue result-script mutation shape, and missing dialogue
  result-script mutation variable references, invalid dialogue Link From
  shape, missing dialogue Link From source topic references, missing dialogue
  Link To target line endpoints, and missing dialogue Link From source line
  endpoints, invalid dialogue prompt route shape, invalid dialogue Speech
  Challenge shape, invalid dialogue skill gate shape, invalid dialogue perk
  gate shape, invalid dialogue faction gate shape, invalid dialogue reputation
  gate shape, invalid dialogue identity gate shape, invalid dialogue local
  world flag gate shape, invalid dialogue event history gate shape, and
  invalid dialogue companion state gate shape, invalid dialogue result-script
  side-effect gate shape, invalid dialogue condition boolean composition
  shape, missing dialogue condition logic references, and duplicate dialogue
  prompt routes.

Gate 43 preserves the earlier asset, voice, dialogue registry, dialogue
voice worklist, and dialogue quest reference validation from Gates 14 through
42.

It intentionally does not create:

- VS Code problem matchers,
- release publishing,
- ZIP/FOMOD package creation,
- deep NIF, DDS, WAV, OGG, LIP, KF, RDT, or BSA validation,
- WAV/OGG sample-rate, bitrate, channel, or codec validation,
- validation that voice filenames correspond to dialogue records in a master file,
- GECK lip processing asset detection,
- full GECK dialogue condition language beyond quest stage and quest variable
  equality skeletons,
- full GECK condition language,
- dialogue quest gate execution, ordering, or compilation semantics,
- raw result-script bodies, full result-script mutation semantics, quest stage
  mutations, lockouts, or branch semantics,
- condition evaluation or result-script execution semantics,
- dialogue condition compilation into plugin records,
- dialogue result-script compilation into plugin records,
- external plugin record validation for quest or topic references,
- reciprocal Link To/Link From requirements,
- full dialogue graph traversal and cycle checks,
- exact GECK priority range, priority ordering, prompt routing execution,
  response routing, and condition-aware prompt selection,
- Speech Challenge success/failure routing, display behavior, exact threshold
  ranges, or execution semantics,
- exact skill taxonomy, skill threshold ranges, skill gate condition mapping,
  or skill gate execution semantics,
- exact perk taxonomy, perk registry resolution, perk condition mapping, or
  perk gate execution semantics,
- exact faction registry resolution, faction relation taxonomy, reputation
  standing taxonomy, or faction/reputation gate execution semantics,
- exact identity taxonomy, actor/player identity mapping, or identity gate
  execution semantics,
- exact local world flag taxonomy, world-state registry resolution,
  boolean/value modeling, scope semantics, GECK condition mapping, or local
  world flag gate execution semantics,
- exact event-history registry resolution, event signal taxonomy, lifecycle
  modeling, consumer routing, time horizon semantics, GECK condition mapping,
  or event history gate execution semantics,
- exact companion registry resolution, companion-specific observer model,
  companion state taxonomy, trust/history/trigger value semantics, GECK
  condition mapping, or companion state gate execution semantics,
- exact result-script side-effect registry resolution, effect taxonomy, raw
  script semantics, execution ordering, GECK condition mapping, or
  side-effect gate execution semantics,
- exact nested condition groups, negation, precedence, short-circuit behavior,
  GECK condition-list mapping, or condition composition execution semantics,
- plugin record compilation,
- external tool-backed asset inspection,
- provider/environment detection or capability scans,
- generation, packaging, release, or capability scan implementations.

Those belong to later gates recorded in `WasteLandForge/planning/`.

## Source and Output Boundaries

Canonical source belongs in version control. Generated and distribution outputs are disposable:

- `schemas/`, `src/`, `tests/`, `fixtures/`, and `docs/` are source or test inputs.
- `generated/` and `dist/` are output locations.
- `.wastelandforge/cache/`, `.wastelandforge/logs/`, and `.wastelandforge/tmp/` are local state.

## Correctness Rules

The core path must work offline and without AI:

- parse,
- normalize,
- validate,
- resolve capabilities,
- generate outputs,
- write build manifests,
- package release candidates.

AI may draft or explain, but AI output is not canonical truth.
