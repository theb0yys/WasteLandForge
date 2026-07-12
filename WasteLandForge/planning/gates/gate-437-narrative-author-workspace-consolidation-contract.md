# Gate 437 - Narrative Author Workspace Consolidation Contract

Status: Complete
Phase: v0.1 desktop usability definition
Decision base: ADR-010, ADR-011, Gates 402-436, app-shell product boundary

## Goal

Define a discoverable, bounded Narrative Author workspace that presents one
implemented workflow at a time, keeps shared status and output visible, and
removes dependence on a long scrolling stack without changing source,
validation, generation, or handoff behavior.

## Evidence classification

- **Documented:** The app shell owns workflow navigation, status dashboards,
  report presentation, advanced logs, and local UI state.
- **Documented:** Canonical source, validation, generation, packaging, and
  handoff correctness remain in existing deterministic services/backend paths.
- **Documented:** Narrative Author now exposes 15 implemented workflows through
  14 shortcut buttons and one continuous form stack.
- **Documented:** Several shortcuts use `BringIntoView`; Narrative Source and
  Dialogue Branch both route to the top rather than uniquely selecting a form.
- **Inferred:** Category navigation plus an explicit workflow selector is the
  smallest consolidation that improves discovery without rewriting authoring
  engines or combining transactions.
- **Open:** A future document explorer, graph editor, undo history, multi-document
  editor, and richer per-workflow persistence remain separate product work.

## Workspace structure

Retain the existing two-column Narrative Author layout:

```text
left:  workflow navigation and selected form
right: existing read-only preview/command output
```

The left column receives a fixed header containing:

- category selector;
- workflow selector for that category;
- selected workflow title;
- shared `NarrativeAuthorStatusTextBlock` status surface.

Only the selected workflow form is visible below the fixed header in its own
scrollable region. The output pane remains visible and does not move inside a
workflow form.

## Categories and workflows

### Source

- Create Narrative Source
- Extend Existing Narrative

### Quest

- Revise Quest Presentation
- Add Quest Stage
- Add Quest Objective
- Add Quest Transition
- Add Quest Variable
- Add Quest Condition
- Add Stage Result Intent

### Dialogue

- Add Dialogue Branch
- Add Dialogue Behavior
- Revise Dialogue Line

### Voice & GECK

- Add Voice Work Item
- Add GECK Binding
- Revise GECK Binding

Every implemented form appears exactly once. Category and workflow labels are
literal user-facing names, not aliases for CLI commands.

## Navigation behavior

- Category selection deterministically selects that category's first workflow
  unless the category's most recently selected workflow is retained in memory.
- Workflow selection hides every other form and reveals the selected form at
  scroll offset zero.
- Switching workflows does not clear controls, loaded choices, preview tokens,
  output text, or the most recent operation status.
- Existing preview invalidation rules remain authoritative when inputs, source,
  manifest, project path, or selection changes.
- No workflow automatically loads source, previews, writes, validates, or
  packages merely because it becomes visible.
- The initial selection is `Source / Create Narrative Source`, matching the
  original workspace's primary creation workflow.

## Status behavior

Move the existing shared status block out of the Create Narrative Source form
into the fixed workspace header. All current handlers continue writing to that
same control. The status must remain visible while any form scrolls and must
not be duplicated into per-form cards or banners.

The selected workflow title identifies which form is active. Existing status
messages retain their current wording and success/failure meaning; Gate 438 does
not introduce a new status state machine or reinterpret backend results.

## Implementation constraints

- Reuse existing controls, event handlers, automation IDs, input defaults,
  authoring services, preview tokens, and command invocations.
- Group each current form into one named container and control only container
  visibility/navigation state.
- Do not duplicate or reconstruct controls dynamically.
- Keep a stable 420-pixel left-column baseline with existing responsive window
  behavior and a persistent output column.
- No nested cards, marketing copy, instructional feature text, or new visual
  asset dependency.
- Keyboard tab order follows category, workflow, selected form, then output.

## Safety boundaries

- No canonical source, schema, registry, semantic rule, diagnostic, generator,
  handoff, CLI, backend, preview-token, or transaction behavior change.
- No automatic file writes, validation, package execution, external-tool launch,
  plugin mutation, game Data/MO2 write, network, release, or AI behavior.
- No removal or renaming of implemented workflows or automation IDs.

## Gate 438 acceptance criteria

- All 15 workflows are reachable through the exact category mapping.
- Exactly one workflow container is visible after every selection.
- Selected forms start at scroll offset zero and key controls fit without
  overlap at 1366x768 and 1920x1080.
- Shared status and output remain visible while the selected form scrolls.
- Switching away and back preserves entered values and valid preview-enabled
  state where source/input has not changed.
- Selection alone performs zero source writes and invokes no backend command.
- Existing focused authoring tests and full solution remain green.
- Published UI Automation visits every workflow, verifies representative
  controls, completes one existing authoring transaction, and confirms the app
  remains responsive.

## Next route

Gate 438: implement categorized Narrative Author navigation, single-form
visibility, fixed shared status, state-preserving switching, responsive checks,
and published-app regression without changing authoring/backend behavior.
