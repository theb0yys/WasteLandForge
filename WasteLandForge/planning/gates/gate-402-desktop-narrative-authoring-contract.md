# Gate 402 - Desktop Narrative Authoring Contract

Status: Complete
Phase: v0.1 product-value definition
Decision base: ADR-007, ADR-009, ADR-010, Gates 370-378, Gates 399-401

## Goal

Define the first desktop source-authoring workflow that turns a small set of
authored quest and dialogue inputs into canonical registries, validates them,
and builds the existing GECK authoring handoff. This gate defines behavior only;
Gate 403 implements the complete vertical slice.

## Research classification

- **Documented:** Canonical source truth is versioned JSON/YAML registry data;
  generated output and GECK sessions are not source truth.
- **Documented:** Quest schema `0.6.0` supports quest identity, stages,
  objectives, transitions, conditions, variables, result intent, and optional
  GECK external references.
- **Documented:** Dialogue schema `0.23.0` requires lines to reference a quest
  and topic and supports optional speaker, prompt, priority, conditions, result
  intent, links, routing, and voice work items.
- **Documented:** Gate 400 packages validated quest/dialogue source into a
  deterministic review handoff without creating plugin records.
- **Inferred:** The smallest useful first authoring surface is one quest with
  start/completion stages and one objective, plus one topic and one dialogue
  line. This is more useful than schema-minimal empty stage arrays while keeping
  the first workflow bounded.
- **Open:** Exact GECK condition functions, record/Form IDs, compilation,
  plugin mutation, voice export, and runtime behavior remain human/GECK work.

## User workflow

The desktop adds a `Narrative Author` workspace with these authored inputs:

- quest slug, title, and optional summary;
- start-stage title and number, defaulting visibly to `10`;
- completion-stage title and number, defaulting visibly to `100`;
- objective text;
- topic slug and title;
- dialogue-line slug, response text, optional speaker, optional prompt, and
  explicit priority when prompt text is supplied;
- optional plugin filename and quest EditorID entered by the user as a pair.

The user performs:

1. `Preview Narrative Source`;
2. reviews exact target paths, derived logical IDs, and formatted JSON;
3. `Create, Validate and Build Handoff` using the unchanged preview token;
4. receives structured validation and GECK-handoff summaries;
5. can open the exact handoff folder and unresolved-actions worklist through
   the existing Project Outputs lane.

## Canonical output

For project ID `<projectId>`, the workflow writes only:

```text
src/registries/quests/main.json
src/registries/dialogue/main.json
wastelandforge.json
```

The quest registry uses schema `0.6.0` and contains:

- registry ID `<projectId>.quests`;
- quest ID `<projectId>.quest.<questSlug>`;
- stages `<questId>.stage.start` and `<questId>.stage.complete`;
- objective `<questId>.objective.primary` referencing both stages;
- transition `<questId>.transition.start.complete` referencing both stages;
- optional GECK external reference only when both plugin and EditorID were
  explicitly supplied.

The dialogue registry uses schema `0.23.0` and contains:

- registry ID `<projectId>.dialogue`;
- topic ID `<projectId>.topic.<topicSlug>`;
- line ID `<projectId>.dialogue.<questSlug>.<lineSlug>`;
- exact references to the authored quest and topic IDs;
- only fields explicitly supplied by the user, except the visible/defaulted
  stage numbers.

The manifest receives exact `registries.quests` and `registries.dialogue`
declarations. No capability or dependency declaration is required because the
handoff is an offline Forge package operation, not a runtime provider output.

## Preview and transaction rules

- Preview performs structured input validation and builds both complete JSON
  documents in memory without writing files.
- The preview token binds normalized inputs plus current manifest and target
  source bytes/existence.
- Any input, manifest, or target-source change invalidates creation.
- Existing quest or dialogue source is refused; append/edit belongs to a later
  contract.
- Conflicting manifest registry paths are refused.
- Creation uses structured JSON APIs and a single guarded transaction. On any
  write failure, original manifest bytes are restored and newly created source
  files/directories are removed where safe.
- Successful creation runs canonical `forge validate .`, then
  `forge package . --target geck-handoff`; package execution is blocked when
  validation fails.

## Input and refusal rules

- Slugs are lowercase ASCII alphanumeric tokens and must derive schema-valid,
  unique logical IDs.
- Titles, objective text, topic title, and response text are non-empty.
- Stage numbers are distinct integers greater than or equal to zero, with start
  lower than completion.
- Prompt text requires an explicit integer priority, matching dialogue schema
  dependent-required behavior.
- Plugin and quest EditorID are either both omitted or both supplied. Plugin
  must end in `.esm` or `.esp` and contain no path separator.
- No raw JSON, script body, condition function, FormID, voice path, or output
  path is accepted through this first UI.
- Invalid project/manifest/schema state, stale preview, existing source,
  conflicting path, duplicate identity, path escape, validation failure, or
  package failure leaves a clear blocking message and no partial canonical
  source transaction.

## Safety boundaries

- No ESP/ESM creation or mutation.
- No GECK/xEdit launch or automation.
- No script generation/compilation, voice export, game Data write, MO2 write,
  game launch, runtime probe, network, release publication, or AI requirement.
- IDs are derived only for Forge source identity. GECK identifiers are never
  guessed or synthesized.

## Gate 403 acceptance criteria

- Preview displays exact source documents and paths with zero writes.
- Stale preview, existing source, invalid stages, incomplete external reference,
  and conflicting manifest path are covered by deterministic Windows tests.
- Successful creation produces schema-valid quest/dialogue registries and exact
  manifest declarations, then passes canonical validation and GECK packaging.
- Resulting handoff reports one quest, one dialogue line, expected worklists,
  and explicit unresolved GECK actions.
- Repeated create refuses overwrite and preserves all source bytes.
- Published desktop automation completes the full author-to-handoff workflow.
- Release build and complete test suite pass.

## Next route

Gate 403: implement the complete Narrative Author desktop vertical slice,
transactional source writer, preview-token protection, canonical validation,
GECK handoff packaging, focused tests, and published-app regression.
