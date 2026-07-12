# Gate 443 - Narrative Author Explorer Contract

Status: Defined
Phase: v0.1 desktop usability
Decision base: Gates 437-442, ADR-007, ADR-010, ADR-011

## Goal

Define a read-only quest and dialogue explorer driven by the validated
Narrative Author inventory. Authors should be able to inspect canonical source
structure, select an authored entity, and explicitly open a compatible existing
workflow without manually rediscovering the same identity in each form.

Gate 443 defines behavior only. Gate 444 owns implementation.

## Research Classification

- **Documented:** quest schema `0.6.0` defines quests with stages, objectives,
  transitions, variables, conditions, stage result scripts, and external
  references.
- **Documented:** dialogue schema `0.23.0` defines topics, quest gates, lines,
  gate/line conditions, and dialogue result scripts.
- **Documented:** Gate 441 exposes authoritative explorer input only while the
  validated inventory state is `Ready`.
- **Documented:** the current workspace catalog contains 15 existing workflows;
  this gate cannot invent additional authoring commands.
- **Inferred:** entity-first navigation reduces repeated loading and identity
  selection while preserving the established preview-and-apply transactions.
- **Open:** GECK record hierarchy, plugin state, runtime condition evaluation,
  voice-file readiness, and in-game relationships are not canonical explorer
  facts and remain excluded.

## Source And State Boundary

The explorer is populated from the same validated structured quest and dialogue
documents used by the Gate 441 inventory. It must not independently parse
unvalidated files or add a second source-discovery path.

- `Ready`: explorer content and route actions are enabled.
- `Not loaded`, `Blocked`, or `Stale`: explorer content is cleared or visibly
  unavailable, and no entity route may be invoked.
- Explicit inventory refresh replaces the explorer snapshot atomically.
- Project-path changes and successful authoring transactions invalidate both
  counts and explorer content together.

No partial hierarchy may be exposed after a validation or parsing failure.

## Explorer Hierarchy

Provide two compact views: `Quests` and `Dialogue`. Use a search field and a
tree/list suited to dense inspection, not nested cards.

The quest view exposes:

- quest identity, title, summary, and GECK-binding presence;
- owned stages and their result intents;
- owned objectives, transitions, variables, and conditions; and
- GECK external references as source declarations only.

The dialogue view exposes:

- topic identity and title;
- lines grouped by authored topic reference, including response/prompt summary;
- quest gates and their conditions; and
- line conditions and dialogue result intents beneath their owning line.

Every node carries a stable canonical logical ID and entity kind. Ordering is
deterministic by display label and then logical ID using ordinal comparison.
Search is case-insensitive over logical ID and authored display text, preserves
ancestor context, performs no source reads, and changes no selection outside
the explorer.

## Supported Workflow Routing

Routing is an explicit `Open in workflow` action. Only these existing routes
may be offered:

- quest: Revise Quest Presentation; Add Quest Stage, Objective, Transition,
  Variable, Condition, or Stage Result Intent; Add or Revise GECK Binding;
- stage: Add Quest Objective, Add Quest Transition, or Add Stage Result Intent;
- objective: Revise Quest Presentation;
- topic: Extend Existing Narrative;
- dialogue line: Add Dialogue Branch, Add Dialogue Behavior, Revise Dialogue
  Line, or Add Voice Work Item;
- GECK external reference: Revise GECK Binding.

Conditions, variables, transitions, result intents, and quest gates remain
inspectable but do not receive invented edit routes where no matching existing
workflow supports editing that exact declaration.

Opening a route selects the existing category/workflow, invokes its existing
read-only loader when required, and preselects the matching entity or owner by
canonical ID. If the entity no longer exists or cannot be selected, routing
fails visibly and leaves apply actions disabled. Routing must never preview,
apply, generate, package, or rebuild a handoff automatically.

## Interaction And Layout

- Place the explorer below the fixed inventory summary and above or beside the
  existing single workflow form without obscuring the persistent output pane.
- Keep the inventory summary, explorer selection, workflow status, and output
  readable at 1366x768 and 1920x1080.
- Preserve explorer view, search text, expanded groups, and selection while
  switching workflow categories during the same `Ready` snapshot.
- Keyboard selection, expansion, search, and route invocation must work without
  requiring a pointer.
- Empty states describe absence of canonical entities, not capability or game
  readiness.

## Determinism And Safety

Explorer population, search, selection, and workflow routing must write zero
project bytes and invoke no Forge backend command. Existing workflow loaders
remain local, read-only, and validation-gated. No new schema, validator,
generator, CLI, backend, package, handoff, GECK automation, external-tool,
plugin, game Data/MO2, network, release, or AI contract is permitted.

## Gate 444 Acceptance

Gate 444 must prove:

1. ExampleMod produces the exact deterministic quest/dialogue hierarchy;
2. optional empty arrays produce stable empty groups without phantom nodes;
3. invalid and stale inventory expose no actionable explorer snapshot;
4. search preserves ancestors and performs zero source writes/backend calls;
5. every offered route maps to one of the 15 catalogued workflows;
6. quest, stage, objective, topic, line, and GECK-reference routing preselects
   the expected existing entity or owner and leaves apply disabled;
7. workflow/category switching preserves explorer state;
8. published UI regression passes at 1366x768 and 1920x1080; and
9. a routed existing workflow still completes through its unchanged explicit
   preview/apply path, after which the explorer becomes stale.

## Stop Point

Gate 443 stops at this contract. No product code, source data, schema,
validator, generator, CLI/backend, package, installer, GECK, or game-facing
behavior changes.

Next route: Gate 444 - implement the validated Narrative Author quest/dialogue
explorer, supported workflow routing, focused tests, and published-app
regression.
